using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppUpgradePrecheckResult
{
	public NavAppRuntimeMetadata AppBeingUpgradedTo { get; set; }

	public INavAppReference OldAppBeingUpgradedFrom { get; set; }

	public ChangeNavAppStateResult Result { get; set; }

	public string Error { get; set; }

	public NavAppUpgradePrecheckResult()
	{
		Result = ChangeNavAppStateResult.AppChangeSuccessful;
	}
}
