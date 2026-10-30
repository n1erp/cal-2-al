using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public abstract class NavDesignerTableFieldBase
{
	public abstract FieldBaseSyntax Syntax { get; }
}
