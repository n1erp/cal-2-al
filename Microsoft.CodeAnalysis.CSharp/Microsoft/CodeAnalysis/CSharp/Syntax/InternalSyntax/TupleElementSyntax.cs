using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TupleElementSyntax : CSharpSyntaxNode
{
	internal readonly TypeSyntax type;

	internal readonly SyntaxToken identifier;

	public TypeSyntax Type => type;

	public SyntaxToken Identifier => identifier;

	internal TupleElementSyntax(SyntaxKind kind, TypeSyntax type, SyntaxToken identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
	}

	internal TupleElementSyntax(SyntaxKind kind, TypeSyntax type, SyntaxToken identifier, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
	}

	internal TupleElementSyntax(SyntaxKind kind, TypeSyntax type, SyntaxToken identifier)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(type);
		this.type = type;
		if (identifier != null)
		{
			AdjustFlagsAndWidth(identifier);
			this.identifier = identifier;
		}
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)type, 
			1 => identifier, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TupleElementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTupleElement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTupleElement(this);
	}

	public TupleElementSyntax Update(TypeSyntax type, SyntaxToken identifier)
	{
		if (type != Type || identifier != Identifier)
		{
			TupleElementSyntax tupleElementSyntax = SyntaxFactory.TupleElement(type, identifier);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				tupleElementSyntax = tupleElementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				tupleElementSyntax = tupleElementSyntax.WithAnnotationsGreen(annotations);
			}
			return tupleElementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TupleElementSyntax(Kind, type, identifier, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TupleElementSyntax(Kind, type, identifier, GetDiagnostics(), annotations);
	}

	internal TupleElementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		TypeSyntax typeSyntax = (TypeSyntax)reader.ReadValue();
		if (typeSyntax != null)
		{
			AdjustFlagsAndWidth(typeSyntax);
			type = typeSyntax;
		}
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
		writer.WriteValue(type);
		writer.WriteValue(identifier);
	}

	static TupleElementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TupleElementSyntax), (ObjectReader r) => new TupleElementSyntax(r));
	}
}
