using Application.Interfaces;
using Domain.DTOs.Requests;
using Domain.DTOs.Requests.API;
using Domain.DTOs.Response;
using Domain.DTOs.Response.API;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Shared.Services
{
    public class API : IAPI
    {
        public async Task<DTOAuthApiResponse> Getauthentication(DTOAPILoginRequest data, string APILoginURL)
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
                    client_id = data.client_id,
                    client_secret = data.client_secret
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(APILoginURL, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new DTOAuthApiResponse
                    {
                        Status = false,
                        Message = $"API Error {response.StatusCode}: {rawResponse}"
                    };
                }

                var result = JsonSerializer.Deserialize<DTOAuthApiResponse>(rawResponse);

                if (result == null)
                {
                    return result ?? new DTOAuthApiResponse
                    {
                        Status = false,
                        Message = "Empty response from API."
                    };
                }
                else
                {
                    result.Message = "Login successful.";
                    result.Status = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                return new DTOAuthApiResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<DTOCrawlerCrawlStatusResponse> GetCrawlerCrawlStatus(string Job_Id,string Token,string StatusUrl)
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
               

                // ===== Body =====
                var payload = new
                {
                    job_id = Job_Id,
                  
                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromMinutes(5);
                // ===== POST Request =====
                var response = await client.PostAsync(StatusUrl, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var result1 = JsonSerializer.Deserialize<DTOCrawlerCrawlStatusResponse>(rawResponse);

                    return new DTOCrawlerCrawlStatusResponse
                    {

                        Code = 400,
                        message = "API ERROR : " + result1.message
                    };


                }
                var result = JsonSerializer.Deserialize<DTOCrawlerCrawlStatusResponse>(rawResponse);


                return result ?? new DTOCrawlerCrawlStatusResponse
                {
                    Code = 400,
                    message = "Empty response from API."
                };
            }
            catch (HttpRequestException ex)
            {
                return new DTOCrawlerCrawlStatusResponse
                {
                    Code = 400,
                    message =
                        "HTTP Connection Error: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
            catch (TaskCanceledException ex)
            {
                return new DTOCrawlerCrawlStatusResponse
                {
                    Code = 408,
                    message = "API request timed out: " + ex.Message
                };
            }
            catch (Exception ex)
            {
                return new DTOCrawlerCrawlStatusResponse
                {
                    Code = 400,
                    message =
                        "Exception: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
        }
        public async Task<DTOCrawlerCrawlErrorResponse> GetCrawlerCrawlErros(string Job_Id, string Token, string StatusUrl)
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                //client.DefaultRequestHeaders.Add("X-CSRFToken", Data.CSRFToken);
                //client.DefaultRequestHeaders.Add("Cookie", "csrftoken=" + Data.CSRFToken + "; sessionid=" + Data.session_key + "");

                // ===== Body =====
                var payload = new
                {
                    job_id = Job_Id,

                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromMinutes(5);
                // ===== POST Request =====
                var response = await client.PostAsync(StatusUrl, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var result1 = JsonSerializer.Deserialize<DTOCrawlerCrawlErrorResponse>(rawResponse);

                    return new DTOCrawlerCrawlErrorResponse
                    {

                        Code = 400,
                        message = "API ERROR : " + result1.message
                    };


                }
                var result = JsonSerializer.Deserialize<DTOCrawlerCrawlErrorResponse>(rawResponse);


                return result ?? new DTOCrawlerCrawlErrorResponse
                {
                    Code = 400,
                    message = "Empty response from API."
                };
            }
            catch (HttpRequestException ex)
            {
                return new DTOCrawlerCrawlErrorResponse
                {
                    Code = 400,
                    message =
                        "HTTP Connection Error: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
            catch (TaskCanceledException ex)
            {
                return new DTOCrawlerCrawlErrorResponse
                {
                    Code = 408,
                    message = "API request timed out: " + ex.Message
                };
            }
            catch (Exception ex)
            {
                return new DTOCrawlerCrawlErrorResponse
                {
                    Code = 400,
                    message =
                        "Exception: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
        }
        public async Task<DTOScraperDataResponse> GetData(DTOScraperDataRequest Data, string APIcrawlURL)
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
               // /client.DefaultRequestHeaders.Add("X-CSRFToken", Data.CSRFToken);
               client.DefaultRequestHeaders.Add("X-API-Key", Data.session_key);
               // client.DefaultRequestHeaders.Add("Cookie", "csrftoken=" + Data.CSRFToken + "; sessionid=" + Data.session_key + "");

                // ===== Body =====
                var payload = new
                {
                    start_url = Data.url,
                    //max_pdfs = Data.max_pdfs,
                    alias = Data.Abbreviation,
                    max_pages = Data.max_pdfs
                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromMinutes(5);
                // ===== POST Request =====
                var response = await client.PostAsync(APIcrawlURL, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if((int)response.StatusCode == 500)
                    {
                        return new DTOScraperDataResponse
                        {
                            //Status = false,
                            message = $"API Error {(int)response.StatusCode}: {rawResponse}"
                        };
                    }
                    var resulterror = JsonSerializer.Deserialize<ErrorResponseDto>(rawResponse);
                    if(resulterror==null)
                    {
                        return new DTOScraperDataResponse
                        {
                            //Status = false,
                            message = $"API Error {(int)response.StatusCode}: {rawResponse}"
                        };
                    }
                    return new DTOScraperDataResponse
                    {
                        //Status = false,
                        message = $"API Error {(int)response.StatusCode}: {resulterror.Errors[0]}"
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

        public async Task<DTOScrapyCrawlResponse> GetData(DTOWebScraperDataRequest Data, string APIcrawlseoURL)
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Data.session_key);
                //client.DefaultRequestHeaders.Add("X-CSRFToken", Data.CSRFToken);
                //client.DefaultRequestHeaders.Add("Cookie", "csrftoken=" + Data.CSRFToken + "; sessionid=" + Data.session_key + "");

                // ===== Body =====
                var payload = new
                {
                    start_url = Data.url,
                    //max_pdfs = Data.max_pdfs,
                    alias = Data.Abbreviation,
                    max_pages = Data.max_pages
                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromMinutes(5);
                // ===== POST Request =====
                var response = await client.PostAsync(APIcrawlseoURL, content);
              
                var rawResponse = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    var result1 = JsonSerializer.Deserialize<DTOValidationErrorResponse>(rawResponse);
                  
                        return new DTOScrapyCrawlResponse
                        {
                           
                            Code = 400,
                            Message = "API ERROR : "+result1.Detail.Message
                        };
                   
                   
                }
                var result = JsonSerializer.Deserialize<DTOScrapyCrawlResponse>(rawResponse);


                return result ?? new DTOScrapyCrawlResponse
                {
                    Code = 400,
                    Message = "Empty response from API."
                };
            }
            catch (HttpRequestException ex)
            {
                return new DTOScrapyCrawlResponse
                {
                    Code = 400,
                    Message =
                        "HTTP Connection Error: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
            catch (TaskCanceledException ex)
            {
                return new DTOScrapyCrawlResponse
                {
                    Code = 408,
                    Message = "API request timed out: " + ex.Message
                };
            }
            catch (Exception ex)
            {
                return new DTOScrapyCrawlResponse
                {
                    Code = 400,
                    Message =
                        "Exception: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
        }

        public async Task<DTOFilterResponse> GetFilter(DTOWebScraperDataRequest Data, string APIuniqueurls)
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
                client.DefaultRequestHeaders.Add("Cookie", "csrftoken=" + Data.CSRFToken + "; sessionid=" + Data.session_key + "");

                // ===== POST Request =====
                var response = await client.GetAsync(APIuniqueurls);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new DTOFilterResponse
                    {
                        //Status = false,
                        message = $"API Error {(int)response.StatusCode}: {rawResponse}"
                    };
                }

                var result = JsonSerializer.Deserialize<DTOFilterResponse>(rawResponse);

                return result ?? new DTOFilterResponse
                {
                    //Status = false,
                    message = "Empty response from API."
                };
            }
            catch (Exception ex)
            {
                return new DTOFilterResponse
                {
                    //Status = false,
                    message = ex.Message
                };
            }
        }

        public async Task<DTOPdfCrawlResultResponse> GetCrawlerPdf(string Job_Id, string Token, string StatusUrl)
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                //client.DefaultRequestHeaders.Add("X-CSRFToken", Data.CSRFToken);
                //client.DefaultRequestHeaders.Add("Cookie", "csrftoken=" + Data.CSRFToken + "; sessionid=" + Data.session_key + "");

                // ===== Body =====
                var payload = new
                {
                    job_id = Job_Id,

                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromMinutes(5);
                // ===== POST Request =====
                var response = await client.PostAsync(StatusUrl, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var result1 = JsonSerializer.Deserialize<DTOPdfCrawlResultResponse>(rawResponse);

                    return new DTOPdfCrawlResultResponse
                    {

                        Code = 400,
                        message = "API ERROR : " + result1.message
                    };


                }
                var result = JsonSerializer.Deserialize<DTOPdfCrawlResultResponse>(rawResponse);


                return result ?? new DTOPdfCrawlResultResponse
                {
                    Code = 400,
                    message = "Empty response from API."
                };
            }
            catch (HttpRequestException ex)
            {
                return new DTOPdfCrawlResultResponse
                {
                    Code = 400,
                    message =
                        "HTTP Connection Error: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
            catch (TaskCanceledException ex)
            {
                return new DTOPdfCrawlResultResponse
                {
                    Code = 408,
                    message = "API request timed out: " + ex.Message
                };
            }
            catch (Exception ex)
            {
                return new DTOPdfCrawlResultResponse
                {
                    Code = 400,
                    message =
                        "Exception: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
        }

        public async Task<DTOCrawlerPdfDownloadSummary> GetCrawlerDownloadPdf(DTOCrawlPdfDownloadRequest Data)
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Data.Token);


                // ===== Body =====
                var payload = new
                {
                    job_id= Data.job_id,

                    verify_ssl= Data.verify_ssl,

                    max_files= Data.max_files,

                    overwrite_existing= Data.overwrite_existing,

                    max_pdf_size_mb= Data.max_pdf_size_mb,

                    replace_if_size_changed= Data.replace_if_size_changed

                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromMinutes(5);
                // ===== POST Request =====
                var response = await client.PostAsync(Data.Url, content);

                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var result1 = JsonSerializer.Deserialize<DTOCrawlerPdfDownloadSummary>(rawResponse);

                    return new DTOCrawlerPdfDownloadSummary
                    {

                        Code = 400,
                        message = "API ERROR : " + result1.message
                    };


                }
                var result = JsonSerializer.Deserialize<DTOCrawlerPdfDownloadSummary>(rawResponse);


                return result ?? new DTOCrawlerPdfDownloadSummary
                {
                    Code = 400,
                    message = "Empty response from API."
                };
            }
            catch (HttpRequestException ex)
            {
                return new DTOCrawlerPdfDownloadSummary
                {
                    Code = 400,
                    message =
                        "HTTP Connection Error: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
            catch (TaskCanceledException ex)
            {
                return new DTOCrawlerPdfDownloadSummary
                {
                    Code = 408,
                    message = "API request timed out: " + ex.Message
                };
            }
            catch (Exception ex)
            {
                return new DTOCrawlerPdfDownloadSummary
                {
                    Code = 400,
                    message =
                        "Exception: " +
                        ex.Message +
                        " | Inner: " +
                        ex.InnerException?.Message
                };
            }
        }
    }
}