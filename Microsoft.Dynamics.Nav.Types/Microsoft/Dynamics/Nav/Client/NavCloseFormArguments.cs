using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Client;

public class NavCloseFormArguments
{
	public NavRecordState State { get; set; }

	public NavDataSet DataSet { get; set; }

	public FormResult ExitAction { get; set; }
}
