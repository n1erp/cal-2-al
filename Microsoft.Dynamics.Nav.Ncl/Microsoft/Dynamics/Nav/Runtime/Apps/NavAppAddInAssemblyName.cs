using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppAddInAssemblyName
{
	public string Name { get; }

	public string PublicKeyToken { get; }

	public Version Version { get; }

	private NavAppAddInAssemblyName(string name, string publicKeyToken, Version version)
	{
		Name = name;
		PublicKeyToken = (string.IsNullOrWhiteSpace(publicKeyToken) ? "null" : publicKeyToken);
		if (version != null)
		{
			int major = version.Major;
			int minor = version.Minor;
			Version = new Version(major, minor, (version.Build >= 0 && version.Build != 65535) ? version.Build : 0, (version.Revision >= 0 && version.Revision != 65535) ? version.Revision : 0);
		}
	}

	public static NavAppAddInAssemblyName CreateFromString(string assemblyName, AssemblyNameSeparatorToken separator = AssemblyNameSeparatorToken.Comma)
	{
		if (separator == AssemblyNameSeparatorToken.Semicolon)
		{
			assemblyName = assemblyName.Replace(';', ',');
		}
		string text = "";
		string[] source = assemblyName.Split(',');
		foreach (string item in source.Select((string t) => t.Trim()))
		{
			text += item;
			if (item.EndsWith("PublicKeyToken=", StringComparison.OrdinalIgnoreCase))
			{
				text += "null";
			}
			text += ",";
		}
		text = text.TrimEnd(',');
		AssemblyName assemblyName2 = new AssemblyName(text);
		string name = assemblyName2.Name;
		string publicKeyToken = ConvertPublicKeyTokenToString(assemblyName2.GetPublicKeyToken());
		Version version = assemblyName2.Version;
		return new NavAppAddInAssemblyName(name, publicKeyToken, version);
	}

	public static NavAppAddInAssemblyName CreateFromComponents(string name, string publicKeyToken, Version version)
	{
		return new NavAppAddInAssemblyName(name, publicKeyToken, version);
	}

	protected bool Equals(NavAppAddInAssemblyName other)
	{
		if (string.Equals(Name, other.Name) && string.Equals(PublicKeyToken, other.PublicKeyToken))
		{
			return object.Equals(Version, other.Version);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((NavAppAddInAssemblyName)obj);
	}

	public override int GetHashCode()
	{
		int num = ((Name != null) ? Name.GetHashCode() : 0);
		num = (num * 397) ^ ((PublicKeyToken != null) ? PublicKeyToken.GetHashCode() : 0);
		return (num * 397) ^ ((Version != null) ? Version.GetHashCode() : 0);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, PublicKeyToken={2}", Name, Version, PublicKeyToken);
	}

	private static string ConvertPublicKeyTokenToString(byte[] value)
	{
		if (value == null || value.Length == 0)
		{
			return "null";
		}
		return BitConverter.ToString(value).Replace("-", string.Empty).ToLowerInvariant();
	}
}
