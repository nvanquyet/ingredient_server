﻿namespace IngredientServer.Utils.Extension;

public static class DateTimeFormat
{
    public static string ToFormattedString(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    public static DateTime FromFormattedString(string dateTimeString)
    {
        if (DateTime.TryParse(dateTimeString, out var dateTime))
        {
            return dateTime;
        }
        
        throw new FormatException("Invalid date time format");
    }
    
    //Get Update time is now
    public static DateTime GetUpdateTimeNow()
    {
        return DateTime.UtcNow;
    }
}