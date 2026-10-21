using AutofacPoc.Application.Abstractions;

namespace AutofacPoc.Application.Distribution;

/// <summary>Use case: "show me where release X has been delivered".</summary>
public class GetDeliveriesService
{
    private readonly IDeliveryHistory _deliveryHistory;

    /// <summary>Called by the DI container.</summary>
    /// <param name="deliveryHistory">Where deliveries are recorded.</param>
    public GetDeliveriesService(IDeliveryHistory deliveryHistory)
    {
        _deliveryHistory = deliveryHistory;
    }

    /// <summary>Returns every recorded delivery of the release (empty if none yet).</summary>
    /// <param name="releaseId">Unique number of the release.</param>
    public List<DeliveryRecord> GetDeliveries(int releaseId)
    {
        return _deliveryHistory.GetForRelease(releaseId);
    }
}
