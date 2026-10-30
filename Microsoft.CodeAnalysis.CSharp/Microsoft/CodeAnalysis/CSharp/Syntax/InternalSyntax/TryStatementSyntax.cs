using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TryStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken tryKeyword;

	internal readonly BlockSyntax block;

	internal readonly GreenNode catches;

	internal readonly FinallyClauseSyntax @finally;

	public SyntaxToken TryKeyword => tryKeyword;

	public BlockSyntax Block => block;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax> Catches => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax>(catches);

	public FinallyClauseSyntax Finally => @finally;

	internal TryStatementSyntax(SyntaxKind kind, SyntaxToken tryKeyword, BlockSyntax block, GreenNode catches, FinallyClauseSyntax @finally, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(tryKeyword);
		this.tryKeyword = tryKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (catches != null)
		{
			AdjustFlagsAndWidth(catches);
			this.catches = catches;
		}
		if (@finally != null)
		{
			AdjustFlagsAndWidth(@finally);
			this.@finally = @finally;
		}
	}

	internal TryStatementSyntax(SyntaxKind kind, SyntaxToken tryKeyword, BlockSyntax block, GreenNode catches, FinallyClauseSyntax @finally, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(tryKeyword);
		this.tryKeyword = tryKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (catches != null)
		{
			AdjustFlagsAndWidth(catches);
			this.catches = catches;
		}
		if (@finally != null)
		{
			AdjustFlagsAndWidth(@finally);
			this.@finally = @finally;
		}
	}

	internal TryStatementSyntax(SyntaxKind kind, SyntaxToken tryKeyword, BlockSyntax block, GreenNode catches, FinallyClauseSyntax @finally)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(tryKeyword);
		this.tryKeyword = tryKeyword;
		AdjustFlagsAndWidth(block);
		this.block = block;
		if (catches != null)
		{
			AdjustFlagsAndWidth(catches);
			this.catches = catches;
		}
		if (@finally != null)
		{
			AdjustFlagsAndWidth(@finally);
			this.@finally = @finally;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => tryKeyword, 
			1 => block, 
			2 => catches, 
			3 => @finally, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTryStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTryStatement(this);
	}

	public TryStatementSyntax Update(SyntaxToken tryKeyword, BlockSyntax block, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CatchClauseSyntax> catches, FinallyClauseSyntax @finally)
	{
		if (tryKeyword != TryKeyword || block != Block || catches != Catches || @finally != Finally)
		{
			TryStatementSyntax tryStatementSyntax = SyntaxFactory.TryStatement(tryKeyword, block, catches, @finally);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				tryStatementSyntax = tryStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				tryStatementSyntax = tryStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return tryStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TryStatementSyntax(Kind, tryKeyword, block, catches, @finally, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TryStatementSyntax(Kind, tryKeyword, block, catches, @finally, GetDiagnostics(), annotations);
	}

	internal TryStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			tryKeyword = syntaxToken;
		}
		BlockSyntax blockSyntax = (BlockSyntax)reader.ReadValue();
		if (blockSyntax != null)
		{
			AdjustFlagsAndWidth(blockSyntax);
			block = blockSyntax;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			catches = greenNode;
		}
		FinallyClauseSyntax finallyClauseSyntax = (FinallyClauseSyntax)reader.ReadValue();
		if (finallyClauseSyntax != null)
		{
			AdjustFlagsAndWidth(finallyClauseSyntax);
			@finally = finallyClauseSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(tryKeyword);
		writer.WriteValue(block);
		writer.WriteValue(catches);
		writer.WriteValue(@finally);
	}

	static TryStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TryStatementSyntax), (ObjectReader r) => new TryStatementSyntax(r));
	}
}
