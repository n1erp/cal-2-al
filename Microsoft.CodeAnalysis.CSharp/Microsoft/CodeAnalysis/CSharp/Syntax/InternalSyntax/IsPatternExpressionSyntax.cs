using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IsPatternExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken isKeyword;

	internal readonly PatternSyntax pattern;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken IsKeyword => isKeyword;

	public PatternSyntax Pattern => pattern;

	internal IsPatternExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken isKeyword, PatternSyntax pattern, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(isKeyword);
		this.isKeyword = isKeyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal IsPatternExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken isKeyword, PatternSyntax pattern, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(isKeyword);
		this.isKeyword = isKeyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal IsPatternExpressionSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken isKeyword, PatternSyntax pattern)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(isKeyword);
		this.isKeyword = isKeyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)expression, 
			1 => isKeyword, 
			2 => pattern, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IsPatternExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIsPatternExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIsPatternExpression(this);
	}

	public IsPatternExpressionSyntax Update(ExpressionSyntax expression, SyntaxToken isKeyword, PatternSyntax pattern)
	{
		if (expression != Expression || isKeyword != IsKeyword || pattern != Pattern)
		{
			IsPatternExpressionSyntax isPatternExpressionSyntax = SyntaxFactory.IsPatternExpression(expression, isKeyword, pattern);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				isPatternExpressionSyntax = isPatternExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				isPatternExpressionSyntax = isPatternExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return isPatternExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new IsPatternExpressionSyntax(Kind, expression, isKeyword, pattern, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new IsPatternExpressionSyntax(Kind, expression, isKeyword, pattern, GetDiagnostics(), annotations);
	}

	internal IsPatternExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			isKeyword = syntaxToken;
		}
		PatternSyntax patternSyntax = (PatternSyntax)reader.ReadValue();
		if (patternSyntax != null)
		{
			AdjustFlagsAndWidth(patternSyntax);
			pattern = patternSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(expression);
		writer.WriteValue(isKeyword);
		writer.WriteValue(pattern);
	}

	static IsPatternExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(IsPatternExpressionSyntax), (ObjectReader r) => new IsPatternExpressionSyntax(r));
	}
}
