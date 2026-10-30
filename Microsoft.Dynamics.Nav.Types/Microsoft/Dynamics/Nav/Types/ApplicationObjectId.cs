using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[DebuggerDisplay("({objectType} {objectNumber})")]
public struct ApplicationObjectId(ObjectType objectType, int objectNumber) : IEquatable<ApplicationObjectId>, IComparable<ApplicationObjectId>, IComparable
{
	[DataMember]
	private readonly int objectNumber = objectNumber;

	[DataMember]
	private readonly ObjectType objectType = objectType;

	public int ObjectNumber
	{
		[DebuggerStepThrough]
		get
		{
			return objectNumber;
		}
	}

	public ObjectType ObjectType
	{
		[DebuggerStepThrough]
		get
		{
			return objectType;
		}
	}

	public override string ToString()
	{
		return objectNumber.ToString(CultureInfo.CurrentCulture);
	}

	public static bool operator ==(ApplicationObjectId left, ApplicationObjectId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ApplicationObjectId left, ApplicationObjectId right)
	{
		return !left.Equals(right);
	}

	public static bool operator <(ApplicationObjectId left, ApplicationObjectId right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(ApplicationObjectId left, ApplicationObjectId right)
	{
		return left.CompareTo(right) > 0;
	}

	public int CompareTo(ApplicationObjectId other)
	{
		int num = objectType - other.objectType;
		if (num != 0)
		{
			return num;
		}
		return objectNumber - other.objectNumber;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(ApplicationObjectId))
		{
			return false;
		}
		return Equals((ApplicationObjectId)obj);
	}

	public override int GetHashCode()
	{
		return objectNumber ^ ((int)objectType << 25);
	}

	public bool Equals(ApplicationObjectId other)
	{
		if (other.objectNumber == objectNumber)
		{
			return other.objectType == objectType;
		}
		return false;
	}

	int IComparable.CompareTo(object obj)
	{
		return CompareTo((ApplicationObjectId)obj);
	}
}
