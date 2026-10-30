using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class EnumExtensionTypeSymbol : ApplicationObjectExtensionTypeSymbol, IRootTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	public override SymbolKind Kind => SymbolKind.EnumExtension;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind => NavTypeKind.EnumExtension;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ImmutableDictionary<string, PropertyTypeInfo>.Empty;

	public abstract ImmutableArray<EnumValueSymbol> Values { get; }

	internal override ImmutableArray<VariableSymbol> GlobalVariables
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	protected EnumExtensionTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}
}
