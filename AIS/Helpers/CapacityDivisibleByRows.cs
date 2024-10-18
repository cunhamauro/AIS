using System.ComponentModel.DataAnnotations;

namespace AIS.Helpers
{
    public class CapacityDivisibleByRows : ValidationAttribute
    {
        private readonly string _rowsPropertyName;

        public CapacityDivisibleByRows(string rowsPropertyName)
        {
            _rowsPropertyName = rowsPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;

            var rowsProperty = model.GetType().GetProperty(_rowsPropertyName);

            if (rowsProperty == null)
            {
                return new ValidationResult($"Unknown property: {_rowsPropertyName}");
            }

            var rowsValue = rowsProperty.GetValue(model);

            if (value is int capacity && rowsValue is int rows)
            {
                if (rows > 0 && capacity % rows == 0)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Capacity must be divisible by Rows");
                }
            }

            return new ValidationResult("Capacity and Rows must be valid integers");
        }
    }
}
