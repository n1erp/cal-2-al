using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IdentifierNameSyntax : SimpleNameSyntax
{
	internal readonly SyntaxToken identifier;

	public override SyntaxToken Identifier => identifier;

	internal IdentifierNameSyntax(SyntaxKind kind, SyntaxToken identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal IdentifierNameSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal IdentifierNameSyntax(SyntaxKind kind, SyntaxToken identifier)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return identifier;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierName(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIdentifierName(this);
	}

	public IdentifierNameSyntax Update(SyntaxToken identifier)
	{
		if (identifier != Identifier)
		{
			IdentifierNameSyntax identifierNameSyntax = SyntaxFactory.IdentifierName(identifier);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				identifierNameSyntax = identifierNameSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				identifierNameSyntax = identifierNameSyntax.WithAnnotationsGreen(annotations);
			}
			return identifierNameSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new IdentifierNameSyntax(Kind, identifier, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new IdentifierNameSyntax(Kind, identifier, GetDiagnostics(), annotations);
	}

	internal IdentifierNameSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			identifier = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(identifier);
	}

	static IdentifierNameSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(IdentifierNameSyntax), (ObjectReader r) => new IdentifierNameSyntax(r));
	}

	public override string ToString()
	{
		return Identifier.Text;
	}
}
