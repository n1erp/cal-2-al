using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

public class FormSelectionState
{
	[DataMember]
	public Guid ServerFormHandle { get; set; }

	[DataMember]
	public byte[][] SelectedRecords { get; set; }

	[DataMember]
	public byte[] CurrentRecord { get; set; }

	[DataMember]
	public bool AllSelected { get; set; }
}
