namespace IngredientServer.Core.Helpers;

/// <summary>
/// Centralized DateTime helper to ensure all DateTime operations use UTC consistently
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Gets current UTC DateTime
    /// </summary>
    public static DateTime UtcNow => DateTime.UtcNow;

    /// <summary>
    /// Gets current UTC DateTime as Date only (midnight UTC)
    /// </summary>
    public static DateTime UtcToday => DateTime.UtcNow.Date;

    /// <summary>
    /// Normalizes a DateTime to UTC. If Kind is Unspecified, assumes it's already UTC.
    /// </summary>
    public static DateTime NormalizeToUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;

        if (dateTime.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        return dateTime.ToUniversalTime();
    }

    /// <summary>
    /// Normalizes a nullable DateTime to UTC
    /// </summary>
    public static DateTime? NormalizeToUtc(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return null;

        return NormalizeToUtc(dateTime.Value);
    }

    /// <summary>
    /// Gets a DateTime representing the start of the day in UTC
    /// </summary>
    public static DateTime StartOfDayUtc(DateTime dateTime)
    {
        var normalized = NormalizeToUtc(dateTime);
        return normalized.Date;
    }

    /// <summary>
    /// Gets a DateTime representing the end of the day in UTC
    /// </summary>
    public static DateTime EndOfDayUtc(DateTime dateTime)
    {
        var normalized = NormalizeToUtc(dateTime);
        return normalized.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Calculates age from DateOfBirth in UTC
    /// </summary>
    public static int? CalculateAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue)
            return null;

        var birthDate = NormalizeToUtc(dateOfBirth.Value);
        var today = UtcToday;
        var age = today.Year - birthDate.Year;
        
        if (today.DayOfYear < birthDate.DayOfYear)
            age--;

        return age;
    }

    /// <summary>
    /// Checks if a date is expired (past today in UTC)
    /// </summary>
    public static bool IsExpired(DateTime expiryDate)
    {
        var normalized = NormalizeToUtc(expiryDate);
        return UtcToday > normalized.Date;
    }

    /// <summary>
    /// Calculates days until expiry
    /// </summary>
    public static int DaysUntilExpiry(DateTime expiryDate)
    {
        var normalized = NormalizeToUtc(expiryDate);
        return (normalized.Date - UtcToday).Days;
    }

    /// <summary>
    /// Checks if a date is expiring soon (within 7 days)
    /// </summary>
    public static bool IsExpiringSoon(DateTime expiryDate)
    {
        var days = DaysUntilExpiry(expiryDate);
        return days is >= 0 and <= 7;
    }
}

