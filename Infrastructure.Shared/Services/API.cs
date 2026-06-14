using Application.Interfaces;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Shared.Services
{
    public class API : IAPI
    {
        public async Task<DTOLoginAPIResponse> Getauthentication(DTOAPILoginRequest data, string APILoginURL)
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

                if (result == null)
                {
                    return result ?? new DTOLoginAPIResponse
                    {
                        Status = false,
                        Message = "Empty response from API."
                    };
                }
                else
                {
                    result.message = "Login successful.";
                    result.Status = true;
                    return result;
                }
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
                client.DefaultRequestHeaders.Add("X-CSRFToken", Data.CSRFToken);
                client.DefaultRequestHeaders.Add("Cookie", "csrftoken=" + Data.CSRFToken + "; sessionid=" + Data.session_key + "");

                // ===== Body =====
                var payload = new
                {
                    url = Data.url,
                    max_pdfs = Data.max_pdfs,
                    abbr=Data.Abbreviation,
                    max_pages=100
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

        public async Task<DTOWebScraperDataResponse> GetData(DTOWebScraperDataRequest Data, string APIcrawlseoURL)
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

                // ===== Body =====
                var payload = new
                {
                    url = Data.url,
                    max_pages = Data.max_pages,
                    abbr = Data.Abbreviation
                };

                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromMinutes(5);
                // ===== POST Request =====
                var response = await client.PostAsync(APIcrawlseoURL, content);
              
                var rawResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 500)
                    {
                        return new DTOWebScraperDataResponse
                        {
                            //Status = false,
                            message = $"API Error {(int)response.StatusCode}: {rawResponse}"
                        };
                    }
                    var resulterror = JsonSerializer.Deserialize<ErrorResponseDto>(rawResponse);
                    if (resulterror == null)
                    {
                        return new DTOWebScraperDataResponse
                        {
                            //Status = false,
                            message = $"API Error {(int)response.StatusCode}: {rawResponse}"
                        };
                    }
                    return new DTOWebScraperDataResponse
                    {
                        //Status = false,
                        message = $"API Error {(int)response.StatusCode}: {resulterror.Errors[0]}"
                    };
                }

                var result = JsonSerializer.Deserialize<DTOWebScraperDataResponse>(rawResponse);

                return result ?? new DTOWebScraperDataResponse
                {
                    //Status = false,
                    message = "Empty response from API."
                };
            }
            catch (Exception ex)
            {
                return new DTOWebScraperDataResponse
                {
                    //Status = false,
                    message = ex.Message
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
    }
}