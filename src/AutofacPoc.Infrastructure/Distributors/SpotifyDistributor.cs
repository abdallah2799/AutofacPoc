using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Infrastructure.Distributors;

/// <summary>
/// Fake Spotify integration: pretends the delivery worked.
/// </summary>
/// <remarks>
/// One of several <see cref="IDspDistributor"/> implementations, one per DSP.
/// The container finds it by scanning this project, and registers it under the key
/// given in <see cref="DistributesToAttribute"/>. No registration code mentions this class.
/// </remarks>
[DistributesTo(Dsp.Spotify)]
public class SpotifyDistributor : IDspDistributor
{
    /// <inheritdoc />
    public DistributionResult Distribute(Release release)
    {
        return new DistributionResult("Spotify", release.Title, "Delivered to Spotify");
    }
}
