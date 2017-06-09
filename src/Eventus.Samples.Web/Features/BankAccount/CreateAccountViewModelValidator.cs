using FluentValidation;

namespace Eventus.Samples.Web.Features.BankAccount
{
    public class CreateAccountViewModelValidator : AbstractValidator<CreateAccountViewModel>
    {
        public CreateAccountViewModelValidator()
        {
            RuleFor(x => x.AccountName).NotEmpty()
                .Length(5, 100);
        }
    }
}