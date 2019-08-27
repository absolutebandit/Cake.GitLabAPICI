namespace Cake.GitLabAPICI.GitLabTypes
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Commit
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("short_id")]
        public string ShortId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("parent_ids")]
        public IList<string> ParentIds { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("author_email")]
        public string AuthorEmail { get; set; }

        [JsonProperty("authored_date")]
        public DateTime AuthoredDate { get; set; }

        [JsonProperty("committer_name")]
        public string SubmitterName { get; set; }

        [JsonProperty("committer_email")]
        public string SubmitterEmail { get; set; }

        [JsonProperty("committed_date")]
        public DateTime SubmittedDate { get; set; }
    }
}
