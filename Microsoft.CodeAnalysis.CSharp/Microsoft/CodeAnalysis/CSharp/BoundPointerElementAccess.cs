using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPointerElementAccess : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Expression, (BoundNode)Index);

	public BoundExpression Expression { get; }

	public BoundExpression Index { get; }

	public bool Checked { get; }

	public BoundPointerElementAccess(SyntaxNode syntax, BoundExpression expression, BoundExpression index, bool @checked, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PointerElementAccess, syntax, type, hasErrors || expression.HasErrors() || index.HasErrors())
	{
		Expression = expression;
		Index = index;
		Checked = @checked;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPointerElementAccess(this);
	}

	public BoundPointerElementAccess Update(BoundExpression expression, BoundExpression index, bool @checked, TypeSymbol type)
	{
		if (expression != Expression || index != Index || @checked != Checked || type != Type)
		{
			return new BoundPointerElementAccess(Syntax, expression, index, @checked, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
