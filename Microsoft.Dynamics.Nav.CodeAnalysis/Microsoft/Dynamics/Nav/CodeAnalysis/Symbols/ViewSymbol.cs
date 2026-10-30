using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ViewSymbol : TypeSymbol, IViewSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>
{
	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public abstract int Id { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.PageViewProperties;

	internal abstract ImmutableArray<ChangeModifySymbol> ModifySymbols { get; }

	internal abstract ImmutableArray<ChangeMoveSymbol> MoveSymbols { get; }

	public override SymbolKind Kind => SymbolKind.View;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitView(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitView(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitView(this, a);
	}
}
