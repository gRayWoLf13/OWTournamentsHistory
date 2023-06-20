using MongoDB.Driver;
using System.Linq.Expressions;

namespace OWTournamentsHistory.DataAccess.Extensions
{
    internal static class AggregationExtensions
    {
        public static IOrderedAggregateFluent<T> SortByAny<T>(this IAggregateFluent<T> aggregate, Expression<Func<T, object>> sortKeySelector, bool sortAscending) =>
            sortAscending
            ? aggregate.SortBy(sortKeySelector)
            : aggregate.SortByDescending(sortKeySelector);
    }
}
