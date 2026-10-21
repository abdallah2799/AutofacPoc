using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Application.Abstractions;

/// <summary>Where releases are stored. Infrastructure decides how.</summary>
public interface IReleaseRepository
{
    Release? GetById(int id);
}
