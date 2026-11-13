
namespace PaymentGateway.Api.Enums;

/// <summary>
/// Represents the processing state of a payment request
/// associated with an idempotency key.
/// </summary>
public enum IdempotencyState
{
    /// <summary>
    /// The payment request has been accepted and is currently being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// The payment has reached a definitive outcome, either authorised or declined.
    /// </summary>
    Completed,

    /// <summary>
    /// The payment outcome is uncertain, for example, due to a bank communication failure.
    /// </summary>
    Unknown
}
