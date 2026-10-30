using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal interface ISyncValidator
{
	IReadOnlyList<string> Validate(ObjectType objectType, int objectId, SyncTableBundle bundle);
}
