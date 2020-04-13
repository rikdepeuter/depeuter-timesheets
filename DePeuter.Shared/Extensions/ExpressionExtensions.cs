using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

public static class ExpressionExtensions
{
    //public static string GetMember<T>(this Expression<Func<T, string>> expression)
    //{
    //    var memberExpression = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;
    //    return memberExpression.Member.Name;
    //}

    public static string GetMember<T, TMember>(this Expression<Func<T, TMember>> expression)
    {
        var memberExpression = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;
        return memberExpression.Member.Name;
    }

    public static IEnumerable<T> Where<T>(this IEnumerable<T> collection, IEnumerable<Func<T, bool>> predicates)
    {
        var q = collection;

        if (predicates != null)
        {
            foreach (var predicate in predicates)
            {
                q = q.Where(predicate);
            }
        }

        return q;
    }
}