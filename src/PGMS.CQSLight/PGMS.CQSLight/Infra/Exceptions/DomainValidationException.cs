using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PGMS.CQSLight.Infra.Exceptions
{
    [Serializable]
    public class DomainValidationException : Exception
    {
	    public List<ValidationResult> ValidationResult { get; }


	    public DomainValidationException(string message) : base(message) { }

        public DomainValidationException(string message, List<ValidationResult> validationResult) : base(message)
        {
	        ValidationResult = validationResult;
        }
    }
}