using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class SymbolAggregator
{
	private class SymbolReferenceInfo
	{
		internal int ObjectId { get; set; }

		internal ObjectType ObjectType { get; set; }

		internal NavBLOB SymbolReference { get; set; }
	}

	private const int ApplicationObjectProcessingLimit = 400;

	private static byte[] ModuleDefinitionStart = Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{{\"Name\":\"{0}\",", "Application"));

	private static byte[] JsonObjectEnd = Encoding.UTF8.GetBytes(new char[1] { '}' });

	private static byte[] JsonArrayEnd = Encoding.UTF8.GetBytes(new char[1] { ']' });

	private static byte[] JsonSeparator = Encoding.UTF8.GetBytes(new char[1] { ',' });

	private static byte[] TablesStart = Encoding.UTF8.GetBytes("\"Tables\":[");

	private static byte[] PagesStart = Encoding.UTF8.GetBytes("\"Pages\":[");

	private static byte[] CodeunitsStart = Encoding.UTF8.GetBytes("\"Codeunits\":[");

	private static byte[] ReportsStart = Encoding.UTF8.GetBytes("\"Reports\":[");

	private static byte[] QueriesStart = Encoding.UTF8.GetBytes("\"Queries\":[");

	private static byte[] XmlPortsStart = Encoding.UTF8.GetBytes("\"XmlPorts\":[");

	private static ObjectType[] ObjectTypes = new ObjectType[6]
	{
		ObjectType.Table,
		ObjectType.CodeUnit,
		ObjectType.Page,
		ObjectType.Report,
		ObjectType.XmlPort,
		ObjectType.Query
	};

	private const int DefaultAggregatedStreamSize = 52428800;

	private const int StreamOffset = 512;

	private static int TempStreamSize = 52428800 / ObjectTypes.Length;

	private readonly IDictionary<int, SymbolReferenceInfo>[] applicationObjectIdsCache = new ConcurrentDictionary<int, SymbolReferenceInfo>[ObjectTypes.Length];

	private readonly IDictionary<ObjectType, HashSet<int>> modifiedApplicationObjects = new ConcurrentDictionary<ObjectType, HashSet<int>>();

	private readonly object mutex = new object();

	private NCLMetaTable objectMetadataTable;

	private NCLMetaField objectIdField;

	private NCLMetaField objectTypeField;

	private NCLMetaField symbolReferenceField;

	private bool readSymbolReferencesAtStartupOnce;

	internal FilterExpression IdFieldFilter { get; set; }

	internal SymbolAggregator()
	{
		for (int i = 0; i < applicationObjectIdsCache.Length; i++)
		{
			applicationObjectIdsCache[i] = new ConcurrentDictionary<int, SymbolReferenceInfo>();
		}
	}

	internal static Version GetApplicationVersion()
	{
		return Version.Parse(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
	}

	internal static bool TryCreateBaseApplicationReferenceSymbolStreamFromObjectMetadata(string name, string publisher, Version version, out Stream stream)
	{
		Version applicationVersion = GetApplicationVersion();
		stream = null;
		if (SymbolReferenceSpecification.IsApplicationSymbolReference(name, publisher) && (version == null || version <= applicationVersion))
		{
			SymbolAggregator symbolAggregator = NavGlobal.NCLMetadata.SymbolAggregator;
			if (symbolAggregator != null)
			{
				using MemoryStream memoryStream = symbolAggregator.CreateBaseAppSymbolReferenceStream();
				if (memoryStream != null && memoryStream.Length > 0)
				{
					stream = new MemoryStream((int)memoryStream.Length + 512);
					ModuleDefinition module = SymbolReferenceJsonReader.ReadModule(memoryStream);
					NavAppPackageWriter.CreateSymbolPackage(stream, module, name, applicationVersion);
					stream.Position = 0L;
				}
			}
		}
		return stream != null;
	}

	internal static string CreateModuleFileName(string publisherName, string appName)
	{
		return publisherName + "_" + appName + "_" + GetApplicationVersion()?.ToString() + ".app";
	}

	internal void StartReadingSymbolReferences()
	{
		if (!ServerUserSettings.Instance.EnableSymbolLoadingAtServerStartup.Value || readSymbolReferencesAtStartupOnce)
		{
			return;
		}
		lock (mutex)
		{
			if (!readSymbolReferencesAtStartupOnce)
			{
				NavTaskFactory.StartNewTask(() =>
				{
					Initialize();
				});
				readSymbolReferencesAtStartupOnce = true;
			}
		}
	}

	internal void OnMetadataChanged(NavTrackingEventArgs trackingEvents)
	{
		if (!ServerUserSettings.Instance.EnableSymbolLoadingAtServerStartup.Value)
		{
			return;
		}
		lock (mutex)
		{
			foreach (NavTrackingObject trackingObject in trackingEvents.TrackingObjects)
			{
				IDictionary<int, SymbolReferenceInfo> cache = GetCache(trackingObject.ObjectType);
				if (cache == null)
				{
					continue;
				}
				cache.Remove(trackingObject.ObjectId);
				if (!modifiedApplicationObjects.ContainsKey(trackingObject.ApplicationObjectId.ObjectType))
				{
					modifiedApplicationObjects.Add(trackingObject.ApplicationObjectId.ObjectType, new HashSet<int> { trackingObject.ApplicationObjectId.ObjectNumber });
					continue;
				}
				HashSet<int> hashSet = modifiedApplicationObjects[trackingObject.ApplicationObjectId.ObjectType];
				if (!hashSet.Contains(trackingObject.ApplicationObjectId.ObjectNumber))
				{
					hashSet.Add(trackingObject.ApplicationObjectId.ObjectNumber);
				}
				if (hashSet.Count % 400 == 0)
				{
					UpdateSymbolReferenceCacheWithModifiedObjects();
				}
			}
		}
		NavGlobal.SystemTenant.SymbolReferenceCache?.InvalidateModuleDefinition("Application", "Microsoft");
	}

	internal MemoryStream CreateBaseAppSymbolReferenceStream()
	{
		if (!ServerUserSettings.Instance.EnableSymbolLoadingAtServerStartup.Value || !readSymbolReferencesAtStartupOnce)
		{
			return null;
		}
		PerformanceLog performanceLog = new PerformanceLog("Creating symbol references trace");
		Stopwatch stopwatch = Stopwatch.StartNew();
		Dictionary<int, SymbolReferenceInfo>[] tempApplicationObjectIdsCache = null;
		lock (mutex)
		{
			UpdateSymbolReferenceCacheWithModifiedObjects();
			tempApplicationObjectIdsCache = new Dictionary<int, SymbolReferenceInfo>[applicationObjectIdsCache.Length];
			for (int i = 0; i < applicationObjectIdsCache.Length; i++)
			{
				tempApplicationObjectIdsCache[i] = new Dictionary<int, SymbolReferenceInfo>(applicationObjectIdsCache[i]);
			}
		}
		if (tempApplicationObjectIdsCache.Length == 0)
		{
			return null;
		}
		MemoryStream stream = MemoryStreamManager.GetStream(52428800);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001VY", DiagnosticParameter.SystemMetadata("Creating symbol references "), MonitorType.None, Category.Development, createNewActivity: false))
		{
			MemoryStream[] memoryStreams = new MemoryStream[tempApplicationObjectIdsCache.Length];
			bool hasAnyContent = false;
			try
			{
				Parallel.For(0, tempApplicationObjectIdsCache.Length, (int counter) =>
				{
					IDictionary<int, SymbolReferenceInfo> dictionary = tempApplicationObjectIdsCache[counter];
					if (memoryStreams[counter] == null)
					{
						memoryStreams[counter] = MemoryStreamManager.GetStream(TempStreamSize);
					}
					int num4 = 0;
					foreach (SymbolReferenceInfo value in dictionary.Values)
					{
						if (value.SymbolReference != null && !value.SymbolReference.IsZeroOrEmpty)
						{
							hasAnyContent = true;
							byte[] bytes = value.SymbolReference.GetBytes();
							int length = value.SymbolReference.GetLength();
							memoryStreams[counter].Write(bytes, 0, length);
							if (num4 < dictionary.Count - 1)
							{
								memoryStreams[counter].Write(JsonSeparator, 0, JsonSeparator.Length);
							}
						}
						num4++;
					}
				});
				if (!hasAnyContent)
				{
					return null;
				}
				stream.Write(ModuleDefinitionStart, 0, ModuleDefinitionStart.Length);
				for (int num = 0; num < tempApplicationObjectIdsCache.Length; num++)
				{
					byte[] jsonArrayStartOfApplicationObjects = GetJsonArrayStartOfApplicationObjects(ObjectTypes[num]);
					if (jsonArrayStartOfApplicationObjects != null)
					{
						stream.Write(jsonArrayStartOfApplicationObjects, 0, jsonArrayStartOfApplicationObjects.Length);
						MemoryStream memoryStream = memoryStreams[num];
						memoryStream.Position = 0L;
						stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
						int num2 = stream.GetBuffer()[stream.Position - 1];
						if (num2 == JsonSeparator[0])
						{
							stream.Position--;
						}
						stream.Write(JsonArrayEnd, 0, JsonArrayEnd.Length);
						if (num < tempApplicationObjectIdsCache.Length - 1)
						{
							stream.Write(JsonSeparator, 0, JsonSeparator.Length);
						}
					}
				}
				stream.Write(JsonObjectEnd, 0, JsonObjectEnd.Length);
				stream.Position = 0L;
			}
			finally
			{
				MemoryStream[] array = memoryStreams;
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					array[num3]?.Dispose();
				}
				stopwatch.Stop();
				performanceLog.RecordTimedEvent("Retrieved all symbol references in: ", stopwatch.Elapsed);
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001VX", Category.Development, Verbosity.Normal, PrivacyClassification.SystemMetadata, performanceLog.Report);
		return stream;
	}

	private static byte[] GetJsonArrayStartOfApplicationObjects(ObjectType objectType)
	{
		return objectType switch
		{
			ObjectType.Table => TablesStart, 
			ObjectType.Page => PagesStart, 
			ObjectType.CodeUnit => CodeunitsStart, 
			ObjectType.Query => QueriesStart, 
			ObjectType.XmlPort => XmlPortsStart, 
			ObjectType.Report => ReportsStart, 
			_ => null, 
		};
	}

	private PositionedFindProviderRequest CreateRequest(FilterExpression filterExpression, bool isFilterForMultipleFields)
	{
		SortingFieldList sortingFields = SortingFieldList.Create(new SortingField[2]
		{
			new SortingField(objectTypeField, SortOrder.Ascending),
			new SortingField(objectIdField, SortOrder.Ascending)
		});
		IList<Tuple<INavFieldMetadata, FilterExpression>> list = new List<Tuple<INavFieldMetadata, FilterExpression>>();
		if (isFilterForMultipleFields)
		{
			list.Add(new Tuple<INavFieldMetadata, FilterExpression>(FilterFieldDictionary.MultipleFields, filterExpression));
		}
		else
		{
			list.Add(new Tuple<INavFieldMetadata, FilterExpression>(objectTypeField, filterExpression));
			if (IdFieldFilter != null)
			{
				list.Add(new Tuple<INavFieldMetadata, FilterExpression>(objectIdField, IdFieldFilter));
			}
		}
		return new PositionedFindProviderRequest(0, objectMetadataTable, DataLockState.None, new FiltersAndMarks(new FilterFieldDictionary(list)), null, SecurityFiltering.Ignored, FieldList.Empty, sortingFields, FindType.All, null, includeCurrent: false);
	}

	private NavBLOB GetSymbolReference(ReadOnlyRecordBuffer recordBuffer)
	{
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(recordBuffer);
		if (NavSqlBlobCommand.GetBlob(objectMetadataTable, NavGlobal.AppDatabase, 0, new FieldList(symbolReferenceField), mutableRecordBuffer, locking: false) == 85132272)
		{
			return null;
		}
		return (NavBLOB)mutableRecordBuffer[symbolReferenceField.FieldIndex];
	}

	private void Initialize()
	{
		objectMetadataTable = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		objectIdField = objectMetadataTable.GetFieldByNo(6);
		objectTypeField = objectMetadataTable.GetFieldByNo(3);
		symbolReferenceField = objectMetadataTable.GetFieldByNo(35);
		PerformanceLog performanceLog = new PerformanceLog("Serializing symbol references to disk ");
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001VU", DiagnosticParameter.SystemMetadata("Reading symbol references "), MonitorType.None, Category.Development, createNewActivity: false))
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			try
			{
				ITenantSessionHandler tenant = NavGlobal.AppDatabase.Tenant;
				tenant.RunTenantActionInSystemSession((NavTenant navTenant, NavSession session) =>
				{
					ObjectType[] objectTypes = ObjectTypes;
					foreach (ObjectType objectType in objectTypes)
					{
						ReadSymbolReferences(objectType);
					}
				});
			}
			catch (Exception exception)
			{
				LogWriterHelper.LogExceptionEvent(exception, ignoreNavBaseExceptionUnlessAttributed: true);
			}
			finally
			{
				stopwatch.Stop();
				performanceLog.RecordTimedEvent("Symbol references were read ", stopwatch.Elapsed);
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QT", Category.Development, Verbosity.Normal, PrivacyClassification.SystemMetadata, performanceLog.Report);
	}

	private void ReadSymbolReferences(ObjectType objectType)
	{
		FilterExpression filterExpression = FilterExpression.Equal(NavOption.Create(objectTypeField.FieldOptionMetadata, (int)objectType), objectTypeField.ExpressionContext);
		ReadSymbolReferences(filterExpression);
	}

	private void ReadSymbolReferences(FilterExpression filterExpression, bool isFilterForMultipleFields = false)
	{
		PositionedFindProviderRequest request = CreateRequest(filterExpression, isFilterForMultipleFields);
		IList<ReadOnlyRecordBuffer> list = NavSqlIsamQueryCommand.FindAll(NavGlobal.AppDatabase, request, null).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = list[i];
			int value = ((NavInteger)readOnlyRecordBuffer[objectIdField.SqlColumnIndex]).Value;
			ObjectType objectType = (ObjectType)readOnlyRecordBuffer[objectTypeField.SqlColumnIndex].ToInt32();
			if (!NavRecord.IsSystemTable(value))
			{
				SymbolReferenceInfo symbolReferenceInfo = new SymbolReferenceInfo
				{
					ObjectId = value,
					ObjectType = objectType,
					SymbolReference = GetSymbolReference(readOnlyRecordBuffer)
				};
				IDictionary<int, SymbolReferenceInfo> cache = GetCache(symbolReferenceInfo.ObjectType);
				if (cache != null)
				{
					cache[symbolReferenceInfo.ObjectId] = symbolReferenceInfo;
				}
			}
		}
	}

	private IDictionary<int, SymbolReferenceInfo> GetCache(ObjectType objectType)
	{
		int num = Array.IndexOf(ObjectTypes, objectType);
		if (num == -1)
		{
			return null;
		}
		return applicationObjectIdsCache[num];
	}

	private void UpdateSymbolReferenceCacheWithModifiedObjects()
	{
		if (modifiedApplicationObjects.Count == 0)
		{
			return;
		}
		ParallelOptions parallelOptions = new ParallelOptions
		{
			MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, modifiedApplicationObjects.Keys.Count)
		};
		Parallel.ForEach(modifiedApplicationObjects.Keys, parallelOptions, (ObjectType objectType) =>
		{
			using (NavThread.AddCurrentThreadToScheduler(NavEnvironment.Instance.ThreadScheduler))
			{
				IList<int> list = modifiedApplicationObjects[objectType].ToList();
				FilterExpression left = FilterExpression.Equal(NavOption.Create(objectTypeField.FieldOptionMetadata, (int)objectType), objectTypeField.ExpressionContext);
				IList<FilterExpression> list2 = new List<FilterExpression>();
				foreach (int item in list)
				{
					FilterExpression filterExpression = FilterExpression.Equal(NavInteger.Create(item), objectIdField.ExpressionContext);
					list2.Add(FilterExpression.Or(filterExpression));
				}
				FilterExpression right = FilterExpression.BalancedOr(list2);
				FilterExpression filterExpression2 = FilterExpression.And(left, right);
				ReadSymbolReferences(filterExpression2, isFilterForMultipleFields: true);
			}
		});
		modifiedApplicationObjects.Clear();
	}
}
