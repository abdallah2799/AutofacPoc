using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Security;
using AutofacPoc.Domain.Distribution;

namespace AutofacPoc.Application.Distribution;

/// <summary>
/// Use case: "distribute release X to DSP Y soon, without making the caller wait".
/// </summary>
/// <remarks>
/// It only checks the request and puts it in the <see cref="IDistributionQueue"/>.
/// A background worker does the actual distribution later (Phase 8).
/// The permission is checked HERE, while we still know who the caller is.
/// </remarks>
public class ScheduleDistributionService
{
    private readonly IReleaseRepository _releaseRepository;
    private readonly IDistributionQueue _queue;
    private readonly ICurrentUser _currentUser;

    /// <summary>Called by the DI container.</summary>
    /// <param name="releaseRepository">Where to find releases.</param>
    /// <param name="queue">The waiting list the background worker reads.</param>
    /// <param name="currentUser">Who is making the request.</param>
    public ScheduleDistributionService(IReleaseRepository releaseRepository, IDistributionQueue queue, ICurrentUser currentUser)
    {
        _releaseRepository = releaseRepository;
        _queue = queue;
        _currentUser = currentUser;
    }

    /// <summary>Checks the request and queues it.</summary>
    /// <param name="releaseId">Unique number of the release.</param>
    /// <param name="dsp">Which DSP to deliver to.</param>
    /// <returns><c>false</c> if the release does not exist; <c>true</c> once it is queued.</returns>
    /// <exception cref="PermissionDeniedException">The caller lacks the "distribute" permission.</exception>
    public bool Schedule(int releaseId, Dsp dsp)
    {
        if (!_currentUser.HasPermission(Permissions.Distribute))
            throw new PermissionDeniedException(Permissions.Distribute);

        if (_releaseRepository.GetById(releaseId) == null)
            return false;

        _queue.Enqueue(new ScheduledDistribution(releaseId, dsp));
        return true;
    }
}
