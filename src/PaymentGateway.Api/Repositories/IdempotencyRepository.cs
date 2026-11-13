
using System.Collections.Concurrent;

using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Repositories;

/// <summary>
/// Provides in-memory storage for idempotency records.
/// Uses ConcurrentDictionary to support thread-safe access
/// from multiple incoming payment requests.
/// </summary>
public class IdempotencyRepository
{
    private readonly ConcurrentDictionary<string, IdempotencyRecord> _records =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to reserve a new idempotency key.
    /// Returns false if the key already exists.
    /// </summary>
    /// <param name="record">The idempotency record to store.</param>
    /// <returns>True if the record was added; otherwise, false.</returns>
    public bool TryAdd(IdempotencyRecord record)
    {
        return _records.TryAdd(record.Key, record);
    }

    /// <summary>
    /// Retrieves an existing idempotency record using its key.
    /// </summary>
    /// <param name="key">The merchant-provided idempotency key.</param>
    /// <returns>The matching record, or null if it does not exist.</returns>
    public IdempotencyRecord? Get(string key)
    {
        _records.TryGetValue(key, out var record);
        return record;
    }

    /// <summary>
    /// Attempts to update an existing record only if its current value
    /// matches the expected record. This helps prevent overwriting
    /// a concurrent update.
    /// </summary>
    /// <param name="key">The idempotency key.</param>
    /// <param name="currentRecord">The expected current record.</param>
    /// <param name="updatedRecord">The replacement record.</param>
    /// <returns>True if the update succeeds; otherwise, false.</returns>
    public bool Update(
        string key,
        IdempotencyRecord currentRecord,
        IdempotencyRecord updatedRecord)
    {
        return _records.TryUpdate(key, updatedRecord, currentRecord);
    }
}
