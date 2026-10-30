using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBadStatement : BoundStatement
{
	protected override ImmutableArray<BoundNode> Children => ChildBoundNodes;

	public ImmutableArray<BoundNode> ChildBoundNodes { get; }

	public BoundBadStatement(SyntaxNode syntax, ImmutableArray<BoundNode> childBoundNodes, bool hasErrors = false)
		: base(BoundKind.BadStatement, syntax, hasErrors || childBoundNodes.HasErrors())
	{
		ChildBoundNodes = childBoundNodes;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBadStatement(this);
	}

	public BoundBadStatement Update(ImmutableArray<BoundNode> childBoundNodes)
	{
		if (childBoundNodes != ChildBoundNodes)
		{
			return new BoundBadStatement(Syntax, childBoundNodes, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
