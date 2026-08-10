using System.Text.Json.Serialization;

namespace Domain.DTOs.Response
{
    public class DTOScrapyCrawlResponse {
        [JsonPropertyName("job_id")] 
        public string? JobId { get; set; } 
        [JsonPropertyName("status")]
        public string? Status { get; set; } 
        [JsonPropertyName("message")] 
        public string? Message { get; set; }
        public int Code { get; set; }
        [JsonPropertyName("start_url")] 
        public string? StartUrl { get; set; } 
        [JsonPropertyName("normalized_url")] 
        public string? NormalizedUrl { get; set; } 
        [JsonPropertyName("alias")] 
        public string? Alias { get; set; } 
        [JsonPropertyName("index_name")] 
        public string? IndexName { get; set; } 
        [JsonPropertyName("registry_reused")] 
        public bool RegistryReused { get; set; } 
        [JsonPropertyName("continue_from_last")] 
        public bool ContinueFromLast { get; set; } 
        [JsonPropertyName("recovery_resumed")] 
        public bool RecoveryResumed { get; set; } 
        [JsonPropertyName("purge_applied")] 
        public bool PurgeApplied { get; set; } 
        [JsonPropertyName("deleted_existing_docs")] 
        public int DeletedExistingDocs { get; set; } 
        [JsonPropertyName("jobdir_path")] 
        public string? JobDirPath { get; set; } 
        [JsonPropertyName("status_url")] 
        public string? StatusUrl { get; set; } 
        [JsonPropertyName("logs_url")] 
        public string? LogsUrl { get; set; } 
        [JsonPropertyName("config_url")] 
        public string? ConfigUrl { get; set; }
 

    }
    public class DTOValidationErrorResponse { [JsonPropertyName("detail")] public DTOValidationErrorDetail? Detail { get; set; } }
    public class DTOValidationErrorDetail { [JsonPropertyName("code")] public string? Code { get; set; } [JsonPropertyName("message")] public string? Message { get; set; } [JsonPropertyName("details")] public DTOValidationErrorDetails? Details { get; set; } }
    public class DTOValidationErrorDetails { [JsonPropertyName("errors")] public List<DTOValidationError>? Errors { get; set; } }
    public class DTOValidationError { [JsonPropertyName("type")] public string? Type { get; set; } [JsonPropertyName("loc")] public List<string>? Loc { get; set; } [JsonPropertyName("msg")] public string? Msg { get; set; } [JsonPropertyName("input")] public string? Input { get; set; } [JsonPropertyName("ctx")] public DTOValidationErrorContext? Ctx { get; set; } }
    public class DTOValidationErrorContext { [JsonPropertyName("error")] public string? Error { get; set; } }
}