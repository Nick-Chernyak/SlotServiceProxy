namespace SlotServiceProxy.Domain.Shared;

/// <summary>
/// Interface for getting current date and time.
/// Very useful for testing purposes, abstracting DateTime.Now() and DateTime.UtcNow() in-built struct calls.
/// </summary>
public interface IDateTimeService
{
    DateTime Now();
}