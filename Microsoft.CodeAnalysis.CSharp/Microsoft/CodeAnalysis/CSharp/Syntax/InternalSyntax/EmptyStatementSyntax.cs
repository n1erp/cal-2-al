using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class EmptyStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal EmptyStatementSyntax(SyntaxKind kind, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal EmptyStatementSyntax(SyntaxKind kind, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal EmptyStatementSyntax(SyntaxKind kind, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return semicolonToken;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.EmptyStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEmptyStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEmptyStatement(this);
	}

	public EmptyStatementSyntax Update(SyntaxToken semicolonToken)
	{
		if (semicolonToken != SemicolonToken)
		{
			EmptyStatementSyntax emptyStatementSyntax = SyntaxFactory.EmptyStatement(semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				emptyStatementSyntax = emptyStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				emptyStatementSyntax = emptyStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return emptyStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new EmptyStatementSyntax(Kind, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new EmptyStatementSyntax(Kind, semicolonToken, GetDiagnostics(), annotations);
	}

	internal EmptyStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			semicolonToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(semicolonToken);
	}

	static EmptyStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(EmptyStatementSyntax), (ObjectReader r) => new EmptyStatementSyntax(r));
	}
}
