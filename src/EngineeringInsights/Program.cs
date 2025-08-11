using System.Text.Json;
using System.Text.Json.Serialization;
using EngineeringInsights.Models;
using EngineeringInsights.Services;

namespace EngineeringInsights;

/// <summary>
/// The program class
/// </summary>
internal class Program
{
    /// <summary>
    /// The Json Serializer Options
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

    /// <summary>
    /// Main
    /// </summary>
    public static async Task Main()
    {
        const string jiraBaseUrl = "https://your-domain.atlassian.net/rest/api/3";
        var jiraEmail = Environment.GetEnvironmentVariable("JIRA_EMAIL") ?? string.Empty;
        var jiraToken = Environment.GetEnvironmentVariable("JIRA_TOKEN") ?? string.Empty;
        var ghToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? string.Empty;

        var jira = new JiraService(jiraBaseUrl, jiraEmail, jiraToken);
        var github = new GitHubService(ghToken);

        var results = new List<InsightResult>
        {
            // Call all Jira insights here:
            await jira.CycleTimeAsync("ABC-123"),
            await jira.AverageLeadTimeAsync("status = Done AND project = XYZ"),
            await jira.TimeToRestoreServiceAsync(),
            await jira.VelocityAsync(123, "customfield_10004"),
            await jira.DefectRatioAsync(),
            await jira.SprintBurndownAsync(1, 123),
            await jira.IssueThroughputAsync(1),
            await jira.LeadTimeAsync("ABC-123"),
            await jira.ReopenedIssuesAsync(),
            await jira.IssueAgingAsync(),
            await jira.BlockedIssuesAsync(),
            await jira.TeamWorkloadBalanceAsync("projectkey"),
            await jira.IncidentFrequencyAsync(),
            await jira.CustomerIssueResolutionTimesAsync(),
            await jira.CrossTeamDependencyBottlenecksAsync(),
            await jira.UntriagedIssuesAsync(),
            await jira.HighPriorityIssueAgingAsync(),
            await jira.EpicProgressTrackingAsync("EPIC-123"),
            await jira.SprintGoalCompletionRatioAsync(123),
            await jira.SprintPredictabilityAsync(123),
            await jira.TechnicalDebtIssuesAsync(),
            await jira.DeveloperSatisfactionProxyAsync(),
            await jira.CrossRepoWorkCoordinationAsync(),
            await jira.TeamCollaborationEfficiencyAsync(),
            await jira.RefactoringEffortsAsync()
        };

        var allRepoResults = new List<RepoInsightsSummary>();

        var repos = github.GetAllRepos();
        foreach (var repo in repos)
        {
            var githubResults = new List<InsightResult>
            {
                await github.PullRequestMergeTimeAsync(repo),
                await github.CommitFrequencyAsync(repo, DateTime.UtcNow.AddDays(-30)),
                await github.PRSizeLinesAsync(repo),
                await github.BuildFailuresFrequencyAsync(repo),
                await github.CodeReviewTimeAsync(repo),
                await github.BranchActivityAsync(repo),
                await github.PRReviewCommentsAsync(repo),
                await github.CodeChurnRateAsync(repo, DateTime.UtcNow.AddDays(-30)),
                await github.CodeOwnershipMetricsAsync(repo, "/path/to/file"),
                await github.AutomationCoverageAsync(repo),
                await github.DeploymentFrequencyAsync(repo),
                await github.BlockedPRsAsync(repo),
                await github.CodeReviewParticipationAsync(repo),
                await github.DeveloperActivityHeatmapAsync(repo),
                await github.SecurityVulnerabilitiesAsync(repo),
                await github.RefactoringEffortsAsync(repo),
                await github.AvgTimeInCodeReviewAsync(repo),
                await github.ContributorOnboardingTimeAsync(repo),
                await github.FeatureToggleUsageAsync(repo),
                await github.DocumentationCoverageAsync(repo),
                await github.CodeMergeConflictsFrequencyAsync(repo),
                await github.TestAutomationFailuresAsync(repo),
                await github.ReleaseNotesCompletenessAsync(repo),
                await github.DependencyUpdatesFrequencyAsync(repo),
                await github.FeatureUsageFeedbackLoopAsync(repo),
                await github.TeamCollaborationEfficiencyAsync(repo)
            };

            githubResults.ForEach(r => r.Repo = repo.Name);

            allRepoResults.Add(new RepoInsightsSummary
            {
                Repo = repo.Name,
                InsightResults = githubResults
            });

        }

        var portfolio = new GitHubPortfolioInsights
        {
            AllRepoResults = allRepoResults
        };

        // Now you can get high-level summaries:
        var avgMetrics = portfolio.Averages;
        Console.WriteLine(avgMetrics);
        var totalMetrics = portfolio.Totals;
        Console.WriteLine(totalMetrics);

        // Or report per-repo detail:
        foreach (var summary in portfolio.AllRepoResults)
        {
            Console.WriteLine($"Repo: {summary.Repo}");
            foreach (var result in summary.InsightResults)
                Console.WriteLine($"  {result.InsightType}: {result.Value}");
        }

        // Output everything as JSON
        JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        var serializedOutput = JsonSerializer.Serialize(allRepoResults, JsonSerializerOptions);
        Console.WriteLine(serializedOutput);
    }
}