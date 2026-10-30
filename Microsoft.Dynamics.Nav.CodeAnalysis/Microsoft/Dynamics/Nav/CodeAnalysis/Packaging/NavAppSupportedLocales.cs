using System;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppSupportedLocales : IEquatable<NavAppSupportedLocales>
{
	public const string LocalePlaceHolder = "{0}";

	private readonly Lazy<string> StringValue;

	public string DefaultLocale { get; }

	public string[] Locales { get; }

	public bool IsEmpty
	{
		get
		{
			if (Locales != null)
			{
				return Locales.Length == 0;
			}
			return true;
		}
	}

	public NavAppSupportedLocales(string[] supportedLocales)
	{
		Locales = supportedLocales ?? Array.Empty<string>();
		if (!IsEmpty)
		{
			DefaultLocale = supportedLocales[0];
		}
		StringValue = new Lazy<string>(() => string.Join(",", Locales));
	}

	public override string ToString()
	{
		return StringValue.Value;
	}

	public override bool Equals(object obj)
	{
		NavAppSupportedLocales navAppSupportedLocales = obj as NavAppSupportedLocales;
		if (navAppSupportedLocales == null)
		{
			return false;
		}
		return Equals(navAppSupportedLocales);
	}

	public bool Equals(NavAppSupportedLocales other)
	{
		if (other == null)
		{
			return false;
		}
		if (Locales.Length == other.Locales.Length && string.Equals(DefaultLocale, other.DefaultLocale, StringComparison.OrdinalIgnoreCase))
		{
			return Locales.All((string x) => other.Locales.Contains(x, StringComparer.OrdinalIgnoreCase));
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Locales.GetHashCode(), DefaultLocale.GetHashCode());
	}

	public static bool operator ==(NavAppSupportedLocales first, NavAppSupportedLocales second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Equals(second);
	}

	public static bool operator !=(NavAppSupportedLocales first, NavAppSupportedLocales second)
	{
		return !(first == second);
	}
}
