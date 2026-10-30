using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ApplicationMethodRequest : IExtensibleDataObject
{
	private NavDataSet dataset;

	private ExtensionDataObject unknownDataBag;

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember]
	public ApplicationCodeType ApplicationCodeType { get; set; }

	[DataMember]
	public Guid ApplicationCodeGuid { get; set; }

	[DataMember]
	public int ObjectId { get; set; }

	[DataMember]
	public string MethodName { get; set; }

	[DataMember]
	public NavDataSet DataSet
	{
		get
		{
			if (dataset != null && dataset.Tables.Count == 0)
			{
				return null;
			}
			return dataset;
		}
		set
		{
			if (value != null && value.Tables.Count == 0)
			{
				dataset = value;
			}
			dataset = value;
		}
	}

	[DataMember]
	public object[] Args { get; set; }

	[DataMember]
	public NavRecordState DataSetState { get; set; }

	[DataMember]
	public IList<string> TextualFilters { get; set; }

	[DataMember]
	public IList<NavFilter> Filters { get; set; }

	[DataMember]
	public MetaQuery Query { get; set; }

	[DataMember]
	public int? RequestedTopNumberOfRows { get; set; }
}
