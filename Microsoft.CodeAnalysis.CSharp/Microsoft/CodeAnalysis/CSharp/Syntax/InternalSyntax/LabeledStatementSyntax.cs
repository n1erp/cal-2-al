using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class LabeledStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken colonToken;

	internal readonly StatementSyntax statement;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken ColonToken => colonToken;

	public StatementSyntax Statement => statement;

	internal LabeledStatementSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal LabeledStatementSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal LabeledStatementSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)identifier, 
			1 => colonToken, 
			2 => statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.LabeledStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabeledStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLabeledStatement(this);
	}

	public LabeledStatementSyntax Update(SyntaxToken identifier, SyntaxToken colonToken, StatementSyntax statement)
	{
		if (identifier != Identifier || colonToken != ColonToken || statement != Statement)
		{
			LabeledStatementSyntax labeledStatementSyntax = SyntaxFactory.LabeledStatement(identifier, colonToken, statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				labeledStatementSyntax = labeledStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				labeledStatementSyntax = labeledStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return labeledStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new LabeledStatementSyntax(Kind, identifier, colonToken, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new LabeledStatementSyntax(Kind, identifier, colonToken, statement, GetDiagnostics(), annotations);
	}

	internal LabeledStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			identifier = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			colonToken = syntaxToken2;
		}
		StatementSyntax statementSyntax = (StatementSyntax)reader.ReadValue();
		if (statementSyntax != null)
		{
			AdjustFlagsAndWidth(statementSyntax);
			statement = statementSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(identifier);
		writer.WriteValue(colonToken);
		writer.WriteValue(statement);
	}

	static LabeledStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(LabeledStatementSyntax), (ObjectReader r) => new LabeledStatementSyntax(r));
	}
}
