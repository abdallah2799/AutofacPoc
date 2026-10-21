namespace AutofacPoc.Application.Abstractions;

/// <summary>
/// The user making the current request, as far as the Application layer needs to know.
/// </summary>
/// <remarks>
/// The API implements it (here: from the <c>X-Permissions</c> request header), so the
/// Application layer never touches HTTP. A real app would read the logged-in user's claims instead.
/// </remarks>
public interface ICurrentUser
{
    /// <summary>True if the current user has this permission.</summary>
    /// <param name="permission">A permission name, e.g. "distribute".</param>
    bool HasPermission(string permission);
}
