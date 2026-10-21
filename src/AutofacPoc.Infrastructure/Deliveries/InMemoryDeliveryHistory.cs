using System.Collections.Concurrent;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Infrastructure.Deliveries;

/// <summary>
/// Fake delivery history kept in memory. A real app would save it to the database.
/// </summary>
/// <remarks>
/// Registered as a singleton so every request sees the same history.
/// <see cref="ConcurrentQueue{T}"/> is simply a list that is safe when several requests add to it at once.
/// </remarks>
public class InMemoryDeliveryHistory : IDeliveryHistory
{
    private readonly ConcurrentQueue<DeliveryRecord> _records = new();

    /// <inheritdoc />
    public void Record(DeliveryRecord record)
    {
        _records.Enqueue(record);
    }

    /// <inheritdoc />
    public List<DeliveryRecord> GetForRelease(int releaseId)
    {
        return _records.Where(r => r.ReleaseId == releaseId).ToList();
    }
}
