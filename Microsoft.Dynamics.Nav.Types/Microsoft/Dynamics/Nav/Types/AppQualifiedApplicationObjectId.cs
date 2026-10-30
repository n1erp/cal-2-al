using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[DebuggerDisplay("({objectType} {objectNumber} {appGroupId})")]
public struct AppQualifiedApplicationObjectId(ObjectType objectType, int objectNumber, int appGroupId) : IEquatable<AppQualifiedApplicationObjectId>, IComparable<AppQualifiedApplicationObjectId>, IComparable
{
	[DataMember]
	private readonly int objectNumber = objectNumber;

	[DataMember]
	private readonly ObjectType objectType = objectType;

	[DataMember]
	private readonly int appGroupId = appGroupId;

	public ApplicationObjectId UnqualifiedId => new ApplicationObjectId(objectType, objectNumber);

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

	public int AppGroupId
	{
		[DebuggerStepThrough]
		get
		{
			return appGroupId;
		}
	}

	public static bool operator ==(AppQualifiedApplicationObjectId left, AppQualifiedApplicationObjectId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AppQualifiedApplicationObjectId left, AppQualifiedApplicationObjectId right)
	{
		return !left.Equals(right);
	}

	public static bool operator <(AppQualifiedApplicationObjectId left, AppQualifiedApplicationObjectId right)
	{
		return left.CompareTo(right) < 0;
	}

	public static bool operator >(AppQualifiedApplicationObjectId left, AppQualifiedApplicationObjectId right)
	{
		return left.CompareTo(right) > 0;
	}

	public int CompareTo(AppQualifiedApplicationObjectId other)
	{
		int num = objectType - other.objectType;
		if (num != 0)
		{
			return num;
		}
		int num2 = objectNumber - other.objectNumber;
		if (num2 != 0)
		{
			return num2;
		}
		return appGroupId - other.appGroupId;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(AppQualifiedApplicationObjectId))
		{
			return false;
		}
		return Equals((AppQualifiedApplicationObjectId)obj);
	}

	public override int GetHashCode()
	{
		return objectNumber ^ ((int)objectType << 25) ^ appGroupId;
	}

	public bool Equals(AppQualifiedApplicationObjectId other)
	{
		if (other.objectNumber == objectNumber && other.objectType == objectType)
		{
			return other.appGroupId == appGroupId;
		}
		return false;
	}

	int IComparable.CompareTo(object obj)
	{
		return CompareTo((AppQualifiedApplicationObjectId)obj);
	}
}
