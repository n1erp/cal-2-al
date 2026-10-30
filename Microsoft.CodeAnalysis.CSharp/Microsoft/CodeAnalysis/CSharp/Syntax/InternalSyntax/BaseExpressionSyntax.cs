using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BaseExpressionSyntax : InstanceExpressionSyntax
{
	internal readonly SyntaxToken token;

	public SyntaxToken Token => token;

	internal BaseExpressionSyntax(SyntaxKind kind, SyntaxToken token, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal BaseExpressionSyntax(SyntaxKind kind, SyntaxToken token, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 1;
		AdjustFlagsAndWidth(token);
		this.token = token;
	}

	internal BaseExpressionSyntax(SyntaxKind kind, SyntaxToken token)
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
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BaseExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBaseExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBaseExpression(this);
	}

	public BaseExpressionSyntax Update(SyntaxToken token)
	{
		if (token != Token)
		{
			BaseExpressionSyntax baseExpressionSyntax = SyntaxFactory.BaseExpression(token);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				baseExpressionSyntax = baseExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				baseExpressionSyntax = baseExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return baseExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new BaseExpressionSyntax(Kind, token, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new BaseExpressionSyntax(Kind, token, GetDiagnostics(), annotations);
	}

	internal BaseExpressionSyntax(ObjectReader reader)
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

	static BaseExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(BaseExpressionSyntax), (ObjectReader r) => new BaseExpressionSyntax(r));
	}
}
