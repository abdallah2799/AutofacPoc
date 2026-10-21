using Autofac;
using Autofac.Extras.DynamicProxy;
using AutofacPoc.Api.Autofac.Distribution;
using AutofacPoc.Api.Autofac.Interceptors;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Infrastructure.Releases;

namespace AutofacPoc.Api.Autofac.Modules;

/// <summary>
/// Registers everything from the <b>Infrastructure</b> layer: which concrete class
/// implements each Application interface.
/// </summary>
/// <remarks>
/// Distributors are found by <b>assembly scanning</b>: adding a new DSP class never changes this file.
/// </remarks>
public class InfrastructureModule : Module
{
    /// <summary>Autofac calls this once, while building the container.</summary>
    /// <param name="builder">The same <see cref="ContainerBuilder"/> used in Program.cs.</param>
    protected override void Load(ContainerBuilder builder)
    {
        // ---- INTERCEPTORS: registered once, then attached to any service below ----
        builder.RegisterType<LoggingInterceptor>();
        builder.RegisterType<SecurityInterceptor>();

        // One for the whole app (built-in DI: AddSingleton): the fake data must be shared.
        // The SAME logging interceptor as the distributors, on a different interface.
        builder.RegisterType<InMemoryReleaseRepository>().As<IReleaseRepository>().SingleInstance()
               .EnableInterfaceInterceptors()
               .InterceptedBy(typeof(LoggingInterceptor));

        // ---- Every distributor, found by ASSEMBLY SCANNING ----
        // Instead of one registration per DSP, Autofac looks through the Infrastructure project
        // (an "assembly" = one compiled project/.dll) and registers every class that implements
        // IDspDistributor. A new DSP class is picked up automatically.
        var infrastructureAssembly = typeof(InMemoryReleaseRepository).Assembly; // any class from that project

        builder.RegisterAssemblyTypes(infrastructureAssembly)
               .AssignableTo<IDspDistributor>()        // only classes that implement IDspDistributor
               .As<IDspDistributor>()                  // included in IEnumerable<IDspDistributor>
               .Keyed<IDspDistributor>(type => GetDsp(type)) // key read from [DistributesTo(...)]
               .InstancePerLifetimeScope()
               .EnableInterfaceInterceptors()                // wrap each one in a runtime proxy...
               .InterceptedBy(typeof(LoggingInterceptor),    // ...that logs first,
                              typeof(SecurityInterceptor));  // ...then checks [RequiresPermission]

        // Phase 5's RegisterDecorator<LoggingDspDistributor, IDspDistributor>() was removed here:
        // LoggingInterceptor now does the same job, for more than one interface.

        // Picks a distributor by key using Autofac's IIndex (see the class for details).
        builder.RegisterType<AutofacDspDistributorProvider>().As<IDspDistributorProvider>().InstancePerLifetimeScope();
    }

    /// <summary>Reads the DSP key from the class's <see cref="DistributesToAttribute"/>.</summary>
    /// <param name="type">A distributor class found by the scan.</param>
    /// <exception cref="InvalidOperationException">The class has no [DistributesTo] attribute.</exception>
    private static object GetDsp(Type type)
    {
        var attribute = (DistributesToAttribute?)Attribute.GetCustomAttribute(type, typeof(DistributesToAttribute));
        if (attribute == null)
            throw new InvalidOperationException($"{type.Name} must have a [DistributesTo(...)] attribute.");

        return attribute.Dsp;
    }
}
