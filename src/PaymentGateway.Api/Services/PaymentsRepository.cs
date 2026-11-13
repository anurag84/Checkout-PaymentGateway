
using System.Collections.Concurrent;

using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Provides in-memory storage and retrieval of payment records.
/// Uses ConcurrentDictionary to support thread-safe access
/// from multiple payment requests.
/// </summary>
public class PaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    /// <summary>
    /// Attempts to add a payment using its unique identifier.
    /// </summary>
    /// <param name="payment">The payment record to store.</param>
    /// <returns>
    /// True if the payment was added successfully;
    /// otherwise, false if the identifier already exists.
    /// </returns>
    public bool Add(Payment payment)
    {
        return _payments.TryAdd(payment.Id, payment);
    }

    /// <summary>
    /// Retrieves a payment using its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>
    /// The matching payment, or null if no payment exists with that identifier.
    /// </returns>
    public Payment? Get(Guid id)
    {
        _payments.TryGetValue(id, out var payment);
        return payment;
    }
}
