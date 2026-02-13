using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using FluentValidation;

namespace DidWeFeedTheCatToday.Validation.Feedings
{
    public class PostFeedingDTOValidator : AbstractValidator<PostFeedingDTO>
    {
        public PostFeedingDTOValidator() 
        {
            RuleFor(x => x.PetId)
                .GreaterThan(0);

            RuleFor(x => x.FeedingTime)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Cannot post feeding in the future.")
                .When(x => x.FeedingTime.HasValue);
        }
    }
}
