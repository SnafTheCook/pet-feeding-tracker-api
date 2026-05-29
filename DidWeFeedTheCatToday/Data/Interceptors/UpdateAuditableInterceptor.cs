using DidWeFeedTheCatToday.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DidWeFeedTheCatToday.Data.Interceptors
{
    public class UpdateAuditableInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            var context = eventData.Context;

            if (context == null) 
                return base.SavingChangesAsync(eventData, result, ct);

            var entries = context.ChangeTracker.Entries<IAuditable>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = DateTime.UtcNow;

                if (entry.State == EntityState.Modified ||  entry.State == EntityState.Added)
                    entry.Entity.LastModifiedAt = DateTime.UtcNow;
            }

            return base.SavingChangesAsync(eventData, result, ct);
        }
    }
}
