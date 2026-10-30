using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class QueryTableConnection : TableConnection
{
	private readonly NavSession session;

	internal override TableConnectionType ConnectionType => TableConnectionType.Query;

	internal override bool SupportsTransactions => false;

	private QueryTableConnection(ITreeObject parent, NavSession session, string name)
		: base(parent, name)
	{
		this.session = session;
	}

	internal static QueryTableConnection Create(ITreeObject parent, NavSession session, string name)
	{
		return new QueryTableConnection(parent, session, name);
	}

	internal DataAccess CreateDataAccess(NCLMetaTable table, GlobalFilters globalFilters)
	{
		CheckDisposed();
		return new DataAccess(session, new QueryTableDataProvider(session, table), globalFilters, DataAccessTableVersionTokens.CreateForTempTable(), VirtualAndTempTransactionalDataCache.Instance);
	}
}
