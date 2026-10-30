using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class NavExceptionSerializationBinder : SerializationBinder
{
	private readonly Type expectedType;

	public NavExceptionSerializationBinder(Type type)
	{
		expectedType = type ?? typeof(Exception);
	}

	public override Type BindToType(string assemblyName, string typeName)
	{
		NavDiagnostics.GetMostSpecificInstance.SendTraceTag("0000C2G", Category.Runtime, Verbosity.Error, "Exception serialization binder on assembly: '{0}', type: '{1}'", DiagnosticParameter.SystemMetadata(assemblyName), DiagnosticParameter.SystemMetadata(typeName));
		if (((expectedType == null) ? new NavSerializationBinder() : new NavSerializationBinder(expectedType)).TryBindToType(assemblyName, typeName, out var bindToType))
		{
			return bindToType;
		}
		bool flag = false;
		if (!flag)
		{
			bindToType = Type.GetType(typeName + ", " + assemblyName);
			Type baseType = bindToType.BaseType;
			while (baseType != null && baseType != expectedType)
			{
				baseType = baseType.BaseType;
			}
			if (baseType != null)
			{
				byte[] publicKeyToken = expectedType.Assembly.GetName().GetPublicKeyToken();
				flag = Enumerable.SequenceEqual(second: bindToType.Assembly.GetName().GetPublicKeyToken(), first: publicKeyToken?) ?? false;
			}
		}
		if (!flag)
		{
			throw NavDiagnostics.GetMostSpecificInstance.SendExceptionTag("0000C2F", Category.Runtime, NavSerializationException.Create(assemblyName, typeName));
		}
		return bindToType;
	}
}
