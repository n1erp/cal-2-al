namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal interface IGenericLanguageClassTypeSymbolFactory : IGenericTypeConstraintValidator
{
	bool RequiresTemplateArgumentValidation { get; }

	NavTypeSymbol Create(TypeSymbol[] templateArguments);
}
