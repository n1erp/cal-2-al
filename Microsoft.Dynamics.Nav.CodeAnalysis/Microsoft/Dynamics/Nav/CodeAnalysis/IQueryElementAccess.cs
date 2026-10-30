namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IQueryElementAccess : IOperation
{
	IOperation Instance { get; }

	IQueryElementSymbol QueryElementSymbol { get; }
}
