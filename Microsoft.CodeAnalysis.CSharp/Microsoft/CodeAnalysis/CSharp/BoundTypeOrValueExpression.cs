using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTypeOrValueExpression : BoundExpression
{
	public BoundTypeOrValueData Data { get; }

	public BoundTypeOrValueExpression(SyntaxNode syntax, BoundTypeOrValueData data, TypeSymbol type, bool hasErrors)
		: base(BoundKind.TypeOrValueExpression, syntax, type, hasErrors)
	{
		Data = data;
	}

	public BoundTypeOrValueExpression(SyntaxNode syntax, BoundTypeOrValueData data, TypeSymbol type)
		: base(BoundKind.TypeOrValueExpression, syntax, type)
	{
		Data = data;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTypeOrValueExpression(this);
	}

	public BoundTypeOrValueExpression Update(BoundTypeOrValueData data, TypeSymbol type)
	{
		if (data != Data || type != Type)
		{
			return new BoundTypeOrValueExpression(Syntax, data, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
