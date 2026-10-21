// =====================================================================
// Entry point of the API and its COMPOSITION ROOT: the one place that
// knows which concrete class implements each interface.
// This project is the baseline: what you get WITHOUT Autofac.
// =====================================================================
using AutofacPoc.Api.MicrosoftDI.Registrations;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Describe enums (like Dsp) by their names ("Spotify") instead of numbers (0),
// so Scalar shows a dropdown of DSP names. The OpenAPI generator reads these JSON settings.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// OpenAPI = a JSON description of every endpoint, built from the controllers
// and their /// comments. Scalar (below) shows it as an interactive page.
builder.Services.AddOpenApi(options =>
{
    // Text shown at the top of the Scalar page.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Autofac POC - Built-in Microsoft DI",
            Version = "v1",
            Description = """
                Part of the **Autofac POC**: one small music distribution API, built twice.

                This API uses ASP.NET Core's **built-in** dependency injection (`Microsoft.Extensions.DependencyInjection`).

                Compare with the **Autofac** API on http://localhost:5002/scalar - same endpoints, same behaviour, different container.
                """,
        };
        return Task.CompletedTask;
    });
});

// ---- Dependency registrations (built-in Microsoft DI), one extension method per layer ----
// Each method (see the Registrations folder) holds that layer's registrations.
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

// Proof of which DI container is running: look for this line in the console at startup.
// Built-in DI prints "ServiceProvider"; Autofac prints "AutofacServiceProvider".
app.Logger.LogInformation("DI container in use: {Container}", app.Services.GetType().Name);

// API docs only while developing, never in production.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                                                   // JSON at /openapi/v1.json
    app.MapScalarApiReference(options => options.WithTitle("Autofac POC - Built-in Microsoft DI")); // UI at /scalar
}

app.MapControllers();

app.Run();
