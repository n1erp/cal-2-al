namespace Microsoft.CodeAnalysis.Operations;

public interface ICompoundAssignmentOperation : IAssignmentOperation, IOperation
{
	BinaryOperatorKind OperatorKind { get; }

	IMethodSymbol OperatorMethod { get; }

	bool IsLifted { get; }

	bool IsChecked { get; }

	CommonConversion InConversion { get; }

	CommonConversion OutConversion { get; }
}
