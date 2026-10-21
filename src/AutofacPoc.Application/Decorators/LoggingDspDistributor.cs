using System.Diagnostics;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;
using Microsoft.Extensions.Logging;

namespace AutofacPoc.Application.Decorators;

/// <summary>
/// A <b>decorator</b>: wraps any <see cref="IDspDistributor"/> and adds logging around it.
/// </summary>
/// <remarks>
/// <para>
/// A decorator implements the SAME interface as the class it wraps, receives that class
/// in its constructor (<c>inner</c>), and forwards every call to it, adding behaviour
/// before and after. Callers can't tell the difference: they still get an <see cref="IDspDistributor"/>.
/// </para>
/// <para>
/// So logging is written ONCE here, instead of inside Spotify, Anghami, Apple Music and
/// YouTube Music. Those classes stay focused on delivering, and don't know they are being logged.
/// </para>
/// <para>
/// The container does the wrapping (Autofac: <c>RegisterDecorator</c>).
/// This class only depends on logging abstractions, not on any container.
/// </para>
/// </remarks>
public class LoggingDspDistributor : IDspDistributor
{
    private readonly IDspDistributor _inner;
    private readonly ILogger<LoggingDspDistributor> _logger;

    /// <summary>Called by the container, which passes in the distributor to wrap.</summary>
    /// <param name="inner">The real distributor (Spotify, Anghami, ...).</param>
    /// <param name="logger">Where to write the log lines.</param>
    public LoggingDspDistributor(IDspDistributor inner, ILogger<LoggingDspDistributor> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    /// <inheritdoc />
    public DistributionResult Distribute(Release release)
    {
        var distributorName = _inner.GetType().Name;
        _logger.LogInformation("[Decorator] {Distributor} starting: release {ReleaseId} '{Title}'",
            distributorName, release.Id, release.Title);

        var stopwatch = Stopwatch.StartNew();
        var result = _inner.Distribute(release); // the real work happens in the wrapped class
        stopwatch.Stop();

        _logger.LogInformation("[Decorator] {Distributor} finished in {ElapsedMs} ms: {Message}",
            distributorName, stopwatch.ElapsedMilliseconds, result.Message);

        return result;
    }
}
