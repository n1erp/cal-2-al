using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceQueryTypeSymbol : QueryTypeSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private ImmutableArray<Symbol> codeMembers;

	private Dictionary<string, ImmutableArray<Symbol>> codeMembersByName;

	public override int Id { get; }

	public override string Name { get; }

	public override QueryTypeKind QueryType => this.GetEnumPropertyValueOrDefault<QueryTypeKind>(PropertyKind.QueryType);

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => ImmutableArray<VariableSymbol>.Empty;

	public ReferenceQueryTypeSymbol(ModuleSymbol module, QueryDefinition query)
		: base(module)
	{
		ReferenceQueryTypeSymbol referenceQueryTypeSymbol = this;
		Id = query.Id ?? (-1);
		Name = query.Name();
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, query);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceQueryTypeSymbol.LazyGetMembers(query));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		LazyGetCodeMembers();
		return codeMembers;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetCodeMembers(ref codeMembersByName, name);
	}

	private void LazyGetCodeMembers()
	{
		if (codeMembers != default(ImmutableArray<Symbol>))
		{
			return;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		try
		{
			instance.AddRange(from x in GetMembers()
				where x.Kind != SymbolKind.QueryDataItem
				select x);
			instance.AddRange(FlattenedColumns);
			instance.AddRange(FlattenedFilters);
			instance.AddRange(FlattenedDataItems);
			ImmutableInterlocked.InterlockedCompareExchange(ref codeMembers, instance.ToImmutable(), default);
		}
		finally
		{
			instance.Free();
		}
	}

	private ImmutableArray<Symbol> LazyGetMembers(QueryDefinition query)
	{
		IEnumerable<ReferenceMethodSymbol> items = query.Methods?.Select((MethodDefinition m) => new ReferenceMethodSymbol(this, m)) ?? SpecializedCollections.EmptyEnumerable<ReferenceMethodSymbol>();
		int lengthSafe = query.Methods.GetLengthSafe();
		IEnumerable<ReferenceQueryDataItemSymbol> items2 = query.Elements?.Select((QueryDataItemDefinition dataItem) => new ReferenceQueryDataItemSymbol(this, dataItem)) ?? SpecializedCollections.EmptyEnumerable<ReferenceQueryDataItemSymbol>();
		int lengthSafe2 = query.Elements.GetLengthSafe();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(lengthSafe + lengthSafe2);
		try
		{
			instance.AddRange(items);
			instance.AddRange(items2);
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}
}
