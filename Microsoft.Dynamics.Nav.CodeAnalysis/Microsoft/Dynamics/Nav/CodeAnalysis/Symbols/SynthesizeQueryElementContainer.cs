using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class SynthesizeQueryElementContainer : SynthesizedSymbolContainer<QueryElementSymbol>
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	private class AliasedQueryElementSymbol : QueryElementSymbol
	{
		private readonly QueryElementSymbol originalSymbol;

		private TypeSymbol lazyType;

		public override SymbolKind Kind => originalSymbol.Kind;

		public override Symbol ContainingSymbol => originalSymbol.ContainingSymbol;

		public override Accessibility DeclaredAccessibility => Accessibility.Private;

		public override ImmutableArray<Location> Locations => originalSymbol.Locations;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => originalSymbol.DeclaringSyntaxReferences;

		public override FieldSymbol RelatedField => originalSymbol.RelatedField;

		public override int Id => originalSymbol.Id;

		public override ImmutableArray<PropertySymbol> Properties => originalSymbol.Properties;

		public override TypeSymbol Type
		{
			get
			{
				if (lazyType != null)
				{
					return lazyType;
				}
				Interlocked.CompareExchange(value: (originalSymbol.Kind != SymbolKind.QueryFilter) ? originalSymbol.Type : (originalSymbol.RelatedField?.Type ?? originalSymbol.Type), location1: ref lazyType, comparand: null);
				return lazyType;
			}
		}

		public override TypeCategoryKind TypeCategory => originalSymbol.TypeCategory;

		public override string Name => originalSymbol.Name;

		public override string MetadataName => originalSymbol.MetadataName;

		public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => originalSymbol.PropertyTypeInfos;

		public AliasedQueryElementSymbol(QueryElementSymbol originalSymbol)
		{
			this.originalSymbol = originalSymbol;
		}

		public override ImmutableArray<Symbol> GetMembers()
		{
			return originalSymbol.GetMembers();
		}

		public override ImmutableArray<Symbol> GetMembers(string name)
		{
			return originalSymbol.GetMembers(name);
		}
	}

	public static SynthesizeQueryElementContainer Create(QueryElementSymbol originalSymbol)
	{
		return new SynthesizeQueryElementContainer(originalSymbol);
	}

	private SynthesizeQueryElementContainer(QueryElementSymbol originalSymbol)
		: base((QueryElementSymbol)new AliasedQueryElementSymbol(originalSymbol))
	{
	}
}
