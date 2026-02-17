using FieldMetadataAPI.DTOs;
using FieldMetadataAPI.Models;
using FieldMetadataAPI.Repositories;

namespace FieldMetadataAPI.Services
{
    /// <summary>
    /// Service interface for Material business logic
    /// </summary>
    public interface IMaterialService
    {
        Task<string> GetNextMatnrAsync();
        Task<MaterialDto> SubmitMaterialAsync(MaterialSubmissionDto submissionDto);
        Task<MaterialDto?> GetMaterialByIdAsync(string matnr);
        Task<PagedResponse<MaterialDto>> GetMaterialsAsync(MaterialQueryDto query);
    }

    /// <summary>
    /// Service implementation for Material business logic
    /// </summary>
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IFieldMetadataRepository _fieldMetadataRepository;
        private readonly ILogger<MaterialService> _logger;

        public MaterialService(
            IMaterialRepository materialRepository,
            IFieldMetadataRepository fieldMetadataRepository,
            ILogger<MaterialService> logger)
        {
            _materialRepository = materialRepository ?? throw new ArgumentNullException(nameof(materialRepository));
            _fieldMetadataRepository = fieldMetadataRepository ?? throw new ArgumentNullException(nameof(fieldMetadataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Pre-generate next Material Number for form display
        /// </summary>
        /// <returns>Next available MATNR</returns>
        public async Task<string> GetNextMatnrAsync()
        {
            _logger.LogInformation("Generating next MATNR for new material form");
            var matnr = await _materialRepository.GenerateMatnrAsync();
            _logger.LogInformation("Generated MATNR: {MATNR}", matnr);
            return matnr;
        }

        /// <summary>
        /// Submit material - creates new or updates existing based on MATNR existence
        /// </summary>
        public async Task<MaterialDto> SubmitMaterialAsync(MaterialSubmissionDto submissionDto)
        {
            // If MATNR is provided, check if it exists
            if (!string.IsNullOrWhiteSpace(submissionDto.MATNR))
            {
                var exists = await _materialRepository.MaterialExistsAsync(submissionDto.MATNR);
                
                if (exists)
                {
                    _logger.LogInformation("Updating existing material {MATNR}", submissionDto.MATNR);
                    return await UpdateMaterialAsync(submissionDto);
                }
                else
                {
                    _logger.LogInformation("Creating new material with provided MATNR {MATNR}", submissionDto.MATNR);
                    return await CreateMaterialAsync(submissionDto, submissionDto.MATNR);
                }
            }
            else
            {
                _logger.LogInformation("Creating new material with auto-generated MATNR");
                return await CreateMaterialAsync(submissionDto, null);
            }
        }

        private async Task<MaterialDto> CreateMaterialAsync(MaterialSubmissionDto submissionDto, string? providedMatnr)
        {
            // Create Material Master
            var materialMaster = new MaterialMaster
            {
                MATNR = providedMatnr ?? "", // Use provided MATNR or empty for auto-generation
                MTART = submissionDto.MTART,
                MEINS = submissionDto.MEINS,
                MBRSH = submissionDto.MBRSH,
                MATKL = submissionDto.MATKL,
                ERSDA = DateTime.Now,
                ERNAM = submissionDto.SubmittedBy ?? "SYSTEM",
                LVORM = " ",
                Status = "ACTIVE",
                CreatedDate = DateTime.Now
            };

            // Create Material Attributes
            var attributes = await BuildAttributesAsync(materialMaster.MATNR, submissionDto.Attributes, submissionDto.SubmittedBy);

            // Save to database (repository will auto-generate MATNR if empty)
            var matnr = await _materialRepository.CreateMaterialAsync(materialMaster, attributes);

            // Return the created material
            return await GetMaterialByIdAsync(matnr) 
                ?? throw new InvalidOperationException("Failed to retrieve created material");
        }

        private async Task<MaterialDto> UpdateMaterialAsync(MaterialSubmissionDto submissionDto)
        {
            var matnr = submissionDto.MATNR!;

            // Check if material exists
            var exists = await _materialRepository.MaterialExistsAsync(matnr);
            if (!exists)
            {
                throw new KeyNotFoundException($"Material {matnr} not found");
            }

            // Get existing material
            var existingMaterial = await _materialRepository.GetMaterialByIdAsync(matnr);
            if (existingMaterial == null)
            {
                throw new KeyNotFoundException($"Material {matnr} not found");
            }

            // Update Material Master (only update provided fields)
            var materialMaster = new MaterialMaster
            {
                MATNR = matnr,
                MTART = submissionDto.MTART ?? existingMaterial.MTART,
                MEINS = submissionDto.MEINS ?? existingMaterial.MEINS,
                MBRSH = submissionDto.MBRSH ?? existingMaterial.MBRSH,
                MATKL = submissionDto.MATKL ?? existingMaterial.MATKL,
                ERSDA = existingMaterial.ERSDA,
                ERNAM = existingMaterial.ERNAM,
                LAEDA = DateTime.Now,
                AENAM = submissionDto.SubmittedBy ?? "SYSTEM",
                LVORM = existingMaterial.LVORM,
                Status = existingMaterial.Status,
                CreatedDate = existingMaterial.CreatedDate,
                ModifiedDate = DateTime.Now
            };

            // Update Material Attributes
            var attributes = await BuildAttributesAsync(matnr, submissionDto.Attributes, submissionDto.SubmittedBy);

            // Save to database
            var success = await _materialRepository.UpdateMaterialAsync(matnr, materialMaster, attributes, submissionDto.SubmittedBy);
            
            if (!success)
            {
                throw new InvalidOperationException($"Failed to update material {matnr}");
            }

            // Return the updated material
            return await GetMaterialByIdAsync(matnr) 
                ?? throw new InvalidOperationException("Failed to retrieve updated material");
        }

        private async Task<List<MaterialAttribute>> BuildAttributesAsync(string matnr, Dictionary<string, object>? attributes, string? modifiedBy)
        {
            var materialAttributes = new List<MaterialAttribute>();

            if (attributes == null || !attributes.Any())
            {
                return materialAttributes;
            }

            // Get field metadata to determine data types
            var allFieldMetadata = await _fieldMetadataRepository.GetAllAsync();
            var fieldMetadataDict = allFieldMetadata.ToDictionary(f => f.FieldName, f => f);

            foreach (var attr in attributes)
            {
                var fieldName = attr.Key;
                var fieldValue = attr.Value?.ToString();

                // Get field metadata if exists
                fieldMetadataDict.TryGetValue(fieldName, out var fieldMetadata);

                var materialAttribute = new MaterialAttribute
                {
                    MATNR = matnr,
                    FieldName = fieldName,
                    FieldValue = fieldValue,
                    DataType = fieldMetadata?.DataType,
                    FieldLength = fieldMetadata?.FieldLength,
                    Decimals = fieldMetadata?.Decimals,
                    LastModified = DateTime.Now,
                    ModifiedBy = modifiedBy ?? "SYSTEM"
                };

                materialAttributes.Add(materialAttribute);
            }

            return materialAttributes;
        }

        public async Task<MaterialDto?> GetMaterialByIdAsync(string matnr)
        {
            var material = await _materialRepository.GetMaterialByIdAsync(matnr);
            if (material == null)
            {
                return null;
            }

            var attributes = await _materialRepository.GetMaterialAttributesAsync(matnr);

            var dto = new MaterialDto
            {
                MATNR = material.MATNR,
                MTART = material.MTART,
                MEINS = material.MEINS,
                MBRSH = material.MBRSH,
                MATKL = material.MATKL,
                ERSDA = material.ERSDA,
                ERNAM = material.ERNAM,
                LAEDA = material.LAEDA,
                AENAM = material.AENAM,
                LVORM = material.LVORM,
                Status = material.Status,
                CreatedDate = material.CreatedDate,
                ModifiedDate = material.ModifiedDate,
                Attributes = attributes.ToDictionary(a => a.FieldName, a => (object)(a.FieldValue ?? ""))
            };

            return dto;
        }

        public async Task<PagedResponse<MaterialDto>> GetMaterialsAsync(MaterialQueryDto query)
        {
            _logger.LogInformation("Getting materials with filters");

            var (materials, totalCount) = await _materialRepository.GetMaterialsAsync(
                query.MATNR,
                query.MTART,
                query.MATKL,
                query.Status,
                query.PageNumber,
                query.PageSize
            );

            var dtos = new List<MaterialDto>();
            
            foreach (var material in materials)
            {
                var attributes = await _materialRepository.GetMaterialAttributesAsync(material.MATNR);
                
                var dto = new MaterialDto
                {
                    MATNR = material.MATNR,
                    MTART = material.MTART,
                    MEINS = material.MEINS,
                    MBRSH = material.MBRSH,
                    MATKL = material.MATKL,
                    ERSDA = material.ERSDA,
                    ERNAM = material.ERNAM,
                    LAEDA = material.LAEDA,
                    AENAM = material.AENAM,
                    LVORM = material.LVORM,
                    Status = material.Status,
                    CreatedDate = material.CreatedDate,
                    ModifiedDate = material.ModifiedDate,
                    Attributes = attributes.ToDictionary(a => a.FieldName, a => (object)(a.FieldValue ?? ""))
                };

                dtos.Add(dto);
            }

            return new PagedResponse<MaterialDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }
    }
}
