using System.Reflection;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Security;
using Castle.DynamicProxy;

namespace AutofacPoc.Api.Autofac.Interceptors;

/// <summary>
/// An <b>interceptor</b> that enforces <see cref="RequiresPermissionAttribute"/> on any method it wraps.
/// </summary>
/// <remarks>
/// <para>
/// Before each call it looks at the method being called. If the method has
/// <c>[RequiresPermission("...")]</c> and the current user lacks that permission, it throws
/// <see cref="PermissionDeniedException"/> and the real method never runs. Methods without
/// the attribute run normally.
/// </para>
/// <para>
/// So protecting a new method anywhere is just adding the attribute; no new class is needed.
/// Compare with <c>SecurityDspDistributor</c> (built-in DI API), which only protects distributors.
/// </para>
/// </remarks>
public class SecurityInterceptor : IInterceptor
{
    private readonly ICurrentUser _currentUser;

    /// <summary>Called by Autofac.</summary>
    /// <param name="currentUser">Who is making the request.</param>
    public SecurityInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>Runs before every intercepted method call.</summary>
    /// <param name="invocation">The call being made: which method, on which object, with which arguments.</param>
    public void Intercept(IInvocation invocation)
    {
        // invocation.Method is the interface method, e.g. IDspDistributor.Distribute,
        // which is where the [RequiresPermission] attribute is written.
        var requirement = invocation.Method.GetCustomAttribute<RequiresPermissionAttribute>();

        if (requirement != null && !_currentUser.HasPermission(requirement.Permission))
            throw new PermissionDeniedException(requirement.Permission);

        invocation.Proceed(); // allowed: run the real method
    }
}
