using DigitalBank.Application.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalBank.Application.Validations
{
    public class TransferRequestDtoValidator : AbstractValidator<TransferRequestDto>
    {
        public TransferRequestDtoValidator()
        {
            RuleFor(x => x.ReceiverWalletId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(1_000_000); // limit (istəsən config edərik)

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }
}
