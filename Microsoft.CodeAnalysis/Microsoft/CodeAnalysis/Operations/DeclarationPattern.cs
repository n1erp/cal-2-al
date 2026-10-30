using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DeclarationPattern : Operation, IDeclarationPatternOperation, IPatternOperation, IOperation
{
	public ISymbol DeclaredSymbol { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public DeclarationPattern(ISymbol declaredSymbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.DeclarationPattern, semanticModel, syntax, type, constantValue, isImplicit)
	{
		DeclaredSymbol = declaredSymbol;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDeclarationPattern(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDeclarationPattern(this, argument);
	}
}
