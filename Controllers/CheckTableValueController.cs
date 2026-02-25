using FieldMetadataAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FieldMetadataAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckTableValueController : ControllerBase
    {
        private readonly ICheckTableValueService _service;
        private readonly ILogger<CheckTableValueController> _logger;

        public CheckTableValueController(
            ICheckTableValueService service,
            ILogger<CheckTableValueController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get Check Table Values
        /// Example:
        /// api/CheckTableValue?tableName=T134
        /// api/CheckTableValue?tableName=T134&keyValue=FERT
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string tableName)
           // [FromQuery] string? keyValue)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest("tableName is required.");

            _logger.LogInformation("Fetching values for {TableName}", tableName);

            var result = await _service.GetByTableNameAsync(tableName);

            if (!result.Any())
                return NotFound("No records found.");

            return Ok(result);
        }
    }
}
