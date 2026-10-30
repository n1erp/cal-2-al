using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ChangeSymbol : ContainerSymbol, IChangeSymbol, ISymbol, IEquatable<ISymbol>
{
	public abstract ChangeKind ChangeKind { get; }

	public abstract SymbolKind ChangeTargetKind { get; }

	public override SymbolKind Kind => SymbolKind.Change;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitChange(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitChange(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitChange(this, a);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	protected static string MakeChangeName()
	{
		return Guid.NewGuid().ToString();
	}
}
