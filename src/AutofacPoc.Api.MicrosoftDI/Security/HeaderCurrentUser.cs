using AutofacPoc.Application.Abstractions;

namespace AutofacPoc.Api.MicrosoftDI.Security;

/// <summary>
/// Reads the current user's permissions from the <c>X-Permissions</c> request header,
/// e.g. <c>X-Permissions: distribute</c> (several can be separated by commas).
/// </summary>
/// <remarks>
/// A deliberately simple stand-in for real authentication, so the POC can be tried from Scalar.
/// A real app would read the logged-in user's claims instead. No header means no permissions.
/// <para>
/// Work running in the background (Phase 8's scheduled distributions) has no HTTP request at all.
/// It runs as the system and is allowed, because the permission was already checked when the job
/// was scheduled (see <c>ScheduleDistributionService</c>).
/// </para>
/// </remarks>
public class HeaderCurrentUser : ICurrentUser
{
    /// <summary>The request header that carries the permissions.</summary>
    public const string HeaderName = "X-Permissions";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Called by the container.</summary>
    /// <param name="httpContextAccessor">Gives access to the current HTTP request.</param>
    public HeaderCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public bool HasPermission(string permission)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return true; // background work: no request, runs as the system (already checked when scheduled)

        var header = httpContext.Request.Headers[HeaderName].ToString();
        var permissions = header.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
