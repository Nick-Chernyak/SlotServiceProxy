using DraliaSlotService.SDK;
using Flurl.Http;
using Flurl.Http.Configuration;
using SlotServiceProxy.Domain;
using SlotServiceProxy.Shared;

namespace DraliaSlotService;

public class DraliaWrapper : IDisposable
{
    private const string BaseUrl = "https://draliatest.azurewebsites.net/api";
    private const string AvailabilitySegment = "Availability";
    private const string GetWeeklySegment = "GetWeeklyAvailability";
    private const string ReserveSlotSegment = "TakeSlot";

    private readonly IFlurlClient _client;

    public DraliaWrapper(IFlurlClientFactory flurlClientFactory)
        => _client = flurlClientFactory
            .Get(BaseUrl)
            .Do(client => client.BaseUrl = BaseUrl)
            .Configure(ConfigureClient);

    public Task<Result<FacilityWeekResponse, Problem>> GetAvailableSlotsPerWeek(DateTime searchDate)
        => _client.Request(AvailabilitySegment, GetWeeklySegment)
            .AppendPathSegment($"{searchDate.GetMondayDateOfCurrentWeek():yyyyMMdd}")
            .GetAsync()
            .To(ConvertResponseTo<FacilityWeekResponse>);

    public Task<VerificationResult<Problem>> TryToReserveSlot(ReserveSlotRequest reserveSlotRequest)
        => _client
            .Request(AvailabilitySegment, ReserveSlotSegment)
            .PostJsonAsync(reserveSlotRequest)
            .To(ConvertResponseTo);

    private static async Task<Result<T, Problem>> ConvertResponseTo<T>(Task<IFlurlResponse> response) where T : IDraliaResponse
    {
        try
        {
            var flurlResponse = await response;
            var responseObject = await flurlResponse.GetJsonAsync<T>();
            return responseObject;
        }
        catch (FlurlHttpTimeoutException)
        {
            return new Problem("Timetable owner not respond. Try again later.", ProblemType.ExternalServiceError);
        }
        catch (FlurlParsingException)
        {
            throw;
        }
        catch (FlurlHttpException ex)
        {
            //Can be use for logging at least.
            var errorMessage = await ex.GetResponseJsonAsync<string>();
            return new Problem($"External service error: {errorMessage}", ProblemType.ExternalServiceError);
        }
    }
    
    // Overload plus a bit copypaste for the edge case (not really realistic) - void responses.
    private static async Task<VerificationResult<Problem>> ConvertResponseTo(Task<IFlurlResponse> response)
    {
        try
        {
            await response;
            return VerificationResult<Problem>.Ok();
        }
        catch (FlurlHttpTimeoutException)
        {
            return VerificationResult<Problem>.Failure(new Problem("Timeout", ProblemType.ExternalServiceError));
        }
        catch (FlurlParsingException)
        {
            throw;
        }
        catch (FlurlHttpException ex)
        {
            var errorMessage = await ex.GetResponseJsonAsync<string>();
            return VerificationResult<Problem>.Failure(new Problem(errorMessage, ProblemType.ExternalServiceError));
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private static void ConfigureClient(ClientFlurlHttpSettings httpSettings)
    {
        var credentials = DummyConfigurator.DraliaCredentials;
        httpSettings.BeforeCall += call => call.Request
            .WithBasicAuth(credentials["Username"], credentials["Password"]);
    }
}