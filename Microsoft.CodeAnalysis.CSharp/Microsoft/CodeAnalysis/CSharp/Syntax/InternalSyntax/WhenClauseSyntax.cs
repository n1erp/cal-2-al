using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class WhenClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken whenKeyword;

	internal readonly ExpressionSyntax condition;

	public SyntaxToken WhenKeyword => whenKeyword;

	public ExpressionSyntax Condition => condition;

	internal WhenClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, ExpressionSyntax condition, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal WhenClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, ExpressionSyntax condition, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal WhenClauseSyntax(SyntaxKind kind, SyntaxToken whenKeyword, ExpressionSyntax condition)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(whenKeyword);
		this.whenKeyword = whenKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)whenKeyword, 
			1 => condition, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.WhenClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitWhenClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitWhenClause(this);
	}

	public WhenClauseSyntax Update(SyntaxToken whenKeyword, ExpressionSyntax condition)
	{
		if (whenKeyword != WhenKeyword || condition != Condition)
		{
			WhenClauseSyntax whenClauseSyntax = SyntaxFactory.WhenClause(whenKeyword, condition);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				whenClauseSyntax = whenClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				whenClauseSyntax = whenClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return whenClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new WhenClauseSyntax(Kind, whenKeyword, condition, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new WhenClauseSyntax(Kind, whenKeyword, condition, GetDiagnostics(), annotations);
	}

	internal WhenClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			whenKeyword = syntaxToken;
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
		writer.WriteValue(whenKeyword);
		writer.WriteValue(condition);
	}

	static WhenClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(WhenClauseSyntax), (ObjectReader r) => new WhenClauseSyntax(r));
	}
}
