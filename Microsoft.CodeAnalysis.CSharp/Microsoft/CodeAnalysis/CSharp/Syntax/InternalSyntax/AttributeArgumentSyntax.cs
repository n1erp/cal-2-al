using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AttributeArgumentSyntax : CSharpSyntaxNode
{
	internal readonly NameEqualsSyntax nameEquals;

	internal readonly NameColonSyntax nameColon;

	internal readonly ExpressionSyntax expression;

	public NameEqualsSyntax NameEquals => nameEquals;

	public NameColonSyntax NameColon => nameColon;

	public ExpressionSyntax Expression => expression;

	internal AttributeArgumentSyntax(SyntaxKind kind, NameEqualsSyntax nameEquals, NameColonSyntax nameColon, ExpressionSyntax expression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		if (nameEquals != null)
		{
			AdjustFlagsAndWidth(nameEquals);
			this.nameEquals = nameEquals;
		}
		if (nameColon != null)
		{
			AdjustFlagsAndWidth(nameColon);
			this.nameColon = nameColon;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal AttributeArgumentSyntax(SyntaxKind kind, NameEqualsSyntax nameEquals, NameColonSyntax nameColon, ExpressionSyntax expression, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		if (nameEquals != null)
		{
			AdjustFlagsAndWidth(nameEquals);
			this.nameEquals = nameEquals;
		}
		if (nameColon != null)
		{
			AdjustFlagsAndWidth(nameColon);
			this.nameColon = nameColon;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal AttributeArgumentSyntax(SyntaxKind kind, NameEqualsSyntax nameEquals, NameColonSyntax nameColon, ExpressionSyntax expression)
		: base(kind)
	{
		SlotCount = 3;
		if (nameEquals != null)
		{
			AdjustFlagsAndWidth(nameEquals);
			this.nameEquals = nameEquals;
		}
		if (nameColon != null)
		{
			AdjustFlagsAndWidth(nameColon);
			this.nameColon = nameColon;
		}
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)nameEquals, 
			1 => nameColon, 
			2 => expression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeArgument(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAttributeArgument(this);
	}

	public AttributeArgumentSyntax Update(NameEqualsSyntax nameEquals, NameColonSyntax nameColon, ExpressionSyntax expression)
	{
		if (nameEquals != NameEquals || nameColon != NameColon || expression != Expression)
		{
			AttributeArgumentSyntax attributeArgumentSyntax = SyntaxFactory.AttributeArgument(nameEquals, nameColon, expression);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				attributeArgumentSyntax = attributeArgumentSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				attributeArgumentSyntax = attributeArgumentSyntax.WithAnnotationsGreen(annotations);
			}
			return attributeArgumentSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AttributeArgumentSyntax(Kind, nameEquals, nameColon, expression, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AttributeArgumentSyntax(Kind, nameEquals, nameColon, expression, GetDiagnostics(), annotations);
	}

	internal AttributeArgumentSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		NameEqualsSyntax nameEqualsSyntax = (NameEqualsSyntax)reader.ReadValue();
		if (nameEqualsSyntax != null)
		{
			AdjustFlagsAndWidth(nameEqualsSyntax);
			nameEquals = nameEqualsSyntax;
		}
		NameColonSyntax nameColonSyntax = (NameColonSyntax)reader.ReadValue();
		if (nameColonSyntax != null)
		{
			AdjustFlagsAndWidth(nameColonSyntax);
			nameColon = nameColonSyntax;
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
		writer.WriteValue(nameColon);
		writer.WriteValue(expression);
	}

	static AttributeArgumentSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AttributeArgumentSyntax), (ObjectReader r) => new AttributeArgumentSyntax(r));
	}
}
