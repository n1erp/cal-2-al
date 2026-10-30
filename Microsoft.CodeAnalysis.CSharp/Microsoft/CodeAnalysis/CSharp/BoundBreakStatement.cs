using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBreakStatement : BoundStatement
{
	public GeneratedLabelSymbol Label { get; }

	public BoundBreakStatement(SyntaxNode syntax, GeneratedLabelSymbol label, bool hasErrors)
		: base(BoundKind.BreakStatement, syntax, hasErrors)
	{
		Label = label;
	}

	public BoundBreakStatement(SyntaxNode syntax, GeneratedLabelSymbol label)
		: base(BoundKind.BreakStatement, syntax)
	{
		Label = label;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBreakStatement(this);
	}

	public BoundBreakStatement Update(GeneratedLabelSymbol label)
	{
		if (label != Label)
		{
			return new BoundBreakStatement(Syntax, label, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
