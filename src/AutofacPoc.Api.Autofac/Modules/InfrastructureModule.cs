using Autofac;
using AutofacPoc.Application.Abstractions;
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

        // One per HTTP request (built-in DI: AddScoped).
        builder.RegisterType<SpotifyDistributor>().As<IDspDistributor>().InstancePerLifetimeScope();
    }
}
