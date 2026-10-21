using Autofac.Features.OwnedInstances;
using AutofacPoc.Application.Abstractions;
using AutofacPoc.Application.Distribution;

namespace AutofacPoc.Api.Autofac.Workers;

/// <summary>
/// Background worker: every second, takes scheduled distributions from the queue and delivers them.
/// </summary>
/// <remarks>
/// <para>
/// <b>The problem:</b> this worker lives for the whole app (a singleton), but each job needs
/// <see cref="DistributeReleaseService"/> and its per-request services (distributor, current user...).
/// Injecting <c>DistributeReleaseService</c> directly would keep ONE instance forever (a "captive dependency").
/// We need a fresh set of services per job, cleaned up after the job.
/// </para>
/// <para>
/// <b>Autofac's answer: <c>Func&lt;Owned&lt;T&gt;&gt;</c></b>, two relationship types combined:
/// <list type="bullet">
/// <item><c>Func&lt;T&gt;</c>: "give me a factory instead of an object"; calling it creates a new one.</item>
/// <item><c>Owned&lt;T&gt;</c>: "the object comes with its own small lifetime scope, and I own it";
/// disposing it disposes everything that was created for it.</item>
/// </list>
/// Both are ordinary constructor parameters: Autofac understands them automatically, nothing is registered
/// for them, and this class never touches the container.
/// </para>
/// <para>
/// Compare with the built-in DI API's worker, which must ask the container itself
/// (<c>CreateScope()</c> + <c>GetRequiredService</c>): the service-locator style.
/// </para>
/// </remarks>
public class ScheduledDistributionWorker : BackgroundService
{
    private readonly IDistributionQueue _queue;
    private readonly Func<Owned<DistributeReleaseService>> _createDistributeService;
    private readonly ILogger<ScheduledDistributionWorker> _logger;

    /// <summary>Called by Autofac when the app starts.</summary>
    /// <param name="queue">Where scheduled distributions wait.</param>
    /// <param name="createDistributeService">Factory: each call gives a fresh, owned <see cref="DistributeReleaseService"/>.</param>
    /// <param name="logger">Where to write progress.</param>
    public ScheduledDistributionWorker(
        IDistributionQueue queue,
        Func<Owned<DistributeReleaseService>> createDistributeService,
        ILogger<ScheduledDistributionWorker> logger)
    {
        _queue = queue;
        _createDistributeService = createDistributeService;
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
                // A fresh unit of work for this job only...
                using (Owned<DistributeReleaseService> owned = _createDistributeService())
                {
                    Process(owned.Value, job);
                } // ...and here everything created for it is disposed.
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
