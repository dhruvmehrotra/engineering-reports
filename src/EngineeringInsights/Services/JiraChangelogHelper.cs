using System.Text.Json;

namespace EngineeringInsights.Services;

/// <summary>
/// The jira changelog helper class
/// </summary>
public static class JiraChangelogHelper
{
    /// <summary>
    /// Gets the status transition times using the specified changelog
    /// </summary>
    /// <param name="changelog">The changelog</param>
    /// <param name="statuses">The statuses</param>
    /// <returns>The times</returns>
    private static List<DateTime> GetStatusTransitionTimes(JsonElement changelog, params string[] statuses)
    {
        var times = new List<DateTime>();
        foreach (var history in changelog.GetProperty("histories").EnumerateArray())
        {
            DateTime changeTime = DateTime.Parse(history.GetProperty("created").GetString() ?? string.Empty);
            times.AddRange(from item in history.GetProperty("items").EnumerateArray()
                where item.GetProperty("field").GetString() == "status"
                select item.GetProperty("toString").GetString()
                into toStatus
                where Array.Exists(statuses, s => s.Equals(toStatus, StringComparison.OrdinalIgnoreCase))
                select changeTime);
        }

        return times;
    }

    /// <summary>
    /// Gets the first status transition time using the specified changelog
    /// </summary>
    /// <param name="changelog">The changelog</param>
    /// <param name="status">The status</param>
    /// <returns>The date time</returns>
    public static DateTime? GetFirstStatusTransitionTime(JsonElement changelog, string status)
    {
        var times = GetStatusTransitionTimes(changelog, status);
        if (times.Count == 0) return null;
        times.Sort();
        return times[0];
    }
}