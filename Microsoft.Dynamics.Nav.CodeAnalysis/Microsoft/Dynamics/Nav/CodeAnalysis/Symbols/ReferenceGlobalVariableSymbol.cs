using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceGlobalVariableSymbol : VariableSymbol
{
	private readonly Lazy<ImmutableArray<AttributeSymbol>> lazyAttributes;

	private readonly Lazy<TypeSymbol> lazyType;

	public override SymbolKind Kind => SymbolKind.GlobalVariable;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override ImmutableArray<AttributeSymbol> Attributes => lazyAttributes.Value;

	public override TypeSymbol Type => lazyType.Value;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	internal ReferenceGlobalVariableSymbol(ContainerSymbol containingSymbol, VariableDefinition variable)
	{
		ReferenceGlobalVariableSymbol owner = this;
		ContainingSymbol = containingSymbol;
		Name = variable.Name();
		lazyType = ReferenceSymbolHelper.CreateLazy(() => SerializableSymbolModelConverter.ResolveType(containingSymbol.ContainingModule, owner, variable.GetTypeDefinition()));
		lazyAttributes = ReferenceSymbolHelper.BuildAttributesLazy(this, variable);
	}
}
