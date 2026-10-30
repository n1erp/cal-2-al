using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ReturnValueAttribute : Attribute
{
	private readonly string variableName;

	public string VariableName => variableName;

	public ReturnValueAttribute()
	{
	}

	public ReturnValueAttribute(string variableName)
	{
		this.variableName = variableName;
	}
}
