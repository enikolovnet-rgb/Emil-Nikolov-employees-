using EmployeePairs.Api.Contracts;
using EmployeePairs.Application.Exceptions;
using EmployeePairs.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePairs.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeePairService _employeePairService;

        public EmployeesController(IEmployeePairService employeePairService)
        {
            _employeePairService = employeePairService;
        }

        [HttpPost("longest-working-pair")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType<EmployeePairResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculateEmployeePairs(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                throw new InvalidInputFileException("The uploaded file is empty.");
            }

            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidInputFileException("Only .csv files are supported.");
            }

            await using var stream = file.OpenReadStream();
            var pair = await _employeePairService.FindLongestWorkingPairAsync(stream, cancellationToken);

            return pair is null
                ? NoContent()
                : Ok(EmployeePairResponse.FromDomain(pair));
        }
    }
}
