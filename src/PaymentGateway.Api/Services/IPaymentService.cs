
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Results;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Defines the operations supported by the payment service,
/// including payment creation and payment retrieval.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Validates and processes a payment request, optionally using
    /// an idempotency key to prevent duplicate processing.
    /// </summary>
    /// <param name="request">The payment details submitted by the merchant.</param>
    /// <param name="idempotencyKey">
    /// An optional merchant-generated key used to identify payment retries.
    /// </param>
    /// <returns>The result of the payment-processing operation.</returns>
    Task<PaymentProcessingResult> ProcessPaymentAsync(
        PostPaymentRequest request,
        string? idempotencyKey);

    /// <summary>
    /// Retrieves an existing payment using its unique identifier.
    /// </summary>
    /// <param name="id">The unique payment identifier.</param>
    /// <returns>The payment details, or null if the payment does not exist.</returns>
    GetPaymentResponse? GetPayment(Guid id);
}
