// =====================================================================
// Entry point of the API and its COMPOSITION ROOT: the one place that
// knows which concrete class implements each interface.
// This project is the same app, with Autofac as the container.
// =====================================================================
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutofacPoc.Api.Autofac.Security;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Security;
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

    // Show the X-Permissions header on every endpoint in Scalar, so it can be filled in there.
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        operation.Parameters ??= new List<IOpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HeaderCurrentUser.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Your permissions (stand-in for a real login). Send `distribute` to be allowed to distribute; leave empty to see the 403.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
        });
        return Task.CompletedTask;
    });
});

// The current user, read from the X-Permissions header (see Security/HeaderCurrentUser.cs).
// Web-specific services are registered here in builder.Services; this works for both containers.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HeaderCurrentUser>();

// ---- Step 1: tell ASP.NET Core to use Autofac instead of its built-in container ----
// After this line, Autofac creates every object in the app: controllers' dependencies,
// our services, and also ASP.NET Core's own services.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// ---- Step 2: register our services, one Autofac MODULE per layer ----
// Each module (see the Modules folder) holds that layer's registrations.
// RegisterAssemblyModules finds and loads EVERY module in this project, so a new module
// is picked up without editing this file.
// Everything added to builder.Services above (AddControllers, AddOpenApi...) still works:
// Autofac imports those registrations automatically.
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterAssemblyModules(typeof(Program).Assembly);
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

// Turn a PermissionDeniedException (thrown by the security check) into HTTP 403 Forbidden.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (PermissionDeniedException ex)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync(ex.Message);
    }
});

app.MapControllers();

app.Run();
