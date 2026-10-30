using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerPageModify : INavDesignerPageChange
{
	public PropertySyntax Syntax { get; }

	public string PropertyName { get; }

	internal NavDesignerPageModify(PropertySyntax property)
	{
		Syntax = property;
		PropertyName = property?.Name?.Identifier.ValueText;
	}
}
