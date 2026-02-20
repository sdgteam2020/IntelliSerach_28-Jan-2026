using Newtonsoft.Json;

namespace DataTransferObject.DTO.Requests
{
    public class DTOSearchQueryRequest
    {
        public int from { get; set; }          // pagination start
        public int size { get; set; }
        public Query query { get; set; }
        public Highlight highlight { get; set; }
        public double? min_score { get; set; }
    }

    public class Query
    {
        public FunctionScore function_score { get; set; }
    }

    public class FunctionScore
    {
        public FunctionScoreQuery query { get; set; }
        public string boost_mode { get; set; }
        public string score_mode { get; set; }
    }

    public class FunctionScoreQuery
    {
        public BoolQuery @bool { get; set; }
    }

    public class BoolQuery
    {
        public List<object> should { get; set; }
        public List<object> filter { get; set; }   // ✅ ADD THIS
    }
    public class IndexTermFilter
    {
        public string _index { get; set; }
    }

    public class IndexTermsFilter
    {
        public List<string> _index { get; set; }
    }
    public class BoolFilter
    {
        public List<object> should { get; set; }


        [JsonProperty("minimum_should_match")]
        public int minimum_should_match { get; set; }
    }

    public class WildcardWrapper
    {
        [JsonProperty("wildcard")]
        public object Wildcard { get; set; }
    }

    public class BoolWrapper
    {
        [JsonProperty("should")]
        public List<object> Should { get; set; }

        [JsonProperty("minimum_should_match")]
        public int MinimumShouldMatch { get; set; }
    }

    public class MatchPhraseWrapper
    {
        public MatchPhraseQuerywithoutfuzzy match_phrase { get; set; }
    }

    public class MatchWrapper
    {
        public MatchQuery match { get; set; }
    }

    public class MatchPhraseQuery
    {
        public ContentQuery content { get; set; }
    }

    public class MatchPhraseQuerywithoutfuzzy
    {
        public ContentQuerywithoutfuzzy content { get; set; }
    }

    public class MatchQuery
    {
        public ContentQuery content { get; set; }
    }

    public class ContentQuery
    {
        public string query { get; set; }
        public int boost { get; set; }
        public string fuzziness { get; set; }
        public bool fuzzy_transpositions { get; set; }
    }

    public class ContentQuerywithoutfuzzy
    {
        public string query { get; set; }
        public int boost { get; set; }
    }

    public class Highlight
    {
        public string[] pre_tags { get; set; }
        public string[] post_tags { get; set; }
        public Dictionary<string, HighlightField> fields { get; set; }
    }

    public class HighlightField
    {
        public int fragment_size { get; set; }
        public int number_of_fragments { get; set; }
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