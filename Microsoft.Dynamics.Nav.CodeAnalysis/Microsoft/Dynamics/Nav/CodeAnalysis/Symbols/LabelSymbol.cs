using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class LabelSymbol : Symbol
{
	public override string Name { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	internal virtual SyntaxNodeOrToken IdentifierNodeOrToken => default;

	public virtual MethodSymbol ContainingMethod
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override Symbol ContainingSymbol
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override SymbolKind Kind => SymbolKind.Label;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitLabel(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitLabel(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitLabel(this, a);
	}

	internal LabelSymbol(string name)
	{
		Name = name;
	}
}
