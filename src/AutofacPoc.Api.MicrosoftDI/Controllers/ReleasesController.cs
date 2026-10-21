using AutofacPoc.Application.Distribution;
using AutofacPoc.Domain.Distribution;
using Microsoft.AspNetCore.Mvc;

namespace AutofacPoc.Api.MicrosoftDI.Controllers;

/// <summary>
/// Endpoints for delivering releases to DSPs.
/// </summary>
/// <remarks>
/// The controller is created by the DI container for every HTTP request.
/// It asks for the two use cases in its constructor and the container builds them
/// (and everything they need) automatically.
/// </remarks>
[ApiController]
[Route("releases")]
[Tags("Releases")]
public class ReleasesController : ControllerBase
{
    private readonly DistributeReleaseService _distributeReleaseService;
    private readonly DistributeToAllDspsService _distributeToAllDspsService;

    /// <summary>Called by the DI container, which supplies both use cases.</summary>
    /// <param name="distributeReleaseService">Sends a release to one DSP.</param>
    /// <param name="distributeToAllDspsService">Sends a release to every DSP.</param>
    public ReleasesController(
        DistributeReleaseService distributeReleaseService,
        DistributeToAllDspsService distributeToAllDspsService)
    {
        _distributeReleaseService = distributeReleaseService;
        _distributeToAllDspsService = distributeToAllDspsService;
    }

    /// <summary>Distribute a release to one DSP, chosen in the URL.</summary>
    /// <remarks>
    /// The DSP is picked **at runtime** from the URL: the container returns the distributor
    /// registered under that key (Spotify, Anghami or AppleMusic).
    ///
    /// Try release **1** or **2** (they exist) and **99** (it doesn't).
    /// </remarks>
    /// <param name="releaseId">Unique number of the release, e.g. 1.</param>
    /// <param name="dsp">Which DSP to deliver to: Spotify, Anghami or AppleMusic.</param>
    /// <response code="200">The release was delivered.</response>
    /// <response code="400">The DSP name is not one we support.</response>
    /// <response code="404">No release has this id.</response>
    [HttpPost("{releaseId}/distribute/{dsp}")]
    [ProducesResponseType(typeof(DistributionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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
    /// </remarks>
    /// <param name="releaseId">Unique number of the release, e.g. 1.</param>
    /// <response code="200">One result per DSP.</response>
    /// <response code="404">No release has this id.</response>
    [HttpPost("{releaseId}/distribute-all")]
    [ProducesResponseType(typeof(List<DistributionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public IActionResult DistributeToAll(int releaseId)
    {
        var results = _distributeToAllDspsService.DistributeToAll(releaseId);
        if (results == null)
            return NotFound($"Release {releaseId} not found");

        return Ok(results);
    }
}
