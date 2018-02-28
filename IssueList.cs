// Generated by:
// http://jira.lawson.com/rest/api/latest/search?jql=Labels=RunGroup&fields=key,worklog
// https://app.quicktype.io/

namespace JWorkLog
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class IssueList
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("startAt")]
        public long StartAt { get; set; }

        [JsonProperty("maxResults")]
        public long MaxResults { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("issues")]
        public Issue[] Issues { get; set; }
    }

    public partial class Issue
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }
    }

    public partial class Fields
    {
        [JsonProperty("worklog")]
        public FieldsWorklog Worklog { get; set; }
    }

    public partial class FieldsWorklog
    {
        [JsonProperty("startAt")]
        public long StartAt { get; set; }

        [JsonProperty("maxResults")]
        public long MaxResults { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("worklogs")]
        public WorklogElement[] Worklogs { get; set; }
    }

    public partial class WorklogElement
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("author")]
        public UserInfo Author { get; set; }

        [JsonProperty("updateAuthor")]
        public UserInfo UpdateAuthor { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonProperty("started")]
        public string Started { get; set; }

        [JsonProperty("timeSpent")]
        public string TimeSpent { get; set; }

        [JsonProperty("timeSpentSeconds")]
        public long TimeSpentSeconds { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class UserInfo
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("avatarUrls")]
        public Dictionary<string, string> AvatarUrls { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }

    public partial class IssueList
    {
        public static IssueList FromJson(string json) => JsonConvert.DeserializeObject<IssueList>(json, JWorkLog.Converter.Settings);
    }
}