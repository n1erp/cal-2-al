using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageGroup : NavDesignerPageControl
{
	internal new PageGroupSyntax Syntax
	{
		get
		{
			return base.Syntax as PageGroupSyntax;
		}
		set
		{
			base.Syntax = value;
		}
	}

	internal NavDesignerPageGroup(string groupName)
		: base(groupName)
	{
		Syntax = SyntaxFactory.PageGroup(SyntaxFactory.Token(SyntaxKind.PageGroupKeyword), groupName);
	}

	internal NavDesignerPageGroup(PageGroupSyntax groupSyntax)
		: base(groupSyntax)
	{
	}
}
