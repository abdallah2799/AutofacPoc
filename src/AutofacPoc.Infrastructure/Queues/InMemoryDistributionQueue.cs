using System.Collections.Concurrent;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Infrastructure.Queues;

/// <summary>
/// Fake job queue kept in memory. A real app would use a durable queue (database table, Azure Service Bus, ...).
/// </summary>
/// <remarks>
/// Registered as a singleton: the API and the background worker must share the same queue.
/// <see cref="ConcurrentQueue{T}"/> is a first-in-first-out list that is safe to use from several threads.
/// </remarks>
public class InMemoryDistributionQueue : IDistributionQueue
{
    private readonly ConcurrentQueue<ScheduledDistribution> _jobs = new();

    /// <inheritdoc />
    public void Enqueue(ScheduledDistribution job)
    {
        _jobs.Enqueue(job);
    }

    /// <inheritdoc />
    public ScheduledDistribution? TryDequeue()
    {
        return _jobs.TryDequeue(out var job) ? job : null;
    }
}
