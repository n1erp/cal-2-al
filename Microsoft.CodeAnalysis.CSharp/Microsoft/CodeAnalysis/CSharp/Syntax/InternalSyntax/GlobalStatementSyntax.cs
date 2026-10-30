using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class GlobalStatementSyntax : MemberDeclarationSyntax
{
	internal readonly StatementSyntax statement;

	public StatementSyntax Statement => statement;

	internal GlobalStatementSyntax(SyntaxKind kind, StatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal GlobalStatementSyntax(SyntaxKind kind, StatementSyntax statement, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal GlobalStatementSyntax(SyntaxKind kind, StatementSyntax statement)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(statement);
		this.statement = statement;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return statement;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.GlobalStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGlobalStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitGlobalStatement(this);
	}

	public GlobalStatementSyntax Update(StatementSyntax statement)
	{
		if (statement != Statement)
		{
			GlobalStatementSyntax globalStatementSyntax = SyntaxFactory.GlobalStatement(statement);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				globalStatementSyntax = globalStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				globalStatementSyntax = globalStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return globalStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new GlobalStatementSyntax(Kind, statement, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new GlobalStatementSyntax(Kind, statement, GetDiagnostics(), annotations);
	}

	internal GlobalStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
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
		writer.WriteValue(statement);
	}

	static GlobalStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(GlobalStatementSyntax), (ObjectReader r) => new GlobalStatementSyntax(r));
	}
}
