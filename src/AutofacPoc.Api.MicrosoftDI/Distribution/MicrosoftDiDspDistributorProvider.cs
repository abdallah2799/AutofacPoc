using AutofacPoc.Application.Abstractions;
using AutofacPoc.Domain.Distribution;

namespace AutofacPoc.Api.MicrosoftDI.Distribution;

/// <summary>
/// Built-in DI implementation of <see cref="IDspDistributorProvider"/>: picks the distributor by DSP key.
/// </summary>
/// <remarks>
/// <para>
/// Since .NET 8 the built-in container supports keyed services
/// (<c>AddKeyedScoped</c>), but it has no equivalent of Autofac's <c>IIndex</c>.
/// To choose a key at runtime, this class takes the whole <see cref="IServiceProvider"/>
/// and asks it directly with <c>GetRequiredKeyedService</c>.
/// </para>
/// <para>
/// Taking <see cref="IServiceProvider"/> is the "service locator" style: the class can
/// pull out anything, so its constructor no longer tells you what it really needs.
/// Compare with <c>AutofacDspDistributorProvider</c>, whose constructor asks only for
/// the distributors.
/// </para>
/// </remarks>
public class MicrosoftDiDspDistributorProvider : IDspDistributorProvider
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Called by the container, which supplies the current request's service provider.</summary>
    /// <param name="serviceProvider">Used to look up the keyed distributor.</param>
    public MicrosoftDiDspDistributorProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IDspDistributor GetFor(Dsp dsp)
    {
        return _serviceProvider.GetRequiredKeyedService<IDspDistributor>(dsp);
    }
}
