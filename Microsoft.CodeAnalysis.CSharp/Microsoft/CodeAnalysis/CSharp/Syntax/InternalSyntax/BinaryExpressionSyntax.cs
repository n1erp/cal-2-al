using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class BinaryExpressionSyntax : ExpressionSyntax
{
	internal readonly ExpressionSyntax left;

	internal readonly SyntaxToken operatorToken;

	internal readonly ExpressionSyntax right;

	public ExpressionSyntax Left => left;

	public SyntaxToken OperatorToken => operatorToken;

	public ExpressionSyntax Right => right;

	internal BinaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal BinaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal BinaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(left);
		this.left = left;
		AdjustFlagsAndWidth(operatorToken);
		this.operatorToken = operatorToken;
		AdjustFlagsAndWidth(right);
		this.right = right;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)left, 
			1 => operatorToken, 
			2 => right, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBinaryExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBinaryExpression(this);
	}

	public BinaryExpressionSyntax Update(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
	{
		if (left != Left || operatorToken != OperatorToken || right != Right)
		{
			BinaryExpressionSyntax binaryExpressionSyntax = SyntaxFactory.BinaryExpression(Kind, left, operatorToken, right);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				binaryExpressionSyntax = binaryExpressionSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				binaryExpressionSyntax = binaryExpressionSyntax.WithAnnotationsGreen(annotations);
			}
			return binaryExpressionSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new BinaryExpressionSyntax(Kind, left, operatorToken, right, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new BinaryExpressionSyntax(Kind, left, operatorToken, right, GetDiagnostics(), annotations);
	}

	internal BinaryExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			left = expressionSyntax;
		}
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			operatorToken = syntaxToken;
		}
		ExpressionSyntax expressionSyntax2 = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax2 != null)
		{
			AdjustFlagsAndWidth(expressionSyntax2);
			right = expressionSyntax2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(left);
		writer.WriteValue(operatorToken);
		writer.WriteValue(right);
	}

	static BinaryExpressionSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(BinaryExpressionSyntax), (ObjectReader r) => new BinaryExpressionSyntax(r));
	}
}
