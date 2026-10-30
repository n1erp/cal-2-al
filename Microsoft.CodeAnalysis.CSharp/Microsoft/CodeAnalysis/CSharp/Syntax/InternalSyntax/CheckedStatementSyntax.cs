using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CheckedStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly BlockSyntax block;

	public SyntaxToken Keyword => keyword;

	public BlockSyntax Block => block;

	internal CheckedStatementSyntax(SyntaxKind kind, SyntaxToken keyword, BlockSyntax block, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal CheckedStatementSyntax(SyntaxKind kind, SyntaxToken keyword, BlockSyntax block, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal CheckedStatementSyntax(SyntaxKind kind, SyntaxToken keyword, BlockSyntax block)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)keyword, 
			1 => block, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CheckedStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCheckedStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCheckedStatement(this);
	}

	public CheckedStatementSyntax Update(SyntaxToken keyword, BlockSyntax block)
	{
		if (keyword != Keyword || block != Block)
		{
			CheckedStatementSyntax checkedStatementSyntax = SyntaxFactory.CheckedStatement(Kind, keyword, block);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				checkedStatementSyntax = checkedStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				checkedStatementSyntax = checkedStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return checkedStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CheckedStatementSyntax(Kind, keyword, block, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CheckedStatementSyntax(Kind, keyword, block, GetDiagnostics(), annotations);
	}

	internal CheckedStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			keyword = syntaxToken;
		}
		BlockSyntax blockSyntax = (BlockSyntax)reader.ReadValue();
		if (blockSyntax != null)
		{
			AdjustFlagsAndWidth(blockSyntax);
			block = blockSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(keyword);
		writer.WriteValue(block);
	}

	static CheckedStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CheckedStatementSyntax), (ObjectReader r) => new CheckedStatementSyntax(r));
	}
}
