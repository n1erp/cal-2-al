using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageActionGroup : NavDesignerPageAction
{
	internal new PageActionGroupSyntax Syntax
	{
		get
		{
			return base.Syntax as PageActionGroupSyntax;
		}
		set
		{
			base.Syntax = value;
		}
	}

	internal NavDesignerPageActionGroup(string groupName)
		: base(groupName)
	{
		Syntax = SyntaxFactory.PageActionGroup(groupName);
	}

	internal NavDesignerPageActionGroup(PageActionGroupSyntax groupSyntax)
		: base(groupSyntax)
	{
	}
}
