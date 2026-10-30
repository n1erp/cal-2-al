using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLabeledStatement : BoundStatement
{
	public LabelSymbol Label { get; }

	public BoundStatement Body { get; }

	public BoundLabeledStatement(SyntaxNode syntax, LabelSymbol label, BoundStatement body, bool hasErrors = false)
		: base(BoundKind.LabeledStatement, syntax, hasErrors || body.HasErrors())
	{
		Label = label;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLabeledStatement(this);
	}

	public BoundLabeledStatement Update(LabelSymbol label, BoundStatement body)
	{
		if (label != Label || body != Body)
		{
			return new BoundLabeledStatement(Syntax, label, body, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
