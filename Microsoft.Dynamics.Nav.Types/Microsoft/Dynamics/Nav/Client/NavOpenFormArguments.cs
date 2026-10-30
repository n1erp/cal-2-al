using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;

namespace Microsoft.Dynamics.Nav.Client;

public class NavOpenFormArguments
{
	public TableCaptions TableCaptions { get; set; }

	public FormProperties FormProperties { get; set; }

	public NavRecordState State { get; set; }

	public string ControlId { get; set; }

	public NavDataSet DataSet { get; set; }
}
