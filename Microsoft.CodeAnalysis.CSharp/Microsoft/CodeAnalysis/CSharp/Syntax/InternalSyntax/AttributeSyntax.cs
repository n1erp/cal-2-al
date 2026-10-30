using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AttributeSyntax : CSharpSyntaxNode
{
	internal readonly NameSyntax name;

	internal readonly AttributeArgumentListSyntax argumentList;

	public NameSyntax Name => name;

	public AttributeArgumentListSyntax ArgumentList => argumentList;

	internal AttributeSyntax(SyntaxKind kind, NameSyntax name, AttributeArgumentListSyntax argumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
	}

	internal AttributeSyntax(SyntaxKind kind, NameSyntax name, AttributeArgumentListSyntax argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
	}

	internal AttributeSyntax(SyntaxKind kind, NameSyntax name, AttributeArgumentListSyntax argumentList)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		if (argumentList != null)
		{
			AdjustFlagsAndWidth(argumentList);
			this.argumentList = argumentList;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)name, 
			1 => argumentList, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttribute(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAttribute(this);
	}

	public AttributeSyntax Update(NameSyntax name, AttributeArgumentListSyntax argumentList)
	{
		if (name != Name || argumentList != ArgumentList)
		{
			AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(name, argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				attributeSyntax = attributeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				attributeSyntax = attributeSyntax.WithAnnotationsGreen(annotations);
			}
			return attributeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AttributeSyntax(Kind, name, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AttributeSyntax(Kind, name, argumentList, GetDiagnostics(), annotations);
	}

	internal AttributeSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		NameSyntax nameSyntax = (NameSyntax)reader.ReadValue();
		if (nameSyntax != null)
		{
			AdjustFlagsAndWidth(nameSyntax);
			name = nameSyntax;
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = (AttributeArgumentListSyntax)reader.ReadValue();
		if (attributeArgumentListSyntax != null)
		{
			AdjustFlagsAndWidth(attributeArgumentListSyntax);
			argumentList = attributeArgumentListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(name);
		writer.WriteValue(argumentList);
	}

	static AttributeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AttributeSyntax), (ObjectReader r) => new AttributeSyntax(r));
	}
}
