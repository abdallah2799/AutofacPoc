using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Releases;
using Microsoft.Extensions.Logging;

namespace AutofacPoc.Infrastructure.Notifications;

/// <summary>
/// Fake label notification: writes the "email" to the log instead of sending it.
/// </summary>
/// <remarks>
/// Only registered when <c>Notifications:Enabled</c> is true in appsettings.json.
/// </remarks>
public class LogDeliveryNotifier : IDeliveryNotifier
{
    private readonly ILogger<LogDeliveryNotifier> _logger;

    /// <summary>Called by the container.</summary>
    /// <param name="logger">Where the pretend emails are written.</param>
    public LogDeliveryNotifier(ILogger<LogDeliveryNotifier> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void NotifyDelivered(Release release, DistributionResult result)
    {
        _logger.LogInformation("[Notifier] Email to label of {Artist}: '{Title}' is now live on {Dsp}",
            release.Artist, release.Title, result.Dsp);
    }
}
