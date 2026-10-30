using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ThisExpressionSyntax : InstanceExpressionSyntax
{
	internal readonly SyntaxToken token;

	public SyntaxToken Token => token;

	internal ThisExpressionSyntax(SyntaxKind kind, SyntaxToken token, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal ThisExpressionSyntax(SyntaxKind kind, SyntaxToken token, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal ThisExpressionSyntax(SyntaxKind kind, SyntaxToken token)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal override GreenNode GetSlot(int index)
	{
		if (index == 0)
		{
			return token;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitThisExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitThisExpression(this);
	}

	public ThisExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			ThisExpressionSyntax thisExpressionSyntax = SyntaxFactory.ThisExpression(token);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				thisExpressionSyntax = thisExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				thisExpressionSyntax = thisExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return thisExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ThisExpressionSyntax(Kind, token, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ThisExpressionSyntax(Kind, token, GetDiagnostics(), annotations);
	}

	internal ThisExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 1;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			token = syntaxToken;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(token);
	}

	static ThisExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ThisExpressionSyntax), (ObjectReader r) => new ThisExpressionSyntax(r));
	}
}
