using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundComplexConditionalReceiver : BoundExpression
{
	public BoundExpression ValueTypeReceiver { get; }

	public BoundExpression ReferenceTypeReceiver { get; }

	public BoundComplexConditionalReceiver(SyntaxNode syntax, BoundExpression valueTypeReceiver, BoundExpression referenceTypeReceiver, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ComplexConditionalReceiver, syntax, type, hasErrors || valueTypeReceiver.HasErrors() || referenceTypeReceiver.HasErrors())
	{
		ValueTypeReceiver = valueTypeReceiver;
		ReferenceTypeReceiver = referenceTypeReceiver;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitComplexConditionalReceiver(this);
	}

	public BoundComplexConditionalReceiver Update(BoundExpression valueTypeReceiver, BoundExpression referenceTypeReceiver, TypeSymbol type)
	{
		if (valueTypeReceiver != ValueTypeReceiver || referenceTypeReceiver != ReferenceTypeReceiver || type != Type)
		{
			return new BoundComplexConditionalReceiver(Syntax, valueTypeReceiver, referenceTypeReceiver, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
