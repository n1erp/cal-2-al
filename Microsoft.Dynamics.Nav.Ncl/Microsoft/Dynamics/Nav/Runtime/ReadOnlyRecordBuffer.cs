using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReadOnlyRecordBuffer : IRecordBuffer
{
	internal static readonly ReadOnlyRecordBuffer[] EmptyArray = new ReadOnlyRecordBuffer[0];

	private readonly NCLMetaApplicationObject metaApplicationObject;

	private readonly bool onlyCurrentKey;

	private readonly NavValue[] fields;

	private NavRecordId recordId;

	public bool OnlyCurrentKey => onlyCurrentKey;

	public NCLMetaApplicationObject MetaApplicationObject => metaApplicationObject;

	public NavValue this[int index] => fields[index];

	public int FieldCount => fields.Length;

	public int CacheSize
	{
		get
		{
			if (metaApplicationObject is NCLMetaTable nCLMetaTable)
			{
				return nCLMetaTable.ApproximateRowByteSize;
			}
			return 2 + FieldCount;
		}
	}

	public NavBigInteger Timestamp
	{
		get
		{
			if (metaApplicationObject.ApplicationObjectType != ObjectType.Table)
			{
				throw new InvalidOperationException("Only tables have a timestamp field.");
			}
			return (NavBigInteger)fields[0];
		}
	}

	public ReadOnlyRecordBuffer(NCLMetaApplicationObject metaApplicationObject, params NavValue[] immutableFields)
		: this(metaApplicationObject, onlyCurrentKey: false, immutableFields)
	{
	}

	public ReadOnlyRecordBuffer(NCLMetaApplicationObject metaApplicationObject, bool onlyCurrentKey, params NavValue[] immutableFields)
		: this(immutableFields)
	{
		if (metaApplicationObject == null)
		{
			throw new ArgumentNullException("metaApplicationObject");
		}
		this.metaApplicationObject = metaApplicationObject;
		this.onlyCurrentKey = onlyCurrentKey;
	}

	public ReadOnlyRecordBuffer(params NavValue[] immutableFields)
	{
		if (immutableFields == null)
		{
			throw new ArgumentNullException("immutableFields");
		}
		fields = immutableFields;
	}

	public NavRecordId GetRecordId()
	{
		if (recordId == null)
		{
			if (!(metaApplicationObject is NCLMetaTable { PrimaryKey: var primaryKey } nCLMetaTable))
			{
				throw new InvalidOperationException();
			}
			NavValue[] array = new NavValue[primaryKey.KeyFieldCount];
			for (int i = 0; i < array.Length; i++)
			{
				NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
				array[i] = fields[keyFieldByIndex.FieldIndex] ?? keyFieldByIndex.InitValue;
			}
			recordId = NavRecordId.CreateFromValues(nCLMetaTable.TableId, array);
		}
		return recordId;
	}

	public void SetTimestamp(NavBigInteger newTimestamp)
	{
		fields[0] = newTimestamp;
	}

	public NavValue[] ToArray()
	{
		NavValue[] array = new NavValue[fields.Length];
		Array.Copy(fields, array, fields.Length);
		return array;
	}

	[Conditional("DEBUG")]
	private void ValidateBlobs()
	{
		for (int i = 0; i < fields.Length; i++)
		{
		}
	}

	[Conditional("DEBUG")]
	private void ValidateBlob(int index)
	{
		NavValue navValue = fields[index];
		if (navValue != null && navValue.NclType == NavNclType.NavBlob)
		{
			_ = (NavBLOB)navValue;
		}
	}

	public override string ToString()
	{
		if (!(MetaApplicationObject is NCLMetaTable nCLMetaTable))
		{
			return base.ToString();
		}
		return string.Join(", ", nCLMetaTable.PrimaryKey.KeyFieldsList.Select((NCLMetaField f) => this[f.ColumnIndex]));
	}

	NavValue IRecordBuffer.GetChangedFieldValue(int index)
	{
		return null;
	}

	NavValue IRecordBuffer.GetOriginalValue(int index)
	{
		return this[index];
	}
}
