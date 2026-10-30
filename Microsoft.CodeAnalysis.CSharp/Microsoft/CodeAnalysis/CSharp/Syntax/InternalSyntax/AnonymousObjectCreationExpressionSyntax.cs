using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class AnonymousObjectCreationExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly SyntaxToken openBraceToken;

	internal readonly GreenNode initializers;

	internal readonly SyntaxToken closeBraceToken;

	public SyntaxToken NewKeyword => newKeyword;

	public SyntaxToken OpenBraceToken => openBraceToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> Initializers => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax>(new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>(initializers));

	public SyntaxToken CloseBraceToken => closeBraceToken;

	internal AnonymousObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openBraceToken, GreenNode initializers, SyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (initializers != null)
		{
			AdjustFlagsAndWidth(initializers);
			this.initializers = initializers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal AnonymousObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openBraceToken, GreenNode initializers, SyntaxToken closeBraceToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (initializers != null)
		{
			AdjustFlagsAndWidth(initializers);
			this.initializers = initializers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal AnonymousObjectCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openBraceToken, GreenNode initializers, SyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		this.openBraceToken = openBraceToken;
		if (initializers != null)
		{
			AdjustFlagsAndWidth(initializers);
			this.initializers = initializers;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		this.closeBraceToken = closeBraceToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => newKeyword, 
			1 => openBraceToken, 
			2 => initializers, 
			3 => closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousObjectCreationExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAnonymousObjectCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAnonymousObjectCreationExpression(this);
	}

	public AnonymousObjectCreationExpressionSyntax Update(SyntaxToken newKeyword, SyntaxToken openBraceToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers, SyntaxToken closeBraceToken)
	{
		if (newKeyword != NewKeyword || openBraceToken != OpenBraceToken || initializers != Initializers || closeBraceToken != CloseBraceToken)
		{
			AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax = SyntaxFactory.AnonymousObjectCreationExpression(newKeyword, openBraceToken, initializers, closeBraceToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				anonymousObjectCreationExpressionSyntax = anonymousObjectCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				anonymousObjectCreationExpressionSyntax = anonymousObjectCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return anonymousObjectCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new AnonymousObjectCreationExpressionSyntax(Kind, newKeyword, openBraceToken, initializers, closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new AnonymousObjectCreationExpressionSyntax(Kind, newKeyword, openBraceToken, initializers, closeBraceToken, GetDiagnostics(), annotations);
	}

	internal AnonymousObjectCreationExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			newKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			openBraceToken = syntaxToken2;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			initializers = greenNode;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			closeBraceToken = syntaxToken3;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(newKeyword);
		writer.WriteValue(openBraceToken);
		writer.WriteValue(initializers);
		writer.WriteValue(closeBraceToken);
	}

	static AnonymousObjectCreationExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(AnonymousObjectCreationExpressionSyntax), (ObjectReader r) => new AnonymousObjectCreationExpressionSyntax(r));
	}
}
