namespace AutofacPoc.Application.Security;

/// <summary>
/// Thrown when the current user lacks the permission a method requires.
/// The API turns it into an HTTP 403 Forbidden response.
/// </summary>
public class PermissionDeniedException : Exception
{
    /// <summary>Creates the exception.</summary>
    /// <param name="permission">The permission that was missing.</param>
    public PermissionDeniedException(string permission)
        : base($"Permission '{permission}' is required.")
    {
    }
}
