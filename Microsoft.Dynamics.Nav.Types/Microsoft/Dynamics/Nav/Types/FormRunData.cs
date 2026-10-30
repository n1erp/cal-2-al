using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormRunData : IExtensibleDataObject
{
	private string fieldName;

	private bool lookupMode;

	private bool storeFilterPersonalization;

	private bool[] tableViewIsSet;

	private PageType pageType;

	private int masterPageId;

	private MasterPage page;

	private NavRecordState state;

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
	public bool StoreFilterPersonalization
	{
		get
		{
			return storeFilterPersonalization;
		}
		set
		{
			storeFilterPersonalization = value;
		}
	}

	[DataMember]
	public bool[] TableViewIsSet
	{
		get
		{
			return tableViewIsSet;
		}
		set
		{
			tableViewIsSet = value;
		}
	}

	[DataMember]
	public string FieldName
	{
		get
		{
			return fieldName;
		}
		set
		{
			fieldName = value;
		}
	}

	[DataMember]
	public Guid FormHandle { get; set; }

	[DataMember]
	public bool LookupMode
	{
		get
		{
			return lookupMode;
		}
		set
		{
			lookupMode = value;
		}
	}

	[DataMember]
	public ReportRequestOptions ReportRequestOptions { get; set; }

	[DataMember]
	public PageType PageType
	{
		get
		{
			return pageType;
		}
		set
		{
			pageType = value;
		}
	}

	[DataMember]
	public int MasterPageId
	{
		get
		{
			return masterPageId;
		}
		set
		{
			masterPageId = value;
		}
	}

	[DataMember]
	public MasterPage Page
	{
		get
		{
			return page;
		}
		set
		{
			page = value;
		}
	}

	[DataMember]
	public NavRecordState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

	public FormRunData()
	{
		ReportRequestOptions = new ReportRequestOptions();
	}
}
