using Autofac;
using AutofacPoc.Api.Autofac.Distribution;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Domain.Distribution;
using AutofacPoc.Infrastructure.Distributors;
using AutofacPoc.Infrastructure.Releases;

namespace AutofacPoc.Api.Autofac.Modules;

/// <summary>
/// Registers everything from the <b>Infrastructure</b> layer: which concrete class
/// implements each Application interface.
/// </summary>
/// <remarks>
/// When a new implementation is added (e.g. a real database repository, or a new DSP),
/// this is the only file that changes.
/// </remarks>
public class InfrastructureModule : Module
{
    /// <summary>Autofac calls this once, while building the container.</summary>
    /// <param name="builder">The same <see cref="ContainerBuilder"/> used in Program.cs.</param>
    protected override void Load(ContainerBuilder builder)
    {
        // One for the whole app (built-in DI: AddSingleton): the fake data must be shared.
        builder.RegisterType<InMemoryReleaseRepository>().As<IReleaseRepository>().SingleInstance();

        // ---- One distributor per DSP ----
        // Each registration exposes the class in TWO ways at once:
        //   .As<IDspDistributor>()             -> included when someone asks for IEnumerable<IDspDistributor>
        //   .Keyed<IDspDistributor>(Dsp.X)     -> found by key, through IIndex<Dsp, IDspDistributor>
        // It is still ONE registration, so within a request both ways return the same object.
        builder.RegisterType<SpotifyDistributor>()
               .As<IDspDistributor>()
               .Keyed<IDspDistributor>(Dsp.Spotify)
               .InstancePerLifetimeScope();

        builder.RegisterType<AnghamiDistributor>()
               .As<IDspDistributor>()
               .Keyed<IDspDistributor>(Dsp.Anghami)
               .InstancePerLifetimeScope();

        builder.RegisterType<AppleMusicDistributor>()
               .As<IDspDistributor>()
               .Keyed<IDspDistributor>(Dsp.AppleMusic)
               .InstancePerLifetimeScope();

        // Picks a distributor by key using Autofac's IIndex (see the class for details).
        builder.RegisterType<AutofacDspDistributorProvider>().As<IDspDistributorProvider>().InstancePerLifetimeScope();
    }
}
