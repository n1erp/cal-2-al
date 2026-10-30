using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AssemblyGetter
{
	private class AssemblyBucket
	{
		public long MaximumMetadataToken { get; set; } = -1L;

		public bool AnyFileChanged { get; set; }

		public List<string> SourceFiles { get; } = new List<string>();

		public List<NCLMetaApplicationObject> MetaApplicationObjects { get; } = new List<NCLMetaApplicationObject>();

		public static AssemblyBucket[] Create(int buckets)
		{
			AssemblyBucket[] array = new AssemblyBucket[buckets];
			for (int i = 0; i < buckets; i++)
			{
				array[i] = new AssemblyBucket();
			}
			return array;
		}
	}

	internal class MetaObject
	{
		internal long Timestamp { get; set; }

		internal int ObjectId { get; set; }

		internal string ObjectName { get; set; }

		internal string SourceFileName { get; set; }

		internal string MetadataFileName { get; set; }

		internal ReadOnlyRecordBuffer Record { get; set; }

		internal string Hash { get; set; }
	}

	private readonly TempPathHelper tempPathHelper;

	private const int StaticAssemblyBuildCommitSize = 100;

	private const string SourceExtension = "cs";

	private const string MetadataExtension = "xml";

	private static readonly long FileDateTimeSeed = new DateTime(2000, 1, 1).Ticks;

	private readonly CSharpCompiler compiler;

	private readonly TempPathHousekeeper housekeeper = new TempPathHousekeeper();

	private const int AssemblyBucketSize = 7;

	private readonly ConcurrentDictionary<ApplicationObjectId, MetaObject> startupApplicationIdCache = new ConcurrentDictionary<ApplicationObjectId, MetaObject>();

	public AssemblyGetter(TempPathHelper tempPathHelper)
	{
		this.tempPathHelper = tempPathHelper ?? throw new ArgumentNullException("tempPathHelper");
		ObjectType[] array = new ObjectType[9]
		{
			ObjectType.Table,
			ObjectType.Page,
			ObjectType.Report,
			ObjectType.CodeUnit,
			ObjectType.XmlPort,
			ObjectType.Query,
			ObjectType.MenuSuite,
			ObjectType.PageExtension,
			ObjectType.TableExtension
		};
		compiler = new CSharpCompiler();
		ObjectType[] array2 = array;
		foreach (ObjectType objectType in array2)
		{
			string path = Path.Combine(this.tempPathHelper.StaticSourcePath, ObjectTypeExtensions.ToUniqueShortString(objectType));
			string path2 = Path.Combine(this.tempPathHelper.BaseSourcePath, ObjectTypeExtensions.ToUniqueShortString(objectType));
			housekeeper.Initialize(path, "*.cs");
			housekeeper.Initialize(path, "*.xml");
			housekeeper.Initialize(path2, "*.cs");
		}
	}

	private static void GetBaseMetadataRecord(int objectId, ObjectType objectType, out ReadOnlyRecordBuffer record, out long objectTimestamp)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(27);
		NavValue[] array = new NavValue[metaTableById.FieldCount];
		array[fieldByNo.FieldIndex] = NavOption.Create(fieldByNo.FieldOptionMetadata, (int)objectType);
		array[fieldByNo2.FieldIndex] = NavInteger.Create(objectId);
		MutableRecordBuffer inputRecord = new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTableById, array));
		NCLMetaTable table = metaTableById;
		record = NavSqlIsamQueryCommand.FindEqual(NavGlobal.AppDatabase, table, 0, 61, locking: false, inputRecord, null, metaTableById.PrimaryKey.DefinedSortingFields);
		if (record == null)
		{
			throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, objectType, objectId, null);
		}
		NCLMetaApplicationObject.VerifyMetadataVersion((NavInteger)record[fieldByNo2.FieldIndex], (ObjectType)((NavOption)record[fieldByNo.FieldIndex]).Value, (NavInteger)record[fieldByNo3.FieldIndex]);
		objectTimestamp = (NavBigInteger)record[metaTableById.TimestampField.FieldIndex];
	}

	internal void DeleteUnneededSources()
	{
		housekeeper.Clear();
	}

	internal void CompileBusinessApplication()
	{
		if (NavEnvironment.DisableCompileBusinessApplication)
		{
			return;
		}
		try
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			EventHandler<NavTrackingEventArgs> value = (object sender, NavTrackingEventArgs args) =>
			{
				foreach (NavTrackingObject trackingObject in args.TrackingObjects)
				{
					MetaObject value2 = null;
					startupApplicationIdCache.TryRemove(trackingObject.ApplicationObjectId, out value2);
				}
			};
			NavGlobal.AppDatabase.MetadataChanged += value;
			int totalAmountOfObjects = 0;
			try
			{
				ITenantSessionHandler tenant = NavGlobal.AppDatabase.Tenant;
				tenant.RunTenantActionInSystemSession((NavTenant navTenant, NavSession session) =>
				{
					ObjectType[] array = new ObjectType[3]
					{
						ObjectType.Table,
						ObjectType.CodeUnit,
						ObjectType.Page
					};
					foreach (ObjectType objectType in array)
					{
						totalAmountOfObjects += InitializeStaticAssembly(tempPathHelper.StaticAssemblyPath, objectType, () => GetBaseMetaObjects(objectType), 7).Count;
					}
				});
			}
			catch (ObjectDisposedException) when (NavEnvironment.Instance.InServerShutdown)
			{
			}
			finally
			{
				NavGlobal.AppDatabase.MetadataChanged -= value;
				if (!NavEnvironment.Instance.InServerShutdown)
				{
					startupApplicationIdCache.Clear();
				}
			}
			stopwatch.Stop();
			if (!NavEnvironment.Instance.InServerShutdown)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The business assemblies have been compiled and loaded. The operation took {0} milliseconds and contained {1} application objects.", stopwatch.ElapsedMilliseconds, totalAmountOfObjects);
				LogWriterHelper.LogEvent(EventLogEntryType.Information, message);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000N0", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, message);
			}
		}
		catch (Exception exception)
		{
			LogWriterHelper.LogExceptionEvent(exception, ignoreNavBaseExceptionUnlessAttributed: true);
		}
	}

	internal void CompileBusinessApplicationEvents()
	{
		if (NavEnvironment.DisableCompileBusinessApplication)
		{
			return;
		}
		try
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			EventHandler<NavTrackingEventArgs> value = (object sender, NavTrackingEventArgs args) =>
			{
				foreach (NavTrackingObject trackingObject in args.TrackingObjects)
				{
					startupApplicationIdCache.TryRemove(trackingObject.ApplicationObjectId, out var _);
				}
			};
			NavGlobal.AppDatabase.MetadataChanged += value;
			int totalAmountOfEvents = 0;
			try
			{
				ITenantSessionHandler tenant = NavGlobal.AppDatabase.Tenant;
				tenant.RunTenantActionInSystemSession((NavTenant navTenant, NavSession session) =>
				{
					List<Type> list = InitializeStaticAssembly(tempPathHelper.StaticSubscriberAssemblyPath, ObjectType.CodeUnit, GetBaseEventSubscriberMetaObjects, 1);
					totalAmountOfEvents += list.Count;
					List<ApplicationObjectId> tableApplicationObjectIds = new List<ApplicationObjectId>();
					List<ApplicationObjectId> codeunitApplicationObjectIds = new List<ApplicationObjectId>();
					List<ApplicationObjectId> pageApplicationObjectIds = new List<ApplicationObjectId>();
					foreach (Type item in list)
					{
						foreach (ApplicationObjectId item2 in NavEventSubscription.FindOriginalApplicationObjectIds(item))
						{
							switch (item2.ObjectType)
							{
							case ObjectType.Table:
								tableApplicationObjectIds.Add(item2);
								break;
							case ObjectType.CodeUnit:
								codeunitApplicationObjectIds.Add(item2);
								break;
							case ObjectType.Page:
								pageApplicationObjectIds.Add(item2);
								break;
							}
						}
					}
					if (tableApplicationObjectIds.Count > 0)
					{
						totalAmountOfEvents += InitializeStaticAssembly(tempPathHelper.StaticPublisherAssemblyPath, ObjectType.Table, () => GetBaseMetaObjects(ObjectType.Table, tableApplicationObjectIds), 1).Count;
					}
					if (codeunitApplicationObjectIds.Count > 0)
					{
						totalAmountOfEvents += InitializeStaticAssembly(tempPathHelper.StaticPublisherAssemblyPath, ObjectType.CodeUnit, () => GetBaseMetaObjects(ObjectType.CodeUnit, codeunitApplicationObjectIds), 1).Count;
					}
					if (pageApplicationObjectIds.Count > 0)
					{
						totalAmountOfEvents += InitializeStaticAssembly(tempPathHelper.StaticPublisherAssemblyPath, ObjectType.Page, () => GetBaseMetaObjects(ObjectType.Page, pageApplicationObjectIds), 1).Count;
					}
				});
			}
			finally
			{
				NavGlobal.AppDatabase.MetadataChanged -= value;
				startupApplicationIdCache.Clear();
			}
			stopwatch.Stop();
			if (!NavEnvironment.Instance.InServerShutdown)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The business assemblies for events have been compiled and loaded. The operation took {0} milliseconds and contained {1} application objects.", stopwatch.ElapsedMilliseconds, totalAmountOfEvents);
				LogWriterHelper.LogEvent(EventLogEntryType.Information, message);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000N1", Category.EventSubscription, Verbosity.Normal, PrivacyClassification.SystemMetadata, message);
			}
		}
		catch (Exception exception)
		{
			LogWriterHelper.LogExceptionEvent(exception, ignoreNavBaseExceptionUnlessAttributed: true);
		}
	}

	public Assembly GetAssemblyForBaseObject(ObjectType objectType, int objectId)
	{
		Tuple<string, long> baseObjectSource = GetBaseObjectSource(objectType, objectId);
		string item = baseObjectSource.Item1;
		long item2 = baseObjectSource.Item2;
		long num = OffsetTimestamp(item2);
		string assemblyPath = Build1To1AssemblyFilePath(objectType, objectId);
		string text = Build1To1SourceFilePath(objectType, objectId, "cs");
		WriteSourceFileToDisk(text, item, num);
		AssemblyBucket assemblyBucket = new AssemblyBucket();
		assemblyBucket.SourceFiles.Add(text);
		assemblyBucket.MaximumMetadataToken = num;
		return MakeBaseAssembly(assemblyPath, assemblyBucket);
	}

	private static Tuple<string, long> GetBaseObjectSource(ObjectType objectType, int objectId)
	{
		GetBaseMetadataRecord(objectId, objectType, out var record, out var objectTimestamp);
		string baseSourceCode = GetBaseSourceCode(record);
		ValidateSource(baseSourceCode, objectType, objectId);
		return Tuple.Create(baseSourceCode, objectTimestamp);
	}

	private static void ValidateSource(string src, ObjectType objectType, int objectId)
	{
		if (string.IsNullOrEmpty(src))
		{
			throw new NavMetadataNotFoundException(objectType, objectId);
		}
		if (src.StartsWith("###", StringComparison.Ordinal))
		{
			throw new NavNCLMetadataCSharpErrorException(CultureInfo.CurrentCulture, objectType, objectId, src.Substring(5), null);
		}
	}

	private static string GetBaseSourceCode(ReadOnlyRecordBuffer record)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(15);
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(record);
		NavSqlBlobCommand.GetBlob(metaTableById, NavGlobal.AppDatabase, 0, new FieldList(fieldByNo), mutableRecordBuffer, locking: false);
		NavBLOB navBLOB = (NavBLOB)mutableRecordBuffer[fieldByNo.FieldIndex];
		return Encoding.UTF8.GetString(navBLOB.GetBytes());
	}

	private List<Type> InitializeStaticAssembly(string assemblyBasePath, ObjectType objectType, Func<List<MetaObject>> metaObjectFunc, int assemblyBucketSize)
	{
		if (NavEnvironment.Instance.InServerShutdown)
		{
			return new List<Type>();
		}
		List<NavTrackingObject> radChanges = new List<NavTrackingObject>();
		EventHandler<NavTrackingEventArgs> value = (object sender, NavTrackingEventArgs args) =>
		{
			IEnumerable<NavTrackingObject> collection = args.TrackingObjects.Where((NavTrackingObject navTrackingObject) => navTrackingObject.ObjectType == objectType);
			lock (radChanges)
			{
				radChanges.AddRange(collection);
			}
		};
		AssemblyBucket[] array = AssemblyBucket.Create(assemblyBucketSize);
		NavGlobal.AppDatabase.MetadataChanged += value;
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, updateOrlockingActivity: true);
			List<MetaObject> list = metaObjectFunc();
			int num = 0;
			NavCurrentThread.Session.BeginTransaction();
			foreach (MetaObject item in list)
			{
				NavCurrentThread.Session.ThrowSessionTerminatedExceptionIfStoppingOrDisposed();
				if (!GetBaseMetadataObjectAndTimestamp(objectType, item, radChanges, out var nclMetaApplicationObject, out var currentTimestamp))
				{
					continue;
				}
				int num2 = item.ObjectId % assemblyBucketSize;
				AssemblyBucket assemblyBucket = array[num2];
				long num3 = OffsetTimestamp(currentTimestamp, item.Hash);
				if (File.GetLastWriteTimeUtc(item.SourceFileName).Ticks != num3)
				{
					string baseSourceCode = GetBaseSourceCode(item.Record);
					if (string.IsNullOrEmpty(baseSourceCode) || baseSourceCode.StartsWith("###", StringComparison.Ordinal))
					{
						continue;
					}
					assemblyBucket.AnyFileChanged = true;
					DoWriteTheFile(item.SourceFileName, baseSourceCode, num3);
				}
				housekeeper.MarkFileAsKnown(item.SourceFileName);
				housekeeper.MarkFileAsKnown(item.MetadataFileName);
				assemblyBucket.SourceFiles.Add(item.SourceFileName);
				assemblyBucket.MetaApplicationObjects.Add(nclMetaApplicationObject);
				if (assemblyBucket.MaximumMetadataToken < num3)
				{
					assemblyBucket.MaximumMetadataToken = num3;
				}
				if (num % 100 == 0)
				{
					navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				}
				num++;
			}
		}
		finally
		{
			NavGlobal.AppDatabase.MetadataChanged -= value;
		}
		List<Type> list2 = new List<Type>();
		for (int num4 = 0; num4 < assemblyBucketSize; num4++)
		{
			NavCurrentThread.Session.ThrowSessionTerminatedExceptionIfStoppingOrDisposed();
			try
			{
				string assemblyPath = Path.Combine(assemblyBasePath, string.Concat("STATIC_" + ObjectTypeExtensions.ToUniqueShortString(objectType), "_", num4, ".dll"));
				Assembly assembly = null;
				if (array[num4].SourceFiles.Any() && !NavEnvironment.Instance.InServerShutdown)
				{
					assembly = MakeBaseAssembly(assemblyPath, array[num4]);
				}
				list2.AddRange(SetBaseClrObjects(assembly, array[num4].MetaApplicationObjects, continueOnErrorSettingClrObjects: true));
			}
			catch (Exception ex)
			{
				LogWriterHelper.LogEvent(EventLogEntryType.Error, string.Format(CultureInfo.InvariantCulture, "The business assembly for type {0} bucket {1} cannot be loaded because of the following error: {2}", objectType, num4, ex.Message));
				NavCurrentThread.Session.Diagnostics.SendExceptionTag("00000N2", Category.Runtime, ex);
			}
		}
		return list2;
	}

	private static bool GetBaseMetadataObjectAndTimestamp(ObjectType objectType, MetaObject metaObject, List<NavTrackingObject> radChanges, out NCLMetaApplicationObject nclMetaApplicationObject, out long currentTimestamp)
	{
		try
		{
			nclMetaApplicationObject = NavGlobal.NCLMetadata.GetMetaApplicationObject(objectType, metaObject.ObjectId, NavAppGroup.BaseGroup.GroupId, requireCompiled: false);
		}
		catch (NavCSideException)
		{
			nclMetaApplicationObject = null;
			currentTimestamp = 0L;
			return false;
		}
		currentTimestamp = nclMetaApplicationObject.Timestamp;
		if (currentTimestamp == 0L)
		{
			if (objectType != ObjectType.Table && objectType != ObjectType.CodeUnit)
			{
				return false;
			}
			currentTimestamp = metaObject.Timestamp;
		}
		lock (radChanges)
		{
			foreach (NavTrackingObject radChange in radChanges)
			{
				if (radChange.ObjectId == metaObject.ObjectId)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal ReadOnlyRecordBuffer GetBaseApplicationObjectRecord(ApplicationObjectId objectId)
	{
		MetaObject value = null;
		if (startupApplicationIdCache.TryGetValue(objectId, out value))
		{
			return value.Record;
		}
		return null;
	}

	private List<MetaObject> GetBaseMetaObjects(ObjectType objectType, List<ApplicationObjectId> objects = null)
	{
		List<MetaObject> list = new List<MetaObject>();
		using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000071))
		{
			navRecord.ALSetRange(3, NavInteger.Create((int)objectType));
			if (objects != null && objects.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ApplicationObjectId @object in objects)
				{
					stringBuilder.Append(@object.ObjectNumber).Append('|');
				}
				stringBuilder.Length--;
				navRecord.ALSetFilter(6, stringBuilder.ToString());
			}
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					MetaObject metaObject = CreateMetaObject(objectType, navRecord);
					startupApplicationIdCache[new ApplicationObjectId(objectType, metaObject.ObjectId)] = metaObject;
					list.Add(metaObject);
				}
				while (navRecord.ALNext(1) != 0);
			}
		}
		return list;
	}

	private List<MetaObject> GetBaseEventSubscriberMetaObjects()
	{
		List<MetaObject> list = new List<MetaObject>();
		using (NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000071))
		{
			navRecord.ALSetRange(3, NavInteger.Create(5));
			navRecord.ALSetRange(34, NavBoolean.True);
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					MetaObject metaObject = CreateMetaObject(ObjectType.CodeUnit, navRecord);
					startupApplicationIdCache[new ApplicationObjectId(ObjectType.CodeUnit, metaObject.ObjectId)] = metaObject;
					list.Add(metaObject);
				}
				while (navRecord.ALNext(1) != 0);
			}
		}
		return list;
	}

	private MetaObject CreateMetaObject(ObjectType objectType, NavRecord objectMetadataRec)
	{
		MetaObject metaObject = new MetaObject();
		metaObject.Timestamp = objectMetadataRec.GetFieldValue(0).ToInt64();
		metaObject.ObjectId = objectMetadataRec.GetFieldValue(6).ToInt32();
		metaObject.ObjectName = NCLCodeLoader.ClassName(objectType, metaObject.ObjectId);
		metaObject.SourceFileName = BuildStaticSourceFilePath(objectType, metaObject.ObjectId, "cs");
		metaObject.MetadataFileName = BuildStaticSourceFilePath(objectType, metaObject.ObjectId, "xml");
		metaObject.Record = objectMetadataRec.RecordImplementation.CopyOfCurrentReadOnlyRecordBuffer();
		metaObject.Hash = objectMetadataRec.GetFieldValue(30).ToString();
		return metaObject;
	}

	private Assembly MakeBaseAssembly(string assemblyPath, AssemblyBucket assemblyBucket)
	{
		if (!assemblyBucket.SourceFiles.Any())
		{
			throw new ArgumentException("Cannot compile an assembly with zero source files");
		}
		string path = Path.ChangeExtension(assemblyPath, "pdb");
		string directoryName = Path.GetDirectoryName(assemblyPath);
		if (directoryName != null && !Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		if (!ServerUserSettings.Instance.EnableDebugging.Value)
		{
			File.Delete(path);
		}
		bool flag = !assemblyBucket.AnyFileChanged && File.Exists(assemblyPath) && FileVersionInfo.GetVersionInfo(assemblyPath).FileVersion == compiler.ExecutingAssemblyFileVersionInfo.FileVersion && File.GetLastWriteTimeUtc(assemblyPath).Ticks == assemblyBucket.MaximumMetadataToken && (!ServerUserSettings.Instance.EnableDebugging.Value || File.GetLastWriteTimeUtc(path).Ticks == assemblyBucket.MaximumMetadataToken);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006S9", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Base MakeAssembly -- Assembly:{0}; AssemblyExists:{1}; KeepExisting:{2}", Path.GetFileName(assemblyPath), File.Exists(assemblyPath), flag);
		Assembly result;
		if (flag)
		{
			byte[] rawAssembly = File.ReadAllBytes(assemblyPath);
			if (ServerUserSettings.Instance.EnableDebugging.Value)
			{
				byte[] rawSymbolStore = File.ReadAllBytes(path);
				result = Assembly.Load(rawAssembly, rawSymbolStore);
			}
			else
			{
				result = Assembly.Load(rawAssembly);
			}
		}
		else
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			string fileName = Path.GetFileName(assemblyPath);
			NavAppCSharpCompilerResult navAppCSharpCompilerResult = compiler.CompileCSharpFiles(fileName, assemblyBucket.SourceFiles.ToArray(), ServerUserSettings.Instance.EnableDebugging.Value);
			if (navAppCSharpCompilerResult.Errors != null)
			{
				throw new InvalidOperationException(navAppCSharpCompilerResult.Errors);
			}
			if (ServerUserSettings.Instance.EnableDebugging.Value)
			{
				result = Assembly.Load(navAppCSharpCompilerResult.AssemblyContent, navAppCSharpCompilerResult.PdbContent);
				File.WriteAllBytes(path, navAppCSharpCompilerResult.PdbContent);
				File.SetLastWriteTimeUtc(path, new DateTime(assemblyBucket.MaximumMetadataToken));
			}
			else
			{
				result = Assembly.Load(navAppCSharpCompilerResult.AssemblyContent);
			}
			File.WriteAllBytes(assemblyPath, navAppCSharpCompilerResult.AssemblyContent);
			File.SetLastWriteTimeUtc(assemblyPath, new DateTime(assemblyBucket.MaximumMetadataToken));
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006SA", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Base assembly compilation finished -- Assembly:{0}; Sources:{1}; Elapsed:{2}", Path.GetFileName(assemblyPath), assemblyBucket.SourceFiles.Count, stopwatch.Elapsed.ToStringWithMilliseconds());
		}
		return result;
	}

	private static List<Type> SetBaseClrObjects(Assembly assembly, IEnumerable<NCLMetaApplicationObject> applicationObjects, bool continueOnErrorSettingClrObjects)
	{
		List<Type> typesLoaded = new List<Type>();
		if (assembly != null)
		{
			foreach (NCLMetaApplicationObject nclMetaApplicationObject in applicationObjects)
			{
				string name = "Microsoft.Dynamics.Nav.BusinessApplication." + NCLCodeLoader.ClassName(nclMetaApplicationObject.ApplicationObjectType, nclMetaApplicationObject.ObjectId);
				Type type = assembly.GetType(name, throwOnError: false, ignoreCase: true);
				if (type == null)
				{
					continue;
				}
				lock (nclMetaApplicationObject)
				{
					Action action = () =>
					{
						nclMetaApplicationObject.SetClrObject(type);
						typesLoaded.Add(type);
					};
					if (continueOnErrorSettingClrObjects)
					{
						try
						{
							action();
						}
						catch (NavNCLObjectNotFoundException)
						{
						}
						catch (NavObjectDefinitionChangedException)
						{
						}
					}
					else
					{
						action();
					}
				}
			}
		}
		return typesLoaded;
	}

	internal static string BuildMetadataFileNameUsingStaticDependency(ObjectType objectType, int objectId)
	{
		string path = Path.Combine(NavEnvironment.Instance.TemporaryPathHelper.StaticSourcePath, ObjectTypeExtensions.ToUniqueShortString(objectType));
		string path2 = ObjectTypeExtensions.ToUniqueShortString(objectType) + "_" + objectId + ".xml";
		return Path.Combine(path, path2);
	}

	private string Build1To1AssemblyFilePath(ObjectType objectType, int objectId)
	{
		string path = ObjectTypeExtensions.ToUniqueShortString(objectType) + "_" + objectId + ".dll";
		return Path.Combine(tempPathHelper.BaseAssemblyPath, ObjectTypeExtensions.ToUniqueShortString(objectType), path);
	}

	private string Build1To1SourceFilePath(ObjectType objectType, int objectId, string extension)
	{
		string path = ObjectTypeExtensions.ToUniqueShortString(objectType) + "_" + objectId + "." + extension;
		return Path.Combine(tempPathHelper.BaseSourcePath, ObjectTypeExtensions.ToUniqueShortString(objectType), path);
	}

	private string BuildStaticSourceFilePath(ObjectType objectType, int objectId, string extension)
	{
		string path = ObjectTypeExtensions.ToUniqueShortString(objectType) + "_" + objectId + "." + extension;
		return Path.Combine(tempPathHelper.StaticSourcePath, ObjectTypeExtensions.ToUniqueShortString(objectType), path);
	}

	private void WriteSourceFileToDisk(string sourceFileName, string source, long timestampWithOffsetApplied)
	{
		housekeeper.MarkFileAsKnown(sourceFileName);
		if (!File.Exists(sourceFileName))
		{
			DoWriteTheFile(sourceFileName, source, timestampWithOffsetApplied);
			return;
		}
		long ticks = File.GetLastWriteTimeUtc(sourceFileName).Ticks;
		if (ticks != timestampWithOffsetApplied)
		{
			DoWriteTheFile(sourceFileName, source, timestampWithOffsetApplied);
		}
	}

	internal static long OffsetTimestamp(long objectTimestamp, string hash = null)
	{
		ushort num = 0;
		if (hash != null && hash.Length > 8)
		{
			num = Convert.ToUInt16(hash.Substring(0, 4), 16);
		}
		return num + (objectTimestamp << 16) + NavGlobal.AppDatabase.SqlDatabaseProperties.DatabaseMagicNo + FileDateTimeSeed;
	}

	private static void DoWriteTheFile(string sourceFileName, string source, long objectTimestamp)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(sourceFileName);
			if (directoryName != null && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			using (StreamWriter streamWriter = new StreamWriter(new FileStream(sourceFileName, FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.None), Encoding.UTF8))
			{
				streamWriter.Write(source);
			}
			File.SetLastWriteTimeUtc(sourceFileName, new DateTime(objectTimestamp));
		}
		catch (Exception exception)
		{
			TempPathHousekeeper.MapClrExceptionAndRethrow(exception);
			throw;
		}
	}
}
