using AutofacPoc.Application.Abstractions;

namespace AutofacPoc.Application.Distribution;

/// <summary>
/// Use case: "distribute release X".
/// It asks for its dependencies in the constructor and never creates them itself.
/// </summary>
public class DistributeReleaseService
{
    private readonly IReleaseRepository _releaseRepository;
    private readonly IDspDistributor _distributor;

    public DistributeReleaseService(IReleaseRepository releaseRepository, IDspDistributor distributor)
    {
        _releaseRepository = releaseRepository;
        _distributor = distributor;
    }

    /// <summary>Returns null when the release doesn't exist.</summary>
    public DistributionResult? Distribute(int releaseId)
    {
        var release = _releaseRepository.GetById(releaseId);
        if (release == null)
            return null;

        return _distributor.Distribute(release);
    }
}
