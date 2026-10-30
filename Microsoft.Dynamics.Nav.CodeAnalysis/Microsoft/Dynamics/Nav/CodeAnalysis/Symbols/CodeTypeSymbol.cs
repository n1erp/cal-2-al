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
internal class CodeTypeSymbol : NavTypeSymbol
{
	private readonly int length;

	private static readonly ConcurrentDictionary<int, CodeTypeSymbol> textCache = new ConcurrentDictionary<int, CodeTypeSymbol>();

	public override int Length => length;

	public override bool HasLength => true;

	protected override Symbol OriginalSymbolDefinition => NavCorLib.CodeType;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitCodeType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitCodeType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitCodeType(this, a);
	}

	internal static CodeTypeSymbol GetInstance(NavTypeKind navTypeKind, int length = int.MaxValue)
	{
		if (navTypeKind == NavTypeKind.Code)
		{
			return textCache.GetOrAdd(length, (int l) => new CodeTypeSymbol(NavTypeKind.Code, l));
		}
		throw ExceptionUtilities.UnexpectedValue(navTypeKind);
	}

	private CodeTypeSymbol(NavTypeKind navTypeKind, int length = int.MaxValue)
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
		CodeTypeSymbol codeTypeSymbol = t2 as CodeTypeSymbol;
		if (codeTypeSymbol == null)
		{
			return false;
		}
		return codeTypeSymbol.length == length;
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
