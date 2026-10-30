using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal interface INavAppTenantOperationRepository
{
	string TenantId { get; }

	void DeleteOperationForId(Guid operationId);

	IEnumerable<NavAppTenantOperationRecord> GetAll();

	NavAppTenantOperationRecord GetOperationForId(Guid operationId);

	NavAppTenantOperationRecord GetOperationForResult(NavAppOperationResult result);

	void UpdateOperationStatus(Guid operationId, NavAppTenantOperationFields.NavAppOperationStatus status, string statusDetails);

	Guid UpdateOrInsert(NavAppOperationRequestReference reference);
}
