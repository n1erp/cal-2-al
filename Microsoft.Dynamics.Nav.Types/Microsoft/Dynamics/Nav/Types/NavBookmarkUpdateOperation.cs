using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public struct NavBookmarkUpdateOperation
{
	private NavRecordOperationTypes operation;

	private byte[] recordId;

	private byte[] newRecordId;

	public byte[] NewRecordId
	{
		[DebuggerStepThrough]
		get
		{
			return newRecordId;
		}
		[DebuggerStepThrough]
		set
		{
			newRecordId = value;
		}
	}

	public byte[] RecordId
	{
		[DebuggerStepThrough]
		get
		{
			return recordId;
		}
		[DebuggerStepThrough]
		set
		{
			recordId = value;
		}
	}

	public NavRecordOperationTypes Operation
	{
		[DebuggerStepThrough]
		get
		{
			return operation;
		}
		[DebuggerStepThrough]
		set
		{
			operation = value;
		}
	}
}
