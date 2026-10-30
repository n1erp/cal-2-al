using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Apps;

public class NavAppTableModificationResponse : ServerResponse
{
	[DataMember]
	public Guid AppId { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public string Publisher { get; set; }

	[DataMember]
	public Version Version { get; set; }

	[DataMember]
	public IEnumerable<int> TablesModified { get; set; }

	[DataMember]
	public IEnumerable<int> TablesAdded { get; set; }

	[DataMember]
	public IEnumerable<NavAppDependencyResponse> Dependencies { get; set; }
}
