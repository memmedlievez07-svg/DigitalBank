using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
public static class QueryableExtension
{
    public static IQueryable<T> ApplyIncludesAndTracking<T>(this IQueryable<T> query, bool tracking = true, params Expression<Func<T, object>>[] includes)
    where T : class
    {
        // 1. include-ları tətbiq edirik
        if (includes is not null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        // 2. tracking rejimini idarə edirik
        if (!tracking)
        {
            query = query.AsNoTrackingWithIdentityResolution();
        }
        return query;
    }
}




