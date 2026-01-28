using DataTransferObject.DTO.Requests;
using DataTransferObject.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLogicsLayer.ScraperAPI
{
    public class API : IAPI
    {
        private readonly string APILoginURL = "https://192.168.10.206/pdf/api/login/";
        private readonly string APIcrawlURL = "https://192.168.10.206/pdf/api/crawl/";

        public async Task<DTOLoginAPIResponse> Getauthentication(DTOAPILoginRequest data)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var client = new HttpClient(handler);

                var json = JsonSerializer.Serialize(new
                {
                    username = data.username,
                    password = data.password
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(APILoginURL, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new DTOLoginAPIResponse
                    {
                        Status = false,
                        Message = $"API Error {response.StatusCode}: {rawResponse}"
                    };
                }

                var result = JsonSerializer.Deserialize<DTOLoginAPIResponse>(rawResponse);

                return result ?? new DTOLoginAPIResponse
                {
                    Status = false,
                    Message = "Empty response from API."
                };
            }
            catch (Exception ex)
            {
                return new DTOLoginAPIResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<DTOScraperDataResponse> GetData(DTOScraperDataRequest Data)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                using var client = new HttpClient(handler);

                // ===== Headers (Same as JS) =====
                client.DefaultRequestHeaders.Add("X-CSRFToken", Data.CSRFToken);
                client.DefaultRequestHeaders.Add("Cookie","csrftoken="+Data.CSRFToken+"; sessionid="+Data.session_key+"");

                // ===== Body =====
                var payload = new
                {
                    url = Data.url,
                    max_pdfs = Data.max_pdfs
                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ===== POST Request =====
                var response = await client.PostAsync(APIcrawlURL, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new DTOScraperDataResponse
                    {
                        //Status = false,
                        message = $"API Error {(int)response.StatusCode}: {rawResponse}"
                    };
                }

                var result = JsonSerializer.Deserialize<DTOScraperDataResponse>(rawResponse);

                return result ?? new DTOScraperDataResponse
                {
                    //Status = false,
                    message = "Empty response from API."
                };
            }
            catch (Exception ex)
            {
                return new DTOScraperDataResponse
                {
                    //Status = false,
                    message = ex.Message
                };
            }
        }
    }
}
