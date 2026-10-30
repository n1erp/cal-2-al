using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal sealed class ModelChangeTool : ReadWriteModelTool
{
	private readonly ModelChangeOptions options;

	private static readonly ModelChangeOptions DefaultOptions = new ModelChangeOptions();

	internal ModelChangeTool(ILogger logger, ModelChangeOptions options = null)
		: base(logger, options)
	{
		this.options = options ?? DefaultOptions;
	}

	public CompareInfoCollection CompareApplicationObject(string[] original, string[] modified, string delta, string originalName = null, string modifiedName = null)
	{
		ApplicationObjectFileInfoCollection originalObjectFiles = ReadObjects(original, originalName);
		ObjectCollection objectCollection = originalObjectFiles.ObjectCollection;
		CheckObjects(objectCollection, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.OriginalParameterText);
		ApplicationObjectFileInfoCollection modifiedObjectFiles = ReadObjects(modified, modifiedName);
		ObjectCollection objectCollection2 = modifiedObjectFiles.ObjectCollection;
		CheckObjects(objectCollection2, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.ModifiedParameterText);
		List<ApplicationObjectFileInfo> source = (from io in objectCollection.GetInvalidApplicationObjects()
			select originalObjectFiles[io]).Union(from io in objectCollection2.GetInvalidApplicationObjects()
			select modifiedObjectFiles[io]).ToList();
		IEnumerable<ApplicationObject> second = source.Select((ApplicationObjectFileInfo o) => o.ApplicationObject);
		objectCollection = new ObjectCollection(objectCollection.Except(second, ApplicationObject.EqualityComparer));
		objectCollection2 = new ObjectCollection(objectCollection2.Except(second, ApplicationObject.EqualityComparer));
		ObjectCollection deltaObjects = CreateDeltaObjects(objectCollection, objectCollection2);
		IEnumerable<ApplicationObject> enumerable = objectCollection.Where((ApplicationObject o) => !deltaObjects.Modifies(o));
		IEnumerable<ApplicationObject> source2 = objectCollection.Except(objectCollection2, ApplicationObject.EqualityComparer);
		ApplicationObjectFileInfoCollection applicationObjectFileInfoCollection = WriteApplicationObjects(deltaObjects, delta, OutputType.CompareResult);
		List<CompareInfo> list = source.Select((ApplicationObjectFileInfo o) => new CompareInfo((InvalidApplicationObject)o.ApplicationObject, o.FileName)).ToList();
		foreach (ApplicationObjectFileInfo item in applicationObjectFileInfoCollection)
		{
			CompareResult result = CompareResult.Inserted;
			string type = item.ApplicationObject.ElementTypeInfo.ElementType.ToString();
			int objectNumber = item.ApplicationObject.Id.ObjectNumber;
			if (item.ApplicationObject.ElementTypeInfo.ElementType == ElementType.Modification)
			{
				string key = item.ApplicationObject.RootElement.GetStringProperty(PropertyType.Target);
				ApplicationObject applicationObject = objectCollection.Single((ApplicationObject o) => o.RootElement.CreateDisplayElementKey() == key);
				type = applicationObject.ElementTypeInfo.ElementType.ToString();
				objectNumber = applicationObject.Id.ObjectNumber;
				result = CompareResult.Delta;
				key = item.ApplicationObject.RootElement.GetStringProperty(PropertyType.Target);
				if (source2.Any((ApplicationObject o) => o.RootElement.CreateDisplayElementKey() == key))
				{
					result = CompareResult.Deleted;
				}
			}
			list.Add(new CompareInfo(type, objectNumber, result, new ApplicationObjectFileInfo(item.FileName, item.ApplicationObject)));
		}
		foreach (ApplicationObject item2 in enumerable)
		{
			list.Add(new CompareInfo(item2.ElementTypeInfo.ElementType.ToString(), item2.Id.ObjectNumber, CompareResult.Identical));
		}
		return new CompareInfoCollection(list);
	}

	public UpdateInfoCollection UpdateApplicationObject(string[] target, string[] delta, string result, string targetName = null, string deltaName = null)
	{
		string fileSpec = (Directory.Exists(result) ? result : Path.GetDirectoryName(result));
		ApplicationObjectFileInfoCollection targetObjectFiles = ReadObjects(target, targetName);
		ObjectCollection objectCollection = targetObjectFiles.ObjectCollection;
		CheckObjects(objectCollection, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.OriginalParameterText);
		ApplicationObjectFileInfoCollection deltaObjectFiles = ReadObjects(delta, deltaName);
		ObjectCollection deltaObjects = deltaObjectFiles.ObjectCollection;
		List<ApplicationObjectFileInfo> source = (from io in objectCollection.GetInvalidApplicationObjects()
			select targetObjectFiles[io]).Union(from io in deltaObjects.GetInvalidApplicationObjects()
			select deltaObjectFiles[io]).ToList();
		IEnumerable<ApplicationObject> second = source.Select((ApplicationObjectFileInfo o) => o.ApplicationObject);
		objectCollection = new ObjectCollection(objectCollection.Except(second, ApplicationObject.EqualityComparer));
		deltaObjects = new ObjectCollection(deltaObjects.Except(second, ApplicationObject.EqualityComparer));
		ObjectCollection objectCollection2 = ApplyDeltaObjects(objectCollection, deltaObjects);
		IEnumerable<ApplicationObject> enumerable = from o in objectCollection2.Except(objectCollection, ApplicationObject.EqualityComparer)
			where o.ElementTypeInfo.ElementType != ElementType.Modification
			select o;
		IEnumerable<ApplicationObject> source2 = objectCollection.Except(objectCollection2, ApplicationObject.EqualityComparer);
		IEnumerable<ApplicationObject> enumerable2 = objectCollection2.Where((ApplicationObject o) => o.ElementTypeInfo.ElementType != ElementType.Modification && !deltaObjects.Modifies(o)).Except(enumerable, ApplicationObject.EqualityComparer);
		ObjectCollection conflictObjects = new ObjectCollection(objectCollection2.GetApplicationObjects(ElementType.Modification));
		IEnumerable<ApplicationObject> enumerable3 = objectCollection2.Where((ApplicationObject o) => conflictObjects.Modifies(o));
		IEnumerable<ApplicationObject> source3 = objectCollection2.Except(enumerable, ApplicationObject.EqualityComparer).Except(enumerable2, ApplicationObject.EqualityComparer).Except(conflictObjects, ApplicationObject.EqualityComparer)
			.Except(enumerable3, ApplicationObject.EqualityComparer);
		foreach (ApplicationObject item in enumerable.Concat(enumerable2))
		{
			SetApplicationObjectProperties(item.ObjectProperties);
		}
		Dictionary<ApplicationObject, FileInfo> resultFileMap = WriteApplicationObjects(objectCollection2.Except(conflictObjects, ApplicationObject.EqualityComparer), result, OutputType.MergeResult).ObjectFileMap;
		Dictionary<ApplicationObject, FileInfo> objectFileMap = WriteApplicationObjects(conflictObjects, fileSpec, OutputType.MergeResult).ObjectFileMap;
		List<UpdateInfo> list = source.Select((ApplicationObjectFileInfo o) => new UpdateInfo((InvalidApplicationObject)o.ApplicationObject, o.FileName)).ToList();
		list.AddRange(enumerable.Select((ApplicationObject ao) => new UpdateInfo(UpdateResult.Inserted, new ApplicationObjectFileInfo(resultFileMap[ao].FullName, ao))));
		list.AddRange(enumerable2.Select((ApplicationObject ao) => new UpdateInfo(UpdateResult.Unchanged, new ApplicationObjectFileInfo(resultFileMap[ao].FullName, ao))));
		list.AddRange(source3.Select((ApplicationObject ao) => new UpdateInfo(UpdateResult.Updated, new ApplicationObjectFileInfo(resultFileMap[ao].FullName, ao))));
		list.AddRange(source2.Select((ApplicationObject o) => new UpdateInfo(o.ElementTypeInfo.ElementType.ToString(), o.Id.ObjectNumber, UpdateResult.Deleted)));
		foreach (ApplicationObject item2 in (IEnumerable<ApplicationObject>)conflictObjects)
		{
			string targetKey = item2.RootElement.GetStringProperty(PropertyType.Target);
			ApplicationObject applicationObject = enumerable3.SingleOrDefault((ApplicationObject o) => o.RootElement.CreateDisplayElementKey() == targetKey);
			if (applicationObject == null)
			{
				IElementKeySegment elementKeySegment = ElementKeyHelper.DisplayProvider.GetElementKeySegments(targetKey).Single();
				list.Add(new UpdateInfo(elementKeySegment.ElementType.ToString(), int.Parse(elementKeySegment.Id, CultureInfo.InvariantCulture), UpdateResult.Conflict, ApplicationObjectFileInfo.Empty, new ApplicationObjectFileInfo(objectFileMap[item2].FullName, item2)));
			}
			else
			{
				list.Add(new UpdateInfo(UpdateResult.Conflict, new ApplicationObjectFileInfo(resultFileMap[applicationObject].FullName, applicationObject), new ApplicationObjectFileInfo(objectFileMap[item2].FullName, item2)));
			}
		}
		string versionListPropertyName = EnumNameLookup<PropertyType>.EnumToString(PropertyType.VersionList);
		return new UpdateInfoCollection(list, deltaObjects.GetElements().Count((IElement e) => e.ElementTypeInfo.ElementType == ElementType.Change && e.GetStringProperty(PropertyType.Property) != versionListPropertyName), conflictObjects.GetElements().Count((IElement e) => e.ElementTypeInfo.ElementType == ElementType.Change));
	}

	public MergeInfoCollection MergeApplicationObject(string[] original, string[] modified, string[] target, string output, string originalName = null, string modifiedName = null, string targetName = null)
	{
		string text = (Directory.Exists(output) ? output : Path.GetDirectoryName(output));
		ApplicationObjectFileInfoCollection originalObjectFiles = ReadObjects(original, originalName);
		ObjectCollection objectCollection = originalObjectFiles.ObjectCollection;
		CheckObjects(objectCollection, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.OriginalParameterText);
		ApplicationObjectFileInfoCollection modifiedObjectFiles = ReadObjects(modified, modifiedName);
		ObjectCollection objectCollection2 = modifiedObjectFiles.ObjectCollection;
		CheckObjects(objectCollection2, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.ModifiedParameterText);
		ApplicationObjectFileInfoCollection targetObjectFiles = ReadObjects(target, targetName);
		ObjectCollection objectCollection3 = targetObjectFiles.ObjectCollection;
		CheckObjects(objectCollection3, InputType.ApplicationObject, NonLocalizable.CheckObjectsOnlyApplicationObjects, NonLocalizable.TargetParameterText);
		List<ApplicationObjectFileInfo> source = (from io in objectCollection.GetInvalidApplicationObjects()
			select originalObjectFiles[io]).Union(from io in objectCollection2.GetInvalidApplicationObjects()
			select modifiedObjectFiles[io]).Union(from io in objectCollection3.GetInvalidApplicationObjects()
			select targetObjectFiles[io]).ToList();
		IEnumerable<ApplicationObject> second = source.Select((ApplicationObjectFileInfo o) => o.ApplicationObject);
		List<MergeInfo> list = source.Select((ApplicationObjectFileInfo of) => new MergeInfo((InvalidApplicationObject)of.ApplicationObject, of.FileName)).ToList();
		objectCollection = new ObjectCollection(objectCollection.Except(second, ApplicationObject.EqualityComparer));
		objectCollection2 = new ObjectCollection(objectCollection2.Except(second, ApplicationObject.EqualityComparer));
		objectCollection3 = new ObjectCollection(objectCollection3.Except(second, ApplicationObject.EqualityComparer));
		ObjectCollection deltaObjects = CreateDeltaObjects(objectCollection, objectCollection2);
		ObjectCollection deletedInModified = new ObjectCollection();
		ObjectCollection changedObjectDeletions = GetChangedObjectDeletions(objectCollection, deltaObjects, objectCollection3, deletedInModified);
		deltaObjects = new ObjectCollection(deltaObjects.Except(changedObjectDeletions, ApplicationObject.EqualityComparer));
		ObjectCollection objectCollection4 = new ObjectCollection(objectCollection.Except(objectCollection3, ApplicationObject.EqualityComparer));
		ObjectCollection objectCollection5 = ApplyDeltaObjects(objectCollection3, deltaObjects);
		ObjectCollection conflictObjects = new ObjectCollection(objectCollection5.Where((ApplicationObject o) => o.ElementTypeInfo.ElementType == ElementType.Modification).Union(changedObjectDeletions));
		IEnumerable<ApplicationObject> mergedObjectsNoModifications = objectCollection5.Except(conflictObjects);
		IEnumerable<ApplicationObject> second2 = objectCollection4.Where((ApplicationObject o) => !deltaObjects.Modifies(o) || deletedInModified.Contains(o));
		foreach (ApplicationObject item in mergedObjectsNoModifications)
		{
			SetApplicationObjectProperties(item.ObjectProperties);
		}
		IEnumerable<ApplicationObject> enumerable = mergedObjectsNoModifications.Except(objectCollection3, ApplicationObject.EqualityComparer);
		IEnumerable<ApplicationObject> source2 = objectCollection3.Except(mergedObjectsNoModifications, ApplicationObject.EqualityComparer);
		IEnumerable<ApplicationObject> enumerable2 = (from o in mergedObjectsNoModifications.Except(enumerable, ApplicationObject.EqualityComparer)
			where !deltaObjects.Modifies(o)
			where !conflictObjects.Modifies(o)
			select o).Union(second2);
		ApplicationObjectFileInfoCollection resultObjectFiles = WriteApplicationObjects(mergedObjectsNoModifications, output, OutputType.MergeResult);
		ApplicationObjectFileInfoCollection conflictObjectFiles = WriteApplicationObjects(conflictObjects, text, OutputType.MergeResult);
		list.AddRange(source2.Select((ApplicationObject o) => new MergeInfo(MergeResult.Deleted, originalObjectFiles[o], modifiedObjectFiles[o], targetObjectFiles[o], null, null)));
		list.AddRange(HandleConflicts(text, originalObjectFiles, modifiedObjectFiles, targetObjectFiles, resultObjectFiles, conflictObjectFiles));
		list.AddRange(enumerable.Select((ApplicationObject ao) => new MergeInfo(MergeResult.Inserted, originalObjectFiles[ao], modifiedObjectFiles[ao], targetObjectFiles[ao], resultObjectFiles[ao], null)));
		list.AddRange(enumerable2.Select((ApplicationObject ao) => new MergeInfo(MergeResult.Unchanged, originalObjectFiles[ao], modifiedObjectFiles[ao], targetObjectFiles[ao], resultObjectFiles[ao], null)));
		IEnumerable<ApplicationObject> second3 = from c in conflictObjects
			select c.RootElement.GetStringProperty(PropertyType.Target) into k
			select mergedObjectsNoModifications.FirstOrDefault((ApplicationObject m) => m.RootElement.CreateDisplayElementKey() == k) into m
			where m != null
			select m;
		list.AddRange(from ao in mergedObjectsNoModifications.Except(second3, ApplicationObject.EqualityComparer).Except(enumerable, ApplicationObject.EqualityComparer).Except(enumerable2, ApplicationObject.EqualityComparer)
				.Except(objectCollection4, ApplicationObject.EqualityComparer)
			select new MergeInfo(MergeResult.Merged, originalObjectFiles[ao], modifiedObjectFiles[ao], targetObjectFiles[ao], resultObjectFiles[ao], null));
		string versionListPropertyName = EnumNameLookup<PropertyType>.EnumToString(PropertyType.VersionList);
		return new MergeInfoCollection(list, deltaObjects.GetElements().Count((IElement e) => e.ElementTypeInfo.ElementType == ElementType.Change && e.GetStringProperty(PropertyType.Property) != versionListPropertyName), conflictObjects.GetElements().Count((IElement e) => e.ElementTypeInfo.ElementType == ElementType.Change));
	}

	private IList<MergeInfo> HandleConflicts(string outputFolder, ApplicationObjectFileInfoCollection originalObjectFiles, ApplicationObjectFileInfoCollection modifiedObjectFiles, ApplicationObjectFileInfoCollection targetObjectFiles, ApplicationObjectFileInfoCollection resultObjectFiles, ApplicationObjectFileInfoCollection conflictObjectFiles)
	{
		string conflictOriginal = Directory.CreateDirectory(Path.Combine(outputFolder, NonLocalizable.ConflictOriginal)).FullName;
		string conflictTarget = Directory.CreateDirectory(Path.Combine(outputFolder, NonLocalizable.ConflictTarget)).FullName;
		string conflictModified = Directory.CreateDirectory(Path.Combine(outputFolder, NonLocalizable.ConflictModified)).FullName;
		ConcurrentBag<MergeInfo> mergeInfos = new ConcurrentBag<MergeInfo>();
		Logger.InvokeParallelProcessing(NonLocalizable.WriteFile, conflictObjectFiles.ObjectCollection.Count(), (IProgressLogger progress) =>
		{
			Parallel.ForEach(conflictObjectFiles.ObjectCollection, ParallelOptions, (ApplicationObject conflictObject) =>
			{
				string stringProperty = conflictObject.RootElement.GetStringProperty(PropertyType.Target);
				ApplicationObjectId id = ElementKeyHelper.GetApplicationObjectIdFromDisplayElementKey(stringProperty);
				ApplicationObject applicationObject = originalObjectFiles.ObjectCollection.FirstOrDefault((ApplicationObject o) => o.Id == id);
				ApplicationObjectFileInfo original = null;
				if (applicationObject != null)
				{
					string fileName = Path.Combine(Path.Combine(conflictOriginal, ReadWriteModelTool.CalcSaveFileName(applicationObject, OutputType.ApplicationObject)));
					original = originalObjectFiles[applicationObject];
					WriteApplicationObject(applicationObject, fileName, progress);
				}
				ApplicationObject applicationObject2 = targetObjectFiles.ObjectCollection.FirstOrDefault((ApplicationObject o) => o.Id == id);
				ApplicationObjectFileInfo target = null;
				if (applicationObject2 != null)
				{
					string fileName2 = Path.Combine(Path.Combine(conflictTarget, ReadWriteModelTool.CalcSaveFileName(applicationObject2, OutputType.ApplicationObject)));
					target = targetObjectFiles[applicationObject2];
					WriteApplicationObject(applicationObject2, fileName2, progress);
				}
				ApplicationObject applicationObject3 = modifiedObjectFiles.ObjectCollection.FirstOrDefault((ApplicationObject o) => o.Id == id);
				ApplicationObjectFileInfo modified = null;
				if (applicationObject3 != null)
				{
					string fileName3 = Path.Combine(Path.Combine(conflictModified, ReadWriteModelTool.CalcSaveFileName(applicationObject3, OutputType.ApplicationObject)));
					modified = modifiedObjectFiles[applicationObject3];
					WriteApplicationObject(applicationObject3, fileName3, progress);
				}
				ApplicationObject index = resultObjectFiles.ObjectCollection.FirstOrDefault((ApplicationObject o) => o.Id == id);
				mergeInfos.Add(new MergeInfo(MergeResult.Conflict, original, modified, target, resultObjectFiles[index], conflictObjectFiles[conflictObject]));
				progress.Increment();
			});
		});
		return mergeInfos.ToList();
	}

	private ObjectCollection GetChangedObjectDeletions(ObjectCollection originalObjects, ObjectCollection deltaObjects, ObjectCollection targetObjects, ObjectCollection deletedInModified)
	{
		IEnumerable<ApplicationObject> enumerable = from d in deltaObjects.GetApplicationObjects(ElementType.Modification)
			where d.RootElement.GetStringProperty(PropertyType.ChangeType) == ChangeTypes.Deletion.ToString()
			select d;
		ObjectCollection objectCollection = new ObjectCollection();
		ObjectCollection objectCollection2 = new ObjectCollection();
		foreach (ApplicationObject item in enumerable)
		{
			string targetKey = ElementKeyHelper.DisplayElementKeyToElementKey(item.RootElement.GetStringProperty(PropertyType.Target));
			ApplicationObject targetDeletedObject = targetObjects.FirstOrDefault((ApplicationObject o) => o.RootElement.ElementKey == targetKey);
			deletedInModified.Add(originalObjects.FirstOrDefault((ApplicationObject o) => o.RootElement.ElementKey == targetKey));
			if (targetDeletedObject != null)
			{
				objectCollection.Add(originalObjects.First((ApplicationObject o) => o.Id == targetDeletedObject.Id));
				objectCollection2.Add(targetDeletedObject);
			}
		}
		ObjectCollection objectCollection3 = new ObjectCollection();
		foreach (ApplicationObject item2 in (IEnumerable<ApplicationObject>)CreateDeltaObjects(objectCollection, objectCollection2))
		{
			string targetKey2 = item2.RootElement.GetStringProperty(PropertyType.Target);
			objectCollection3.Add(enumerable.First((ApplicationObject d) => d.RootElement.GetStringProperty(PropertyType.Target) == targetKey2));
		}
		return objectCollection3;
	}

	private ObjectCollection CreateDeltaObjects(ObjectCollection originalList, ObjectCollection modifiedList)
	{
		ObjectCollection deltaList = new ObjectCollection();
		var toCompare = (from o in originalList.GetApplicationObjects()
			join m in modifiedList.GetApplicationObjects() on o.Id equals m.Id
			select new
			{
				Original = o,
				Modified = m
			}).ToArray();
		Logger.InvokeParallelProcessing(NonLocalizable.CompareObjects, toCompare.Length, (IProgressLogger progress) =>
		{
			Parallel.ForEach(toCompare, ParallelOptions, c =>
			{
				try
				{
					IElement element = new ComputeChanges(c.Original.RootElement, options.GetComputeChangesOptions()).Compute(c.Modified.RootElement);
					if (element != null)
					{
						if (c.Modified.ObjectProperties.VersionList != c.Original.ObjectProperties.VersionList)
						{
							IElement element2 = GenericElement.Create(ElementType.Change);
							element2.SetStringProperty(PropertyType.Property, EnumNameLookup<PropertyType>.EnumToString(PropertyType.VersionList));
							element2.SetStringProperty(PropertyType.ChangeType, EnumNameLookup<ChangeTypes>.EnumToString(ChangeTypes.PropertyModification));
							element2.SetStringProperty(PropertyType.OriginalValue, c.Original.ObjectProperties.VersionList);
							element2.SetStringProperty(PropertyType.ModifiedValue, c.Modified.ObjectProperties.VersionList);
							element.AddSubElement(element2);
						}
						ApplicationObject applicationObject2 = new ApplicationObject(element, c.Modified.ObjectProperties)
						{
							ShouldEncode = (c.Original.ShouldEncode || c.Modified.ShouldEncode)
						};
						deltaList.Add(applicationObject2);
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLine(c.Modified.RootElement.ElementKey + " " + ex.Message, MessageType.Warning);
				}
				progress.Increment();
			});
		});
		foreach (ApplicationObject item in modifiedList.GetApplicationObjects().Except(originalList.GetApplicationObjects(), ApplicationObject.EqualityComparer))
		{
			deltaList.Add(item);
		}
		foreach (ApplicationObject item2 in originalList.GetApplicationObjects().Except(modifiedList.GetApplicationObjects(), ApplicationObject.EqualityComparer))
		{
			ApplicationObject applicationObject = new ApplicationObject(new ComputeChanges(item2.RootElement, options.GetComputeChangesOptions()).Compute(null));
			applicationObject.ShouldEncode = item2.ShouldEncode;
			deltaList.Add(applicationObject);
		}
		return deltaList;
	}

	private ObjectCollection ApplyDeltaObjects(ObjectCollection targetList, ObjectCollection deltaList)
	{
		string versionListName = EnumNameLookup<PropertyType>.EnumToString(PropertyType.VersionList);
		ObjectCollection resultList = new ObjectCollection();
		IEnumerable<ApplicationObject> applicationObjects = deltaList.GetApplicationObjects(ElementType.Modification);
		var toCompare = (from t in targetList.GetApplicationObjects()
			join d in applicationObjects on t.Id equals d.Id
			select new
			{
				Target = t,
				Delta = d
			}).ToArray();
		Logger.InvokeParallelProcessing(NonLocalizable.UpdateObjects, toCompare.Length, (IProgressLogger progress) =>
		{
			Parallel.ForEach(toCompare, ParallelOptions, c =>
			{
				try
				{
					IElement element = c.Delta.RootElement.GetSubElements(ElementType.Change).FirstOrDefault((IElement e) => e.GetStringProperty(PropertyType.Property) == versionListName);
					if (element != null)
					{
						c.Delta.RootElement.RemoveSubElement(element);
					}
					IElement element2 = new ApplyChanges(c.Target.RootElement, options.GetApplyChangesOptions()).Apply(c.Delta.RootElement, out var conflict2);
					if (element2 != null)
					{
						ApplicationObject applicationObject2 = new ApplicationObject(element2, MergeApplicationObjectProperties(c.Target.ObjectProperties, c.Delta.ObjectProperties))
						{
							ShouldEncode = (c.Target.ShouldEncode | c.Delta.ShouldEncode)
						};
						resultList.Add(applicationObject2);
						if (conflict2 != null)
						{
							resultList.Add(new ApplicationObject(conflict2));
						}
					}
				}
				catch (Exception ex)
				{
					Logger.WriteLine("Merge failed for:" + c.Target.RootElement.ElementKey + " " + ex.Message, MessageType.Warning);
				}
				progress.Increment();
			});
		});
		IEnumerable<ApplicationObject> enumerable = deltaList.GetApplicationObjects().Except(applicationObjects);
		IEnumerable<ApplicationObject> enumerable2 = targetList.GetApplicationObjects().Except(toCompare.Select(c => c.Target), ApplicationObject.EqualityComparer);
		foreach (var item in from target in enumerable
			join delta in enumerable2 on target.Id equals delta.Id
			select new { target, delta })
		{
			IElement rootElement = new ApplyChanges(item.target.RootElement, options.GetApplyChangesOptions()).Apply(item.delta.RootElement, out var conflict);
			if (conflict != null)
			{
				resultList.Add(new ApplicationObject(conflict));
			}
			ApplicationObject applicationObject = new ApplicationObject(rootElement);
			applicationObject.ShouldEncode = item.target.ShouldEncode | item.delta.ShouldEncode;
			resultList.Add(applicationObject);
		}
		enumerable2.Except(enumerable, ApplicationObject.EqualityComparer).ForEach((ApplicationObject ao) =>
		{
			resultList.Add(ao);
		});
		enumerable.Except(enumerable2, ApplicationObject.EqualityComparer).ForEach((ApplicationObject ao) =>
		{
			resultList.Add(ao);
		});
		(from m in applicationObjects.Except(toCompare.Select(c => c.Delta))
			where m.RootElement.GetStringProperty(PropertyType.ChangeType) != ChangeTypes.Deletion.ToString()
			select m).ForEach((ApplicationObject ao) =>
		{
			resultList.Add(ao);
		});
		return resultList;
	}

	private static void CheckObjects(ObjectCollection objects, InputType inputType, string errorMessage, string objectSource)
	{
		if (inputType == InputType.ApplicationObject && objects.Any((ApplicationObject o) => o.ElementTypeInfo.ElementType == ElementType.Modification))
		{
			throw new ModelToolException(errorMessage, objectSource);
		}
	}

	private ApplicationObjectProperties MergeApplicationObjectProperties(ApplicationObjectProperties target, ApplicationObjectProperties modified)
	{
		ApplicationObjectProperties applicationObjectProperties = new ApplicationObjectProperties();
		switch (options.DateTimeProperty)
		{
		case DateTimePropertyAction.FromModified:
			applicationObjectProperties.Date = modified.Date;
			applicationObjectProperties.Time = modified.Time;
			break;
		case DateTimePropertyAction.FromTarget:
			applicationObjectProperties.Date = target.Date;
			applicationObjectProperties.Time = target.Time;
			break;
		case DateTimePropertyAction.Now:
			applicationObjectProperties.Date = options.DateNowString;
			applicationObjectProperties.Time = options.TimeNowString;
			break;
		}
		switch (options.ModifiedProperty)
		{
		case ModifiedPropertyAction.FromModified:
			applicationObjectProperties.Modified = modified.Modified;
			break;
		case ModifiedPropertyAction.FromTarget:
			applicationObjectProperties.Modified = target.Modified;
			break;
		}
		switch (options.VersionListProperty)
		{
		case VersionListPropertyAction.FromModified:
			applicationObjectProperties.VersionList = modified.VersionList;
			break;
		case VersionListPropertyAction.FromTarget:
			applicationObjectProperties.VersionList = target.VersionList;
			break;
		}
		SetApplicationObjectProperties(applicationObjectProperties);
		return applicationObjectProperties;
	}

	private void SetApplicationObjectProperties(ApplicationObjectProperties properties)
	{
		switch (options.DateTimeProperty)
		{
		case DateTimePropertyAction.Now:
			properties.Date = options.DateNowString;
			properties.Time = options.TimeNowString;
			break;
		case DateTimePropertyAction.Clear:
			properties.Date = string.Empty;
			properties.Time = string.Empty;
			break;
		}
		switch (options.ModifiedProperty)
		{
		case ModifiedPropertyAction.Yes:
			properties.Modified = "Yes";
			break;
		case ModifiedPropertyAction.No:
			properties.Modified = string.Empty;
			break;
		}
		if (options.VersionListProperty == VersionListPropertyAction.Clear)
		{
			properties.VersionList = string.Empty;
		}
	}
}
