using System.ComponentModel;

namespace EngineeringInsights.Models;

public enum InsightType
{
    [Description("Story points completed per sprint")]
    Velocity,

    [Description("Time from work start to feature delivery")]
    CycleTime,

    [Description("Defects versus completed tasks")]
    DefectRatio,

    [Description("Remaining work over sprint timeline")]
    SprintBurndown,

    [Description("Issues completed in a period")]
    IssueThroughput,

    [Description("Time from issue creation to completion")]
    LeadTime,

    [Description("Time from issue creation to completion on an average")]
    AverageLeadTime,

    [Description("Time from PR creation to merge")]
    PullRequestMergeTime,

    [Description("Time spent in PR review")]
    CodeReviewTime,

    [Description("Lines added/removed per PR")]
    PRSizeLinesChanged,

    [Description("Commits by repo/dev over time")]
    CommitFrequency,

    [Description("Active branches and merges")]
    BranchActivity,

    [Description("Time issues remain in status")]
    IssueAging,

    [Description("Count of reopened issues")]
    ReopenedIssues,

    [Description("Flagged/blocked issues")]
    BlockedIssues,

    [Description("Coverage over time")] 
    TestCoverageTrends,

    [Description("Issues assigned per engineer")]
    TeamWorkloadBalance,

    [Description("Number of bugs/incidents")]
    IncidentFrequency,

    [Description("Frequency of deployments/releases")]
    DeploymentFrequency,

    [Description("Incident resolution duration")]
    TimeToRestoreService,

    [Description("Customer tickets -- time to resolve")]
    CustomerIssueResolutionTimes,

    [Description("Blocked issues due to dependencies")]
    CrossTeamBottlenecks,

    [Description("Balance of meetings and coding")]
    MeetingsVsCodingTime,

    [Description("Created but unassigned issues")]
    UntriagedIssues,

    [Description("Lines added/deleted, revisited")]
    CodeChurnRate,

    [Description("Main contributors to files")]
    CodeOwnershipMetrics,

    [Description("PR review comments cadence")]
    PRReviewComments,

    [Description("Status of CI/CD automation")]
    AutomationCoverage,

    [Description("CI build failures frequency")]
    BuildFailuresFrequency,

    [Description("Ratio of planned vs completed goals")]
    SprintGoalCompletionRatio,

    [Description("Aging of critical/urgent issues")]
    HighPriorityIssueAging,

    [Description("PRs stalled due to unresolved reviews")]
    BlockedPRs,

    [Description("Link customer feedback to GitHub/Jira issues")]
    FeatureUsageFeedbackLoops,

    [Description("Number of reviewers per PR")]
    CodeReviewParticipation,

    [Description("Commits/PRs by time")] DeveloperActivityHeatmap,

    [Description("Progress on epics and stories")]
    EpicProgressTracking,

    [Description("Open/closed security issues")]
    SecurityVulnerabilities,

    [Description("Labeled refactoring changes/issues")]
    RefactoringEfforts,

    [Description("Delivered vs committed in sprint")]
    SprintPredictability,

    [Description("PR time between review request and merge")]
    AvgTimeInCodeReview,

    [Description("Time to productivity for new contributor")]
    ContributorOnboardingTime,

    [Description("Usage of feature toggles via issues/PR meta")]
    FeatureToggleUsage,

    [Description("PRs/issues relating to docs")]
    DocumentationCoverage,

    [Description("Merge conflict indicator on PRs")]
    CodeMergeConflictsFrequency,

    [Description("Failing automation runs")]
    TestAutomationFailures,

    [Description("PRs/issues with release note attached")]
    ReleaseNotesCompleteness,

    [Description("How often dependencies are updated")]
    DependencyUpdatesFrequency,

    [Description("Issues tagged as tech debt")]
    TechnicalDebtIssues,

    [Description("Developer feedback via incidents/surveys")]
    DeveloperSatisfactionProxy,

    [Description("Linked PRs/Jira across repositories")]
    CrossRepoWorkCoordination,

    [Description("Resolution speed, comments frequency")]
    TeamCollaborationEfficiency
}
