using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDiscardExpression : BoundExpression
{
	public override Symbol ExpressionSymbol => new DiscardSymbol(Type);

	public override object Display => ((object)Type) ?? ((object)"_");

	public BoundExpression SetInferredType(TypeSymbol type)
	{
		return Update(type);
	}

	public BoundDiscardExpression FailInference(Binder binder, DiagnosticBag diagnosticsOpt)
	{
		if (diagnosticsOpt != null)
		{
			Binder.Error(diagnosticsOpt, ErrorCode.ERR_DiscardTypeInferenceFailed, Syntax);
		}
		return Update(binder.CreateErrorType("var"));
	}

	public BoundDiscardExpression(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.DiscardExpression, syntax, type, hasErrors)
	{
	}

	public BoundDiscardExpression(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.DiscardExpression, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDiscardExpression(this);
	}

	public BoundDiscardExpression Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundDiscardExpression(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
