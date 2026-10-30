using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceViewSymbol : ViewSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<ChangeMoveSymbol>> lazyMoveSymbols;

	private readonly Lazy<ImmutableArray<ChangeModifySymbol>> lazyModifySymbols;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override int Id { get; }

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	internal override ImmutableArray<ChangeModifySymbol> ModifySymbols => lazyModifySymbols.Value;

	internal override ImmutableArray<ChangeMoveSymbol> MoveSymbols => lazyMoveSymbols.Value;

	internal ReferenceViewSymbol(Symbol containingSymbol, ViewDefinition viewDefinition)
	{
		ReferenceViewSymbol referenceViewSymbol = this;
		DebugAssertHelper.Assert(viewDefinition != null);
		ContainingSymbol = containingSymbol;
		Name = viewDefinition.Name();
		Id = viewDefinition.Id ?? (-1);
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, viewDefinition);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(LazyGetMembers);
		lazyMoveSymbols = ReferenceSymbolHelper.CreateLazy(() => referenceViewSymbol.LazyGetMoveSymbols(viewDefinition));
		lazyModifySymbols = ReferenceSymbolHelper.CreateLazy(() => referenceViewSymbol.LazyGetModifySymbols(viewDefinition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ImmutableArray<Symbol> LazyGetMembers()
	{
		ImmutableArray<PropertySymbol> properties = Properties;
		ImmutableArray<ChangeModifySymbol> modifySymbols = ModifySymbols;
		ImmutableArray<ChangeMoveSymbol> moveSymbols = MoveSymbols;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(properties.Length + modifySymbols.Length + moveSymbols.Length);
		instance.AddRange(properties);
		instance.AddRange(modifySymbols);
		instance.AddRange(moveSymbols);
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<ChangeModifySymbol> LazyGetModifySymbols(ViewDefinition viewDefinition)
	{
		if (viewDefinition.ControlChanges == null)
		{
			return ImmutableArray<ChangeModifySymbol>.Empty;
		}
		ArrayBuilder<ChangeModifySymbol> instance = ArrayBuilder<ChangeModifySymbol>.GetInstance();
		ExtensionControlDefinition[] controlChanges = viewDefinition.ControlChanges;
		foreach (ExtensionControlDefinition extensionControlDefinition in controlChanges)
		{
			if (extensionControlDefinition.ChangeKind.IsModify())
			{
				instance.Add(new ReferenceChangeModifyControlSymbol(this, extensionControlDefinition));
			}
		}
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<ChangeMoveSymbol> LazyGetMoveSymbols(ViewDefinition viewDefinition)
	{
		if (viewDefinition.ControlChanges == null)
		{
			return ImmutableArray<ChangeMoveSymbol>.Empty;
		}
		ArrayBuilder<ChangeMoveSymbol> instance = ArrayBuilder<ChangeMoveSymbol>.GetInstance();
		ExtensionControlDefinition[] controlChanges = viewDefinition.ControlChanges;
		foreach (ExtensionControlDefinition extensionControlDefinition in controlChanges)
		{
			if (extensionControlDefinition.ChangeKind.IsMove())
			{
				instance.Add(new ReferenceChangeMoveControlSymbol(this, extensionControlDefinition));
			}
		}
		return instance.ToImmutableAndFree();
	}
}
