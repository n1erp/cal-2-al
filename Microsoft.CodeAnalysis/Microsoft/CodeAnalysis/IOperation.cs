using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis;

[InternalImplementationOnly]
public interface IOperation
{
	IOperation Parent { get; }

	OperationKind Kind { get; }

	SyntaxNode Syntax { get; }

	ITypeSymbol Type { get; }

	Optional<object> ConstantValue { get; }

	IEnumerable<IOperation> Children { get; }

	string Language { get; }

	bool IsImplicit { get; }

	SemanticModel SemanticModel { get; }

	void Accept(OperationVisitor visitor);

	TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument);
}
