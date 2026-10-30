using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class DynamicResultSetQueryEnumerator : IEnumerator<ReadOnlyRecordBuffer>, IDisposable, IEnumerator
{
	private readonly NCLMetaQueryDefinition queryDefinition;

	private readonly SqlTableDataProvider dataProvider;

	private readonly int companyToken;

	private readonly FiltersAndMarks queryFilters;

	private readonly TableFilterDictionary globalAndSecurityFilters;

	private readonly NavSession session;

	private readonly SecurityFiltering securityFiltering;

	private NCLMetaQueryDataItem[] dataItems;

	private int skipNumberOfRows;

	private int topNumberOfRows;

	private IEnumerator<ReadOnlyRecordBuffer> staticResultSetEnumerator;

	private Func<bool>[] bumperTokens;

	object IEnumerator.Current => staticResultSetEnumerator.Current;

	public ReadOnlyRecordBuffer Current => staticResultSetEnumerator.Current;

	internal DynamicResultSetQueryEnumerator(NCLMetaQueryDefinition queryDefinition, SqlTableDataProvider dataProvider, int companyToken, int topNumberOfRows, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, NavSession session, SecurityFiltering securityFiltering, IEnumerator<ReadOnlyRecordBuffer> staticResultSetEnumerator)
	{
		this.queryDefinition = queryDefinition;
		this.session = session;
		this.securityFiltering = securityFiltering;
		this.companyToken = companyToken;
		this.queryFilters = queryFilters;
		this.globalAndSecurityFilters = globalAndSecurityFilters;
		this.topNumberOfRows = topNumberOfRows;
		this.skipNumberOfRows = skipNumberOfRows;
		dataItems = NCLMetaQueryDefinition.GetAllDataItems(queryDefinition).ToArray();
		this.dataProvider = dataProvider;
		bumperTokens = new Func<bool>[dataItems.Length];
		this.staticResultSetEnumerator = staticResultSetEnumerator;
		RefreshResultSetBumperTokens();
	}

	private void RefreshResultSetBumperTokens()
	{
		for (int i = 0; i < dataItems.Length; i++)
		{
			bumperTokens[i] = session.DataAccessSource.TableVersionTokens.CreateTableVersionStillValidFunction(companyToken, dataItems[i].MetaTable.TableId, session.DataAccessSource.GlobalFilters, dynamic: true);
		}
	}

	public bool MoveNext()
	{
		bool flag = true;
		for (int i = 0; i < dataItems.Length; i++)
		{
			if (!bumperTokens[i]())
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			RefreshResultSetBumperTokens();
			MutableRecordBuffer startingPosition = ((staticResultSetEnumerator.Current == null) ? null : new MutableRecordBuffer(staticResultSetEnumerator.Current));
			staticResultSetEnumerator = dataProvider.GetQueryResult(queryDefinition, companyToken, topNumberOfRows, skipNumberOfRows, queryFilters, globalAndSecurityFilters, startingPosition, securityFiltering).GetEnumerator();
		}
		if (topNumberOfRows > 0)
		{
			topNumberOfRows--;
		}
		return staticResultSetEnumerator.MoveNext();
	}

	public void Reset()
	{
		staticResultSetEnumerator.Reset();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			bumperTokens = null;
			dataItems = null;
			staticResultSetEnumerator = null;
		}
	}
}
