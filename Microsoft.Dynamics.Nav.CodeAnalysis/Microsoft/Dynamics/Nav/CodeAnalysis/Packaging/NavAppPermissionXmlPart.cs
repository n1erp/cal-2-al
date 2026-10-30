using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public enum NavAppPermissionXmlPart
{
	PermissionSets,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "CSide field name")]
	RoleID,
	RoleName,
	PermissionSet,
	Permission,
	ReadPermission,
	InsertPermission,
	ModifyPermission,
	DeletePermission,
	ExecutePermission,
	ObjectType,
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID", Justification = "CSide field name")]
	ObjectID,
	SecurityFilter
}
