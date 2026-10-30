using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class DiscardSymbol : Symbol, IDiscardSymbol, ISymbol, IEquatable<ISymbol>
{
	ITypeSymbol IDiscardSymbol.Type => Type;

	public TypeSymbol Type { get; }

	public override Symbol ContainingSymbol => null;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsAbstract => false;

	public override bool IsExtern => false;

	public override bool IsImplicitlyDeclared => true;

	public override bool IsOverride => false;

	public override bool IsSealed => false;

	public override bool IsStatic => false;

	public override bool IsVirtual => false;

	public override SymbolKind Kind => SymbolKind.Discard;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	internal override ObsoleteAttributeData ObsoleteAttributeData => null;

	public DiscardSymbol(TypeSymbol type)
	{
		Type = type;
	}

	internal static DiscardSymbol CreateForTest(ITypeSymbol type)
	{
		return new DiscardSymbol((TypeSymbol)type);
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDiscard(this, a);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDiscard(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDiscard(this);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitDiscard(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDiscard(this);
	}

	public override bool Equals(object obj)
	{
		if (obj is DiscardSymbol discardSymbol)
		{
			return Type.Equals(discardSymbol.Type);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Type.GetHashCode();
	}
}
