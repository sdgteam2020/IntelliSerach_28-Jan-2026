using Application.Interfaces;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Infrastructure.Shared.Services
{
    public class Search : ISearch
    {
        private readonly HttpClient _httpClient;

        public Search(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string> GetResponse(DTOSerchRequest Request,  string Url, string encodedKey)
        {

            if (string.IsNullOrWhiteSpace(Url) ||
       !Uri.TryCreate(Url, UriKind.Absolute, out var searchUri))
            {
                throw new ArgumentException("A valid Elasticsearch URL is required.", nameof(Url));
            }

            if (string.IsNullOrWhiteSpace(encodedKey))
                throw new ArgumentException("An encoded API key is required.", nameof(encodedKey));


            if (!string.IsNullOrWhiteSpace(Request.DataString.Trim()) &&
                         Request.DataString.Trim().StartsWith("(") &&
                         Request.DataString.Trim().EndsWith(")"))
            {
                Request.Type = 2;
                Request.DataString =
                            Request.DataString.Trim().Substring(
                                1,
                                Request.DataString.Trim().Length - 2
                            );
            }
            else if (!string.IsNullOrWhiteSpace(Request.DataString.Trim()) &&
                         Request.DataString.Trim().Length >= 2 &&
                Request.DataString.Trim().StartsWith("\"") &&
                Request.DataString.Trim().EndsWith("\""))
            {
                Request.Type = 3;
                Request.DataString =
                            Request.DataString.Trim().Substring(
                                1,
                                Request.DataString.Trim().Length - 2
                            );
            }
            string originalQuery = Regex.Replace(
    Request.DataString?.Trim() ?? string.Empty,
    @"\s+",
    " ");

            bool isQuotedSearch =
        originalQuery.Length >= 2 &&
        originalQuery.StartsWith("\"") &&
        originalQuery.EndsWith("\"");

            bool isExactSearch = Request.Type == 3 || isQuotedSearch;

            bool isFuzzySearch =
                !isExactSearch &&
                (Request.Type == 2 ||
                 (originalQuery.StartsWith("(") && originalQuery.EndsWith(")")));

            string searchText = originalQuery;

            if (isQuotedSearch)
            {
                searchText = originalQuery[1..^1].Trim();
            }
            else if (isFuzzySearch &&
                     searchText.StartsWith("(") &&
                     searchText.EndsWith(")"))
            {
                searchText = searchText[1..^1].Trim();
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                throw new ArgumentException("Search text cannot be empty.");
            }

            var searchFields = new List<string>
            {
                "content^5",
                "headings^4",
                "table_rows.raw_text^2.5"
            };

            // Use exact original words. Don't remove stop words here.
            var mustQueries = new List<object>();
            var shouldQueries = new List<object>();





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
                mustQueries.Add(new
                {
                    multi_match = new
                    {
                        query = searchText,
                        type = "phrase",
                        fields = searchFields
                    }
                });
            }
            else if (isFuzzySearch)
            {
                mustQueries.Add(new
                {
                    multi_match = new
                    {
                        query = searchText,
                        type = "best_fields",
                        fields = searchFields,
                        @operator = "and",
                        fuzziness = "AUTO",
                        fuzzy_transpositions = true
                    }
                });
            }
            else
            {
                // Normal search: require all meaningful query terms in one field.
                string normalQuery = RemoveStopWords(searchText);

                if (string.IsNullOrWhiteSpace(normalQuery))
                {
                    normalQuery = searchText;
                }

                mustQueries.Add(new
                {
                    multi_match = new
                    {
                        query = normalQuery,
                        type = "best_fields",
                        fields = searchFields,
                        @operator = "and"
                    }
                });

                // Optional phrase match improves ranking; it does not decide eligibility.
                shouldQueries.Add(new
                {
                    multi_match = new
                    {
                        query = searchText,
                        type = "phrase",
                        fields = searchFields,
                        boost = 5
                    }
                });
            }

            var filters = new List<object>();

            string selectedIndex = Request.Filter?.Trim() ?? string.Empty;
            bool searchAll = string.IsNullOrEmpty(selectedIndex) ||
                             selectedIndex.Equals("All", StringComparison.OrdinalIgnoreCase) ||
                             selectedIndex == "*";

            if (!searchAll)
            {
                filters.Add(new
                {
                    @bool = new
                    {
                        should = new object[]
                        {
                // Direct crawler/source index
                new
                {
                    term = new Dictionary<string, object>
                    {
                        ["_index"] = selectedIndex
                    }
                },

                // Files belonging to the selected source index
                new
                {
                    @bool = new
                    {
                        filter = new object[]
                        {
                            new
                            {
                                term = new Dictionary<string, object>
                                {
                                    ["_index"] = "fs-intellisearch-207"
                                }
                            },
                            new
                            {
                                wildcard = new Dictionary<string, object>
                                {
                                    ["path.real"] = $"*\\{selectedIndex}\\*"
                                }
                            }
                        }
                    }
                }
                        },
                        minimum_should_match = 1
                    }
                });
            }

            // ============================================================
            // ELASTICSEARCH QUERY
            // ============================================================

            var dto = new DTOSearchQueryRequest
            {
                from = Request.from,
                size = Request.size,
                track_total_hits = true,
                _source = new SourceFilter
                {
                    excludes = new List<string> { "" }
                },
                collapse = new Collapse
                {
                    field = "chunk.source_document_id.keyword"
                },

                query = new SearchQuery
                {
                    @bool = new SearchBoolQuery
                    {
                        must = mustQueries,
                        should = shouldQueries.Count > 0 ? shouldQueries : null,
                        filter = filters.Count > 0 ? filters : null,

                        // must guarantees a match; the optional phrase only boosts ranking.
                        minimum_should_match = null
                    }
                },
                // ========================================================
                // INDEX COUNT / INDEX LIST AGGREGATION
                // ========================================================

                aggs = new Dictionary<string, object>
                {
                    ["unique_results"] = new
                    {
                        cardinality = new
                        {
                            field = "chunk.source_document_id.keyword",
                            precision_threshold = 40000
                        }
                    },
                    ["indexes_used"] = new
                    {
                        terms = new
                        {
                            field = "_index",
                            size = 1000
                        }
                    }
                },


                // ========================================================
                // HIGHLIGHT ALL SEARCHABLE WEB CRAWLER FIELDS
                // ========================================================

                // ========================================================
                // HIGHLIGHT CONFIGURATION WITH FORCE HIGHLIGHT QUERY
                // ========================================================
                highlight = new Highlight
                {
                    require_field_match = false,
                    pre_tags = new[] { "<mark class=\"marks\">" },
                    post_tags = new[] { "</mark>" },

                    // Force highlighter to match ALL terms in originalQuery (including stop words)
                    highlight_query = isExactSearch
    ? new
    {
        multi_match = new
        {
            query = searchText,
            type = "phrase",
            fields = searchFields
        }
    }
    : isFuzzySearch
        ? new
        {
            multi_match = new
            {
                query = searchText,
                type = "best_fields",
                fields = searchFields,
                @operator = "and",
                fuzziness = "AUTO",
                fuzzy_transpositions = true
            }
        }
        : new
        {
            multi_match = new
            {
                query = RemoveStopWords(searchText),
                type = "best_fields",
                fields = searchFields,
                @operator = "and"
            }
        },
                    fields = new Dictionary<string, HighlightField>
            {
                { "title", new HighlightField { type = "unified", number_of_fragments = 0 } },
                { "headings", new HighlightField { type = "unified", number_of_fragments = 0 } },
                //{ "meta_description", new HighlightField { type = "unified", fragment_size = 300, number_of_fragments = 3 } },
                { "content", new HighlightField { type = "unified", fragment_size = 600, number_of_fragments = 3, boundary_scanner = "sentence" } },
                //{ "table_rows.columns.value", new HighlightField { type = "unified", fragment_size = 300, number_of_fragments = 3 } },
                { "table_rows.raw_text", new HighlightField { type = "unified", fragment_size = 150, number_of_fragments = 3 } },
                //{ "table_rows.raw", new HighlightField { type = "unified", fragment_size = 150, number_of_fragments = 3 } },
                //{ "table_rows.columns.label", new HighlightField { type = "unified", fragment_size = 150, number_of_fragments = 3 } },
                //{ "table_rows.columns.name", new HighlightField { type = "unified", fragment_size = 150, number_of_fragments = 3 } }
            },
              
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
          });

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, searchUri);
            httpRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", encodedKey);
            httpRequest.Content = new StringContent(
                jsonBody,
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(httpRequest);
            string responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Elasticsearch search failed ({(int)response.StatusCode}): {responseString}");
            }

            return responseString;
        }
        private static string RemoveStopWords(string text)
        {
            var stopWords = new HashSet<string>(
                new[]
                {
            "a", "an", "the", "is", "am", "are", "was", "were",
            "be", "been", "being", "has", "have", "had",
            "do", "does", "did", "of", "in", "on", "at", "to",
            "for", "from", "with", "by", "and", "or", "but"
                },
                StringComparer.OrdinalIgnoreCase);

            return string.Join(
                " ",
                text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(word => !stopWords.Contains(word)));
        }

        public async Task<List<DTOIndexesDetailsResponse>> IndexesDetails(string Url, string encodedKey)
        {
            if (!Uri.TryCreate(Url, UriKind.Absolute, out var indicesUri))
                throw new ArgumentException("A valid Elasticsearch URL is required.", nameof(Url));

            if (string.IsNullOrWhiteSpace(encodedKey))
                throw new ArgumentException("An encoded API key is required.", nameof(encodedKey));

            using var indicesRequest = new HttpRequestMessage(HttpMethod.Get, indicesUri);
            indicesRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", encodedKey);

            using var indicesResponse = await _httpClient.SendAsync(indicesRequest);
            var indicesJson = await indicesResponse.Content.ReadAsStringAsync();

            if (!indicesResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Could not retrieve Elasticsearch indices ({(int)indicesResponse.StatusCode}): {indicesJson}");
            }

            var indexesDetails = JsonConvert.DeserializeObject<List<DTOIndexesDetailsResponse>>(indicesJson)
                ?? new List<DTOIndexesDetailsResponse>();

            var userIndices = indexesDetails
                .Where(i => i != null &&
                            !string.IsNullOrWhiteSpace(i.index) &&
                            !i.index.StartsWith(".", StringComparison.Ordinal))
                .ToList();

            var baseUri = new Uri(indicesUri.GetLeftPart(UriPartial.Authority));
            var searchUri = new Uri(baseUri, "/*,-*_folder/_search");


            var query = new
            {
                size = 0,
                query = new
                {
                    @bool = new
                    {
                        // Exclude failed documents from all aggregations
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
                                }
                            },
                            deleted_documents = new
                            {
                                filter = new
                                {
                                    term = new Dictionary<string, object>
                        {
                            { "crawler.is_deleted", true }
                        }
                                }
                            }
                        }
                    }
                }
            };
            using var searchRequest = new HttpRequestMessage(HttpMethod.Post, searchUri);
            searchRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", encodedKey);
            searchRequest.Content = new StringContent(
                JsonConvert.SerializeObject(query),
                Encoding.UTF8,
                "application/json");

            using var searchResponse = await _httpClient.SendAsync(searchRequest);
            var searchJson = await searchResponse.Content.ReadAsStringAsync();

            if (!searchResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Elasticsearch index aggregation failed ({(int)searchResponse.StatusCode}): {searchJson}");
            }

            var searchResult = JsonConvert.DeserializeObject<ElasticUniqueFileResponse>(searchJson);
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

                if (bucket != null)
                {
                    index.DocsCount = Convert.ToInt32(bucket.unique_files.doc_count + bucket.url_count.doc_count);
                    index.IsCrawlerInstall = bucket.unique_files.doc_count > 0;
                    index.DocsDeleted = bucket.deleted_documents.doc_count;
                }
            }

            return userIndices ?? new List<DTOIndexesDetailsResponse>();
        }

        public async Task<List<FileSource>> GetDocDetailsByIndexName(string baseUrl,string indexName,int OcrType, string encodedKey)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new ArgumentException(
                    "A valid Elasticsearch base URL is required.",
                    nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name is required.", nameof(indexName));

            if (string.IsNullOrWhiteSpace(encodedKey))
                throw new ArgumentException("An encoded API key is required.", nameof(encodedKey));

            if (OcrType < 1 || OcrType > 5)
                throw new ArgumentOutOfRangeException(
                    nameof(OcrType),
                    "OCR type must be between 1 and 5.");


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


            if (OcrType >= 1 && OcrType <= 4)
            {
                // Normal file/OCR filter condition
                shouldQueries.Add(new
                {
                    @bool = new
                    {
                        filter = filters
                    }
                });
            }


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
            if (OcrType == 5)
            {
                shouldQueries.Add(new
                {
                    term = new Dictionary<string, object>
        {
            { "crawler.is_deleted", true }
        }
                });
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
            "title",
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

                size = 10000
            };

            var requestUri = new Uri(
        baseUri,
        $"/{Uri.EscapeDataString(indexName)}/_search");

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", encodedKey);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(query),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Elasticsearch search failed ({(int)response.StatusCode}): {json}");
            }

            var result = JsonConvert.DeserializeObject<ElasticsearchSearchResponse>(json);

            return result?.Hits?.Hitss?
                .Where(hit => hit?.Source != null)
                .Select(hit => hit.Source)
                .ToList()
                ?? new List<FileSource>();
        }
        public async Task<DTODocCountByIndexNameResponse> GetDocCountByIndexName(string baseUrl,string indexName, string encodedKey)
        {

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new ArgumentException(
                    "A valid Elasticsearch base URL is required.",
                    nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name is required.", nameof(indexName));

            if (string.IsNullOrWhiteSpace(encodedKey))
                throw new ArgumentException("An encoded API key is required.", nameof(encodedKey));

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
            var escapedIndexName = Uri.EscapeDataString(indexName);
            var requestUri = new Uri(
                baseUri,
                $"/{escapedIndexName}/_search");

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", encodedKey);
            request.Content = new StringContent(
                JsonConvert.SerializeObject(query),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Elasticsearch document-count request failed " +
                    $"({(int)response.StatusCode}): {json}");
            }

            var result = JsonConvert.DeserializeObject<DTODocCountByIndexName>(json);
            if (result == null)
                return new DTODocCountByIndexNameResponse();
            return new DTODocCountByIndexNameResponse
            {

            Total = result.Hits.Total.Value,
            Size = result.Aggregations.Total_Original_Size.Value,
            Ocr_Files=result.Aggregations.Ocr_Files.Doc_Count,
            Non_Ocr_Files=result.Aggregations.Non_Ocr_Files.Doc_Count,
            File_Extensions = result.Aggregations.File_Extensions.Buckets,
            Total_failed_files = result.Aggregations.Failed_Files.Doc_Count,
            };
           
        }
        public async Task<List<DTOStatisticsHit>> GetstatisticsByIndexName(string baseUrl,string indexName,string encodedKey)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new ArgumentException(
                    "A valid Elasticsearch base URL is required.",
                    nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name is required.", nameof(indexName));

            if (string.IsNullOrWhiteSpace(encodedKey))
                throw new ArgumentException("An encoded API key is required.", nameof(encodedKey));

            var query = new
            {
                query = new
                {
                    term = new Dictionary<string, object>
                    {
                        ["indexName.keyword"] = indexName
                    }
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(baseUri, "/indexingstatistics/_search"))
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(query),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", encodedKey);

            using var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Elasticsearch search failed ({(int)response.StatusCode}): {json}");
            }

            var result = JsonConvert.DeserializeObject<DTOStatisticsDetailsResponse>(json);

            return result?.Hits?.Hits ?? new List<DTOStatisticsHit>();
        }
        public async Task<DTOGetDoc_WebCountResponse> GetCountDoc_Web(string baseUrl,string encodedKey)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new ArgumentException("A valid Elasticsearch base URL is required.", nameof(baseUrl));

            if (string.IsNullOrWhiteSpace(encodedKey))
                throw new ArgumentException("An encoded API key is required.", nameof(encodedKey));

         
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", encodedKey);

            string requestBody =
                "{\"index\":\"fs-*\"}\n" +
                "{\"size\":0,\"query\":{\"bool\":{\"filter\":[{\"bool\":{\"should\":[{\"bool\":{\"filter\":[{\"term\":{\"chunk.enabled\":false}},{\"term\":{\"crawler.is_deleted\":false}}]}},{\"exists\":{\"field\":\"url\"}}],\"minimum_should_match\":1}}],\"must_not\":[{\"term\":{\"crawler.index_status\":\"failed\"}}]}}}\n" +
                "{\"index\":\"crawler_registry_scrapy\"}\n" +
                "{\"size\":0,\"query\":{\"exists\":{\"field\":\"normalized_url\"}}}\n";

            using var content = new StringContent(requestBody, Encoding.UTF8, "application/x-ndjson");

            var response = await _httpClient.PostAsync(
                new Uri(baseUri, "/_msearch"),
                content);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Elasticsearch error {(int)response.StatusCode}: {json}");

            return JsonConvert.DeserializeObject<DTOGetDoc_WebCountResponse>(json)
                ?? new DTOGetDoc_WebCountResponse
                {
                    Responses = new List<SearchResponseDto>()
                };
        }

    }
}