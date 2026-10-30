using System;
using System.Globalization;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public struct SubsystemVersion : IEquatable<SubsystemVersion>
{
	public int Major { get; }

	public int Minor { get; }

	public static SubsystemVersion None => default;

	public static SubsystemVersion Windows2000 => new SubsystemVersion(5, 0);

	public static SubsystemVersion WindowsXP => new SubsystemVersion(5, 1);

	public static SubsystemVersion WindowsVista => new SubsystemVersion(6, 0);

	public static SubsystemVersion Windows7 => new SubsystemVersion(6, 1);

	public static SubsystemVersion Windows8 => new SubsystemVersion(6, 2);

	public bool IsValid
	{
		get
		{
			if (Major >= 0 && Minor >= 0 && Major < 65536)
			{
				return Minor < 65536;
			}
			return false;
		}
	}

	private SubsystemVersion(int major, int minor)
	{
		Major = major;
		Minor = minor;
	}

	public static bool TryParse(string str, out SubsystemVersion version)
	{
		version = None;
		if (!string.IsNullOrWhiteSpace(str))
		{
			int num = str.IndexOf('.');
			string text;
			string text2;
			if (num >= 0)
			{
				if (str.Length == num + 1)
				{
					return false;
				}
				text = str.Substring(0, num);
				text2 = str.Substring(num + 1);
			}
			else
			{
				text = str;
				text2 = null;
			}
			if (text != text.Trim() || !int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var result) || result >= 65356 || result < 0)
			{
				return false;
			}
			int result2 = 0;
			if (text2 != null && (text2 != text2.Trim() || !int.TryParse(text2, NumberStyles.None, CultureInfo.InvariantCulture, out result2) || result2 >= 65356 || result2 < 0))
			{
				return false;
			}
			version = new SubsystemVersion(result, result2);
			return true;
		}
		return false;
	}

	public static SubsystemVersion Create(int major, int minor)
	{
		return new SubsystemVersion(major, minor);
	}

	internal static SubsystemVersion Default(OutputKind outputKind, Platform platform)
	{
		if (platform == Platform.Arm)
		{
			return Windows8;
		}
		switch (outputKind)
		{
		case OutputKind.ConsoleApplication:
		case OutputKind.WindowsApplication:
		case OutputKind.DynamicallyLinkedLibrary:
		case OutputKind.NetModule:
			return new SubsystemVersion(4, 0);
		case OutputKind.WindowsRuntimeMetadata:
		case OutputKind.WindowsRuntimeApplication:
			return Windows8;
		default:
			throw new ArgumentOutOfRangeException(CodeAnalysisResources.OutputKindNotSupported, "outputKind");
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is SubsystemVersion)
		{
			return Equals((SubsystemVersion)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Minor.GetHashCode(), Major.GetHashCode());
	}

	public bool Equals(SubsystemVersion other)
	{
		if (Major == other.Major)
		{
			return Minor == other.Minor;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{Major}.{Minor:00}";
	}
}
