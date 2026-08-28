using Application.Interfaces;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace Infrastructure.Shared.Services
{
    public class Search : ISearch
    {
        public async Task<string> GetResponse(
    DTOSerchRequest Request,
    string Url,
    string UserName,
    string Password)
        {
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

            // ============================================================
            // SEARCH TYPE
            //
            // Type 1 = Normal search
            // Type 2 = Fuzzy search
            // Type 3 = Exact/Phrase search
            // ============================================================

            if (Request.Type == 3)
            {
                // Exact phrase only
                shouldQueries.Add(
                    new MultiMatchWrapper
                    {
                        multi_match = new MultiMatchQuery
                        {
                            query = Request.DataString,
                            type = "phrase",
                            fields = searchFields,
                            boost = 3
                        }
                    }
                );
            }
            else
            {
                // --------------------------------------------------------
                // Phrase match gets higher priority
                // --------------------------------------------------------

                shouldQueries.Add(
                    new MultiMatchWrapper
                    {
                        multi_match = new MultiMatchQuery
                        {
                            query = Request.DataString,
                            type = "phrase",
                            fields = searchFields,
                            boost = 2
                        }
                    }
                );

                // --------------------------------------------------------
                // Normal / Fuzzy search
                // --------------------------------------------------------

                if (Request.Type == 1 || Request.Type == 2)
                {
                    shouldQueries.Add(
                        new MultiMatchWrapper
                        {
                            multi_match = new MultiMatchQuery
                            {
                                query = Request.DataString,
                                type = "best_fields",
                                fields = searchFields,

                                // Type 2 = fuzzy
                                // Type 1 = no fuzzy matching
                                fuzziness = Request.Type == 2
                                    ? "AUTO"
                                    : "0",

                                fuzzy_transpositions = Request.Type == 2
                            }
                        }
                    );
                }
            }

            // ============================================================
            // FILTER
            // ============================================================

            if (Request.Filter == "*!")
            {
                // Keep your existing special case

                filters.Add(
                    new
                    {
                        @bool = new
                        {
                            should = new List<object>
                            {
                        new
                        {
                            wildcard = new Dictionary<string, string>
                            {
                                {
                                    "path.real",
                                    $"*\\\\{Request.Filter}\\\\*"
                                }
                            }
                        }
                            },

                            minimum_should_match = 1
                        }
                    }
                );
            }
            else if (Request.Filter == "*")
            {
                // IMPORTANT:
                // No _index filter is required for All.
                //
                // Do NOT use:
                //
                // terms : { "_index" : ["*"] }
                //
                // because terms query performs exact matching.
                //
                // Calling /_search without index filter already searches
                // all available indexes accessible through this URL.
            }
            else if (!string.IsNullOrWhiteSpace(Request.Filter))
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

                //{
                //    "table_rows.raw_text",
                //    new HighlightField
                //    {
                //        fragment_size = 150,
                //        number_of_fragments = 3
                //    }
                //},

                //{
                //    "table_rows.raw",
                //    new HighlightField
                //    {
                //        fragment_size = 150,
                //        number_of_fragments = 3
                //    }
                //},

                //{
                //    "table_rows.columns.label",
                //    new HighlightField
                //    {
                //        fragment_size = 150,
                //        number_of_fragments = 3
                //    }
                //},

                //{
                //    "table_rows.columns.name",
                //    new HighlightField
                //    {
                //        fragment_size = 150,
                //        number_of_fragments = 3
                //    }
                //},

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

        public async Task<List<DTOIndexesDetailsResponse>> IndexesDetails(string Url, string UserName, string Password)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(UserName, Password);

            var response = await client.GetAsync(Url);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new List<DTOIndexesDetailsResponse>();

            // Debug check
            if (!responseString.Trim().StartsWith("["))
                throw new Exception("Elasticsearch did not return JSON: " + responseString);

            var indexesDetailsResponses =
                JsonConvert.DeserializeObject<List<DTOIndexesDetailsResponse>>(responseString);
            var userIndices = indexesDetailsResponses
                            .Where(i => !i.index.StartsWith("."))
                            .ToList();

            return userIndices ?? new List<DTOIndexesDetailsResponse>();
        }

        public async Task<List<FileSource>> GetDocDetailsByIndexName(
       string baseUrl,
       string indexName,
       string userName,
       string password)
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            using var client = CreateHttpClient(userName, password);
            var query = new
            {
                _source = new[]
                {
        "file.FileName",
        "file.Extension",
        "path.real",
        "url"
    },
                query = new
                {
                    match_all = new { }
                },
                size = 100
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
                JsonConvert.DeserializeObject<ElasticsearchSearchResponse>(json);

            return result?.Hits?.Hitss?
                .Select(x => x.Source)
                .ToList()
                ?? new List<FileSource>();
        }
    }
}