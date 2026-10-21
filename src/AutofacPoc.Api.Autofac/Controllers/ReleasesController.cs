using AutofacPoc.Application.Distribution;
using Microsoft.AspNetCore.Mvc;

namespace AutofacPoc.Api.Autofac.Controllers;

[ApiController]
[Route("releases")]
public class ReleasesController : ControllerBase
{
    private readonly DistributeReleaseService _distributeReleaseService;

    // The container sees this constructor and passes the service in.
    public ReleasesController(DistributeReleaseService distributeReleaseService)
    {
        _distributeReleaseService = distributeReleaseService;
    }

    // POST /releases/1/distribute
    [HttpPost("{releaseId}/distribute")]
    public IActionResult Distribute(int releaseId)
    {
        var result = _distributeReleaseService.Distribute(releaseId);
        if (result == null)
            return NotFound($"Release {releaseId} not found");

        return Ok(result);
    }
}
