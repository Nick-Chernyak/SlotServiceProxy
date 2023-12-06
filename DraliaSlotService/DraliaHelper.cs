namespace DraliaSlotService.SDK;

public static class DraliaHelper
{
    public static DateTime GetMondayDateOfCurrentWeek(DateTime baseDate)
    {
        if (baseDate.DayOfWeek is DayOfWeek.Monday)
            return baseDate.Date;
        
        var daysSinceMonday = ((int)baseDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return baseDate.AddDays(-daysSinceMonday);
    }
}