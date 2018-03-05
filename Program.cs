using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text.RegularExpressions;

namespace JWorkLog
{
    class Program
    {
        const string apiRoot = "http://jira.lawson.com/rest/api/latest/";
        static void Main(string[] args)
        {
            Issue[] issueList;
            var query = "Labels=RunGroup";
            int argType = 0;
            int monthOffset = -1;
            bool verbose = false;
            
            foreach (var arg in args)
            {
                if (arg == "-q" || arg == "/q")
                    argType = 1; // Specify query
                else if (arg == "-o" || arg == "/o")
                    argType = 2; // Month offset
                else if (arg == "-v" || arg == "/v")
                    verbose = true;
                else if (arg == "-h" || arg == "--help" || arg == "/h" || arg == "/help" || arg == "/?" || arg == "-?")
                {
                    Console.WriteLine("JWorkLog [-q \"<JIRA query>\"] [-o <month offset>] [-h]\n"
                        + "   -q   Specifies a JIRA query the same as JIRA's advanced view.\n"
                        + "        The default query is Labels=RunGroups.\n"
                        + "   -o   Specified month offset from current month. Default is -1.\n"
                        + "   -h   Displays this help page.\n"
                        + "   -v   Verbose includes query string and other info in output.\n\n"
                        + "   Example: JWorkLog -q \"updated>=-8w AND project=HCM AND type=Bug AND component=\"GHR- Core HR\"\" -o -2 > output.csv");
                    return;
                }
                else if (argType == 1)
                    query = arg;
                else if (argType == 2)
                    monthOffset = int.Parse(arg);
            }
            if (verbose)
            {
                Console.WriteLine($"Query=\"{query}\"");
            }
            issueList = GetIssueList(query);
            if (verbose)
            {
                Console.WriteLine($"{issueList.Length} issues queried.");
            }
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(monthOffset);
            var endDate = startDate.AddMonths(1);
            var hours = new TimeTrack();
            Regex reworkexp = new Regex(@"^rework\s*\(\s*(\d+)\s*\)", RegexOptions.IgnoreCase);
            foreach (var issue in issueList)
            {
                if (issue.Fields.Worklog == null)
                    continue;
                foreach (var workEntry in issue.Fields.Worklog.Worklogs)
                {
                    if (workEntry.Started < startDate || workEntry.Started >= endDate)
                        continue;
                    if (!hours.TryGetValue(workEntry.Author.DisplayName, out IssueEntries userIssues))
                        hours.Add(workEntry.Author.DisplayName,
                            userIssues = new IssueEntries());
                    if (!userIssues.TryGetValue(issue.Key, out IssueEntry issueTimes))
                    {
                        userIssues.Add(issue.Key,
                            issueTimes = new IssueEntry() {
                                Key=issue.Key,
                                Summary=issue.Fields.Summary,
                                Remarks=issue.Fields.Status.Name,
                                EstimatedTime=TimeSpan.FromSeconds(issue.Fields.Timetracking.OriginalEstimateSeconds.GetValueOrDefault())
                            });
                    }
                    var reworkSpec = reworkexp.Match(workEntry.Comment);
                    int reworkCount = 0;
                    bool IsRework = true;
                    if (reworkSpec.Success)
                    {
                        reworkCount = int.Parse(reworkSpec.Groups[1].Captures[0].Value);
                    } else if (workEntry.Comment.StartsWith("rework", StringComparison.InvariantCultureIgnoreCase))
                    {
                        reworkCount = 1;
                    } else if (workEntry.Comment.IndexOf("rework", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                    }
                    else IsRework = false;

                    issueTimes.Add(new TimeEntry() {
                        AddReworks = reworkCount,
                        IsRework = IsRework,
                        Start = workEntry.Started,
                        Length =TimeSpan.FromSeconds(workEntry.TimeSpentSeconds)
                    });
                }
            }

            Console.WriteLine("Sequence,Developer Name,Deliverable Name,Summary,No. of Peer Review Defects," +
                "No. of Customer Review Defects,Planned Effort (Hrs.),Actual Effort (Hrs.) in {0}," +
                "Rework Effort To Fix Peer Review Comments (Hrs.)," +
                "Rework Effort To Fix Customer Review Comments (Hrs.),Remarks",
                System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(startDate.Month));

            int sequence = 0;
            foreach(var userIssue in hours)
            {
                foreach(var issueTime in userIssue.Value)
                {
                    Console.Write($"{++sequence},{userIssue.Key},{issueTime.Key},{issueTime.Value.Summary}");
                    TimeEntry normal = new TimeEntry(), rework = new TimeEntry();
                    int reworkCount = 0;
                    foreach(var entry in issueTime.Value)
                    {
                        if (entry.IsRework)
                            rework.Length += entry.Length;
                        else
                            normal.Length += entry.Length;
                        reworkCount += entry.AddReworks;
                    }
                    Console.Write($",{reworkCount},0,{issueTime.Value.EstimatedTime.TotalHours}");
                    Console.Write($",{normal.Length.TotalHours},{rework.Length.TotalHours}");
                    Console.WriteLine($",0,{issueTime.Value.Remarks}");
                }
            }
        }

        static Issue[] GetIssueList(string query)
        {
            string search = apiRoot + $"search?jql={query}&fields=key,worklog,timetracking,summary,status&maxResults=200";
            var wc = new System.Net.WebClient();
            var jsonIssues = wc.DownloadString(search);
            var result = IssueList.FromJson(jsonIssues);
            if (result.MaxResults < result.Total)
            {
                Console.WriteLine($"WARNING: Number of results {result.Total} exceeded max {result.MaxResults}.");
                Console.WriteLine("Results are incomplete.");
            }
            return result.Issues;
        }
    }

    class TimeTrack : Dictionary<string, IssueEntries>
    {
    }

    class IssueEntries : Dictionary<string, IssueEntry>
    {
    }
    class IssueEntry : List<TimeEntry>
    {
        public string Key { get; set; }
        public string Summary { get; set; }
        public string Remarks { get; set; }
        public TimeSpan EstimatedTime { get; set; }
    }
    class TimeEntry
    {
        public bool IsRework;
        public int AddReworks;
        public DateTime Start;
        public TimeSpan Length;
    }
}
