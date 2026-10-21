using AutofacPoc.Application.Abstractions;
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

        // One per HTTP request.
        services.AddScoped<IDspDistributor, SpotifyDistributor>();
        return services;
    }
}
