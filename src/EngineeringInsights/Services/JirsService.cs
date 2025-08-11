using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EngineeringInsights.Models;

namespace EngineeringInsights.Services;

/// <summary>
/// The jira service class
/// </summary>
public class JiraService
{
    /// <summary>
    /// The http
    /// </summary>
    private readonly HttpClient _http;

    /// <summary>
    /// The base url
    /// </summary>
    private readonly string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="JiraService"/> class
    /// </summary>
    /// <param name="baseUrl">The base url</param>
    /// <param name="email">The email</param>
    /// <param name="apiToken">The api token</param>
    public JiraService(string baseUrl, string email, string apiToken)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _http = new HttpClient();
        var byteArray = Encoding.ASCII.GetBytes($"{email}:{apiToken}");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Searches the issues using the specified jql
    /// </summary>
    /// <param name="jql">The jql</param>
    /// <param name="fields">The fields</param>
    /// <returns>A task containing the json document</returns>
    private async Task<JsonDocument> SearchIssuesAsync(string jql, string[]? fields = null)
    {
        var url = $"{_baseUrl}/search";
        var payload = new { jql, fields = fields ?? [] };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var res = await _http.PostAsync(url, content);
        res.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await res.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Gets the all issues paged using the specified jql
    /// </summary>
    /// <param name="jql">The jql</param>
    /// <param name="maxPerPage">The max per page</param>
    /// <returns>The all issues</returns>
    public async Task<List<JsonElement>> GetAllIssuesPagedAsync(string jql, int maxPerPage = 50)
    {
        var allIssues = new List<JsonElement>();
        int startAt = 0;
        while (true)
        {
            var payload = new { jql, startAt, maxResults = maxPerPage };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync($"{_baseUrl}/search", content);
            res.EnsureSuccessStatusCode();
            var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            var issues = doc.RootElement.GetProperty("issues").EnumerateArray().ToList();
            allIssues.AddRange(issues);
            int total = doc.RootElement.GetProperty("total").GetInt32();
            startAt += issues.Count;
            if (startAt >= total) break;
        }

        return allIssues;
    }

    /// <summary>
    /// Velocities the sprint id
    /// </summary>
    /// <param name="sprintId">The sprint id</param>
    /// <param name="storyPointsField">The story points field</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> VelocityAsync(int sprintId, string storyPointsField)
    {
        var issues = await GetAllIssuesPagedAsync($"sprint = {sprintId} AND issuetype = Story AND status = Done");
        double totalPoints = issues
            .Select(i =>
                i.GetProperty("fields").TryGetProperty(storyPointsField, out var sp) &&
                sp.ValueKind == JsonValueKind.Number
                    ? sp.GetDouble()
                    : 0)
            .Sum();
        return new InsightResult { InsightType = InsightType.Velocity, Value = totalPoints, Source = "Jira" };
    }

    /// <summary>
    /// Cycles the time using the specified issue key
    /// </summary>
    /// <param name="issueKey">The issue key</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CycleTimeAsync(string issueKey)
    {
        var res = await _http.GetAsync($"{_baseUrl}/issue/{issueKey}?expand=changelog");
        res.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        var changelog = doc.RootElement.GetProperty("changelog");
        var start = JiraChangelogHelper.GetFirstStatusTransitionTime(changelog, "In Progress");
        var end = JiraChangelogHelper.GetFirstStatusTransitionTime(changelog, "Done");
        return new InsightResult
        {
            InsightType = InsightType.CycleTime,
            Value = (start.HasValue && end.HasValue) ? (end.Value - start.Value).TotalHours : null, Source = "Jira"
        };
    }

    /// <summary>
    /// Defects the ratio
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> DefectRatioAsync()
    {
        var bugs = await GetAllIssuesPagedAsync("issuetype = Bug");
        var completed = await GetAllIssuesPagedAsync("status = Done AND issuetype in (Story, Task)");
        return new InsightResult
        {
            InsightType = InsightType.DefectRatio,
            Value = completed.Count > 0 ? (double)bugs.Count / completed.Count : 0, Source = "Jira"
        };
    }

    /// <summary>
    /// Sprints the burndown using the specified board id
    /// </summary>
    /// <param name="boardId">The board id</param>
    /// <param name="sprintId">The sprint id</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> SprintBurndownAsync(int boardId, int sprintId)
    {
        var url = $"{_baseUrl.Replace("/api/3", "")}/agile/1.0/board/{boardId}/sprint/{sprintId}/burndown";
        var res = await _http.GetAsync(url);
        res.EnsureSuccessStatusCode();
        return new InsightResult
        {
            InsightType = InsightType.SprintBurndown, Value = await res.Content.ReadAsStringAsync(), Source = "Jira"
        };
    }

    /// <summary>
    /// Issues the throughput using the specified days
    /// </summary>
    /// <param name="days">The days</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> IssueThroughputAsync(int days)
    {
        return new InsightResult
        {
            InsightType = InsightType.IssueThroughput,
            Value = (await GetAllIssuesPagedAsync($"status = Done AND resolved >= -{days}d")).Count, Source = "Jira"
        };
    }

    /// <summary>
    /// Leads the time using the specified issue key Description = "Time in hours from created to Done" 
    /// </summary>
    /// <param name="issueKey">The issue key</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> LeadTimeAsync(string issueKey)
    {
        // Lead time = Created to Done
        var res = await _http.GetAsync($"{_baseUrl}/issue/{issueKey}?expand=changelog");
        res.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        DateTime created = DateTime.Parse(doc.RootElement.GetProperty("fields").GetProperty("created").GetString());
        DateTime? done = null;
        foreach (var hist in doc.RootElement.GetProperty("changelog").GetProperty("histories").EnumerateArray())
        {
            foreach (var item in hist.GetProperty("items").EnumerateArray())
            {
                if (item.GetProperty("field").GetString() == "status" &&
                    item.GetProperty("toString").GetString() == "Done")
                    done = DateTime.Parse(hist.GetProperty("created").GetString());
            }
        }

        double? hours = (done.HasValue) ? (done.Value - created).TotalHours : null;
        return new InsightResult
            { InsightType = InsightType.LeadTime, Value = hours, Source = "Jira" };
    }

    /// <summary>
    /// Averages the lead time using the specified jql
    /// </summary>
    /// <param name="jql">The jql</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> AverageLeadTimeAsync(string jql)
    {
        var issues = await GetAllIssuesPagedAsync(jql);
        var times = issues
            .Where(i => i.GetProperty("fields").TryGetProperty("resolutiondate", out var rd) &&
                        rd.ValueKind == JsonValueKind.String)
            .Select(i =>
                (DateTime.Parse(i.GetProperty("fields").GetProperty("resolutiondate").GetString()) -
                 DateTime.Parse(i.GetProperty("fields").GetProperty("created").GetString())).TotalHours)
            .ToList();
        return new InsightResult
        {
            InsightType = InsightType.AverageLeadTime, Value = times.Count != 0 ? times.Average() : null,
            Source = "Jira"
        };
    }

    /// <summary>
    /// Reopeneds the issues
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> ReopenedIssuesAsync() =>
        new InsightResult
        {
            InsightType = InsightType.ReopenedIssues,
            Value = (await GetAllIssuesPagedAsync("status WAS Done AND status = Reopened")).Count, Source = "Jira"
        };

    /// <summary>
    /// Issues the aging
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> IssueAgingAsync() =>
        new InsightResult
        {
            InsightType = InsightType.IssueAging,
            Value = (await GetAllIssuesPagedAsync("status != Done")).Average(i =>
                (DateTime.UtcNow - DateTime.Parse(i.GetProperty("fields").GetProperty("created").GetString()))
                .TotalDays),
            Source = "Jira"
        };

    /// <summary>
    /// Blockeds the issues
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> BlockedIssuesAsync() =>
        new InsightResult
        {
            InsightType = InsightType.BlockedIssues, Value = (await GetAllIssuesPagedAsync("Flagged = Impediment")).Count,
            Source = "Jira"
        };

    /// <summary>
    /// Teams the workload balance using the specified project key
    /// </summary>
    /// <param name="projectKey">The project key</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> TeamWorkloadBalanceAsync(string projectKey)
    {
        var issues = await GetAllIssuesPagedAsync($"project = {projectKey} AND status != Done");
        var dict = issues.GroupBy(i =>
                i.GetProperty("fields").TryGetProperty("assignee", out var a) && a.ValueKind != JsonValueKind.Null
                    ? a.GetProperty("displayName").GetString()
                    : "Unassigned")
            .ToDictionary(g => g.Key, g => g.Count());
        return new InsightResult { InsightType = InsightType.TeamWorkloadBalance, Value = dict, Source = "Jira" };
    }

    /// <summary>
    /// Incidents the frequency
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> IncidentFrequencyAsync() =>
        new InsightResult
        {
            InsightType = InsightType.IncidentFrequency, Value = (await GetAllIssuesPagedAsync("issuetype = Bug")).Count,
            Source = "Jira"
        };

    /// <summary>
    /// Times the to restore service
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> TimeToRestoreServiceAsync()
    {
        var issues = await GetAllIssuesPagedAsync("issuetype = Incident AND status = Resolved");
        var times = issues
            .Where(i => i.GetProperty("fields").TryGetProperty("resolutiondate", out var rd) &&
                        rd.ValueKind == JsonValueKind.String)
            .Select(i =>
                (DateTime.Parse(i.GetProperty("fields").GetProperty("resolutiondate").GetString()) -
                 DateTime.Parse(i.GetProperty("fields").GetProperty("created").GetString())).TotalHours)
            .ToList();
        return new InsightResult
            { InsightType = InsightType.TimeToRestoreService, Value = times.Any() ? times.Average() : null, Source = "Jira" };
    }

    /// <summary>
    /// Customers the issue resolution times
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CustomerIssueResolutionTimesAsync() =>
        new InsightResult
        {
            InsightType = InsightType.CustomerIssueResolutionTimes,
            Value = (await GetAllIssuesPagedAsync("project = SERVICEDESK AND status = Resolved")).Count, Source = "Jira"
        };

    /// <summary>
    /// Crosses the team dependency bottlenecks
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CrossTeamDependencyBottlenecksAsync() =>
        new InsightResult
        {
            InsightType = InsightType.CrossTeamBottlenecks,
            Value = (await GetAllIssuesPagedAsync("issue in linkedIssuesOf(\"type=Story\", \"is blocked by\")")).Count,
            Source = "Jira"
        };

    /// <summary>
    /// Untriageds the issues
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> UntriagedIssuesAsync() =>
        new InsightResult
        {
            InsightType = InsightType.UntriagedIssues, Value = (await GetAllIssuesPagedAsync("assignee IS EMPTY")).Count,
            Source = "Jira"
        };

    /// <summary>
    /// Highs the priority issue aging
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> HighPriorityIssueAgingAsync()
    {
        var issues = await GetAllIssuesPagedAsync("priority = High AND status != Done");
        return new InsightResult
        {
            InsightType = InsightType.HighPriorityIssueAging,
            Value = issues.Any()
                ? issues.Average(i =>
                    (DateTime.UtcNow - DateTime.Parse(i.GetProperty("fields").GetProperty("created").GetString()))
                    .TotalDays)
                : 0,
            Source = "Jira"
        };
    }

    /// <summary>
    /// Epics the progress tracking using the specified epic key
    /// </summary>
    /// <param name="epicKey">The epic key</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> EpicProgressTrackingAsync(string epicKey) =>
        new InsightResult
        {
            InsightType = InsightType.EpicProgressTracking, Value = (await GetAllIssuesPagedAsync($"\"Epic Link\" = {epicKey}")).Count,
            Source = "Jira"
        };

    /// <summary>
    /// Sprints the goal completion ratio using the specified sprint id
    /// </summary>
    /// <param name="sprintId">The sprint id</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> SprintGoalCompletionRatioAsync(int sprintId)
    {
        var committed = await GetAllIssuesPagedAsync($"sprint = {sprintId}");
        var done = committed.Count(i =>
            i.GetProperty("fields").GetProperty("status").GetProperty("name").GetString() == "Done");
        return new InsightResult
        {
            InsightType = InsightType.SprintGoalCompletionRatio, Value = committed.Count > 0 ? ((double)done / committed.Count) : 0,
            Source = "Jira"
        };
    }

    /// <summary>
    /// Sprints the predictability using the specified sprint id
    /// </summary>
    /// <param name="sprintId">The sprint id</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> SprintPredictabilityAsync(int sprintId)
    {
        // Shortcut for demo: reuse SprintGoalCompletionRatio
        return await SprintGoalCompletionRatioAsync(sprintId);
    }

    /// <summary>
    /// Technicals the debt issues
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> TechnicalDebtIssuesAsync() =>
        new InsightResult
        {
            InsightType = InsightType.TechnicalDebtIssues, Value = (await GetAllIssuesPagedAsync("labels = tech-debt")).Count,
            Source = "Jira"
        };

    /// <summary>
    /// Developers the satisfaction proxy
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> DeveloperSatisfactionProxyAsync() =>
        new InsightResult
            { InsightType = InsightType.DeveloperSatisfactionProxy, Value = "Use incident rate or survey integration", Source = "Jira" };

    /// <summary>
    /// Crosses the repo work coordination
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CrossRepoWorkCoordinationAsync() =>
        new InsightResult
            { InsightType = InsightType.CrossRepoWorkCoordination, Value = "Requires cross-repo issue linking", Source = "Jira" };

    /// <summary>
    /// Teams the collaboration efficiency
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> TeamCollaborationEfficiencyAsync() =>
        new InsightResult
        {
            InsightType = InsightType.TeamCollaborationEfficiency, Value = "Calculate avg comments, resolution speed", Source = "Jira"
        };

    /// <summary>
    /// Refactorings the efforts
    /// </summary>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> RefactoringEffortsAsync() =>
        new InsightResult
        {
            InsightType = InsightType.RefactoringEfforts, Value = (await GetAllIssuesPagedAsync("labels = refactor")).Count,
            Source = "Jira"
        };

    // If you need any other specific Jira insights, let me know!
}