using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SwitchStatementSyntax : StatementSyntax
{
	internal readonly SyntaxToken switchKeyword;

	internal readonly SyntaxToken openParenToken;

	internal readonly ExpressionSyntax expression;

	internal readonly SyntaxToken closeParenToken;

	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode sections;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken SwitchKeyword => switchKeyword;

	public SyntaxToken OpenParenToken => openParenToken;

	public ExpressionSyntax Expression => expression;

	public SyntaxToken CloseParenToken => closeParenToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax> Sections => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax>(sections);

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal SwitchStatementSyntax(SyntaxKind kind, SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, GreenNode sections, SyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(switchKeyword);
		this.switchKeyword = switchKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (sections != null)
		{
			AdjustFlagsAndWidth(sections);
			this.sections = sections;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal SwitchStatementSyntax(SyntaxKind kind, SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, GreenNode sections, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 7;
		AdjustFlagsAndWidth(switchKeyword);
		this.switchKeyword = switchKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (sections != null)
		{
			AdjustFlagsAndWidth(sections);
			this.sections = sections;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal SwitchStatementSyntax(SyntaxKind kind, SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, GreenNode sections, SyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(switchKeyword);
		this.switchKeyword = switchKeyword;
		AdjustFlagsAndWidth(openParenToken);
		this.openParenToken = openParenToken;
		AdjustFlagsAndWidth(expression);
		this.expression = expression;
		AdjustFlagsAndWidth(closeParenToken);
		this.closeParenToken = closeParenToken;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (sections != null)
		{
			AdjustFlagsAndWidth(sections);
			this.sections = sections;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => switchKeyword, 
			1 => openParenToken, 
			2 => expression, 
			3 => closeParenToken, 
			4 => openBraceToken, 
			5 => sections, 
			6 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSwitchStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSwitchStatement(this);
	}

	public SwitchStatementSyntax Update(SyntaxToken switchKeyword, SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SwitchSectionSyntax> sections, SyntaxToken closeBraceToken)
	{
		if (switchKeyword != SwitchKeyword || openParenToken != OpenParenToken || expression != Expression || closeParenToken != CloseParenToken || openBraceToken != OpenBraceToken || sections != Sections || closeBraceToken != CloseBraceToken)
		{
			SwitchStatementSyntax switchStatementSyntax = SyntaxFactory.SwitchStatement(switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				switchStatementSyntax = switchStatementSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				switchStatementSyntax = switchStatementSyntax.WithAnnotationsGreen(annotations);
			}
			return switchStatementSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new SwitchStatementSyntax(Kind, switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new SwitchStatementSyntax(Kind, switchKeyword, openParenToken, expression, closeParenToken, openBraceToken, sections, closeBraceToken, GetDiagnostics(), annotations);
	}

	internal SwitchStatementSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 7;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			switchKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			openParenToken = syntaxToken2;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			expression = expressionSyntax;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			closeParenToken = syntaxToken3;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			openBraceToken = syntaxToken4;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			sections = greenNode;
		}
		SyntaxToken syntaxToken5 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken5 != null)
		{
			AdjustFlagsAndWidth(syntaxToken5);
			closeBraceToken = syntaxToken5;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(switchKeyword);
		writer.WriteValue(openParenToken);
		writer.WriteValue(expression);
		writer.WriteValue(closeParenToken);
		writer.WriteValue(openBraceToken);
		writer.WriteValue(sections);
		writer.WriteValue(closeBraceToken);
	}

	static SwitchStatementSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(SwitchStatementSyntax), (ObjectReader r) => new SwitchStatementSyntax(r));
	}
}
