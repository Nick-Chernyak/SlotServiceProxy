using SlotServiceProxy.Application.Slots.SDK;
using Swashbuckle.AspNetCore.Filters;

namespace SlotServiceProxy.Swagger;

public record ReserveSlotExamples : IExamplesProvider<ReserveSlotDto>
{
    public ReserveSlotDto GetExamples()
        => new()
        {
            FacilityId = "Id_Which_Returned_With_Calendar",
            Slot = new SlotDto
            {
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddHours(1)
            },
            Patient = new PatientDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@email.com",
                PhoneNumber = "+1234567890"
            },
            Comments = "I feel very bad, please help me!"
        };
}

public record ReservedSlotExamples : IExamplesProvider<ReservedSlotDto>
{
    public ReservedSlotDto GetExamples()
        => new("Id_Which_Returned_With_Calendar", new SlotDto
        {
            Start = DateTime.UtcNow,
            End = DateTime.UtcNow.AddHours(1)
        });
}