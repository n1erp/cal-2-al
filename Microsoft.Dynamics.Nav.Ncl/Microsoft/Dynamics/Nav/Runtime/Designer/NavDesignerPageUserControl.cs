using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageUserControl : NavDesignerPageControl
{
	internal new PageUserControlSyntax Syntax
	{
		get
		{
			return base.Syntax as PageUserControlSyntax;
		}
		set
		{
			base.Syntax = value;
		}
	}

	internal NavDesignerPageUserControl(PageUserControlSyntax userControlSyntax)
		: base(userControlSyntax)
	{
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj as NavDesignerPageUserControl);
	}

	public override int GetHashCode()
	{
		return ControlName?.GetHashCode() ?? 0;
	}

	public static bool operator ==(NavDesignerPageUserControl left, NavDesignerPageUserControl right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(NavDesignerPageUserControl left, NavDesignerPageUserControl right)
	{
		return !object.Equals(left, right);
	}
}
