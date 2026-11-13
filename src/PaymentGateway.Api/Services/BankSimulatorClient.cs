
using System.Text.Json;

using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Implements the bank client responsible for sending payment
/// authorisation requests to the bank simulator over HTTP.
/// </summary>
public class BankSimulatorClient : IBankClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initialises the bank client with an HttpClient instance.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to communicate with the bank.</param>
    public BankSimulatorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Sends a payment authorisation request to the bank simulator
    /// and translates the HTTP response into a BankAuthorizationResult.
    /// </summary>
    /// <param name="request">The payment details required by the bank.</param>
    /// <returns>The bank authorisation result, including any communication errors.</returns>
    public async Task<BankAuthorizationResult> AuthorizePaymentAsync(
        BankPaymentRequest request)
    {
        try
        {
            // Submit the payment request to the bank simulator.
            using var response = await _httpClient.PostAsJsonAsync(
                "/payments",
                request);

            var result = new BankAuthorizationResult
            {
                StatusCode = response.StatusCode
            };

            // Deserialize the bank's response when the HTTP request succeeds.
            if (response.IsSuccessStatusCode)
            {
                result.Response = await response.Content
                    .ReadFromJsonAsync<BankPaymentResponse>();
            }
            else
            {
                // Capture the response body for diagnostic purposes.
                result.ErrorMessage = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
        catch (HttpRequestException)
        {
            // The bank could not be reached or the HTTP request failed.
            return new BankAuthorizationResult
            {
                ErrorMessage = "Unable to communicate with the bank."
            };
        }
        catch (TaskCanceledException)
        {
            // The request may have exceeded the configured HTTP timeout.
            return new BankAuthorizationResult
            {
                ErrorMessage = "The bank request timed out."
            };
        }
        catch (JsonException)
        {
            // The bank returned a response that could not be deserialized.
            return new BankAuthorizationResult
            {
                ErrorMessage = "The bank returned an invalid response."
            };
        }
    }
}
