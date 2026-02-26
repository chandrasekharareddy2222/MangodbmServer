using FluentValidation;
using FieldMetadataAPI.DTOs;

namespace FieldMetadataAPI.Validators
{
    /// <summary>
    /// Validator for CreateFieldMetadataDto
    /// </summary>
    public class CreateFieldMetadataValidator : AbstractValidator<CreateFieldMetadataDto>
    {
        public CreateFieldMetadataValidator()
        {
            RuleFor(x => x.FieldName)
                .NotEmpty().WithMessage("FieldName is required.")
                .MaximumLength(100).WithMessage("FieldName cannot exceed 100 characters.");

            RuleFor(x => x.DataElement)
                .MaximumLength(100).WithMessage("DataElement cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.DataElement));

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.KeyField)
                .MaximumLength(1).WithMessage("KeyField must be a single character.")
                .When(x => !string.IsNullOrEmpty(x.KeyField));

            RuleFor(x => x.CheckTable)
                .MaximumLength(100).WithMessage("CheckTable cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.CheckTable));

            RuleFor(x => x.DataType)
                .MaximumLength(50).WithMessage("DataType cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.DataType));

            RuleFor(x => x.FieldLength)
                .GreaterThan(0).WithMessage("FieldLength must be greater than 0.")
                .When(x => x.FieldLength.HasValue && x.FieldLength.Value > 0);

            RuleFor(x => x.Decimals)
                .GreaterThanOrEqualTo(0).WithMessage("Decimals must be 0 or greater.")
                .When(x => x.Decimals.HasValue);

            RuleFor(x => x.HasDropdown)
                .MaximumLength(1).WithMessage("HasDropdown must be a single character.")
                .When(x => !string.IsNullOrEmpty(x.HasDropdown));

            RuleFor(x => x.UIAssignmentBlock)
                .MaximumLength(100).WithMessage("TableGroup cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.UIAssignmentBlock));
        }
    }

    /// <summary>
    /// Validator for UpdateFieldMetadataDto
    /// </summary>
    public class UpdateFieldMetadataValidator : AbstractValidator<UpdateFieldMetadataDto>
    {
        public UpdateFieldMetadataValidator()
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.CheckTable)
                .MaximumLength(100).WithMessage("CheckTable cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.CheckTable));

            RuleFor(x => x.HasDropdown)
                .MaximumLength(1).WithMessage("HasDropdown must be a single character.")
                .When(x => !string.IsNullOrEmpty(x.HasDropdown));

            RuleFor(x => x.UIAssignmentBlock)
                .MaximumLength(100).WithMessage("TableGroup cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.UIAssignmentBlock));
        }
    }

    /// <summary>
    /// Validator for FieldMetadataQueryDto
    /// </summary>
    public class FieldMetadataQueryValidator : AbstractValidator<FieldMetadataQueryDto>
    {
        public FieldMetadataQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");

            RuleFor(x => x.FieldName)
                .MaximumLength(100).WithMessage("FieldName cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.FieldName));

            RuleFor(x => x.UIAssignmentBlock)
                .MaximumLength(100).WithMessage("TableGroup cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.UIAssignmentBlock));

            RuleFor(x => x.DataType)
                .MaximumLength(50).WithMessage("DataType cannot exceed 50 characters.")
                .When(x => !string.IsNullOrEmpty(x.DataType));
        }
    }

    /// <summary>
    /// Validator for BulkUpdateMandatoryDto
    /// </summary>
    public class BulkUpdateMandatoryValidator : AbstractValidator<BulkUpdateMandatoryDto>
    {
        public BulkUpdateMandatoryValidator()
        {
            RuleFor(x => x.Updates)
                .NotEmpty().WithMessage("Updates list cannot be empty.")
                .Must(x => x != null && x.Count > 0).WithMessage("At least one update is required.");

            RuleForEach(x => x.Updates)
                .ChildRules(update =>
                {
                    update.RuleFor(u => u.FieldName)
                        .NotEmpty().WithMessage("FieldName cannot be empty.")
                        .MaximumLength(100).WithMessage("FieldName cannot exceed 100 characters.");
                });
        }
    }
}
