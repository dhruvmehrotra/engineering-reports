namespace EngineeringInsights.Models;

/// <summary>
/// The insight result class
/// </summary>
public class InsightResult
{
    /// <summary>
    /// Gets or sets the value of the insight type
    /// </summary>
    public InsightType InsightType { get; init; }

    /// <summary>
    /// Gets the value of the insight type description
    /// </summary>
    public string InsightTypeDescription => InsightType.Description();

    /// <summary>
    /// Gets or inits the value of the value
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// Gets or sets the value of the source
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets or sets the value of the repo
    /// </summary>
    public string? Repo { get; set; } // Optional: Set repo name for all results

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
        return string.Concat("InsightType: ", InsightType, " InsightTypeDescription: ", InsightTypeDescription,
            " Source: ", Source, " Repo: ", Repo);
    }
}