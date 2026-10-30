using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundIdentifierListPropertyValue : BoundPropertyValue
{
	private string valueText = string.Empty;

	public ImmutableArray<BoundExpression> Value { get; private set; }

	internal override object ValueObject => Value;

	internal override string ValueText
	{
		get
		{
			if (string.IsNullOrEmpty(valueText))
			{
				PropertyTypeInfo propertyTypeInfo = GetPropertyTypeInfo();
				switch (propertyTypeInfo.Kind)
				{
				case PropertyKind.ApplicationArea:
					valueText = GenerateApplicationAreaText();
					break;
				case PropertyKind.SumIndexFields:
				case PropertyKind.SqlIndex:
				case PropertyKind.RequestFilterFields:
				case PropertyKind.CalcFields:
					valueText = MetadataEmitterHelper.GetFieldPrefixedAttributeValues(Value.Select((BoundExpression v) => v.ExpressionSymbol).Cast<FieldSymbol>());
					break;
				case PropertyKind.DataCaptionFields:
				case PropertyKind.ODataKeyFields:
					valueText = GetListOfIdsFromSymbols(Value, ",");
					break;
				case PropertyKind.IndentationControls:
					valueText = GetListOfIdsFromSymbols(Value, ";");
					break;
				case PropertyKind.FreezeColumn:
				case PropertyKind.Provider:
					DebugAssertHelper.Assert(Value.Length == 1);
					valueText = GetListOfIdsFromSymbols(Value, ";");
					break;
				case PropertyKind.Customizations:
					valueText = GetListOfNamesFromSymbols(Value, ",");
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(propertyTypeInfo.Kind);
				}
			}
			return valueText;
		}
	}

	public BoundIdentifierListPropertyValue(SyntaxNode syntax, ImmutableArray<BoundExpression> value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.IdentifierListPropertyValue, syntax, type, hasErrors || value.HasErrors())
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitIdentifierListPropertyValue(this);
	}

	public BoundIdentifierListPropertyValue Update(ImmutableArray<BoundExpression> value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundIdentifierListPropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	private string GetListOfNamesFromSymbols(IEnumerable<BoundExpression> value, string separator)
	{
		IEnumerable<string> values = from IObjectTypeSymbol x in value.Select((BoundExpression x) => x.ExpressionSymbol)
			select x.Name;
		return string.Join(separator, values);
	}

	private static string GetListOfIdsFromSymbols(IEnumerable<BoundExpression> value, string separator)
	{
		IEnumerable<int> values = from ISymbolWithId x in value.Select((BoundExpression x) => x.ExpressionSymbol)
			select x.Id;
		return string.Join(separator, values);
	}

	private string GenerateApplicationAreaText()
	{
		string text = string.Empty;
		for (int i = 0; i < Value.Length; i++)
		{
			BoundLiteral boundLiteral = (BoundLiteral)Value[i];
			text = text + "#" + boundLiteral.ConstantValue.StringValue;
			if (i < Value.Length - 1)
			{
				text += ",";
			}
		}
		return text;
	}
}
