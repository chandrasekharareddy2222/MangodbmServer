using Microsoft.AspNetCore.Mvc;
using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Services;
using FluentValidation;

namespace FieldMetadataAPI.Controllers
{
    /// <summary>
    /// API Controller for Material operations
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/materials")]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _service;
        private readonly ILogger<MaterialController> _logger;
        private readonly IValidator<MaterialSubmissionDto> _submissionValidator;
        private readonly IValidator<MaterialQueryDto> _queryValidator;

        public MaterialController(
            IMaterialService service,
            ILogger<MaterialController> logger,
            IValidator<MaterialSubmissionDto> submissionValidator,
            IValidator<MaterialQueryDto> queryValidator)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _submissionValidator = submissionValidator ?? throw new ArgumentNullException(nameof(submissionValidator));
            _queryValidator = queryValidator ?? throw new ArgumentNullException(nameof(queryValidator));
        }

        /// <summary>
        /// Pre-generate Material Number for new material form
        /// </summary>
        /// <returns>Next available MATNR</returns>
        /// <remarks>
        /// Call this endpoint when opening the create material form.
        /// The generated MATNR can be displayed in the form's Material Number field.
        /// </remarks>
        [HttpGet("generate-matnr")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateMatnr()
        {
            _logger.LogInformation("GET request received to generate MATNR");

            try
            {
                var matnr = await _service.GetNextMatnrAsync();
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Material Number generated successfully",
                    Data = matnr
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating MATNR");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating Material Number",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Submit material data (CREATE if MATNR is null/empty, UPDATE if MATNR is provided)
        /// </summary>
        /// <param name="submissionDto">Material submission data</param>
        /// <returns>Created or updated material with MATNR</returns>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(ApiResponse<MaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MaterialDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitMaterial([FromBody] MaterialSubmissionDto submissionDto)
        {
            var isUpdate = !string.IsNullOrWhiteSpace(submissionDto.MATNR);
            _logger.LogInformation("POST request received to {Action} material", isUpdate ? "update" : "create");

            var validationResult = await _submissionValidator.ValidateAsync(submissionDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            try
            {
                var result = await _service.SubmitMaterialAsync(submissionDto);

                if (isUpdate)
                {
                    return Ok(ApiResponse<MaterialDto>.SuccessResponse(result, $"Material {result.MATNR} updated successfully"));
                }
                else
                {
                    return CreatedAtAction(
                        nameof(GetMaterialById),
                        new { matnr = result.MATNR },
                        ApiResponse<MaterialDto>.SuccessResponse(result, $"Material {result.MATNR} created successfully")
                    );
                }
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Material not found");
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get material by MATNR
        /// </summary>
        /// <param name="matnr">Material number</param>
        /// <returns>Material details with all attributes</returns>
        [HttpGet("{matnr}")]
        [ProducesResponseType(typeof(ApiResponse<MaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMaterialById(string matnr)
        {
            _logger.LogInformation("GET request received for material: {MATNR}", matnr);

            var result = await _service.GetMaterialByIdAsync(matnr);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Material {matnr} not found"));
            }

            return Ok(ApiResponse<MaterialDto>.SuccessResponse(result, "Material retrieved successfully"));
        }

        /// <summary>
        /// Get all materials with optional filtering and pagination
        /// </summary>
        /// <param name="query">Query parameters for filtering and pagination</param>
        /// <returns>Paginated list of materials</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<MaterialDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMaterials([FromQuery] MaterialQueryDto query)
        {
            _logger.LogInformation("GET request received for all materials");

            var validationResult = await _queryValidator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            var result = await _service.GetMaterialsAsync(query);
            return Ok(ApiResponse<PagedResponse<MaterialDto>>.SuccessResponse(result, "Materials retrieved successfully"));
        }
    }
}
