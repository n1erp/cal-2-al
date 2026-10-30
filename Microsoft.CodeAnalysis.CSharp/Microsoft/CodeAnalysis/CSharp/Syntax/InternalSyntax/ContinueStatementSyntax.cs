using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ContinueStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken continueKeyword;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken ContinueKeyword => continueKeyword;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal ContinueStatementSyntax(SyntaxKind kind, SyntaxToken continueKeyword, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(continueKeyword);
		this.continueKeyword = continueKeyword;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ContinueStatementSyntax(SyntaxKind kind, SyntaxToken continueKeyword, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(continueKeyword);
		this.continueKeyword = continueKeyword;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ContinueStatementSyntax(SyntaxKind kind, SyntaxToken continueKeyword, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(continueKeyword);
		this.continueKeyword = continueKeyword;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)continueKeyword, 
			1 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ContinueStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitContinueStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitContinueStatement(this);
	}

	public ContinueStatementSyntax Update(SyntaxToken continueKeyword, SyntaxToken semicolonToken)
	{
		if (continueKeyword != ContinueKeyword || semicolonToken != SemicolonToken)
		{
			ContinueStatementSyntax continueStatementSyntax = SyntaxFactory.ContinueStatement(continueKeyword, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				continueStatementSyntax = continueStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				continueStatementSyntax = continueStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return continueStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ContinueStatementSyntax(Kind, continueKeyword, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ContinueStatementSyntax(Kind, continueKeyword, semicolonToken, GetDiagnostics(), annotations);
	}

	internal ContinueStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			continueKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			semicolonToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(continueKeyword);
		writer.WriteValue(semicolonToken);
	}

	static ContinueStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ContinueStatementSyntax), (ObjectReader r) => new ContinueStatementSyntax(r));
	}
}
