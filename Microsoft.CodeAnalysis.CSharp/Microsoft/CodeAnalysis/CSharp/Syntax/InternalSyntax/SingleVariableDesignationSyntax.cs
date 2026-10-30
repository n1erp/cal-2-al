using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SingleVariableDesignationSyntax : VariableDesignationSyntax
{
	internal readonly SyntaxToken identifier;

	public SyntaxToken Identifier => identifier;

	internal SingleVariableDesignationSyntax(SyntaxKind kind, SyntaxToken identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal SingleVariableDesignationSyntax(SyntaxKind kind, SyntaxToken identifier, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal SingleVariableDesignationSyntax(SyntaxKind kind, SyntaxToken identifier)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSingleVariableDesignation(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSingleVariableDesignation(this);
	}

	public SingleVariableDesignationSyntax Update(SyntaxToken identifier)
	{
		if (identifier != Identifier)
		{
			SingleVariableDesignationSyntax singleVariableDesignationSyntax = SyntaxFactory.SingleVariableDesignation(identifier);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				singleVariableDesignationSyntax = singleVariableDesignationSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				singleVariableDesignationSyntax = singleVariableDesignationSyntax.WithAnnotationsGreen(annotations);
			}
			return singleVariableDesignationSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new SingleVariableDesignationSyntax(Kind, identifier, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new SingleVariableDesignationSyntax(Kind, identifier, GetDiagnostics(), annotations);
	}

	internal SingleVariableDesignationSyntax(ObjectReader reader)
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

	static SingleVariableDesignationSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(SingleVariableDesignationSyntax), (ObjectReader r) => new SingleVariableDesignationSyntax(r));
	}
}
