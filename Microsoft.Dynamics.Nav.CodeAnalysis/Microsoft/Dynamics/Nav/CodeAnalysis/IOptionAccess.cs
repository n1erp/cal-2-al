namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IOptionAccess : IOperation
{
	IOperation Instance { get; }

	IOptionSymbol OptionSymbol { get; }
}
