using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMI.CQSLight.Extensions
{
	public static class DataAnnotationValidatorExtensions
	{
		public static bool IsValid(this object obj)
		{
			List<ValidationResult> validationResults;
			var result = Validate(obj, out validationResults);

			return result;
		}

		public static bool Validate(this object obj, out List<ValidationResult> results)
		{
			ValidationContext context = new ValidationContext(obj, null, null);
			results = new List<ValidationResult>();

			return Validator.TryValidateObject(obj, context, results, true);
		}
	}
}