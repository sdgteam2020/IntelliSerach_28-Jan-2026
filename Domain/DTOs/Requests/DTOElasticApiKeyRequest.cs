using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.DTOs.Requests
{
    public class DTOElasticApiKeyRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("expiration")]
        public string? Expiration { get; set; }

        [JsonPropertyName("role_descriptors")]
        public Dictionary<string, ApiKeyRoleDescriptor> RoleDescriptors { get; set; } = new();
    }

    public class ApiKeyRoleDescriptor
    {
        [JsonPropertyName("cluster")]
        public List<string> Cluster { get; set; } = new();

        [JsonPropertyName("indices")]
        public List<ApiKeyIndexPermission> Indices { get; set; } = new();

        [JsonPropertyName("applications")]
        public List<ApiKeyApplicationPermission> Applications { get; set; } = new();

        [JsonPropertyName("run_as")]
        public List<string> RunAs { get; set; } = new();
    }

    public class ApiKeyIndexPermission
    {
        [JsonPropertyName("names")]
        public List<string> Names { get; set; } = new();

        [JsonPropertyName("privileges")]
        public List<string> Privileges { get; set; } = new();
    }

    public class ApiKeyApplicationPermission
    {
        [JsonPropertyName("application")]
        public string Application { get; set; } = string.Empty;

        [JsonPropertyName("privileges")]
        public List<string> Privileges { get; set; } = new();

        [JsonPropertyName("resources")]
        public List<string> Resources { get; set; } = new();
    }
}
