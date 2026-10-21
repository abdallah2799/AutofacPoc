using AutofacPoc.Application.Abstractions;
using AutofacPoc.Domain.Distribution;

namespace AutofacPoc.Application.Distribution;

/// <summary>
/// Use case: "distribute release X to DSP Y", where Y is chosen at runtime.
/// </summary>
/// <remarks>
/// <para>
/// This class uses <b>constructor injection</b>: it lists what it needs as
/// constructor parameters and never creates them with <c>new</c>.
/// </para>
/// <para>
/// It does not know which distributor classes exist. It asks the
/// <see cref="IDspDistributorProvider"/> for the one matching the requested DSP,
/// so adding a DSP never changes this class.
/// </para>
/// </remarks>
public class DistributeReleaseService
{
    private readonly IReleaseRepository _releaseRepository;
    private readonly IDspDistributorProvider _distributorProvider;

    /// <summary>Called by the DI container, which supplies both dependencies.</summary>
    /// <param name="releaseRepository">Where to find releases.</param>
    /// <param name="distributorProvider">Finds the distributor for a DSP.</param>
    public DistributeReleaseService(IReleaseRepository releaseRepository, IDspDistributorProvider distributorProvider)
    {
        _releaseRepository = releaseRepository;
        _distributorProvider = distributorProvider;
    }

    /// <summary>Finds the release and sends it to the requested DSP.</summary>
    /// <param name="releaseId">Unique number of the release.</param>
    /// <param name="dsp">Which DSP to deliver to.</param>
    /// <returns>The result, or <c>null</c> if the release does not exist.</returns>
    public DistributionResult? Distribute(int releaseId, Dsp dsp)
    {
        var release = _releaseRepository.GetById(releaseId);
        if (release == null)
            return null;

        var distributor = _distributorProvider.GetFor(dsp);
        return distributor.Distribute(release);
    }
}
