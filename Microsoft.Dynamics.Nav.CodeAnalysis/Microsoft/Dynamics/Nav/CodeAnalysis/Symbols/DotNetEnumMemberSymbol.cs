using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class DotNetEnumMemberSymbol : OptionSymbol, IDotNetTypeSymbol, IRootTypeSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId
{
	public override Accessibility DeclaredAccessibility { get; } = Accessibility.Public;

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	public override int Ordinal { get; }

	public DotNetTypeInfo DotNetTypeInfo { get; }

	public int Id { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetEnumMember(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetEnumMember(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetEnumMember(this, a);
	}

	internal DotNetEnumMemberSymbol(DotNetTypeSymbol owner, string name, long ordinal, int memberId)
	{
		DotNetTypeInfo = owner.DotNetTypeInfo;
		ContainingSymbol = owner;
		Name = name;
		Ordinal = (int)ordinal;
		Type = owner;
		Id = memberId;
	}
}
