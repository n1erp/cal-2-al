using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundContinueStatement : BoundStatement
{
	public GeneratedLabelSymbol Label { get; }

	public BoundContinueStatement(SyntaxNode syntax, GeneratedLabelSymbol label, bool hasErrors)
		: base(BoundKind.ContinueStatement, syntax, hasErrors)
	{
		Label = label;
	}

	public BoundContinueStatement(SyntaxNode syntax, GeneratedLabelSymbol label)
		: base(BoundKind.ContinueStatement, syntax)
	{
		Label = label;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitContinueStatement(this);
	}

	public BoundContinueStatement Update(GeneratedLabelSymbol label)
	{
		if (label != Label)
		{
			return new BoundContinueStatement(Syntax, label, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
