namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IArgument : IOperation
{
	IParameterSymbol Parameter { get; }

	IOperation Value { get; }
}
