using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNewT : BoundExpression
{
	public BoundObjectInitializerExpressionBase InitializerExpressionOpt { get; }

	public BoundNewT(SyntaxNode syntax, BoundObjectInitializerExpressionBase initializerExpressionOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NewT, syntax, type, hasErrors || initializerExpressionOpt.HasErrors())
	{
		InitializerExpressionOpt = initializerExpressionOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNewT(this);
	}

	public BoundNewT Update(BoundObjectInitializerExpressionBase initializerExpressionOpt, TypeSymbol type)
	{
		if (initializerExpressionOpt != InitializerExpressionOpt || type != Type)
		{
			return new BoundNewT(Syntax, initializerExpressionOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
