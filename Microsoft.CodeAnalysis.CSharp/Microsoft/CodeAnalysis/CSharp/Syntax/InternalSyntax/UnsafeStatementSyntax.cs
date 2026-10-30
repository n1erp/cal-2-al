using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class UnsafeStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken unsafeKeyword;

	internal readonly BlockSyntax block;

	public SyntaxToken UnsafeKeyword => unsafeKeyword;

	public BlockSyntax Block => block;

	internal UnsafeStatementSyntax(SyntaxKind kind, SyntaxToken unsafeKeyword, BlockSyntax block, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(unsafeKeyword);
		this.unsafeKeyword = unsafeKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal UnsafeStatementSyntax(SyntaxKind kind, SyntaxToken unsafeKeyword, BlockSyntax block, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(unsafeKeyword);
		this.unsafeKeyword = unsafeKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal UnsafeStatementSyntax(SyntaxKind kind, SyntaxToken unsafeKeyword, BlockSyntax block)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(unsafeKeyword);
		this.unsafeKeyword = unsafeKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)unsafeKeyword, 
			1 => block, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.UnsafeStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUnsafeStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUnsafeStatement(this);
	}

	public UnsafeStatementSyntax Update(SyntaxToken unsafeKeyword, BlockSyntax block)
	{
		if (unsafeKeyword != UnsafeKeyword || block != Block)
		{
			UnsafeStatementSyntax unsafeStatementSyntax = SyntaxFactory.UnsafeStatement(unsafeKeyword, block);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				unsafeStatementSyntax = unsafeStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				unsafeStatementSyntax = unsafeStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return unsafeStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new UnsafeStatementSyntax(Kind, unsafeKeyword, block, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new UnsafeStatementSyntax(Kind, unsafeKeyword, block, GetDiagnostics(), annotations);
	}

	internal UnsafeStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			unsafeKeyword = syntaxToken;
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
		writer.WriteValue(unsafeKeyword);
		writer.WriteValue(block);
	}

	static UnsafeStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(UnsafeStatementSyntax), (ObjectReader r) => new UnsafeStatementSyntax(r));
	}
}
