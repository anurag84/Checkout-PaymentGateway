
namespace PaymentGateway.Api.Models.Responses;

/// <summary>
/// Represents the response returned after a payment creation request.
/// </summary>
public class PostPaymentResponse
{
    /// <summary>
    /// Gets or sets the unique identifier assigned to the payment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the resulting payment status,
    /// such as Authorized, Declined or Rejected.
    /// </summary>
    public PaymentStatus Status { get; set; }
}
