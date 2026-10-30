using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class NullableTypeSyntax : TypeSyntax
{
	internal readonly TypeSyntax elementType;

	internal readonly SyntaxToken questionToken;

	public TypeSyntax ElementType => elementType;

	public SyntaxToken QuestionToken => questionToken;

	internal NullableTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken questionToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elementType);
		this.elementType = elementType;
		AdjustFlagsAndWidth(questionToken);
		this.questionToken = questionToken;
	}

	internal NullableTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken questionToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(elementType);
		this.elementType = elementType;
		AdjustFlagsAndWidth(questionToken);
		this.questionToken = questionToken;
	}

	internal NullableTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken questionToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elementType);
		this.elementType = elementType;
		AdjustFlagsAndWidth(questionToken);
		this.questionToken = questionToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)elementType, 
			1 => questionToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.NullableTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNullableType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNullableType(this);
	}

	public NullableTypeSyntax Update(TypeSyntax elementType, SyntaxToken questionToken)
	{
		if (elementType != ElementType || questionToken != QuestionToken)
		{
			NullableTypeSyntax nullableTypeSyntax = SyntaxFactory.NullableType(elementType, questionToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				nullableTypeSyntax = nullableTypeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				nullableTypeSyntax = nullableTypeSyntax.WithAnnotationsGreen(annotations);
			}
			return nullableTypeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new NullableTypeSyntax(Kind, elementType, questionToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new NullableTypeSyntax(Kind, elementType, questionToken, GetDiagnostics(), annotations);
	}

	internal NullableTypeSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			elementType = typeSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			questionToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(elementType);
		writer.WriteValue(questionToken);
	}

	static NullableTypeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(NullableTypeSyntax), (ObjectReader r) => new NullableTypeSyntax(r));
	}
}
