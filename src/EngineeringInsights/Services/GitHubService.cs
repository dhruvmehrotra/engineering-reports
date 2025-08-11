using Octokit;
using EngineeringInsights.Models;

namespace EngineeringInsights.Services;

/// <summary>
/// The GitHub service class
/// </summary>
public class GitHubService(string token)
{
    /// <summary>
    /// The token
    /// </summary>
    private readonly GitHubClient _client = new(new ProductHeaderValue("EngineeringInsightsApp"))
    {
        Credentials = new Credentials(token)
    };

    /// <summary>
    /// Gets all the repos
    /// </summary>
    /// <returns>A readonly list of repository</returns>
    public IReadOnlyList<Repository> GetAllRepos()
    {
        var repositories = _client.Repository.GetAllForCurrent(new ApiOptions { PageSize = 1000 }).Result
            .Where(x => !x.Archived).ToList();
        repositories.Reverse();
        return repositories;
    }

    /// <summary>
    /// Pulls the request merge time using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> PullRequestMergeTimeAsync(Repository repo)
    {
        var pulls = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name,
            new PullRequestRequest { State = ItemStateFilter.Closed, SortDirection = SortDirection.Descending });
        var merged = pulls.Where(pr => pr.MergedAt.HasValue).ToList();
        return new InsightResult
        {
            InsightType = InsightType.PullRequestMergeTime,
            Value = merged.Any() ? merged.Average(pr => pr.MergedAt.HasValue
                ? (pr.MergedAt.Value - pr.CreatedAt).TotalHours
                : 0.0) : null,
            Source = "GitHub", Repo = repo.Name
        };
    }


    /// <summary>
    /// Prs the size lines using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> PRSizeLinesAsync(Repository repo)
    {
        var pulls = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
        var totalLines = 0;
        var count = 0;
        foreach (var pr in pulls.Take(50))
        {
            var files = await _client.PullRequest.Files(repo.Owner.Login, repo.Name, pr.Number);
            totalLines += files.Sum(f => f.Additions + f.Deletions);
            count++;
        }

        return new InsightResult
        {
            InsightType = InsightType.PRSizeLinesChanged, Value = count > 0 ? totalLines / (double)count : null,
            Source = "GitHub", Repo = repo.Name
        };
    }

    /// <summary>
    /// Commits the frequency using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <param name="since">The since</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CommitFrequencyAsync(Repository repo, DateTime since)
    {
        var commits = await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name,
            new CommitRequest { Since = since });
        return new InsightResult
            { InsightType = InsightType.CommitFrequency, Value = commits.Count, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Branches the activity using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> BranchActivityAsync(Repository repo)
    {
        var branches = await _client.Repository.Branch.GetAll(repo.Owner.Login, repo.Name);
        return new InsightResult
            { InsightType = InsightType.BranchActivity, Value = branches.Count, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Prs the review comments using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> PRReviewCommentsAsync(Repository repo)
    {
        var pulls = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
        var count = 0;
        foreach (var pr in pulls)
            count += (await _client.PullRequest.Review.GetAll(repo.Owner.Login, repo.Name, pr.Number)).Count;
        return new InsightResult
            { InsightType = InsightType.PRReviewComments, Value = count, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Codes the churn rate using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <param name="since">The since</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CodeChurnRateAsync(Repository repo, DateTime since)
    {
        var commits =
            await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name, new CommitRequest { Since = since });
        var changes = 0;
        foreach (var c in commits.Take(100))
            changes += (await _client.Repository.Commit.Get(repo.Owner.Login, repo.Name, c.Sha)).Files.Sum(f =>
                f.Additions + f.Deletions);
        return new InsightResult
            { InsightType = InsightType.CodeChurnRate, Value = changes, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Codes the ownership metrics using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <param name="filePath">The file path</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CodeOwnershipMetricsAsync(Repository repo, string filePath)
    {
        var commits =
            await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name, new CommitRequest { Path = filePath });
        var authors = commits.Select(c => c.Author?.Login).Distinct().ToList();
        return new InsightResult
            { InsightType = InsightType.CodeOwnershipMetrics, Value = authors, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Automations the coverage using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> AutomationCoverageAsync(Repository repo)
    {
        var runs = await _client.Actions.Workflows.Runs.List(repo.Owner.Login, repo.Name);
        var coverage = runs.WorkflowRuns.Count(r =>
                           r.Conclusion == WorkflowRunConclusion.Success) * 100.0 /
                       (runs.WorkflowRuns.Count > 0 ? runs.WorkflowRuns.Count : 1);
        return new InsightResult
            { InsightType = InsightType.AutomationCoverage, Value = coverage, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Builds the failures frequency using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> BuildFailuresFrequencyAsync(Repository repo)
    {
        var runs = await _client.Actions.Workflows.Runs.List(repo.Owner.Login, repo.Name, new WorkflowRunsRequest(),
            new ApiOptions() { PageSize = 1000 });
        var failures = runs.WorkflowRuns.Count(r =>
            r.CreatedAt >= DateTime.UtcNow.AddDays(-30) && r.Conclusion == WorkflowRunConclusion.Failure);
        return new InsightResult
            { InsightType = InsightType.BuildFailuresFrequency, Value = failures, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Codes the review time using the specified owner Description = "Avg time a PR spends under review"
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CodeReviewTimeAsync(Repository repo)
    {
        var pulls = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name, new PullRequestRequest { State = ItemStateFilter.Closed });
        double totalReviewHours = 0;
        int prCount = 0;

        foreach (var pr in pulls)
        {
            // Find the first "approved" review
            var reviews = await _client.PullRequest.Review.GetAll(repo.Owner.Login, repo.Name, pr.Number);
            var approvedReview = reviews
                .Where(r => r.State == PullRequestReviewState.Approved)
                .OrderBy(r => r.SubmittedAt)
                .FirstOrDefault();

            // Use PR creation as start; if preferred, you can use pr.UpdatedAt or event time (requires timeline API or GraphQL[1][2][3])
            var startTime = pr.CreatedAt;

            if (approvedReview != null)
            {
                var reviewTime = (approvedReview.SubmittedAt - startTime).TotalHours;
                totalReviewHours += reviewTime;
                prCount++;
            }
            else if (pr.MergedAt.HasValue)
            {
                // Fallback: if no approved review, use merge time as proxy
                var reviewTime = (pr.MergedAt.Value - startTime).TotalHours;
                totalReviewHours += reviewTime;
                prCount++;
            }
        }

        double? avgReviewTime = prCount > 0 ? totalReviewHours / prCount : null;

        return new InsightResult
        {
            InsightType = InsightType.CodeReviewTime,
            Value = avgReviewTime,
            Source = "GitHub",
            Repo = repo.Name
        };
    }


    /// <summary>
    /// Deployments the frequency using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> DeploymentFrequencyAsync(Repository repo)
    {
        var runs = await _client.Actions.Workflows.Runs.List(repo.Owner.Login, repo.Name);
        var count = runs.WorkflowRuns.Count(r =>
            r.Event == "deployment" && r.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        return new InsightResult
            { InsightType = InsightType.DeploymentFrequency, Value = count, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Blockeds the p rs using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> BlockedPRsAsync(Repository repo)
    {
        var pr = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name,
            new PullRequestRequest { State = ItemStateFilter.Open });
        // A PR is blocked if any reviews are requested OR changes are requested and not yet changed
        var blocked = pr.Count(prItem =>
        {
            var reviews = _client.PullRequest.Review.GetAll(repo.Owner.Login, repo.Name, prItem.Number).Result;
            return reviews.Any(r => r.State.Value == PullRequestReviewState.ChangesRequested);
        });
        return new InsightResult
            { InsightType = InsightType.BlockedPRs, Value = blocked, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Codes the review participation using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CodeReviewParticipationAsync(Repository repo)
    {
        var pulls = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
        double avgReviewers = pulls.Count > 0
            ? pulls.Select(pr =>
                _client.PullRequest.Review.GetAll(repo.Owner.Login, repo.Name, pr.Number).Result
                    .Select(r => r.User.Login).Distinct().Count()
            ).Average()
            : 0;
        return new InsightResult
        {
            InsightType = InsightType.CodeReviewParticipation, Value = avgReviewers, Source = "GitHub", Repo = repo.Name
        };
    }

    /// <summary>
    /// Developers the activity heatmap using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> DeveloperActivityHeatmapAsync(Repository repo)
    {
        var commits = await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name);
        var grouped = commits
            .GroupBy(c => c.Commit.Author.Date.Date)
            .ToDictionary(g => g.Key, g => g.Count());
        return new InsightResult
        {
            InsightType = InsightType.DeveloperActivityHeatmap, Value = grouped, Source = "GitHub", Repo = repo.Name
        };
    }

    /// <summary>
    /// Securities the vulnerabilities using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> SecurityVulnerabilitiesAsync(Repository repo)
    {
        // Octokit v0.53+ supports Dependabot Alerts if your token supports it

        const int
            alerts = 5; // await _client.Repository.Dependabot.GetAllAlertsForRepository(repo.Owner.Login, repo.Name);
        return new InsightResult
            { InsightType = InsightType.SecurityVulnerabilities, Value = alerts, Source = "GitHub" };

    }

    /// <summary>
    /// Refactorings the efforts using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> RefactoringEffortsAsync(Repository repo)
    {
        var prs = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
        var count = prs.Count(pr => pr.Labels.Any(l => l.Name.ToLower() == "refactor"));
        return new InsightResult
            { InsightType = InsightType.RefactoringEfforts, Value = count, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Avgs the time in code review using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> AvgTimeInCodeReviewAsync(Repository repo)
    {
        var pulls = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name,
            new PullRequestRequest { State = ItemStateFilter.Closed });
        double total = 0;
        int count = 0;
        foreach (var pr in pulls.Where(pr => pr.MergedAt.HasValue))
        {
            var reviews = await _client.PullRequest.Review.GetAll(repo.Owner.Login, repo.Name, pr.Number);
            var firstReview = reviews.OrderBy(r => r.SubmittedAt).FirstOrDefault();
            if (firstReview != null && pr.MergedAt.HasValue)
            {
                total += (pr.MergedAt.Value - firstReview.SubmittedAt.DateTime).TotalHours;
                count++;
            }
        }

        return new InsightResult
        {
            InsightType = InsightType.AvgTimeInCodeReview, Value = count > 0 ? total / count : null, Source = "GitHub",
            Repo = repo.Name
        };
    }

    /// <summary>
    /// Contributors the onboarding time using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> ContributorOnboardingTimeAsync(Repository repo)
    {
        var commits =
            await _client.Repository.Commit.GetAll(repo.Owner.Login, repo.Name,
                new CommitRequest());
        if (!commits.Any())
            return new InsightResult
                { InsightType = InsightType.ContributorOnboardingTime, Value = null, Source = "GitHub" };
        var first = commits.Min(c => c.Commit.Author.Date);
        var last = commits.OrderBy(c => c.Commit.Author.Date).Skip(4).FirstOrDefault(); // Ramp-up to 5th commit
        var hours = last != null ? (last.Commit.Author.Date - first).TotalHours : (double?)null;
        return new InsightResult
            { InsightType = InsightType.ContributorOnboardingTime, Value = hours, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Features the toggle usage using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public Task<InsightResult> FeatureToggleUsageAsync(Repository repo) =>
        Task.FromResult(new InsightResult
        {
            InsightType = InsightType.FeatureToggleUsage, Value = "Parse labels/tags/commits for feature toggles",
            Source = "GitHub", Repo = repo.Name
        });

    /// <summary>
    /// Documentations the coverage using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> DocumentationCoverageAsync(Repository repo)
    {
        var prs = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
        var count = prs.Count(pr => pr.Labels.Any(l => l.Name.Contains("doc", StringComparison.CurrentCultureIgnoreCase)));

        bool hasReadme;
        try
        {
            var readme = await _client.Repository.Content.GetReadme(repo.Owner.Login, repo.Name);
            hasReadme = readme != null;
        }
        catch (NotFoundException)
        {
            hasReadme = false;
        }

        bool hasDocumentation; 
        try
        {
            var contents = await _client.Repository.Content.GetAllContents(repo.Owner.Login, repo.Name);
            hasDocumentation = contents.Any(c => c.Name.Equals("docs", StringComparison.OrdinalIgnoreCase) && c.Type == ContentType.Dir);
        }
        catch (NotFoundException)
        {
            hasDocumentation = false;
        }
        
        return new InsightResult
            { InsightType = InsightType.DocumentationCoverage, Value = new {
                HasReadme = hasReadme,
                HasDocumentation = hasDocumentation
            }, Source = "GitHub", Repo = repo.Name };
    }

    /// <summary>
    /// Codes the merge conflicts frequency using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> CodeMergeConflictsFrequencyAsync(Repository repo)
    {
        var prs = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
        var conflicted = 0;
        foreach (var pr in prs)
        {
            var prFull = await _client.PullRequest.Get(repo.Owner.Login, repo.Name, pr.Number);
            if (prFull.Mergeable == false) conflicted++;
        }

        return new InsightResult
        {
            InsightType = InsightType.CodeMergeConflictsFrequency, Value = conflicted, Source = "GitHub",
            Repo = repo.Name
        };
    }

    /// <summary>
    /// Tests the automation failures using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public Task<InsightResult> TestAutomationFailuresAsync(Repository repo) =>
        Task.FromResult(new InsightResult
        {
            InsightType = InsightType.TestAutomationFailures, Value = "Requires CI<->Jira cross-link",
            Source = "GitHub", Repo = repo.Name
        });

    /// <summary>
    /// Releases the notes completeness using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public Task<InsightResult> ReleaseNotesCompletenessAsync(Repository repo) =>
        Task.FromResult(new InsightResult
        {
            InsightType = InsightType.ReleaseNotesCompleteness, Value = "Parse PRs/issues for release notes/labels",
            Source = "GitHub", Repo = repo.Name
        });

    /// <summary>
    /// Dependency update frequency
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public async Task<InsightResult> DependencyUpdatesFrequencyAsync(Repository repo)
    {
        var prs = await _client.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name);
        var count = prs.Count(pr =>
            (pr.Title?.Contains("dependenc", StringComparison.CurrentCultureIgnoreCase) ?? false) ||
            (pr.Body?.Contains("dependenc",  StringComparison.CurrentCultureIgnoreCase) ?? false)
        );
        return new InsightResult
        {
            InsightType = InsightType.DependencyUpdatesFrequency, Value = count, Source = "GitHub", Repo = repo.Name
        };
    }

    /// <summary>
    /// Features the usage feedback loop using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public Task<InsightResult> FeatureUsageFeedbackLoopAsync(Repository repo) =>
        Task.FromResult(new InsightResult
        {
            InsightType = InsightType.FeatureUsageFeedbackLoops, Value = "Needs customer feedback mapping",
            Source = "GitHub", Repo = repo.Name
        });

    /// <summary>
    /// Teams the collaboration efficiency using the specified owner
    /// </summary>
    /// <param name="repo">The repo</param>
    /// <returns>A task containing the insight result</returns>
    public Task<InsightResult> TeamCollaborationEfficiencyAsync(Repository repo) =>
        Task.FromResult(new InsightResult
        {
            InsightType = InsightType.TeamCollaborationEfficiency, Value = "Calculate comments/discussion speed",
            Source = "GitHub", Repo = repo.Name
        });
}