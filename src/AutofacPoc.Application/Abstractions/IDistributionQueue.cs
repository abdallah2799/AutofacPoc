using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Waiting list of scheduled distributions. The API adds to it; a background worker takes from it.
/// </summary>
public interface IDistributionQueue
{
    /// <summary>Adds a distribution to the end of the waiting list.</summary>
    /// <param name="job">The distribution to do later.</param>
    void Enqueue(ScheduledDistribution job);

    /// <summary>Takes the next distribution, or returns <c>null</c> when the list is empty.</summary>
    ScheduledDistribution? TryDequeue();
}
