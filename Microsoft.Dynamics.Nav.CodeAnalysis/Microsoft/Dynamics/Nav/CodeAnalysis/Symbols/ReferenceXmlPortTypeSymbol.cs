using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceXmlPortTypeSymbol : XmlPortTypeSymbol
{
	private readonly Lazy<ImmutableArray<VariableSymbol>> lazyGlobalVariables;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	public override int Id { get; }

	public override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => lazyGlobalVariables.Value;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public ReferenceXmlPortTypeSymbol(ModuleSymbol module, XmlPortDefinition xmlport)
		: base(module)
	{
		ReferenceXmlPortTypeSymbol referenceXmlPortTypeSymbol = this;
		DebugAssertHelper.Assert(xmlport != null);
		Id = xmlport.Id ?? (-1);
		Name = xmlport.Name().UnquoteIdentifier();
		lazyGlobalVariables = ReferenceSymbolHelper.BuildGlobalVariablesLazy(this, xmlport);
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, xmlport);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceXmlPortTypeSymbol.LazyGetMembers(xmlport));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ImmutableArray<Symbol> LazyGetMembers(XmlPortDefinition xmlport)
	{
		int lengthSafe = xmlport.Methods.GetLengthSafe();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(GlobalVariables.Length + Properties.Length + lengthSafe);
		if (lengthSafe > 0)
		{
			MethodDefinition[] methods = xmlport.Methods;
			foreach (MethodDefinition method in methods)
			{
				instance.Add(new ReferenceMethodSymbol(this, method));
			}
		}
		instance.AddRange(GlobalVariables);
		instance.AddRange(Properties);
		return instance.ToImmutableAndFree();
	}
}
