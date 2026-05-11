using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using FluentValidation;

namespace DidWeFeedTheCatToday.Validation.Pets
{
    public class CommandPetDTOValidator : AbstractValidator<CommandPetDTO>
    {
        public CommandPetDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(5);
        }
    }
}
