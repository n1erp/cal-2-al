using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLDotNetVariableNotCreatedException : NavNCLException
{
	public NavNCLDotNetVariableNotCreatedException()
		: base(Lang.DotNetVariableNotCreated)
	{
	}

	public NavNCLDotNetVariableNotCreatedException(string message)
		: base(message)
	{
	}

	public NavNCLDotNetVariableNotCreatedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLDotNetVariableNotCreatedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLDotNetVariableNotCreatedException Create(CultureInfo culture, string methodScopeName, ObjectType objectType, string objectName, string methodName, string typeName)
	{
		string message = (string.IsNullOrEmpty(typeName) ? string.Format(culture, Lang.DotNetVariableNotCreated, methodScopeName, objectType, objectName, methodName) : string.Format(culture, Lang.DotNetVariableNotCreated2, methodScopeName, objectType, objectName, methodName, typeName));
		return new NavNCLDotNetVariableNotCreatedException(message);
	}

	public static NavNCLDotNetVariableNotCreatedException Create(CultureInfo culture, string methodScopeName, ObjectType objectType, string objectName, string typeName)
	{
		string message = (string.IsNullOrEmpty(typeName) ? string.Format(culture, Lang.DotNetVariableNotCreated4, methodScopeName, objectType, objectName) : string.Format(culture, Lang.DotNetVariableNotCreated3, methodScopeName, objectType, objectName, typeName));
		return new NavNCLDotNetVariableNotCreatedException(message);
	}
}
