using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ResultSetEnumerator
{
	private readonly NavSession session;

	private readonly DataAccess dataAccess;

	private readonly int transactionBumperToken;

	private readonly Func<bool> isStillValidFunc;

	protected int Index { get; set; }

	protected bool IsReversed { get; set; }

	public bool IsStillValid => isStillValidFunc();

	public bool IsCurrentResultValid
	{
		get
		{
			if (IsStillValid)
			{
				return !ResultSet.IsResultDeleted(Index);
			}
			return false;
		}
	}

	public ResultSet ResultSet { get; protected set; }

	protected internal DataAccess DataAccess => dataAccess;

	protected internal NavSession Session => session;

	public ResultSetEnumerator(ResultSet resultSet, NavSession session, DataAccess dataAccess, int transactionBumperToken, Func<bool> isStillValidFunc)
	{
		ResultSet = resultSet;
		this.session = session;
		this.dataAccess = dataAccess;
		this.transactionBumperToken = transactionBumperToken;
		this.isStillValidFunc = isStillValidFunc;
		Index = -1;
	}

	public ResultSetEnumerator Clone()
	{
		return new ResultSetEnumerator(ResultSet, Session, DataAccess, transactionBumperToken, isStillValidFunc)
		{
			Index = Index,
			IsReversed = IsReversed
		};
	}

	public ResultSetEnumerator CreateWithNewResultSet(ResultSet newResultSet)
	{
		return new ResultSetEnumerator(newResultSet, session, DataAccess, transactionBumperToken, isStillValidFunc);
	}

	public ResultSetEnumerator UpdateCurrentRowAndClone(ReadOnlyRecordBuffer recordBuffer, Func<bool> newIsStillValidFunc)
	{
		if (!ResultSet.TryUpdateAtIndex(Index, recordBuffer))
		{
			return this;
		}
		return new ResultSetEnumerator(ResultSet, Session, DataAccess, transactionBumperToken, newIsStillValidFunc)
		{
			Index = Index,
			IsReversed = IsReversed
		};
	}

	public ResultSetMoveResult TryMoveNext(out MutableRecordBuffer recordBuffer)
	{
		if (!IsStillValid)
		{
			recordBuffer = null;
			return ResultSetMoveResult.ResultSetNoLongerValid;
		}
		if (IsReversed)
		{
			return TryMovePreviousImpl(out recordBuffer);
		}
		return TryMoveNextImpl(out recordBuffer);
	}

	public ResultSetMoveResult TryMovePrevious(out MutableRecordBuffer recordBuffer)
	{
		if (!IsStillValid)
		{
			recordBuffer = null;
			return ResultSetMoveResult.ResultSetNoLongerValid;
		}
		if (IsReversed)
		{
			return TryMoveNextImpl(out recordBuffer);
		}
		return TryMovePreviousImpl(out recordBuffer);
	}

	private ResultSetMoveResult TryMoveNextImpl(out MutableRecordBuffer recordBuffer)
	{
		ResultSetMoveResult resultSetMoveResult = TryGetRecordBufferFromResultSet(Index + 1, out recordBuffer);
		if (resultSetMoveResult == ResultSetMoveResult.Ok)
		{
			Index++;
		}
		return resultSetMoveResult;
	}

	private ResultSetMoveResult TryMovePreviousImpl(out MutableRecordBuffer recordBuffer)
	{
		if (Index > 0)
		{
			ResultSetMoveResult resultSetMoveResult = TryGetRecordBufferFromResultSet(Index - 1, out recordBuffer);
			if (resultSetMoveResult == ResultSetMoveResult.Ok)
			{
				Index--;
			}
			return resultSetMoveResult;
		}
		if (!(ResultSet.FindRequest is PositionedFindCacheRequest { FindType: not FindType.FirstOnly } positionedFindCacheRequest))
		{
			recordBuffer = null;
			return ResultSetMoveResult.NoMoreResults;
		}
		if (!DataAccess.DataProvider.SupportReversedLoading)
		{
			recordBuffer = null;
			return ResultSetMoveResult.NoMoreResults;
		}
		PositionedFindCacheRequest reversedFindPositionRequest = GetReversedFindPositionRequest(positionedFindCacheRequest);
		ResultSetEnumerator resultSetEnumerator = dataAccess.FindFromPosition(reversedFindPositionRequest);
		ResultSet resultSet = ResultSet;
		int index = Index;
		ResultSet = resultSetEnumerator.ResultSet;
		Index = -1;
		IsReversed = !IsReversed;
		ResultSetMoveResult resultSetMoveResult2 = TryMoveNextImpl(out recordBuffer);
		if (resultSetMoveResult2 != ResultSetMoveResult.Ok)
		{
			ResultSet = resultSet;
			Index = index;
			IsReversed = !IsReversed;
		}
		return resultSetMoveResult2;
	}

	private ResultSetMoveResult TryGetRecordBufferFromResultSet(int fetchIndex, out MutableRecordBuffer recordBuffer)
	{
		ResultSetMoveResult resultSetMoveResult = ResultSet.TryGetAtIndex(fetchIndex, out var recordBuffer2);
		if (resultSetMoveResult == ResultSetMoveResult.Ok)
		{
			recordBuffer = new MutableRecordBuffer(recordBuffer2, transactionBumperToken);
			return ResultSetMoveResult.Ok;
		}
		recordBuffer = null;
		return resultSetMoveResult;
	}

	private static PositionedFindCacheRequest GetReversedFindPositionRequest(PositionedFindCacheRequest positionedFindDataCacheRequest)
	{
		return new PositionedFindCacheRequest(positionedFindDataCacheRequest.CompanyToken, positionedFindDataCacheRequest.MetaApplicationObject, positionedFindDataCacheRequest.Cacheable, positionedFindDataCacheRequest.FiltersAndMarks, null, positionedFindDataCacheRequest.FlowFieldSecurityFiltering, positionedFindDataCacheRequest.AutoCalcFields, positionedFindDataCacheRequest.SortingFields.Reversed, positionedFindDataCacheRequest.FindType, positionedFindDataCacheRequest.StartingPosition, !positionedFindDataCacheRequest.IncludeCurrent, positionedFindDataCacheRequest.TopNumberOfRowsToReturn, positionedFindDataCacheRequest.SkipNumberOfRows, positionedFindDataCacheRequest.Timeout);
	}
}
