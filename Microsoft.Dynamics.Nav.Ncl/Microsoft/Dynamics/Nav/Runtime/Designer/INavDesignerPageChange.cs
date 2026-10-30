using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal interface INavDesignerPageChange
{
	PropertySyntax Syntax { get; }
}
