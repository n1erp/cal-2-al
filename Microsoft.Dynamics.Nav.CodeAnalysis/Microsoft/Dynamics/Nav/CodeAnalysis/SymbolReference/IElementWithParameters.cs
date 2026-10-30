namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

internal interface IElementWithParameters
{
	ParameterDefinition[] Parameters { get; set; }
}
