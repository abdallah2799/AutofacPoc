// =====================================================================
// Entry point of the API and its COMPOSITION ROOT: the one place that
// knows which concrete class implements each interface.
// This project is the same app, with Autofac as the container.
// =====================================================================
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Infrastructure.Distributors;
using AutofacPoc.Infrastructure.Releases;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// OpenAPI = a JSON description of every endpoint, built from the controllers
// and their /// comments. Scalar (below) shows it as an interactive page.
builder.Services.AddOpenApi(options =>
{
    // Text shown at the top of the Scalar page.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Autofac POC - Autofac",
            Version = "v1",
            Description = """
                Part of the **Autofac POC**: one small music distribution API, built twice.

                This API uses **Autofac** as the dependency injection container.

                Compare with the **built-in DI** API on http://localhost:5001/scalar - same endpoints, same behaviour, different container.
                """,
        };
        return Task.CompletedTask;
    });
});

// ---- Step 1: tell ASP.NET Core to use Autofac instead of its built-in container ----
// After this line, Autofac creates every object in the app: controllers' dependencies,
// our services, and also ASP.NET Core's own services.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// ---- Step 2: register our services with Autofac's ContainerBuilder ----
// Read each line as: "when someone asks for X (.As<X>), create Y (RegisterType<Y>)".
// Everything added to builder.Services above (AddControllers, AddOpenApi...) still works:
// Autofac imports those registrations automatically.
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    // SingleInstance = one object for the whole app (built-in DI: AddSingleton).
    container.RegisterType<InMemoryReleaseRepository>().As<IReleaseRepository>().SingleInstance();

    // InstancePerLifetimeScope = one object per HTTP request (built-in DI: AddScoped).
    // ASP.NET Core starts a new Autofac "lifetime scope" for every request.
    container.RegisterType<SpotifyDistributor>().As<IDspDistributor>().InstancePerLifetimeScope();

    // No .As<...>(): registered as its own class, because the controller asks for
    // DistributeReleaseService itself, not for an interface.
    container.RegisterType<DistributeReleaseService>().InstancePerLifetimeScope();
});

var app = builder.Build();

// Proof of which DI container is running: look for this line in the console at startup.
// Built-in DI prints "ServiceProvider"; Autofac prints "AutofacServiceProvider".
app.Logger.LogInformation("DI container in use: {Container}", app.Services.GetType().Name);

// API docs only while developing, never in production.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                                                   // JSON at /openapi/v1.json
    app.MapScalarApiReference(options => options.WithTitle("Autofac POC - Autofac")); // UI at /scalar
}

app.MapControllers();

app.Run();
