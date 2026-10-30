using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class PointerTypeSyntax : TypeSyntax
{
	internal readonly TypeSyntax elementType;

	internal readonly SyntaxToken asteriskToken;

	public TypeSyntax ElementType => elementType;

	public SyntaxToken AsteriskToken => asteriskToken;

	internal PointerTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken asteriskToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elementType);
		this.elementType = elementType;
		AdjustFlagsAndWidth(asteriskToken);
		this.asteriskToken = asteriskToken;
	}

	internal PointerTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken asteriskToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(elementType);
		this.elementType = elementType;
		AdjustFlagsAndWidth(asteriskToken);
		this.asteriskToken = asteriskToken;
	}

	internal PointerTypeSyntax(SyntaxKind kind, TypeSyntax elementType, SyntaxToken asteriskToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(elementType);
		this.elementType = elementType;
		AdjustFlagsAndWidth(asteriskToken);
		this.asteriskToken = asteriskToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)elementType, 
			1 => asteriskToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.PointerTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPointerType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPointerType(this);
	}

	public PointerTypeSyntax Update(TypeSyntax elementType, SyntaxToken asteriskToken)
	{
		if (elementType != ElementType || asteriskToken != AsteriskToken)
		{
			PointerTypeSyntax pointerTypeSyntax = SyntaxFactory.PointerType(elementType, asteriskToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				pointerTypeSyntax = pointerTypeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				pointerTypeSyntax = pointerTypeSyntax.WithAnnotationsGreen(annotations);
			}
			return pointerTypeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new PointerTypeSyntax(Kind, elementType, asteriskToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new PointerTypeSyntax(Kind, elementType, asteriskToken, GetDiagnostics(), annotations);
	}

	internal PointerTypeSyntax(ObjectReader reader)
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
			asteriskToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(elementType);
		writer.WriteValue(asteriskToken);
	}

	static PointerTypeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(PointerTypeSyntax), (ObjectReader r) => new PointerTypeSyntax(r));
	}
}
