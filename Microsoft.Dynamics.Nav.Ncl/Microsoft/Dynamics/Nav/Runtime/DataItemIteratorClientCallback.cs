using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class DataItemIteratorClientCallback : DataItemResultSetProcessor
{
	private const int PagingTreshold = 100;

	private Guid executionId;

	private bool paging;

	public IClientCallback ClientCallback { get; private set; }

	public bool IsPageFull
	{
		get
		{
			if (paging)
			{
				return RowCounter >= 100;
			}
			return false;
		}
	}

	protected DataItemIteratorClientCallback(NavDataSet dataSet, Guid executionId, bool paging)
		: base(dataSet)
	{
		this.executionId = executionId;
		this.paging = paging;
		ClientCallback = NavCurrentThread.Session.ClientCallback;
	}

	protected virtual void SendDataSetToTargetDevice(bool isFinalDataSetPiece)
	{
		if (paging && ClientCallback != null)
		{
			DataSetRequest request = CreateRequest(isFinalDataSetPiece);
			if (ClientCallback.DataSetPageReady(request))
			{
				throw new NavControlException(NavControlStatement.ClientCancel);
			}
		}
	}

	protected virtual DataSetRequest CreateRequest(bool isFinalDataSetPiece)
	{
		DataSetRequest dataSetRequest = new DataSetRequest();
		dataSetRequest.DataSet = DataSet;
		dataSetRequest.FinalDataSet = isFinalDataSetPiece;
		dataSetRequest.ExecutionGuid = executionId;
		return dataSetRequest;
	}

	protected abstract void CancelOnTargetDevice();

	public void ResetPage()
	{
		DataSet = DataSet.CloneStructure();
		RowCounter = 0;
	}

	public override void Finish()
	{
		base.Finish();
		SendDataSetToTargetDevice(isFinalDataSetPiece: true);
	}

	public override void EndOfRow()
	{
		if (IsPageFull)
		{
			SendDataSetToTargetDevice(isFinalDataSetPiece: false);
			ResetPage();
		}
		base.EndOfRow();
	}

	public override void Abort()
	{
		CancelOnTargetDevice();
	}

	public override void CleanupOnError()
	{
	}
}
