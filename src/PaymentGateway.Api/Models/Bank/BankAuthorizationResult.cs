
using System.Net;

namespace PaymentGateway.Api.Models.Bank;

/// <summary>
/// Represents the result of a payment authorisation request
/// sent to the acquiring bank.
/// </summary>
public class BankAuthorizationResult
{
    /// <summary>
    /// Gets or sets the HTTP status code returned by the bank.
    /// Null when no HTTP response was received.
    /// </summary>
    public HttpStatusCode? StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the deserialised response received from the bank.
    /// Null when the response could not be obtained or parsed.
    /// </summary>
    public BankPaymentResponse? Response { get; set; }

    /// <summary>
    /// Gets or sets the error message when the authorisation request fails.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indicates whether the bank returned HTTP 200
    /// with a valid authorisation response.
    /// A successful response may still represent a declined payment.
    /// </summary>
    public bool IsSuccess =>
        StatusCode == HttpStatusCode.OK && Response != null;
}
