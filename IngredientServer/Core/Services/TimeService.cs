using IngredientServer.Core.Interfaces.Services;
using TimeZoneConverter;

namespace IngredientServer.Core.Services;

/// <summary>
/// Implementation của ITimeService
/// Sử dụng UTC làm chuẩn cho tất cả operations
/// </summary>
public class TimeService : ITimeService
{
    private static readonly TimeZoneInfo VietnamTimeZone = 
        TZConvert.GetTimeZoneInfo("Asia/Ho_Chi_Minh");

    public DateTime UtcNow => DateTime.UtcNow;
    
    public DateTime UtcToday => DateTime.UtcNow.Date;
    
    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone);
    
    public DateTime LocalToday => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone).Date;
    
    public DateTime ToLocalTime(DateTime utcTime)
    {
        if (utcTime.Kind == DateTimeKind.Unspecified)
        {
            // Nếu không có timezone info, giả định là UTC
            utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
        }
        
        if (utcTime.Kind == DateTimeKind.Utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, VietnamTimeZone);
        }
        
        // Nếu đã là local time, return as is
        return utcTime;
    }
    
    public DateTime ToUtcTime(DateTime localTime)
    {
        if (localTime.Kind == DateTimeKind.Unspecified)
        {
            // Nếu không có timezone info, giả định là local time và convert sang UTC
            var local = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(local, VietnamTimeZone);
        }
        
        if (localTime.Kind == DateTimeKind.Local)
        {
            return TimeZoneInfo.ConvertTimeToUtc(localTime, VietnamTimeZone);
        }
        
        // Nếu đã là UTC, return as is
        return localTime;
    }
    
    public int CalculateAge(DateTime dateOfBirth)
    {
        var today = LocalToday;
        var age = today.Year - dateOfBirth.Year;
        
        // Trừ đi 1 năm nếu chưa đến sinh nhật trong năm nay
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }
        
        return age;
    }
    
    public int DaysBetween(DateTime startDate, DateTime endDate)
    {
        // Đảm bảo cả hai đều là date (không có time)
        var start = startDate.Date;
        var end = endDate.Date;
        
        return (end - start).Days;
    }
}





