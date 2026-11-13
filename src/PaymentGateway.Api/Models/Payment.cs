
namespace PaymentGateway.Api.Models;

/// <summary>
/// Represents a payment stored by the payment gateway
/// after receiving a definitive authorisation decision from the bank.
/// </summary>
public class Payment
{
    /// <summary>
    /// Gets or sets the unique identifier assigned to the payment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the final business outcome of the payment:
    /// Authorized, Declined or Rejected.
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the last four digits of the payment card.
    /// The full card number is not retained.
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
    /// For example, 100 represents £1.00.
    /// </summary>
    public int Amount { get; set; }
}
