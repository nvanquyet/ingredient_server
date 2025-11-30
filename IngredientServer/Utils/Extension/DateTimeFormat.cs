using IngredientServer.Core.Helpers;

namespace IngredientServer.Utils.Extension;

public static class DateTimeFormat
{
    /// <summary>
    /// Formats DateTime to string (assumes UTC)
    /// </summary>
    public static string ToFormattedString(this DateTime dateTime)
    {
        var normalized = DateTimeHelper.NormalizeToUtc(dateTime);
        return normalized.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// Parses string to DateTime and normalizes to UTC
    /// </summary>
    public static DateTime FromFormattedString(string dateTimeString)
    {
        if (DateTime.TryParse(dateTimeString, out var dateTime))
        {
            return DateTimeHelper.NormalizeToUtc(dateTime);
        }
        
        throw new FormatException("Invalid date time format");
    }
    
    /// <summary>
    /// Gets current UTC time (use DateTimeHelper.UtcNow instead)
    /// </summary>
    [Obsolete("Use DateTimeHelper.UtcNow instead")]
    public static DateTime GetUpdateTimeNow()
    {
        return DateTimeHelper.UtcNow;
    }
}