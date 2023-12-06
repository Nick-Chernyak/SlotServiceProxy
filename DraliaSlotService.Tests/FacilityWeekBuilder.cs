using DraliaSlotService.SDK;
using SlotServiceProxy.Domain.Shared;
using SlotServiceProxy.Domain.Shared.ValueObjects;

namespace DraliaSlotService.Tests;

public static class FacilityWeekBuilder
{
    /// <summary>
    /// Since Facility model is not part of calendar build logic, it is not necessary to build it specifically,
    /// -> we can use just set dummy values.
    /// </summary>
    private static Facility DefaultFacility
        => new("Facility_Id", "Facility_Name", "Facility_Address");
    
    public static readonly int DefaultSlotDurationInMin = 30;
    
    public static readonly FacilityWeekResponse EmptyWeek = new(
        DefaultFacility,
        DefaultSlotDurationInMin,
        null,
        null, 
        null, 
        null, 
        null, 
        null, 
        null);

    public static FacilityWeekResponse BuildFullWeekWithoutBusySlots(WorkPeriod workPeriod)
        => EmptyWeek
            .WithMonday(BuildEmptyDay(workPeriod))
            .WithTuesday(BuildEmptyDay(workPeriod))
            .WithWednesday(BuildEmptyDay(workPeriod))
            .WithThursday(BuildEmptyDay(workPeriod))
            .WithFriday(BuildEmptyDay(workPeriod))
            .WithSaturday(BuildEmptyDay(workPeriod))
            .WithSunday(BuildEmptyDay(workPeriod));
    
    public static FacilityDay BuildEmptyDay(WorkPeriod workPeriod) 
        => new(workPeriod, Array.Empty<DailyTimeRange>());
    
    public static FacilityWeekResponse WithMonday(this FacilityWeekResponse facilityWeekResponse,
        FacilityDay monday) => facilityWeekResponse with {Monday = monday};
    
    public static FacilityWeekResponse WithTuesday(this FacilityWeekResponse facilityWeekResponse,
        FacilityDay tuesday) => facilityWeekResponse with {Tuesday = tuesday};
    
    public static FacilityWeekResponse WithWednesday(this FacilityWeekResponse facilityWeekResponse,
        FacilityDay wednesday) => facilityWeekResponse with {Wednesday = wednesday};
    
    public static FacilityWeekResponse WithThursday(this FacilityWeekResponse facilityWeekResponse,
        FacilityDay thursday) => facilityWeekResponse with {Thursday = thursday};
    
    public static FacilityWeekResponse WithFriday(this FacilityWeekResponse facilityWeekResponse,
        FacilityDay friday) => facilityWeekResponse with {Friday = friday};
    
    public static FacilityWeekResponse WithSaturday(this FacilityWeekResponse facilityWeekResponse,
        FacilityDay saturday) => facilityWeekResponse with {Saturday = saturday};
    
    public static FacilityWeekResponse WithSunday(this FacilityWeekResponse facilityWeekResponse,
        FacilityDay sunday) => facilityWeekResponse with {Sunday = sunday};

    public static FacilityWeekResponse WithoutDay(this FacilityWeekResponse facilityWeek, DayOfWeek dayOfWeek)
    {
        //Quick and dirty way to remove day from facility week.
        //This or reflection, but since we don't expect in near future to have more than 7 days in week,
        //this is fine.
        facilityWeek = dayOfWeek switch
        {
            DayOfWeek.Sunday => facilityWeek with {Sunday = null},
            DayOfWeek.Monday => facilityWeek with {Monday = null},
            DayOfWeek.Tuesday => facilityWeek with {Tuesday = null},
            DayOfWeek.Wednesday => facilityWeek with {Wednesday = null},
            DayOfWeek.Thursday => facilityWeek with {Thursday = null},
            DayOfWeek.Friday => facilityWeek with {Friday = null},
            DayOfWeek.Saturday => facilityWeek with {Saturday = null},
            _ => facilityWeek
        };

        return facilityWeek;
    }

}