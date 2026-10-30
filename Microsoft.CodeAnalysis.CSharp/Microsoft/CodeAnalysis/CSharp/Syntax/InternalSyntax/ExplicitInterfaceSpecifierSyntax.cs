using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExplicitInterfaceSpecifierSyntax : CSharpSyntaxNode
{
	internal readonly NameSyntax name;

	internal readonly SyntaxToken dotToken;

	public NameSyntax Name => name;

	public SyntaxToken DotToken => dotToken;

	internal ExplicitInterfaceSpecifierSyntax(SyntaxKind kind, NameSyntax name, SyntaxToken dotToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
	}

	internal ExplicitInterfaceSpecifierSyntax(SyntaxKind kind, NameSyntax name, SyntaxToken dotToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
	}

	internal ExplicitInterfaceSpecifierSyntax(SyntaxKind kind, NameSyntax name, SyntaxToken dotToken)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(name);
		this.name = name;
		AdjustFlagsAndWidth(dotToken);
		this.dotToken = dotToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)name, 
			1 => dotToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExplicitInterfaceSpecifierSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExplicitInterfaceSpecifier(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExplicitInterfaceSpecifier(this);
	}

	public ExplicitInterfaceSpecifierSyntax Update(NameSyntax name, SyntaxToken dotToken)
	{
		if (name != Name || dotToken != DotToken)
		{
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = SyntaxFactory.ExplicitInterfaceSpecifier(name, dotToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				explicitInterfaceSpecifierSyntax = explicitInterfaceSpecifierSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				explicitInterfaceSpecifierSyntax = explicitInterfaceSpecifierSyntax.WithAnnotationsGreen(annotations);
			}
			return explicitInterfaceSpecifierSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ExplicitInterfaceSpecifierSyntax(Kind, name, dotToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ExplicitInterfaceSpecifierSyntax(Kind, name, dotToken, GetDiagnostics(), annotations);
	}

	internal ExplicitInterfaceSpecifierSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		NameSyntax nameSyntax = (NameSyntax)reader.ReadValue();
		if (nameSyntax != null)
		{
			AdjustFlagsAndWidth(nameSyntax);
			name = nameSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			dotToken = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(name);
		writer.WriteValue(dotToken);
	}

	static ExplicitInterfaceSpecifierSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ExplicitInterfaceSpecifierSyntax), (ObjectReader r) => new ExplicitInterfaceSpecifierSyntax(r));
	}
}
