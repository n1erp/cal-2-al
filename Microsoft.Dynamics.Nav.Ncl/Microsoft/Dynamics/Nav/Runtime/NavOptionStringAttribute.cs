using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class NavOptionStringAttribute : Attribute
{
	private string optionString = "";

	public string OptionString
	{
		get
		{
			return optionString;
		}
		set
		{
			optionString = value;
		}
	}
}
