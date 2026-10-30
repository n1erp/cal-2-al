using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class LabelSymbol : Symbol, ILabelSymbol, ISymbol, IEquatable<ISymbol>
{
	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw new NotSupportedException();
		}
	}

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

	IMethodSymbol ILabelSymbol.ContainingMethod => ContainingMethod;

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLabel(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitLabel(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitLabel(this);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitLabel(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitLabel(this);
	}
}
