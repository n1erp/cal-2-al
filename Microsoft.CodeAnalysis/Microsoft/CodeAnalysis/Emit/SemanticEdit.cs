using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

public struct SemanticEdit : IEquatable<SemanticEdit>
{
	public SemanticEditKind Kind { get; }

	public ISymbol OldSymbol { get; }

	public ISymbol NewSymbol { get; }

	public Func<SyntaxNode, SyntaxNode> SyntaxMap { get; }

	public bool PreserveLocalVariables { get; }

	public SemanticEdit(SemanticEditKind kind, ISymbol oldSymbol, ISymbol newSymbol, Func<SyntaxNode, SyntaxNode> syntaxMap = null, bool preserveLocalVariables = false)
	{
		if (oldSymbol == null && kind != SemanticEditKind.Insert)
		{
			throw new ArgumentNullException("oldSymbol");
		}
		if (newSymbol == null && kind != SemanticEditKind.Delete)
		{
			throw new ArgumentNullException("newSymbol");
		}
		if (kind <= SemanticEditKind.None || kind > SemanticEditKind.Delete)
		{
			throw new ArgumentOutOfRangeException("kind");
		}
		Kind = kind;
		OldSymbol = oldSymbol;
		NewSymbol = newSymbol;
		PreserveLocalVariables = preserveLocalVariables;
		SyntaxMap = syntaxMap;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(OldSymbol, Hash.Combine(NewSymbol, (int)Kind));
	}

	public override bool Equals(object obj)
	{
		if (obj is SemanticEdit)
		{
			return Equals((SemanticEdit)obj);
		}
		return false;
	}

	public bool Equals(SemanticEdit other)
	{
		if (Kind == other.Kind && ((OldSymbol == null) ? (other.OldSymbol == null) : OldSymbol.Equals(other.OldSymbol)))
		{
			if (NewSymbol != null)
			{
				return NewSymbol.Equals(other.NewSymbol);
			}
			return other.NewSymbol == null;
		}
		return false;
	}
}
