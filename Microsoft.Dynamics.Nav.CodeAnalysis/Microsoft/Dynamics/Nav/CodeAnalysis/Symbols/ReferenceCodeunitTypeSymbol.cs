using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceCodeunitTypeSymbol : CodeunitTypeSymbol
{
	private readonly Lazy<ImmutableArray<VariableSymbol>> lazyVariables;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<TableTypeSymbol> lazyRelatedTable;

	private Dictionary<string, ImmutableArray<Symbol>> membersByName;

	public override int Id { get; }

	public override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => lazyVariables.Value;

	internal override TableTypeSymbol RelatedTable => lazyRelatedTable?.Value;

	internal ReferenceCodeunitTypeSymbol(ModuleSymbol module, CodeunitDefinition codeunit)
		: base(module)
	{
		ReferenceCodeunitTypeSymbol referenceCodeunitTypeSymbol = this;
		DebugAssertHelper.Assert(codeunit != null);
		Id = codeunit.Id ?? (-1);
		Name = codeunit.Name().UnquoteIdentifier();
		lazyVariables = ReferenceSymbolHelper.BuildGlobalVariablesLazy(this, codeunit);
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, codeunit);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceCodeunitTypeSymbol.GetMethods(codeunit));
		lazyRelatedTable = ReferenceSymbolHelper.CreateLazy(GetRelatedTable);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (membersByName == null)
		{
			Dictionary<string, ImmutableArray<Symbol>> value = GetMembers().Concat(GlobalVariables).ToDictionaryOfArrays((Symbol m) => m.Name, SemanticFacts.NameEqualityComparer);
			Interlocked.CompareExchange(ref membersByName, value, null);
		}
		if (membersByName.TryGetValue(name, out var value2))
		{
			return value2;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	private ImmutableArray<Symbol> GetMethods(CodeunitDefinition codeunit)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(codeunit.Methods.GetLengthSafe());
		instance.AddRange(codeunit.Methods?.Select((MethodDefinition m) => new ReferenceMethodSymbol(this, m)) ?? SpecializedCollections.EmptyEnumerable<ReferenceMethodSymbol>());
		CodeunitObjectMembers.AddBuiltinMethods(instance, this);
		return instance.ToImmutableAndFree();
	}

	private TableTypeSymbol GetRelatedTable()
	{
		PropertySymbol property = GetProperty(PropertyKind.TableNo);
		if (property == null)
		{
			return null;
		}
		return ReferenceSymbolHelper.GetRelatedTableSymbol(this, property.Value.ToString().UnquoteIdentifier());
	}
}
