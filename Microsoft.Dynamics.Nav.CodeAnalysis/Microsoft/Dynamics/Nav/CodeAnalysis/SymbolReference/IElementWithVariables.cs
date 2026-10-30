namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

internal interface IElementWithVariables
{
	VariableDefinition[] Variables { get; set; }
}
