namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ITestFilterFieldAccess : IOperation
{
	IOperation Instance { get; }

	IFieldSymbol FieldSymbol { get; }
}
