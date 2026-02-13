using DidWeFeedTheCatToday.Shared.Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DidWeFeedTheCatToday.Validation
{
    public class ValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null) 
                    continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                var validator = _serviceProvider.GetService(validatorType);

                if (validator is null) 
                    continue;

                var validationContext = new ValidationContext<object>(argument);
                var result = await ((IValidator)validator).ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(e => e.ErrorMessage);
                    context.Result = new BadRequestObjectResult(ApiResponse<object>.Fail(string.Join(" | ", errors)));
                    return;
                }
            }

            await next();
        }
    }
}
