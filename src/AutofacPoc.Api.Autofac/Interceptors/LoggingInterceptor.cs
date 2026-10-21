using System.Diagnostics;
using Castle.DynamicProxy;

namespace AutofacPoc.Api.Autofac.Interceptors;

/// <summary>
/// An <b>interceptor</b> that logs ANY method call on ANY service it is attached to.
/// </summary>
/// <remarks>
/// <para>
/// Compare with the Phase 5 decorator: <c>LoggingDspDistributor</c> implements
/// <c>IDspDistributor</c>, so it can only wrap distributors. This class implements Castle's
/// <see cref="IInterceptor"/> instead, so the same class can wrap distributors, the release
/// repository, or any other interface.
/// </para>
/// <para>
/// How it works: Autofac (through Castle DynamicProxy) creates a "proxy" object at runtime
/// that implements the service's interface. Every call on the proxy comes here first as an
/// <see cref="IInvocation"/>. <c>invocation.Proceed()</c> runs the real method.
/// </para>
/// </remarks>
public class LoggingInterceptor : IInterceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    /// <summary>Called by Autofac.</summary>
    /// <param name="logger">Where to write the log lines.</param>
    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    /// <summary>Runs around every intercepted method call.</summary>
    /// <param name="invocation">The call being made: which method, on which object, with which arguments.</param>
    public void Intercept(IInvocation invocation)
    {
        // e.g. "AnghamiDistributor.Distribute" or "InMemoryReleaseRepository.GetById"
        var methodName = $"{invocation.TargetType?.Name ?? invocation.Method.DeclaringType?.Name}.{invocation.Method.Name}";
        _logger.LogInformation("[Interceptor] {Method} starting", methodName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            invocation.Proceed(); // run the real method (or the next interceptor)
            _logger.LogInformation("[Interceptor] {Method} finished in {ElapsedMs} ms",
                methodName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Interceptor] {Method} failed after {ElapsedMs} ms: {Error}",
                methodName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw; // logging must not hide the error
        }
    }
}
