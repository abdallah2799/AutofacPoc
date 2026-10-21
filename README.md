# Autofac POC

Learning [Autofac](https://autofac.org/) step by step, in an ASP.NET Core Web API built on Clean Architecture.

The same tiny **music distribution API** (send a release to Spotify, Anghami, Apple Music…) exists twice:

| Project | Container | Port |
|---|---|---|
| `AutofacPoc.Api.MicrosoftDI` | ASP.NET Core's built-in DI | 5001 |
| `AutofacPoc.Api.Autofac` | Autofac | 5002 |

Domain, Application and Infrastructure are shared, so comparing the two APIs shows exactly what Autofac adds.

## Phases
- [x] **0** – Setup: layers, both APIs answer `POST /releases/{id}/distribute`
- [ ] **1** – Plug Autofac into ASP.NET Core, basic registration
- [ ] **2** – Modules: each layer registers its own services
- [ ] **3** – Keyed services + `IEnumerable`: pick the DSP at runtime, distribute to all
- [ ] **4** – Assembly scanning: a new DSP = one new class
- [ ] **5** – Decorators: logging wrapper
- [ ] **6** – Interceptors (AOP): logging + security for many services
- [ ] **7** – Property injection: `BaseController`

## Run
```bash
dotnet run --project src/AutofacPoc.Api.MicrosoftDI
dotnet run --project src/AutofacPoc.Api.Autofac
```
Then use the `.http` file in each API project, or `curl -X POST http://localhost:5001/releases/1/distribute`.
