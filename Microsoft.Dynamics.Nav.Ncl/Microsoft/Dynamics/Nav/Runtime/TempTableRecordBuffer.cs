using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TempTableRecordBuffer : IRecordBuffer
{
	private readonly NavValue[] items;

	private readonly NCLMetaTable metaTable;

	public int FieldCount => items.Length;

	public NavValue this[int index]
	{
		get
		{
			return items[index];
		}
		set
		{
			items[index] = value;
		}
	}

	public IEnumerable<NavValue> Items => items;

	public NCLMetaApplicationObject MetaApplicationObject => metaTable;

	public NCLMetaTable MetaTable => metaTable;

	public TempTableRecordBuffer(NCLMetaTable metaTable, params NavValue[] items)
	{
		if (metaTable == null)
		{
			throw new ArgumentNullException("metaTable");
		}
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		this.items = items;
		this.metaTable = metaTable;
		for (int i = 0; i < this.items.Length; i++)
		{
			NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
			if (fieldByIndex.FieldClass == FieldClass.FlowField)
			{
				this.items[i] = fieldByIndex.InitValue;
			}
		}
	}

	public NavValue GetChangedFieldValue(int index)
	{
		throw new NotSupportedException();
	}

	public NavValue GetOriginalValue(int index)
	{
		return this[index];
	}

	public NavRecordId GetRecordId()
	{
		return NavRecordId.CreateFromValues(metaTable.TableId, metaTable.PrimaryKey.KeyFieldsList.Select((NCLMetaField kf) => items[kf.FieldIndex]).ToArray());
	}

	public NavValue[] ToArray()
	{
		NavValue[] array = new NavValue[items.Length];
		Array.Copy(items, array, items.Length);
		return array;
	}

	internal void CloneBlobs(MutableRecordBuffer recordBuffer)
	{
		if (recordBuffer.MetaTable.BlobFields == null)
		{
			return;
		}
		NCLMetaField[] blobFields = recordBuffer.MetaTable.BlobFields;
		foreach (NCLMetaField nCLMetaField in blobFields)
		{
			NavBLOB navBLOB = (NavBLOB)recordBuffer.GetChangedFieldValue(nCLMetaField.FieldIndex);
			if (navBLOB != null && navBLOB.IsDirty)
			{
				items[nCLMetaField.FieldIndex] = (navBLOB.IsZeroOrEmpty ? NavBLOB.Default() : new NavBLOB(navBLOB.GetBytes()));
			}
		}
	}

	public ReadOnlyRecordBuffer ToReadOnlyBuffer()
	{
		NavValue[] array = ToArray();
		if (metaTable.BlobFields != null)
		{
			NCLMetaField[] blobFields = metaTable.BlobFields;
			foreach (NCLMetaField nCLMetaField in blobFields)
			{
				NavBLOB navBLOB = (NavBLOB)items[nCLMetaField.FieldIndex];
				if (navBLOB != null && !navBLOB.IsZeroOrEmpty)
				{
					array[nCLMetaField.FieldIndex] = new NavBLOB(navBLOB.GetLength());
				}
			}
		}
		return new ReadOnlyRecordBuffer(metaTable, array);
	}
}
