using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class MissingTypeSymbol : TypeSymbol
{
	private class MissingOwnerSymbol : Symbol
	{
		private readonly Symbol containingSymbol;

		public override SymbolKind Kind => SymbolKind.Undefined;

		public override Symbol ContainingSymbol => containingSymbol;

		public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

		internal MissingOwnerSymbol(MissingTypeSymbol parent)
		{
			containingSymbol = parent;
		}

		public override void Accept(SymbolVisitor visitor)
		{
			visitor.DefaultVisit(this);
		}

		public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
		{
			return visitor.DefaultVisit(this);
		}

		internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
		{
			return visitor.DefaultVisit(this, a);
		}
	}

	private readonly Symbol containingSymbol;

	internal static string MissingTypeSymbolName = "__MissingTypeSymbol__";

	private static readonly Lazy<MissingTypeSymbol> instance = new Lazy<MissingTypeSymbol>(() => new MissingTypeSymbol());

	internal static MissingTypeSymbol Instance => instance.Value;

	public override string Name => MissingTypeSymbolName;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Unknown;

	public override SymbolKind Kind => SymbolKind.Undefined;

	public override Symbol ContainingSymbol => containingSymbol;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	private MissingTypeSymbol()
	{
		containingSymbol = new MissingOwnerSymbol(this);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
