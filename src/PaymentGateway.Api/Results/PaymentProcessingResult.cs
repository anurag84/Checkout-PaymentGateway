
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Results;

/// <summary>
/// Represents the result of a payment-processing operation.
/// Provides a consistent response structure for the controller
/// to determine the appropriate HTTP response.
/// </summary>
public class PaymentProcessingResult
{
    /// <summary>
    /// Gets or sets the overall outcome of the processing operation.
    /// </summary>
    public PaymentProcessingOutcome Outcome { get; set; }

    /// <summary>
    /// Gets or sets the payment response when a payment result is available.
    /// </summary>
    public PostPaymentResponse? PaymentResponse { get; set; }

    /// <summary>
    /// Gets or sets any errors encountered during payment processing.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
