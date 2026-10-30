using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class NavDotNetTypeInfoAttribute : Attribute
{
	public string TypeName { get; private set; }

	public string AssemblyName { get; private set; }

	public NavDotNetTypeInfoAttribute()
	{
	}

	public NavDotNetTypeInfoAttribute(string typeName, string assemblyName)
	{
		TypeName = typeName;
		AssemblyName = assemblyName;
	}
}
