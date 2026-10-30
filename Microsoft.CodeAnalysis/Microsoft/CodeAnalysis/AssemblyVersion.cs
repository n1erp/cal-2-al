using System;

namespace Microsoft.CodeAnalysis;

internal struct AssemblyVersion(ushort major, ushort minor, ushort build, ushort revision) : IEquatable<AssemblyVersion>, IComparable<AssemblyVersion>
{
	private readonly ushort _major = major;

	private readonly ushort _minor = minor;

	private readonly ushort _build = build;

	private readonly ushort _revision = revision;

	public int Major => _major;

	public int Minor => _minor;

	public int Build => _build;

	public int Revision => _revision;

	private ulong ToInteger()
	{
		return ((ulong)_major << 48) | ((ulong)_minor << 32) | ((ulong)_build << 16) | _revision;
	}

	public int CompareTo(AssemblyVersion other)
	{
		ulong num = ToInteger();
		ulong num2 = other.ToInteger();
		if (num != num2)
		{
			if (num >= num2)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(AssemblyVersion other)
	{
		return ToInteger() == other.ToInteger();
	}

	public override bool Equals(object obj)
	{
		if (obj is AssemblyVersion)
		{
			return Equals((AssemblyVersion)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((_major & 0xF) << 28) | ((_minor & 0xFF) << 20) | ((_build & 0xFF) << 12) | (_revision & 0xFFF);
	}

	public static bool operator ==(AssemblyVersion left, AssemblyVersion right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AssemblyVersion left, AssemblyVersion right)
	{
		return !left.Equals(right);
	}

	public static bool operator <(AssemblyVersion left, AssemblyVersion right)
	{
		return left.ToInteger() < right.ToInteger();
	}

	public static bool operator <=(AssemblyVersion left, AssemblyVersion right)
	{
		return left.ToInteger() <= right.ToInteger();
	}

	public static bool operator >(AssemblyVersion left, AssemblyVersion right)
	{
		return left.ToInteger() > right.ToInteger();
	}

	public static bool operator >=(AssemblyVersion left, AssemblyVersion right)
	{
		return left.ToInteger() >= right.ToInteger();
	}

	public static explicit operator AssemblyVersion(Version version)
	{
		return new AssemblyVersion((ushort)version.Major, (ushort)version.Minor, (ushort)version.Build, (ushort)version.Revision);
	}

	public static explicit operator Version(AssemblyVersion version)
	{
		return new Version(version.Major, version.Minor, version.Build, version.Revision);
	}
}
