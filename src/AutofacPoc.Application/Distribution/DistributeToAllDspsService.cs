using AutofacPoc.Application.Abstractions;

namespace AutofacPoc.Application.Distribution;

/// <summary>
/// Use case: "distribute release X to EVERY DSP we support".
/// </summary>
/// <remarks>
/// <para>
/// It asks for <c>IEnumerable&lt;IDspDistributor&gt;</c>: when a constructor asks for
/// a list of an interface, the container passes <b>every</b> registered implementation.
/// </para>
/// <para>
/// So this class never names Spotify, Anghami or Apple Music. Register a new
/// distributor and it is included here automatically.
/// </para>
/// </remarks>
public class DistributeToAllDspsService
{
    private readonly IReleaseRepository _releaseRepository;
    private readonly IEnumerable<IDspDistributor> _distributors;

    /// <summary>Called by the DI container, which supplies the repository and all distributors.</summary>
    /// <param name="releaseRepository">Where to find releases.</param>
    /// <param name="distributors">Every registered distributor, one per DSP.</param>
    public DistributeToAllDspsService(IReleaseRepository releaseRepository, IEnumerable<IDspDistributor> distributors)
    {
        _releaseRepository = releaseRepository;
        _distributors = distributors;
    }

    /// <summary>Finds the release and sends it to every DSP.</summary>
    /// <param name="releaseId">Unique number of the release.</param>
    /// <returns>One result per DSP, or <c>null</c> if the release does not exist.</returns>
    public List<DistributionResult>? DistributeToAll(int releaseId)
    {
        var release = _releaseRepository.GetById(releaseId);
        if (release == null)
            return null;

        var results = new List<DistributionResult>();
        foreach (var distributor in _distributors)
        {
            results.Add(distributor.Distribute(release));
        }
        return results;
    }
}
