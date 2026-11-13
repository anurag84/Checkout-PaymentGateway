
namespace PaymentGateway.Api.Models.Requests;

/// <summary>
/// Represents the payment details submitted by a merchant
/// when requesting a payment authorisation.
/// </summary>
public class PostPaymentRequest
{
    /// <summary>
    /// Gets or sets the card number provided for the payment.
    /// Must contain between 14 and 19 numeric digits.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the card expiry month, from 1 to 12.
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Gets or sets the card expiry year.
    /// The expiry date must be in the future.
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Gets or sets the three-letter payment currency code,
    /// such as GBP, USD or EUR.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment amount in minor currency units.
    /// For example, 100 represents £1.00.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Gets or sets the card security code supplied for authorisation.
    /// Must not be persisted or logged.
    /// </summary>
    public string Cvv { get; set; } = string.Empty;
}
