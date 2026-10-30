using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BuiltInReturnValueSymbol : ReturnValueSymbol
{
	private readonly Lazy<TypeSymbol> lazyType;

	private readonly TypeSymbol eagerType;

	public override bool IsNamed => false;

	public override bool IsOptional { get; }

	public override bool TypeFromFirstArgument { get; }

	public override TypeSymbol Type => eagerType ?? lazyType.Value;

	public override Symbol ContainingSymbol => null;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitBuiltInReturnValue(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitBuiltInReturnValue(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitBuiltInReturnValue(this, a);
	}

	public BuiltInReturnValueSymbol(TypeSymbol type, bool isOptional = false, bool typeFromFirstArgument = false)
	{
		eagerType = type;
		IsOptional = isOptional;
		TypeFromFirstArgument = typeFromFirstArgument;
	}

	public BuiltInReturnValueSymbol(Func<TypeSymbol> type, bool isOptional = false, bool typeFromFirstArgument = false)
	{
		lazyType = new Lazy<TypeSymbol>(type);
		IsOptional = isOptional;
		TypeFromFirstArgument = typeFromFirstArgument;
	}
}
