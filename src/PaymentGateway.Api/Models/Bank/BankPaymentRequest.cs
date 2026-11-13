
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Bank;

/// <summary>
/// Represents the payment authorisation request sent to the acquiring bank.
/// Property names are mapped to the bank simulator's expected JSON format.
/// </summary>
public class BankPaymentRequest
{
    /// <summary>
    /// Gets or sets the card number used for payment authorisation.
    /// </summary>
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the card expiry date in MM/YYYY format.
    /// </summary>
    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the three-letter payment currency code.
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment amount in minor currency units.
    /// For example, 100 represents £1.00.
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    /// <summary>
    /// Gets or sets the card security code required for authorisation.
    /// This value must not be persisted or logged.
    /// </summary>
    [JsonPropertyName("cvv")]
    public string Cvv { get; set; } = string.Empty;
}
