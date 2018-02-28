using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace JWorkLog
{
    class Program
    {
        const string apiRoot = "http://jira.lawson.com/rest/api/latest/";
        static void Main(string[] args)
        {
            Issue[] issueList;
            if (args.Length == 0)
                issueList = GetIssueList("Labels=RunGroup");
            else
                issueList = GetIssueList(args[0]);

            var hours = new TimeTrack();
            foreach (var issue in issueList)
            {
                if (issue.Fields.Worklog == null)
                    continue;
                foreach (var workEntry in issue.Fields.Worklog.Worklogs)
                {
                    if (!hours.TryGetValue(workEntry.Author.DisplayName, out IssueEntries userIssues))
                        hours.Add(workEntry.Author.DisplayName,
                            userIssues = new IssueEntries());
                    if (!userIssues.TryGetValue(issue.Key, out List<TimeEntry> issueTimes))
                        userIssues.Add(issue.Key,
                            issueTimes = new List<TimeEntry>());
                    issueTimes.Add(new TimeEntry() {
                        Start = Convert.ToDateTime(workEntry.Started),
                        Length =TimeSpan.FromSeconds(workEntry.TimeSpentSeconds)
                    });
                }
            }

            Console.Write("Developer,Issue");
            for(int monthIndex=0; monthIndex < 12; monthIndex++)
                Console.Write("," + System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[monthIndex]);
            Console.WriteLine();
            foreach(var userIssue in hours)
            {
                Console.WriteLine(userIssue.Key);
                foreach(var issueTime in userIssue.Value)
                {
                    Console.Write("," + issueTime.Key);
                    var byMonth = issueTime.Value.GroupBy((t) => t.Start.Month, (t) => t.Length).ToDictionary((m) => m.Key);
                    for(int month=1; month<=12; month++)
                    {
                        if (byMonth.ContainsKey(month))
                            Console.Write(",{0}", byMonth[month].Sum((t) => t.TotalHours));
                        else
                            Console.Write(",");
                    }
                    Console.WriteLine();
                }
            }
        }

        static Issue[] GetIssueList(string query)
        {
            string search = apiRoot + $"search?jql={query}&fields=key,worklog";
            var wc = new System.Net.WebClient();
            var jsonIssues = wc.DownloadString(search);
            return IssueList.FromJson(jsonIssues).Issues;
        }
    }

    class TimeTrack : Dictionary<string, IssueEntries>
    {
    }

    class IssueEntries : Dictionary<string, List<TimeEntry>> { }

    struct TimeEntry
    {
        public DateTime Start;
        public TimeSpan Length;
    }
}
