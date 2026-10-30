using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundConversion : BoundExpression, IConversionExpression, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => Type;

	public BoundExpression Operand { get; private set; }

	public ConversionKind ConversionKind { get; private set; }

	public override LookupResultKind ResultKind => _ResultKind;

	public MethodSymbol SymbolOpt { get; private set; }

	public BoundExpression TargetOpt { get; private set; }

	public bool IsArrayIndex { get; private set; }

	public ConstantValue ConstantValueOpt { get; private set; }

	IOperation IConversionExpression.Operand => Operand;

	protected override OperationKind ExpressionKind => OperationKind.ConversionExpression;

	public BoundConversion(SyntaxNode syntax, BoundExpression operand, ConversionKind conversionKind, LookupResultKind resultKind, MethodSymbol symbolOpt, BoundExpression targetOpt, bool isArrayIndex, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.Conversion, syntax, type, hasErrors || operand.HasErrors() || targetOpt.HasErrors())
	{
		Operand = operand;
		ConversionKind = conversionKind;
		_ResultKind = resultKind;
		SymbolOpt = symbolOpt;
		TargetOpt = targetOpt;
		IsArrayIndex = isArrayIndex;
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConversion(this);
	}

	public BoundConversion Update(BoundExpression operand, ConversionKind conversionKind, LookupResultKind resultKind, MethodSymbol symbolOpt, BoundExpression targetOpt, bool isArrayIndex, ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (operand != Operand || conversionKind != ConversionKind || resultKind != ResultKind || symbolOpt != SymbolOpt || targetOpt != TargetOpt || isArrayIndex != IsArrayIndex || constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundConversion(Syntax, operand, conversionKind, resultKind, symbolOpt, targetOpt, isArrayIndex, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConversionExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitConversionExpression(this, argument);
	}

	public static BoundConversion SynthesizedNonUserDefined(SyntaxNode syntax, BoundExpression operand, ConversionKind kind, TypeSymbol type, ConstantValue constantValueOpt = null)
	{
		return new BoundConversion(syntax, operand, kind, LookupResultKind.Viable, null, null, isArrayIndex: false, constantValueOpt, type)
		{
			WasCompilerGenerated = true
		};
	}

	public static BoundConversion Synthesized(SyntaxNode syntax, BoundExpression operand, Conversion conversion, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
	{
		return new BoundConversion(syntax, operand, conversion, constantValueOpt, type, null, hasErrors || !conversion.IsValid)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, ConstantValue constantValueOpt, TypeSymbol type, BoundExpression targetOpt, bool hasErrors = false)
		: this(syntax, operand, conversion.Kind, LookupResultKind.Viable, null, targetOpt, conversion.IsArrayIndex, constantValueOpt, type, hasErrors || !conversion.IsValid)
	{
	}

	public BoundConversion(SyntaxNode syntax, BoundExpression operand, ConversionKind conversionKind, TypeSymbol type, BoundExpression targetOpt, bool hasErrors = false)
		: this(syntax, operand, conversionKind, LookupResultKind.Viable, null, targetOpt, isArrayIndex: false, null, type, hasErrors)
	{
	}
}
