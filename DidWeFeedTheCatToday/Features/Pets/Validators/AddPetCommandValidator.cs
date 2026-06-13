using FluentValidation;

namespace DidWeFeedTheCatToday.Features.Pets.Validators
{
    public class AddPetCommandValidator : AbstractValidator<AddPetCommand>
    {
        public AddPetCommandValidator()
        {
            RuleFor(x => x.PetDto.Name)
                .NotEmpty()
                .MinimumLength(5)
                .WithMessage("Name must be at least 5 characters long.");

            RuleFor(x => x.PetDto.Age)
                .InclusiveBetween(0, 100);
        }
    }
}
