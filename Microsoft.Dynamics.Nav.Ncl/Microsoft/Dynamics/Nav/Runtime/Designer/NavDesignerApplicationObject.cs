using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerApplicationObject : NavDesignerObject
{
	public ApplicationObjectId ObjectId { get; private set; }

	protected NavDesignerApplicationObject(ApplicationObjectId objectId, string objectName)
		: base(objectName)
	{
		ObjectId = objectId;
	}

	protected NavDesignerApplicationObject(ApplicationObjectId objectId, string objectName, ApplicationObjectSyntax syntax)
		: base(objectName, syntax)
	{
		ObjectId = objectId;
	}

	public static NavDesignerApplicationObject CreateFromSyntax(ApplicationObjectSyntax syntax)
	{
		int objectId = syntax.GetObjectId();
		string objectName = syntax.GetObjectName();
		ApplicationObjectId objectId2 = new ApplicationObjectId(NavDesignerUtil.GetApplicationObjectTypeFromSyntaxKind(syntax.Kind), objectId);
		return new NavDesignerApplicationObject(objectId2, objectName, syntax);
	}
}
