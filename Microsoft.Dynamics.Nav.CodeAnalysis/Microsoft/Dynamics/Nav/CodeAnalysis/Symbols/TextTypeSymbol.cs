using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TextTypeSymbol : NavTypeSymbol
{
	private readonly int length;

	private static readonly ConcurrentDictionary<int, TextTypeSymbol> textCache = new ConcurrentDictionary<int, TextTypeSymbol>();

	internal override TypeSymbol BaseType => TextClassTypeSymbol.Instance;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override int Length => length;

	public override bool HasLength => length != int.MaxValue;

	protected override Symbol OriginalSymbolDefinition => NavCorLib.TextType;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTextType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTextType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTextType(this, a);
	}

	internal static TextTypeSymbol GetInstance(NavTypeKind navTypeKind, int length = int.MaxValue)
	{
		if (navTypeKind == NavTypeKind.Text)
		{
			return textCache.GetOrAdd(length, (int l) => new TextTypeSymbol(NavTypeKind.Text, l));
		}
		throw ExceptionUtilities.UnexpectedValue(navTypeKind);
	}

	private TextTypeSymbol(NavTypeKind navTypeKind, int length = int.MaxValue)
		: base(navTypeKind)
	{
		this.length = length;
	}

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		TextTypeSymbol textTypeSymbol = t2 as TextTypeSymbol;
		if (textTypeSymbol == null)
		{
			return false;
		}
		return textTypeSymbol.length == length;
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
