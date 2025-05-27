using System.ComponentModel.DataAnnotations;

namespace Common.Attributes
{
    public class FutureDateAttribute : ValidationAttribute
    {
        /// <summary>
        /// Minimum time advance for the date to be considered valid.
        /// </summary>
        public int MinimumDaysAdvance { get; set; } = 0;

        /// <summary>
        /// Minimum time advance for the date to be considered valid.
        /// </summary>
        public int MinimumMonthsAdvance { get; set; } = 0;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime dateValue)
            {
                return new ValidationResult("Invalid date format.");
            }

            var minimumDate = DateTime.Now.AddMonths(MinimumMonthsAdvance).AddDays(MinimumDaysAdvance);
            if (dateValue < minimumDate)
            {
                return new ValidationResult(ErrorMessage ?? $"Date must be at least {MinimumDaysAdvance} days in the future.");
            }

            return ValidationResult.Success;
        }
    }
}
