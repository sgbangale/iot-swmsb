using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWMSB.COMMON
{
    public class Validation
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
    public static class Extensions
    {
        public static Validation Validate<T>(this T obj)
        {

            ValidationContext context = new ValidationContext
            (obj, null, null);
            List<ValidationResult> validationResults = new
            List<ValidationResult>();
            bool valid = Validator.TryValidateObject
            (obj, context, validationResults, true);
            if (!valid)
            {
                foreach (ValidationResult validationResult in
                validationResults)
                {

                    return new Validation { ErrorMessage = validationResult.ErrorMessage, Success = false };
                }
            }
            return new Validation { Success = true };

        }
    }
}
