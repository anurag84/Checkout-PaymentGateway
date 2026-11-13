
namespace PaymentGateway.Api.Models;

/// <summary>
/// Represents the outcome of a payment-processing operation,
/// including successful processing and technical or idempotency scenarios.
/// </summary>
public enum PaymentProcessingOutcome
{
    /// <summary>
    /// Payment processing completed with a definitive result,
    /// such as Authorized, Declined or Rejected.
    /// </summary>
    Completed,

    /// <summary>
    /// The gateway could not obtain a definitive response
    /// from the acquiring bank.
    /// </summary>
    BankUnavailable,

    /// <summary>
    /// The idempotency key was reused with different payment details.
    /// </summary>
    IdempotencyConflict,

    /// <summary>
    /// Another request using the same idempotency key is still processing.
    /// </summary>
    IdempotencyInProgress
}
