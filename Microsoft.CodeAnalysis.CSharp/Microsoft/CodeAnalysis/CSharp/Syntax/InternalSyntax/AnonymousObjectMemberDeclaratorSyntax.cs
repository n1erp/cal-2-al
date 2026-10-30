using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AnonymousObjectMemberDeclaratorSyntax : CSharpSyntaxNode
{
	internal readonly NameEqualsSyntax nameEquals;

	internal readonly ExpressionSyntax expression;

	public NameEqualsSyntax NameEquals => nameEquals;

	public ExpressionSyntax Expression => expression;

	internal AnonymousObjectMemberDeclaratorSyntax(SyntaxKind kind, NameEqualsSyntax nameEquals, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		if (nameEquals != null)
		{
			AdjustFlagsAndWidth(nameEquals);
			this.nameEquals = nameEquals;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal AnonymousObjectMemberDeclaratorSyntax(SyntaxKind kind, NameEqualsSyntax nameEquals, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		if (nameEquals != null)
		{
			AdjustFlagsAndWidth(nameEquals);
			this.nameEquals = nameEquals;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal AnonymousObjectMemberDeclaratorSyntax(SyntaxKind kind, NameEqualsSyntax nameEquals, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 2;
		if (nameEquals != null)
		{
			AdjustFlagsAndWidth(nameEquals);
			this.nameEquals = nameEquals;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)nameEquals, 
			1 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectMemberDeclaratorSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAnonymousObjectMemberDeclarator(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAnonymousObjectMemberDeclarator(this);
	}

	public AnonymousObjectMemberDeclaratorSyntax Update(NameEqualsSyntax nameEquals, ExpressionSyntax expression)
	{
		if (nameEquals != NameEquals || expression != Expression)
		{
			AnonymousObjectMemberDeclaratorSyntax anonymousObjectMemberDeclaratorSyntax = SyntaxFactory.AnonymousObjectMemberDeclarator(nameEquals, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				anonymousObjectMemberDeclaratorSyntax = anonymousObjectMemberDeclaratorSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				anonymousObjectMemberDeclaratorSyntax = anonymousObjectMemberDeclaratorSyntax.WithAnnotationsGreen(annotations);
			}
			return anonymousObjectMemberDeclaratorSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AnonymousObjectMemberDeclaratorSyntax(Kind, nameEquals, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AnonymousObjectMemberDeclaratorSyntax(Kind, nameEquals, expression, GetDiagnostics(), annotations);
	}

	internal AnonymousObjectMemberDeclaratorSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		NameEqualsSyntax nameEqualsSyntax = (NameEqualsSyntax)reader.ReadValue();
		if (nameEqualsSyntax != null)
		{
			AdjustFlagsAndWidth(nameEqualsSyntax);
			nameEquals = nameEqualsSyntax;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(nameEquals);
		writer.WriteValue(expression);
	}

	static AnonymousObjectMemberDeclaratorSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AnonymousObjectMemberDeclaratorSyntax), (ObjectReader r) => new AnonymousObjectMemberDeclaratorSyntax(r));
	}
}
