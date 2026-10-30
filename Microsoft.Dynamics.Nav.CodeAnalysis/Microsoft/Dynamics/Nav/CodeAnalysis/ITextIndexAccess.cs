namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITextIndexAccess : IOperation
{
	IOperation TextExpression { get; }

	IOperation Index { get; }
}
