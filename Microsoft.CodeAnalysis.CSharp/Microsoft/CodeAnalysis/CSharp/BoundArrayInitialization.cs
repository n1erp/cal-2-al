using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundArrayInitialization : BoundExpression
{
	public ImmutableArray<BoundExpression> Initializers { get; }

	public BoundArrayInitialization(SyntaxNode syntax, ImmutableArray<BoundExpression> initializers, bool hasErrors = false)
		: base(BoundKind.ArrayInitialization, syntax, null, hasErrors || initializers.HasErrors())
	{
		Initializers = initializers;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitArrayInitialization(this);
	}

	public BoundArrayInitialization Update(ImmutableArray<BoundExpression> initializers)
	{
		if (initializers != Initializers)
		{
			return new BoundArrayInitialization(Syntax, initializers, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
