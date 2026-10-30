using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavTableView : ICloneable, IExtensibleDataObject
{
	private int tableId;

	private bool ascending;

	private NavFilterGroup[] currentFilters = new NavFilterGroup[0];

	private NavFilterGroup searchFilter;

	[NonSerialized]
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
	public int TableId
	{
		get
		{
			return tableId;
		}
		set
		{
			tableId = value;
		}
	}

	[DataMember]
	public int[] CurrentSortingFieldIds { get; set; }

	[DataMember]
	public bool Ascending
	{
		get
		{
			return ascending;
		}
		set
		{
			ascending = value;
		}
	}

	[DataMember]
	public NavFilterGroup[] CurrentFilters
	{
		get
		{
			return currentFilters;
		}
		set
		{
			currentFilters = value;
		}
	}

	[DataMember]
	public NavFilterGroup SearchFilter
	{
		get
		{
			return searchFilter;
		}
		set
		{
			searchFilter = value;
		}
	}

	public bool CompareCurrentSortingFieldIds(int[] sortingFieldIds)
	{
		if (CurrentSortingFieldIds.Length != sortingFieldIds.Length)
		{
			return false;
		}
		return !CurrentSortingFieldIds.Where((int t, int i) => t != sortingFieldIds[i]).Any();
	}

	public byte[] ToByteArray()
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, this);
		memoryStream.Flush();
		return memoryStream.ToArray();
	}

	public static NavTableView Create(byte[] bytes)
	{
		using MemoryStream serializationStream = new MemoryStream(bytes);
		return (NavTableView)new BinaryFormatter
		{
			Binder = new NavSerializationBinder(typeof(NavTableView))
		}.Deserialize(serializationStream);
	}

	public object Clone()
	{
		NavTableView navTableView = (NavTableView)MemberwiseClone();
		navTableView.CurrentFilters = new NavFilterGroup[CurrentFilters.Length];
		for (int i = 0; i < CurrentFilters.Length; i++)
		{
			navTableView.CurrentFilters[i] = (NavFilterGroup)CurrentFilters[i].Clone();
		}
		return navTableView;
	}
}
