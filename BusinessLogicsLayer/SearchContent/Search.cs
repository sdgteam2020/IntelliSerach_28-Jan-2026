using DataTransferObject.DTO.Requests;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BusinessLogicsLayer.SearchContent
{
    public class Search : ISearch
    {
        public async Task<string> GetResponse(DTOSerchRequest Request, string Url, string UserName, string Password)
        {
            Request.Filter = Request.Filter == "All" ? "*" : Request.Filter;
            Request.Filter = Request.Filter.Replace("https://", "").Replace("http://", "").Trim();
            

            var filters = new List<object>();

            // ✅ CASE 1: ALL → path filter on asdc_new ONLY
            if (Request.Filter == "*!")
            {
                filters.Add(new
                {
                    terms = new
                    {
                        _index = new List<string> { "*" }
                    }
                });

                filters.Add(new
                {
                    @bool = new BoolFilter
                    {
                        should = new List<object>
            {
                new WildcardWrapper
                {
                    Wildcard = new Dictionary<string, string>
                    {
                       { "path.real", $"*\\\\{Request.Filter}\\\\*" }

                    }
                }
            },
                        minimum_should_match = 1
                    }
                });
            }
            else if (Request.Filter == "*")
            {
                // ✅ CASE 2: Specific domain → index filter ONLY
                var indexList = new List<string> { "*" };

                filters.Add(new
                {
                    terms = new
                    {
                        _index = indexList
                    }
                });
            }
            else if (!string.IsNullOrWhiteSpace(Request.Filter))
            {
                filters.Add(new
                {
                    @bool = new
                    {
                        should = new List<object>
            {
                // 🔹 1. seo_{Filter} → full index
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
                                    _index = new List<string> { $"seo_{Request.Filter}" }
                                }
                            }
                        }
                    }
                },

                // 🔹 2. asdc_new → ONLY path.real contains "asdc_new"
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
                                    _index = new List<string> { "asdc_new" }
                                }
                            },
                            new
                            {
                                wildcard = new Dictionary<string, string>
                                {
                                     { "path.real", $"*\\\\{Request.Filter}\\\\*" }
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

            //else if (!string.IsNullOrWhiteSpace(Request.Filter))
            //{
            //    // ✅ CASE 2: Specific domain → index filter ONLY
            //    var indexList = new List<string> { $"seo_{Request.Filter}" };

            //    filters.Add(new
            //    {
            //        terms = new
            //        {
            //            _index = indexList
            //        }
            //    });
            //}

            // Convert the elasticsearchQuery object to a JSON string
            var dto = new DTOSearchQueryRequest
            {
                from = Request.from,
                size = Request.size,
                min_score = 1.1,   // ✅ FILTER LOW RELEVANCE RESULTS
                query = new Query
                {
                    function_score = new FunctionScore
                    {
                        boost_mode = "multiply",
                        score_mode = "sum",
                        query = new FunctionScoreQuery
                        {
                            @bool = new BoolQuery
                            {
                                should = new List<object>
                    {
                        new MatchPhraseWrapper
                        {
                            match_phrase = new MatchPhraseQuerywithoutfuzzy
                            {
                                content = new ContentQuerywithoutfuzzy
                                {
                                    query = Request.DataString,
                                    boost = 1
                                }
                            }
                        },
                        new MatchWrapper
                        {
                            match = new MatchQuery
                            {
                                content = new ContentQuery
                                {
                                    query = Request.DataString,
                                    boost = 1,
                                    fuzziness= "AUTO",
                                    fuzzy_transpositions=true
                                }
                            }
                        }
                    },

                                filter = filters.Any() ? filters : null   // 👈 OPTIONAL FILTER
                            }
                        }
                    }
                },

                // ✅ HIGHLIGHT ADDED HERE
                highlight = new Highlight
                {
                    pre_tags = new[]
          {
            "<mark class=\"marks\">"
        },
                    post_tags = new[] { "</mark>" },
                    fields = new Dictionary<string, HighlightField>
        {
            {
                "content",
                new HighlightField
                {
                    fragment_size = 150,
                    number_of_fragments = 3
                }
            }
        }
                }
            };

            //"content", new
            //{
            //    pre_tags = new[] { "<strong style=\"background-color:green;color:white; font-weight:bold; padding:1px;\">" },
            //    post_tags = new[] { "</strong>" }
            //}

            string jsonBody = JsonConvert.SerializeObject(dto);

            // Add this line before making the request to bypass SSL certificate validation for all requests in this handler's lifetime
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // Create HttpClient with basic auth
            using var client = CreateHttpClient(UserName, Password);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(Url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // With the following corrected line:

                return responseString;
            }
            else
            {
                return "Not Found";
            }
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
    }
}