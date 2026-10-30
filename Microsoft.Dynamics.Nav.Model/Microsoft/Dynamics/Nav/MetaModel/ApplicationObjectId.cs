using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.MetaModel;

[DebuggerDisplay("({objectType} {objectNumber})")]
public struct ApplicationObjectId(ElementType objectType, int objectNumber) : IEquatable<ApplicationObjectId>, IComparable<ApplicationObjectId>, IComparable
{
	private readonly int objectNumber = objectNumber;

	private readonly ElementType objectType = objectType;

	public int ObjectNumber
	{
		[DebuggerStepThrough]
		get
		{
			return objectNumber;
		}
	}

	public ElementType ObjectType
	{
		[DebuggerStepThrough]
		get
		{
			return objectType;
		}
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
