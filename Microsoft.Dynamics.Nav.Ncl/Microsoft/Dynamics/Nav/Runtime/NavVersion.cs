using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavVersion : NavValue, IEquatable<string>, IEquatable<Version>, IEquatable<NavVersion>
{
	private static readonly Version MinVersion = new Version(0, 0, 0, 0);

	private static readonly Version MaxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

	private static readonly NavVersion DefaultValue = new NavVersion(0, 0, 0, 0);

	private readonly Version value;

	public int ALMajor => value.Major;

	public int ALMinor => value.Minor;

	public int ALBuild => value.Build;

	public int ALRevision => value.Revision;

	public override NavNclType NclType => NavNclType.NavVersion;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => value.Equals(DefaultValue.value);

	public Version Value => value;

	public override object ValueAsObject => Value;

	public static NavVersion Default => DefaultValue;

	public static NavVersion MinValue { get; } = new NavVersion(MinVersion);

	public static NavVersion MaxValue { get; } = new NavVersion(MaxVersion);

	public static NavVersion ALCreate(string value)
	{
		return new NavVersion(value);
	}

	public static NavVersion ALCreate(int major, int minor)
	{
		return new NavVersion(major, minor);
	}

	public static NavVersion ALCreate(int major, int minor, int build)
	{
		return new NavVersion(major, minor, build);
	}

	public static NavVersion ALCreate(int major, int minor, int build, int revision)
	{
		return new NavVersion(major, minor, build, revision);
	}

	public void ALAssign(NavVersion other)
	{
		Clone(other);
	}

	public void ALAssign(NavValue other)
	{
		NavVersion navVersion = other as NavVersion;
		if (navVersion != null)
		{
			ALAssign(navVersion);
			return;
		}
		throw new NavNCLArgumentException(other, "ALAssign", 1);
	}

	public NavVersion(int major, int minor)
		: this(major, minor, 0)
	{
	}

	public NavVersion(int major, int minor, int build)
		: this(major, minor, build, 0)
	{
	}

	public NavVersion(int major, int minor, int build, int revision)
	{
		value = CreateWithMinSubparts(major, minor, build, revision);
	}

	public NavVersion(string input)
	{
		if (!Version.TryParse(input, out var result))
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.NavVersionParseError, input));
		}
		value = CreateWithMinSubparts(result.Major, result.Minor, result.Build, result.Revision);
	}

	public NavVersion(Version version)
	{
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		value = CreateWithMinSubparts(version.Major, version.Minor, version.Build, version.Revision);
	}

	private static Version CreateWithMinSubparts(int major, int minor, int build, int revision)
	{
		return new Version(Math.Max(major, MinVersion.Major), Math.Max(minor, MinVersion.Minor), Math.Max(build, MinVersion.Build), Math.Max(revision, MinVersion.Revision));
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public NavVersion Clone()
	{
		return new NavVersion(value);
	}

	public static NavVersion Clone(NavVersion other)
	{
		return new NavVersion(other?.value);
	}

	public static NavVersion CreateFromObject(object value)
	{
		if (value == null)
		{
			return DefaultValue;
		}
		if (value is NavVersion)
		{
			return new NavVersion(((NavVersion)value).value);
		}
		if (value is string && Version.TryParse(value as string, out var result))
		{
			return new NavVersion(result);
		}
		Version version = value as Version;
		if (version != null)
		{
			return new NavVersion(version);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavVersion), value);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj == this)
		{
			return true;
		}
		if (obj is NavValue other)
		{
			return Equals(other);
		}
		if (obj is Version)
		{
			return Value == (Version)obj;
		}
		if (obj is string)
		{
			return Value == new NavVersion((string)obj).Value;
		}
		return ThrowUnableToCompareException(obj);
	}

	public override bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		switch (other.NclType)
		{
		case NavNclType.NavText:
		case NavNclType.NavCode:
			return Value == new NavVersion(other.ToString(invariant: true)).Value;
		case NavNclType.NavVersion:
			return Value == (other as NavVersion)?.Value;
		default:
			return ThrowUnableToCompareException(other);
		}
	}

	public bool Equals(NavVersion other)
	{
		return Value == other?.Value;
	}

	public bool Equals(Version other)
	{
		if ((object)other == null)
		{
			return false;
		}
		return Value == CreateWithMinSubparts(other.Major, other.Minor, other.Build, other.Revision);
	}

	public bool Equals(string other)
	{
		return Value == new NavVersion(other).Value;
	}

	public static bool operator ==(NavVersion left, NavVersion right)
	{
		if ((object)left == right)
		{
			return true;
		}
		NavVersion navVersion = left ?? DefaultValue;
		NavVersion navVersion2 = right ?? DefaultValue;
		return navVersion.Value.Equals(navVersion2.Value);
	}

	public static bool operator !=(NavVersion left, NavVersion right)
	{
		return !(left == right);
	}

	public static bool operator <(NavVersion left, NavVersion right)
	{
		return OperatorCompareToWithNullCoercion(left, right) < 0;
	}

	public static bool operator >(NavVersion left, NavVersion right)
	{
		return OperatorCompareToWithNullCoercion(left, right) > 0;
	}

	public static bool operator <=(NavVersion left, NavVersion right)
	{
		return OperatorCompareToWithNullCoercion(left, right) <= 0;
	}

	public static bool operator >=(NavVersion left, NavVersion right)
	{
		return OperatorCompareToWithNullCoercion(left, right) >= 0;
	}

	private static int OperatorCompareToWithNullCoercion(NavVersion left, NavVersion right)
	{
		NavVersion navVersion = left ?? Default;
		NavVersion other = right ?? Default;
		return navVersion.CompareTo(other);
	}

	public static implicit operator NavVersion(Version version)
	{
		return new NavVersion(version);
	}

	public override int CompareTo(NavValue other)
	{
		if (other == null)
		{
			return Value.CompareTo(Default.Value);
		}
		switch (other.NclType)
		{
		case NavNclType.NavBigText:
		case NavNclType.NavText:
		case NavNclType.NavCode:
			return Value.CompareTo(new NavVersion(other.ToString(invariant: true)).value);
		case NavNclType.NavVersion:
			return Value.CompareTo(((NavVersion)other).Value);
		default:
			throw NavValue.CreateUnableToCreateFromObjectException(GetType(), other);
		}
	}
}
