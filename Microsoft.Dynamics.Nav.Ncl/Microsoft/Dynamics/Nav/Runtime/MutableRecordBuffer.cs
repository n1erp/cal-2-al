using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class MutableRecordBuffer : IRecordBuffer
{
	internal const int NoTransactionToken = 0;

	private readonly ReadOnlyRecordBuffer readOnlyBuffer;

	private readonly int transactionToken;

	private NavValue[] changedValues;

	private NavRecordId recordId;

	private readonly bool isTable;

	private bool isCopiedFromTemporary;

	public ReadOnlyRecordBuffer ReadOnlyBuffer
	{
		[DebuggerStepThrough]
		get
		{
			return readOnlyBuffer;
		}
	}

	public NCLMetaApplicationObject MetaApplicationObject => readOnlyBuffer.MetaApplicationObject;

	public NCLMetaTable MetaTable => (NCLMetaTable)readOnlyBuffer.MetaApplicationObject;

	public int TransactionToken
	{
		[DebuggerStepThrough]
		get
		{
			return transactionToken;
		}
	}

	public int FieldCount
	{
		[DebuggerStepThrough]
		get
		{
			return readOnlyBuffer.FieldCount;
		}
	}

	public NavValue this[int index]
	{
		get
		{
			if (changedValues != null)
			{
				NavValue navValue = changedValues[index];
				if (navValue != null)
				{
					return navValue;
				}
			}
			return GetOriginalValue(index);
		}
		set
		{
			if (value != null)
			{
				if (changedValues == null)
				{
					changedValues = new NavValue[FieldCount];
				}
				changedValues[index] = value;
			}
			else if (changedValues != null)
			{
				changedValues[index] = null;
			}
			if (!isTable)
			{
				return;
			}
			if (index == 0)
			{
				NCLMetaField userDefinedTimestampField = MetaTable.UserDefinedTimestampField;
				if (userDefinedTimestampField != null && MetaTable.SqlHasTimestamp)
				{
					changedValues[userDefinedTimestampField.FieldIndex] = changedValues[index];
				}
			}
			if (recordId != null && MetaTable.GetFieldByIndex(index).FieldIsPartOfPrimaryKey)
			{
				recordId = null;
			}
		}
	}

	public bool IsDirty => changedValues != null;

	public bool IsPrimaryKeyDirty
	{
		get
		{
			if (changedValues != null)
			{
				NCLMetaTable metaTable = MetaTable;
				NCLMetaKey primaryKey = metaTable.PrimaryKey;
				for (int i = 0; i < primaryKey.KeyFieldCount; i++)
				{
					NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
					if (changedValues[keyFieldByIndex.FieldIndex] != null)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public NavBigInteger Timestamp
	{
		get
		{
			if (!isTable)
			{
				throw new InvalidOperationException("Only tables have a timestamp field.");
			}
			NavValue changedFieldValue = GetChangedFieldValue(0);
			if (changedFieldValue != null)
			{
				return (NavBigInteger)changedFieldValue;
			}
			return OriginalTimestamp;
		}
	}

	internal bool IsCopiedFromTemporary
	{
		get
		{
			return isCopiedFromTemporary;
		}
		set
		{
			isCopiedFromTemporary = value;
		}
	}

	private NavBigInteger OriginalTimestamp
	{
		get
		{
			if (!isTable)
			{
				throw new InvalidOperationException("Only tables have a timestamp field.");
			}
			return (NavBigInteger)readOnlyBuffer[0];
		}
	}

	public MutableRecordBuffer(ReadOnlyRecordBuffer readOnlyBuffer)
		: this(readOnlyBuffer, 0)
	{
	}

	public MutableRecordBuffer(ReadOnlyRecordBuffer readOnlyBuffer, MutableRecordBuffer changelistBuffer, int transactionToken)
		: this(readOnlyBuffer, transactionToken)
	{
		changedValues = changelistBuffer.changedValues;
	}

	public MutableRecordBuffer(ReadOnlyRecordBuffer readOnlyBuffer, NavValue[] changeList, int transactionToken)
		: this(readOnlyBuffer, transactionToken)
	{
		changedValues = changeList;
	}

	public MutableRecordBuffer(ReadOnlyRecordBuffer readOnlyBuffer, int transactionToken)
	{
		if (readOnlyBuffer == null)
		{
			throw new ArgumentNullException("readOnlyBuffer");
		}
		this.readOnlyBuffer = readOnlyBuffer;
		this.transactionToken = transactionToken;
		isTable = MetaApplicationObject != null && MetaApplicationObject.ApplicationObjectType == ObjectType.Table;
	}

	public NavValue GetOriginalValue(int index)
	{
		if (index != 0 || !isTable)
		{
			return readOnlyBuffer[index];
		}
		return OriginalTimestamp;
	}

	public NavValue GetChangedFieldValue(int index)
	{
		if (changedValues != null)
		{
			return changedValues[index];
		}
		return null;
	}

	public bool IsChangedValueSameAsOriginalValue(int index)
	{
		return GetChangedFieldValue(index)?.Equals(readOnlyBuffer[index]) ?? true;
	}

	public NavRecordId GetRecordId()
	{
		if (recordId == null)
		{
			if (!IsPrimaryKeyDirty)
			{
				recordId = readOnlyBuffer.GetRecordId();
			}
			else
			{
				NCLMetaTable metaTable = MetaTable;
				NCLMetaKey primaryKey = metaTable.PrimaryKey;
				NavValue[] array = new NavValue[primaryKey.KeyFieldCount];
				for (int i = 0; i < array.Length; i++)
				{
					NCLMetaField keyFieldByIndex = primaryKey.GetKeyFieldByIndex(i);
					array[i] = this[keyFieldByIndex.FieldIndex] ?? keyFieldByIndex.InitValue;
				}
				recordId = new NavRecordId(metaTable.TableId, array);
			}
		}
		return recordId;
	}

	public NavValue[] ToArray()
	{
		NavValue[] array = readOnlyBuffer.ToArray();
		if (changedValues != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				NavValue navValue = changedValues[i];
				if (navValue != null)
				{
					array[i] = navValue;
				}
			}
		}
		return array;
	}

	public void MarkAllFieldsAsChanged(bool includePrimaryKeyFields, bool checkMetadataForDefaults)
	{
		NCLMetaTable metaTable = MetaTable;
		if (changedValues == null)
		{
			changedValues = readOnlyBuffer.ToArray();
			if (checkMetadataForDefaults)
			{
				for (int i = 0; i < changedValues.Length; i++)
				{
					if (changedValues[i] == null)
					{
						NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
						if (fieldByIndex.FieldClass == FieldClass.Normal && (!fieldByIndex.FieldIsPartOfPrimaryKey | includePrimaryKeyFields))
						{
							changedValues[i] = fieldByIndex.InitValue;
						}
					}
				}
			}
			else if (!includePrimaryKeyFields)
			{
				NCLMetaKey primaryKey = metaTable.PrimaryKey;
				for (int j = 0; j < primaryKey.KeyFieldCount; j++)
				{
					changedValues[primaryKey.GetKeyFieldByIndex(j).FieldIndex] = null;
				}
			}
		}
		else
		{
			for (int k = 0; k < changedValues.Length; k++)
			{
				if (changedValues[k] != null)
				{
					continue;
				}
				NCLMetaField nCLMetaField = null;
				if (!includePrimaryKeyFields)
				{
					nCLMetaField = metaTable.GetFieldByIndex(k);
					if (nCLMetaField.FieldIsPartOfPrimaryKey)
					{
						continue;
					}
				}
				NavValue navValue = readOnlyBuffer[k];
				if (checkMetadataForDefaults && navValue == null)
				{
					if (nCLMetaField == null)
					{
						nCLMetaField = metaTable.GetFieldByIndex(k);
					}
					if (nCLMetaField.FieldClass == FieldClass.Normal)
					{
						navValue = nCLMetaField.InitValue;
					}
				}
				changedValues[k] = navValue;
			}
		}
		if (metaTable.BlobFieldCount <= 0)
		{
			return;
		}
		NCLMetaField[] blobFields = metaTable.BlobFields;
		foreach (NCLMetaField nCLMetaField2 in blobFields)
		{
			NavBLOB navBLOB = (NavBLOB)changedValues[nCLMetaField2.FieldIndex];
			if (navBLOB != null && navBLOB == GetOriginalValue(nCLMetaField2.FieldIndex))
			{
				changedValues[nCLMetaField2.FieldIndex] = new NavBLOB(navBLOB.GetLength());
			}
		}
	}

	public void OptimizeChanges()
	{
		if (changedValues == null)
		{
			return;
		}
		NCLMetaTable metaTable = MetaTable;
		for (int i = 1; i < metaTable.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
			if (fieldByIndex.FieldNclType == NavNclType.NavBlob)
			{
				continue;
			}
			NavValue navValue = changedValues[i];
			if (navValue != null)
			{
				NavValue other = readOnlyBuffer[i];
				if (navValue.Equals(other))
				{
					changedValues[i] = null;
				}
			}
		}
	}

	public MutableRecordBuffer Clone()
	{
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(readOnlyBuffer, transactionToken);
		if (changedValues != null)
		{
			mutableRecordBuffer.changedValues = (NavValue[])changedValues.Clone();
			NCLMetaField[] blobFields = MetaTable.BlobFields;
			if (blobFields != null)
			{
				for (int i = 0; i < blobFields.Length; i++)
				{
					int columnIndex = blobFields[i].ColumnIndex;
					NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer.changedValues[columnIndex];
					if (navBLOB != null)
					{
						mutableRecordBuffer.changedValues[columnIndex] = navBLOB.DeepCopy();
					}
				}
			}
		}
		return mutableRecordBuffer;
	}

	public override string ToString()
	{
		if (!(MetaApplicationObject is NCLMetaTable nCLMetaTable))
		{
			return base.ToString();
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		NCLMetaField[] allFields = nCLMetaTable.AllFields;
		foreach (NCLMetaField nCLMetaField in allFields)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat(CultureInfo.CurrentCulture, "[{0}] original: {{{1}}}", nCLMetaField.FieldIndex, GetOriginalValue(nCLMetaField.FieldIndex));
			NavValue changedFieldValue = GetChangedFieldValue(nCLMetaField.FieldIndex);
			if (changedFieldValue != null)
			{
				stringBuilder.AppendFormat(CultureInfo.CurrentCulture, " changed: {{{0}}}", changedFieldValue);
			}
		}
		return stringBuilder.ToString();
	}

	public static MutableRecordBuffer ComputeChangeset(MutableRecordBuffer inputRecordBuffer, ReadOnlyRecordBuffer bufferInDatabase)
	{
		MutableRecordBuffer mutableRecordBuffer = inputRecordBuffer.Clone();
		mutableRecordBuffer.MarkAllFieldsAsChanged(includePrimaryKeyFields: false, checkMetadataForDefaults: true);
		MutableRecordBuffer mutableRecordBuffer2 = new MutableRecordBuffer(bufferInDatabase);
		NCLMetaTable metaTable = inputRecordBuffer.MetaTable;
		mutableRecordBuffer2[0] = bufferInDatabase[0];
		for (int i = 1; i < metaTable.FieldCount; i++)
		{
			NCLMetaField fieldByIndex = metaTable.GetFieldByIndex(i);
			if (fieldByIndex.FieldNclType == NavNclType.NavBlob || fieldByIndex.FieldClass == FieldClass.FlowField)
			{
				mutableRecordBuffer2[i] = inputRecordBuffer[i];
			}
			else if (fieldByIndex.FieldClass == FieldClass.Normal)
			{
				NavValue navValue = inputRecordBuffer[i];
				NavValue objB = bufferInDatabase[i];
				if (!object.Equals(navValue, objB))
				{
					mutableRecordBuffer2[i] = navValue ?? fieldByIndex.InitValue;
				}
			}
		}
		return mutableRecordBuffer2;
	}
}
