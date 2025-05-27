using System.ComponentModel.DataAnnotations;

namespace StockAppWeb.Models
{
    /// <summary>
    /// Validates that a date is in the future.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        private readonly int _minDays;

        /// <summary>
        /// Initializes a new instance of the <see cref="FutureDateAttribute"/> class.
        /// </summary>
        /// <param name="minDays">The minimum number of days the date must be in the future from today.</param>
        public FutureDateAttribute(int minDays = 1)
        {
            _minDays = minDays;
            ErrorMessage = $"Date must be at least {_minDays} day(s) in the future.";
        }

        /// <summary>
        /// Determines whether the specified value is a valid future date.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A validation result.</returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                DateTime minimumDate = DateTime.Today.AddDays(_minDays);
                if (date.Date < minimumDate)
                {
                    return new ValidationResult(ErrorMessage);
                }
                return ValidationResult.Success;
            }
        
            return new ValidationResult("Value is not a valid date.");
        }
    }
}
