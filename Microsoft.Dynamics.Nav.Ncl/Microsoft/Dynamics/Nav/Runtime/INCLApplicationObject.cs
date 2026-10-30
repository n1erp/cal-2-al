using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INCLApplicationObject<in TSubject>
{
	NavAppRuntimeMetadata OwningApp { get; }

	void ApplyRuntimeDeltas(ApplicationObjectId subjectId, TSubject subject);
}
