using AutofacPoc.Api.MicrosoftDI.Distribution;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Decorators;
using AutofacPoc.Infrastructure.Releases;

namespace AutofacPoc.Api.MicrosoftDI.Registrations;

/// <summary>
/// Registers everything from the <b>Infrastructure</b> layer: which concrete class
/// implements each Application interface.
/// </summary>
/// <remarks>
/// Built-in DI version of grouping registrations (an extension method).
/// Compare with <c>InfrastructureModule</c> in the Autofac API.
/// </remarks>
public static class InfrastructureRegistrations
{
    /// <summary>Adds the Infrastructure layer's services.</summary>
    /// <param name="services">The app's service collection (<c>builder.Services</c>).</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // One for the whole app: the fake data must be shared.
        services.AddSingleton<IReleaseRepository, InMemoryReleaseRepository>();

        // ---- Every distributor, found by scanning (written by hand) ----
        // The built-in container has no assembly scanning, so we do it ourselves with reflection:
        // list every class in the Infrastructure project that implements IDspDistributor.
        // (The Scrutor NuGet package adds scanning to the built-in container, as an alternative.)
        var infrastructureAssembly = typeof(InMemoryReleaseRepository).Assembly; // any class from that project
        var distributorTypes = infrastructureAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && typeof(IDspDistributor).IsAssignableFrom(type));

        foreach (var distributorType in distributorTypes)
        {
            var dsp = GetDsp(distributorType);

            // Step 1: register the real class by itself, so it can be created and then wrapped.
            services.AddScoped(distributorType);

            // Step 2: register under its key, but return the real class WRAPPED in the logging decorator.
            // The built-in container has no decorator feature, so we build the wrapper ourselves with `new`.
            // (The Scrutor package adds services.Decorate<...>() as an alternative.)
            services.AddKeyedScoped<IDspDistributor>(dsp, (sp, key) =>
                new LoggingDspDistributor(
                    (IDspDistributor)sp.GetRequiredService(distributorType),
                    sp.GetRequiredService<ILogger<LoggingDspDistributor>>()));

            // Step 3: register AGAIN without a key, or it won't appear in IEnumerable<IDspDistributor>.
            // The lambda forwards to the keyed (already wrapped) one, so both return the same object per request.
            services.AddScoped<IDspDistributor>(sp => sp.GetRequiredKeyedService<IDspDistributor>(dsp));
        }

        // Picks a distributor by key at runtime (see the class for details).
        services.AddScoped<IDspDistributorProvider, MicrosoftDiDspDistributorProvider>();
        return services;
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
