using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class ArrayTypeSymbol : TypeSymbol
{
	public TypeSymbol ElementType { get; }

	public ImmutableArray<int> Dimensions { get; }

	public override SymbolKind Kind => SymbolKind.ArrayType;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Array;

	public override NavTypeKind NavTypeKind => NavTypeKind.Array;

	public override Symbol ContainingSymbol => null;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override int Length => 0;

	public override bool HasLength => false;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitArrayType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitArrayType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitArrayType(this, a);
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private ArrayTypeSymbol(TypeSymbol elementType, ImmutableArray<int> dimensions)
	{
		DebugAssertHelper.Assert((object)elementType != null);
		DebugAssertHelper.Assert((object)dimensions != null);
		ElementType = elementType;
		Dimensions = dimensions;
	}

	internal static ArrayTypeSymbol Create(TypeSymbol elementType, ImmutableArray<int> dimensions)
	{
		return new ArrayTypeSymbol(elementType, dimensions);
	}

	internal bool HasSameShapeAs(ArrayTypeSymbol other)
	{
		if (Dimensions.Length == other.Dimensions.Length)
		{
			return Dimensions.SequenceEqual(other.Dimensions);
		}
		return false;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override bool Equals(TypeSymbol t2)
	{
		ArrayTypeSymbol arrayTypeSymbol = t2 as ArrayTypeSymbol;
		if (arrayTypeSymbol == null)
		{
			return false;
		}
		return Equals(arrayTypeSymbol);
	}

	public bool Equals(ArrayTypeSymbol other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (ElementType.Equals(other.ElementType))
		{
			return HasSameShapeAs(other);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		ArrayTypeSymbol arrayTypeSymbol = obj as ArrayTypeSymbol;
		if (arrayTypeSymbol != null)
		{
			return Equals(arrayTypeSymbol);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int h = 0;
		for (int i = 0; i < Dimensions.Length; i++)
		{
			h = Hash.Combine(h, Dimensions[i].GetHashCode());
		}
		return Hash.Combine(h, ElementType.GetHashCode());
	}
}
