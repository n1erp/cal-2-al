using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDotNetCreateException : NavNCLException
{
	public NavNCLDotNetCreateException()
	{
	}

	public NavNCLDotNetCreateException(string message)
		: base(message)
	{
	}

	public NavNCLDotNetCreateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDotNetCreateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLDotNetCreateException Create(CultureInfo culture, string assemblyName, Exception innerException)
	{
		return new NavNCLDotNetCreateException(string.Format(culture, Lang.DotNetLoadError, assemblyName), innerException)
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}

	public static NavNCLDotNetCreateException Create(CultureInfo culture, string assemblyName, string typeName, Exception innerException)
	{
		return new NavNCLDotNetCreateException(string.Format(culture, Lang.DotNetCreateError, assemblyName, typeName), innerException)
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}

	public static NavNCLDotNetCreateException Create(CultureInfo culture, SerializableObjectContract serializedObject, Exception innerException)
	{
		return CreateSerializedObject(culture, serializedObject.AssemblyName, serializedObject.TypeName, innerException);
	}

	public static NavNCLDotNetCreateException CreateSerializedObject(CultureInfo culture, string assemblyName, string typeName, Exception innerException)
	{
		return new NavNCLDotNetCreateException(string.Format(culture, Lang.DotNetCreateSerializeError, assemblyName, typeName), innerException)
		{
			FatalityScope = NavExceptionFatalityScope.Call
		};
	}
}
