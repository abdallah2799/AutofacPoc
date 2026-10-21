using AutofacPoc.Domain.Distribution;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Marks an <see cref="IDspDistributor"/> class with the DSP it delivers to,
/// e.g. <c>[DistributesTo(Dsp.Spotify)]</c>.
/// </summary>
/// <remarks>
/// <para>
/// When the container <b>scans</b> the Infrastructure project for distributors (Phase 4),
/// it reads this attribute to know which key to register each class under.
/// </para>
/// <para>
/// It is plain metadata on the class: it does not depend on Autofac or any other container.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class DistributesToAttribute : Attribute
{
    /// <summary>The DSP this distributor delivers to.</summary>
    public Dsp Dsp { get; }

    /// <summary>Creates the attribute.</summary>
    /// <param name="dsp">The DSP this distributor delivers to.</param>
    public DistributesToAttribute(Dsp dsp)
    {
        Dsp = dsp;
    }
}
