using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Infrastructure.Distributors;

/// <summary>
/// Fake Anghami integration: pretends the delivery worked.
/// </summary>
/// <remarks>
/// One of several <see cref="IDspDistributor"/> implementations, one per DSP.
/// It only does the DSP-specific delivery; recording and notifying are shared in <see cref="DspDistributorBase"/>.
/// The container finds it by scanning this project, and registers it under the key
/// given in <see cref="DistributesToAttribute"/>. No registration code mentions this class.
/// </remarks>
[DistributesTo(Dsp.Anghami)]
public class AnghamiDistributor : DspDistributorBase
{
    /// <inheritdoc />
    protected override DistributionResult DeliverToDsp(Release release)
    {
        return new DistributionResult("Anghami", release.Title, "Delivered to Anghami");
    }
}
