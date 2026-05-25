using Application.DTO;
using Core.Enums;
using FluentValidation;

namespace Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de pedido inválido. Valores aceitos: 0 (Standard), 1 (Express), 2 (Subscription).");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("A lista de itens é obrigatória.")
            .NotEmpty().WithMessage("O pedido deve conter ao menos um item.");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateItemDtoValidator());
    }
}
