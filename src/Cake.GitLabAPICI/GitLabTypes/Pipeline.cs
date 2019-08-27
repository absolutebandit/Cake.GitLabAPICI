namespace Cake.GitLabAPICI.GitLabTypes
{
    using Newtonsoft.Json;

    public class Pipeline
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("ref")]
        public string Ref { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
