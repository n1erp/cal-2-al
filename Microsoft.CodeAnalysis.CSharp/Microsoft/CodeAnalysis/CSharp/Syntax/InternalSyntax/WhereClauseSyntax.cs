using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class WhereClauseSyntax : QueryClauseSyntax
{
	internal readonly SyntaxToken whereKeyword;

	internal readonly ExpressionSyntax condition;

	public SyntaxToken WhereKeyword => whereKeyword;

	public ExpressionSyntax Condition => condition;

	internal WhereClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, ExpressionSyntax condition, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal WhereClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, ExpressionSyntax condition, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal WhereClauseSyntax(SyntaxKind kind, SyntaxToken whereKeyword, ExpressionSyntax condition)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(whereKeyword);
		this.whereKeyword = whereKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)whereKeyword, 
			1 => condition, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.WhereClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhereClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWhereClause(this);
	}

	public WhereClauseSyntax Update(SyntaxToken whereKeyword, ExpressionSyntax condition)
	{
		if (whereKeyword != WhereKeyword || condition != Condition)
		{
			WhereClauseSyntax whereClauseSyntax = SyntaxFactory.WhereClause(whereKeyword, condition);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				whereClauseSyntax = whereClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				whereClauseSyntax = whereClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return whereClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new WhereClauseSyntax(Kind, whereKeyword, condition, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new WhereClauseSyntax(Kind, whereKeyword, condition, GetDiagnostics(), annotations);
	}

	internal WhereClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			whereKeyword = syntaxToken;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			condition = expressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(whereKeyword);
		writer.WriteValue(condition);
	}

	static WhereClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(WhereClauseSyntax), (ObjectReader r) => new WhereClauseSyntax(r));
	}
}
