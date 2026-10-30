using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class QueryTableDataProvider : ExternalDataProvider
{
	private readonly NavSession navSession;

	private readonly NCLMetaTable nclMetaTable;

	private readonly NCLMetaQuery nclMetaQuery;

	private static readonly NavBigInteger virtualTimeStamp = NavBigInteger.Create(1L);

	public override bool SupportsAutoCalcFields => false;

	internal QueryTableDataProvider(NavSession navSession, NCLMetaTable nclMetaTable)
		: base(nclMetaTable)
	{
		if (nclMetaTable == null)
		{
			throw new ArgumentNullException("nclMetaTable");
		}
		if (nclMetaTable.TableType != TableType.Query)
		{
			throw new NavNotSupportedException();
		}
		this.navSession = navSession ?? throw new ArgumentNullException("navSession");
		this.nclMetaTable = nclMetaTable;
		nclMetaQuery = this.navSession.NCLMetadata.GetMetaQueryById(this.nclMetaTable.TableId);
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		throw new NavNotSupportedException();
	}

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		DataProvider.ValidateNotNull(request, "request");
		using (NavQuery navQuery = nclMetaQuery.CreateObjectInstance(navSession))
		{
			navQuery.ALTopNumberOfRowsToReturn = 1;
			navQuery.ALSkipNumberOfRows = 0;
			List<NavValue> list = request.RecordId?.Fields?.ToList();
			if (list != null && list.Count == nclMetaTable.PrimaryKey.KeyFieldCount)
			{
				NavValue[] array = new NavValue[nclMetaQuery.QueryDefinition.Columns.Count];
				for (int i = 0; i < list.Count; i++)
				{
					NCLMetaField keyFieldByIndex = nclMetaTable.PrimaryKey.GetKeyFieldByIndex(i);
					array[keyFieldByIndex.ColumnIndex - 1] = list[i];
				}
				ApplyFilters(navQuery, request.FiltersAndMarks);
				navQuery.FindData(new MutableRecordBuffer(new ReadOnlyRecordBuffer(nclMetaQuery, array)), includeCurrent: true);
				if (navQuery.ALRead(DataError.TrapError))
				{
					recordBuffer = EntityToRecordBuffer(navQuery);
					return true;
				}
			}
		}
		recordBuffer = null;
		return false;
	}

	public override int Count(DataProviderRequest request)
	{
		using NavQuery navQuery = nclMetaQuery.CreateObjectInstance(navSession);
		ApplyFilters(navQuery, request.FiltersAndMarks);
		return navQuery.GetCount();
	}

	public override FieldDictionary<NavValue> CalcNumeric(CalcNumericProviderRequest request)
	{
		throw new NotSupportedException();
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		throw CreateReadOnlyException();
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		throw CreateReadOnlyException();
	}

	public override void ModifyAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldDictionary<NavValue> newFieldValues)
	{
		throw CreateReadOnlyException();
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		throw CreateReadOnlyException();
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		throw CreateReadOnlyException();
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> FindByPositionImplementation(PositionedFindProviderRequest request)
	{
		MutableRecordBuffer startingBuffer = null;
		if (request.StartingPosition != null)
		{
			List<NavValue> list = request.StartingPosition.ToArray().ToList();
			if (!list.TrueForAll((NavValue nv) => nv == null))
			{
				list.RemoveAt(0);
				startingBuffer = new MutableRecordBuffer(new ReadOnlyRecordBuffer(nclMetaQuery, list.ToArray()));
			}
		}
		SortingFieldList sortingFields = null;
		if (request.SortingFields != null)
		{
			sortingFields = SortingFieldList.Create(request.SortingFields.Select((INavSortingField sf) => NCLMetaQueryOrderBy.Create(sf.Order, nclMetaQuery.QueryDefinition.GetColumnByName(sf.Field.DescriptiveName), extraColumnAutomaticallyAddedForUniqueness: false)));
		}
		using NavQuery query = nclMetaQuery.CreateObjectInstance(navSession);
		query.ALSkipNumberOfRows = request.SkipNumberOfRows;
		query.ALTopNumberOfRowsToReturn = request.TopNumberOfRowsToReturn;
		ApplyFilters(query, request.FiltersAndMarks);
		query.FindData(startingBuffer, request.IncludeCurrent, sortingFields);
		while (query.ALRead(DataError.TrapError))
		{
			yield return EntityToRecordBuffer(query);
		}
	}

	private ReadOnlyRecordBuffer EntityToRecordBuffer(NavQuery query)
	{
		NavValue[] array = new NavValue[nclMetaTable.FieldCount];
		array[0] = virtualTimeStamp;
		NavValue[] array2 = query.CurrentDataRow.ReadOnlyBuffer.ToArray();
		Array.Copy(array2, 0, array, 1, array2.Length);
		return new ReadOnlyRecordBuffer(nclMetaTable, array);
	}

	private void ApplyFilters(NavQuery query, FiltersAndMarks filtersAndMarks)
	{
		IEnumerable<Tuple<NCLMetaQueryColumn, string>> enumerable = Enumerable.Empty<Tuple<NCLMetaQueryColumn, string>>();
		query.RunOnBeforeOpentrigger();
		if (filtersAndMarks?.Filters?.Items != null)
		{
			enumerable = filtersAndMarks.Filters.Items.Select((Tuple<INavFieldMetadata, FilterExpression> i) => Tuple.Create(nclMetaQuery.QueryDefinition.GetColumnByName(i.Item1.DescriptiveName), i.Item2.ToString()));
		}
		foreach (Tuple<NCLMetaQueryColumn, string> item in enumerable)
		{
			query.SetFilter(item.Item1, item.Item2);
		}
	}

	private NavCSideException CreateReadOnlyException()
	{
		return new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.TableIsReadOnly, MetaTable.TableId));
	}
}
