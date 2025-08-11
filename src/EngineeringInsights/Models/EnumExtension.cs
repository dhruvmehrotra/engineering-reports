using System.ComponentModel;
using System.Reflection;

namespace EngineeringInsights.Models;

public static class EnumExtensions
{
    /// <summary>
    /// Gets the DescriptionAttribute value for an enum member, or falls back to the enum name.
    /// </summary>
    public static string Description(this Enum value)
    {
        var type = value.GetType();
        var name = value.ToString();
        var field = type.GetField(name);
        if (field == null)
            return name;

        var attr = field.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? name;
    }
}
