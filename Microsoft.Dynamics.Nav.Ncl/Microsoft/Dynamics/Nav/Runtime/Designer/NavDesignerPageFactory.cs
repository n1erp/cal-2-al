using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal abstract class NavDesignerPageFactory
{
	internal static NavDesignerPageControl CreatePageControl(ControlBaseSyntax syntax)
	{
		return syntax.Kind switch
		{
			SyntaxKind.PageField => new NavDesignerPageField(syntax as PageFieldSyntax), 
			SyntaxKind.PageUserControl => new NavDesignerPageUserControl(syntax as PageUserControlSyntax), 
			SyntaxKind.PageGroup => new NavDesignerPageGroup(syntax as PageGroupSyntax), 
			_ => new NavDesignerPageControl(syntax), 
		};
	}

	internal static NavDesignerPageControl CreatePageControl(ControlBaseSyntax syntax, PropertyListSyntax propertyListSyntax)
	{
		return syntax.Kind switch
		{
			SyntaxKind.PageField => (NavDesignerPageControl)new NavDesignerPageField((syntax as PageFieldSyntax).WithPropertyList(propertyListSyntax)), 
			SyntaxKind.PageUserControl => new NavDesignerPageUserControl((syntax as PageUserControlSyntax).WithPropertyList(propertyListSyntax)), 
			SyntaxKind.PageGroup => new NavDesignerPageGroup((syntax as PageGroupSyntax).WithPropertyList(propertyListSyntax)), 
			_ => null, 
		};
	}

	internal static NavDesignerPageAction CreatePageAction(ActionBaseSyntax syntax)
	{
		return syntax.Kind switch
		{
			SyntaxKind.PageAction => new NavDesignerPageAction(syntax as PageActionSyntax), 
			SyntaxKind.PageActionGroup => new NavDesignerPageActionGroup(syntax as PageActionGroupSyntax), 
			_ => new NavDesignerPageAction(syntax), 
		};
	}

	internal static NavDesignerPageAction CreatePageAction(ActionBaseSyntax syntax, PropertyListSyntax propertyListSyntax)
	{
		return syntax.Kind switch
		{
			SyntaxKind.PageAction => new NavDesignerPageAction((syntax as PageActionSyntax).WithPropertyList(propertyListSyntax)), 
			SyntaxKind.PageActionGroup => new NavDesignerPageAction((syntax as PageActionGroupSyntax).WithPropertyList(propertyListSyntax)), 
			_ => null, 
		};
	}

	internal static NavDesignerPageView CreatePageView(PageViewSyntax syntax)
	{
		SyntaxKind kind = syntax.Kind;
		if (kind == SyntaxKind.PageView)
		{
			return new NavDesignerPageView(syntax);
		}
		return null;
	}

	internal static NavDesignerPageView CreatePageView(PageViewSyntax syntax, PropertyListSyntax propertyListSyntax)
	{
		SyntaxKind kind = syntax.Kind;
		if (kind == SyntaxKind.PageView)
		{
			return new NavDesignerPageView(syntax.WithPropertyList(propertyListSyntax));
		}
		return null;
	}
}
