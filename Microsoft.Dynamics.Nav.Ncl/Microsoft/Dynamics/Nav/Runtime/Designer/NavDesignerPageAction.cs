using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageAction
{
	internal string ActionName { get; }

	internal ActionBaseSyntax Syntax { get; set; }

	internal NavDesignerPageAction(string actionName)
	{
		ActionName = actionName;
		Syntax = SyntaxFactory.PageAction(actionName).WithPropertyList(GetDefaultPropertyList());
	}

	internal NavDesignerPageAction(ActionBaseSyntax actionSyntax)
	{
		Syntax = actionSyntax;
		ActionName = actionSyntax?.Name?.Identifier.Text?.UnquoteIdentifier();
	}

	protected static PropertyListSyntax GetDefaultPropertyList()
	{
		return NavDesignerUtil.GetDefaultPropertyList();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((NavDesignerPageAction)obj);
	}

	public bool Equals(NavDesignerPageAction other)
	{
		return string.Equals(ActionName, other.ActionName);
	}

	public override int GetHashCode()
	{
		return ActionName?.GetHashCode() ?? 0;
	}

	public static bool operator ==(NavDesignerPageAction left, NavDesignerPageAction right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(NavDesignerPageAction left, NavDesignerPageAction right)
	{
		return !object.Equals(left, right);
	}
}
