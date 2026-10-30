using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class TupleTypeSyntax : TypeSyntax
{
	internal readonly SyntaxToken openParenToken;

	internal readonly GreenNode elements;

	internal readonly SyntaxToken closeParenToken;

	public SyntaxToken OpenParenToken => openParenToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TupleElementSyntax> Elements => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TupleElementSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(elements));

	public SyntaxToken CloseParenToken => closeParenToken;

	internal TupleTypeSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode elements, SyntaxToken closeParenToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			this.elements = elements;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal TupleTypeSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode elements, SyntaxToken closeParenToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			this.elements = elements;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal TupleTypeSyntax(SyntaxKind kind, SyntaxToken openParenToken, GreenNode elements, SyntaxToken closeParenToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		if (elements != null)
		{
			AdjustFlagsAndWidth(elements);
			this.elements = elements;
		}
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openParenToken, 
			1 => elements, 
			2 => closeParenToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.TupleTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTupleType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTupleType(this);
	}

	public TupleTypeSyntax Update(SyntaxToken openParenToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<TupleElementSyntax> elements, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || elements != Elements || closeParenToken != CloseParenToken)
		{
			TupleTypeSyntax tupleTypeSyntax = SyntaxFactory.TupleType(openParenToken, elements, closeParenToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				tupleTypeSyntax = tupleTypeSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				tupleTypeSyntax = tupleTypeSyntax.WithAnnotationsGreen(annotations);
			}
			return tupleTypeSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new TupleTypeSyntax(Kind, openParenToken, elements, closeParenToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new TupleTypeSyntax(Kind, openParenToken, elements, closeParenToken, GetDiagnostics(), annotations);
	}

	internal TupleTypeSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			openParenToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			elements = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			closeParenToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(openParenToken);
		writer.WriteValue(elements);
		writer.WriteValue(closeParenToken);
	}

	static TupleTypeSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(TupleTypeSyntax), (ObjectReader r) => new TupleTypeSyntax(r));
	}
}
