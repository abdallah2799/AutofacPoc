using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Api.MicrosoftDI.Workers;

/// <summary>
/// Background worker: every second, takes scheduled distributions from the queue and delivers them.
/// </summary>
/// <remarks>
/// <para>
/// <b>The problem:</b> this worker lives for the whole app (a singleton), but each job needs
/// <see cref="DistributeReleaseService"/> and its per-request services. We need a fresh set per job.
/// </para>
/// <para>
/// The built-in container has no <c>Func&lt;T&gt;</c> or <c>Owned&lt;T&gt;</c>, so the worker takes
/// <see cref="IServiceScopeFactory"/>, opens a scope per job and <b>asks the container</b> for the service
/// with <c>GetRequiredService</c>. That's the <b>service-locator</b> style: the constructor doesn't show
/// that this class really needs <c>DistributeReleaseService</c>, and a missing registration fails only when
/// a job runs. Compare with the Autofac API's worker, which takes <c>Func&lt;Owned&lt;DistributeReleaseService&gt;&gt;</c>.
/// </para>
/// </remarks>
public class ScheduledDistributionWorker : BackgroundService
{
    private readonly IDistributionQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledDistributionWorker> _logger;

    /// <summary>Called by the container when the app starts.</summary>
    /// <param name="queue">Where scheduled distributions wait.</param>
    /// <param name="scopeFactory">Used to open a fresh scope per job.</param>
    /// <param name="logger">Where to write progress.</param>
    public ScheduledDistributionWorker(
        IDistributionQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledDistributionWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Runs until the app stops.</summary>
    /// <param name="stoppingToken">Signals that the app is shutting down.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ScheduledDistribution? job;
            while ((job = _queue.TryDequeue()) != null)
            {
                // A fresh scope for this job only...
                using (var scope = _scopeFactory.CreateScope())
                {
                    // ...and we have to ASK the container for what we need (service locator).
                    var service = scope.ServiceProvider.GetRequiredService<DistributeReleaseService>();
                    Process(service, job);
                } // ...and here everything created in the scope is disposed.
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private void Process(DistributeReleaseService service, ScheduledDistribution job)
    {
        try
        {
            var result = service.Distribute(job.ReleaseId, job.Dsp);
            _logger.LogInformation("[Worker] Scheduled distribution done: release {ReleaseId} -> {Message}",
                job.ReleaseId, result?.Message ?? "release not found");
        }
        catch (Exception ex)
        {
            // One failed job must not stop the worker.
            _logger.LogWarning("[Worker] Scheduled distribution failed: release {ReleaseId} to {Dsp}: {Error}",
                job.ReleaseId, job.Dsp, ex.Message);
        }
    }
}
