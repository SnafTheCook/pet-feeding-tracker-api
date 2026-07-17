using DidWeFeedTheCatToday.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Feedings.Commands
{
    public record DeleteFeedingCommand(int Id) : IRequest<bool>;

    public class DeleteFeedingHandler(AppDbContext context) : IRequestHandler<DeleteFeedingCommand, bool>
    {
        public async Task<bool> Handle(DeleteFeedingCommand request, CancellationToken ct)
        {
            var feeding = await context.Feedings.FirstOrDefaultAsync(f => f.Id == request.Id, ct);
            if (feeding == null) return false;

            context.Feedings.Remove(feeding);
            await context.SaveChangesAsync(ct);
            return true;
        }
    }
}
