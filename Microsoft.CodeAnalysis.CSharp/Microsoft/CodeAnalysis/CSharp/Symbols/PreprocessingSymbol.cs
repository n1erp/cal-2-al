using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class PreprocessingSymbol : Symbol, IPreprocessingSymbol, ISymbol, IEquatable<ISymbol>
{
	private readonly string _name;

	public override string Name => _name;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Symbol.GetDeclaringSyntaxReferenceHelper<CSharpSyntaxNode>(Locations);

	public override SymbolKind Kind => SymbolKind.Preprocessing;

	public override Symbol ContainingSymbol => null;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override bool IsStatic => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	internal override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal PreprocessingSymbol(string name)
	{
		_name = name;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj == null)
		{
			return false;
		}
		if (obj is PreprocessingSymbol preprocessingSymbol)
		{
			return Name.Equals(preprocessingSymbol.Name);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		throw new NotImplementedException();
	}

	public override void Accept(SymbolVisitor visitor)
	{
		throw new NotSupportedException();
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		throw new NotSupportedException();
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		throw new NotSupportedException();
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		throw new NotSupportedException();
	}
}
