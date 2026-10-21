namespace AutofacPoc.Application.Distribution;

/// <summary>What the API returns after a distribution attempt.</summary>
public class DistributionResult
{
    public string Dsp { get; }
    public string ReleaseTitle { get; }
    public string Message { get; }

    public DistributionResult(string dsp, string releaseTitle, string message)
    {
        Dsp = dsp;
        ReleaseTitle = releaseTitle;
        Message = message;
    }
}
