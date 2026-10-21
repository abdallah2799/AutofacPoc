using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Application.Abstractions;

/// <summary>Sends a release to ONE digital service provider (Spotify, Anghami, ...).</summary>
public interface IDspDistributor
{
    DistributionResult Distribute(Release release);
}
