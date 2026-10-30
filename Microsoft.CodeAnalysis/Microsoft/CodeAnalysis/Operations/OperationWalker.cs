using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

public abstract class OperationWalker : OperationVisitor
{
	private int _recursionDepth;

	internal void VisitArray<T>(IEnumerable<T> operations) where T : IOperation
	{
		foreach (T operation in operations)
		{
			VisitOperationArrayElement(operation);
		}
	}

	internal void VisitOperationArrayElement<T>(T operation) where T : IOperation
	{
		Visit(operation);
	}

	public override void Visit(IOperation operation)
	{
		if (operation != null)
		{
			_recursionDepth++;
			try
			{
				StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
				operation.Accept(this);
			}
			finally
			{
				_recursionDepth--;
			}
		}
	}

	public override void DefaultVisit(IOperation operation)
	{
		VisitArray(operation.Children);
	}

	internal override void VisitNoneOperation(IOperation operation)
	{
		VisitArray(operation.Children);
	}
}
