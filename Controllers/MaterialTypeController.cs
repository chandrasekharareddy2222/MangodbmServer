using FieldMetadataAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FieldMetadataAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MaterialTypeController : ControllerBase
    {
        private readonly IMaterialTypeService _service;

        public MaterialTypeController(IMaterialTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterialTypes()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
    }
}