using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class InitializerExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode expressions;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> Expressions => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(expressions));

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal InitializerExpressionSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode expressions, SyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			this.expressions = expressions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal InitializerExpressionSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode expressions, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			this.expressions = expressions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal InitializerExpressionSyntax(SyntaxKind kind, SyntaxToken openBraceToken, GreenNode expressions, SyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (expressions != null)
		{
			AdjustFlagsAndWidth(expressions);
			this.expressions = expressions;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => openBraceToken, 
			1 => expressions, 
			2 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInitializerExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInitializerExpression(this);
	}

	public InitializerExpressionSyntax Update(SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ExpressionSyntax> expressions, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || expressions != Expressions || closeBraceToken != CloseBraceToken)
		{
			InitializerExpressionSyntax initializerExpressionSyntax = SyntaxFactory.InitializerExpression(Kind, openBraceToken, expressions, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				initializerExpressionSyntax = initializerExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				initializerExpressionSyntax = initializerExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return initializerExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InitializerExpressionSyntax(Kind, openBraceToken, expressions, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InitializerExpressionSyntax(Kind, openBraceToken, expressions, closeBraceToken, GetDiagnostics(), annotations);
	}

	internal InitializerExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			openBraceToken = syntaxToken;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			expressions = greenNode;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			closeBraceToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(openBraceToken);
		writer.WriteValue(expressions);
		writer.WriteValue(closeBraceToken);
	}

	static InitializerExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(InitializerExpressionSyntax), (ObjectReader r) => new InitializerExpressionSyntax(r));
	}
}
