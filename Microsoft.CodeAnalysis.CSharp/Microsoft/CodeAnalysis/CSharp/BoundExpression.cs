using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundExpression : BoundNode
{
	public virtual ConstantValue ConstantValue => null;

	public virtual Symbol ExpressionSymbol => null;

	public virtual LookupResultKind ResultKind => LookupResultKind.Viable;

	public virtual bool SuppressVirtualCalls => false;

	public virtual object Display => Type;

	public TypeSymbol Type { get; }

	protected BoundExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(kind, syntax, hasErrors)
	{
		Type = type;
	}

	protected BoundExpression(BoundKind kind, SyntaxNode syntax, TypeSymbol type)
		: base(kind, syntax)
	{
		Type = type;
	}
}
