using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CaseSwitchLabelSyntax : SwitchLabelSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly ExpressionSyntax value;

	internal readonly SyntaxToken colonToken;

	public override SyntaxToken Keyword => keyword;

	public ExpressionSyntax Value => value;

	public override SyntaxToken ColonToken => colonToken;

	internal CaseSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, ExpressionSyntax value, SyntaxToken colonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(value);
		this.value = value;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal CaseSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, ExpressionSyntax value, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(value);
		this.value = value;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal CaseSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, ExpressionSyntax value, SyntaxToken colonToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(value);
		this.value = value;
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)keyword, 
			1 => value, 
			2 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CaseSwitchLabelSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCaseSwitchLabel(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCaseSwitchLabel(this);
	}

	public CaseSwitchLabelSyntax Update(SyntaxToken keyword, ExpressionSyntax value, SyntaxToken colonToken)
	{
		if (keyword != Keyword || value != Value || colonToken != ColonToken)
		{
			CaseSwitchLabelSyntax caseSwitchLabelSyntax = SyntaxFactory.CaseSwitchLabel(keyword, value, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				caseSwitchLabelSyntax = caseSwitchLabelSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				caseSwitchLabelSyntax = caseSwitchLabelSyntax.WithAnnotationsGreen(annotations);
			}
			return caseSwitchLabelSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CaseSwitchLabelSyntax(Kind, keyword, value, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CaseSwitchLabelSyntax(Kind, keyword, value, colonToken, GetDiagnostics(), annotations);
	}

	internal CaseSwitchLabelSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			keyword = syntaxToken;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			value = expressionSyntax;
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
		writer.WriteValue(keyword);
		writer.WriteValue(value);
		writer.WriteValue(colonToken);
	}

	static CaseSwitchLabelSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CaseSwitchLabelSyntax), (ObjectReader r) => new CaseSwitchLabelSyntax(r));
	}
}
