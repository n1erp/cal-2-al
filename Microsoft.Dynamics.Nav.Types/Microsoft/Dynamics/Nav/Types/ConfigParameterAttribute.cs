using System;

namespace Microsoft.Dynamics.Nav.Types;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigParameterAttribute : Attribute
{
	public bool CmdletParameter { get; set; }

	public bool Required { get; set; }

	public int Position { get; set; }

	public string[] AliasNames { get; set; }

	public ConfigParameterAttribute()
	{
		CmdletParameter = true;
		Position = int.MinValue;
	}
}
