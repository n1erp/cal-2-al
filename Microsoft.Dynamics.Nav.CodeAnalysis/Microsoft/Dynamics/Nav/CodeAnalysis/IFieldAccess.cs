namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IFieldAccess : IOperation
{
	IOperation Instance { get; }

	IFieldSymbol FieldSymbol { get; }
}
