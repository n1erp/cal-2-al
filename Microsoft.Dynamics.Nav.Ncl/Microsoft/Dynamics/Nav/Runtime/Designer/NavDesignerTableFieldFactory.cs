using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerTableFieldFactory
{
	internal static NavDesignerTableFieldBase CreateTableField(FieldBaseSyntax fieldBaseSyntax)
	{
		if (!(fieldBaseSyntax is FieldSyntax fieldSyntax))
		{
			if (fieldBaseSyntax is FieldModificationSyntax syntax)
			{
				return new NavDesignerTableFieldModification(syntax);
			}
			return null;
		}
		return fieldSyntax.Type.Kind switch
		{
			SyntaxKind.DataType => (NavDesignerTableFieldBase)new NavDesignerSimpleTableField(fieldSyntax), 
			SyntaxKind.OptionDataType => new NavDesignerOptionTableField(fieldSyntax), 
			SyntaxKind.LengthDataType => new NavDesignerTextTableField(fieldSyntax), 
			_ => new NavDesignerTableField(fieldSyntax), 
		};
	}

	internal static NavDesignerTableField CreateTableField(int fieldNumber, string fieldName, NavType fieldType, int length = 0)
	{
		switch (fieldType)
		{
		case NavType.Date:
		case NavType.Time:
		case NavType.Decimal:
		case NavType.Boolean:
		case NavType.Integer:
		case NavType.BigInteger:
		case NavType.DateTime:
			return new NavDesignerSimpleTableField(fieldNumber, fieldName, fieldType);
		case NavType.Option:
			return new NavDesignerOptionTableField(fieldNumber, fieldName);
		case NavType.Text:
		case NavType.Code:
			return new NavDesignerTextTableField(fieldNumber, fieldName, fieldType, length);
		default:
			return new NavDesignerTableField(fieldNumber, fieldName, fieldType);
		}
	}
}
