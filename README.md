# Autofac POC

A step-by-step **proof of concept** for learning [Autofac](https://autofac.org/), the dependency injection (DI)
container for .NET, inside a realistic **ASP.NET Core Web API** built on **Clean Architecture**.

## What this repo is for

1. **Learn Autofac's features one at a time.** Each phase adds exactly one feature, with comments explaining it.
2. **Decide whether Autofac is worth using** over ASP.NET Core's built-in DI. The same API is built twice,
   so you can see what Autofac adds and what it costs.

The example is a tiny **music distribution API**: it sends a release (a song or album) to DSPs such as Spotify,
Anghami or Apple Music. It was chosen because real distribution code needs exactly what Autofac is good at:
one interface with many implementations chosen at runtime, plus logging and security wrapped around many methods.

## How the repo is organised

```
src/
├── AutofacPoc.Domain           Business objects (Release). References nothing.
├── AutofacPoc.Application      Use cases + interfaces they need (IDspDistributor...). → Domain
├── AutofacPoc.Infrastructure   Implementations of those interfaces (SpotifyDistributor...). → Application
├── AutofacPoc.Api.MicrosoftDI  Web API using the BUILT-IN container      → http://localhost:5001
└── AutofacPoc.Api.Autofac      The SAME Web API using AUTOFAC            → http://localhost:5002
```

Domain, Application and Infrastructure are **shared** and never mention a DI container.
Only the two API projects differ, mostly in `Program.cs`, which is the **composition root**
(the one place that decides which class implements which interface).

## How to run it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

Start each API in its own terminal. They use different ports, so both can run at the same time:

```bash
dotnet run --project src/AutofacPoc.Api.MicrosoftDI   # opens http://localhost:5001/scalar
dotnet run --project src/AutofacPoc.Api.Autofac       # opens http://localhost:5002/scalar
```

## How to try the endpoints

- **Scalar** (opens automatically): an interactive page listing every endpoint with its description.
  Pick an endpoint, fill in the parameters and click **Send**.
- **`.http` files**: each API project has one (`AutofacPoc.Api.*.http`). In Rider or VS Code,
  click **Send request** above a call.
- **curl**:
  ```bash
  curl -X POST http://localhost:5001/releases/1/distribute
  ```

| Endpoint | What it does |
|---|---|
| `POST /releases/{releaseId}/distribute` | Sends the release to Spotify. Releases `1` and `2` exist; `99` returns 404. |

## How to follow the phases

Each phase is **one commit** on `main`. To study a phase:

1. Read its section below.
2. Open the commit on GitHub (or run `git show <commit>`) to see exactly what changed.
3. Compare `src/AutofacPoc.Api.MicrosoftDI/Program.cs` with `src/AutofacPoc.Api.Autofac/Program.cs`.
4. Run both APIs and call the same endpoint on each.

The code is documented with `///` comments; they also show up as tooltips in your IDE and as descriptions in Scalar.

## Phases

| # | Feature | Status |
|---|---|---|
| 0 | Setup: Clean Architecture layers, two APIs, Scalar docs | ✅ Done |
| 1 | Plug Autofac into ASP.NET Core, basic registration | ✅ Done |
| 2 | **Modules**: each layer registers its own services | ⏳ Next |
| 3 | **Keyed services + `IEnumerable`**: pick the DSP at runtime, distribute to all | |
| 4 | **Assembly scanning**: a new DSP = one new class, no registration edits | |
| 5 | **Decorators**: a logging wrapper around `IDspDistributor` | |
| 6 | **Interceptors (AOP)**: one logging and one security interceptor for many services | |
| 7 | **Property injection**: a `BaseController` that gets its logger automatically | |

### Phase 0: Setup

- The solution follows Clean Architecture: dependencies point inwards (`Api → Infrastructure → Application → Domain`).
- `DistributeReleaseService` uses **constructor injection**: it asks for `IReleaseRepository` and `IDspDistributor`
  and never creates them itself.
- Both APIs still use the built-in container and are identical apart from their port and title.
  Phase 1 switches one of them to Autofac.
- Scalar + OpenAPI document the endpoints; `Directory.Build.props` turns on XML docs for every project
  so `///` comments flow into Scalar.
- **Limitation to remember:** `IDspDistributor` can only map to one class (Spotify). Phase 3 fixes that.

### Phase 1: Plug Autofac into ASP.NET Core

**Package:** `Autofac.Extensions.DependencyInjection` (it brings in `Autofac` itself). Only the Autofac API has it.

**Two steps in `Program.cs`:**

```csharp
// 1. Use Autofac instead of the built-in container.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// 2. Register services with Autofac.
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterType<InMemoryReleaseRepository>().As<IReleaseRepository>().SingleInstance();
    container.RegisterType<SpotifyDistributor>().As<IDspDistributor>().InstancePerLifetimeScope();
    container.RegisterType<DistributeReleaseService>().InstancePerLifetimeScope();
});
```

**Same registrations, two syntaxes:**

| Built-in DI (`Api.MicrosoftDI`) | Autofac (`Api.Autofac`) | Meaning |
|---|---|---|
| `AddSingleton<IX, X>()` | `RegisterType<X>().As<IX>().SingleInstance()` | One object for the whole app |
| `AddScoped<IX, X>()` | `RegisterType<X>().As<IX>().InstancePerLifetimeScope()` | One object per HTTP request |
| `AddTransient<IX, X>()` | `RegisterType<X>().As<IX>()` (the default) | A new object every time one is needed |

**Things to notice:**
- Nothing else changed: not the controller, not the Application or Infrastructure layers. Swapping the container
  only touches the composition root.
- `builder.Services.AddControllers()` and `AddOpenApi()` still work. Autofac imports everything registered in
  `builder.Services`, so you can mix both styles, which helps when moving an existing project to Autofac step by step.
- Each API logs which container it runs at startup: `DI container in use: ServiceProvider` (built-in) vs
  `DI container in use: AutofacServiceProvider` (Autofac).
- At this point Autofac does the same job as the built-in container with slightly longer syntax.
  Its real advantages start in Phase 2.
