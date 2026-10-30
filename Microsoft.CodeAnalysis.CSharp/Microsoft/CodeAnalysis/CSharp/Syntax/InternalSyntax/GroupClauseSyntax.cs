using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class GroupClauseSyntax : SelectOrGroupClauseSyntax
{
	internal readonly SyntaxToken groupKeyword;

	internal readonly ExpressionSyntax groupExpression;

	internal readonly SyntaxToken byKeyword;

	internal readonly ExpressionSyntax byExpression;

	public SyntaxToken GroupKeyword => groupKeyword;

	public ExpressionSyntax GroupExpression => groupExpression;

	public SyntaxToken ByKeyword => byKeyword;

	public ExpressionSyntax ByExpression => byExpression;

	internal GroupClauseSyntax(SyntaxKind kind, SyntaxToken groupKeyword, ExpressionSyntax groupExpression, SyntaxToken byKeyword, ExpressionSyntax byExpression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(groupKeyword);
		this.groupKeyword = groupKeyword;
		AdjustFlagsAndWidth(groupExpression);
		this.groupExpression = groupExpression;
		AdjustFlagsAndWidth(byKeyword);
		this.byKeyword = byKeyword;
		AdjustFlagsAndWidth(byExpression);
		this.byExpression = byExpression;
	}

	internal GroupClauseSyntax(SyntaxKind kind, SyntaxToken groupKeyword, ExpressionSyntax groupExpression, SyntaxToken byKeyword, ExpressionSyntax byExpression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(groupKeyword);
		this.groupKeyword = groupKeyword;
		AdjustFlagsAndWidth(groupExpression);
		this.groupExpression = groupExpression;
		AdjustFlagsAndWidth(byKeyword);
		this.byKeyword = byKeyword;
		AdjustFlagsAndWidth(byExpression);
		this.byExpression = byExpression;
	}

	internal GroupClauseSyntax(SyntaxKind kind, SyntaxToken groupKeyword, ExpressionSyntax groupExpression, SyntaxToken byKeyword, ExpressionSyntax byExpression)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(groupKeyword);
		this.groupKeyword = groupKeyword;
		AdjustFlagsAndWidth(groupExpression);
		this.groupExpression = groupExpression;
		AdjustFlagsAndWidth(byKeyword);
		this.byKeyword = byKeyword;
		AdjustFlagsAndWidth(byExpression);
		this.byExpression = byExpression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)groupKeyword, 
			1 => groupExpression, 
			2 => byKeyword, 
			3 => byExpression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.GroupClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGroupClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitGroupClause(this);
	}

	public GroupClauseSyntax Update(SyntaxToken groupKeyword, ExpressionSyntax groupExpression, SyntaxToken byKeyword, ExpressionSyntax byExpression)
	{
		if (groupKeyword != GroupKeyword || groupExpression != GroupExpression || byKeyword != ByKeyword || byExpression != ByExpression)
		{
			GroupClauseSyntax groupClauseSyntax = SyntaxFactory.GroupClause(groupKeyword, groupExpression, byKeyword, byExpression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				groupClauseSyntax = groupClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				groupClauseSyntax = groupClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return groupClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new GroupClauseSyntax(Kind, groupKeyword, groupExpression, byKeyword, byExpression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new GroupClauseSyntax(Kind, groupKeyword, groupExpression, byKeyword, byExpression, GetDiagnostics(), annotations);
	}

	internal GroupClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			groupKeyword = syntaxToken;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			groupExpression = expressionSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			byKeyword = syntaxToken2;
		}
		ExpressionSyntax expressionSyntax2 = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax2 != null)
		{
			AdjustFlagsAndWidth(expressionSyntax2);
			byExpression = expressionSyntax2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(groupKeyword);
		writer.WriteValue(groupExpression);
		writer.WriteValue(byKeyword);
		writer.WriteValue(byExpression);
	}

	static GroupClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(GroupClauseSyntax), (ObjectReader r) => new GroupClauseSyntax(r));
	}
}
