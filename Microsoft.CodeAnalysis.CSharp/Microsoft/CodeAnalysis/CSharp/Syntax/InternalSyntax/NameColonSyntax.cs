using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class NameColonSyntax : CSharpSyntaxNode
{
	internal readonly IdentifierNameSyntax name;

	internal readonly SyntaxToken colonToken;

	public IdentifierNameSyntax Name => name;

	public SyntaxToken ColonToken => colonToken;

	internal NameColonSyntax(SyntaxKind kind, IdentifierNameSyntax name, SyntaxToken colonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal NameColonSyntax(SyntaxKind kind, IdentifierNameSyntax name, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal NameColonSyntax(SyntaxKind kind, IdentifierNameSyntax name, SyntaxToken colonToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)name, 
			1 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.NameColonSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNameColon(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNameColon(this);
	}

	public NameColonSyntax Update(IdentifierNameSyntax name, SyntaxToken colonToken)
	{
		if (name != Name || colonToken != ColonToken)
		{
			NameColonSyntax nameColonSyntax = SyntaxFactory.NameColon(name, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				nameColonSyntax = nameColonSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				nameColonSyntax = nameColonSyntax.WithAnnotationsGreen(annotations);
			}
			return nameColonSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new NameColonSyntax(Kind, name, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new NameColonSyntax(Kind, name, colonToken, GetDiagnostics(), annotations);
	}

	internal NameColonSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)reader.ReadValue();
		if (identifierNameSyntax != null)
		{
			AdjustFlagsAndWidth(identifierNameSyntax);
			name = identifierNameSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			colonToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(name);
		writer.WriteValue(colonToken);
	}

	static NameColonSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(NameColonSyntax), (ObjectReader r) => new NameColonSyntax(r));
	}
}
