using System.ComponentModel.DataAnnotations;

namespace SlotServiceProxy.Application.Slots.SDK;

/// <summary>
/// Represent slim patient basic data model.
/// </summary>
/// This is request -> model never initiated from the code.
// ReSharper disable once ClassNeverInstantiated.Global
public record PatientDto
{
    /// <summary>
    /// Patient first name.
    /// Required.
    /// Max length: 30.
    /// </summary>
    [Required]
    [StringLength(30)]
    public required string FirstName { get; init; }
    /// <summary>
    /// Patient last name.
    /// Required.
    /// Max length: 30.
    /// </summary>
    [Required]
    [StringLength(30)]
    public required string LastName { get; init; }
    
    /// <summary>
    /// Patient email address.
    /// Required.
    /// </summary>
    [Required]
    [RegularExpression(Domain.Shared.ValueObjects.Email.EmailPattern)]
    public required string Email { get; init; }

    /// <summary>
    /// Patient phone number.
    /// Required.
    /// Format: E.164 standard.
    /// </summary>
    [Required]
    [RegularExpression(Domain.Shared.ValueObjects.PhoneNumber.PhoneRegex)]
    public required string PhoneNumber { get; init; }
}