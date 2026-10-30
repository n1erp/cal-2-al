using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageControl
{
	internal string ControlName { get; }

	internal ControlBaseSyntax Syntax { get; set; }

	internal NavDesignerPageControl(string fieldName)
	{
		ControlName = fieldName;
	}

	internal NavDesignerPageControl(ControlBaseSyntax controlSyntax)
	{
		Syntax = controlSyntax;
		ControlName = controlSyntax?.Name?.Identifier.Text?.UnquoteIdentifier();
	}

	protected static PropertyListSyntax GetDefaultPropertyList()
	{
		return NavDesignerUtil.GetDefaultPropertyList();
	}

	protected virtual bool Equals(NavDesignerPageControl other)
	{
		return string.Equals(ControlName, other.ControlName);
	}
}
