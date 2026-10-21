using AutofacPoc.Application.Abstractions;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Infrastructure.Releases;

/// <summary>
/// Fake database: a fixed list of releases kept in memory.
/// </summary>
/// <remarks>
/// Infrastructure layer: the concrete "how" behind the Application interfaces.
/// A real project would query SQL Server here; the rest of the app would not change.
/// It is registered as a singleton, so the same list is used by every request.
/// </remarks>
public class InMemoryReleaseRepository : IReleaseRepository
{
    private readonly List<Release> _releases = new()
    {
        new Release(1, "Lost in Cairo", "Nour"),
        new Release(2, "Desert Nights", "Karim"),
    };

    /// <inheritdoc />
    public Release? GetById(int id)
    {
        return _releases.FirstOrDefault(r => r.Id == id);
    }
}
