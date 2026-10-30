using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TypeConstraintSyntax : TypeParameterConstraintSyntax
{
	internal readonly TypeSyntax type;

	public TypeSyntax Type => type;

	internal TypeConstraintSyntax(SyntaxKind kind, TypeSyntax type, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypeConstraintSyntax(SyntaxKind kind, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal TypeConstraintSyntax(SyntaxKind kind, TypeSyntax type)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TypeConstraintSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeConstraint(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeConstraint(this);
	}

	public TypeConstraintSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			TypeConstraintSyntax typeConstraintSyntax = SyntaxFactory.TypeConstraint(type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				typeConstraintSyntax = typeConstraintSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				typeConstraintSyntax = typeConstraintSyntax.WithAnnotationsGreen(annotations);
			}
			return typeConstraintSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TypeConstraintSyntax(Kind, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TypeConstraintSyntax(Kind, type, GetDiagnostics(), annotations);
	}

	internal TypeConstraintSyntax(ObjectReader reader)
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

	static TypeConstraintSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TypeConstraintSyntax), (ObjectReader r) => new TypeConstraintSyntax(r));
	}
}
