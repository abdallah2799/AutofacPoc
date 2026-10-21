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
│   ├── Registrations/          AddApplication(), AddInfrastructure() extension methods
│   └── Distribution/           Picks a DSP's distributor by key (built-in keyed services)
└── AutofacPoc.Api.Autofac      The SAME Web API using AUTOFAC            → http://localhost:5002
    ├── Modules/                ApplicationModule, InfrastructureModule
    └── Distribution/           Picks a DSP's distributor by key (Autofac IIndex)
```

Domain, Application and Infrastructure are **shared** and never mention a DI container.
Only the two API projects differ: their **composition root** (`Program.cs` plus the `Registrations/` or `Modules/`
folder) is the one place that decides which class implements which interface.

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
  curl -X POST http://localhost:5001/releases/1/distribute/Spotify
  ```

| Endpoint | What it does |
|---|---|
| `POST /releases/{releaseId}/distribute/{dsp}` | Sends the release to one DSP: `Spotify`, `Anghami` or `AppleMusic`. |
| `POST /releases/{releaseId}/distribute-all` | Sends the release to every DSP and returns one result each. |

Releases `1` and `2` exist; `99` returns 404. An unknown DSP (e.g. `TikTok`) returns 400.

## How to follow the phases

Each phase is **one commit** on `main`. To study a phase:

1. Read its section below.
2. Open the commit on GitHub (or run `git show <commit>`) to see exactly what changed.
3. Compare the two composition roots: `Program.cs` + `Registrations/` (built-in) with `Program.cs` + `Modules/` (Autofac).
4. Run both APIs and call the same endpoint on each.

The code is documented with `///` comments; they also show up as tooltips in your IDE and as descriptions in Scalar.

## Phases

| # | Feature | Status |
|---|---|---|
| 0 | Setup: Clean Architecture layers, two APIs, Scalar docs | ✅ Done |
| 1 | Plug Autofac into ASP.NET Core, basic registration | ✅ Done |
| 2 | **Modules**: each layer registers its own services | ✅ Done |
| 3 | **Keyed services + `IEnumerable`**: pick the DSP at runtime, distribute to all | ✅ Done |
| 4 | **Assembly scanning**: a new DSP = one new class, no registration edits | ⏳ Next |
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
  (The Phase 0 endpoint `POST /releases/{id}/distribute` was replaced in Phase 3.)

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

### Phase 2: Modules

**Problem:** with every registration in `Program.cs`, the file grows with each new service, and one file has to know
every class from every layer.

**Autofac's answer: a module per layer.** A module is a class that inherits `Autofac.Module` and puts its
registrations in `Load`:

```csharp
public class InfrastructureModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<InMemoryReleaseRepository>().As<IReleaseRepository>().SingleInstance();
        builder.RegisterType<SpotifyDistributor>().As<IDspDistributor>().InstancePerLifetimeScope();
    }
}
```

`Program.cs` then registers whole layers:

```csharp
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule<ApplicationModule>();
    container.RegisterModule<InfrastructureModule>();
});
```

**The built-in DI equivalent** is an extension method per layer (`Api.MicrosoftDI/Registrations/`):

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
```

**Honest comparison:** for simple grouping like this, modules and extension methods do the same job.
Modules become better when you need more:

| | Extension methods (built-in) | Autofac modules |
|---|---|---|
| Group registrations per layer | ✅ | ✅ |
| Settings passed in as properties, e.g. `new InfrastructureModule { UseFakeDsps = true }` | Only as method parameters | ✅ Module is a normal class |
| Find and register **all** modules in a project automatically (`RegisterAssemblyModules`) | ❌ Call each method yourself | ✅ (shown in Phase 4) |
| React to every registration, e.g. attach logging to all services | ❌ | ✅ (advanced, via `AttachToComponentRegistration`) |

**Where should modules live?** Here they are in the API project, so Domain, Application and Infrastructure stay free
of any DI container (they don't reference Autofac at all). Many teams instead put each module **inside its layer**
(e.g. `InfrastructureModule` in the Infrastructure project). That keeps registrations next to the classes,
but every layer then depends on the Autofac package. Both are valid; pick one and stay consistent.

### Phase 3: Keyed services + `IEnumerable`

**Problem:** there is one `IDspDistributor` per DSP (Spotify, Anghami, Apple Music). We need to:
1. pick **one** at runtime, from the URL: `POST /releases/1/distribute/Anghami`;
2. use **all** of them at once: `POST /releases/1/distribute-all`.

**1. Pick one by key (keyed services).** Each distributor is registered under a key, the `Dsp` enum value.
In Autofac, one registration can expose the class both ways:

```csharp
builder.RegisterType<AnghamiDistributor>()
       .As<IDspDistributor>()                  // included in IEnumerable<IDspDistributor>
       .Keyed<IDspDistributor>(Dsp.Anghami)    // found by key
       .InstancePerLifetimeScope();
```

To look one up, a class simply asks for **`IIndex<Dsp, IDspDistributor>`**, a dictionary of all keyed distributors
that Autofac fills in:

```csharp
public AutofacDspDistributorProvider(IIndex<Dsp, IDspDistributor> distributors) { ... }
public IDspDistributor GetFor(Dsp dsp) => _distributors[dsp];
```

**2. Use all of them (`IEnumerable<T>`).** When a constructor asks for `IEnumerable<IDspDistributor>`, the container
passes every registered distributor. `DistributeToAllDspsService` never names a DSP, so a new DSP is included
automatically. This part works the same in both containers.

**Keeping Autofac out of the Application layer:** the use case can't ask for Autofac's `IIndex` (Application must
not reference Autofac). So Application defines a tiny interface, `IDspDistributorProvider` ("give me the distributor
for this DSP"), and each API implements it with its own container. This is the price of strict Clean Architecture.

**The built-in DI equivalent** (`Api.MicrosoftDI/Registrations/InfrastructureRegistrations.cs`):

```csharp
// Step 1: keyed registration (supported since .NET 8)
services.AddKeyedScoped<IDspDistributor, AnghamiDistributor>(Dsp.Anghami);
// Step 2: register AGAIN without a key, or it won't appear in IEnumerable<IDspDistributor>
services.AddScoped<IDspDistributor>(sp => sp.GetRequiredKeyedService<IDspDistributor>(Dsp.Anghami));
```

And to choose a key at runtime, the provider takes the whole `IServiceProvider`:

```csharp
public MicrosoftDiDspDistributorProvider(IServiceProvider serviceProvider) { ... }
public IDspDistributor GetFor(Dsp dsp) => _serviceProvider.GetRequiredKeyedService<IDspDistributor>(dsp);
```

**Comparison:**

| | Built-in DI | Autofac |
|---|---|---|
| Register a class under a key | ✅ `AddKeyedScoped(key)` (.NET 8+) | ✅ `.Keyed<T>(key)` |
| Same class keyed **and** in `IEnumerable<T>` | Two registrations per DSP (the second forwards to the first) | One registration: `.As<T>().Keyed<T>(key)` |
| Choose the key at runtime | Inject `IServiceProvider` and ask it ("service locator": the constructor no longer shows what the class needs) | Inject `IIndex<TKey, T>`, a normal typed dependency |
| Key known at compile time | `[FromKeyedServices(Dsp.Spotify)]` on a parameter | `[KeyFilter(Dsp.Spotify)]` on a parameter (its class registered with `.WithAttributeFiltering()`) |
| `IEnumerable<T>` of all implementations | ✅ | ✅ |

**Try it:** in Scalar, the `dsp` parameter is a dropdown (Spotify / Anghami / AppleMusic). This needs
`ConfigureHttpJsonOptions(... JsonStringEnumConverter ...)` in `Program.cs`, which makes the OpenAPI document list
enum **names** instead of numbers.

**Still to improve:** adding a DSP means a new class **plus** new registration lines. Phase 4 removes the
registration lines with assembly scanning.
