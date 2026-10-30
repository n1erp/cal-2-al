using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavTaskNotSupportedOnDeviceException : NavMetadataException
{
	public NavTaskNotSupportedOnDeviceException()
	{
	}

	public NavTaskNotSupportedOnDeviceException(string message)
		: base(message)
	{
	}

	public static NavTaskNotSupportedOnDeviceException CreateAndLogRuntimeWarning(string message)
	{
		NavTaskNotSupportedOnDeviceException exception = new NavTaskNotSupportedOnDeviceException(message);
		return NavDiagnostics.GlobalInstance.SendExceptionTag("00001ZF", Category.Runtime, exception, Verbosity.Warning) as NavTaskNotSupportedOnDeviceException;
	}

	public NavTaskNotSupportedOnDeviceException(string message, Exception inner)
		: base(message, inner)
	{
	}

	private NavTaskNotSupportedOnDeviceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
