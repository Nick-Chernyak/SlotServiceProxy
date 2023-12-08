using SlotServiceProxy.Application.Slots.SDK;
using Swashbuckle.AspNetCore.Filters;

namespace SlotServiceProxy.Swagger;

public class CalendarDtoExamples : IExamplesProvider<CalendarDto>
{
    public CalendarDto GetExamples()
        => new()
        {
            Id = "Id_Which_Returned_With_Calendar",
            CurrentWeek = new CurrentWeekDto()
            {
                From = DateTime.Now,
                Days = new[]
                {
                    new DayDto
                    {
                        Date = DateTime.Now,
                        FreeSlots = new[]
                        {
                            new SlotDto
                            {
                                Start = DateTime.Now,
                                End = DateTime.Now.AddHours(1)
                            },
                            new SlotDto
                            {
                                Start = DateTime.Now.AddHours(1),
                                End = DateTime.Now.AddHours(2)
                            }
                        }
                    },
                    new DayDto
                    {
                        Date = DateTime.Now.AddDays(1),
                        FreeSlots = new[]
                        {
                            new SlotDto
                            {
                                Start = DateTime.Now.AddDays(1),
                                End = DateTime.Now.AddDays(1).AddHours(1)
                            },
                        }
                    }
                }
            }
        };
}