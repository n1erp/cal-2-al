using System.Globalization;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class QueryClientCallback : DataItemIteratorClientCallback
{
	private readonly NavQuery navQuery;

	private int? requestedTopNumberOfRows;

	public QueryClientCallback(NavQuery query)
		: base(NavDataSetBuilder.CreateNavDataSet(query.NCLMetaQuery.MetaQuery), query.ExecutionGuid, query.Paging)
	{
		if (query.Paging)
		{
			ClientCallback.VerifyCallbackAllowed(query);
		}
		navQuery = query;
	}

	protected override void CancelOnTargetDevice()
	{
		if (navQuery.Paging)
		{
			DataSetRequest dataSetRequest = new DataSetRequest();
			dataSetRequest.ExecutionGuid = navQuery.ExecutionGuid;
			dataSetRequest.Cancel = true;
			dataSetRequest.FinalDataSet = true;
			dataSetRequest.Information = new QueryRequestInformation();
			ClientCallback.DataSetPageReady(dataSetRequest);
		}
	}

	protected override DataSetRequest CreateRequest(bool isFinalDataSetPiece)
	{
		DataSetRequest dataSetRequest = base.CreateRequest(isFinalDataSetPiece);
		QueryRequestInformation queryRequestInformation = (QueryRequestInformation)(dataSetRequest.Information = new QueryRequestInformation());
		queryRequestInformation.MetadataToken = NavGlobal.MetadataProvider.VersionNumber;
		queryRequestInformation.Id = navQuery.NCLMetaQuery.ObjectId;
		queryRequestInformation.Caption = ((!requestedTopNumberOfRows.HasValue) ? navQuery.Caption : string.Format(CultureInfo.InvariantCulture, "{0} - Preview of first {1} rows.", navQuery.Caption, requestedTopNumberOfRows.Value));
		return dataSetRequest;
	}

	public void SetSpecialCaptionInformation(int topNumber)
	{
		requestedTopNumberOfRows = topNumber;
	}
}
