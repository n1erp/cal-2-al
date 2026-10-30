using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public interface INavDesignerPageActionChange
{
	SyntaxKind SyntaxKind { get; }

	ActionChangeBaseSyntax Syntax { get; }
}
