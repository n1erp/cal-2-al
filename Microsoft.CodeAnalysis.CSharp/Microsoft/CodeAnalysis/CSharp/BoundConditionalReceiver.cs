using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConditionalReceiver : BoundExpression
{
	public int Id { get; }

	public BoundConditionalReceiver(SyntaxNode syntax, int id, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ConditionalReceiver, syntax, type, hasErrors)
	{
		Id = id;
	}

	public BoundConditionalReceiver(SyntaxNode syntax, int id, TypeSymbol type)
		: base(BoundKind.ConditionalReceiver, syntax, type)
	{
		Id = id;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConditionalReceiver(this);
	}

	public BoundConditionalReceiver Update(int id, TypeSymbol type)
	{
		if (id != Id || type != Type)
		{
			return new BoundConditionalReceiver(Syntax, id, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
