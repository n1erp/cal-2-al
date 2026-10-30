using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConversion : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public ConversionKind ConversionKind => Conversion.Kind;

	public bool IsExtensionMethod => Conversion.IsExtensionMethod;

	public MethodSymbol SymbolOpt => Conversion.Method;

	public override Symbol ExpressionSymbol => SymbolOpt;

	public ImmutableArray<MethodSymbol> OriginalUserDefinedConversionsOpt { get; }

	public override bool SuppressVirtualCalls => IsBaseConversion;

	public BoundExpression Operand { get; }

	public Conversion Conversion { get; }

	public bool IsBaseConversion { get; }

	public bool Checked { get; }

	public bool ExplicitCastInCode { get; }

	public ConstantValue ConstantValueOpt { get; }

	internal bool ConversionHasSideEffects()
	{
		switch (ConversionKind)
		{
		case ConversionKind.Identity:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
			return false;
		case ConversionKind.ExplicitNumeric:
			return Checked;
		default:
			return true;
		}
	}

	public static BoundConversion SynthesizedNonUserDefined(SyntaxNode syntax, BoundExpression operand, Conversion conversion, TypeSymbol type, ConstantValue constantValueOpt = null)
	{
		return new BoundConversion(syntax, operand, conversion, isBaseConversion: false, @checked: false, explicitCastInCode: false, constantValueOpt, type)
		{
			WasCompilerGenerated = true
		};
	}

	public static BoundConversion Synthesized(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, bool explicitCastInCode, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
	{
		return new BoundConversion(syntax, operand, conversion, @checked, explicitCastInCode, constantValueOpt, type, hasErrors || !conversion.IsValid)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, bool explicitCastInCode, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operand, conversion, isBaseConversion: false, @checked, explicitCastInCode, constantValueOpt, type, hasErrors || !conversion.IsValid)
	{
		OriginalUserDefinedConversionsOpt = conversion.OriginalUserDefinedConversions;
	}

	public BoundConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool isBaseConversion, bool @checked, bool explicitCastInCode, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Conversion, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
		Conversion = conversion;
		IsBaseConversion = isBaseConversion;
		Checked = @checked;
		ExplicitCastInCode = explicitCastInCode;
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConversion(this);
	}

	public BoundConversion Update(BoundExpression operand, Conversion conversion, bool isBaseConversion, bool @checked, bool explicitCastInCode, ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (operand != Operand || conversion != Conversion || isBaseConversion != IsBaseConversion || @checked != Checked || explicitCastInCode != ExplicitCastInCode || constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundConversion(Syntax, operand, conversion, isBaseConversion, @checked, explicitCastInCode, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
