using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class DotNetEventSymbol : EventSymbol, IDotNetControlAddInMemberSymbol, ISymbol, IEquatable<ISymbol>
{
	private readonly Lazy<ImmutableArray<ParameterSymbol>> lazyAlParameters;

	private readonly Lazy<ImmutableArray<ParameterSymbol>> lazyDotNetParameters;

	public override string Name { get; }

	public override ImmutableArray<ParameterSymbol> Parameters => lazyAlParameters.Value;

	public ImmutableArray<ParameterSymbol> DotNetParameters => lazyDotNetParameters.Value;

	public override ImmutableArray<AttributeSymbol> Attributes { get; } = ImmutableArray<AttributeSymbol>.Empty;

	public override Symbol ContainingSymbol { get; }

	public override Accessibility DeclaredAccessibility { get; } = Accessibility.Public;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public bool IsVisibleInControlAddIn { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetEvent(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetEvent(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetEvent(this, a);
	}

	public DotNetEventSymbol(Symbol containingSymbol, string name, Func<Symbol, ImmutableArray<ParameterSymbol>> getDotNetParameters, Func<ImmutableArray<ParameterSymbol>, ImmutableArray<ParameterSymbol>> mapDotNetToAlParameters, bool isVisibleInControlAddIn)
	{
		DotNetEventSymbol dotNetEventSymbol = this;
		ContainingSymbol = containingSymbol;
		Name = name;
		IsVisibleInControlAddIn = isVisibleInControlAddIn;
		lazyDotNetParameters = new Lazy<ImmutableArray<ParameterSymbol>>(() => getDotNetParameters(dotNetEventSymbol));
		lazyAlParameters = new Lazy<ImmutableArray<ParameterSymbol>>(() => mapDotNetToAlParameters(dotNetEventSymbol.DotNetParameters));
	}
}
