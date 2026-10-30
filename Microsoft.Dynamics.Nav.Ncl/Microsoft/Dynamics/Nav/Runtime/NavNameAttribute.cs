using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class NavNameAttribute : Attribute
{
	public string Name { get; private set; }

	public NavNameAttribute(string name)
	{
		Name = name;
	}
}
