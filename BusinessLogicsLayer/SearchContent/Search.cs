using DataTransferObject.DTO.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLogicsLayer.SearchContent
{
    public class Search : ISearch
    {
        public async Task<string> GetResponse(DTOSerchRequest Request, string Url, string UserName, string Password)
        {

            // Convert the elasticsearchQuery object to a JSON string
            var dto = new DTOSearchQueryRequest
            {
                from=Request.from,
                size=Request.size,
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
                    }
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
