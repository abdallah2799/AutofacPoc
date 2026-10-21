using Autofac.Features.Indexed;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Domain.Distribution;

namespace AutofacPoc.Api.Autofac.Distribution;

/// <summary>
/// Autofac implementation of <see cref="IDspDistributorProvider"/>: picks the distributor by DSP key.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IIndex{TKey, TValue}"/> is an Autofac feature: ask for
/// <c>IIndex&lt;Dsp, IDspDistributor&gt;</c> and Autofac passes in a ready-made
/// "dictionary" of every distributor registered with <c>.Keyed&lt;IDspDistributor&gt;(dsp)</c>.
/// </para>
/// <para>
/// It is a normal constructor dependency, so this class never touches the container itself.
/// Objects are only created when looked up, so asking for Anghami does not build Spotify.
/// </para>
/// <para>
/// This small class exists only to keep Autofac out of the Application layer.
/// If Application referenced Autofac, the use case could ask for <c>IIndex</c> directly.
/// </para>
/// </remarks>
public class AutofacDspDistributorProvider : IDspDistributorProvider
{
    private readonly IIndex<Dsp, IDspDistributor> _distributors;

    /// <summary>Called by Autofac, which supplies the index of keyed distributors.</summary>
    /// <param name="distributors">Every distributor registered under a <see cref="Dsp"/> key.</param>
    public AutofacDspDistributorProvider(IIndex<Dsp, IDspDistributor> distributors)
    {
        _distributors = distributors;
    }

    /// <inheritdoc />
    public IDspDistributor GetFor(Dsp dsp)
    {
        return _distributors[dsp];
    }
}
