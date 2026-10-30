using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerObject
{
	public string ObjectName { get; private set; }

	internal virtual ObjectSyntax Syntax { get; private set; }

	protected NavDesignerObject(string objectName)
	{
		ObjectName = objectName;
	}

	protected NavDesignerObject(string objectName, ObjectSyntax syntax)
	{
		ObjectName = objectName;
		Syntax = syntax;
	}

	public static NavDesignerObject CreateFromObjectSyntax(ObjectSyntax syntax)
	{
		string objectName = syntax.GetObjectName() ?? string.Empty;
		return new NavDesignerObject(objectName, syntax);
	}
}
