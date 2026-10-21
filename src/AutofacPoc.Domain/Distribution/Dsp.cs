namespace AutofacPoc.Domain.Distribution;

/// <summary>
/// The digital service providers (DSPs) we can deliver releases to.
/// </summary>
/// <remarks>
/// Each value is also the <b>key</b> the DI container uses to find that DSP's distributor
/// (see "keyed services" in Phase 3 of the README).
/// </remarks>
public enum Dsp
{
    /// <summary>Spotify.</summary>
    Spotify,

    /// <summary>Anghami.</summary>
    Anghami,

    /// <summary>Apple Music.</summary>
    AppleMusic,

    /// <summary>YouTube Music.</summary>
    YouTubeMusic,
}
