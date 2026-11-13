
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Bank;

/// <summary>
/// Represents the response received from the acquiring bank
/// following a payment authorisation request.
/// </summary>
public class BankPaymentResponse
{
    /// <summary>
    /// Gets or sets whether the bank authorised the payment.
    /// False indicates a valid payment decline, not a technical failure.
    /// </summary>
    [JsonPropertyName("authorized")]
    public bool Authorized { get; set; }

    /// <summary>
    /// Gets or sets the authorisation code returned by the bank.
    /// This may be null when the bank does not provide a code.
    /// </summary>
    [JsonPropertyName("authorization_code")]
    public string? AuthorizationCode { get; set; }
}
