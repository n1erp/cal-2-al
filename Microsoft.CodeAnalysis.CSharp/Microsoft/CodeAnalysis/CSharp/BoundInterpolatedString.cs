using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundInterpolatedString : BoundExpression
{
	public ImmutableArray<BoundExpression> Parts { get; }

	public BoundInterpolatedString(SyntaxNode syntax, ImmutableArray<BoundExpression> parts, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.InterpolatedString, syntax, type, hasErrors || parts.HasErrors())
	{
		Parts = parts;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitInterpolatedString(this);
	}

	public BoundInterpolatedString Update(ImmutableArray<BoundExpression> parts, TypeSymbol type)
	{
		if (parts != Parts || type != Type)
		{
			return new BoundInterpolatedString(Syntax, parts, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
