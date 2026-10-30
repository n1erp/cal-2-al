using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArrayCreation : BoundExpression
{
	public ImmutableArray<BoundExpression> Bounds { get; }

	public BoundArrayInitialization InitializerOpt { get; }

	public BoundArrayCreation(SyntaxNode syntax, ImmutableArray<BoundExpression> bounds, BoundArrayInitialization initializerOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ArrayCreation, syntax, type, hasErrors || bounds.HasErrors() || initializerOpt.HasErrors())
	{
		Bounds = bounds;
		InitializerOpt = initializerOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArrayCreation(this);
	}

	public BoundArrayCreation Update(ImmutableArray<BoundExpression> bounds, BoundArrayInitialization initializerOpt, TypeSymbol type)
	{
		if (bounds != Bounds || initializerOpt != InitializerOpt || type != Type)
		{
			return new BoundArrayCreation(Syntax, bounds, initializerOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
