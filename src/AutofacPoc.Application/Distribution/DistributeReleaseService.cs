using AutofacPoc.Application.Abstractions;

namespace AutofacPoc.Application.Distribution;

/// <summary>
/// Use case: "distribute release X".
/// </summary>
/// <remarks>
/// <para>
/// This class shows <b>constructor injection</b>: it lists what it needs as
/// constructor parameters and never creates them with <c>new</c>.
/// The DI container sees the constructor and passes the right objects in.
/// </para>
/// <para>
/// It only knows the interfaces, so it works unchanged with any container
/// and any implementation, and it is easy to unit-test with fakes.
/// </para>
/// </remarks>
public class DistributeReleaseService
{
    private readonly IReleaseRepository _releaseRepository;
    private readonly IDspDistributor _distributor;

    /// <summary>Called by the DI container, which supplies both dependencies.</summary>
    /// <param name="releaseRepository">Where to find releases.</param>
    /// <param name="distributor">Which DSP to send them to.</param>
    public DistributeReleaseService(IReleaseRepository releaseRepository, IDspDistributor distributor)
    {
        _releaseRepository = releaseRepository;
        _distributor = distributor;
    }

    /// <summary>Finds the release and sends it to the DSP.</summary>
    /// <param name="releaseId">Unique number of the release.</param>
    /// <returns>The result, or <c>null</c> if the release does not exist.</returns>
    public DistributionResult? Distribute(int releaseId)
    {
        var release = _releaseRepository.GetById(releaseId);
        if (release == null)
            return null;

        return _distributor.Distribute(release);
    }
}
