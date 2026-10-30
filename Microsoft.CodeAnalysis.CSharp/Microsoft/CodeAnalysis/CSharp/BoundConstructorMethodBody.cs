using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConstructorMethodBody : BoundMethodBodyBase
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundExpressionStatement Initializer { get; }

	public BoundConstructorMethodBody(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundExpressionStatement initializer, BoundBlock blockBody, BoundBlock expressionBody, bool hasErrors = false)
		: base(BoundKind.ConstructorMethodBody, syntax, blockBody, expressionBody, hasErrors || initializer.HasErrors() || blockBody.HasErrors() || expressionBody.HasErrors())
	{
		Locals = locals;
		Initializer = initializer;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConstructorMethodBody(this);
	}

	public BoundConstructorMethodBody Update(ImmutableArray<LocalSymbol> locals, BoundExpressionStatement initializer, BoundBlock blockBody, BoundBlock expressionBody)
	{
		if (locals != Locals || initializer != Initializer || blockBody != BlockBody || expressionBody != ExpressionBody)
		{
			return new BoundConstructorMethodBody(Syntax, locals, initializer, blockBody, expressionBody, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
