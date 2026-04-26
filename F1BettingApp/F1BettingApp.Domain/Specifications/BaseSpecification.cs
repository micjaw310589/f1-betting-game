using System.Linq.Expressions;

namespace F1BettingApp.Domain.Specifications
{
    /// <summary>
    /// Base implementation of the specification pattern
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        /// <summary>
        /// Gets the expression that defines the specification
        /// </summary>
        public Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Gets the include expressions for eager loading
        /// </summary>
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        /// <summary>
        /// Gets the include string expressions for eager loading
        /// </summary>
        public List<string> IncludeStrings { get; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSpecification{T}"/> class
        /// </summary>
        /// <param name="criteria">The criteria expression</param>
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
        }

        /// <summary>
        /// Adds an include expression for eager loading
        /// </summary>
        /// <param name="includeExpression">The include expression</param>
        public void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Adds an include string for eager loading
        /// </summary>
        /// <param name="includeString">The include string</param>
        public void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }
    }
}