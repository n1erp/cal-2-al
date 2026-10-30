using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Data;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[DebuggerDisplay("{dataSetName}")]
public sealed class NavDataSet : IExtensibleDataObject
{
	private const string DefaultDataSetName = "NewDataSet";

	private CultureInfo culture;

	private string dataSetName;

	private NavDataTableCollection tables;

	private ExtensionDataObject unknownDataBag;

	private static int CompressionThreshold;

	private readonly bool useCompactSerialization = ((ConfigSettings.Instance != null) ? ConfigSettings.Instance.EnableCompactSerialization : FeatureSwitches.Instance.EnableCompactSerialization);

	ExtensionDataObject IExtensibleDataObject.ExtensionData
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

	public string DataSetName
	{
		get
		{
			return dataSetName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Cannot change the name of the DataSet to an empty string.", "value");
			}
			dataSetName = value;
		}
	}

	public CultureInfo Locale
	{
		get
		{
			return culture;
		}
		set
		{
			if (value == null || culture.Equals(value))
			{
				return;
			}
			culture = value;
			foreach (NavDataTable table in Tables)
			{
				table.Locale = value;
			}
		}
	}

	public NavDataTableCollection Tables => tables;

	public bool HasData
	{
		get
		{
			if (Tables.Count > 0)
			{
				return Tables.Any((NavDataTable table) => table.Rows.Count > 0);
			}
			return false;
		}
	}

	[DataMember]
	private HybridDataStore DataSet
	{
		get
		{
			return Serialize();
		}
		set
		{
			Deserialize(value);
		}
	}

	private static void InitializeCompressionThreshold()
	{
		if (CompressionThreshold == 0)
		{
			CompressionThreshold = ((ConfigSettings.Instance != null) ? ConfigSettings.Instance.ClientServicesCompressionThreshold : ServerUserSettings.Instance.ClientServicesCompressionThreshold.ValueKB);
		}
	}

	public NavDataSet()
	{
		tables = new NavDataTableCollection(this);
		dataSetName = "NewDataSet";
		culture = CultureInfo.CurrentCulture;
	}

	public NavDataSet(HybridDataStore data)
		: this()
	{
		Deserialize(data);
	}

	public void Clear()
	{
		if (tables != null)
		{
			for (int i = 0; i < tables.Count; i++)
			{
				tables[i].Clear();
			}
		}
	}

	private HybridDataStore Serialize()
	{
		InitializeCompressionThreshold();
		using HybridMemoryCompressStream hybridMemoryCompressStream = new HybridMemoryCompressStream(CompressionThreshold);
		using BinaryWriter binaryWriter = new BinaryWriter(hybridMemoryCompressStream);
		binaryWriter.Write(DataSetName);
		binaryWriter.Write(Locale.LCID);
		binaryWriter.Write(Tables.Count);
		foreach (NavDataTable table in Tables)
		{
			binaryWriter.Write(table.TableName);
			binaryWriter.Write(table.Locale.LCID);
			binaryWriter.Write(table.Columns.Count);
			foreach (NavDataColumn column in table.Columns)
			{
				binaryWriter.Write(column.ColumnName);
				binaryWriter.Write((byte)column.NavTypeCode);
				if (column.HasExtendedProperties)
				{
					binaryWriter.Write(column.ExtendedProperties.Count);
					foreach (object key in column.ExtendedProperties.Keys)
					{
						NavSerializer.WriteTypeAndItem(binaryWriter, key.GetType().TypeHandle, key, useCompactSerialization);
						object obj = column.ExtendedProperties[key];
						NavSerializer.WriteTypeAndItem(binaryWriter, obj.GetType().TypeHandle, obj, useCompactSerialization);
					}
				}
				else
				{
					binaryWriter.Write(0);
				}
			}
		}
		foreach (NavDataTable table2 in Tables)
		{
			binaryWriter.Write(table2.Rows.Count);
			foreach (NavDataRow row in table2.Rows)
			{
				for (int i = 0; i < table2.Columns.Count; i++)
				{
					NavSerializer.WriteItem(binaryWriter, table2.Columns[i].NavTypeCode, row[i], useCompactSerialization);
				}
			}
		}
		return hybridMemoryCompressStream.GetDataStore();
	}

	private void Deserialize(HybridDataStore data)
	{
		if (data == null)
		{
			return;
		}
		using HybridMemoryDecompressStream input = new HybridMemoryDecompressStream(data);
		using BinaryReader binaryReader = new BinaryReader(input);
		ClearAll();
		DataSetName = binaryReader.ReadString();
		Locale = new CultureInfo(binaryReader.ReadInt32());
		int num = binaryReader.ReadInt32();
		if (num > 0)
		{
			Tables.Clear();
		}
		for (int i = 0; i < num; i++)
		{
			Tables.Add(binaryReader.ReadString());
			Tables[i].Locale = new CultureInfo(binaryReader.ReadInt32());
			int num2 = binaryReader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				string columnName = binaryReader.ReadString();
				NavTypeCode typeCode = (NavTypeCode)binaryReader.ReadByte();
				NavDataColumn navDataColumn = Tables[i].Columns.Add(columnName, typeCode);
				int num3 = binaryReader.ReadInt32();
				if (num3 > 0)
				{
					for (int k = 0; k < num3; k++)
					{
						object key = NavSerializer.ReadTypeAndItem(binaryReader);
						object value = NavSerializer.ReadTypeAndItem(binaryReader);
						navDataColumn.ExtendedProperties.Add(key, value);
					}
				}
			}
		}
		foreach (NavDataTable table in Tables)
		{
			int count = table.Columns.Count;
			int num4 = binaryReader.ReadInt32();
			for (int l = 0; l < num4; l++)
			{
				NavDataRow navDataRow = table.NewRow();
				for (int m = 0; m < count; m++)
				{
					try
					{
						navDataRow.SetDeserializedValue(m, NavSerializer.ReadItem(binaryReader, table.Columns[m].NavTypeCode));
					}
					catch (IOException ex)
					{
						EventLogWriter.Write(ConstructReadItemErrorMessage(ex, binaryReader, table.TableName, m, l), EventLogEntryType.Error, null, 0, null, 0);
						throw new NavClientIOException(ex.Message, ex).MarkFatalityScope(NavExceptionFatalityScope.Call);
					}
				}
				table.Rows.Add(navDataRow);
			}
		}
	}

	private void ClearAll()
	{
		Clear();
		dataSetName = "NewDataSet";
		culture = CultureInfo.CurrentCulture;
		tables = new NavDataTableCollection(this);
	}

	private static string ConstructReadItemErrorMessage(Exception ex, BinaryReader binReader, string tableName, int columnNumber, int rowNumber)
	{
		StringBuilder stringBuilder = new StringBuilder(ex.ToString());
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Failed to read {0} column {1} in row {2} from the stream.", tableName, columnNumber.ToString(CultureInfo.InvariantCulture), rowNumber.ToString(CultureInfo.InvariantCulture));
		if (binReader.BaseStream != null)
		{
			stringBuilder.AppendLine("Stream length: " + binReader.BaseStream.Length.ToString(CultureInfo.InvariantCulture));
			stringBuilder.AppendLine("Stream position: " + binReader.BaseStream.Position.ToString(CultureInfo.InvariantCulture));
		}
		return stringBuilder.ToString();
	}

	public NavDataSet Clone()
	{
		return new NavDataSet(Serialize());
	}

	public NavDataSet CloneStructure()
	{
		NavDataSet navDataSet = new NavDataSet();
		foreach (NavDataTable table in Tables)
		{
			navDataSet.Tables.Add(table.Clone());
		}
		return navDataSet;
	}
}
