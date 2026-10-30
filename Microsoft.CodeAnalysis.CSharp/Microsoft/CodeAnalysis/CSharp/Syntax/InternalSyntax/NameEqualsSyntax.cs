using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class NameEqualsSyntax : CSharpSyntaxNode
{
	internal readonly IdentifierNameSyntax name;

	internal readonly SyntaxToken equalsToken;

	public IdentifierNameSyntax Name => name;

	public SyntaxToken EqualsToken => equalsToken;

	internal NameEqualsSyntax(SyntaxKind kind, IdentifierNameSyntax name, SyntaxToken equalsToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
	}

	internal NameEqualsSyntax(SyntaxKind kind, IdentifierNameSyntax name, SyntaxToken equalsToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
	}

	internal NameEqualsSyntax(SyntaxKind kind, IdentifierNameSyntax name, SyntaxToken equalsToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(equalsToken);
		this.equalsToken = equalsToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)name, 
			1 => equalsToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.NameEqualsSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNameEquals(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNameEquals(this);
	}

	public NameEqualsSyntax Update(IdentifierNameSyntax name, SyntaxToken equalsToken)
	{
		if (name != Name || equalsToken != EqualsToken)
		{
			NameEqualsSyntax nameEqualsSyntax = SyntaxFactory.NameEquals(name, equalsToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				nameEqualsSyntax = nameEqualsSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				nameEqualsSyntax = nameEqualsSyntax.WithAnnotationsGreen(annotations);
			}
			return nameEqualsSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new NameEqualsSyntax(Kind, name, equalsToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new NameEqualsSyntax(Kind, name, equalsToken, GetDiagnostics(), annotations);
	}

	internal NameEqualsSyntax(ObjectReader reader)
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
			equalsToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(name);
		writer.WriteValue(equalsToken);
	}

	static NameEqualsSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(NameEqualsSyntax), (ObjectReader r) => new NameEqualsSyntax(r));
	}
}
