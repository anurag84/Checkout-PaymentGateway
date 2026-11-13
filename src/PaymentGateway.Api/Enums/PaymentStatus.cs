
namespace PaymentGateway.Api.Models;

/// <summary>
/// Represents the final business status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The acquiring bank approved the payment.
    /// </summary>
    Authorized,

    /// <summary>
    /// The acquiring bank declined the payment.
    /// </summary>
    Declined,

    /// <summary>
    /// The gateway rejected the payment due to invalid request details
    /// before sending it to the acquiring bank.
    /// </summary>
    Rejected
}
