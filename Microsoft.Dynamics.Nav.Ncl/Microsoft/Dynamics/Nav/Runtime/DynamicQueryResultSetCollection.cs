using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class DynamicQueryResultSetCollection : IEnumerable<ReadOnlyRecordBuffer>, IEnumerable
{
	private readonly NCLMetaQueryDefinition queryDefinition;

	private readonly SqlTableDataProvider dataProvider;

	private readonly int companyToken;

	private readonly int skipNumberOfRows;

	private readonly int topNumberOfRows;

	private readonly FiltersAndMarks queryFilters;

	private readonly TableFilterDictionary globalAndSecurityFilters;

	private readonly NavSession session;

	private readonly SecurityFiltering securityFiltering;

	private IEnumerable<ReadOnlyRecordBuffer> staticResultSet;

	internal DynamicQueryResultSetCollection(NCLMetaQueryDefinition queryDefinition, SqlTableDataProvider dataProvider, int companyToken, int topNumberOfRows, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, NavSession session, SecurityFiltering securityFiltering, IEnumerable<ReadOnlyRecordBuffer> staticResultSet)
	{
		this.queryDefinition = queryDefinition;
		this.dataProvider = dataProvider;
		this.companyToken = companyToken;
		this.topNumberOfRows = topNumberOfRows;
		this.skipNumberOfRows = skipNumberOfRows;
		this.queryFilters = queryFilters;
		this.globalAndSecurityFilters = globalAndSecurityFilters;
		this.session = session;
		this.securityFiltering = securityFiltering;
		this.staticResultSet = staticResultSet;
	}

	public IEnumerator<ReadOnlyRecordBuffer> GetEnumerator()
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = staticResultSet;
		staticResultSet = null;
		return new DynamicResultSetQueryEnumerator(queryDefinition, dataProvider, companyToken, topNumberOfRows, skipNumberOfRows, queryFilters, globalAndSecurityFilters, session, securityFiltering, enumerable.GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
