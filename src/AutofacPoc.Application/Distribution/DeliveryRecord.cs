namespace AutofacPoc.Application.Distribution;

/// <summary>One successful delivery of a release to a DSP, kept in the delivery history.</summary>
public class DeliveryRecord
{
    /// <summary>Unique number of the release that was delivered.</summary>
    public int ReleaseId { get; }

    /// <summary>Title of the release that was delivered.</summary>
    public string ReleaseTitle { get; }

    /// <summary>The DSP it was delivered to, e.g. "Spotify".</summary>
    public string Dsp { get; }

    /// <summary>When it was delivered (UTC).</summary>
    public DateTime DeliveredAtUtc { get; }

    /// <summary>Creates a record.</summary>
    /// <param name="releaseId">Unique number of the release.</param>
    /// <param name="releaseTitle">Title of the release.</param>
    /// <param name="dsp">The DSP it was delivered to.</param>
    /// <param name="deliveredAtUtc">When it was delivered (UTC).</param>
    public DeliveryRecord(int releaseId, string releaseTitle, string dsp, DateTime deliveredAtUtc)
    {
        ReleaseId = releaseId;
        ReleaseTitle = releaseTitle;
        Dsp = dsp;
        DeliveredAtUtc = deliveredAtUtc;
    }
}
