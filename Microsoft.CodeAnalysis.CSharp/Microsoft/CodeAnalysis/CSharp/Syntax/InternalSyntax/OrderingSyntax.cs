using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class OrderingSyntax : CSharpSyntaxNode
{
	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken ascendingOrDescendingKeyword;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken AscendingOrDescendingKeyword => ascendingOrDescendingKeyword;

	internal OrderingSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken ascendingOrDescendingKeyword, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (ascendingOrDescendingKeyword != null)
		{
			AdjustFlagsAndWidth(ascendingOrDescendingKeyword);
			this.ascendingOrDescendingKeyword = ascendingOrDescendingKeyword;
		}
	}

	internal OrderingSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken ascendingOrDescendingKeyword, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (ascendingOrDescendingKeyword != null)
		{
			AdjustFlagsAndWidth(ascendingOrDescendingKeyword);
			this.ascendingOrDescendingKeyword = ascendingOrDescendingKeyword;
		}
	}

	internal OrderingSyntax(SyntaxKind kind, ExpressionSyntax expression, SyntaxToken ascendingOrDescendingKeyword)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		if (ascendingOrDescendingKeyword != null)
		{
			AdjustFlagsAndWidth(ascendingOrDescendingKeyword);
			this.ascendingOrDescendingKeyword = ascendingOrDescendingKeyword;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)expression, 
			1 => ascendingOrDescendingKeyword, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.OrderingSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOrdering(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOrdering(this);
	}

	public OrderingSyntax Update(ExpressionSyntax expression, SyntaxToken ascendingOrDescendingKeyword)
	{
		if (expression != Expression || ascendingOrDescendingKeyword != AscendingOrDescendingKeyword)
		{
			OrderingSyntax orderingSyntax = SyntaxFactory.Ordering(Kind, expression, ascendingOrDescendingKeyword);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				orderingSyntax = orderingSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				orderingSyntax = orderingSyntax.WithAnnotationsGreen(annotations);
			}
			return orderingSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new OrderingSyntax(Kind, expression, ascendingOrDescendingKeyword, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new OrderingSyntax(Kind, expression, ascendingOrDescendingKeyword, GetDiagnostics(), annotations);
	}

	internal OrderingSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
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
			ascendingOrDescendingKeyword = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(expression);
		writer.WriteValue(ascendingOrDescendingKeyword);
	}

	static OrderingSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(OrderingSyntax), (ObjectReader r) => new OrderingSyntax(r));
	}
}
