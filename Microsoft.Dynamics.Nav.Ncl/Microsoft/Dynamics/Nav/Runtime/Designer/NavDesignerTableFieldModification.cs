using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public class NavDesignerTableFieldModification : NavDesignerTableFieldBase
{
	public override FieldBaseSyntax Syntax { get; }

	public NavDesignerTableFieldModification(FieldModificationSyntax syntax)
	{
		Syntax = syntax;
	}
}
