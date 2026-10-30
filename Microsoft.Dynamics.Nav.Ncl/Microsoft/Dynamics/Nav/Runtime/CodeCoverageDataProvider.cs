using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Debugger;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CodeCoverageDataProvider : RangeBasedComputedDataProvider, ITreeObject, IDisposable
{
	private struct CoveredObject : IComparable<CoveredObject>
	{
		private readonly ApplicationObjectId objectId;

		private readonly List<CodeCoverageScopeCounter> scopeCounters;

		public List<CodeCoverageScopeCounter> ScopeCounters
		{
			[DebuggerStepThrough]
			get
			{
				return scopeCounters;
			}
		}

		public ApplicationObjectId ObjectId
		{
			[DebuggerStepThrough]
			get
			{
				return objectId;
			}
		}

		public CoveredObject(ApplicationObjectId objectId, List<CodeCoverageScopeCounter> scopeCounters)
		{
			this.objectId = objectId;
			this.scopeCounters = scopeCounters;
		}

		public CoveredObject(ApplicationObjectId objectId)
		{
			this.objectId = objectId;
			scopeCounters = null;
		}

		public int CompareTo(CoveredObject other)
		{
			return objectId.CompareTo(other.objectId);
		}
	}

	private static NavOption[] lineTypeOptions;

	private readonly TreeHandler tree;

	private readonly List<CoveredObject> sortedObjects;

	private readonly ALCodeEnvironment codeEnvironment;

	private readonly Dictionary<ApplicationObjectId, WeakReference> cachedRecordsByType;

	public override int TableId => 2000000049;

	public TreeHandler Tree => tree;

	public static CodeCoverageDataProvider GetInstanceForSession(NavSession session)
	{
		return session.Tree.Children.OfType<CodeCoverageDataProvider>().FirstOrDefault() ?? new CodeCoverageDataProvider(session);
	}

	private CodeCoverageDataProvider(NavSession session)
	{
		tree = TreeHandler.CreateTreeHandler(session, this);
		codeEnvironment = session.NCLMetadata.CodeEnvironment;
		sortedObjects = new List<CoveredObject>();
		cachedRecordsByType = new Dictionary<ApplicationObjectId, WeakReference>();
	}

	public void SetScopeCounters(IEnumerable<CodeCoverageScopeCounter> scopeCounters, IEnumerable<ApplicationObjectId> objectsToEnsureAreIncluded)
	{
		Dictionary<ApplicationObjectId, CoveredObject> dictionary = (from scopeCounter in scopeCounters
			group scopeCounter by ALCodeEnvironment.EncodedTypeAndObjectTokenFromScopeId(scopeCounter.ScopeId) into typeGroup
			let objectId = codeEnvironment.ObjectIdFromCompressedObjectIdToken(typeGroup.Key)
			select new CoveredObject(objectId, typeGroup.ToList())).ToDictionary((CoveredObject o) => o.ObjectId, (CoveredObject o) => o);
		foreach (ApplicationObjectId item in objectsToEnsureAreIncluded)
		{
			if (!dictionary.ContainsKey(item))
			{
				dictionary.Add(item, new CoveredObject(item, new List<CodeCoverageScopeCounter>(0)));
			}
		}
		sortedObjects.Clear();
		sortedObjects.AddRange(dictionary.Values);
		sortedObjects.Sort();
		cachedRecordsByType.Clear();
	}

	public IEnumerable<CodeCoverageScopeCounter> GetScopeCounters()
	{
		return sortedObjects.Where((CoveredObject co) =>
		{
			CoveredObject coveredObject = co;
			return coveredObject.ScopeCounters != null;
		}).SelectMany((CoveredObject co) =>
		{
			CoveredObject coveredObject = co;
			return coveredObject.ScopeCounters;
		}, (CoveredObject co, CodeCoverageScopeCounter scopeCounter) => scopeCounter.Clone());
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
		{
			NCLOptionMetadata fieldOptionMetadata = MetaTable.GetFieldByNo(1).FieldOptionMetadata;
			return ComputedVirtualDataProvider.EnumerateOptionsInRange(fieldOptionMetadata, range, sortOrder, (NavOption option) => new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, option));
		}
		case 2:
			return EnumerateObjectNumbers((NavOption)precedingKeyFieldValues[1], range, sortOrder);
		case 3:
			return EnumerateLines((NavOption)precedingKeyFieldValues[1], (NavInteger)precedingKeyFieldValues[2], precedingKeyFieldValues[3].ToInt32(), range, sortOrder, nonPrimaryKeyFilters);
		default:
			throw new NotSupportedException();
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateObjectNumbers(NavOption objectTypeOption, Range range, SortOrder sortOrder)
	{
		ObjectType objectType = (ObjectType)objectTypeOption.ToInt32();
		if (!range.GetInclusiveIntegerBounds(0, int.MaxValue, out var low, out var high))
		{
			yield break;
		}
		CoveredObject item = new CoveredObject(new ApplicationObjectId(objectType, (sortOrder == SortOrder.Ascending) ? low : high));
		int index = sortedObjects.BinarySearch(item);
		if (index < 0)
		{
			index = ~index;
			if (sortOrder == SortOrder.Descending)
			{
				index--;
			}
		}
		for (; index >= 0 && index < sortedObjects.Count; index += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			ApplicationObjectId objectId = sortedObjects[index].ObjectId;
			if (objectId.ObjectType == objectType && objectId.ObjectNumber <= high && objectId.ObjectNumber >= low)
			{
				yield return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, objectTypeOption, NavInteger.Create(objectId.ObjectNumber), NavInteger.Create(index));
				continue;
			}
			break;
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> EnumerateLines(NavOption objectTypeOption, NavInteger objectNumber, int indexInSortedList, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		CoveredObject coveredObject = sortedObjects[indexInSortedList];
		ReadOnlyRecordBuffer[] records;
		if (cachedRecordsByType.TryGetValue(coveredObject.ObjectId, out var value))
		{
			ReadOnlyRecordBuffer[] array;
			records = (array = (ReadOnlyRecordBuffer[])value.Target);
			if (array != null)
			{
				goto IL_0097;
			}
		}
		records = ComputeRecordsForObject(objectTypeOption, objectNumber, coveredObject);
		cachedRecordsByType[coveredObject.ObjectId] = new WeakReference(records);
		goto IL_0097;
		IL_0097:
		if (!range.GetInclusiveIntegerBounds(0, records.Length - 1, out var low, out var high))
		{
			yield break;
		}
		for (int i = ((sortOrder == SortOrder.Ascending) ? low : high); i >= low && i <= high; i += ((sortOrder == SortOrder.Ascending) ? 1 : (-1)))
		{
			if (records[i].PassesFieldFilters(nonPrimaryKeyFilters))
			{
				yield return records[i];
			}
		}
	}

	private ReadOnlyRecordBuffer[] ComputeRecordsForObject(NavOption objectTypeOption, NavInteger objectNumber, CoveredObject coveredObject)
	{
		ApplicationObjectId objectId = coveredObject.ObjectId;
		NavAppGroup navAppGroup = NavCurrentThread.Session.NavAppGroup;
		SpecificNavAppObjectMetadataSummaryPair objectSummary = navAppGroup.GetObjectSummary(objectId);
		if (objectSummary != null && !objectSummary.RuntimeMetadata.ShowMyCode)
		{
			return new ReadOnlyRecordBuffer[0];
		}
		ObjectSourceInfo sourceInfo = codeEnvironment.GetSourceInfo(objectId);
		string sourceCode = codeEnvironment.GetSourceCodeInfo(objectId).SourceCode;
		string[] array = sourceCode.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
		NavValue[][] array2 = new NavValue[array.Length + 1][];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = new NavValue[7]
			{
				VirtualDataProvider.VirtualTimeStamp,
				objectTypeOption,
				objectNumber,
				NavInteger.Create(i),
				(i == 0) ? GetLineTypeOption(CodeCoderageSourceLineType.Object) : GetLineTypeOption(CodeCoderageSourceLineType.Empty),
				GetTruncatedTextValue(5, (i == 0) ? ScaffoldingHelper.GetTypeString(objectId) : array[i - 1]),
				NavInteger.Create(0)
			};
		}
		var enumerable = from methodSourceInfo in sourceInfo.Methods
			let scopeId = codeEnvironment.GetOrAssignScopeId(objectId, methodSourceInfo.LocalsType, setBreakpointFlag: false)
			join codeCoverageScopeCounter in coveredObject.ScopeCounters on scopeId equals codeCoverageScopeCounter.ScopeId into scopeCounterGroup
			from scopeCounter2 in scopeCounterGroup.DefaultIfEmpty()
			select new
			{
				methodSourceInfo = methodSourceInfo,
				scopeCounter = scopeCounter2
			};
		foreach (var item in enumerable)
		{
			array2[item.methodSourceInfo.SignatureLineNumber + 1][4] = GetLineTypeOption(CodeCoderageSourceLineType.Function);
			List<StatementSourceInfo> list = item.methodSourceInfo.Statements.ToList();
			for (int num = 0; num < list.Count - 1; num++)
			{
				StatementSourceInfo statementSourceInfo = list[num];
				CodeCoverageScopeCounter scopeCounter = item.scopeCounter;
				int num2 = (int)((scopeCounter != null && num < scopeCounter.StatementHitCounts.Length) ? scopeCounter.StatementHitCounts[num] : 0);
				if (statementSourceInfo.SourceSpan.From.Line + 1 >= 0 && statementSourceInfo.SourceSpan.From.Line + 1 < array2.Length)
				{
					NavValue[] array3 = array2[statementSourceInfo.SourceSpan.From.Line + 1];
					array3[4] = GetLineTypeOption(CodeCoderageSourceLineType.Code);
					int num3 = array3[6].ToInt32();
					if (num2 > num3)
					{
						array3[6] = NavInteger.Create(num2);
					}
				}
			}
		}
		ReadOnlyRecordBuffer[] array4 = new ReadOnlyRecordBuffer[array2.Length];
		for (int num4 = 0; num4 < array2.Length; num4++)
		{
			array4[num4] = new ReadOnlyRecordBuffer(MetaTable, array2[num4]);
		}
		return array4;
	}

	public override void DeleteAll(int companyToken, NCLMetaTable metaTable, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		if (filtersAndMarks.IsEmpty)
		{
			cachedRecordsByType.Clear();
			sortedObjects.Clear();
			return;
		}
		FilterExpression filterExpression = null;
		FilterExpression filterExpression2 = null;
		bool flag = filtersAndMarks.MarkedRecords == null;
		int num = 0;
		while (flag && num < filtersAndMarks.Filters.Items.Length)
		{
			Tuple<INavFieldMetadata, FilterExpression> tuple = filtersAndMarks.Filters.Items[num];
			switch (((NCLMetaField)tuple.Item1).FieldNo)
			{
			case 1:
				filterExpression = tuple.Item2;
				break;
			case 2:
				filterExpression2 = tuple.Item2;
				break;
			default:
				flag = false;
				break;
			}
			num++;
		}
		if (flag)
		{
			NCLMetaField fieldByNo = MetaTable.GetFieldByNo(1);
			bool[] array = new bool[fieldByNo.FieldOptionMetadata.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = filterExpression?.Evaluate(NavOption.Create(fieldByNo.FieldOptionMetadata, i)) ?? true;
			}
			int num2 = 0;
			for (int j = 0; j < sortedObjects.Count; j++)
			{
				CoveredObject coveredObject = sortedObjects[j];
				if (array[(int)coveredObject.ObjectId.ObjectType] && (filterExpression2 == null || filterExpression2.Evaluate(NavInteger.Create(coveredObject.ObjectId.ObjectNumber))))
				{
					cachedRecordsByType.Remove(coveredObject.ObjectId);
					continue;
				}
				if (num2 < j)
				{
					sortedObjects[num2] = sortedObjects[j];
				}
				num2++;
			}
			if (num2 < sortedObjects.Count)
			{
				sortedObjects.RemoveRange(num2, sortedObjects.Count - num2);
			}
			return;
		}
		IEnumerable<ReadOnlyRecordBuffer> source = Find(new FindProviderRequest(0, MetaTable, DataLockState.None, filtersAndMarks, null, securityFiltering, FieldList.Empty, MetaTable.PrimaryKey.DefinedSortingFields, FindType.Normal));
		foreach (ReadOnlyRecordBuffer item in source.ToList())
		{
			Delete(0, new MutableRecordBuffer(item), recordHasBeenReadWithLockInsideThisTransaction: false);
		}
	}

	public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId((ObjectType)recordBuffer[1].ToInt32(), recordBuffer[2].ToInt32());
		int lineNumber = recordBuffer[3].ToInt32() - 1;
		CodeCoderageSourceLineType codeCoderageSourceLineType = (CodeCoderageSourceLineType)recordBuffer[4].ToInt32();
		int num = recordBuffer[6].ToInt32();
		cachedRecordsByType.Remove(applicationObjectId);
		if ((codeCoderageSourceLineType == CodeCoderageSourceLineType.Code && num >= 0) || codeCoderageSourceLineType == CodeCoderageSourceLineType.Object)
		{
			AssignHitCountOnLine(applicationObjectId, lineNumber, num, createIfNecessary: true);
		}
		outputRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, recordBuffer.ToArray());
		return InsertResult.Success;
	}

	public override DeleteResult Delete(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId((ObjectType)recordBuffer[1].ToInt32(), recordBuffer[2].ToInt32());
		cachedRecordsByType.Remove(applicationObjectId);
		int lineNumber = recordBuffer[3].ToInt32() - 1;
		switch ((CodeCoderageSourceLineType)recordBuffer[4].ToInt32())
		{
		case CodeCoderageSourceLineType.Object:
		{
			int num = sortedObjects.BinarySearch(new CoveredObject(applicationObjectId));
			if (num >= 0)
			{
				sortedObjects.RemoveAt(num);
			}
			break;
		}
		case CodeCoderageSourceLineType.Code:
			AssignHitCountOnLine(applicationObjectId, lineNumber, 0, createIfNecessary: false);
			break;
		}
		return DeleteResult.Success;
	}

	private void AssignHitCountOnLine(ApplicationObjectId objectId, int lineNumber, int hitCount, bool createIfNecessary)
	{
		CoveredObject item = new CoveredObject(objectId);
		ObjectSourceInfo sourceInfo;
		try
		{
			sourceInfo = codeEnvironment.GetSourceInfo(objectId);
		}
		catch (NavNCLMetadataObjectNotFoundException)
		{
			return;
		}
		int num = -1;
		if (sortedObjects.Count > 0)
		{
			int num2 = sortedObjects[sortedObjects.Count - 1].ObjectId.CompareTo(objectId);
			if (num2 != 0)
			{
				num = ((num2 >= 0) ? sortedObjects.BinarySearch(item) : (~sortedObjects.Count));
			}
			else
			{
				num = sortedObjects.Count - 1;
			}
		}
		if (num < 0)
		{
			if (!createIfNecessary)
			{
				return;
			}
			num = ~num;
			item = new CoveredObject(objectId, new List<CodeCoverageScopeCounter>());
			sortedObjects.Insert(num, item);
		}
		else
		{
			item = sortedObjects[num];
		}
		if (lineNumber < 0 || lineNumber > 65535)
		{
			return;
		}
		StatementSourceInfo statementSourceInfo = sourceInfo.FindStatementForPosition(new SourcePosition((ushort)lineNumber, 0));
		if (statementSourceInfo == null || statementSourceInfo.SourceSpan.From.Line != lineNumber)
		{
			return;
		}
		int orAssignScopeId = codeEnvironment.GetOrAssignScopeId(objectId, statementSourceInfo.Method.LocalsType, setBreakpointFlag: false);
		CodeCoverageScopeCounter codeCoverageScopeCounter = null;
		for (int num3 = item.ScopeCounters.Count - 1; num3 >= 0; num3--)
		{
			if (item.ScopeCounters[num3].ScopeId == orAssignScopeId)
			{
				codeCoverageScopeCounter = item.ScopeCounters[num3];
				break;
			}
		}
		if (codeCoverageScopeCounter == null)
		{
			if (!createIfNecessary)
			{
				return;
			}
			codeCoverageScopeCounter = new CodeCoverageScopeCounter(orAssignScopeId);
			item.ScopeCounters.Add(codeCoverageScopeCounter);
		}
		ReadOnlyCollection<SourceSpan> sourceSpans = statementSourceInfo.Method.SourceSpans;
		for (int i = statementSourceInfo.StatementNumber; i < sourceSpans.Count && sourceSpans[i].From.Line == lineNumber; i++)
		{
			codeCoverageScopeCounter.SetHitCount(i, (ushort)hitCount);
		}
	}

	private NavOption GetLineTypeOption(CodeCoderageSourceLineType lineType)
	{
		if (lineTypeOptions == null)
		{
			NCLOptionMetadata fieldOptionMetadata = MetaTable.GetFieldByNo(4).FieldOptionMetadata;
			NavOption[] array = new NavOption[fieldOptionMetadata.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = NavOption.Create(fieldOptionMetadata, i);
			}
			lineTypeOptions = array;
		}
		return lineTypeOptions[(int)lineType];
	}

	public void Dispose()
	{
		tree.Dispose();
	}
}
