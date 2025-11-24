using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.BusinessRules
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla specyfikacji implementująca wspólną logikę.
    /// </summary>
    /// <typeparam name="T">Typ obiektu do walidacji</typeparam>
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        public abstract string SpecificationName { get; }

        public virtual bool IsSatisfiedBy(T item)
        {
            return !GetValidationErrors(item).Any();
        }

        public abstract IEnumerable<ValidationResult> GetValidationErrors(T item);
    }

    /// <summary>
    /// Specyfikacja kompozytowa umożliwiająca łączenie wielu specyfikacji.
    /// </summary>
    /// <typeparam name="T">Typ obiektu do walidacji</typeparam>
    public class CompositeSpecification<T> : BaseSpecification<T>
    {
        private readonly List<ISpecification<T>> _specifications;

        public CompositeSpecification(params ISpecification<T>[] specifications)
        {
            _specifications = specifications?.ToList() ?? new List<ISpecification<T>>();
        }

        public override string SpecificationName => 
            $"Composite({string.Join(", ", _specifications.Select(s => s.SpecificationName))})";

        public override IEnumerable<ValidationResult> GetValidationErrors(T item)
        {
            var errors = new List<ValidationResult>();
            
            foreach (var specification in _specifications)
            {
                errors.AddRange(specification.GetValidationErrors(item));
            }
            
            return errors;
        }

        /// <summary>
        /// Dodaje specyfikację do kompozytu.
        /// </summary>
        public CompositeSpecification<T> And(ISpecification<T> specification)
        {
            _specifications.Add(specification);
            return this;
        }
    }
}