using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class NameAttribute : Attribute
{
	public readonly string Name;

	public NameAttribute(string name)
	{
		Name = name;
	}
}
