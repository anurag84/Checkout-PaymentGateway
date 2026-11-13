
using System.Security.Cryptography;
using System.Text;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Generates a consistent cryptographic fingerprint of a payment request.
/// Used to detect changes when an idempotency key is reused.
/// </summary>
public class PaymentRequestHasher
{
    /// <summary>
    /// Generates a SHA-256 hash from the relevant payment details.
    /// </summary>
    /// <param name="request">The payment request to fingerprint.</param>
    /// <returns>A hexadecimal SHA-256 hash of the normalised payment details.</returns>
    public string GenerateHash(PostPaymentRequest request)
    {
        // Normalise the payment details so equivalent requests produce the same hash.
        var normalizedRequest = string.Join("|",
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Currency.ToUpperInvariant(),
            request.Amount);

        // Generate a SHA-256 fingerprint from the normalised request.
        var bytes = Encoding.UTF8.GetBytes(normalizedRequest);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}
