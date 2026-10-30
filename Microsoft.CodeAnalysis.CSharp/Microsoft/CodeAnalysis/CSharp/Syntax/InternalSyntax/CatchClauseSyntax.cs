using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CatchClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken catchKeyword;

	internal readonly CatchDeclarationSyntax declaration;

	internal readonly CatchFilterClauseSyntax filter;

	internal readonly BlockSyntax block;

	public SyntaxToken CatchKeyword => catchKeyword;

	public CatchDeclarationSyntax Declaration => declaration;

	public CatchFilterClauseSyntax Filter => filter;

	public BlockSyntax Block => block;

	internal CatchClauseSyntax(SyntaxKind kind, SyntaxToken catchKeyword, CatchDeclarationSyntax declaration, CatchFilterClauseSyntax filter, BlockSyntax block, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(catchKeyword);
		this.catchKeyword = catchKeyword;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (filter != null)
		{
			AdjustFlagsAndWidth(filter);
			this.filter = filter;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal CatchClauseSyntax(SyntaxKind kind, SyntaxToken catchKeyword, CatchDeclarationSyntax declaration, CatchFilterClauseSyntax filter, BlockSyntax block, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(catchKeyword);
		this.catchKeyword = catchKeyword;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (filter != null)
		{
			AdjustFlagsAndWidth(filter);
			this.filter = filter;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal CatchClauseSyntax(SyntaxKind kind, SyntaxToken catchKeyword, CatchDeclarationSyntax declaration, CatchFilterClauseSyntax filter, BlockSyntax block)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(catchKeyword);
		this.catchKeyword = catchKeyword;
		if (declaration != null)
		{
			AdjustFlagsAndWidth(declaration);
			this.declaration = declaration;
		}
		if (filter != null)
		{
			AdjustFlagsAndWidth(filter);
			this.filter = filter;
		}
		AdjustFlagsAndWidth(block);
		this.block = block;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)catchKeyword, 
			1 => declaration, 
			2 => filter, 
			3 => block, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CatchClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCatchClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCatchClause(this);
	}

	public CatchClauseSyntax Update(SyntaxToken catchKeyword, CatchDeclarationSyntax declaration, CatchFilterClauseSyntax filter, BlockSyntax block)
	{
		if (catchKeyword != CatchKeyword || declaration != Declaration || filter != Filter || block != Block)
		{
			CatchClauseSyntax catchClauseSyntax = SyntaxFactory.CatchClause(catchKeyword, declaration, filter, block);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				catchClauseSyntax = catchClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				catchClauseSyntax = catchClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return catchClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CatchClauseSyntax(Kind, catchKeyword, declaration, filter, block, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CatchClauseSyntax(Kind, catchKeyword, declaration, filter, block, GetDiagnostics(), annotations);
	}

	internal CatchClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			catchKeyword = syntaxToken;
		}
		CatchDeclarationSyntax catchDeclarationSyntax = (CatchDeclarationSyntax)reader.ReadValue();
		if (catchDeclarationSyntax != null)
		{
			AdjustFlagsAndWidth(catchDeclarationSyntax);
			declaration = catchDeclarationSyntax;
		}
		CatchFilterClauseSyntax catchFilterClauseSyntax = (CatchFilterClauseSyntax)reader.ReadValue();
		if (catchFilterClauseSyntax != null)
		{
			AdjustFlagsAndWidth(catchFilterClauseSyntax);
			filter = catchFilterClauseSyntax;
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
		writer.WriteValue(catchKeyword);
		writer.WriteValue(declaration);
		writer.WriteValue(filter);
		writer.WriteValue(block);
	}

	static CatchClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CatchClauseSyntax), (ObjectReader r) => new CatchClauseSyntax(r));
	}
}
