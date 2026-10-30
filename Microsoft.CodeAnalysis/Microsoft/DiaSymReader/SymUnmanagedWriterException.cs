using System;

namespace Microsoft.DiaSymReader;

internal sealed class SymUnmanagedWriterException : Exception
{
	public string ImplementationModuleName { get; }

	public SymUnmanagedWriterException()
	{
	}

	public SymUnmanagedWriterException(string message)
		: base(message)
	{
	}

	public SymUnmanagedWriterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public SymUnmanagedWriterException(string message, Exception innerException, string implementationModuleName)
		: base(message, innerException)
	{
		ImplementationModuleName = implementationModuleName;
	}

	internal SymUnmanagedWriterException(Exception innerException, string implementationModuleName)
		: this(innerException.Message, innerException, implementationModuleName)
	{
	}
}
