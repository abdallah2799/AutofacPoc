using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Api.MicrosoftDI.Registrations;

/// <summary>
/// Registers everything from the <b>Application</b> layer (the use cases).
/// </summary>
/// <remarks>
/// The built-in DI has no "module" concept. The common way to group registrations
/// is an <b>extension method</b> on <see cref="IServiceCollection"/>, so that
/// <c>Program.cs</c> can call <c>builder.Services.AddApplication()</c>.
/// Compare with <c>ApplicationModule</c> in the Autofac API.
/// </remarks>
public static class ApplicationRegistrations
{
    /// <summary>Adds the Application layer's services.</summary>
    /// <param name="services">The app's service collection (<c>builder.Services</c>).</param>
    /// <returns>The same collection, so calls can be chained.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<DistributeReleaseService>();
        services.AddScoped<DistributeToAllDspsService>();
        return services;
    }
}
