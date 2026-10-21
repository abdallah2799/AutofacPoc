using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Sends a release to ONE digital service provider (DSP), e.g. Spotify.
/// </summary>
/// <remarks>
/// This is the key interface of the POC. Each DSP gets its own implementation
/// (SpotifyDistributor, AnghamiDistributor, ...). Later phases use Autofac to
/// pick the right one at runtime, wrap it with logging/security, and register
/// new ones automatically.
/// </remarks>
public interface IDspDistributor
{
    /// <summary>Delivers the release to this distributor's DSP.</summary>
    /// <param name="release">The release to deliver.</param>
    /// <returns>What happened, to send back to the API caller.</returns>
    DistributionResult Distribute(Release release);
}
