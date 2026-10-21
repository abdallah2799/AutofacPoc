using AutofacPoc.Api.MicrosoftDI.Distribution;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Domain.Distribution;
using AutofacPoc.Infrastructure.Distributors;
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

        // ---- One distributor per DSP: step 1, register each under its key ----
        services.AddKeyedScoped<IDspDistributor, SpotifyDistributor>(Dsp.Spotify);
        services.AddKeyedScoped<IDspDistributor, AnghamiDistributor>(Dsp.Anghami);
        services.AddKeyedScoped<IDspDistributor, AppleMusicDistributor>(Dsp.AppleMusic);

        // ---- Step 2: register them AGAIN without a key ----
        // In the built-in container, keyed registrations are NOT included when someone asks for
        // IEnumerable<IDspDistributor>. So each DSP needs a second, unkeyed registration.
        // The lambda forwards to the keyed one, so both return the same object per request.
        // (Autofac does both in one registration: .As<...>().Keyed<...>(key).)
        services.AddScoped<IDspDistributor>(sp => sp.GetRequiredKeyedService<IDspDistributor>(Dsp.Spotify));
        services.AddScoped<IDspDistributor>(sp => sp.GetRequiredKeyedService<IDspDistributor>(Dsp.Anghami));
        services.AddScoped<IDspDistributor>(sp => sp.GetRequiredKeyedService<IDspDistributor>(Dsp.AppleMusic));

        // Picks a distributor by key at runtime (see the class for details).
        services.AddScoped<IDspDistributorProvider, MicrosoftDiDspDistributorProvider>();
        return services;
    }
}
