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
    /// Format:
    /// </summary>
    [Required]
    //TODO: add validation of email address.
    public required string Email { get; init; }

    /// <summary>
    /// Patient phone number.
    /// Required.
    /// Regex format: "^\+(?:[0-9]●?){6,14}[0-9]$".
    /// </summary>
    [Required]
    [RegularExpression("^(\\(\\d{3}\\)|\\d{3})-?\\d{3}-?\\d{4}$")]
    public required string PhoneNumber { get; init; }
}