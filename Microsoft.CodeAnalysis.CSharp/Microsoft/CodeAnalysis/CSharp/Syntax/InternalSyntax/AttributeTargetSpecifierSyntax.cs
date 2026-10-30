using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AttributeTargetSpecifierSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken colonToken;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken ColonToken => colonToken;

	internal AttributeTargetSpecifierSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxToken colonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal AttributeTargetSpecifierSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal AttributeTargetSpecifierSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxToken colonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)identifier, 
			1 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AttributeTargetSpecifierSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeTargetSpecifier(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAttributeTargetSpecifier(this);
	}

	public AttributeTargetSpecifierSyntax Update(SyntaxToken identifier, SyntaxToken colonToken)
	{
		if (identifier != Identifier || colonToken != ColonToken)
		{
			AttributeTargetSpecifierSyntax attributeTargetSpecifierSyntax = SyntaxFactory.AttributeTargetSpecifier(identifier, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				attributeTargetSpecifierSyntax = attributeTargetSpecifierSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				attributeTargetSpecifierSyntax = attributeTargetSpecifierSyntax.WithAnnotationsGreen(annotations);
			}
			return attributeTargetSpecifierSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AttributeTargetSpecifierSyntax(Kind, identifier, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AttributeTargetSpecifierSyntax(Kind, identifier, colonToken, GetDiagnostics(), annotations);
	}

	internal AttributeTargetSpecifierSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			identifier = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			colonToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(identifier);
		writer.WriteValue(colonToken);
	}

	static AttributeTargetSpecifierSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AttributeTargetSpecifierSyntax), (ObjectReader r) => new AttributeTargetSpecifierSyntax(r));
	}
}
