using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Infrastructure.Distributors;

/// <summary>
/// Shared base class of every DSP distributor. It does the business steps common to all DSPs:
/// deliver (the DSP-specific part), record the delivery, and notify the label.
/// </summary>
/// <remarks>
/// <para>
/// <b>Property injection, the real benefit:</b> the shared services below are PROPERTIES that the
/// container sets after creating the distributor. So the DSP classes (Spotify, Anghami, Apple Music,
/// YouTube Music, and every future one) have no constructor at all, and adding another shared service
/// here later changes ZERO DSP classes.
/// </para>
/// <para>
/// With constructor injection instead, every DSP class would need
/// <c>public SpotifyDistributor(IDeliveryHistory history, IDeliveryNotifier? notifier) : base(history, notifier)</c>,
/// and each new shared service would mean editing every DSP class again.
/// </para>
/// <para>
/// Autofac sets the properties automatically (<c>.PropertiesAutowired()</c>). The built-in container
/// can't, so the built-in API sets them by hand in its registration code.
/// </para>
/// </remarks>
public abstract class DspDistributorBase : IDspDistributor
{
    /// <summary>
    /// Where every delivery is recorded. <b>Set by the container</b> (property injection).
    /// </summary>
    /// <remarks>Must be <c>public</c> with a <c>set</c>, otherwise Autofac ignores it.</remarks>
    public IDeliveryHistory DeliveryHistory { get; set; } = null!;

    /// <summary>
    /// Tells the label about the delivery. <b>Optional:</b> stays <c>null</c> when notifications are
    /// switched off in appsettings.json, because then no notifier is registered.
    /// </summary>
    public IDeliveryNotifier? DeliveryNotifier { get; set; }

    /// <inheritdoc />
    public DistributionResult Distribute(Release release)
    {
        // 1. The DSP-specific part, written in each subclass.
        var result = DeliverToDsp(release);

        // 2. Shared business steps, written once for all DSPs.
        DeliveryHistory.Record(new DeliveryRecord(release.Id, release.Title, result.Dsp, DateTime.UtcNow));
        DeliveryNotifier?.NotifyDelivered(release, result); // "?." = skip when there is no notifier

        return result;
    }

    /// <summary>Sends the release to this DSP. Each subclass implements only this.</summary>
    /// <param name="release">The release to deliver.</param>
    /// <returns>What happened.</returns>
    protected abstract DistributionResult DeliverToDsp(Release release);
}
