using System.Linq.Expressions;

namespace Server.API.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T), "c");
            var body = Expression.AndAlso(
                Expression.Invoke(expr1, parameter),
                Expression.Invoke(expr2, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}
