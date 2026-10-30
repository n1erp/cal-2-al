using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class GridLayoutPropertyValueValidator
{
	public static bool ValidatePropertyValue(string propertyName, string propertyValue, PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		PageGroupSyntax pageGroupSyntax = propertyValueSyntax.Parent.Parent.Parent as PageGroupSyntax;
		DebugAssertHelper.Assert(pageGroupSyntax != null || propertyValueSyntax.Parent.Parent.Parent is ControlModifyChangeSyntax);
		int num;
		if (pageGroupSyntax != null)
		{
			num = ((pageGroupSyntax.ControlKeyword.Kind == SyntaxKind.PageGridKeyword) ? 1 : 0);
			if (num == 0)
			{
				diagnostics.Add(ErrorCode.ERR_GridLayoutMustBeOnGrid, propertyValueSyntax.Parent.Location);
			}
		}
		else
		{
			num = 1;
		}
		return (byte)num != 0;
	}
}
