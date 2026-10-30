using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal abstract class GenericLanguageClassTypeSymbolFactory : IGenericLanguageClassTypeSymbolFactory, IGenericTypeConstraintValidator
{
	private readonly ConcurrentDictionary<TypeSymbol[], NavTypeSymbol> TypeCache = new ConcurrentDictionary<TypeSymbol[], NavTypeSymbol>(GenericTypeArgumentsEqualityComparer.Instance);

	public virtual bool RequiresTemplateArgumentValidation { get; }

	public int NumberOfTemplateArguments => TypeConstraintValidator.NumberOfTemplateArguments;

	protected abstract IGenericTypeConstraintValidator TypeConstraintValidator { get; }

	public NavTypeSymbol Create(TypeSymbol[] templateArguments)
	{
		return TypeCache.GetOrAdd(templateArguments, ConstructNavType(templateArguments));
	}

	public ImmutableArray<bool> ValidateTemplateArguments(TypeSymbol[] templateArguments)
	{
		return TypeConstraintValidator.ValidateTemplateArguments(templateArguments);
	}

	protected abstract NavTypeSymbol ConstructNavType(TypeSymbol[] templateArguments);
}
