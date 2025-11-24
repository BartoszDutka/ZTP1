using System.ComponentModel.DataAnnotations;

namespace ZtpLab1.DataTransferObjects
{
    /// <summary>
    /// Standardowy model odpowiedzi API z obsługą błędów walidacji.
    /// </summary>
    /// <typeparam name="T">Typ danych zwracanych w odpowiedzi</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Dane zwracane w odpowiedzi (jeśli operacja się powiodła).
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Ogólny komunikat o wyniku operacji.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Szczegółowe błędy walidacji.
        /// </summary>
        public IEnumerable<ValidationError>? ValidationErrors { get; set; }

        /// <summary>
        /// Kod błędu HTTP.
        /// </summary>
        public int? ErrorCode { get; set; }

        /// <summary>
        /// Timestamp odpowiedzi.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, int errorCode = 400, IEnumerable<ValidationError>? validationErrors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                ValidationErrors = validationErrors
            };
        }

        public static ApiResponse<T> ValidationErrorResponse(IEnumerable<ValidationResult> validationResults)
        {
            var validationErrors = validationResults.Select(vr => new ValidationError
            {
                Field = vr.MemberNames.FirstOrDefault() ?? "Nieznane",
                Message = vr.ErrorMessage ?? "Błąd walidacji"
            });

            return new ApiResponse<T>
            {
                Success = false,
                Message = "Dane nie przeszły walidacji.",
                ErrorCode = 422, // Unprocessable Entity
                ValidationErrors = validationErrors
            };
        }

        // Metody pomocnicze dla zachowania kompatybilności
        public static ApiResponse<T> CreateSuccess(T data, string? message = null) => SuccessResponse(data, message);
        public static ApiResponse<T> CreateError(string message, int errorCode = 400) => ErrorResponse(message, errorCode);
        public static ApiResponse<T> CreateValidationError(IEnumerable<ValidationError> validationErrors) => ErrorResponse("Dane nie przeszły walidacji.", 422, validationErrors);
        public static ApiResponse<T> CreateValidationError(IEnumerable<ValidationResult> validationResults) => ValidationErrorResponse(validationResults);
    }

    /// <summary>
    /// Model szczegółowego błędu walidacji.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Nazwa pola, które nie przeszło walidacji.
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Komunikat błędu walidacji.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Uproszczona odpowiedź bez danych (dla operacji DELETE, PUT).
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Czy operacja zakończyła się sukcesem.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Ogólny komunikat o wyniku operacji.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Szczegółowe błędy walidacji.
        /// </summary>
        public IEnumerable<ValidationError>? ValidationErrors { get; set; }

        /// <summary>
        /// Kod błędu HTTP.
        /// </summary>
        public int? ErrorCode { get; set; }

        /// <summary>
        /// Timestamp odpowiedzi.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse SuccessResponse(string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponse ErrorResponse(string message, int errorCode = 400, IEnumerable<ValidationError>? validationErrors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                ValidationErrors = validationErrors
            };
        }

        // Metody pomocnicze dla zachowania kompatybilności
        public static ApiResponse CreateSuccessResponse(string? message = null) => SuccessResponse(message);
        public static ApiResponse CreateErrorResponse(string message, int errorCode = 400) => ErrorResponse(message, errorCode);
    }
}