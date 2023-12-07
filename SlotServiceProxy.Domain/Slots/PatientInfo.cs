
using SlotServiceProxy.Domain.Shared.ValueObjects;

namespace SlotServiceProxy.Domain.Slots;

/// <summary>
/// Simple strong-typed model of basic data about patient and contact information.
/// </summary>
public record PatientInfo(NotEmptyString Name, NotEmptyString SecondName, Email Email, PhoneNumber Phone);