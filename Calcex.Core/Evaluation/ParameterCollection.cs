using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Calcex.Evaluation
{
    public interface IParameterCollection
    {
        Expression this[string paramName] { get; }
        IEnumerable<ParameterExpression> GetParameters();
    }

    public class ParameterCollection<T> : IParameterCollection
    {
        private readonly Dictionary<string, ParameterExpression> parameterExpressions;

        public ParameterCollection(params string[] parameters)
        {
            parameterExpressions = parameters.ToDictionary(s => s, s => Expression.Parameter(typeof(T), s));
        }

        public Expression this[string paramName]
        {
            get
            {
                parameterExpressions.TryGetValue(paramName, out ParameterExpression expr);
                return expr;
            }
        }

        public IEnumerable<ParameterExpression> GetParameters()
            => parameterExpressions.Values;
    }

    public class IndexedParameterExpression : IParameterCollection
    {
        private readonly ParameterExpression indexedParameter = Expression.Parameter(typeof(double[]));
        private readonly string[] parameters;

        public IndexedParameterExpression(params string[] parameters)
        {
            this.parameters = parameters;
        }

        public Expression this[string paramName]
        {
            get
            {
                var index = Array.IndexOf(parameters, paramName);
                if (index < 0) return null;
                return Expression.ArrayIndex(indexedParameter, Expression.Constant(index));
            }
        }

        public IEnumerable<ParameterExpression> GetParameters()
        {
            yield return indexedParameter;
        }
    }
}
