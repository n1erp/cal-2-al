using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class BranchStatement : Operation, IBranchOperation, IOperation
{
	public ILabelSymbol Target { get; }

	public BranchKind BranchKind { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public BranchStatement(ILabelSymbol target, BranchKind branchKind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Branch, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Target = target;
		BranchKind = branchKind;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitBranch(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitBranch(this, argument);
	}
}
