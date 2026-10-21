// =====================================================================
// Entry point of the API and its COMPOSITION ROOT: the one place that
// knows which concrete class implements each interface.
// This project is the same app, with Autofac as the container.
// =====================================================================
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

// ---- Dependency registrations (built-in Microsoft DI) ----
// Read each line as: "when someone asks for X, give them Y".
// Singleton = one object for the whole app. Scoped = one object per HTTP request.
builder.Services.AddSingleton<IReleaseRepository, InMemoryReleaseRepository>();
builder.Services.AddScoped<IDspDistributor, SpotifyDistributor>();
builder.Services.AddScoped<DistributeReleaseService>();

var app = builder.Build();

// API docs only while developing, never in production.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                                                   // JSON at /openapi/v1.json
    app.MapScalarApiReference(options => options.WithTitle("Autofac POC - Autofac")); // UI at /scalar
}

app.MapControllers();

app.Run();
