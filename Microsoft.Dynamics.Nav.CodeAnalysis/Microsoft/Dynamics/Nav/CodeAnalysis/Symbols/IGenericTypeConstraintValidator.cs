using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal interface IGenericTypeConstraintValidator
{
	int NumberOfTemplateArguments { get; }

	ImmutableArray<bool> ValidateTemplateArguments(TypeSymbol[] templateArguments);
}
