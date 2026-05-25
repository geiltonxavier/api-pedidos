using Application.DTO;
using FluentValidation;

namespace Application.Validators;

public sealed class UpdateItemDtoValidator : AbstractValidator<UpdateItemDto>
{
    public UpdateItemDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição do item é obrigatória.")
            .MaximumLength(200).WithMessage("A descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("O preço unitário deve ser maior que zero.");
    }
}
