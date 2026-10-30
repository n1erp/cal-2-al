using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class PageRequestDefinition : IExtensibleDataObject
{
	private int[] normalFields;

	private int[] calcFields;

	private int pageSize;

	private int pageSizeInOppositeDirection;

	private ReadDirection readDirection;

	private byte[] startingBookmark;

	private bool excludeStartingRecord;

	private int[] lookupFieldIds;

	private object[] lookupFieldValues;

	private bool includeNonRowData;

	private StartingPage startFromPage;

	private bool includeMoreDataInformation = true;

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
	public bool IsSubFormUpdateRequest { get; set; }

	[DataMember]
	public int[] NormalFields
	{
		[DebuggerStepThrough]
		get
		{
			return normalFields;
		}
		[DebuggerStepThrough]
		set
		{
			normalFields = value;
		}
	}

	[DataMember]
	public int[] CalcFields
	{
		[DebuggerStepThrough]
		get
		{
			return calcFields;
		}
		[DebuggerStepThrough]
		set
		{
			calcFields = value;
		}
	}

	[DataMember]
	public int PageSize
	{
		[DebuggerStepThrough]
		get
		{
			return pageSize;
		}
		[DebuggerStepThrough]
		set
		{
			pageSize = value;
		}
	}

	[DataMember]
	public int PageSizeInOppositeDirection
	{
		[DebuggerStepThrough]
		get
		{
			return pageSizeInOppositeDirection;
		}
		[DebuggerStepThrough]
		set
		{
			pageSizeInOppositeDirection = value;
		}
	}

	[DataMember]
	public ReadDirection ReadDirection
	{
		[DebuggerStepThrough]
		get
		{
			return readDirection;
		}
		[DebuggerStepThrough]
		set
		{
			readDirection = value;
		}
	}

	[DataMember]
	public StartingPage StartFromPage
	{
		[DebuggerStepThrough]
		get
		{
			return startFromPage;
		}
		[DebuggerStepThrough]
		set
		{
			startFromPage = value;
		}
	}

	[DataMember]
	public bool IncludeMoreDataInformation
	{
		[DebuggerStepThrough]
		get
		{
			return includeMoreDataInformation;
		}
		[DebuggerStepThrough]
		set
		{
			includeMoreDataInformation = value;
		}
	}

	[DataMember]
	public bool IncludeNonRowData
	{
		[DebuggerStepThrough]
		get
		{
			return includeNonRowData;
		}
		[DebuggerStepThrough]
		set
		{
			includeNonRowData = value;
		}
	}

	[DataMember]
	public byte[] StartingBookmark
	{
		[DebuggerStepThrough]
		get
		{
			return startingBookmark;
		}
		[DebuggerStepThrough]
		set
		{
			startingBookmark = value;
		}
	}

	[DataMember]
	public bool ExcludeStartingRecord
	{
		[DebuggerStepThrough]
		get
		{
			return excludeStartingRecord;
		}
		[DebuggerStepThrough]
		set
		{
			excludeStartingRecord = value;
		}
	}

	[DataMember]
	public object[] LookupFieldValues
	{
		[DebuggerStepThrough]
		get
		{
			return lookupFieldValues;
		}
		[DebuggerStepThrough]
		set
		{
			lookupFieldValues = value;
		}
	}

	[DataMember]
	public int[] LookupFieldIds
	{
		[DebuggerStepThrough]
		get
		{
			return lookupFieldIds;
		}
		[DebuggerStepThrough]
		set
		{
			lookupFieldIds = value;
		}
	}
}
