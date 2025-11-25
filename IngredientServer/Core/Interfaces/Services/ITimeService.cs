namespace IngredientServer.Core.Interfaces.Services;

/// <summary>
/// Service để quản lý thời gian thống nhất trong toàn bộ ứng dụng
/// Tất cả thời gian đều sử dụng UTC để đảm bảo tính nhất quán
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Lấy thời gian hiện tại theo UTC
    /// </summary>
    DateTime UtcNow { get; }
    
    /// <summary>
    /// Lấy ngày hiện tại theo UTC (chỉ phần date, không có time)
    /// </summary>
    DateTime UtcToday { get; }
    
    /// <summary>
    /// Lấy thời gian hiện tại theo timezone của user (Asia/Ho_Chi_Minh)
    /// </summary>
    DateTime LocalNow { get; }
    
    /// <summary>
    /// Lấy ngày hiện tại theo timezone của user
    /// </summary>
    DateTime LocalToday { get; }
    
    /// <summary>
    /// Convert UTC time sang local time (Asia/Ho_Chi_Minh)
    /// </summary>
    DateTime ToLocalTime(DateTime utcTime);
    
    /// <summary>
    /// Convert local time sang UTC time
    /// </summary>
    DateTime ToUtcTime(DateTime localTime);
    
    /// <summary>
    /// Tính tuổi từ ngày sinh
    /// </summary>
    int CalculateAge(DateTime dateOfBirth);
    
    /// <summary>
    /// Tính số ngày giữa hai ngày
    /// </summary>
    int DaysBetween(DateTime startDate, DateTime endDate);
}





