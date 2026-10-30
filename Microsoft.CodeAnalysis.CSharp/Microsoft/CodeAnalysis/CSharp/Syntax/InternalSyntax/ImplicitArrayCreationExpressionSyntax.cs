using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ImplicitArrayCreationExpressionSyntax : ExpressionSyntax
{
	internal readonly SyntaxToken newKeyword;

	internal readonly SyntaxToken openBracketToken;

	internal readonly GreenNode commas;

	internal readonly SyntaxToken closeBracketToken;

	internal readonly InitializerExpressionSyntax initializer;

	public SyntaxToken NewKeyword => newKeyword;

	public SyntaxToken OpenBracketToken => openBracketToken;

	public Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Commas => new Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>(commas);

	public SyntaxToken CloseBracketToken => closeBracketToken;

	public InitializerExpressionSyntax Initializer => initializer;

	internal ImplicitArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openBracketToken, GreenNode commas, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (commas != null)
		{
			AdjustFlagsAndWidth(commas);
			this.commas = commas;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal ImplicitArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openBracketToken, GreenNode commas, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 5;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (commas != null)
		{
			AdjustFlagsAndWidth(commas);
			this.commas = commas;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal ImplicitArrayCreationExpressionSyntax(SyntaxKind kind, SyntaxToken newKeyword, SyntaxToken openBracketToken, GreenNode commas, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(newKeyword);
		this.newKeyword = newKeyword;
		AdjustFlagsAndWidth(openBracketToken);
		this.openBracketToken = openBracketToken;
		if (commas != null)
		{
			AdjustFlagsAndWidth(commas);
			this.commas = commas;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		this.closeBracketToken = closeBracketToken;
		AdjustFlagsAndWidth(initializer);
		this.initializer = initializer;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => newKeyword, 
			1 => openBracketToken, 
			2 => commas, 
			3 => closeBracketToken, 
			4 => initializer, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitImplicitArrayCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitImplicitArrayCreationExpression(this);
	}

	public ImplicitArrayCreationExpressionSyntax Update(SyntaxToken newKeyword, SyntaxToken openBracketToken, Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> commas, SyntaxToken closeBracketToken, InitializerExpressionSyntax initializer)
	{
		if (newKeyword != NewKeyword || openBracketToken != OpenBracketToken || commas != Commas || closeBracketToken != CloseBracketToken || initializer != Initializer)
		{
			ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpressionSyntax = SyntaxFactory.ImplicitArrayCreationExpression(newKeyword, openBracketToken, commas, closeBracketToken, initializer);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				implicitArrayCreationExpressionSyntax = implicitArrayCreationExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				implicitArrayCreationExpressionSyntax = implicitArrayCreationExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return implicitArrayCreationExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ImplicitArrayCreationExpressionSyntax(Kind, newKeyword, openBracketToken, commas, closeBracketToken, initializer, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ImplicitArrayCreationExpressionSyntax(Kind, newKeyword, openBracketToken, commas, closeBracketToken, initializer, GetDiagnostics(), annotations);
	}

	internal ImplicitArrayCreationExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 5;
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
			openBracketToken = syntaxToken2;
		}
		GreenNode greenNode = (GreenNode)reader.ReadValue();
		if (greenNode != null)
		{
			AdjustFlagsAndWidth(greenNode);
			commas = greenNode;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			closeBracketToken = syntaxToken3;
		}
		InitializerExpressionSyntax initializerExpressionSyntax = (InitializerExpressionSyntax)reader.ReadValue();
		if (initializerExpressionSyntax != null)
		{
			AdjustFlagsAndWidth(initializerExpressionSyntax);
			initializer = initializerExpressionSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(newKeyword);
		writer.WriteValue(openBracketToken);
		writer.WriteValue(commas);
		writer.WriteValue(closeBracketToken);
		writer.WriteValue(initializer);
	}

	static ImplicitArrayCreationExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ImplicitArrayCreationExpressionSyntax), (ObjectReader r) => new ImplicitArrayCreationExpressionSyntax(r));
	}
}
