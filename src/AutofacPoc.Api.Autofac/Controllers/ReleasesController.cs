using AutofacPoc.Application.Distribution;
using Microsoft.AspNetCore.Mvc;

namespace AutofacPoc.Api.Autofac.Controllers;

/// <summary>
/// Endpoints for delivering releases to DSPs.
/// </summary>
/// <remarks>
/// The controller is created by the DI container for every HTTP request.
/// It asks for <see cref="DistributeReleaseService"/> in its constructor and the
/// container builds it (and everything it needs) automatically.
/// </remarks>
[ApiController]
[Route("releases")]
[Tags("Releases")]
public class ReleasesController : ControllerBase
{
    private readonly DistributeReleaseService _distributeReleaseService;

    /// <summary>Called by the DI container, which supplies the service.</summary>
    /// <param name="distributeReleaseService">The use case that does the work.</param>
    public ReleasesController(DistributeReleaseService distributeReleaseService)
    {
        _distributeReleaseService = distributeReleaseService;
    }

    /// <summary>Distribute a release to Spotify.</summary>
    /// <remarks>
    /// Sends the release to the DSP and returns the outcome.
    /// For now every release goes to Spotify; choosing the DSP comes in a later phase.
    ///
    /// Try release **1** or **2** (they exist) and **99** (it doesn't).
    /// </remarks>
    /// <param name="releaseId">Unique number of the release, e.g. 1.</param>
    /// <response code="200">The release was delivered.</response>
    /// <response code="404">No release has this id.</response>
    [HttpPost("{releaseId}/distribute")]
    [ProducesResponseType(typeof(DistributionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult Distribute(int releaseId)
    {
        var result = _distributeReleaseService.Distribute(releaseId);
        if (result == null)
            return NotFound($"Release {releaseId} not found");

        return Ok(result);
    }
}
