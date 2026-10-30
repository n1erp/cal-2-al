using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class PlaceholderExpression : Operation, IPlaceholderOperation, IOperation
{
	public PlaceholderKind PlaceholderKind { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public PlaceholderExpression(PlaceholderKind placeholderKind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.None, semanticModel, syntax, type, constantValue, isImplicit)
	{
		PlaceholderKind = placeholderKind;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitPlaceholder(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitPlaceholder(this, argument);
	}
}
