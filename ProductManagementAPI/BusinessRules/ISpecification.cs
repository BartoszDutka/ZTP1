using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.BusinessRules
{
    /// <summary>
    /// Podstawowy interfejs dla wzorca Specification Pattern.
    /// Umożliwia sprawdzanie czy obiekt spełnia określone kryteria.
    /// </summary>
    /// <typeparam name="T">Typ obiektu do walidacji</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Sprawdza czy obiekt spełnia specyfikację.
        /// </summary>
        /// <param name="item">Obiekt do sprawdzenia</param>
        /// <returns>True jeśli obiekt spełnia specyfikację, false w przeciwnym razie</returns>
        bool IsSatisfiedBy(T item);

        /// <summary>
        /// Zwraca szczegółowe informacje o błędach walidacji.
        /// </summary>
        /// <param name="item">Obiekt do sprawdzenia</param>
        /// <returns>Lista błędów walidacji</returns>
        IEnumerable<ValidationResult> GetValidationErrors(T item);

        /// <summary>
        /// Nazwa specyfikacji dla celów diagnostycznych.
        /// </summary>
        string SpecificationName { get; }
    }
}