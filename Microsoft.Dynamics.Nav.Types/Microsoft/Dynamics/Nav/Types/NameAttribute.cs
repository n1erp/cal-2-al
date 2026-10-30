using System;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class NameAttribute : Attribute
{
	public string Name { get; private set; }

	public NameAttribute(string name)
	{
		Name = name;
	}
}
