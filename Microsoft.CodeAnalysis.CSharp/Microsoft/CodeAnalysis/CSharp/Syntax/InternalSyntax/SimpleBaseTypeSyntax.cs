using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SimpleBaseTypeSyntax : BaseTypeSyntax
{
	internal readonly TypeSyntax type;

	public override TypeSyntax Type => type;

	internal SimpleBaseTypeSyntax(SyntaxKind kind, TypeSyntax type, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal SimpleBaseTypeSyntax(SyntaxKind kind, TypeSyntax type, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(type);
		this.type = type;
	}

	internal SimpleBaseTypeSyntax(SyntaxKind kind, TypeSyntax type)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SimpleBaseTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleBaseType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSimpleBaseType(this);
	}

	public SimpleBaseTypeSyntax Update(TypeSyntax type)
	{
		if (type != Type)
		{
			SimpleBaseTypeSyntax simpleBaseTypeSyntax = SyntaxFactory.SimpleBaseType(type);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				simpleBaseTypeSyntax = simpleBaseTypeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				simpleBaseTypeSyntax = simpleBaseTypeSyntax.WithAnnotationsGreen(annotations);
			}
			return simpleBaseTypeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new SimpleBaseTypeSyntax(Kind, type, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new SimpleBaseTypeSyntax(Kind, type, GetDiagnostics(), annotations);
	}

	internal SimpleBaseTypeSyntax(ObjectReader reader)
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

	static SimpleBaseTypeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(SimpleBaseTypeSyntax), (ObjectReader r) => new SimpleBaseTypeSyntax(r));
	}
}
