using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public interface INavDesignerPageViewChange
{
	SyntaxKind SyntaxKind { get; }

	ViewChangeBaseSyntax Syntax { get; }
}
