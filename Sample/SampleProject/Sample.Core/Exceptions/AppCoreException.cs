using System;

namespace Sample.Core.Exceptions;

public class AppCoreException : Exception
{
	public AppCoreException(string message) : base(message)
	{
	}
}