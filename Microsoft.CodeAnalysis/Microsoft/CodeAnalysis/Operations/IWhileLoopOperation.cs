namespace Microsoft.CodeAnalysis.Operations;

public interface IWhileLoopOperation : ILoopOperation, IOperation
{
	IOperation Condition { get; }

	bool ConditionIsTop { get; }

	bool ConditionIsUntil { get; }

	IOperation IgnoredCondition { get; }
}
