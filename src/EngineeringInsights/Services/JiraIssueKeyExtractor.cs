using System.Text.RegularExpressions;

namespace EngineeringInsights.Services;

/// <summary>
/// The jira issue key extractor class
/// </summary>
public static class JiraIssueKeyExtractor
{
    /// <summary>
    /// The compiled
    /// </summary>
    private static readonly Regex IssueKeyRegex = new Regex(@"\b[A-Z]+-\d+\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Extracts the issue keys using the specified text
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The keys</returns>
    public static HashSet<string> ExtractIssueKeys(string text)
    {
        var keys = new HashSet<string>();
        if (string.IsNullOrEmpty(text)) return keys;
        foreach (Match match in IssueKeyRegex.Matches(text))
            keys.Add(match.Value);
        return keys;
    }
}