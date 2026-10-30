using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class GetReportOptionsEventArgs : EventArgs
{
	public string ReportOptions { get; set; }

	public ObjectType ObjectType { get; set; }
}
