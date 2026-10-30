using System;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class SwtTokenSigningKey : IEquatable<SwtTokenSigningKey>
{
	private byte[] signingKey;

	public const string SettingName = "ClientServicesTokenSigningKey";

	public string Value
	{
		get
		{
			return Convert.ToBase64String(signingKey);
		}
		set
		{
			signingKey = Convert.FromBase64String(value);
		}
	}

	public SwtTokenSigningKey(string value)
		: this(value, "ClientServicesTokenSigningKey")
	{
	}

	public SwtTokenSigningKey(string value, string settingName)
	{
		try
		{
			Value = value;
		}
		catch (FormatException)
		{
			throw new NavConfigurationException(Lang.ClientServicesTokenSigningKeyInvalid, settingName);
		}
	}

	public static SwtTokenSigningKey Parse(string value, string settingName)
	{
		return new SwtTokenSigningKey(value, settingName);
	}

	public static bool TryParse(string value, out SwtTokenSigningKey result)
	{
		result = null;
		try
		{
			result = new SwtTokenSigningKey(value, string.Empty);
		}
		catch (NavConfigurationException)
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return Value;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		SwtTokenSigningKey swtTokenSigningKey = obj as SwtTokenSigningKey;
		if (swtTokenSigningKey == null)
		{
			return false;
		}
		return Value.Equals(swtTokenSigningKey.Value);
	}

	public bool Equals(SwtTokenSigningKey other)
	{
		if (other == null)
		{
			return false;
		}
		return Value.Equals(other.Value);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(SwtTokenSigningKey first, SwtTokenSigningKey second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Value.Equals(second.Value);
	}

	public static bool operator !=(SwtTokenSigningKey first, SwtTokenSigningKey second)
	{
		return !(first == second);
	}
}
