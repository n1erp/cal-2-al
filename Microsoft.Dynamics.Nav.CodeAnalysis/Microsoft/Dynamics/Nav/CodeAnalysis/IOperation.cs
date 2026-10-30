using Microsoft.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IOperation
{
	OperationKind Kind { get; }

	bool IsInvalid { get; }

	SyntaxNode Syntax { get; }

	ITypeSymbol Type { get; }

	Optional<object> ConstantValue { get; }

	void Accept(OperationVisitor visitor);

	TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument);
}
