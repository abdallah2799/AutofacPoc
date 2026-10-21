using Autofac;
using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Api.Autofac.Modules;

/// <summary>
/// Registers everything from the <b>Application</b> layer (the use cases).
/// </summary>
/// <remarks>
/// <para>
/// An Autofac <b>module</b> is a class that groups related registrations.
/// You inherit from <see cref="Module"/> and put the registrations in <see cref="Load"/>.
/// <c>Program.cs</c> then adds the whole group with one line:
/// <c>container.RegisterModule&lt;ApplicationModule&gt;()</c>.
/// </para>
/// <para>
/// One module per layer keeps <c>Program.cs</c> short, and each layer's
/// registrations live in one place that is easy to find.
/// </para>
/// </remarks>
public class ApplicationModule : Module
{
    /// <summary>Autofac calls this once, while building the container.</summary>
    /// <param name="builder">The same <see cref="ContainerBuilder"/> used in Program.cs.</param>
    protected override void Load(ContainerBuilder builder)
    {
        // One per HTTP request (built-in DI: AddScoped).
        // No .As<...>(): registered as its own class, because the controller asks for
        // these services themselves, not for an interface.
        builder.RegisterType<DistributeReleaseService>().InstancePerLifetimeScope();
        builder.RegisterType<DistributeToAllDspsService>().InstancePerLifetimeScope();
        builder.RegisterType<GetDeliveriesService>().InstancePerLifetimeScope();
    }
}
