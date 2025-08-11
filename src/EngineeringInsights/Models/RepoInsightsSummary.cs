namespace EngineeringInsights.Models;

/// <summary>
/// The repo insights summary class
/// </summary>
public class RepoInsightsSummary
{
    /// <summary>
    /// Gets or inits the value of the repo
    /// </summary>
    public required string Repo { get; init; }
    
    /// <summary>
    /// Gets or inits the value of the insight results
    /// </summary>
    public List<InsightResult> InsightResults { get; init; } = [];
}