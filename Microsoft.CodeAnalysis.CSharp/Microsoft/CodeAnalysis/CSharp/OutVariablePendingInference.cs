namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class OutVariablePendingInference : VariablePendingInference
{
	public override object Display => string.Empty;

	protected override ErrorCode InferenceFailedError => ErrorCode.ERR_TypeInferenceFailedForImplicitlyTypedOutVariable;

	public OutVariablePendingInference(SyntaxNode syntax, Symbol variableSymbol, BoundExpression receiverOpt, bool hasErrors = false)
		: base(BoundKind.OutVariablePendingInference, syntax, variableSymbol, receiverOpt, hasErrors || receiverOpt.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitOutVariablePendingInference(this);
	}

	public OutVariablePendingInference Update(Symbol variableSymbol, BoundExpression receiverOpt)
	{
		if (variableSymbol != VariableSymbol || receiverOpt != ReceiverOpt)
		{
			return new OutVariablePendingInference(Syntax, variableSymbol, receiverOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
