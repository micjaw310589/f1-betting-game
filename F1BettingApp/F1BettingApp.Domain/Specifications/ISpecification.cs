using System.Linq.Expressions;

namespace F1BettingApp.Domain.Specifications
{
    /// <summary>
    /// Base interface for the specification pattern
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Gets the expression that defines the specification
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Gets the include expressions for eager loading
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Gets the include string expressions for eager loading
        /// </summary>
        List<string> IncludeStrings { get; }

        /// <summary>
        /// Adds an include expression for eager loading
        /// </summary>
        /// <param name="includeExpression">The include expression</param>
        void AddInclude(Expression<Func<T, object>> includeExpression);

        /// <summary>
        /// Adds an include string for eager loading
        /// </summary>
        /// <param name="includeString">The include string</param>
        void AddInclude(string includeString);
    }
}