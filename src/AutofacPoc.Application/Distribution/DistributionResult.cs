namespace AutofacPoc.Application.Distribution;

/// <summary>The outcome of sending a release to a DSP.</summary>
public class DistributionResult
{
    /// <summary>The DSP the release was sent to, e.g. "Spotify".</summary>
    public string Dsp { get; }

    /// <summary>Title of the release that was sent.</summary>
    public string ReleaseTitle { get; }

    /// <summary>Human-readable outcome, e.g. "Delivered to Spotify".</summary>
    public string Message { get; }

    /// <summary>Creates a result.</summary>
    /// <param name="dsp">The DSP the release was sent to.</param>
    /// <param name="releaseTitle">Title of the release.</param>
    /// <param name="message">Human-readable outcome.</param>
    public DistributionResult(string dsp, string releaseTitle, string message)
    {
        Dsp = dsp;
        ReleaseTitle = releaseTitle;
        Message = message;
    }
}
