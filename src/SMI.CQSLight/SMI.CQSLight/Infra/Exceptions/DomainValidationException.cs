using System;

namespace SMI.CQSLight.Infra.Exceptions
{
    [Serializable]
    public class DomainValidationException : Exception
    {
        public DomainValidationException(string message) : base(message) { }
    }
}