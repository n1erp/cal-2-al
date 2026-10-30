using System;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public struct NavTrackingObject(long maxChangeTrackingNumberObjectMetadata, ApplicationObjectId applicationObjectId, ObjectTrackingChangeType changeType, PackageId appPackageId, string tenantId, ObjectTrackingChangeOperation changeOperation) : IEquatable<NavTrackingObject>
{
	private readonly long maxChangeTrackingNumberObjectMetadata = maxChangeTrackingNumberObjectMetadata;

	private readonly ApplicationObjectId applicationObjectId = applicationObjectId;

	private readonly ObjectTrackingChangeType changeType = changeType;

	private readonly PackageId appPackageId = appPackageId;

	private readonly string tenantId = tenantId;

	public long MaxChangeTrackingNumberObjectMetadata => maxChangeTrackingNumberObjectMetadata;

	public ApplicationObjectId ApplicationObjectId => applicationObjectId;

	public ObjectType ObjectType => applicationObjectId.ObjectType;

	public int ObjectId => applicationObjectId.ObjectNumber;

	public PackageId AppPackageId => appPackageId;

	public string TenantId => tenantId;

	public ObjectTrackingChangeType ChangeType => changeType;

	public ObjectTrackingChangeOperation ChangeOperation { get; } = changeOperation;

	public override bool Equals(object obj)
	{
		if (!(obj is NavTrackingObject))
		{
			return false;
		}
		return Equals((NavTrackingObject)obj);
	}

	public bool Equals(NavTrackingObject other)
	{
		if (maxChangeTrackingNumberObjectMetadata.Equals(other.maxChangeTrackingNumberObjectMetadata) && applicationObjectId.Equals(other.applicationObjectId) && ((object)appPackageId).Equals((object)other.appPackageId) && tenantId.Equals(other.tenantId) && changeType.Equals(other.changeType) && ChangeOperation.Equals(other.ChangeOperation))
		{
			return true;
		}
		return false;
	}

	public static bool operator ==(NavTrackingObject trackingObject1, NavTrackingObject trackingObject2)
	{
		return trackingObject1.Equals(trackingObject2);
	}

	public static bool operator !=(NavTrackingObject trackingObject1, NavTrackingObject trackingObject2)
	{
		return !trackingObject1.Equals(trackingObject2);
	}

	public override int GetHashCode()
	{
		return maxChangeTrackingNumberObjectMetadata.GetHashCode() ^ applicationObjectId.GetHashCode() ^ ((object)appPackageId).GetHashCode() ^ tenantId.GetHashCode() ^ ChangeOperation.GetHashCode();
	}
}
