using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BreakStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken breakKeyword;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken BreakKeyword => breakKeyword;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal BreakStatementSyntax(SyntaxKind kind, SyntaxToken breakKeyword, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(breakKeyword);
		this.breakKeyword = breakKeyword;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal BreakStatementSyntax(SyntaxKind kind, SyntaxToken breakKeyword, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(breakKeyword);
		this.breakKeyword = breakKeyword;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal BreakStatementSyntax(SyntaxKind kind, SyntaxToken breakKeyword, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(breakKeyword);
		this.breakKeyword = breakKeyword;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)breakKeyword, 
			1 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BreakStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBreakStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBreakStatement(this);
	}

	public BreakStatementSyntax Update(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
	{
		if (breakKeyword != BreakKeyword || semicolonToken != SemicolonToken)
		{
			BreakStatementSyntax breakStatementSyntax = SyntaxFactory.BreakStatement(breakKeyword, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				breakStatementSyntax = breakStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				breakStatementSyntax = breakStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return breakStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new BreakStatementSyntax(Kind, breakKeyword, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new BreakStatementSyntax(Kind, breakKeyword, semicolonToken, GetDiagnostics(), annotations);
	}

	internal BreakStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			breakKeyword = syntaxToken;
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
		writer.WriteValue(breakKeyword);
		writer.WriteValue(semicolonToken);
	}

	static BreakStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(BreakStatementSyntax), (ObjectReader r) => new BreakStatementSyntax(r));
	}
}
