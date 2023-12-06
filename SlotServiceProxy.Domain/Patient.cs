
using SlotServiceProxy.Domain.Shared.ValueObjects;

namespace SlotServiceProxy.Domain.Slots;

//Draft
public record Patient(NotEmptyString Name, NotEmptyString SecondName, string? Email, string? Phone);