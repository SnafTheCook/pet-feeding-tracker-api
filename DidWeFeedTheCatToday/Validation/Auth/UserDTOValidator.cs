using DidWeFeedTheCatToday.DTOs.Auth;
using FluentValidation;

namespace DidWeFeedTheCatToday.Validation.Auth
{
    public class UserDTOValidator : AbstractValidator<UserDTO>
    {
        public UserDTOValidator() 
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username cannot be empty.")
                .MinimumLength(4).WithMessage("Username must contain at least 4 characters.")
                .MaximumLength(36).WithMessage("Username cannot be longer than 36 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty.")
                .MinimumLength(8).WithMessage("Password must contain at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a number.")
                .Matches("[!@#$%^&*()_+=\\[{\\]};:<>|./?,-]").WithMessage("Password must contain a special character.");
        }
    }
}
