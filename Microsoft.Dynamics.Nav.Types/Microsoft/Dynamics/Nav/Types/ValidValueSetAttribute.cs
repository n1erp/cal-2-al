using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ValidValueSetAttribute : Attribute
{
	public HashSet<string> ValidValues { get; }

	public ValidValueSetAttribute(params string[] validValues)
	{
		ValidValues = new HashSet<string>(validValues, StringComparer.OrdinalIgnoreCase);
	}
}
