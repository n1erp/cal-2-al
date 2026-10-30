using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBadStatement : BoundStatement, IInvalidStatement, IOperation
{
	public ImmutableArray<BoundNode> ChildBoundNodes { get; private set; }

	protected override OperationKind StatementKind => OperationKind.InvalidStatement;

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

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInvalidStatement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInvalidStatement(this, argument);
	}
}
