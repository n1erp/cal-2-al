using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLMetadataCompileErrorException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataCompileErrorException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataCompileErrorException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLMetadataCompileErrorException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNCLMetadataCompileErrorException(string moduleName, string errors, Exception innerException)
		: base(CreateMessage(moduleName, errors), innerException)
	{
	}

	protected NavNCLMetadataCompileErrorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string moduleName, string errors)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.CouldNotCompileAssembly, moduleName, errors);
	}
}
