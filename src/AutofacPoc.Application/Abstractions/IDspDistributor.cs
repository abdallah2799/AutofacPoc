using AutofacPoc.Application.Distribution;
using AutofacPoc.Application.Security;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Sends a release to ONE digital service provider (DSP), e.g. Spotify.
/// </summary>
/// <remarks>
/// This is the key interface of the POC. Each DSP has its own implementation
/// (SpotifyDistributor, AnghamiDistributor, AppleMusicDistributor).
/// The container registers all of them, so the app can:
/// <list type="bullet">
/// <item>pick ONE by its <see cref="Domain.Distribution.Dsp"/> key, through <see cref="IDspDistributorProvider"/>;</item>
/// <item>get ALL of them at once, by asking for <c>IEnumerable&lt;IDspDistributor&gt;</c>.</item>
/// </list>
/// </remarks>
public interface IDspDistributor
{
    /// <summary>Delivers the release to this distributor's DSP.</summary>
    /// <param name="release">The release to deliver.</param>
    /// <returns>What happened, to send back to the API caller.</returns>
    /// <remarks>Only callers with the "distribute" permission may run it (checked by the security interceptor/decorator).</remarks>
    [RequiresPermission(Permissions.Distribute)]
    DistributionResult Distribute(Release release);
}
