using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class ExternalDataProvider : DataProvider
{
	public int TableId => MetaTable.TableId;

	public NCLMetaTable MetaTable { get; private set; }

	public override bool ShouldResultSetBufferRows => false;

	public override bool SupportsAutoCalcFields => true;

	internal ExternalDataProvider(NCLMetaTable metaTable)
	{
		MetaTable = metaTable;
	}

	public override IEnumerable<ReadOnlyRecordBuffer> Find(FindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = FindImplementation(request);
		if (request.FindType != FindType.FirstOnly)
		{
			return enumerable;
		}
		return enumerable.Take(1);
	}

	public override IEnumerable<ReadOnlyRecordBuffer> FindFromPosition(PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = FindByPositionImplementation(request);
		if (request.FindType != FindType.FirstOnly)
		{
			return enumerable;
		}
		return enumerable.Take(1);
	}

	public override bool Exists(ExistsProviderRequest request)
	{
		return FindImplementation(MakeGenericFindrequest(request)).Any();
	}

	public override void BeginTransaction(TransactionType transactionType)
	{
	}

	public override void EndTransaction(bool commit)
	{
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		throw new NotSupportedException();
	}

	private IEnumerable<ReadOnlyRecordBuffer> FindImplementation(FindProviderRequest request)
	{
		PositionedFindProviderRequest request2 = new PositionedFindProviderRequest(request, null, includeCurrent: false);
		return FindByPositionImplementation(request2);
	}

	protected abstract IEnumerable<ReadOnlyRecordBuffer> FindByPositionImplementation(PositionedFindProviderRequest request);

	private static FindProviderRequest MakeGenericFindrequest(DataProviderRequest filteredDataRequest)
	{
		return new FindProviderRequest(filteredDataRequest.CompanyToken, filteredDataRequest.MetaApplicationObject, filteredDataRequest.LockState, filteredDataRequest.FiltersAndMarks, null, filteredDataRequest.FlowFieldSecurityFiltering, FieldList.Empty, ((NCLMetaTable)filteredDataRequest.MetaApplicationObject).PrimaryKey.DefinedSortingFields, FindType.All);
	}

	protected static FilterExpression GetCombinedExpressionFilter(FiltersAndMarks filtersAndMarks, FilterExpression positionFilter)
	{
		return FilterExpression.And(positionFilter, filtersAndMarks.GetCombinedFilter(), (filtersAndMarks.Filters == null) ? null : filtersAndMarks.Filters.ConcatenatedFilterFlowFields);
	}
}
