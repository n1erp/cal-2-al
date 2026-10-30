using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class OutDeconstructVarPendingInference : BoundExpression
{
	public BoundDeconstructValuePlaceholder Placeholder;

	public override object Display => string.Empty;

	public BoundDeconstructValuePlaceholder SetInferredType(TypeSymbol type, Binder binder, bool success)
	{
		Placeholder = new BoundDeconstructValuePlaceholder(Syntax, binder.LocalScopeDepth, type, HasErrors || !success);
		return Placeholder;
	}

	public BoundDeconstructValuePlaceholder FailInference(Binder binder)
	{
		return SetInferredType(binder.CreateErrorType(), binder, success: false);
	}

	public OutDeconstructVarPendingInference(SyntaxNode syntax, bool hasErrors)
		: base(BoundKind.OutDeconstructVarPendingInference, syntax, null, hasErrors)
	{
	}

	public OutDeconstructVarPendingInference(SyntaxNode syntax)
		: base(BoundKind.OutDeconstructVarPendingInference, syntax, null)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitOutDeconstructVarPendingInference(this);
	}

	public OutDeconstructVarPendingInference Update()
	{
		return this;
	}
}
