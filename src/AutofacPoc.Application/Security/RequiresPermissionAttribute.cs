namespace AutofacPoc.Application.Security;

/// <summary>
/// Marks a method that only callers with a given permission may run,
/// e.g. <c>[RequiresPermission(Permissions.Distribute)]</c>.
/// </summary>
/// <remarks>
/// <para>
/// The attribute itself does nothing: it is a label. Something has to read it before
/// the method runs. In the Autofac API that is <c>SecurityInterceptor</c>, which checks
/// every intercepted method automatically. In the built-in DI API it is a hand-written
/// decorator (<c>SecurityDspDistributor</c>).
/// </para>
/// <para>
/// This is <b>business</b> security inside services ("may this user distribute?").
/// Checking who may call an endpoint at all is ASP.NET Core's job (<c>[Authorize]</c>).
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class RequiresPermissionAttribute : Attribute
{
    /// <summary>The permission the caller needs, e.g. "distribute".</summary>
    public string Permission { get; }

    /// <summary>Creates the attribute.</summary>
    /// <param name="permission">The permission the caller needs.</param>
    public RequiresPermissionAttribute(string permission)
    {
        Permission = permission;
    }
}
