using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Infrastructure.Distributors;
using AutofacPoc.Infrastructure.Releases;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ---- Dependency registrations (built-in Microsoft DI) ----
builder.Services.AddSingleton<IReleaseRepository, InMemoryReleaseRepository>();
builder.Services.AddScoped<IDspDistributor, SpotifyDistributor>();
builder.Services.AddScoped<DistributeReleaseService>();

var app = builder.Build();

app.MapControllers();

app.Run();
