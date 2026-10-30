using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotSupportedFileDialogProviderException : NavNotSupportedException
{
	public NavNotSupportedFileDialogProviderException()
		: base(Lang.NavFileOperationsNotSupported)
	{
	}

	public NavNotSupportedFileDialogProviderException(string message)
		: base(message)
	{
	}

	public NavNotSupportedFileDialogProviderException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotSupportedFileDialogProviderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
