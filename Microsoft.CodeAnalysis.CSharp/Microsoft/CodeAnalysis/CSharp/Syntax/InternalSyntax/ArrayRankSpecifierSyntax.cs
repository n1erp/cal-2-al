using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ArrayRankSpecifierSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken openBracketToken;

	internal readonly GreenNode sizes;

	internal readonly SyntaxToken closeBracketToken;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> Sizes => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(sizes));

	public SyntaxToken CloseBracketToken => closeBracketToken;

	internal ArrayRankSpecifierSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode sizes, SyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (sizes != null)
		{
			AdjustFlagsAndWidth(sizes);
			this.sizes = sizes;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal ArrayRankSpecifierSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode sizes, SyntaxToken closeBracketToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (sizes != null)
		{
			AdjustFlagsAndWidth(sizes);
			this.sizes = sizes;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal ArrayRankSpecifierSyntax(SyntaxKind kind, SyntaxToken openBracketToken, GreenNode sizes, SyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (sizes != null)
		{
			AdjustFlagsAndWidth(sizes);
			this.sizes = sizes;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openBracketToken, 
			1 => sizes, 
			2 => closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ArrayRankSpecifierSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArrayRankSpecifier(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArrayRankSpecifier(this);
	}

	public ArrayRankSpecifierSyntax Update(SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> sizes, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || sizes != Sizes || closeBracketToken != CloseBracketToken)
		{
			ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = SyntaxFactory.ArrayRankSpecifier(openBracketToken, sizes, closeBracketToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				arrayRankSpecifierSyntax = arrayRankSpecifierSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				arrayRankSpecifierSyntax = arrayRankSpecifierSyntax.WithAnnotationsGreen(annotations);
			}
			return arrayRankSpecifierSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ArrayRankSpecifierSyntax(Kind, openBracketToken, sizes, closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ArrayRankSpecifierSyntax(Kind, openBracketToken, sizes, closeBracketToken, GetDiagnostics(), annotations);
	}

	internal ArrayRankSpecifierSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			openBracketToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			sizes = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			closeBracketToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(openBracketToken);
		writer.WriteValue(sizes);
		writer.WriteValue(closeBracketToken);
	}

	static ArrayRankSpecifierSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ArrayRankSpecifierSyntax), (ObjectReader r) => new ArrayRankSpecifierSyntax(r));
	}
}
