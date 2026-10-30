using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceControlSymbol : ControlSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<ControlSymbol>> lazyControls;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<ActionSymbol>> lazyActions;

	private readonly Lazy<PageTypeSymbol> lazyPagePartSymbol;

	private readonly Lazy<TypeSymbol> lazyType;

	private readonly Lazy<ControlAddInTypeSymbol> lazyControlAddInSymbol;

	private readonly Lazy<FieldSymbol> lazyFieldSymbol;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol { get; }

	public override ControlKind ControlKind { get; }

	public override int Id { get; }

	public override string Name { get; }

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<ControlSymbol> Controls => lazyControls.Value;

	public override ImmutableArray<ActionSymbol> Actions => lazyActions.Value;

	public override ImmutableArray<MethodSymbol> Triggers { get; } = ImmutableArray<MethodSymbol>.Empty;

	public override TypeSymbol Type => lazyType.Value;

	public override PageTypeSymbol RelatedPartSymbol => lazyPagePartSymbol.Value;

	public override ControlAddInTypeSymbol RelatedControlAddInSymbol => lazyControlAddInSymbol.Value;

	public override FieldSymbol RelatedFieldSymbol => lazyFieldSymbol.Value;

	internal ReferenceControlSymbol(Symbol containingSymbol, ControlDefinition controlDefinition)
	{
		ReferenceControlSymbol referenceControlSymbol = this;
		DebugAssertHelper.Assert(controlDefinition != null);
		ContainingSymbol = containingSymbol;
		Name = controlDefinition.Name();
		Id = controlDefinition.Id ?? (-1);
		ControlKind = controlDefinition.Kind;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, controlDefinition);
		lazyActions = ReferenceSymbolHelper.CreateLazy(() => controlDefinition.Actions.AsImmutableOrEmpty((Func<ActionDefinition, ActionSymbol>)((ActionDefinition a) => new ReferenceActionSymbol(referenceControlSymbol, a))));
		lazyControls = ReferenceSymbolHelper.CreateLazy(() => controlDefinition.Controls.AsImmutableOrEmpty((Func<ControlDefinition, ControlSymbol>)((ControlDefinition c) => new ReferenceControlSymbol(referenceControlSymbol, c))));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(LazyGetMembers);
		lazyFieldSymbol = ReferenceSymbolHelper.CreateLazy(GetRelatedFieldSymbolLazy);
		lazyControlAddInSymbol = ReferenceSymbolHelper.CreateLazy(() => referenceControlSymbol.GetRelatedControlAddInSymbolLazy(controlDefinition));
		lazyType = ReferenceSymbolHelper.CreateLazy(() => referenceControlSymbol.GetTypeLazy(controlDefinition));
		lazyPagePartSymbol = ReferenceSymbolHelper.CreateLazy(() => referenceControlSymbol.GetPagePartSymbolLazy(controlDefinition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ControlAddInTypeSymbol GetRelatedControlAddInSymbolLazy(ControlDefinition controlDefinition)
	{
		if (ControlKind != ControlKind.UserControl)
		{
			return null;
		}
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = ContainingModule.GetObjectSymbolsByNameAcrossModules(SymbolKind.ControlAddIn, controlDefinition.RelatedControlAddIn);
		if (objectSymbolsByNameAcrossModules.Length == 1)
		{
			return objectSymbolsByNameAcrossModules[0] as ControlAddInTypeSymbol;
		}
		return null;
	}

	private TypeSymbol GetTypeLazy(ControlDefinition controlDefinition)
	{
		TypeSymbol typeSymbol = SerializableSymbolModelConverter.ResolveType(ContainingModule, this, controlDefinition.GetTypeDefinition());
		if ((object)typeSymbol == MissingTypeSymbol.Instance)
		{
			return NavCorLib.None;
		}
		return typeSymbol;
	}

	private PageTypeSymbol GetPagePartSymbolLazy(ControlDefinition controlDefinition)
	{
		if (controlDefinition.RelatedPagePartId != null && ControlKind == ControlKind.Part)
		{
			TypeDefinition typeDefinition = new TypeDefinition
			{
				Name = "Page",
				Subtype = controlDefinition.RelatedPagePartId
			};
			return SerializableSymbolModelConverter.ResolveType(ContainingModule, this, typeDefinition) as PageTypeSymbol;
		}
		return null;
	}

	private FieldSymbol GetRelatedFieldSymbolLazy()
	{
		TableTypeSymbol tableTypeSymbol = (TableTypeSymbol)this.GetContainingPage().RelatedTable;
		if (tableTypeSymbol == null)
		{
			return null;
		}
		PropertySymbol sourceExpression = Properties.Where((PropertySymbol p) => p.Name.Equals("SourceExpression", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
		return (from f in tableTypeSymbol.GetFields()
			where f.Name.Equals(sourceExpression.ValueText, StringComparison.OrdinalIgnoreCase)
			select f).FirstOrDefault();
	}

	private ImmutableArray<Symbol> LazyGetMembers()
	{
		ImmutableArray<PropertySymbol> properties = Properties;
		ImmutableArray<ActionSymbol> actions = Actions;
		ImmutableArray<ControlSymbol> controls = Controls;
		ImmutableArray<MethodSymbol> triggers = Triggers;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(properties.Length + actions.Length + controls.Length + triggers.Length);
		instance.AddRange(properties);
		instance.AddRange(actions);
		instance.AddRange(controls);
		instance.AddRange(triggers);
		if (RelatedPartSymbol != null)
		{
			instance.AddRange(new SynthesizedPropertySymbol(this, "Page", PageReferenceTypeSymbol.Create(RelatedPartSymbol)));
		}
		return instance.ToImmutableAndFree();
	}
}
