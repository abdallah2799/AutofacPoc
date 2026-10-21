using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// Finds releases. The Application layer only says WHAT it needs;
/// the Infrastructure layer decides HOW (database, file, fake list...).
/// </summary>
/// <remarks>
/// Because the use case depends on this interface and not on a concrete class,
/// the DI container (built-in or Autofac) chooses the implementation at startup.
/// </remarks>
public interface IReleaseRepository
{
    /// <summary>Returns the release with this id, or <c>null</c> if there is none.</summary>
    /// <param name="id">Unique number of the release.</param>
    Release? GetById(int id);
}
