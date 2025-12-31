using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace JobTracks.Common
{
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly DateTime _minDate;

        public DateRangeAttribute(string minimumValue)
        {
            if (!DateTime.TryParseExact(minimumValue, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _minDate))
            {
                throw new ArgumentException("Date must be in dd-MM-yyyy format");
            }
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (value is DateTime dateValue)
            {
                if (dateValue >= _minDate)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage ?? $"Date must be after {_minDate:dd-MM-yyyy}.");
                }
            }

            return new ValidationResult("Invalid date format.");
        }
    }
}
