using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Application.Security;
using AutofacPoc.Domain.Releases;

namespace AutofacPoc.Application.Decorators;

/// <summary>
/// A decorator that checks the "distribute" permission before letting a distributor run.
/// </summary>
/// <remarks>
/// <para>
/// Used only by the <b>built-in DI</b> API, which has no interceptors. It is the
/// hand-written equivalent of Autofac's <c>SecurityInterceptor</c>.
/// </para>
/// <para>
/// Notice its limits: it only protects <see cref="IDspDistributor"/>, and the permission
/// is written here by hand. Protecting another interface would need another class like this one.
/// The interceptor instead reads <see cref="RequiresPermissionAttribute"/> from any method it wraps.
/// </para>
/// </remarks>
public class SecurityDspDistributor : IDspDistributor
{
    private readonly IDspDistributor _inner;
    private readonly ICurrentUser _currentUser;

    /// <summary>Called by the container code, which passes in the distributor to protect.</summary>
    /// <param name="inner">The real distributor (Spotify, Anghami, ...).</param>
    /// <param name="currentUser">Who is making the request.</param>
    public SecurityDspDistributor(IDspDistributor inner, ICurrentUser currentUser)
    {
        _inner = inner;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public DistributionResult Distribute(Release release)
    {
        if (!_currentUser.HasPermission(Permissions.Distribute))
            throw new PermissionDeniedException(Permissions.Distribute);

        return _inner.Distribute(release);
    }
}
