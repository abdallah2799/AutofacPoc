using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Keeps a record of every delivery (which release went to which DSP, and when).
/// </summary>
/// <remarks>
/// Needed by EVERY distributor, so it is provided once, to their shared base class
/// <c>DspDistributorBase</c>, through <b>property injection</b> (Phase 7).
/// </remarks>
public interface IDeliveryHistory
{
    /// <summary>Adds a delivery to the history.</summary>
    /// <param name="record">The delivery to record.</param>
    void Record(DeliveryRecord record);

    /// <summary>Returns every recorded delivery of one release, oldest first.</summary>
    /// <param name="releaseId">Unique number of the release.</param>
    List<DeliveryRecord> GetForRelease(int releaseId);
}
