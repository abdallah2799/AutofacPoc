using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Tells the label that their release is now live on a DSP (e.g. by email).
/// </summary>
/// <remarks>
/// <b>Optional:</b> it is only registered when <c>Notifications:Enabled</c> is true in appsettings.json.
/// When it isn't registered, the distributors' shared base class simply has no notifier and skips this step.
/// That "may be missing" case is the second good use of <b>property injection</b> (Phase 7).
/// </remarks>
public interface IDeliveryNotifier
{
    /// <summary>Notifies the label about a successful delivery.</summary>
    /// <param name="release">The release that was delivered.</param>
    /// <param name="result">The delivery result.</param>
    void NotifyDelivered(Release release, DistributionResult result);
}
