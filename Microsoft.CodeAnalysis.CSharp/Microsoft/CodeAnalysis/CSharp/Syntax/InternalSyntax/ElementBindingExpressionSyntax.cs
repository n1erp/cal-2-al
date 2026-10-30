using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ElementBindingExpressionSyntax : ExpressionSyntax
{
	internal readonly BracketedArgumentListSyntax argumentList;

	public BracketedArgumentListSyntax ArgumentList => argumentList;

	internal ElementBindingExpressionSyntax(SyntaxKind kind, BracketedArgumentListSyntax argumentList, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ElementBindingExpressionSyntax(SyntaxKind kind, BracketedArgumentListSyntax argumentList, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal ElementBindingExpressionSyntax(SyntaxKind kind, BracketedArgumentListSyntax argumentList)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(argumentList);
		this.argumentList = argumentList;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return argumentList;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ElementBindingExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitElementBindingExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitElementBindingExpression(this);
	}

	public ElementBindingExpressionSyntax Update(BracketedArgumentListSyntax argumentList)
	{
		if (argumentList != ArgumentList)
		{
			ElementBindingExpressionSyntax elementBindingExpressionSyntax = SyntaxFactory.ElementBindingExpression(argumentList);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				elementBindingExpressionSyntax = elementBindingExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				elementBindingExpressionSyntax = elementBindingExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return elementBindingExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ElementBindingExpressionSyntax(Kind, argumentList, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ElementBindingExpressionSyntax(Kind, argumentList, GetDiagnostics(), annotations);
	}

	internal ElementBindingExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		BracketedArgumentListSyntax bracketedArgumentListSyntax = (BracketedArgumentListSyntax)reader.ReadValue();
		if (bracketedArgumentListSyntax != null)
		{
			AdjustFlagsAndWidth(bracketedArgumentListSyntax);
			argumentList = bracketedArgumentListSyntax;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(argumentList);
	}

	static ElementBindingExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ElementBindingExpressionSyntax), (ObjectReader r) => new ElementBindingExpressionSyntax(r));
	}
}
