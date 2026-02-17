using FluentValidation;
using FieldMetadataAPI.DTOs;

namespace FieldMetadataAPI.Validators
{
    /// <summary>
    /// Validator for MaterialSubmissionDto
    /// </summary>
    public class MaterialSubmissionValidator : AbstractValidator<MaterialSubmissionDto>
    {
        public MaterialSubmissionValidator()
        {
            // MATNR is always required
            RuleFor(x => x.MATNR)
                .NotEmpty().WithMessage("MATNR (Material Number) is required.")
                .MaximumLength(18).WithMessage("MATNR cannot exceed 18 characters.");

            // All other fields are optional - only validate length when provided
            RuleFor(x => x.MTART)
                .MaximumLength(4).WithMessage("MTART cannot exceed 4 characters.")
                .When(x => !string.IsNullOrEmpty(x.MTART));

            RuleFor(x => x.MEINS)
                .MaximumLength(3).WithMessage("MEINS cannot exceed 3 characters.")
                .When(x => !string.IsNullOrEmpty(x.MEINS));

            RuleFor(x => x.MBRSH)
                .MaximumLength(1).WithMessage("MBRSH cannot exceed 1 character.")
                .When(x => !string.IsNullOrEmpty(x.MBRSH));

            RuleFor(x => x.MATKL)
                .MaximumLength(9).WithMessage("MATKL cannot exceed 9 characters.")
                .When(x => !string.IsNullOrEmpty(x.MATKL));

            RuleFor(x => x.SubmittedBy)
                .MaximumLength(100).WithMessage("SubmittedBy cannot exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.SubmittedBy));
        }
    }

    /// <summary>
    /// Validator for MaterialQueryDto
    /// </summary>
    public class MaterialQueryValidator : AbstractValidator<MaterialQueryDto>
    {
        public MaterialQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");

            RuleFor(x => x.MATNR)
                .MaximumLength(18).WithMessage("MATNR cannot exceed 18 characters.")
                .When(x => !string.IsNullOrEmpty(x.MATNR));

            RuleFor(x => x.MTART)
                .MaximumLength(4).WithMessage("MTART cannot exceed 4 characters.")
                .When(x => !string.IsNullOrEmpty(x.MTART));

            RuleFor(x => x.MATKL)
                .MaximumLength(9).WithMessage("MATKL cannot exceed 9 characters.")
                .When(x => !string.IsNullOrEmpty(x.MATKL));

            RuleFor(x => x.Status)
                .Must(s => s == null || new[] { "ACTIVE", "BLOCKED", "DELETED", "PENDING" }.Contains(s.ToUpper()))
                .WithMessage("Status must be one of: ACTIVE, BLOCKED, DELETED, PENDING.")
                .When(x => !string.IsNullOrEmpty(x.Status));
        }
    }
}
