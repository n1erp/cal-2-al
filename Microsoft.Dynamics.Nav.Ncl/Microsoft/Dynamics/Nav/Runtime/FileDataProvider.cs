using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FileDataProvider : ComputedVirtualDataProvider
{
	public static readonly FileDataProvider Instance = new FileDataProvider();

	public override int TableId => 2000000022;

	private FileDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinFilterForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filter, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
		{
			if (IsEqualFilter(filter, out var value))
			{
				NavValue[] immutableFields = new NavValue[2]
				{
					VirtualDataProvider.VirtualTimeStamp,
					GetTruncatedCodeValue(1, ((NavCode)value).Value)
				};
				return new ReadOnlyRecordBuffer[1]
				{
					new ReadOnlyRecordBuffer(MetaTable, immutableFields)
				};
			}
			break;
		}
		case 2:
			return EnumerateIsFile(precedingKeyFieldValues, filter, sortOrder);
		case 3:
			return (from record in GetItemsInTheDirectory(precedingKeyFieldValues, filter)
				where record.PassesFieldFilters(nonPrimaryKeyFilters)
				select record).OrderBySortingFields(SortingFieldList.Create(new SortingField[1]
			{
				new SortingField(MetaTable.GetFieldByNo(3), sortOrder)
			}), FindType.Normal);
		}
		return Enumerable.Empty<ReadOnlyRecordBuffer>();
	}

	public override FieldDictionary<NavValue> GetBlobContent(GetBlobContentProviderRequest request)
	{
		try
		{
			if (request.BlobsToGet.Count != 1)
			{
				throw new InvalidOperationException("File table should have only one BLOB field");
			}
			Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[request.BlobsToGet.Count];
			NCLMetaField nCLMetaField = (NCLMetaField)request.BlobsToGet[0];
			if (nCLMetaField.FieldNclType != NavNclType.NavBlob)
			{
				throw new InvalidOperationException("This method can only get the content of blobs");
			}
			NavBLOB navBLOB = (NavBLOB)request.RecordBuffer[nCLMetaField.FieldIndex];
			if (request.RecordBuffer[2].ToBoolean())
			{
				string path = Path.Combine(request.RecordBuffer[1].ToString(), request.RecordBuffer[3].ToString());
				if (File.Exists(path))
				{
					using FileStream stream = File.OpenRead(path);
					navBLOB.AssignFromStream(stream);
				}
			}
			else
			{
				navBLOB.Clear();
			}
			array[0] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, navBLOB);
			return new FieldDictionary<NavValue>(array);
		}
		catch (SystemException ex)
		{
			throw new NavALException(ex.Message, ex);
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateIsFile(ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filter, SortOrder sortOrder)
	{
		NavValue[] array = ((sortOrder != SortOrder.Ascending) ? new NavBoolean[2]
		{
			NavBoolean.True,
			NavBoolean.False
		} : new NavBoolean[2]
		{
			NavBoolean.False,
			NavBoolean.True
		});
		NavValue[] array2 = array;
		NavValue[] array3 = array2;
		foreach (NavValue navValue in array3)
		{
			if (filter.Evaluate(navValue))
			{
				yield return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, precedingKeyFieldValues[1], navValue);
			}
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetItemsInTheDirectory(ReadOnlyRecordBuffer precedingKeyFieldValues, FilterExpression filterForTheNameField)
	{
		NavFile.VerifyALFileAccessEnabled();
		NavCode currentPath = new NavCode(2048, NavFile.ExpandFileName(DataError.ThrowError, precedingKeyFieldValues[1].ToString(invariant: true)));
		string text = currentPath.ToString();
		if (string.IsNullOrEmpty(text))
		{
			yield break;
		}
		DirectoryInfo currentDirectory = new DirectoryInfo(text);
		if (!currentDirectory.Exists)
		{
			yield break;
		}
		bool isFile = precedingKeyFieldValues[2].ToBoolean();
		IEnumerable<FileSystemInfo> fileSystemInfoList;
		try
		{
			fileSystemInfoList = ((!isFile) ? ((IEnumerable<FileSystemInfo>)currentDirectory.EnumerateDirectories()) : ((IEnumerable<FileSystemInfo>)currentDirectory.EnumerateFiles()));
		}
		catch (SystemException ex)
		{
			throw new NavALException(ex.Message, ex);
		}
		foreach (ReadOnlyRecordBuffer readOnlyRecordBuffersFromFileSystemInfo in GetReadOnlyRecordBuffersFromFileSystemInfoList(currentPath, filterForTheNameField, fileSystemInfoList, isFile))
		{
			yield return readOnlyRecordBuffersFromFileSystemInfo;
		}
		if (!isFile && currentDirectory.Parent != null)
		{
			NavValue name = GetName(".");
			if (filterForTheNameField.Evaluate(name))
			{
				yield return CreateReadOnlyRecordBuffer(currentPath, isFile: false, currentDirectory, name);
			}
			name = GetName("..");
			if (filterForTheNameField.Evaluate(name))
			{
				yield return CreateReadOnlyRecordBuffer(currentPath, isFile: false, currentDirectory.Parent, name);
			}
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetReadOnlyRecordBuffersFromFileSystemInfoList(NavValue currentPath, FilterExpression filterForTheNameField, IEnumerable<FileSystemInfo> fileSystemInfoList, bool isFile)
	{
		foreach (FileSystemInfo fileSystemInfo in fileSystemInfoList)
		{
			NavValue name = GetName(fileSystemInfo.Name);
			if (filterForTheNameField.Evaluate(name))
			{
				yield return CreateReadOnlyRecordBuffer(currentPath, isFile, fileSystemInfo, name);
			}
		}
	}

	private ReadOnlyRecordBuffer CreateReadOnlyRecordBuffer(NavValue currentPath, bool isFile, FileSystemInfo fileSystemInfo, NavValue name)
	{
		try
		{
			int num = (isFile ? Int64FileSizeToInt32(((FileInfo)fileSystemInfo).Length) : 0);
			return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, currentPath, NavBoolean.Create(isFile), name, NavInteger.Create(num), NavDateTime.Create(fileSystemInfo.LastWriteTimeUtc).DatePart, NavDateTime.Create(fileSystemInfo.LastWriteTimeUtc).TimePart, new NavBLOB(num));
		}
		catch (SystemException ex)
		{
			throw new NavALException(ex.Message, ex);
		}
	}

	private static bool IsEqualFilter(FilterExpression filter, out NavValue value)
	{
		value = null;
		RangeList rangeList;
		try
		{
			rangeList = filter.ToRangeList();
		}
		catch (CannotConvertWildcardIntoRangeException)
		{
			return false;
		}
		if (rangeList.Ranges.Count != 1)
		{
			return false;
		}
		Range range = rangeList.Ranges[0];
		bool flag = range != null && range.IsHighDefined && range.IsLowDefined && range.IsLowIncluded && range.IsHighIncluded && range.HighValue == range.LowValue;
		if (flag)
		{
			value = range.HighValue;
		}
		return flag;
	}

	private static int Int64FileSizeToInt32(long fileSize)
	{
		if (fileSize <= (long)NavInteger.MaxValue)
		{
			return (int)fileSize;
		}
		return -1;
	}

	private NavText GetName(string name)
	{
		return GetTruncatedTextValue(3, name);
	}
}
