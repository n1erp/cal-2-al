using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class FinallyClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken finallyKeyword;

	internal readonly BlockSyntax block;

	public SyntaxToken FinallyKeyword => finallyKeyword;

	public BlockSyntax Block => block;

	internal FinallyClauseSyntax(SyntaxKind kind, SyntaxToken finallyKeyword, BlockSyntax block, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(finallyKeyword);
		this.finallyKeyword = finallyKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal FinallyClauseSyntax(SyntaxKind kind, SyntaxToken finallyKeyword, BlockSyntax block, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(finallyKeyword);
		this.finallyKeyword = finallyKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal FinallyClauseSyntax(SyntaxKind kind, SyntaxToken finallyKeyword, BlockSyntax block)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(finallyKeyword);
		this.finallyKeyword = finallyKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)finallyKeyword, 
			1 => block, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.FinallyClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFinallyClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFinallyClause(this);
	}

	public FinallyClauseSyntax Update(SyntaxToken finallyKeyword, BlockSyntax block)
	{
		if (finallyKeyword != FinallyKeyword || block != Block)
		{
			FinallyClauseSyntax finallyClauseSyntax = SyntaxFactory.FinallyClause(finallyKeyword, block);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				finallyClauseSyntax = finallyClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				finallyClauseSyntax = finallyClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return finallyClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new FinallyClauseSyntax(Kind, finallyKeyword, block, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new FinallyClauseSyntax(Kind, finallyKeyword, block, GetDiagnostics(), annotations);
	}

	internal FinallyClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			finallyKeyword = syntaxToken;
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
		writer.WriteValue(finallyKeyword);
		writer.WriteValue(block);
	}

	static FinallyClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(FinallyClauseSyntax), (ObjectReader r) => new FinallyClauseSyntax(r));
	}
}
