using FieldMetadataAPI.DTOs;
using FluentValidation;

namespace FieldMetadataAPI.Validators
{
    public class CreateCheckTableValueDtoValidator : AbstractValidator<CreateCheckTableValueDto>
    {
        public CreateCheckTableValueDtoValidator()
        {
            RuleFor(x => x.CheckTableName)
                .NotEmpty().WithMessage("Table name is required.")
                .MaximumLength(50);

            RuleFor(x => x.KeyValue)
                .NotEmpty().WithMessage("Key Value is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200);

            RuleFor(x => x.ValidFrom)
                .LessThan(x => x.ValidTo)
                .WithMessage("ValidFrom must be less than ValidTo.");
        }
    }
    public class UpdateCheckTableValueDtoValidator : AbstractValidator<UpdateCheckTableValueDto>
    {
        public UpdateCheckTableValueDtoValidator()
        {
            RuleFor(x => x.KeyValue)
                .NotEmpty().WithMessage("Key Value is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200);

            RuleFor(x => x.ValidFrom)
                .LessThan(x => x.ValidTo)
                .WithMessage("ValidFrom must be less than ValidTo.");
        }
    }
    public class CheckTableQueryDtoValidator : AbstractValidator<CheckTableQueryDto>
    {
        public CheckTableQueryDtoValidator()
        {
            RuleFor(x => x.TableName)
                .NotEmpty().WithMessage("tableName is required.")
                .MaximumLength(50);
        }
    }
    public class CheckTableValueImportRowDtoValidator : AbstractValidator<CheckTableValueImportRowDto>
    {
        public CheckTableValueImportRowDtoValidator()
        {
            RuleFor(x => x.KeyValue)
                .NotEmpty().WithMessage("KeyValue is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(200);

            // If AdditionalInfo required:
            RuleFor(x => x.AdditionalInfo).NotEmpty().WithMessage("AdditionalInfo is required.");
        }
    }
}