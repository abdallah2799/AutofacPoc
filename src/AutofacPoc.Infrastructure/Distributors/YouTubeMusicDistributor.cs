using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Infrastructure.Distributors;

/// <summary>
/// Fake YouTube Music integration: pretends the delivery worked.
/// </summary>
/// <remarks>
/// One of several <see cref="IDspDistributor"/> implementations, one per DSP.
/// It only does the DSP-specific delivery; recording and notifying are shared in <see cref="DspDistributorBase"/>.
/// The container finds it by scanning this project, and registers it under the key
/// given in <see cref="DistributesToAttribute"/>. No registration code mentions this class.
/// Added in Phase 4 to prove it: this file and one enum value were the only changes needed.
/// </remarks>
[DistributesTo(Dsp.YouTubeMusic)]
public class YouTubeMusicDistributor : DspDistributorBase
{
    /// <inheritdoc />
    protected override DistributionResult DeliverToDsp(Release release)
    {
        return new DistributionResult("YouTube Music", release.Title, "Delivered to YouTube Music");
    }
}
