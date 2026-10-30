using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPseudoVariable : BoundExpression
{
	public LocalSymbol LocalSymbol { get; }

	public PseudoVariableExpressions EmitExpressions { get; }

	public BoundPseudoVariable(SyntaxNode syntax, LocalSymbol localSymbol, PseudoVariableExpressions emitExpressions, TypeSymbol type, bool hasErrors)
		: base(BoundKind.PseudoVariable, syntax, type, hasErrors)
	{
		LocalSymbol = localSymbol;
		EmitExpressions = emitExpressions;
	}

	public BoundPseudoVariable(SyntaxNode syntax, LocalSymbol localSymbol, PseudoVariableExpressions emitExpressions, TypeSymbol type)
		: base(BoundKind.PseudoVariable, syntax, type)
	{
		LocalSymbol = localSymbol;
		EmitExpressions = emitExpressions;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPseudoVariable(this);
	}

	public BoundPseudoVariable Update(LocalSymbol localSymbol, PseudoVariableExpressions emitExpressions, TypeSymbol type)
	{
		if (localSymbol != LocalSymbol || emitExpressions != EmitExpressions || type != Type)
		{
			return new BoundPseudoVariable(Syntax, localSymbol, emitExpressions, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
