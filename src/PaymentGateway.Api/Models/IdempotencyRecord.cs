
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Results;

namespace PaymentGateway.Api.Models;

/// <summary>
/// Represents an idempotency record used to track a payment request
/// and prevent duplicate processing when merchants retry requests.
/// </summary>
public class IdempotencyRecord
{
    /// <summary>
    /// Gets the unique key supplied by the merchant
    /// to identify a payment operation.
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Gets the fingerprint of the original payment request.
    /// Used to detect when the same key is reused with different details.
    /// </summary>
    public string RequestHash { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current processing state of the payment request.
    /// </summary>
    public IdempotencyState State { get; init; }

    /// <summary>
    /// Gets the original processing result, when available.
    /// Allows the gateway to return the previous result for a retry.
    /// </summary>
    public PaymentProcessingResult? Result { get; init; }
}
