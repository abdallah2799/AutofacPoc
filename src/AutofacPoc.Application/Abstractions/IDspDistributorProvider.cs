using AutofacPoc.Domain.Distribution;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Gives back the distributor for a DSP chosen at runtime (e.g. from the URL).
/// </summary>
/// <remarks>
/// <para>
/// Why an interface instead of using the container directly? The Application layer
/// must not depend on Autofac or on ASP.NET Core. So it only describes what it needs here,
/// and each API project implements it with its own container's "keyed services" feature:
/// </para>
/// <list type="bullet">
/// <item>Autofac API: <c>AutofacDspDistributorProvider</c>, using Autofac's <c>IIndex&lt;Dsp, IDspDistributor&gt;</c>.</item>
/// <item>Built-in API: <c>MicrosoftDiDspDistributorProvider</c>, using <c>IServiceProvider.GetRequiredKeyedService</c>.</item>
/// </list>
/// </remarks>
public interface IDspDistributorProvider
{
    /// <summary>Returns the distributor registered for this DSP.</summary>
    /// <param name="dsp">Which DSP to deliver to.</param>
    IDspDistributor GetFor(Dsp dsp);
}
