using System;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppResourceName
{
	public string Value { get; }

	public NavAppResourceName(string rawValue)
	{
		if (rawValue == null)
		{
			throw new ArgumentNullException("rawValue");
		}
		if (string.IsNullOrWhiteSpace(rawValue))
		{
			throw new ArgumentException(Lang.ParameterNullOrEmpty, "rawValue");
		}
		Value = NormalizeName(rawValue);
	}

	public static implicit operator NavAppResourceName(string rawValue)
	{
		return new NavAppResourceName(rawValue);
	}

	private static string NormalizeName(string rawValue)
	{
		return rawValue.Trim().ToLowerInvariant();
	}
}
