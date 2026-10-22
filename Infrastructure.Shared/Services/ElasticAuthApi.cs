using Application.Interfaces;
using Azure.Core;
using Domain.DTOs.Requests;
using Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Shared.Services
{
    public class ElasticAuthApi : IElasticAuthApi
    {
        private readonly HttpClient _httpClient;

        public ElasticAuthApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DTOElasticApiKeyResponse> LoginElastic(DTOElasticApiKeyRequest dTOElasticApiKeyRequest, string baseUrl, string username,  string password)
        {
            ArgumentNullException.ThrowIfNull(dTOElasticApiKeyRequest);

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new ArgumentException("A valid Elasticsearch base URL is required.", nameof(baseUrl));

            var apiKeyUri = new Uri(baseUri, "/_security/api_key");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiKeyUri)
            {
                Content = JsonContent.Create(dTOElasticApiKeyRequest)
            };

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{username}:{password}"));

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);

            using var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Elasticsearch API-key request failed ({(int)response.StatusCode}): {responseBody}");

            return JsonSerializer.Deserialize<DTOElasticApiKeyResponse>(
                       responseBody,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? throw new InvalidOperationException("Elasticsearch returned an empty or invalid API-key response.");
        }
    }
}
