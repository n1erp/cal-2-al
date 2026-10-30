using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DeconstructionVariablePendingInference : VariablePendingInference
{
	public override object Display
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	protected override ErrorCode InferenceFailedError => ErrorCode.ERR_TypeInferenceFailedForImplicitlyTypedDeconstructionVariable;

	public DeconstructionVariablePendingInference(SyntaxNode syntax, Symbol variableSymbol, BoundExpression receiverOpt, bool hasErrors = false)
		: base(BoundKind.DeconstructionVariablePendingInference, syntax, variableSymbol, receiverOpt, hasErrors || receiverOpt.HasErrors())
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDeconstructionVariablePendingInference(this);
	}

	public DeconstructionVariablePendingInference Update(Symbol variableSymbol, BoundExpression receiverOpt)
	{
		if (variableSymbol != VariableSymbol || receiverOpt != ReceiverOpt)
		{
			return new DeconstructionVariablePendingInference(Syntax, variableSymbol, receiverOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
