using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSourceDocumentIndex : BoundExpression
{
	public DebugSourceDocument Document { get; }

	public BoundSourceDocumentIndex(SyntaxNode syntax, DebugSourceDocument document, TypeSymbol type, bool hasErrors)
		: base(BoundKind.SourceDocumentIndex, syntax, type, hasErrors)
	{
		Document = document;
	}

	public BoundSourceDocumentIndex(SyntaxNode syntax, DebugSourceDocument document, TypeSymbol type)
		: base(BoundKind.SourceDocumentIndex, syntax, type)
	{
		Document = document;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSourceDocumentIndex(this);
	}

	public BoundSourceDocumentIndex Update(DebugSourceDocument document, TypeSymbol type)
	{
		if (document != Document || type != Type)
		{
			return new BoundSourceDocumentIndex(Syntax, document, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
