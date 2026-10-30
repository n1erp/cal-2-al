using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Method)]
public sealed class NavFunctionVisibilityAttribute : Attribute
{
	public FunctionVisibility FunctionVisibility { get; }

	public NavFunctionVisibilityAttribute(FunctionVisibility functionVisibility)
	{
		FunctionVisibility = functionVisibility;
	}
}
