using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Domain.DTOs.Requests
{
    public class DTOSearchQueryRequest
    {
        public int from { get; set; }

        public int size { get; set; }

        public SearchQuery query { get; set; }

        public Highlight highlight { get; set; }
    }
    public class SearchQuery
    {
        public SearchBoolQuery @bool { get; set; }
    }
    public class SearchBoolQuery
    {
        public List<object> should { get; set; }

        public List<object> filter { get; set; }

        public int minimum_should_match { get; set; }
    }
    public class MultiMatchWrapper
    {
        public MultiMatchQuery multi_match { get; set; }
    }
    public class MultiMatchQuery
    {
        public string query { get; set; }

        public string type { get; set; }

        public List<string> fields { get; set; }

        [JsonProperty(
            NullValueHandling = NullValueHandling.Ignore)]
        public double? boost { get; set; }

        [JsonProperty(
            NullValueHandling = NullValueHandling.Ignore)]
        public string fuzziness { get; set; }

        [JsonProperty(
            NullValueHandling = NullValueHandling.Ignore)]
        public bool? fuzzy_transpositions { get; set; }
    }
    public class Highlight
    {
        public string[] pre_tags { get; set; }

        public string[] post_tags { get; set; }

        public Dictionary<string, HighlightField> fields { get; set; }
    }
    public class HighlightField
    {
        [JsonProperty(
            NullValueHandling = NullValueHandling.Ignore)]
        public int? fragment_size { get; set; }

        public int number_of_fragments { get; set; }
        public string? type { get; set; }
        public string? boundary_scanner { get; set; }
    }
 

    //public class DTOSearchQueryRequest
    //{
    //    public int from { get; set; }
    //    public int size { get; set; }
    //    public Query query { get; set; }
    //    public Highlight highlight { get; set; }
    //}
    //public class Query
    //{
    //    public BoolQuery Bool { get; set; }
    //}
    //public class BoolQuery
    //{
    //    public List<ShouldQuery> should { get; set; }
    //}
    //public class ShouldQuery
    //{
    //    public MatchPhraseQuery match_phrase { get; set; }
    //    public MatchQuery match { get; set; }
    //}

    //public class MatchPhraseQuery
    //{
    //    public FieldQuery content { get; set; }
    //}
    //public class MatchQuery
    //{
    //    public FieldQuery content { get; set; }
    //}
    //public class FieldQuery
    //{
    //    public string query { get; set; }
    //    public int boost { get; set; }
    //}
    //public class Highlight
    //{
    //    public Dictionary<string, object> fields { get; set; }
    //}
}