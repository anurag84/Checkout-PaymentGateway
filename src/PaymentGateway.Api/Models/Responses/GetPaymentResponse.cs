
namespace PaymentGateway.Api.Models.Responses;

/// <summary>
/// Represents the payment details returned when a merchant
/// retrieves an existing payment by its identifier.
/// </summary>
public class GetPaymentResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the payment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the final business status of the payment.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the last four digits of the payment card.
    /// The full card number is never exposed in the response.
    /// </summary>
    public string CardNumberLastFour { get; set; }

    /// <summary>
    /// Gets or sets the card expiry month.
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Gets or sets the card expiry year.
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// Gets or sets the three-letter payment currency code.
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Gets or sets the payment amount in minor currency units.
    /// </summary>
    public int Amount { get; set; }
}
