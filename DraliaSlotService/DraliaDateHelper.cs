namespace DraliaSlotService;

public static class DraliaDateHelper
{
    public static DateTime GetMondayDateOfCurrentWeek(this DateTime baseDate)
    {
        if (baseDate.DayOfWeek is DayOfWeek.Monday)
            return baseDate.Date;
        
        var daysSinceMonday = ((int)baseDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return baseDate.AddDays(-daysSinceMonday);
    }
    
    
    /// <summary>
    /// Get exact Date of the provided day of week in scope of current natural week based on <paramref name="baseDate"/>.
    /// </summary>
    public static DateTime GetDateTimeBaseOnDayOfWeek(this DateTime baseDate, DayOfWeek dayOfWeek)
    {
        var daysSinceMonday = ((int)dayOfWeek - (int)baseDate.DayOfWeek + 7) % 7;
        return baseDate.AddDays(daysSinceMonday);
    }
}