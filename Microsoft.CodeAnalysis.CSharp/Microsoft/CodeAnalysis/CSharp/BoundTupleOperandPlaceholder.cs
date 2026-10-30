using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTupleOperandPlaceholder : BoundValuePlaceholderBase
{
	public BoundTupleOperandPlaceholder(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.TupleOperandPlaceholder, syntax, type, hasErrors)
	{
	}

	public BoundTupleOperandPlaceholder(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.TupleOperandPlaceholder, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTupleOperandPlaceholder(this);
	}

	public BoundTupleOperandPlaceholder Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundTupleOperandPlaceholder(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
