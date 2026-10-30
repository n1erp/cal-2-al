using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class TriggerEventTypeInfo
{
	internal string Name { get; }

	internal string SyntaxOwner { get; }

	internal ImmutableArray<TriggerParameter> Parameters { get; }

	internal SymbolKind ElementType { get; }

	public TriggerEventTypeInfo(string syntaxOwner, string name, SymbolKind elementType, params TriggerParameter[] parameters)
	{
		Name = name;
		SyntaxOwner = syntaxOwner;
		ElementType = elementType;
		Parameters = parameters.AsImmutableOrEmpty();
	}
}
