using AutofacPoc.Domain.Distribution;

namespace AutofacPoc.Application.Distribution;

/// <summary>A distribution that was requested now and will be done shortly by a background worker.</summary>
public class ScheduledDistribution
{
    /// <summary>Unique number of the release to deliver.</summary>
    public int ReleaseId { get; }

    /// <summary>Which DSP to deliver to.</summary>
    public Dsp Dsp { get; }

    /// <summary>Creates a scheduled distribution.</summary>
    /// <param name="releaseId">Unique number of the release to deliver.</param>
    /// <param name="dsp">Which DSP to deliver to.</param>
    public ScheduledDistribution(int releaseId, Dsp dsp)
    {
        ReleaseId = releaseId;
        Dsp = dsp;
    }
}
