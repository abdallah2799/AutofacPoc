using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Infrastructure.Distributors;

/// <summary>
/// Fake Spotify integration: pretends the delivery worked.
/// </summary>
/// <remarks>
/// For now it is the only <see cref="IDspDistributor"/>. Phase 3 adds more DSPs
/// and shows how Autofac picks the right one per request.
/// </remarks>
public class SpotifyDistributor : IDspDistributor
{
    /// <inheritdoc />
    public DistributionResult Distribute(Release release)
    {
        return new DistributionResult("Spotify", release.Title, "Delivered to Spotify");
    }
}
