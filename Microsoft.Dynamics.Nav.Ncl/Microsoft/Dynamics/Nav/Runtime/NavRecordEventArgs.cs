using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavRecordEventArgs : EventArgs
{
	private NavRecordOperationTypes operationType;

	public NavRecordOperationTypes OperationType => operationType;

	public NavRecordEventArgs(NavRecordOperationTypes operationTypeParameter)
	{
		switch (operationTypeParameter)
		{
		default:
			throw new ArgumentException("operationType");
		case NavRecordOperationTypes.Insert:
		case NavRecordOperationTypes.Modify:
		case NavRecordOperationTypes.Delete:
		case NavRecordOperationTypes.Rename:
		case NavRecordOperationTypes.Navigation:
			operationType = operationTypeParameter;
			break;
		}
	}
}
