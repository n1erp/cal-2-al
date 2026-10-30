using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavTenantDatabaseSchemaFault
{
	private readonly NavTenantDatabaseSchemaErrorType errorType;

	private readonly string tableName;

	private readonly int tableId;

	private readonly string errorMessage;

	[DataMember]
	public NavTenantDatabaseSchemaErrorType ErrorType => errorType;

	[DataMember]
	public string TableName => tableName;

	[DataMember]
	public int TableId => tableId;

	[DataMember]
	public string ErrorMessage => errorMessage;

	public NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType errorType, string tableName, int tableId, string errorMessage = null)
	{
		this.errorType = errorType;
		this.tableName = tableName;
		this.tableId = tableId;
		this.errorMessage = errorMessage;
	}
}
