using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public interface INavDesignerPageControlChange
{
	SyntaxKind SyntaxKind { get; }

	ControlChangeBaseSyntax Syntax { get; }
}
