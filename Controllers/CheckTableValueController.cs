using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FieldMetadataAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/check-table-value")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
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

        /// <summary>
        /// Creates a new Check Table Value record
        /// 
        [HttpPost]
        public async Task<IActionResult> Create(
                [FromBody] CreateCheckTableValueDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data.");

            _logger.LogInformation(
                "Creating CheckTableValue for {Table}",
                dto.CheckTableName);

            var id = await _service.CreateAsync(dto);

            return Ok(new
            {
                Message = "Record created successfully",
                CheckTableID = id
            });
        }

        /// <summary>
        /// Update Check Table Value
        /// api/CheckTableValue
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCheckTableValueDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data.");

            _logger.LogInformation(
                "Updating CheckTableValue {Id}", id);

            var updated = await _service.UpdateAsync(id, dto);

            if (!updated)
                return NotFound("Record not found.");

            return Ok(new
            {
                Message = "Record updated successfully"
            });
        }
        /// <summary>
        /// Soft Delete Check Table Value
        /// api/CheckTableValue
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation(
                "Soft deleting CheckTableValue {Id}", id);

            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound("Record not found.");

            return Ok(new
            {
                Message = "Record deleted successfully"
            });
        }

        [HttpPost("upload/{tableName}")]
        public async Task<IActionResult> UploadCsv(
      string tableName,
      IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var result = await _service.UploadCsvAsync(tableName, file);

            return Ok(new
            {
                Message = "CSV Uploaded Successfully"
            });
        }
    }
}
