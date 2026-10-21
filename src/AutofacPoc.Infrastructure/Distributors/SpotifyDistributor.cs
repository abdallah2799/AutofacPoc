using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Infrastructure.Distributors;

/// <summary>Fake Spotify integration: pretends the upload worked.</summary>
public class SpotifyDistributor : IDspDistributor
{
    public DistributionResult Distribute(Release release)
    {
        return new DistributionResult("Spotify", release.Title, "Delivered to Spotify");
    }
}
