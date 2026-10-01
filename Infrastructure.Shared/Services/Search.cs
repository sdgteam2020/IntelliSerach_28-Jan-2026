using Application.Interfaces;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Infrastructure.Shared.Services
{
    public class Search : ISearch
    {
        public async Task<string> GetResponse(DTOSerchRequest Request,  string Url, string UserName, string Password)
        {
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "a", "an", "the",
    "is", "am", "are", "was", "were",
    "be", "been", "being",
    "has", "have", "had",
    "do", "does", "did",
    "of", "in", "on", "at", "to", "for",
    "from", "with", "by",
    "and", "or", "but"
};

            string originalQuery = Request.DataString?
                .Trim() ?? string.Empty;

            originalQuery = Regex.Replace(
                originalQuery,
                @"\s+",
                " "
            );

            var words = originalQuery
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !stopWords.Contains(x.Trim()))
                .ToArray();

            string keywordQuery = string.Join(" ", words);
            Request.Filter = Request.Filter == "All" ? "*" : Request.Filter;

            var filters = new List<object>();
            var shouldQueries = new List<object>();

            // ============================================================
            // Fields searched for Web Crawler data
            // ============================================================

            var searchFields = new List<string>
    {
        "title^5",
        "headings^4",
        "meta_description^3",

        "table_rows.columns.label^3",
        "table_rows.columns.name^3",
        "table_rows.columns.value^3",

        "table_rows.raw_text^2.5",
        "table_rows.raw^2",

        "content^2"
    };

            // ------------------------------------------------------------
            // Prepare search text
            // ------------------------------------------------------------
            string searchText = Request.DataString?.Trim() ?? "";

            bool isQuotedSearch =
                searchText.Length >= 2 &&
                searchText.StartsWith("\"") &&
                searchText.EndsWith("\"");

            bool isExactSearch =
                Request.Type == 3 || isQuotedSearch;


            // Remove surrounding quotes for exact search
            if (isQuotedSearch)
            {
                searchText = searchText.Substring(
                    1,
                    searchText.Length - 2
                ).Trim();
            }


            // ============================================================
            // SEARCH TYPE
            //
            // Type 1 = Normal search
            // Type 2 = Fuzzy search
            // Type 3 = Exact/Phrase search
            // ============================================================

            if (isExactSearch)
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    shouldQueries.Add(
                        new MultiMatchWrapper
                        {
                            multi_match = new MultiMatchQuery
                            {
                                query = searchText,
                                type = "phrase",
                                fields = searchFields,
                                boost = 5
                               



                            }
                        }
                    );
                }
            }
            else
            {
                // --------------------------------------------------------
                // 1. Original phrase gets higher priority
                // --------------------------------------------------------
                if (!string.IsNullOrWhiteSpace(originalQuery))
                {
                    shouldQueries.Add(
                        new MultiMatchWrapper
                        {
                            multi_match = new MultiMatchQuery
                            {
                                query = originalQuery,      // ORIGINAL QUERY
                                type = "phrase",
                                fields = searchFields,
                                boost = 10
                            }
                        }
                    );
                }

                // --------------------------------------------------------
                // 2. Normal / Fuzzy keyword search
                // --------------------------------------------------------
                bool isFuzzy =
                    Request.Type == 2 ||
                    (
                        !string.IsNullOrWhiteSpace(Request.DataString) &&
                        Request.DataString.StartsWith("(") &&
                        Request.DataString.EndsWith(")")
                    );

                if (Request.Type == 1 || isFuzzy)
                {
                    string searchText1 = keywordQuery;   // IMPORTANT: keywordQuery here

                    // Remove surrounding ( ) if fuzzy search syntax is used
                    if (
                        !string.IsNullOrWhiteSpace(searchText1) &&
                        searchText1.StartsWith("(") &&
                        searchText1.EndsWith(")")
                    )
                    {
                        searchText1 =
                            searchText1.Substring(
                                1,
                                searchText1.Length - 2
                            );
                    }

                    if (!string.IsNullOrWhiteSpace(searchText1))
                    {
                        shouldQueries.Add(
                            new MultiMatchWrapper
                            {
                                multi_match = new MultiMatchQuery
                                {
                                    query = searchText1,       // keywordQuery goes here
                                    type = "best_fields",
                                    fields = searchFields,

                                    Operator = "and",

                                    fuzziness = isFuzzy
                                        ? "AUTO"
                                        : "0",

                                    fuzzy_transpositions = isFuzzy
                                }
                            }
                        );
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(Request.Filter))
            {
                // ========================================================
                // Specific selected source
                //
                // Example:
                // Request.Filter = "fs-mou"
                //
                // Search:
                // 1. fs-mou index directly
                // 2. asdc_new where path.real contains \fs-mou\
                // ========================================================

                filters.Add(
                    new
                    {
                        @bool = new
                        {
                            should = new List<object>
                            {
                        // ------------------------------------------------
                        // 1. Actual Web Crawler index
                        // ------------------------------------------------

                        new
                        {
                            @bool = new
                            {
                                filter = new List<object>
                                {
                                    new
                                    {
                                        terms = new
                                        {
                                            _index = new List<string>
                                            {
                                                Request.Filter
                                            }
                                        }
                                    }
                                }
                            }
                        },

                        // ------------------------------------------------
                        // 2. Files stored inside asdc_new
                        // ------------------------------------------------

                        new
                        {
                            @bool = new
                            {
                                filter = new List<object>
                                {
                                    new
                                    {
                                        terms = new
                                        {
                                            _index = new List<string>
                                            {
                                                "asdc_new"
                                            }
                                        }
                                    },

                                    new
                                    {
                                        wildcard =
                                            new Dictionary<string, string>
                                            {
                                                {
                                                    "path.real",
                                                    $"*\\\\{Request.Filter}\\\\*"
                                                }
                                            }
                                    }
                                }
                            }
                        }
                            },

                            minimum_should_match = 1
                        }
                    }
                );
            }

            // ============================================================
            // ELASTICSEARCH QUERY
            // ============================================================

            var dto = new DTOSearchQueryRequest
            {
                from = Request.from,
                size = Request.size,

                query = new SearchQuery
                {
                    @bool = new SearchBoolQuery
                    {
                        should = shouldQueries,

                        filter = filters.Any()
                            ? filters
                            : null,

                        minimum_should_match = 1
                    }
                },
                // ========================================================
                // INDEX COUNT / INDEX LIST AGGREGATION
                // ========================================================

                aggs = new Dictionary<string, object>
    {
        {
            "indexes_used",
            new
            {
                terms = new
                {
                    field = "_index",
                    size = 1000
                },
            }
        }
    },


                // ========================================================
                // HIGHLIGHT ALL SEARCHABLE WEB CRAWLER FIELDS
                // ========================================================

                highlight = new Highlight
                {
                    pre_tags = new[]
                    {
                "<mark class=\"marks\">"
            },

                    post_tags = new[]
                    {
                "</mark>"
            },

                    fields = new Dictionary<string, HighlightField>
            {
                {
                    "title",
                    new HighlightField
                    {
                        number_of_fragments = 0
                    }
                },

                {
                    "headings",
                    new HighlightField
                    {
                        fragment_size = 300,
                        number_of_fragments = 3
                    }
                },
               


                {
                    "meta_description",
                    new HighlightField
                    {
                        fragment_size = 300,
                        number_of_fragments = 3
                    }
                },

                {
                    "content",
                    new HighlightField
                    {
                        type="unified",
                        fragment_size = 600,
                        number_of_fragments = 3,
                        boundary_scanner= "sentence"
                    }
                },
                {
                "table_rows.columns.value",
                new HighlightField
                {
                    fragment_size = 300,
                    number_of_fragments = 3
                }
            },
                {
                    "table_rows.raw_text",
                    new HighlightField
                    {
                        fragment_size = 150,
                        number_of_fragments = 3
                    }
                },

                {
                    "table_rows.raw",
                    new HighlightField
                    {
                        fragment_size = 150,
                        number_of_fragments = 3
                    }
                },

                {
                    "table_rows.columns.label",
                    new HighlightField
                    {
                        fragment_size = 150,
                        number_of_fragments = 3
                    }
                },

                {
                    "table_rows.columns.name",
                    new HighlightField
                    {
                        fragment_size = 150,
                        number_of_fragments = 3
                    }
                },

                //{
                //    "table_rows.columns.value",
                //    new HighlightField
                //    {
                //        fragment_size = 150,
                //        number_of_fragments = 3
                //    }
                //}
            }
                }
            };

            // ============================================================
            // SERIALIZE
            // ============================================================

            string jsonBody = JsonConvert.SerializeObject(
                dto,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );

            // ============================================================
            // ELASTICSEARCH REQUEST
            // ============================================================

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(
                UserName,
                Password
            );

            using var content = new StringContent(
                jsonBody,
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                Url,
                content
            );

            var responseString =
                await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return responseString;
            }

            return "Not Found";
        }

        private static HttpClient CreateHttpClient(string username, string password)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) => true // For localhost/testing
            };

            var client = new HttpClient(handler);

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            return client;
        }

        //public async Task<List<DTOIndexesDetailsResponse>> IndexesDetails(string Url, string UserName, string Password)
        //{
        //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

        //    using var client = CreateHttpClient(UserName, Password);

        //    var response = await client.GetAsync(Url);
        //    var responseString = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //        return new List<DTOIndexesDetailsResponse>();

        //    // Debug check
        //    if (!responseString.Trim().StartsWith("["))
        //        throw new Exception("Elasticsearch did not return JSON: " + responseString);

        //    var indexesDetailsResponses =
        //        JsonConvert.DeserializeObject<List<DTOIndexesDetailsResponse>>(responseString);
        //    var userIndices = indexesDetailsResponses
        //                    .Where(i => !i.index.StartsWith("."))
        //                    .ToList();

        //    return userIndices ?? new List<DTOIndexesDetailsResponse>();
        //}
        public async Task<List<DTOIndexesDetailsResponse>> IndexesDetails(
    string Url,
    string UserName,
    string Password)
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(UserName, Password);

            // 1. Get Index Details
            var response = await client.GetAsync(Url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new List<DTOIndexesDetailsResponse>();

            if (!responseString.Trim().StartsWith("["))
                throw new Exception("Elasticsearch did not return JSON: " + responseString);


            var indexesDetailsResponses =
                JsonConvert.DeserializeObject<List<DTOIndexesDetailsResponse>>(responseString);


            var userIndices = indexesDetailsResponses
                .Where(i => !i.index.StartsWith("."))
                .ToList();

            var uri = new Uri(Url);

            string baseUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            // 2. Get Unique Files Count (ignore chunks)
            var searchUrl = baseUrl + "/*,-*_folder/_search";


            var query = new
            {
                size = 0,
                query = new
                {
                    @bool = new
                    {
                        // Exclude failed documents
                        must_not = new object[]
            {
                new
                {
                    term = new Dictionary<string, object>
                    {
                        { "crawler.index_status", "failed" }
                    }
                }
            }
                    }
                },
                aggs = new
                {
                    indices = new
                    {
                        terms = new
                        {
                            field = "_index",
                            size = 1000
                        },

                        aggs = new
                        {
                            url_count = new
                            {
                                filter = new
                                {
                                    exists = new
                                    {
                                        field = "url"
                                    }
                                },

                                aggs = new
                                {
                                    count = new
                                    {
                                        cardinality = new
                                        {
                                            field = "url.keyword"
                                        }
                                    }
                                }
                            },


                            unique_files = new
                            {
                                filter = new
                                {
                                    @bool = new
                                    {
                                        filter = new object[]
                            {
                                new
                                {
                                    term = new Dictionary<string, object>
                                    {
                                        { "chunk.enabled", false }
                                    }
                                },
                                new
                                {
                                    term = new Dictionary<string, object>
                                    {
                                        { "crawler.is_deleted", false }
                                    }
                                }
                            }
                                    }
                                },

                                aggs = new
                                {
                                    count = new
                                    {
                                        cardinality = new
                                        {
                                            field = "file.FileName.keyword"
                                        }
                                    }
                                },
                            }
                        }
                    }
                }
            };
            var jsonQuery = JsonConvert.SerializeObject(query);

            var searchResponse = await client.PostAsync(
                searchUrl,
                new StringContent(jsonQuery, Encoding.UTF8, "application/json")
            );


            var searchString = await searchResponse.Content.ReadAsStringAsync();


            var searchResult =
                JsonConvert.DeserializeObject<ElasticUniqueFileResponse>(searchString);



            // 3. Merge Unique File Count
            foreach (var index in userIndices)
            {
                var bucket = searchResult?
                    .aggregations?
                    .indices?
                    .buckets?
                    .FirstOrDefault(x =>
                        x.key.Equals(index.index, StringComparison.OrdinalIgnoreCase));


                Console.WriteLine(
                    $"Index: {index.index}, Found: {bucket != null}"
                );

                if(bucket!=null)
                {
                    index.DocsCount = Convert.ToInt32(bucket.unique_files.doc_count + bucket.url_count.doc_count);
                    index.IsCrawlerInstall = bucket.unique_files.doc_count > 0;
                }
            }


            return userIndices ?? new List<DTOIndexesDetailsResponse>();
        }

        public async Task<List<FileSource>> GetDocDetailsByIndexName(
        string baseUrl,
        string indexName,
        int OcrType,
        string userName,
        string password)
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(userName, password);

            // =========================================================
            // COMMON FILTERS
            // =========================================================
            var filters = new List<object>
    {
        new
        {
            term = new Dictionary<string, object>
            {
                { "chunk.enabled", false }
            }
        },
        new
        {
            term = new Dictionary<string, object>
            {
                { "crawler.is_deleted", false }
            }
        }
    };

            // =========================================================
            // OCR FILTER
            // =========================================================
            if (OcrType == 1 || OcrType == 2)
            {
                // OcrType 1 = OCR true
                // OcrType 2 = OCR false
                filters.Add(
                    new
                    {
                        term = new Dictionary<string, object>
                        {
                    { "crawler.ocr_used", OcrType == 1 }
                        }
                    }
                );

                // For OCR/non-OCR file records,
                // URL should NOT exist
                filters.Add(
                    new
                    {
                        @bool = new
                        {
                            must_not = new object[]
                            {
                        new
                        {
                            exists = new
                            {
                                field = "url"
                            }
                        }
                            }
                        }
                    }
                );
            }

            // =========================================================
            // BUILD SHOULD LIST
            // =========================================================
            var shouldQueries = new List<object>();

            // Normal file/OCR filter condition
            shouldQueries.Add(
                new
                {
                    @bool = new
                    {
                        filter = filters
                    }
                }
            );

            // =========================================================
            // ONLY OcrType == 4
            // URL must exist
            // =========================================================
            if (OcrType == 4)
            {
                shouldQueries.Add(
                    new
                    {
                        exists = new Dictionary<string, object>
                        {
                    { "field", "url" }
                        }
                    }
                );
            }

            // =========================================================
            // QUERY
            // =========================================================
            var query = new
            {
                _source = new[]
                {
            "file.FileName",
            "file.Extension",
            "file.filesize",
            "path.real",
            "path.virtual",
            "url",
            "crawler.ocr_used"
        },

                query = new
                {
                    @bool = new
                    {
                        should = shouldQueries,

                        minimum_should_match = 1,

                        must_not = new object[]
                        {
                    new
                    {
                        term = new Dictionary<string, object>
                        {
                            { "crawler.index_status", "failed" }
                        }
                    }
                        }
                    }
                },

                size = 5000
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(query),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                $"{baseUrl}/{indexName}/_search",
                content
            );

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            var result =
                JsonConvert.DeserializeObject<ElasticsearchSearchResponse>(json);

            return result?.Hits?.Hitss?
                .Select(x => x.Source)
                .ToList()
                ?? new List<FileSource>();
        }
        public async Task<DTODocCountByIndexNameResponse> GetDocCountByIndexName(string baseUrl,string indexName,string userName,string password)
        {

            DTODocCountByIndexNameResponse dTODocCountByIndexNameResponse = new DTODocCountByIndexNameResponse();

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(userName, password);
            var query = new
            {
                size = 0,

                track_total_hits = true,

                query = new
                {
                    @bool = new
                    {
                        filter = new object[]
            {
                new
                {
                    term = new Dictionary<string, object>
                    {
                        { "chunk.enabled", false }
                    }
                },
                new
                {
                    term = new Dictionary<string, object>
                    {
                        { "crawler.is_deleted", false }
                    }
                }
            },

                        minimum_should_match = 0
                    }
                },

                aggs = new
                {
                    total_original_size = new
                    {
                        sum = new
                        {
                            field = "file.filesize"
                        }
                    },

                    ocr_files = new
                    {
                        filter = new
                        {
                            term = new Dictionary<string, object>
                {
                    { "crawler.ocr_used", true }
                }
                        }
                    },

                    non_ocr_files = new
                    {
                        filter = new
                        {
                            @bool = new
                            {
                                must = new object[]
                    {
                        new
                        {
                            term = new Dictionary<string, object>
                            {
                                { "crawler.ocr_used", false }
                            }
                        }
                    },

                                must_not = new object[]
                    {
                        new
                        {
                            term = new Dictionary<string, object>
                            {
                                { "crawler.index_status", "failed" }
                            }
                        }
                    }
                            }
                        }
                    },

                    file_extensions = new
                    {
                        terms = new
                        {
                            field = "file.extension",
                            size = 100
                        }
                    },

                    failed_files = new
                    {
                        filter = new
                        {
                            exists = new Dictionary<string, object>
                {
                    { "field", "meta.failed_reason" }
                }
                        },

                        aggs = new
                        {
                            failed_reason_count = new
                            {
                                terms = new
                                {
                                    field = "meta.failed_reason.keyword",
                                    size = 100
                                }
                            }
                        }
                    }
                }
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(query),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"{baseUrl}/{indexName}/_search",
                content);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            var result =
                JsonConvert.DeserializeObject<DTODocCountByIndexName>(json);
            if(result==null)
                return dTODocCountByIndexNameResponse;

            dTODocCountByIndexNameResponse.Total = result.Hits.Total.Value;
            dTODocCountByIndexNameResponse.Size = result.Aggregations.Total_Original_Size.Value;
            dTODocCountByIndexNameResponse.Ocr_Files=result.Aggregations.Ocr_Files.Doc_Count;
            dTODocCountByIndexNameResponse.Non_Ocr_Files=result.Aggregations.Non_Ocr_Files.Doc_Count;
            dTODocCountByIndexNameResponse.File_Extensions = result.Aggregations.File_Extensions.Buckets;
            dTODocCountByIndexNameResponse.Total_failed_files = result.Aggregations.Failed_Files.Doc_Count;

            return dTODocCountByIndexNameResponse;
        }
        public async Task<List<DTOStatisticsHit>> GetstatisticsByIndexName(string baseUrl,string indexName,string userName, string password)
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(userName, password);
            var query = new
            {
                query = new
                {
                    term = new Dictionary<string, object>
            {
                {
                    "indexName.keyword",
                    indexName
                }
            }
                    }
                };
            var content = new StringContent(
                JsonConvert.SerializeObject(query),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(
                $"{baseUrl}/indexingstatistics/_search",
                content);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            var result =
                JsonConvert.DeserializeObject<DTOStatisticsDetailsResponse>(json);
            if (result == null)
                return null;

            return result.Hits.Hits;
        }
        public async Task<DTOGetDoc_WebCountResponse> GetCountDoc_Web(
      string baseUrl,
      string userName,
      string password)
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(userName, password);

            // _msearch requires NDJSON format.
            // Every header/query must be on a separate line.
            // Final newline is also required.
            string requestBody =
                "{\"index\":\"fs-*\"}\n" +
                "{\"size\":0,\"query\":{\"bool\":{\"filter\":[{\"bool\":{\"should\":[{\"bool\":{\"filter\":[{\"term\":{\"chunk.enabled\":false}},{\"term\":{\"crawler.is_deleted\":false}}]}},{\"exists\":{\"field\":\"url\"}}],\"minimum_should_match\":1}}],\"must_not\":[{\"term\":{\"crawler.index_status\":\"failed\"}}]}}}\n" +
                "{\"index\":\"crawler_registry_scrapy\"}\n" +
                "{\"size\":0,\"query\":{\"exists\":{\"field\":\"normalized_url\"}}}\n";

            using var content = new StringContent(
                requestBody,
                Encoding.UTF8,
                "application/x-ndjson");

            var response = await client.PostAsync(
                $"{baseUrl.TrimEnd('/')}/_msearch",
                content);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(
                    $"Elasticsearch error {(int)response.StatusCode}: {json}");

            var result =
                JsonConvert.DeserializeObject<DTOGetDoc_WebCountResponse>(json);

            return result ?? new DTOGetDoc_WebCountResponse
            {
                Responses = new List<SearchResponseDto>()
            };
        }

    }
}