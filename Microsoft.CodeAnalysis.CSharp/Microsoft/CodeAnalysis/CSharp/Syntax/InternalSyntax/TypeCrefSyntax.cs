using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeCrefSyntax : CrefSyntax
{
	internal readonly TypeSyntax type;

	public TypeSyntax Type => type;

	internal TypeCrefSyntax(SyntaxKind kind, TypeSyntax type, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypeCrefSyntax(SyntaxKind kind, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypeCrefSyntax(SyntaxKind kind, TypeSyntax type)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return type;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeCrefSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeCref(this);
	}

	public TypeCrefSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			TypeCrefSyntax typeCrefSyntax = SyntaxFactory.TypeCref(type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeCrefSyntax = typeCrefSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeCrefSyntax = typeCrefSyntax.WithAnnotationsGreen(annotations);
			}
			return typeCrefSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TypeCrefSyntax(Kind, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TypeCrefSyntax(Kind, type, GetDiagnostics(), annotations);
	}

	internal TypeCrefSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(type);
	}

	static TypeCrefSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TypeCrefSyntax), (ObjectReader r) => new TypeCrefSyntax(r));
	}
}
