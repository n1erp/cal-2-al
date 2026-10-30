using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BuiltInParameterSymbol : ParameterSymbol
{
	private readonly Lazy<TypeSymbol> lazyType;

	private readonly TypeSymbol eagerType;

	public override TypeSymbol Type => eagerType ?? lazyType.Value;

	public override string Name { get; }

	public override int Ordinal { get; }

	public override bool IsVar { get; }

	public override bool IsOptional { get; }

	public override bool IsParams { get; }

	public override bool IsMember { get; }

	public override bool IsMemberReference { get; }

	public override bool IsArray { get; }

	public override bool TypeFromFirstArgument { get; }

	public override bool IsSameVar { get; }

	public override bool AddExpectedType { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitBuiltInParameter(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitBuiltInParameter(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitBuiltInParameter(this, a);
	}

	public BuiltInParameterSymbol(Symbol containingSymbol, TypeSymbol type, string name, int ordinal, bool isVar = false, bool isOptional = false, bool isParams = false, bool isMember = false, bool isMemberReference = false, bool typeFromFirstArgument = false, bool isArray = false, bool isSameVar = false, bool addExpectedType = false)
		: this(containingSymbol, () => type, name, ordinal, isVar, isOptional, isParams, isMember, isMemberReference, typeFromFirstArgument, isArray, isSameVar, addExpectedType)
	{
		eagerType = (isArray ? ArrayTypeSymbol.Create(type, ImmutableArray<int>.Empty) : type);
	}

	public BuiltInParameterSymbol(Symbol containingSymbol, Func<TypeSymbol> typeConstructor, string name, int ordinal, bool isVar = false, bool isOptional = false, bool isParams = false, bool isMember = false, bool isMemberReference = false, bool typeFromFirstArgument = false, bool isArray = false, bool isSameVar = false, bool addExpectedType = false)
		: this(containingSymbol, name, ordinal, isVar, isOptional, isParams, isMember, isMemberReference, typeFromFirstArgument, isArray, isSameVar, addExpectedType)
	{
		DebugAssertHelper.Assert(typeConstructor != null);
		lazyType = new Lazy<TypeSymbol>(() => (!isArray) ? typeConstructor() : ArrayTypeSymbol.Create(typeConstructor(), ImmutableArray<int>.Empty));
	}

	private BuiltInParameterSymbol(Symbol containingSymbol, string name, int ordinal, bool isVar = false, bool isOptional = false, bool isParams = false, bool isMember = false, bool isMemberReference = false, bool typeFromFirstArgument = false, bool isArray = false, bool isSameVar = false, bool addExpectedType = false)
	{
		ContainingSymbol = containingSymbol;
		Name = name;
		Ordinal = ordinal;
		IsVar = isVar;
		IsOptional = isOptional;
		IsParams = isParams;
		IsMember = isMember;
		IsMemberReference = isMemberReference;
		TypeFromFirstArgument = typeFromFirstArgument;
		IsArray = isArray;
		IsSameVar = isSameVar;
		AddExpectedType = addExpectedType;
	}

	public override string GetDocumentation()
	{
		return DocumentationFacts.GetDocumentation(this);
	}
}
