using EngineeringInsights.Models;

namespace EngineeringInsights.Services;

/// <summary>
/// The git hub portfolio insights class
/// </summary>
public class GitHubPortfolioInsights
{
    /// <summary>
    /// Gets or inits the value of the all repo results
    /// </summary>
    public List<RepoInsightsSummary> AllRepoResults { get; init; } = [];

    /// <summary>
    /// Gets the value of the averages
    /// </summary>
    public Dictionary<InsightType, double?> Averages
    {
        get
        {
            var allResults = AllRepoResults.SelectMany(r => r.InsightResults).ToList();
            var grouped = allResults
                .Where(r => r.Value is double or int)
                .GroupBy(r => r.InsightType);

            var averages = new Dictionary<InsightType, double?>();
            foreach (var g in grouped)
            {
                var nums = g.Select(r => Convert.ToDouble(r.Value)).ToList();
                averages[g.Key] = nums.Count > 0 ? nums.Average() : null;
            }

            return averages;
        }
    }

    /// <summary>
    /// Gets the value of the totals
    /// </summary>
    public Dictionary<InsightType, double?> Totals
    {
        get
        {
            var allResults = AllRepoResults.SelectMany(r => r.InsightResults).ToList();
            var grouped = allResults
                .Where(r => r.Value is double or int)
                .GroupBy(r => r.InsightType);

            var totals = new Dictionary<InsightType, double?>();
            foreach (var g in grouped)
            {
                var nums = g.Select(r => Convert.ToDouble(r.Value)).ToList();
                totals[g.Key] = nums.Count > 0 ? nums.Sum() : null;
            }

            return totals;
        }
    }
}