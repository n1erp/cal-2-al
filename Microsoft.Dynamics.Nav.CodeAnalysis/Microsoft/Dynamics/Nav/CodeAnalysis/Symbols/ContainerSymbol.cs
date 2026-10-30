using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class ContainerSymbol : Symbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>
{
	private Dictionary<string, ImmutableArray<Symbol>> membersByName;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitContainer(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitContainer(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitContainer(this, a);
	}

	public abstract ImmutableArray<Symbol> GetMembers();

	internal virtual ImmutableArray<Symbol> GetMembersUnordered()
	{
		return GetMembers();
	}

	public virtual ImmutableArray<Symbol> GetMembers(string name)
	{
		if (membersByName == null)
		{
			Dictionary<string, ImmutableArray<Symbol>> value = GetMembers().ToDictionaryOfArrays((Symbol m) => m.Name, SemanticFacts.NameEqualityComparer);
			Interlocked.CompareExchange(ref membersByName, value, null);
		}
		if (membersByName.TryGetValue(name, out var value2))
		{
			return value2;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	internal virtual ImmutableArray<Symbol> GetCodeMembers()
	{
		return GetMembers();
	}

	internal virtual ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetMembers(name);
	}

	ImmutableArray<ISymbol> IContainerSymbol.GetMembers()
	{
		return GetMembers().CastArray<ISymbol>();
	}

	ImmutableArray<ISymbol> IContainerSymbol.GetMembers(string name)
	{
		return GetMembers(name).CastArray<ISymbol>();
	}

	protected ImmutableArray<Symbol> GetCodeMembers(ref Dictionary<string, ImmutableArray<Symbol>> codeMembersByName, string name)
	{
		if (codeMembersByName == null)
		{
			Dictionary<string, ImmutableArray<Symbol>> value = GetCodeMembers().ToDictionaryOfArrays((Symbol m) => m.Name, SemanticFacts.NameEqualityComparer);
			Interlocked.CompareExchange(ref codeMembersByName, value, null);
		}
		if (codeMembersByName.TryGetValue(name, out var value2))
		{
			return value2;
		}
		return ImmutableArray<Symbol>.Empty;
	}
}
