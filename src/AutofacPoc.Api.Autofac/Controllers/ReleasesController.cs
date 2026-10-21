using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Distribution;
using Microsoft.AspNetCore.Mvc;

namespace AutofacPoc.Api.Autofac.Controllers;

/// <summary>
/// Endpoints for delivering releases to DSPs.
/// </summary>
/// <remarks>
/// The controller is created by the DI container for every HTTP request.
/// It asks for the use cases in its constructor and the container builds them
/// (and everything they need) automatically.
/// </remarks>
[ApiController]
[Route("releases")]
[Tags("Releases")]
public class ReleasesController : ControllerBase
{
    private readonly DistributeReleaseService _distributeReleaseService;
    // Lazy<T> (Phase 8): the service, and the 4 distributors it needs, are only built the first time
    // .Value is read, i.e. only on requests that call distribute-all. Autofac supports Lazy<T> automatically.
    private readonly Lazy<DistributeToAllDspsService> _distributeToAllDspsService;
    private readonly GetDeliveriesService _getDeliveriesService;
    private readonly ScheduleDistributionService _scheduleDistributionService;

    /// <summary>Called by the DI container, which supplies the use cases.</summary>
    /// <param name="distributeReleaseService">Sends a release to one DSP.</param>
    /// <param name="distributeToAllDspsService">Sends a release to every DSP.</param>
    /// <param name="getDeliveriesService">Reads the delivery history.</param>
    /// <param name="scheduleDistributionService">Queues a distribution for the background worker.</param>
    public ReleasesController(
        DistributeReleaseService distributeReleaseService,
        Lazy<DistributeToAllDspsService> distributeToAllDspsService,
        GetDeliveriesService getDeliveriesService,
        ScheduleDistributionService scheduleDistributionService)
    {
        _distributeReleaseService = distributeReleaseService;
        _distributeToAllDspsService = distributeToAllDspsService;
        _getDeliveriesService = getDeliveriesService;
        _scheduleDistributionService = scheduleDistributionService;
    }

    /// <summary>Distribute a release to one DSP, chosen in the URL.</summary>
    /// <remarks>
    /// The DSP is picked **at runtime** from the URL: the container returns the distributor
    /// registered under that key (Spotify, Anghami or AppleMusic).
    ///
    /// Try release **1** or **2** (they exist) and **99** (it doesn't).
    ///
    /// **Permission needed:** send the header `X-Permissions: distribute`, or you get **403**.
    /// </remarks>
    /// <param name="releaseId">Unique number of the release, e.g. 1.</param>
    /// <param name="dsp">Which DSP to deliver to: Spotify, Anghami or AppleMusic.</param>
    /// <response code="200">The release was delivered.</response>
    /// <response code="400">The DSP name is not one we support.</response>
    /// <response code="403">The caller lacks the "distribute" permission.</response>
    /// <response code="404">No release has this id.</response>
    [HttpPost("{releaseId}/distribute/{dsp}")]
    [ProducesResponseType(typeof(DistributionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult Distribute(int releaseId, Dsp dsp)
    {
        var result = _distributeReleaseService.Distribute(releaseId, dsp);
        if (result == null)
            return NotFound($"Release {releaseId} not found");

        return Ok(result);
    }

    /// <summary>Distribute a release to every DSP at once.</summary>
    /// <remarks>
    /// Uses every distributor the container knows about (one per DSP),
    /// so a newly registered DSP is included without changing this endpoint.
    ///
    /// **Permission needed:** send the header `X-Permissions: distribute`, or you get **403**.
    /// </remarks>
    /// <param name="releaseId">Unique number of the release, e.g. 1.</param>
    /// <response code="200">One result per DSP.</response>
    /// <response code="403">The caller lacks the "distribute" permission.</response>
    /// <response code="404">No release has this id.</response>
    [HttpPost("{releaseId}/distribute-all")]
    [ProducesResponseType(typeof(List<DistributionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult DistributeToAll(int releaseId)
    {
        var results = _distributeToAllDspsService.Value.DistributeToAll(releaseId); // built here, on first use
        if (results == null)
            return NotFound($"Release {releaseId} not found");

        return Ok(results);
    }

    /// <summary>Schedule a distribution: answer now, deliver shortly in the background.</summary>
    /// <remarks>
    /// Returns **202 Accepted** straight away. A background worker delivers the release about a second later;
    /// check `GET /releases/{releaseId}/deliveries` to see it.
    ///
    /// **Permission needed:** `X-Permissions: distribute` (checked now, while we know who the caller is).
    /// </remarks>
    /// <param name="releaseId">Unique number of the release, e.g. 1.</param>
    /// <param name="dsp">Which DSP to deliver to.</param>
    /// <response code="202">Queued; it will be delivered shortly.</response>
    /// <response code="400">The DSP name is not one we support.</response>
    /// <response code="403">The caller lacks the "distribute" permission.</response>
    /// <response code="404">No release has this id.</response>
    [HttpPost("{releaseId}/schedule/{dsp}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult Schedule(int releaseId, Dsp dsp)
    {
        if (!_scheduleDistributionService.Schedule(releaseId, dsp))
            return NotFound($"Release {releaseId} not found");

        return Accepted((string?)null, $"Release {releaseId} scheduled for {dsp}");
    }

    /// <summary>See where a release has been delivered.</summary>
    /// <remarks>
    /// Every successful delivery is recorded by the distributors' shared base class
    /// (<c>DspDistributorBase</c>), using a delivery history it receives through **property injection**.
    ///
    /// Distribute a release first, then call this to see the record. No permission needed to read.
    /// </remarks>
    /// <param name="releaseId">Unique number of the release, e.g. 1.</param>
    /// <response code="200">Every recorded delivery of this release, oldest first (empty if none yet).</response>
    [HttpGet("{releaseId}/deliveries")]
    [ProducesResponseType(typeof(List<DeliveryRecord>), StatusCodes.Status200OK)]
    public IActionResult GetDeliveries(int releaseId)
    {
        return Ok(_getDeliveriesService.GetDeliveries(releaseId));
    }
}
