using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class NavSerializationBinder : SerializationBinder
{
	private readonly string whitelistedAssemblyName;

	private readonly string whitelistedTypeName;

	private static readonly HashSet<string> ProductAssemblies = InitAssemblies();

	private static HashSet<string> InitAssemblies()
	{
		Assembly assembly = typeof(NavSerializationBinder).Assembly;
		return new HashSet<string>(from assemblyName in assembly.GetReferencedAssemblies().AsEnumerable()
			select assemblyName.FullName) { assembly.FullName, "System.Core, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=b77a5c561934e089" };
	}

	public NavSerializationBinder()
	{
	}

	public NavSerializationBinder(Assembly whitelistedAssembly)
	{
		lock (ProductAssemblies)
		{
			if (ProductAssemblies.Add(whitelistedAssembly.FullName))
			{
				AssemblyName[] referencedAssemblies = whitelistedAssembly.GetReferencedAssemblies();
				foreach (AssemblyName assemblyName in referencedAssemblies)
				{
					ProductAssemblies.Add(assemblyName.FullName);
				}
			}
		}
	}

	public NavSerializationBinder(Type whitelistedType)
		: this(whitelistedType.Assembly)
	{
		whitelistedAssemblyName = whitelistedType.Assembly.FullName;
		whitelistedTypeName = whitelistedType.FullName;
	}

	public override Type BindToType(string assemblyName, string typeName)
	{
		if (!TryBindToType(assemblyName, typeName, out var bindToType))
		{
			throw NavDiagnostics.GetMostSpecificInstance.SendExceptionTag("0000230", Category.Runtime, NavSerializationException.Create(assemblyName, typeName));
		}
		return bindToType;
	}

	internal bool TryBindToType(string assemblyName, string typeName, out Type bindToType)
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(whitelistedTypeName) && string.Equals(typeName, whitelistedTypeName, StringComparison.Ordinal) && string.Equals(assemblyName, whitelistedAssemblyName, StringComparison.Ordinal))
		{
			flag = true;
		}
		if (!flag)
		{
			flag = ProductAssemblies.Contains(assemblyName);
		}
		bindToType = (flag ? Type.GetType(typeName + ", " + assemblyName) : null);
		return flag;
	}
}
