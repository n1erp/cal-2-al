using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class MetadataProvider
{
	public const string UniquePrefixForRemovedControlBaseDefinition = "β";

	public const string MasterPageHomepageClientStartup = "MasterPageHomepageClientStartup";

	public const string MasterNavigationClientStartup = "MasterNavigationClientStartup";

	public const string MenuSuiteObjectsRange = "1000..1999";

	private const string MasterNavigation = "MasterNavigation";

	private const string XMLSchemaUri = "http://www.w3.org/2001/XMLSchema-instance";

	private const string TableNameSeparator = "$";

	private const string QueryPageApplicationArea = "#All";

	internal static readonly Lazy<XmlSerializer> MasterPageSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(MasterPage)));

	internal static readonly Lazy<XmlSerializer> MasterNavigationSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(MasterNavigation)));

	internal static readonly Lazy<XmlSerializer> PageDefinitionSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(PageDefinition)));

	private long versionNumber;

	private bool trackingObjects;

	private static ElementRemovalOption elementRemovalOption = ServerUserSettings.Instance.UIElementRemovalOption.Value;

	private readonly INavV1PageConfiguration navV1PageConfigurationRepository = NavEnvironment.Instance.NavV1PageConfigurationRepository;

	private NavigationDataAggregator extensionNavigationDataAggregator = new NavigationDataAggregator(NavEnvironment.Instance.NavAppMetadataRetriever);

	private static readonly QueryCategoryCache queryCategoryCacheInstance = new QueryCategoryCache();

	private static readonly char[] splitChar = new char[1] { ',' };

	private ConcurrentDictionary<int, NCLObjectXmlMetadata> legacyEnumMetadataCache = new ConcurrentDictionary<int, NCLObjectXmlMetadata>();

	public long VersionNumber => versionNumber + (NavGlobal.AppDatabase?.ChangeListener?.MaxChangeTrackingNumberObjectMetadata).GetValueOrDefault();

	internal MetadataProvider()
	{
	}

	public static void Initialize()
	{
		XmlSerializer value = MasterPageSerializer.Value;
		XmlSerializer value2 = MasterNavigationSerializer.Value;
		_ = PageDefinitionSerializer.Value;
		if (value != null && value2 == null)
		{
		}
	}

	public void TrackObjects()
	{
		if (!trackingObjects)
		{
			NavGlobal.AppDatabase.NavAppObjectMetadataChanged += NavAppObjectMetadataChanged;
			NavGlobal.AppDatabase.NavAppChanged += NavAppChanged;
			trackingObjects = true;
		}
	}

	public void UpdateMetadataVersion()
	{
		Interlocked.Increment(ref versionNumber);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00008L0", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Updated Metadata version number: " + versionNumber);
	}

	private void NavAppObjectMetadataChanged(object sender, NavTrackingEventArgs eventArgs)
	{
		UpdateMetadataVersion();
	}

	private void NavAppChanged(object sender, NavTrackingEventArgs eventArgs)
	{
		extensionNavigationDataAggregator.InvalidateNavigationDataCache(eventArgs);
		UpdateQueryCategories(eventArgs);
	}

	public MetaTable GetTableMetadata(int id, int appGroupId = -1, bool requireCompiled = true)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaTableById(id, out var metaTable, appGroupId, requireCompiled))
		{
			throw new NavMetadataNotFoundException(ObjectType.Table, id);
		}
		MetaTable metaTableClone = metaTable.GetMetaTableClone();
		int appGroupId2 = NavCurrentThread.TryResolveAppGroup()?.GroupId ?? metaTable.MetadataAppGroup.GroupId;
		string language = NavCurrentThread.Session?.AbbreviatedLanguageName ?? "ENU";
		int languageId = NavCurrentThread.Session?.LCID ?? NavEnvironment.DefaultLanguage;
		metaTableClone.MergeMultiLanguageTexts(appGroupId2, includeFields: true);
		metaTableClone.ApplyLanguage(language, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(languageId)));
		TranslateCaptionClassOnMetaTableFields(metaTableClone, appGroupId, requireCompiled);
		metaTableClone.VersionNumber = VersionNumber;
		return metaTableClone;
	}

	private static void TranslateCaptionClassOnMetaTableFields(MetaTable tableMetadata, int appGroupId, bool requireCompiled)
	{
		if (NavCurrentThread.Session == null || !NavCurrentThread.Session.IsCompanyOpen || !NavCurrentThread.Session.HasExecutePermission(new ApplicationObjectId(ObjectType.Table, tableMetadata.Id)) || !tableMetadata.Fields.Any((MetaField f) => !string.IsNullOrEmpty(f.CaptionClass)))
		{
			return;
		}
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(tableMetadata.Id, appGroupId, requireCompiled);
		using NavRecord navRecord = metaTableById.CreateObjectInstance(NavCurrentThread.Session);
		foreach (MetaField item in tableMetadata.Fields.Where((MetaField f) => !string.IsNullOrEmpty(f.CaptionClass)))
		{
			item.Caption = navRecord.GetDynamicCaption(item.Id);
		}
	}

	public virtual MetaReport GetReportMetadata(int id)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Report, id, out var metaApplicationObject, -1, requireCompiled: false))
		{
			throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, ObjectType.Report, id, null);
		}
		return GetReportMetadata((NCLMetaReport)metaApplicationObject);
	}

	internal MetaReport GetReportMetadata(NCLMetaReport nclMetaReport)
	{
		int groupId = nclMetaReport.MetadataAppGroup.GroupId;
		int groupId2 = NavCurrentThread.ResolveAppGroup().GroupId;
		if (elementRemovalOption != ElementRemovalOption.None)
		{
			return new MetaReport(nclMetaReport.MetaReportXml.DocumentElement, CreateRequestPage, groupId, groupId2, (MasterPage masterPage) => RemoveItemsOnPageBasedOnLicenseAndApplicationArea(masterPage, null));
		}
		return new MetaReport(nclMetaReport.MetaReportXml.DocumentElement, CreateRequestPage, groupId, groupId2);
	}

	public virtual MetaQuery GetQueryMetadata(int id)
	{
		NCLMetaQuery nclMetaQuery;
		return GetQueryMetadata(id, out nclMetaQuery);
	}

	private MetaQuery GetQueryMetadata(int id, out NCLMetaQuery nclMetaQuery)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Query, id, out var metaApplicationObject, -1, requireCompiled: false))
		{
			throw new NavMetadataNotFoundException(ObjectType.Query, id);
		}
		nclMetaQuery = (NCLMetaQuery)metaApplicationObject;
		int groupId = nclMetaQuery.MetadataAppGroup.GroupId;
		int groupId2 = NavCurrentThread.ResolveAppGroup().GroupId;
		MetaQuery metaQuery = new MetaQuery(nclMetaQuery.MetaQueryXml.DocumentElement, groupId, groupId2);
		metaQuery.MetadataToken = VersionNumber;
		metaQuery.ResolveColumnTypes(NCLMetaQuery.FieldTypeFromTableNoResolver);
		if (NavCurrentThread.Session != null)
		{
			string abbreviatedLanguageName = NavCurrentThread.Session.AbbreviatedLanguageName;
			string abbreviatedLanguageName2 = LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID));
			metaQuery.ApplyLanguage(abbreviatedLanguageName, abbreviatedLanguageName2, NCLMetaQuery.FieldFromTableNoResolver);
		}
		return metaQuery;
	}

	public virtual MetaTable GetQueryTableMetadata(int queryId)
	{
		MetaQuery queryMetadata = GetQueryMetadata(queryId, out var nclMetaQuery);
		return NCLMetaTable.CreateFromQuery(nclMetaQuery, queryMetadata).GetMetaTableOriginal();
	}

	public virtual MetaXmlPort GetXmlPortMetadata(int id)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.XmlPort, id, out var metaApplicationObject, -1, requireCompiled: false))
		{
			throw new NavMetadataNotFoundException(ObjectType.XmlPort, id);
		}
		return GetXmlPortMetadata((NCLMetaXmlPort)metaApplicationObject);
	}

	public virtual MetaXmlPort GetXmlPortMetadata(NCLMetaXmlPort nclMetaXmlPort)
	{
		int groupId = nclMetaXmlPort.MetadataAppGroup.GroupId;
		int groupId2 = NavCurrentThread.ResolveAppGroup().GroupId;
		if (elementRemovalOption != ElementRemovalOption.None)
		{
			return new MetaXmlPort(nclMetaXmlPort.XmlMetadata, CreateXmlPage, groupId, groupId2, (MasterPage masterPage) => RemoveItemsOnPageBasedOnLicenseAndApplicationArea(masterPage, null));
		}
		return new MetaXmlPort(nclMetaXmlPort.XmlMetadata, CreateXmlPage, groupId, groupId2);
	}

	public virtual NCLOptionMetadata GetEnumMetadata(int id)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Enum, id, out var metaApplicationObject, -1, requireCompiled: false))
		{
			throw new NavMetadataNotFoundException(ObjectType.Enum, id);
		}
		NCLMetaEnum nCLMetaEnum = (NCLMetaEnum)metaApplicationObject;
		return nCLMetaEnum.GetNCLEnumMetadata();
	}

	internal void AddLegacyEnumMetadata(int enumId, string enumName, string optionString, string optionCaptions)
	{
		if (NavCurrentThread.ResolveAppGroup().GroupId != 0)
		{
			throw new NavNCLInvalidOperationException();
		}
		XElement xElement = new XElement("Enum");
		xElement.SetAttributeValue("ID", enumId.ToString(CultureInfo.InvariantCulture));
		xElement.SetAttributeValue("Name", enumName);
		xElement.SetAttributeValue("Extensible", "1");
		string[] array = optionString.Split(splitChar);
		string[] array2 = MultiLanguage.SplitMultiLanguageOptions(optionCaptions);
		if (array2.Length != array.Length)
		{
			array2 = null;
		}
		XElement xElement2 = new XElement("Values");
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]))
			{
				XElement xElement3 = new XElement("Value");
				xElement3.SetAttributeValue("Name", array[i]);
				xElement3.SetAttributeValue("Ordinal", i.ToString(CultureInfo.InvariantCulture));
				if (array2 != null)
				{
					XElement xElement4 = new XElement("CaptionML");
					xElement4.Value = array2[i];
					xElement3.Add(xElement4);
				}
				xElement2.Add(xElement3);
			}
		}
		xElement.Add(xElement2);
		XDocument xDocument = new XDocument();
		xDocument.Add(xElement);
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		xmlDocument.Load(XmlHelper.SafeXmlReader(new StringReader(xDocument.ToString())));
		NCLObjectXmlMetadata xmlMetadata = new NCLObjectXmlMetadata(xmlDocument, 0L, NavText.Empty);
		legacyEnumMetadataCache.AddOrUpdate(enumId, xmlMetadata, (int id, NCLObjectXmlMetadata old) => xmlMetadata);
	}

	internal NCLObjectXmlMetadata GetLegacyEnumMetadata(int enumId)
	{
		if (!legacyEnumMetadataCache.TryGetValue(enumId, out var value))
		{
			return null;
		}
		return value;
	}

	public MasterPage GetMasterPageWithoutConfiguration(int id, string personalizationId = null, bool applyPersonalization = false, bool applyDesignerChanges = false, string viewName = null)
	{
		PageDefinition pageDefinition = ((applyDesignerChanges && NavCurrentThread.Session.CurrentDesignerExtension != null) ? NavCurrentThread.Session.CurrentDesignerExtension.GetPageDefinition(id, viewName) : GetPageDefinition(id));
		pageDefinition.SetLanguage(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)));
		MasterPage masterPage = GetMasterPage(pageDefinition.Properties.PageType);
		MetaTable metaTable = null;
		Dictionary<int, MetaTable> dictionary = new Dictionary<int, MetaTable>();
		int num = NCLMetadataCacheEntry.ResolveGroupId(-1);
		MergeMasterPageWithPageDefinition(num, masterPage, pageDefinition);
		masterPage.LanguageAppGroupId = num;
		if (applyPersonalization)
		{
			masterPage = ApplyPageCustomizationsV1(masterPage, id, personalizationId, applyPersonalization);
		}
		SetPageHelpBaseLink(masterPage);
		if (pageDefinition.Properties.PageType != PageType.ReportPreview)
		{
			masterPage.PageProperties.HelpRelativeUrl = GetRelativeHelpUrl(id);
		}
		if (masterPage.PageProperties.SourceObject.SourceTable > 0)
		{
			metaTable = GetTableMetadata(masterPage.PageProperties.SourceObject.SourceTable);
		}
		LoadExpressionRelationTables(masterPage, dictionary);
		PropertiesSolveHelper.SolvePropertiesDefaulting(masterPage, metaTable, dictionary);
		ControlAddInMetadataProvider.SolveControlAddInProperties(masterPage);
		return masterPage;
	}

	private static void MergeMasterPageWithPageDefinition(int appGroupId, MasterPage masterPage, PageDefinition pageDefinition)
	{
		pageDefinition = UpdatePageDefinitionWithQueryActions(appGroupId, pageDefinition);
		MasterPageMergeHelper.MergeMasterPageAndPage(masterPage, pageDefinition);
	}

	internal static PageDefinition UpdatePageDefinitionWithQueryActions(int appGroupId, PageDefinition pageDefinition)
	{
		if (FeatureSwitches.Instance.DisableSmartListActions)
		{
			return pageDefinition;
		}
		string queryCategory = pageDefinition.Properties.QueryCategory;
		if (string.IsNullOrEmpty(queryCategory))
		{
			return pageDefinition;
		}
		IList<MetaQuery> list = queryCategoryCacheInstance.QueryList(appGroupId, queryCategory)?.OrderBy((MetaQuery c) => c.CaptionML).ToList();
		if (list.Count == 0)
		{
			return pageDefinition;
		}
		ActionContainerDefinition actionContainerDefinition = BuildActionContainerDefinition(2000100000, pageDefinition.Properties.ControlGUID, list);
		if (actionContainerDefinition != null && actionContainerDefinition.Actions.Count > 0)
		{
			pageDefinition.ActionContainers.Add(actionContainerDefinition);
		}
		return pageDefinition;
	}

	internal static void UpdateQueryCategories(NavTrackingEventArgs eventArgs)
	{
		if (FeatureSwitches.Instance.DisableSmartListActions)
		{
			return;
		}
		int num = NCLMetadataCacheEntry.ResolveGroupId(-1);
		List<int> list = null;
		List<int> list2 = null;
		List<int> list3 = null;
		foreach (NavTrackingObject item in eventArgs.TrackingObjects.Where((NavTrackingObject x) => x.ApplicationObjectId.ObjectType == ObjectType.Query))
		{
			List<int> list4 = null;
			switch (item.ChangeOperation)
			{
			case ObjectTrackingChangeOperation.Delete:
				list4 = list ?? (list = new List<int>());
				break;
			case ObjectTrackingChangeOperation.Insert:
				list4 = list3 ?? (list3 = new List<int>());
				break;
			case ObjectTrackingChangeOperation.Update:
				list4 = list2 ?? (list2 = new List<int>());
				break;
			}
			list4?.Add(item.ObjectId);
		}
		if (list != null)
		{
			queryCategoryCacheInstance.Remove(num, list);
		}
		if (list2 != null)
		{
			queryCategoryCacheInstance.Update(num, list2);
		}
		if (list3 != null)
		{
			queryCategoryCacheInstance.Add(num, list3);
		}
	}

	private static ActionContainerDefinition BuildActionContainerDefinition(int startId, string controlGuid, IList<MetaQuery> queryList)
	{
		ActionContainerDefinition actionContainerDefinition = new ActionContainerDefinition();
		actionContainerDefinition.ActionContainerType = ActionContainerType.AutoQueryActions;
		actionContainerDefinition.ID = startId++;
		foreach (MetaQuery query in queryList)
		{
			ActionDefinition actionDefinition = new ActionDefinition();
			actionDefinition.ID = startId++;
			actionDefinition.Name = query.Caption;
			actionDefinition.ControlGUID = controlGuid;
			actionDefinition.RunPageMode = RunPageMode.View;
			actionDefinition.RunObjectType = RunObjectType.Query;
			actionDefinition.PushAction = PushActionType.RunObject;
			actionDefinition.TargetID = query.Id;
			actionDefinition.ApplicationArea = "#All";
			actionDefinition.Actions = new List<ActionBaseDefinition>();
			if (actionContainerDefinition.Actions == null)
			{
				actionContainerDefinition.Actions = new List<ActionBaseDefinition>();
			}
			actionContainerDefinition.Actions.Add(actionDefinition);
		}
		return actionContainerDefinition;
	}

	private static string GetRelativeHelpUrl(int pageId)
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000198 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000198);
			navRecord.ALSetFilter(1, "%1", NavInteger.Create(pageId));
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				return navRecord.GetFieldValueSafe(2, NavType.Text).ToString();
			}
		}
		return string.Empty;
	}

	private void LoadExpressionRelationTables(MasterPage masterPage, Dictionary<int, MetaTable> expressionTables)
	{
		if (masterPage == null)
		{
			return;
		}
		foreach (DataFieldDefinition expression in masterPage.Expressions)
		{
			if (expression.TableNo != 0 && expression.FieldNo != 0)
			{
				MetaTable tableMetadata = GetTableMetadata(expression.TableNo);
				if (tableMetadata != null && !expressionTables.ContainsKey(tableMetadata.Id))
				{
					expressionTables.Add(tableMetadata.Id, tableMetadata);
				}
			}
		}
	}

	private static ObjectType GetObjectTypeFromRunObject(AccessByObjectType runObjectType)
	{
		return runObjectType switch
		{
			AccessByObjectType.Page => ObjectType.Page, 
			AccessByObjectType.Report => ObjectType.Report, 
			AccessByObjectType.Codeunit => ObjectType.CodeUnit, 
			AccessByObjectType.XMLport => ObjectType.XmlPort, 
			AccessByObjectType.TableData => ObjectType.TableData, 
			AccessByObjectType.Table => ObjectType.Table, 
			AccessByObjectType.Query => ObjectType.Query, 
			AccessByObjectType.System => ObjectType.System, 
			_ => ObjectType.LimitedUsageTableData, 
		};
	}

	private static AccessByObjectType ConvertRunObjectTypeToObjectType(RunObjectType runObjectType)
	{
		return runObjectType switch
		{
			RunObjectType.None => AccessByObjectType.None, 
			RunObjectType.Page => AccessByObjectType.Page, 
			RunObjectType.Report => AccessByObjectType.Report, 
			RunObjectType.Codeunit => AccessByObjectType.Codeunit, 
			RunObjectType.XMLport => AccessByObjectType.XMLport, 
			RunObjectType.Query => AccessByObjectType.Query, 
			_ => AccessByObjectType.None, 
		};
	}

	private static bool IsRunObjectAllowedFromPermissions(ActionDefinition action)
	{
		PermissionMask licenseAndUserPermissionMaskForObject = GetLicenseAndUserPermissionMaskForObject(ConvertRunObjectTypeToObjectType(action.RunObjectType), action.TargetID);
		if (HaveRightPermissions(licenseAndUserPermissionMaskForObject, PermissionMask.Execute))
		{
			if (action.RunObjectSrcTable != 0 && !NavRecord.IsTableVirtual(action.RunObjectSrcTable))
			{
				PermissionMask licenseAndUserPermissionMaskForObject2 = GetLicenseAndUserPermissionMaskForObject(ObjectType.TableData, action.RunObjectSrcTable);
				PermissionMask licenseAndUserPermissionMaskForObject3 = GetLicenseAndUserPermissionMaskForObject(ObjectType.Table, action.RunObjectSrcTable);
				if (!HaveRightPermissions(licenseAndUserPermissionMaskForObject2, PermissionMask.Read | PermissionMask.IndirectRead) || !HaveRightPermissions(licenseAndUserPermissionMaskForObject3, PermissionMask.Execute))
				{
					return false;
				}
				PermissionMask licenseFilePermissionMask = GetLicenseFilePermissionMask(ObjectType.TableData, action.RunObjectSrcTable);
				if (!HaveRightPermissions(licenseFilePermissionMask, PermissionMask.Read | PermissionMask.IndirectRead))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private static bool IsRelatedObjectPermissionsAllowedFromLicense(AccessByObjectType objectType, int objectId, PermissionMask permissionMask)
	{
		PermissionMask licenseAndUserPermissionMaskForObject = GetLicenseAndUserPermissionMaskForObject(objectType, objectId);
		return HaveRightPermissions(licenseAndUserPermissionMaskForObject, permissionMask);
	}

	private static bool HaveRightPermissions(PermissionMask objectPermission, PermissionMask requiredPermission)
	{
		return (objectPermission & requiredPermission) != 0;
	}

	private static PermissionMask GetLicenseFilePermissionMask(ObjectType objectType, int objectId)
	{
		NavSession session = NavCurrentThread.Session;
		return session.License.GetLicensePermissionMask(objectType, objectId);
	}

	private static PermissionMask GetLicenseFilePermissionMask(AccessByObjectType relatedObjectType, int objectId)
	{
		ObjectType objectTypeFromRunObject = GetObjectTypeFromRunObject(relatedObjectType);
		return GetLicenseFilePermissionMask(objectTypeFromRunObject, objectId);
	}

	private static PermissionMask GetLicenseAndUserPermissionMaskForObject(AccessByObjectType relatedObjectType, int objectId)
	{
		ObjectType objectTypeFromRunObject = GetObjectTypeFromRunObject(relatedObjectType);
		return GetLicenseAndUserPermissionMaskForObject(objectTypeFromRunObject, objectId);
	}

	private static bool IsPermissionEngineActivated()
	{
		return NavCurrentThread.Session.PermissionSystemActivated;
	}

	private static PermissionMask GetLicenseAndUserPermissionMaskForObject(ObjectType objectType, int objectId)
	{
		NavSession session = NavCurrentThread.Session;
		if (elementRemovalOption == ElementRemovalOption.LicenseFileAndUserPermissions && IsPermissionEngineActivated())
		{
			PermissionManager permissionManager = session.Permissions.GetPermissionManager(session.Company.CompanyNameToken);
			return permissionManager.GetPermissions(new ApplicationObjectId(objectType, objectId));
		}
		return GetLicenseFilePermissionMask(objectType, objectId);
	}

	private static bool IsRelatedObjectVirtualTable(RelatedObjectDefinition relatedObject)
	{
		if (relatedObject.ObjectType == AccessByObjectType.TableData || relatedObject.ObjectType == AccessByObjectType.Table)
		{
			return NavRecord.IsTableVirtual(relatedObject.ObjectID);
		}
		return false;
	}

	private static bool IsTableRelationAllowedFromPermissions(ControlDefinition controlDefinition)
	{
		bool flag = false;
		foreach (RelatedObjectDefinition relatedObject in controlDefinition.RelatedObjects)
		{
			if (relatedObject.RelatedBy == RelationType.TableRelation)
			{
				flag = true;
				if (IsRelatedObjectVirtualTable(relatedObject))
				{
					return true;
				}
				PermissionMask licenseAndUserPermissionMaskForObject = GetLicenseAndUserPermissionMaskForObject(relatedObject.ObjectType, relatedObject.ObjectID);
				if (HaveRightPermissions(licenseAndUserPermissionMaskForObject, (PermissionMask)relatedObject.PermissionMask))
				{
					return true;
				}
			}
		}
		return !flag;
	}

	private static bool IsFieldRelatedObjectPermissionsAllowedFromPermissions(ControlDefinition controlDefinition)
	{
		foreach (RelatedObjectDefinition relatedObject in controlDefinition.RelatedObjects)
		{
			if (relatedObject.RelatedBy != RelationType.TableRelation && !IsRelatedObjectVirtualTable(relatedObject))
			{
				PermissionMask licenseAndUserPermissionMaskForObject = GetLicenseAndUserPermissionMaskForObject(relatedObject.ObjectType, relatedObject.ObjectID);
				if (!HaveRightPermissions(licenseAndUserPermissionMaskForObject, (PermissionMask)relatedObject.PermissionMask))
				{
					return false;
				}
				PermissionMask licenseFilePermissionMask = GetLicenseFilePermissionMask(relatedObject.ObjectType, relatedObject.ObjectID);
				if (relatedObject.RelatedBy == RelationType.CalcFormula && !HaveRightPermissions(licenseFilePermissionMask, PermissionMask.Insert | PermissionMask.IndirectInsert))
				{
					return false;
				}
			}
		}
		return IsTableRelationAllowedFromPermissions(controlDefinition);
	}

	public static bool IsTableFieldAccessibleBasedOnLicenseAndUserPermissions(MetaField field)
	{
		PermissionMask licenseAndUserPermissionMaskForObject = GetLicenseAndUserPermissionMaskForObject(field.AccessByObjectType, field.AccessByObjectID);
		return HaveRightPermissions(licenseAndUserPermissionMaskForObject, (PermissionMask)field.AccessByPermissionMask);
	}

	private static bool IsPageFieldTableRelationAllowedFromPermission(ControlDefinition control)
	{
		bool flag = false;
		foreach (TableRelationDefinition tableRelation in control.TableRelations)
		{
			flag = true;
			if (NavRecord.IsTableVirtual(tableRelation.TableID))
			{
				return true;
			}
			PermissionMask licenseAndUserPermissionMaskForObject = GetLicenseAndUserPermissionMaskForObject(ObjectType.TableData, tableRelation.TableID);
			if (HaveRightPermissions(licenseAndUserPermissionMaskForObject, PermissionMask.Read | PermissionMask.IndirectRead))
			{
				return true;
			}
		}
		return !flag;
	}

	private static bool RemoveAction(ElementDefinition element, IList<MasterPage> relatedMasterPages, bool applyApplicationArea)
	{
		if (element is ActionDefinition actionDefinition)
		{
			if (applyApplicationArea && !IsApplicationAreaEnabled(actionDefinition.ApplicationArea, actionDefinition.ResourceIdentifier))
			{
				return true;
			}
			if (actionDefinition.RunObjectType != RunObjectType.None && actionDefinition.TargetID != 0 && !IsRunObjectAllowedFromPermissions(actionDefinition))
			{
				return true;
			}
			if (actionDefinition.AccessByObjectType != AccessByObjectType.None && actionDefinition.AccessByObjectID != 0 && !IsRelatedObjectPermissionsAllowedFromLicense(actionDefinition.AccessByObjectType, actionDefinition.AccessByObjectID, (PermissionMask)actionDefinition.AccessByPermissionMask))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsApplicationAreaEnabled(string applicationArea, string resourceIdentifier)
	{
		if (!string.IsNullOrEmpty(resourceIdentifier))
		{
			switch (resourceIdentifier)
			{
			default:
				return true;
			case "MenuItemNotes":
			case "MenuItemLinks":
				break;
			}
		}
		return NavCurrentThread.Session.IsApplicationAreaEnabled(applicationArea);
	}

	private bool RemoveControl(ElementDefinition element, IList<MasterPage> relatedMasterPages, bool applyApplicationArea)
	{
		if (element is ControlDefinition controlDefinition)
		{
			if (applyApplicationArea && !IsApplicationAreaEnabled(controlDefinition.ApplicationArea, controlDefinition.ResourceIdentifier))
			{
				return true;
			}
			if (!IsPageFieldTableRelationAllowedFromPermission(controlDefinition))
			{
				return true;
			}
			if (controlDefinition.RelatedObjects.Any() && !IsFieldRelatedObjectPermissionsAllowedFromPermissions(controlDefinition))
			{
				return true;
			}
		}
		if (element is InfopartPageDefinition infopartPageDefinition && !PropertyHelper.PropertyIsFalse(infopartPageDefinition.Visible))
		{
			if (applyApplicationArea && !IsApplicationAreaEnabled(infopartPageDefinition.ApplicationArea, infopartPageDefinition.ResourceIdentifier))
			{
				return true;
			}
			if (infopartPageDefinition.AccessByObjectType != AccessByObjectType.None && infopartPageDefinition.AccessByObjectID != 0 && !IsRelatedObjectPermissionsAllowedFromLicense(infopartPageDefinition.AccessByObjectType, infopartPageDefinition.AccessByObjectID, (PermissionMask)infopartPageDefinition.AccessByPermissionMask))
			{
				return true;
			}
			PermissionMask licenseAndUserPermissionMaskForObject = GetLicenseAndUserPermissionMaskForObject(ObjectType.Page, infopartPageDefinition.PagePartID);
			if (HaveRightPermissions(licenseAndUserPermissionMaskForObject, PermissionMask.Execute))
			{
				if (infopartPageDefinition.RunObjectSrcTable != 0)
				{
					PermissionMask licenseAndUserPermissionMaskForObject2 = GetLicenseAndUserPermissionMaskForObject(ObjectType.TableData, infopartPageDefinition.RunObjectSrcTable);
					PermissionMask licenseAndUserPermissionMaskForObject3 = GetLicenseAndUserPermissionMaskForObject(ObjectType.Table, infopartPageDefinition.RunObjectSrcTable);
					if ((!HaveRightPermissions(licenseAndUserPermissionMaskForObject2, PermissionMask.Read) || !HaveRightPermissions(licenseAndUserPermissionMaskForObject3, PermissionMask.Execute)) && !IsSourceTableTemporary(infopartPageDefinition.PagePartID, relatedMasterPages))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
		if (element is InfopartSystemDefinition infopartSystemDefinition && applyApplicationArea && !IsApplicationAreaEnabled(infopartSystemDefinition.ApplicationArea, infopartSystemDefinition.ResourceIdentifier))
		{
			return true;
		}
		return false;
	}

	private bool IsSourceTableTemporary(int pagePartId, IList<MasterPage> relatedMasterPages)
	{
		MasterPage masterPage = ((relatedMasterPages == null) ? GetMasterPage(pagePartId, DataSourceType.Table, string.Empty, applyPersonalization: false) : relatedMasterPages.FirstOrDefault((MasterPage mp) => mp.ID == pagePartId));
		if (masterPage == null)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.MasterPageForPartNotFound, pagePartId));
		}
		return masterPage.PageProperties.SourceObject.SourceTableTemporary;
	}

	public MasterPage GetTableMasterPage(int tableId, string personalizationId)
	{
		MasterPage tableMasterPageUnsolved = GetTableMasterPageUnsolved(tableId);
		MasterPage masterPage = MergePageAndTable(tableMasterPageUnsolved);
		if (!string.IsNullOrEmpty(personalizationId))
		{
			NavSession session = NavCurrentThread.Session;
			NavXmlDelta personalizationDelta = GetPersonalizationDelta(9599, personalizationId, session, applyPersonalization: true);
			if (personalizationDelta != null)
			{
				XmlDocument xmlDocument = SerializeMasterPage(masterPage);
				NavXmlDelta.Apply(xmlDocument, personalizationDelta);
				try
				{
					masterPage = DeserializeMasterPage(xmlDocument);
					masterPage.PageProperties.PersonalizationTypeApplied = CustomizationType.Legacy;
				}
				catch (InvalidOperationException innerException)
				{
					LogWriterHelper.LogExceptionEvent(new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.PersonalizationPageError, 9599), innerException));
					throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, 9599), innerException);
				}
			}
		}
		masterPage.VersionNumber = VersionNumber;
		SolveDefaultFilterColumnProperty(masterPage);
		return masterPage;
	}

	public MasterPage GetQueryMasterPage(int queryId)
	{
		return GetMergedQueryMasterPage(queryId);
	}

	public MasterPage GetMasterPage(int id, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization)
	{
		return GetMasterPage(id, dataSourceType, personalizationId, applyPersonalization, forceElementRemovalOptionIfApplicable: true);
	}

	private MasterPage GetMasterPage(int id, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, bool forceElementRemovalOptionIfApplicable, string viewName = null)
	{
		MasterPage masterPage;
		if (NavCurrentThread.Session.CurrentDesignerExtension != null && dataSourceType != DataSourceType.Query)
		{
			applyPersonalization &= NavCurrentThread.Session.CurrentDesignerLevel == DesignerLevels.Personalization;
			masterPage = GetMasterPageForDesigner(id, personalizationId, applyPersonalization, viewName);
			if (NavCurrentThread.Session.CurrentDesignerExtension is NavDevelopmentDesignerExtension)
			{
				return masterPage;
			}
		}
		else
		{
			masterPage = GetMasterPageUnsolved(id, dataSourceType, personalizationId, applyPersonalization, viewName);
			if (dataSourceType == DataSourceType.Table)
			{
				masterPage = MergePageAndTable(masterPage);
			}
		}
		if (dataSourceType != DataSourceType.Table)
		{
			return masterPage;
		}
		if (elementRemovalOption != ElementRemovalOption.None && (forceElementRemovalOptionIfApplicable || masterPage.FindAll((ElementDefinition element) => element is InfopartPageDefinition).Count == 0))
		{
			masterPage = RemoveItemsOnPageBasedOnLicenseAndApplicationArea(masterPage, null);
		}
		return masterPage;
	}

	public MasterPage GetMasterPageForDesigner(int pageId, string personalizationId = null, bool applyPersonalization = false, string viewName = null)
	{
		MasterPage masterPageWithoutConfiguration = GetMasterPageWithoutConfiguration(pageId, personalizationId, applyPersonalization, applyDesignerChanges: true, viewName);
		if (masterPageWithoutConfiguration != null)
		{
			masterPageWithoutConfiguration.VersionNumber = VersionNumber;
		}
		return masterPageWithoutConfiguration;
	}

	private MasterPage MergePageAndTable(MasterPage masterPage)
	{
		MetaTable metaTable = null;
		Dictionary<int, MetaTable> dictionary = new Dictionary<int, MetaTable>();
		if (masterPage != null)
		{
			if (masterPage.PageProperties.SourceObject.SourceTable > 0)
			{
				metaTable = GetTableMetadata(masterPage.PageProperties.SourceObject.SourceTable);
				SolveDefaultFilterColumnProperty(masterPage);
			}
			LoadExpressionRelationTables(masterPage, dictionary);
		}
		PropertiesSolveHelper.SolvePropertiesDefaulting(masterPage, metaTable, dictionary);
		return masterPage;
	}

	public MasterPage GetTableMasterPageUnsolved(int id)
	{
		MasterPage mergedTableMasterPage = GetMergedTableMasterPage(id);
		ControlAddInMetadataProvider.SolveControlAddInProperties(mergedTableMasterPage);
		mergedTableMasterPage.VersionNumber = VersionNumber;
		return mergedTableMasterPage;
	}

	private static IList<ControlBaseDefinition> GetControl<TDefinition>(TDefinition item)
	{
		if (item is ControlGroupBaseDefinition controlGroupBaseDefinition)
		{
			return controlGroupBaseDefinition.Controls;
		}
		if (item is ControlContainerPlaceHolder controlContainerPlaceHolder)
		{
			return controlContainerPlaceHolder.Controls;
		}
		return new List<ControlBaseDefinition>(0);
	}

	private static IList<ControlBaseDefinition> GetPart<TDefinition>(TDefinition item)
	{
		if (item is ControlContainerPlaceHolder controlContainerPlaceHolder)
		{
			return controlContainerPlaceHolder.Controls;
		}
		return new List<ControlBaseDefinition>(0);
	}

	internal static void RemoveInList<TDefinition>(bool isDepartmentPage, List<string> removedControls, IList<TDefinition> collection, Func<TDefinition, IList<MasterPage>, bool, bool> predicate, Func<TDefinition, IList<TDefinition>> subnodeSelector, IList<MasterPage> relatedMasterPages, bool applyApplicationArea) where TDefinition : ElementDefinition
	{
		for (int num = collection.Count() - 1; num >= 0; num--)
		{
			TDefinition val = collection[num];
			RemoveInList(isDepartmentPage, removedControls, subnodeSelector(val), predicate, subnodeSelector, relatedMasterPages, applyApplicationArea);
			if (predicate(val, relatedMasterPages, applyApplicationArea))
			{
				if (val is ControlDefinition controlDefinition && !string.IsNullOrEmpty(controlDefinition.DataColumnName))
				{
					removedControls.Add(controlDefinition.DataColumnName);
				}
				else if (val is ControlBaseDefinition controlBaseDefinition && !string.IsNullOrEmpty(controlBaseDefinition.Name))
				{
					removedControls.Add("β" + controlBaseDefinition.Name);
				}
				if (val is UIElementDefinition uIElementDefinition)
				{
					removedControls.Add(uIElementDefinition.ControlGUID);
				}
				collection.Remove(val);
			}
			if (val is ControlGroupBaseDefinition controlGroupBaseDefinition && !controlGroupBaseDefinition.Controls.Any() && controlGroupBaseDefinition.Actions.Count == 0 && string.IsNullOrEmpty(controlGroupBaseDefinition.InstructionalText))
			{
				collection.Remove(val);
			}
			if (isDepartmentPage)
			{
				if (val is ActionContainerDefinition actionContainerDefinition && !actionContainerDefinition.Actions.Any())
				{
					collection.Remove(val);
				}
				if (val is ActionGroupDefinition actionGroupDefinition && !actionGroupDefinition.Actions.Any())
				{
					collection.Remove(val);
				}
			}
		}
	}

	internal static void ApplyRecursively<TDefinition>(IList<TDefinition> collection, Func<TDefinition, bool> predicate, Func<TDefinition, IList<TDefinition>> subnodeSelector, Action<TDefinition> action) where TDefinition : UIElementDefinition
	{
		for (int num = collection.Count() - 1; num >= 0; num--)
		{
			TDefinition val = collection[num];
			ApplyRecursively(subnodeSelector(val), predicate, subnodeSelector, action);
			if (predicate(val))
			{
				action(val);
			}
		}
	}

	public MasterPage GetMasterPageUnsolved(int id, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, string viewName = null)
	{
		MasterPage mergedMasterPage = GetMergedMasterPage(id, dataSourceType, applyPersonalization, viewName);
		ControlAddInMetadataProvider.SolveControlAddInProperties(mergedMasterPage);
		mergedMasterPage = ApplyPageCustomizationsV1(mergedMasterPage, id, personalizationId, applyPersonalization);
		mergedMasterPage.VersionNumber = VersionNumber;
		return mergedMasterPage;
	}

	private MasterPage ApplyPageCustomizationsV1(MasterPage masterPage, int id, string personalizationId, bool applyPersonalization)
	{
		NavSession session = NavCurrentThread.Session;
		if (masterPage.PageProperties.PageType == PageType.RoleCenter && id == GetHomepageID())
		{
			personalizationId = "";
		}
		NavXmlDelta navXmlDelta = ((masterPage.PageProperties.ConfigurationTypeApplied == CustomizationType.None) ? GetConfigurationDelta(id, personalizationId, session, applyPersonalization) : null);
		NavXmlDelta navXmlDelta2 = ((masterPage.PageProperties.PersonalizationTypeApplied == CustomizationType.None) ? GetPersonalizationDelta(id, personalizationId, session, applyPersonalization) : null);
		if (navXmlDelta != null || navXmlDelta2 != null)
		{
			bool deltaApplied = false;
			bool deltaApplied2 = false;
			CommandBarDefinition commandBarForOriginatingActions = (CommandBarDefinition)masterPage.CommandBar.Clone();
			XmlDocument xmlDocument = SerializeMasterPage(masterPage);
			if (navXmlDelta != null)
			{
				xmlDocument = ApplyDeltaAndCheckDuplicates(id, personalizationId, xmlDocument, navXmlDelta, session, isConfigurationDelta: true, out deltaApplied);
			}
			if (navXmlDelta2 != null)
			{
				xmlDocument = ApplyDeltaAndCheckDuplicates(id, personalizationId, xmlDocument, navXmlDelta2, session, isConfigurationDelta: false, out deltaApplied2);
			}
			try
			{
				masterPage = DeserializeMasterPage(xmlDocument);
			}
			catch (InvalidOperationException innerException)
			{
				LogWriterHelper.LogExceptionEvent(new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.PersonalizationPageError, id), innerException));
				throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, id), innerException);
			}
			masterPage = RemoveOrphanedAndUpdateClonedActions(masterPage, commandBarForOriginatingActions);
			MasterPageMergeHelper.SetCommandBarLanguage(NavCurrentThread.Session.Culture, masterPage.CommandBar);
			if (deltaApplied)
			{
				masterPage.PageProperties.ConfigurationTypeApplied = CustomizationType.Legacy;
			}
			if (deltaApplied2)
			{
				masterPage.PageProperties.PersonalizationTypeApplied = CustomizationType.Legacy;
			}
		}
		return masterPage;
	}

	private XmlDocument ApplyDeltaAndCheckDuplicates(int pageId, string personalizationId, XmlDocument document, NavXmlDelta delta, NavSession session, bool isConfigurationDelta, out bool deltaApplied)
	{
		XmlDocument xmlDocument = (XmlDocument)document.CloneNode(deep: true);
		NavXmlDelta.Apply(document, delta);
		List<XElement> list = FindDeltaDuplicateIds(document);
		if (list.Any())
		{
			if (isConfigurationDelta)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Duplicate items were detected in the configuration for page {0} in profile {1} personalizationid {2}. An administrator of the profile must redo any previous configuration changes to the page layout.", pageId, session.ProfileKey.ProfileId, personalizationId) + ComposeDuplicatesErrorMessage(list, document.OuterXml).EscapeCurlyBraces();
				session.Diagnostics.SendTraceTag("0000113", Category.UIChanges, Verbosity.Warning, PrivacyClassification.SystemMetadata, message);
				EventLogWriter.Write(message, EventLogEntryType.Warning, null, 0, null, 0);
			}
			else
			{
				session.Diagnostics.SendTraceTag("00001VN", Category.UIChanges, Verbosity.Warning, "Duplicate items were detected in the personalization for page {0} in profile {1} personalizationid {2} for user {3}. The personalization entry has been deleted, and the user will have to redo any previous changes made to the page layout." + ComposeDuplicatesErrorMessage(list, document.OuterXml).EscapeCurlyBraces(), DiagnosticParameter.SystemMetadata(pageId), DiagnosticParameter.SystemMetadata(session.ProfileKey.ProfileId), DiagnosticParameter.SystemMetadata(personalizationId), DiagnosticParameter.Euii(session.UserDisplayName));
				string message2 = string.Format(CultureInfo.InvariantCulture, "Duplicate items were detected in the personalization for page {0} in profile {1} personalizationid {2} for user {3}. The personalization entry has been deleted, and the user will have to redo any previous changes made to the page layout.", pageId, session.ProfileKey.ProfileId, personalizationId, session.UserDisplayName) + ComposeDuplicatesErrorMessage(list, document.OuterXml);
				EventLogWriter.Write(message2, EventLogEntryType.Warning, null, 0, null, 0);
			}
			document = xmlDocument;
			if (!isConfigurationDelta)
			{
				navV1PageConfigurationRepository.DeletePersonalization(pageId, personalizationId);
			}
			deltaApplied = false;
		}
		else
		{
			deltaApplied = true;
		}
		return document;
	}

	private static string ComposeDuplicatesErrorMessage(IEnumerable<XElement> duplicates, string deltaAsXmlString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<string> wantedAttributes = new List<string> { "ID", "Name", "type" };
		foreach (XElement duplicate in duplicates)
		{
			stringBuilder.AppendLine("Duplicate item - Start");
			foreach (XAttribute item in from a in duplicate.Attributes()
				where wantedAttributes.Contains(a.Name.LocalName)
				select a)
			{
				stringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, "Property:{0} - Value:{1}", item.Name.LocalName, item.Value));
			}
			stringBuilder.AppendLine("Duplicate item - End");
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine("Full xml document with conflicting ids - Start");
		stringBuilder.Append(deltaAsXmlString);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Full xml document with conflicting ids - End");
		return stringBuilder.ToString();
	}

	private static List<XElement> FindDeltaDuplicateIds(XmlDocument document)
	{
		XDocument xDocument = XDocument.Parse(document.OuterXml);
		return (from idElement in xDocument.Elements().Descendants()
			where CheckDuplicateElementType(idElement)
			group idElement by idElement.Attribute("ID").Value into @group
			where @group.Count() > 1
			select @group).SelectMany((IGrouping<string, XElement> duplicateElement) => duplicateElement).ToList();
	}

	private static bool CheckDuplicateElementType(XElement element)
	{
		if (element.Attribute("ID") != null && element.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance"))?.Value != "ActionGroupDefinition" && element.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance"))?.Value != "NavigationGroupDefinition")
		{
			return int.Parse(element.Attribute("ID")?.Value, CultureInfo.InvariantCulture) < 0;
		}
		return false;
	}

	private static NavXmlDelta GetPersonalizationDelta(int id, string personalizationId, NavSession session, bool applyPersonalization)
	{
		if (CanApplyPersonalization(session) & applyPersonalization)
		{
			return NavCurrentThread.Session.UserPageMetadataCacheV1.Get(id, personalizationId);
		}
		return null;
	}

	private static NavAppObjectMetadataRuntimeDeltas GetUserPageMetadataDeltas(int pageId)
	{
		return NavCurrentThread.Session.UserPageMetadataCache.Get(pageId);
	}

	private static MasterPage RemoveOrphanedAndUpdateClonedActions(MasterPage masterPage, CommandBarDefinition commandBarForOriginatingActions)
	{
		FindClonedAndOriginatingActions(masterPage.CommandBar, commandBarForOriginatingActions, out var clonedActionDefinitions, out var originatingActionDefinitions);
		IList<int> originatingActionIds = originatingActionDefinitions.Keys.Select((ActionBaseDefinition originatingAction) => originatingAction.ID).ToList();
		ApplyRecursively(masterPage.CommandBar.Actions, (ActionBaseDefinition clone) => clone.ClonedFromID > 0 && !originatingActionIds.Contains(clone.ClonedFromID), (ActionBaseDefinition node) => node.Actions, (ActionBaseDefinition item) =>
		{
			item.Visible = "0";
		});
		HashSet<int> originatingCloneIds = new HashSet<int>(from k in clonedActionDefinitions.Keys
			where k.ClonedFromID < 0
			select k.ClonedFromID);
		List<ActionBaseDefinition> originatingClonedDefinations = clonedActionDefinitions.Keys.Where((ActionBaseDefinition k) => originatingCloneIds.Contains(k.ID)).ToList();
		foreach (KeyValuePair<ActionBaseDefinition, UIElementDefinition> item in clonedActionDefinitions)
		{
			List<ActionBaseDefinition> actions = GetActions(item);
			int index = actions.IndexOf(item.Key);
			if (item.Key is ActionGroupDefinition definition)
			{
				actions[index] = UpdateClonedActionBase(originatingActionDefinitions.Keys, originatingClonedDefinations, definition, UpdateActionGroup);
			}
			else if (item.Key is ActionDefinition definition2)
			{
				actions[index] = UpdateClonedActionBase(originatingActionDefinitions.Keys, originatingClonedDefinations, definition2, UpdateAction);
			}
		}
		return masterPage;
	}

	private static List<ActionBaseDefinition> GetActions(KeyValuePair<ActionBaseDefinition, UIElementDefinition> clonedActionBaseDefinition)
	{
		List<ActionBaseDefinition> result = null;
		if (clonedActionBaseDefinition.Value is ActionBaseDefinition actionBaseDefinition)
		{
			result = actionBaseDefinition.Actions;
		}
		else if (clonedActionBaseDefinition.Value is CommandBarDefinition commandBarDefinition)
		{
			result = commandBarDefinition.Actions;
		}
		return result;
	}

	private static void UpdateActionGroup(ActionGroupDefinition configured, ActionGroupDefinition cloneOfOriginating)
	{
		cloneOfOriginating.Actions = configured.Actions;
	}

	private static void UpdateAction(ActionDefinition configured, ActionDefinition cloneOfOriginating)
	{
		cloneOfOriginating.Promoted = configured.Promoted;
		cloneOfOriginating.IsIconBig = configured.IsIconBig;
		cloneOfOriginating.IsIconBigSpecified = configured.IsIconBigSpecified;
	}

	internal static TActionBase UpdateClonedActionBase<TActionBase>(IEnumerable<ActionBaseDefinition> originatingActionBaseItems, IEnumerable<ActionBaseDefinition> originatingClonedDefinations, TActionBase definition, Action<TActionBase, TActionBase> transferProperties) where TActionBase : ActionBaseDefinition
	{
		if (originatingActionBaseItems == null)
		{
			return definition;
		}
		IEnumerable<ActionBaseDefinition> originatingItems = originatingActionBaseItems.Concat(originatingClonedDefinations);
		ActionBaseDefinition actionBaseDefinition = FindRootBaseItem(originatingItems, definition.ClonedFromID);
		if (actionBaseDefinition == null || !(actionBaseDefinition is TActionBase))
		{
			return definition;
		}
		string captionML = definition.CaptionML;
		string controlGUID = definition.ControlGUID;
		int clonedFromID = definition.ClonedFromID;
		string visible = definition.Visible;
		int iD = definition.ID;
		TActionBase val = (TActionBase)actionBaseDefinition.Clone();
		transferProperties?.Invoke(definition, val);
		definition = val;
		definition.ID = iD;
		definition.ControlGUID = controlGUID;
		definition.ClonedFromID = clonedFromID;
		if (PropertyHelper.PropertyIsFalse(visible) || (PropertyHelper.PropertyIsTrue(visible) && !PropertyHelper.IsDynamic(definition.Visible)))
		{
			definition.Visible = visible;
		}
		if (!string.IsNullOrEmpty(captionML))
		{
			definition.CaptionML = captionML;
		}
		return definition;
	}

	private static ActionBaseDefinition FindRootBaseItem(IEnumerable<ActionBaseDefinition> originatingItems, int clonedFromId)
	{
		HashSet<int> hashSet = new HashSet<int>();
		ActionBaseDefinition currentItem;
		for (currentItem = originatingItems.FirstOrDefault((ActionBaseDefinition a) => a.ID == clonedFromId); currentItem != null; currentItem = originatingItems.FirstOrDefault((ActionBaseDefinition a) => a.ID == currentItem.ClonedFromID))
		{
			if (currentItem.ID >= 0)
			{
				return currentItem;
			}
			if (hashSet.Contains(currentItem.ID))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LY", Category.UIChanges, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Detected infinite loop condition in FindRootBaseItem. ID = [{0}], ClonedFromID = [{1}], Name = [{2}], Caption = [{3}], VisitedIDs = [{4}]", currentItem.ID, currentItem.ClonedFromID, currentItem.Name, currentItem.Caption, string.Join(",", hashSet));
				return null;
			}
			hashSet.Add(currentItem.ID);
		}
		return null;
	}

	internal MasterPage RemoveItemsOnPageBasedOnLicenseAndApplicationArea(MasterPage masterPage, IList<MasterPage> relatedMasterPages)
	{
		masterPage = UpdateClonedActionsFromOriginates(masterPage);
		RemoveInList(isDepartmentPage: false, masterPage.RemovedControls, masterPage.CommandBar.Actions, RemoveAction, (ActionBaseDefinition node) => node.Actions, relatedMasterPages, applyApplicationArea: true);
		RemoveInList(isDepartmentPage: false, masterPage.RemovedControls, masterPage.ContentArea.Controls, RemoveControl, GetControl, relatedMasterPages, applyApplicationArea: true);
		foreach (ControlContainerPlaceHolder control in masterPage.ContentArea.Controls)
		{
			for (int num = control.Controls.Count() - 1; num >= 0; num--)
			{
				RemoveInList(isDepartmentPage: false, masterPage.RemovedControls, control.Controls[num].Actions, RemoveAction, (ActionBaseDefinition node) => node.Actions, relatedMasterPages, applyApplicationArea: true);
			}
		}
		RemoveInList(isDepartmentPage: false, masterPage.RemovedControls, masterPage.InfopartsArea.Controls, RemoveControl, GetPart, relatedMasterPages, applyApplicationArea: true);
		return masterPage;
	}

	private static MasterPage UpdateClonedActionsFromOriginates(MasterPage masterPage)
	{
		FindClonedAndOriginatingActions(masterPage.CommandBar, masterPage.CommandBar, out var clonedActionDefinitions, out var originatingActionDefinitions);
		UpdateClonedInMasterPage(clonedActionDefinitions.Keys.ToList(), originatingActionDefinitions.Keys.ToList(), (ActionBaseDefinition a) => a.ClonedFromID != 0);
		return masterPage;
	}

	private static void FindClonedAndOriginatingActions(CommandBarDefinition configuredCommandBar, CommandBarDefinition commandBarForOriginatingActions, out Dictionary<ActionBaseDefinition, UIElementDefinition> clonedActionDefinitions, out Dictionary<ActionBaseDefinition, UIElementDefinition> originatingActionDefinitions)
	{
		clonedActionDefinitions = FindInList(configuredCommandBar, configuredCommandBar.Actions, (ActionBaseDefinition a) => a.ClonedFromID != 0);
		if (!clonedActionDefinitions.Any())
		{
			originatingActionDefinitions = new Dictionary<ActionBaseDefinition, UIElementDefinition>();
			return;
		}
		IList<int> originatingActionIds = clonedActionDefinitions.Keys.Select((ActionBaseDefinition a) => a.ClonedFromID).ToList();
		originatingActionDefinitions = FindInList(commandBarForOriginatingActions, commandBarForOriginatingActions.Actions, (ActionBaseDefinition a) => originatingActionIds.Contains(a.ID));
	}

	private static void UpdateClonedInMasterPage(IList<ActionBaseDefinition> definition, IList<ActionBaseDefinition> originatingActions, Func<ActionBaseDefinition, bool> predicate)
	{
		for (int i = 0; i < definition.Count; i++)
		{
			if (definition[i] is ActionDefinition actionDefinition && predicate(actionDefinition))
			{
				ActionDefinition originateActionFromList = GetOriginateActionFromList(originatingActions, actionDefinition);
				if (originateActionFromList != null)
				{
					actionDefinition.AccessByObjectID = originateActionFromList.AccessByObjectID;
					actionDefinition.AccessByObjectType = originateActionFromList.AccessByObjectType;
					actionDefinition.AccessByPermissionMask = originateActionFromList.AccessByPermissionMask;
					actionDefinition.ApplicationArea = originateActionFromList.ApplicationArea;
				}
			}
			UpdateClonedInMasterPage(definition[i].Actions, originatingActions, predicate);
		}
	}

	private static ActionDefinition GetOriginateActionFromList(IList<ActionBaseDefinition> originatingActions, ActionDefinition clonedAction)
	{
		foreach (ActionBaseDefinition originatingAction in originatingActions)
		{
			if (originatingAction is ActionDefinition actionDefinition && clonedAction.ClonedFromID == actionDefinition.ID)
			{
				return actionDefinition;
			}
		}
		return null;
	}

	private static Dictionary<ActionBaseDefinition, UIElementDefinition> FindInList(UIElementDefinition parent, IList<ActionBaseDefinition> list, Func<ActionBaseDefinition, bool> predicate)
	{
		Dictionary<ActionBaseDefinition, UIElementDefinition> result = new Dictionary<ActionBaseDefinition, UIElementDefinition>();
		FindInListCore(parent, list, result, predicate);
		return result;
	}

	private static void FindInListCore(UIElementDefinition listParent, IList<ActionBaseDefinition> list, Dictionary<ActionBaseDefinition, UIElementDefinition> result, Func<ActionBaseDefinition, bool> predicate)
	{
		for (int i = 0; i < list.Count; i++)
		{
			ActionBaseDefinition actionBaseDefinition = list[i];
			if (predicate(actionBaseDefinition))
			{
				result.Add(actionBaseDefinition, listParent);
			}
			FindInListCore(actionBaseDefinition, actionBaseDefinition.Actions, result, predicate);
		}
	}

	private static MasterNavigation RemoveActionsInNavigationPaneBasedOnLicense(MasterNavigation masterNavigation)
	{
		masterNavigation.RemovedNavigationControls = new List<string>();
		foreach (NavigationGroupDefinition item in masterNavigation.NavigationPane.Actions.OfType<NavigationGroupDefinition>())
		{
			RemoveInList(item.NavigationGroup == NavigationGroupType.Departments, masterNavigation.RemovedNavigationControls, item.Actions, RemoveAction, (ActionBaseDefinition node) => node.Actions, null, applyApplicationArea: true);
		}
		return masterNavigation;
	}

	public static bool CanApplyPersonalization(NavSession session)
	{
		if (!session.ConfigurationModeEnabled)
		{
			return !session.PersonalizationDisabled;
		}
		return false;
	}

	private static NavXmlDelta GetConfigurationDelta(int id, string personalizationId, NavSession session, bool applyPersonalization)
	{
		NavXmlDelta result = null;
		if (!session.ConfigurationModeEnabled || (session.ConfigurationModeEnabled & applyPersonalization))
		{
			result = ((session.ProfileKey != null) ? NavEnvironment.Instance.NavProfilePageMetadataCacheV1.Get(session.ProfileKey, id, personalizationId) : NavEnvironment.Instance.NavProfilePageMetadataCacheV1.Get(id, personalizationId));
		}
		return result;
	}

	private static NavAppObjectMetadataRuntimeDeltas GetProfilePageMetadataDeltas(int pageId, NavProfileKey profileKey)
	{
		if (profileKey == null)
		{
			profileKey = NavConfigurationProfile.GetUserProfile(NavCurrentThread.Session.User.Id);
		}
		return NavEnvironment.Instance.NavProfilePageMetadataCache.Get(profileKey, pageId);
	}

	private static void SolveDefaultFilterColumnProperty(MasterPage masterPage)
	{
		string defaultFilterColumnName = NavPageDataPersonalizationHelper.ReadValue(masterPage.PageProperties.SourceObject.SourceTable, ObjectType.Table, masterPage.PageProperties.SourceObject.SourceTable.ToString(CultureInfo.InvariantCulture), PageDataIdentifier.DefaultFilterColumnName);
		masterPage.DefaultFilterColumnName = defaultFilterColumnName;
	}

	private MasterPage GetMergedQueryMasterPage(int queryId)
	{
		NavCurrentThread.Session?.Diagnostics?.SendTraceTag("000080D", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Generate Master page for Query ID: {0}", queryId);
		MetaTable queryTableMetadata = GetQueryTableMetadata(queryId);
		MasterPage queryMasterPage = GetQueryMasterPage();
		queryMasterPage.Caption = queryTableMetadata.Caption;
		queryMasterPage.CaptionML = queryTableMetadata.CaptionML;
		queryMasterPage.Name = queryTableMetadata.Name;
		queryMasterPage.ID = queryId;
		queryMasterPage.ResourceIdentifier = Guid.NewGuid().ToString();
		queryMasterPage.InfopartsArea = new InfopartsAreaDefinition
		{
			Controls = new List<ControlBaseDefinition>
			{
				new ControlContainerPlaceHolder
				{
					ContainerType = ControlContainerType.FactBoxArea,
					Controls = new List<ControlBaseDefinition>()
				}
			}
		};
		queryMasterPage.PageProperties = new PageProperties
		{
			PageType = PageType.ListQuery,
			IsSystemPage = false,
			Editable = false,
			ApplicationArea = "#All",
			SourceObject = new SourceObjectDefinition
			{
				SourceTable = queryId,
				PageDataSource = DataSourceType.Query
			}
		};
		List<DataFieldDefinition> list = BuildQueryPageControlDefinitions(queryId, queryTableMetadata, queryMasterPage);
		if (list != null)
		{
			queryMasterPage.Expressions = list;
		}
		return queryMasterPage;
	}

	private static List<DataFieldDefinition> BuildQueryPageControlDefinitions(int queryId, MetaTable metaTable, MasterPage masterPage)
	{
		if (!(masterPage.ContentArea.Find((ElementDefinition elementDefinition) => elementDefinition is RepeaterDefinition) is RepeaterDefinition repeaterDefinition))
		{
			return null;
		}
		List<DataFieldDefinition> list = null;
		foreach (MetaField field in metaTable.Fields)
		{
			ControlDefinition controlDefinition = new ControlDefinition();
			if (field.Name.Contains("$"))
			{
				controlDefinition.Visible = "0";
			}
			controlDefinition.ID = field.Id;
			controlDefinition.ControlGUID = GenerateQueryGuid(queryId, field);
			controlDefinition.Name = field.Name;
			controlDefinition.ApplicationArea = "#All";
			controlDefinition.DataColumnName = field.Id.ToString(CultureInfo.InvariantCulture);
			controlDefinition.Triggers = new List<ControlTriggerDefinition>();
			controlDefinition.HasOnValidateEventSubscriber = false;
			controlDefinition.TableRelations = new List<TableRelationDefinition>();
			controlDefinition.RelatedObjects = new List<RelatedObjectDefinition>();
			if (field.Type == NavType.Decimal)
			{
				if (list == null)
				{
					list = masterPage.Expressions ?? new List<DataFieldDefinition>();
				}
				string text = (controlDefinition.FormatColumnName = "Control" + field.Id + "_Format");
				DataFieldDefinition dataFieldDefinition = new DataFieldDefinition();
				dataFieldDefinition.Name = text;
				dataFieldDefinition.Datatype = DataType.Text;
				dataFieldDefinition.SourceExpression = text;
				dataFieldDefinition.ExpressionType = SourceExpressionType.Property;
				list.Add(dataFieldDefinition);
			}
			repeaterDefinition.Controls.Add(controlDefinition);
		}
		return list;
	}

	private MasterPage GetMergedTableMasterPage(int tableId)
	{
		MetaTable tableMetadata = GetTableMetadata(tableId);
		MasterPage tableMasterPage = GetTableMasterPage();
		tableMasterPage.Caption = tableMetadata.Caption;
		tableMasterPage.Name = tableMetadata.Name;
		tableMasterPage.ID = 9599;
		tableMasterPage.ResourceIdentifier = Guid.NewGuid().ToString();
		tableMasterPage.InfopartsArea = new InfopartsAreaDefinition
		{
			Controls = new List<ControlBaseDefinition>
			{
				new ControlContainerPlaceHolder
				{
					ContainerType = ControlContainerType.FactBoxArea,
					Controls = new List<ControlBaseDefinition>()
				}
			}
		};
		tableMasterPage.PageProperties = new PageProperties
		{
			PageType = PageType.List,
			IsSystemPage = true,
			SourceObject = new SourceObjectDefinition
			{
				SourceTable = tableMetadata.Id,
				PageDataSource = DataSourceType.Table,
				DelayedInsert = true
			}
		};
		IEnumerable<int> validateEventIds = ((MetaTable.GetValidateEventSubscriptionsOnTable != null) ? MetaTable.GetValidateEventSubscriptionsOnTable(tableId, 0) : Enumerable.Empty<int>());
		RepeaterDefinition repeaterDefinition = tableMasterPage.ContentArea.Find((ElementDefinition elementDefinition) => elementDefinition is RepeaterDefinition) as RepeaterDefinition;
		foreach (MetaField field in tableMetadata.Fields)
		{
			if (ShouldIncludeField(field))
			{
				ControlDefinition controlDefinition = CreateControlDefinition(tableId, field, validateEventIds);
				if (field.Type == NavType.Decimal)
				{
					tableMasterPage.Expressions.Add(new DataFieldDefinition
					{
						Name = controlDefinition.FormatColumnName,
						Datatype = DataType.Text,
						ExpressionType = SourceExpressionType.Property
					});
				}
				repeaterDefinition.Controls.Add(controlDefinition);
			}
		}
		return tableMasterPage;
	}

	internal static ControlDefinition CreateControlDefinition(int tableId, MetaField field, IEnumerable<int> validateEventIds)
	{
		ControlDefinition controlDefinition = new ControlDefinition();
		controlDefinition.ID = field.Id;
		controlDefinition.ControlGUID = GenerateTableGuid(tableId, field);
		controlDefinition.Name = field.Name;
		controlDefinition.DataColumnName = field.Id.ToString(CultureInfo.InvariantCulture);
		controlDefinition.Triggers = new List<ControlTriggerDefinition>();
		controlDefinition.HasOnValidateEventSubscriber = validateEventIds.Contains(field.Id);
		if (field.Type == NavType.Decimal)
		{
			string formatColumnName = "Control" + field.Id + "_Format";
			controlDefinition.FormatColumnName = formatColumnName;
		}
		return controlDefinition;
	}

	private static string GenerateTableGuid(int tableId, MetaField field)
	{
		return GenerateObjectControlGuid(tableId, field, "0a10bb11-5c18-4cfd");
	}

	private static string GenerateQueryGuid(int queryId, MetaField field)
	{
		return GenerateObjectControlGuid(queryId, field, "f9a239db-2bec-491d");
	}

	private static string GenerateObjectControlGuid(int objectId, MetaField field, string guidPrefix)
	{
		StringBuilder stringBuilder = new StringBuilder(guidPrefix);
		stringBuilder.Append('-').Append(objectId.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0'));
		stringBuilder.Append('-').Append(field.Id.ToString(CultureInfo.InvariantCulture).PadLeft(12, '0'));
		return stringBuilder.ToString();
	}

	private static bool ShouldIncludeField(MetaField field)
	{
		if (FieldClass.FlowFilter != field.FieldClass)
		{
			return field.Enabled;
		}
		return false;
	}

	private MasterPage GetMergedMasterPage(int id, DataSourceType dataSourceType, bool applyPersonalization, string viewName)
	{
		if (SystemPages.IsServerSystemPageId(id))
		{
			throw new NavMetadataNotFoundException(ObjectType.Page, id);
		}
		if (dataSourceType == DataSourceType.Query)
		{
			return GetQueryMasterPage(id);
		}
		PageDefinition pageDefinition = GetPageDefinition(id);
		if (!string.IsNullOrEmpty(viewName))
		{
			ApplyPageViewCustomizations(pageDefinition, viewName, applyPersonalization);
		}
		else if (applyPersonalization)
		{
			ApplyPageCustomizations(pageDefinition);
		}
		pageDefinition.SetLanguage(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)));
		MasterPage masterPage = GetMasterPage(pageDefinition.Properties.PageType);
		int num = NCLMetadataCacheEntry.ResolveGroupId(-1);
		MergeMasterPageWithPageDefinition(num, masterPage, pageDefinition);
		masterPage.LanguageAppGroupId = num;
		if (pageDefinition.Properties.PageType != PageType.ReportPreview)
		{
			masterPage.PageProperties.HelpRelativeUrl = GetRelativeHelpUrl(id);
		}
		SetPageHelpBaseLink(masterPage);
		return masterPage;
	}

	private static void SetPageHelpBaseLink(MasterPage masterPage)
	{
		string helpBaseLinkForCurrentLocale = GetHelpBaseLinkForCurrentLocale();
		if (!string.IsNullOrEmpty(helpBaseLinkForCurrentLocale))
		{
			masterPage.PageProperties.HelpBaseLink = helpBaseLinkForCurrentLocale;
		}
	}

	private static string GetHelpBaseLinkForCurrentLocale()
	{
		string b = NavCurrentThread.Session?.Culture.Name;
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
		foreach (NavAppRuntimeMetadata orderedAppMetadatum in navAppGroup.OrderedAppMetadata)
		{
			NavAppManifest manifest = NavEnvironment.Instance.NavAppMetadataRetriever.GetManifest(orderedAppMetadatum.PackageId);
			if (!(manifest?.AppSupportedLocales != null))
			{
				continue;
			}
			string[] locales = manifest.AppSupportedLocales.Locales;
			foreach (string a in locales)
			{
				if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(manifest.AppHelpBaseUrl))
				{
					return manifest.AppHelpBaseUrl;
				}
			}
		}
		return null;
	}

	public MasterPage GetMasterPage(PageType pageType)
	{
		string name = string.Format(CultureInfo.InvariantCulture, "MasterPage{0}", pageType);
		return LoadMasterPageTemplate(name);
	}

	private static MasterPage GetTableMasterPage()
	{
		return LoadMasterPageTemplate("MasterPageTable");
	}

	private static MasterPage GetQueryMasterPage()
	{
		return LoadMasterPageTemplate("MasterPageListQuery");
	}

	public MasterPage GetSystemPart(SystemPartTypes partType)
	{
		PageDefinition systemPartPageDefinition = GetSystemPartPageDefinition(partType);
		MasterPage masterPage = GetMasterPage(systemPartPageDefinition.Properties.PageType);
		MasterPageMergeHelper.MergeMasterPageAndPage(masterPage, systemPartPageDefinition);
		masterPage.LanguageAppGroupId = NCLMetadataCacheEntry.ResolveGroupId(-1);
		if (systemPartPageDefinition.Properties.PageType != PageType.ReportPreview)
		{
			masterPage.PageProperties.HelpRelativeUrl = GetRelativeHelpUrl(masterPage.ID);
		}
		MasterPageMergeHelper.SetTemplateLanguage(NavCurrentThread.Session.Culture, masterPage);
		masterPage.VersionNumber = VersionNumber;
		return masterPage;
	}

	public static PageDefinition GetSystemPartPageDefinition(SystemPartTypes partType)
	{
		PageDefinition pageDefinition = null;
		byte[] array = null;
		switch (partType)
		{
		case SystemPartTypes.MyNotes:
			array = NonLocalizableFiles.MyNotes;
			break;
		case SystemPartTypes.Notes:
			array = NonLocalizableFiles.Notes;
			break;
		case SystemPartTypes.Outlook:
			array = NonLocalizableFiles.Outlook;
			break;
		case SystemPartTypes.RecordLinks:
			array = NonLocalizableFiles.RecordLinks;
			break;
		}
		if (array != null)
		{
			try
			{
				pageDefinition = PageDefinitionSerializer.Value.Deserialize(XmlHelper.SafeXmlReader(new MemoryStream(array))) as PageDefinition;
			}
			catch (InvalidOperationException innerException)
			{
				throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "Metadata for specified system part: {0} is not valid.", partType), innerException);
			}
		}
		if (pageDefinition == null)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "Metadata for specified system part: {0} is not valid.", partType));
		}
		return pageDefinition;
	}

	private static MasterPage LoadMasterPageTemplate(string name)
	{
		byte[] xmlFileFromResource = GetXmlFileFromResource(name);
		if (xmlFileFromResource == null || xmlFileFromResource.Length == 0)
		{
			throw new NavPageTypeNotSupportedException(name);
		}
		MasterPage masterPage;
		try
		{
			masterPage = (MasterPage)MasterPageSerializer.Value.Deserialize(XmlHelper.SafeXmlReader(new MemoryStream(xmlFileFromResource)));
		}
		catch (InvalidOperationException innerException)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "Cannot deserialize the master page."), innerException);
		}
		MasterPageMergeHelper.SetTemplateLanguage(NavCurrentThread.Session.Culture, masterPage);
		return masterPage;
	}

	private static MasterNavigation LoadMasterNavigationTemplate()
	{
		MasterNavigation masterNavigation;
		try
		{
			byte[] xmlFileFromResource = GetXmlFileFromResource("MasterNavigation");
			if (xmlFileFromResource == null || xmlFileFromResource.Length == 0)
			{
				throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The MasterPage template {0} was not found in the resource file.", "MasterNavigation"));
			}
			masterNavigation = (MasterNavigation)MasterNavigationSerializer.Value.Deserialize(XmlHelper.SafeXmlReader(new MemoryStream(xmlFileFromResource)));
		}
		catch (InvalidOperationException innerException)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The MasterNavigation template {0} was not found in the resource file.", "MasterNavigation"), innerException);
		}
		if (masterNavigation == null)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The MasterNavigation template {0} was not found in the resource file.", "MasterNavigation"));
		}
		MasterPageMergeHelper.SetTemplateLanguage(NavCurrentThread.Session.Culture, masterNavigation);
		return masterNavigation;
	}

	public MasterNavigation GetNavigationFrame(int homepageId, bool applyPersonalization)
	{
		MasterNavigation masterNavigation = LoadMasterNavigationTemplate();
		SetHomepageAction(masterNavigation, homepageId);
		MasterPage masterPageUnsolved;
		try
		{
			masterPageUnsolved = GetMasterPageUnsolved(homepageId, DataSourceType.Table, string.Empty, applyPersonalization);
		}
		catch (NavMetadataNotFoundException)
		{
			throw new InvalidHomepageException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.MetadataNotFound, ObjectType.Page, homepageId));
		}
		if (masterPageUnsolved.PageProperties.PageType != PageType.RoleCenter)
		{
			throw new InvalidHomepageException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.MetadataInvalidHomepageType, homepageId));
		}
		if (masterPageUnsolved.NavigationArea == null || masterPageUnsolved.NavigationArea.Actions == null)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The ActionContainer in homepage {0} is null.", homepageId));
		}
		NavAppGroup appGroup = NavCurrentThread.ResolveAppGroup();
		NavigationDefinition navigationDepartments = GetNavigationDepartments(appGroup);
		if (navigationDepartments != null)
		{
			if (navigationDepartments.ActionContainers == null)
			{
				throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The ActionContainer in the departments (MenuSuite) is null."));
			}
			MasterPageMergeHelper.MergeNavigationPageAndPage(masterNavigation, navigationDepartments);
		}
		if (masterNavigation.NavigationPane != null)
		{
			foreach (NavigationGroupDefinition action in masterNavigation.NavigationPane.Actions)
			{
				foreach (ActionBaseDefinition action2 in action.Actions)
				{
					if (!(action2 is ActionContainerPlaceHolder actionContainerPlaceHolder))
					{
						continue;
					}
					foreach (ActionContainerPlaceHolder action3 in masterPageUnsolved.NavigationArea.Actions)
					{
						if (actionContainerPlaceHolder.ContainerType == action3.ContainerType)
						{
							actionContainerPlaceHolder.Actions = action3.Actions;
							actionContainerPlaceHolder.ControlGUID = action3.ControlGUID;
							actionContainerPlaceHolder.ID = action3.ID;
							actionContainerPlaceHolder.Name = action3.Name;
							if (action.NavigationGroup == NavigationGroupType.Home)
							{
								action.ToolTip = action3.ToolTip;
								action.ToolTipML = action3.ToolTipML;
							}
							break;
						}
					}
				}
			}
		}
		masterNavigation.Caption = ProductNames.ProductNameNewClient;
		masterNavigation.SetLanguage(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)));
		return masterNavigation;
	}

	public MasterNavigation GetNavigationFrame(bool applyPersonalization, NavProfileKey profileKey = null)
	{
		MasterNavigation masterNavigation = GetNavigationFrame(GetHomepageID(profileKey), applyPersonalization);
		if (elementRemovalOption != ElementRemovalOption.None)
		{
			masterNavigation = RemoveActionsInNavigationPaneBasedOnLicense(masterNavigation);
		}
		return masterNavigation;
	}

	private void SetHomepageAction(MasterNavigation masterNavigation, int homepageID)
	{
		if (!(masterNavigation.Find(FindHomePageAction) is ActionDefinition actionDefinition))
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The RoleCenter action in Role Center {0} was not found.", homepageID));
		}
		actionDefinition.TargetID = homepageID;
	}

	public MasterNavigation GetProductOverview(ProductOverviewResourceType productOverviewResourceType)
	{
		return productOverviewResourceType switch
		{
			ProductOverviewResourceType.Departments => DepartmentsMasterNavigation(), 
			ProductOverviewResourceType.RoleCenters => throw new NotImplementedException(string.Format(NavCurrentThread.Session.RuntimeLanguage, productOverviewResourceType.ToString(), "The method or operation is not implemented.")), 
			_ => throw new ArgumentException("UnsupportedType"), 
		};
	}

	public MasterPage GetHomepage(string personalizationId, bool applyPersonalization)
	{
		MasterPage masterPage = GetMasterPageUnsolved(GetHomepageID(), DataSourceType.Table, personalizationId, applyPersonalization);
		if (elementRemovalOption != ElementRemovalOption.None)
		{
			masterPage = RemoveItemsOnPageBasedOnLicenseAndApplicationArea(masterPage, null);
		}
		return masterPage;
	}

	private static int GetHomepageID(NavProfileKey profileKey = null)
	{
		if (profileKey == null)
		{
			NavSession session = NavCurrentThread.Session;
			profileKey = session.ProfileKey ?? NavConfigurationProfile.GetUserProfile(session.User.Id);
		}
		if (profileKey != null)
		{
			int profileRoleCenterId = NavConfigurationProfile.GetProfileRoleCenterId(profileKey);
			if (profileRoleCenterId == -1)
			{
				throw new NavProfileException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.ProfileNotFound, profileKey.ProfileId, profileKey.Scope));
			}
			return profileRoleCenterId;
		}
		return NavCurrentThread.Session.SystemCodeunitFactory.SystemActionTriggers.InvokeGetDefaultRoleCenter();
	}

	private bool FindHomePageAction(ElementDefinition element)
	{
		if (element is ActionDefinition actionDefinition)
		{
			return actionDefinition.ControlGUID.Equals("{EC29FEAF-58AE-4c7a-8C7F-792D5CFD9AA0}");
		}
		return false;
	}

	public void SaveConfiguration(MasterPage configuredMasterPage, string personalizationId)
	{
		if (configuredMasterPage == null)
		{
			throw new ArgumentNullException("configuredMasterPage");
		}
		if (configuredMasterPage.PageProperties == null)
		{
			throw new InvalidOperationException("MasterPage.PageProperties is null");
		}
		if (configuredMasterPage.PageProperties.PageType == PageType.RoleCenter && configuredMasterPage.ID == GetHomepageID())
		{
			personalizationId = "";
		}
		NavSession session = NavCurrentThread.Session;
		session.Diagnostics.SendTraceTag("00000Z1", Category.UIChanges, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Start save UI change. Mode: {0} - Profile:{1} - Page: {2} - PersonalizationID:{3}", session.ConfigurationModeEnabled ? "Configuration" : "Personalization", configuredMasterPage.Caption, configuredMasterPage.ID, personalizationId);
		Guid id = session.User.Id;
		MasterPage originalMasterPage = GetOriginalMasterPage(configuredMasterPage, personalizationId);
		XmlDocument source = SerializeMasterPage(originalMasterPage);
		XmlDocument target = SerializeMasterPage(configuredMasterPage);
		NavXmlDelta navXmlDelta = NavXmlDelta.Build(source, target, new XmlDeltaCalculationRules(originalMasterPage));
		if (session.ConfigurationModeEnabled)
		{
			NavProfileKey navProfileKey = session.ProfileKey;
			if (navProfileKey == null)
			{
				navProfileKey = NavConfigurationProfile.GetUserProfile(id);
			}
			if (navProfileKey == null || string.IsNullOrEmpty(navProfileKey.ProfileId))
			{
				throw new NavProfileException(Lang.UserProfileNotFound);
			}
			if (session.ProfileKey.Scope == ProfileScope.Tenant)
			{
				throw new NavProfileException(Lang.ConfigurationChangesOnTenantProfileNotAllowed);
			}
			if (navXmlDelta != null && 0 < navXmlDelta.Items.Count)
			{
				navV1PageConfigurationRepository.SaveConfigurationDelta(navProfileKey, configuredMasterPage.ID, personalizationId, navXmlDelta);
			}
			else
			{
				navV1PageConfigurationRepository.DeleteConfiguration(navProfileKey, configuredMasterPage.ID, personalizationId);
			}
		}
		else
		{
			if (!configuredMasterPage.PageProperties.IsSystemPage)
			{
				navXmlDelta = ProcessHiddenElementDeltasDueToLicencing(navXmlDelta, configuredMasterPage, personalizationId);
			}
			if (navXmlDelta != null && 0 < navXmlDelta.Items.Count)
			{
				navV1PageConfigurationRepository.SavePersonalizationDelta(configuredMasterPage.ID, personalizationId, navXmlDelta);
			}
			else
			{
				navV1PageConfigurationRepository.DeletePersonalization(configuredMasterPage.ID, personalizationId);
			}
		}
		session.Diagnostics.SendTraceTag("00000Z2", Category.UIChanges, Verbosity.Normal, PrivacyClassification.SystemMetadata, "End save UI change. Mode: {0} - Profile:{1} - Page: {2} - PersonalizationID:{3}", session.ConfigurationModeEnabled ? "Configuration" : "Personalization", configuredMasterPage.Caption, configuredMasterPage.ID, personalizationId);
	}

	private MasterPage GetOriginalMasterPage(MasterPage configuredMasterPage, string personalizationId)
	{
		MasterPage masterPage;
		if (configuredMasterPage.PageProperties.IsSystemPage)
		{
			masterPage = GetTableMasterPage(configuredMasterPage.PageProperties.SourceObject.SourceTable, null);
		}
		else if (NavCurrentThread.Session.ConfigurationModeEnabled)
		{
			masterPage = GetMasterPageWithoutConfiguration(configuredMasterPage.ID);
			if (elementRemovalOption != ElementRemovalOption.None)
			{
				masterPage = RemoveItemsOnPageBasedOnLicenseAndApplicationArea(masterPage, null);
			}
		}
		else
		{
			masterPage = GetMasterPage(configuredMasterPage.ID, configuredMasterPage.PageProperties.SourceObject.PageDataSource, personalizationId, applyPersonalization: false);
		}
		return masterPage;
	}

	private NavXmlDelta ProcessHiddenElementDeltasDueToLicencing(NavXmlDelta delta, MasterPage configuredMasterPage, string personalizationId)
	{
		delta = delta ?? new NavXmlDelta();
		List<NavXmlBase> list = new List<NavXmlBase>();
		List<string> list2 = new List<string>();
		List<string> removedControls = GetMasterPage(configuredMasterPage.ID, configuredMasterPage.PageProperties.SourceObject.PageDataSource, personalizationId, applyPersonalization: true).RemovedControls;
		NavXmlDelta navXmlDelta = navV1PageConfigurationRepository.LoadPersonalizationDelta(configuredMasterPage.ID, personalizationId);
		if (configuredMasterPage.PageProperties.PageType == PageType.RoleCenter)
		{
			list2 = GetNavigationFrame(applyPersonalization: true).RemovedNavigationControls ?? list2;
		}
		IEnumerable<NavXmlBase> enumerable = navXmlDelta?.Items;
		foreach (NavXmlBase item in enumerable ?? Enumerable.Empty<NavXmlBase>())
		{
			if (removedControls.Contains(item.Id))
			{
				list.Add(item);
			}
			else if (list2.Contains(item.Id))
			{
				list.Add(item);
			}
		}
		list2.ForEach((string item) =>
		{
			delta.Items.Where((NavXmlBase d) => d.Id == item).ToList().ForEach((NavXmlBase d) =>
			{
				delta.Items.Remove(d);
			});
		});
		list.ForEach((NavXmlBase d) =>
		{
			delta.Items.Add(d);
		});
		return delta;
	}

	public static void SavePageDataPersonalization(string personalizationId, int objectId, ObjectType objectType, string pageDataIdentifier, string value)
	{
		NavPageDataPersonalizationHelper.SaveValue(personalizationId, objectId, objectType, pageDataIdentifier, value);
	}

	private static XmlDocument SerializeMasterPage(MasterPage page)
	{
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		XmlWriter xmlWriter = xmlDocument.CreateNavigator().AppendChild();
		try
		{
			MasterPageSerializer.Value.Serialize(xmlWriter, page, new XmlSerializerNamespaces());
		}
		catch (InvalidOperationException innerException)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "Cannot serialize the master page"), innerException);
		}
		xmlWriter.Close();
		return xmlDocument;
	}

	private static MasterPage DeserializeMasterPage(XmlDocument xmlDocument)
	{
		return (MasterPage)MasterPageSerializer.Value.Deserialize(new XmlNodeReader(xmlDocument.DocumentElement));
	}

	private static byte[] GetXmlFileFromResource(string filename)
	{
		return NavResourceManager.GetMasterPageXml(filename);
	}

	public virtual PageDefinition GetPageDefinition(int id, int appGroupId = -1)
	{
		PageDefinition item = GetPageDefinitionWithExtensions(id, appGroupId).Item;
		item = (PageDefinition)item.Clone();
		return NCLMetaForm.ApplyAppGroupAwareEnumMetadataToPageExpressions(item);
	}

	public (PageDefinition, NavAppRuntimeMetadata) GetBasePageDefinition(int id)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Page, id, out var metaApplicationObject, -1, requireCompiled: false))
		{
			throw new NavMetadataNotFoundException(ObjectType.Page, id);
		}
		NCLMetaForm nCLMetaForm = (NCLMetaForm)metaApplicationObject;
		return (nCLMetaForm.GetBasePageDefinition(NavCurrentThread.ResolveAppGroup().GroupId), metaApplicationObject.OwningApp);
	}

	public MetadataExtension<PageDefinition> GetPageDefinitionWithExtensions(int id, int appGroupId = -1)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Page, id, out var metaApplicationObject, appGroupId, requireCompiled: false))
		{
			throw new NavMetadataNotFoundException(ObjectType.Page, id);
		}
		NCLMetaForm nCLMetaForm = (NCLMetaForm)metaApplicationObject;
		try
		{
			int tenantAppGroupId = ((appGroupId == -1) ? NavCurrentThread.ResolveAppGroup().GroupId : appGroupId);
			MetadataExtension<PageDefinition> pageDefinitionWithExtensions = nCLMetaForm.GetPageDefinitionWithExtensions(tenantAppGroupId);
			if (pageDefinitionWithExtensions.Item.ID != id)
			{
				throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.PageDefinitionIsInvalid, id));
			}
			return pageDefinitionWithExtensions;
		}
		catch (InvalidOperationException ex)
		{
			LogWriterHelper.LogExceptionEvent(ex);
			throw new NavNSInvalidPageDefinitionException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.CannotDeserializeThePageDefinition, id), ex);
		}
	}

	private NavigationDefinition GetNavigationDepartments(NavAppGroup appGroup)
	{
		NavigationDefinition navigationDefinition = ((NavCurrentThread.Session.ClientConnectionType != ConnectionType.WebClient) ? NavGlobal.NCLObjectXmlMetadataLoader.GetNavigationDefintion("1000..1999", appGroup) : NavGlobal.NCLObjectXmlMetadataLoader.GetNavigationDefintion(string.Format(CultureInfo.CurrentCulture, "{0}|{1}", 2100000000, "1000..1999"), appGroup));
		extensionNavigationDataAggregator.AppendExtensionNavigationData(navigationDefinition, appGroup);
		return navigationDefinition;
	}

	private MasterNavigation DepartmentsMasterNavigation()
	{
		MasterNavigation masterNavigation = LoadMasterNavigationTemplate();
		NavAppGroup appGroup = NavCurrentThread.ResolveAppGroup();
		NavigationDefinition navigationDefintion = NavGlobal.NCLObjectXmlMetadataLoader.GetNavigationDefintion("1000..1999", appGroup);
		extensionNavigationDataAggregator.AppendExtensionNavigationData(navigationDefintion, appGroup);
		if (navigationDefintion != null)
		{
			if (navigationDefintion.ActionContainers == null)
			{
				throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The ActionContainer in the departments (MenuSuite) is null."));
			}
			MasterPageMergeHelper.MergeNavigationPageAndPage(masterNavigation, navigationDefintion);
		}
		masterNavigation.SetLanguage(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)));
		return masterNavigation;
	}

	internal MasterPage CreateRequestPage(PageDefinition pageDefinition, KeyValuePair<int, string>[] captionML, int id)
	{
		return CreatePage(pageDefinition, captionML, id, PageType.ReportPreview);
	}

	private MasterPage CreateXmlPage(PageDefinition pageDefinition, KeyValuePair<int, string>[] captionML, int id)
	{
		return CreatePage(pageDefinition, captionML, id, PageType.XmlPort);
	}

	private MasterPage CreatePage(PageDefinition pageDefinition, KeyValuePair<int, string>[] captionML, int id, PageType pageType)
	{
		MasterPage masterPage = GetMasterPage(pageType);
		pageDefinition = NCLMetaForm.ApplyAppGroupAwareEnumMetadataToPageExpressions(pageDefinition);
		int num = NCLMetadataCacheEntry.ResolveGroupId(-1);
		if (pageType == PageType.ReportPreview)
		{
			ModifyReportRequestPage(pageDefinition, id, masterPage);
		}
		else
		{
			MergeMasterPageWithPageDefinition(num, masterPage, pageDefinition);
		}
		masterPage.LanguageAppGroupId = num;
		if (pageType != PageType.ReportPreview)
		{
			masterPage.PageProperties.HelpRelativeUrl = GetRelativeHelpUrl(id);
		}
		string abbreviatedLanguageName = LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID));
		masterPage.SetLanguage(NavCurrentThread.Session.AbbreviatedLanguageName, abbreviatedLanguageName);
		if (pageType != PageType.FilterPage)
		{
			NCLCaptionStrings nCLCaptionStrings = new NCLCaptionStrings(captionML, masterPage.Caption);
			masterPage.Caption = nCLCaptionStrings.GetValueOrDefault();
		}
		else
		{
			masterPage.Caption = pageDefinition.Caption;
			masterPage.CaptionML = abbreviatedLanguageName + "=" + pageDefinition.Caption;
			masterPage.PageProperties.IsSystemPage = true;
		}
		masterPage.ID = id;
		masterPage.VersionNumber = NavGlobal.MetadataProvider.VersionNumber;
		Dictionary<int, MetaTable> dictionary = new Dictionary<int, MetaTable>();
		LoadExpressionRelationTables(masterPage, dictionary);
		PropertiesSolveHelper.SolvePropertiesDefaulting(masterPage, null, dictionary);
		SolveRequestPageProperies(masterPage);
		return masterPage;
	}

	private static void ModifyReportRequestPage(PageDefinition pageDefinition, int id, MasterPage masterPage)
	{
		if (masterPage.ContentArea.Controls[0] is ControlContainerPlaceHolder { Controls: null } controlContainerPlaceHolder)
		{
			controlContainerPlaceHolder.Controls = new List<ControlBaseDefinition>();
		}
		ControlGroupDefinition controlGroupDefinition = (ControlGroupDefinition)masterPage.Find((ElementDefinition element) => element is ControlGroupDefinition && ((ControlGroupDefinition)element).ControlGUID == "{47328C3B-4E38-478E-8907-C188351EF109}");
		if (controlGroupDefinition != null)
		{
			if (!pageDefinition.Properties.SourceObject.SaveValues || !NavObjectOptions.Exists(id, ObjectType.Report))
			{
				controlGroupDefinition.Visible = "0";
			}
			if (!FeatureSwitches.Instance.DisableReportSettingsEdit)
			{
				string abbreviatedLanguageName = LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID));
				controlGroupDefinition.InstructionalTextML = abbreviatedLanguageName + "=" + string.Format(CultureInfo.CurrentCulture, Lang.RequestPageSavedSettingsInstructions, Lang.RequestPageLatestSavedSettingsName);
			}
		}
		ControlDefinition controlDefinition = (ControlDefinition)masterPage.Find((ElementDefinition element) => element is ControlDefinition && ((ControlDefinition)element).ControlGUID == "{47889756-5016-46B4-8A54-C958239E96C8}");
		if (controlDefinition != null)
		{
			controlDefinition.DataColumnName = "ObjectOptions";
			if (NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Page, ObjectOptionsHelper.ReportSettingsLookupForm, out var _, -1, requireCompiled: false))
			{
				controlDefinition.LookupFormID = ServerUserSettings.Instance.ReportSettingsLookupForm.Value;
			}
			AddTableRelationForObjectOptions(id, controlDefinition);
		}
		MasterPageMergeHelper.MergeMasterPageAndPage(masterPage, pageDefinition);
		if (FeatureSwitches.Instance.DisableReportSettingsEdit && controlDefinition != null && pageDefinition.Properties.SourceObject.SaveValues)
		{
			ControlBaseDefinition controlBaseDefinition = masterPage.ContentArea.Controls.Find((ControlBaseDefinition control) => control is ControlContainerPlaceHolder && ((ControlContainerPlaceHolder)control).ContainerType == ControlContainerType.ContentArea);
			if (controlBaseDefinition != null)
			{
				IEnumerable<UIElementDefinition> enumerable = controlBaseDefinition.FindAll((ElementDefinition element) => element is UIElementDefinition).OfType<UIElementDefinition>();
				foreach (UIElementDefinition item2 in enumerable)
				{
					if (!PropertyHelper.IsDynamic(item2.Enabled))
					{
						item2.Enabled = "IsLatestSavedSettings";
					}
				}
			}
			ControlBaseDefinition controlBaseDefinition2 = masterPage.ContentArea.Controls.Find((ControlBaseDefinition control) => control is ControlContainerPlaceHolder && ((ControlContainerPlaceHolder)control).ContainerType == ControlContainerType.RequestPageFilters);
			if (controlBaseDefinition2 != null)
			{
				IEnumerable<ControlGroupBaseDefinition> enumerable2 = masterPage.FindAll((ElementDefinition element) => element is ControlGroupBaseDefinition && ((ControlGroupBaseDefinition)element).Controls.All((ControlBaseDefinition control) => control is FilterControlDefinition)).OfType<ControlGroupBaseDefinition>();
				foreach (ControlGroupBaseDefinition item3 in enumerable2)
				{
					item3.Enabled = "IsLatestSavedSettings";
				}
			}
			ActionDefinition actionDefinition = (ActionDefinition)masterPage.CommandBar.Find((ElementDefinition element) => element is ActionDefinition && ((ActionDefinition)element).ControlGUID == "{D57E55A5-99AE-44c5-AA69-8C1F489C5CA2}");
			if (actionDefinition != null)
			{
				actionDefinition.Enabled = "IsLatestSavedSettings";
			}
			DataFieldDefinition item = new DataFieldDefinition
			{
				Name = "IsLatestSavedSettings",
				Datatype = DataType.Boolean,
				ExpressionType = SourceExpressionType.VariableIncludedInDataSet,
				SourceExpression = "IsLatestSavedSettings",
				ExpressionIsAssignable = true,
				IsDataSetVar = true
			};
			masterPage.Expressions.Add(item);
		}
		AddSourceExpression(masterPage, "ObjectOptions", "ParameterSet", DataType.Text, isAssignable: true);
	}

	private static void AddSourceExpression(MasterPage masterPage, string controlDataColumnName, string sourceExpression, DataType dataType, bool isAssignable)
	{
		DataFieldDefinition item = new DataFieldDefinition
		{
			Name = controlDataColumnName,
			Datatype = dataType,
			ExpressionType = SourceExpressionType.SourceExpression,
			SourceExpression = sourceExpression,
			ExpressionIsAssignable = isAssignable
		};
		masterPage.Expressions.Add(item);
	}

	private static void AddTableRelationForObjectOptions(int id, ControlDefinition configurationName)
	{
		TableRelationDefinition tableRelationDefinition = new TableRelationDefinition
		{
			TableID = 2000000196,
			TableName = "ObjectOptions",
			FieldID = 1,
			Filters = new List<FilterDefinition>(),
			Conditions = new List<ConditionDefinition>()
		};
		FilterDefinition item = new FilterDefinition
		{
			FilterGroup = 2,
			FieldID = 2,
			FilterType = FilterType.CONST,
			FilterValue = id.ToString(CultureInfo.CurrentCulture)
		};
		FilterDefinition item2 = new FilterDefinition
		{
			FilterGroup = 2,
			FieldID = 3,
			FilterType = FilterType.CONST,
			FilterValue = "Report"
		};
		FilterDefinition item3 = new FilterDefinition
		{
			FilterGroup = 2,
			FieldID = 4,
			FilterType = FilterType.CONST,
			FilterValue = NavFilterProcessingHelper.EscapeFilterValue(NavCurrentThread.Session.CompanyName)
		};
		FilterDefinition item4 = new FilterDefinition
		{
			FilterGroup = 2,
			FieldID = 5,
			FilterType = FilterType.FILTER,
			FilterValue = NavFilterProcessingHelper.EscapeFilterValue(NavCurrentThread.Session.User.Name) + "|''"
		};
		tableRelationDefinition.Filters.Add(item);
		tableRelationDefinition.Filters.Add(item2);
		tableRelationDefinition.Filters.Add(item3);
		tableRelationDefinition.Filters.Add(item4);
		configurationName.TableRelations.Add(tableRelationDefinition);
	}

	private void SolveRequestPageProperies(MasterPage masterPage)
	{
		foreach (ControlGroupDefinition item in masterPage.FindAll(IsControlGroupDefinition))
		{
			if (!string.IsNullOrEmpty(item.Caption))
			{
				continue;
			}
			FilterControlDefinition filterDefinition = GetFilterDefinition(item);
			if (filterDefinition == null || (masterPage.PageProperties.PageType == PageType.FilterPage && !string.IsNullOrEmpty(item.Caption)))
			{
				continue;
			}
			MetaTable tableMetadata = GetTableMetadata(filterDefinition.FilterTableID);
			if (tableMetadata != null)
			{
				if (!string.IsNullOrEmpty(tableMetadata.Caption))
				{
					item.Caption = tableMetadata.Caption;
					item.CaptionML = tableMetadata.MergedCaptionMultiLanguage;
				}
				else
				{
					item.Caption = tableMetadata.Name;
				}
			}
		}
	}

	private static FilterControlDefinition GetFilterDefinition(ControlGroupDefinition controlGroupDef)
	{
		foreach (ControlBaseDefinition control in controlGroupDef.Controls)
		{
			if (control is FilterControlDefinition result)
			{
				return result;
			}
		}
		return null;
	}

	private static bool IsControlGroupDefinition(ElementDefinition element)
	{
		return element is ControlGroupDefinition;
	}

	public MasterPage GetFilterPageForRecord(PageDefinition pageDefinition, IList<NavRecordRef> tables)
	{
		int languageAppGroupId = NavCurrentThread.ResolveAppGroup().GroupId;
		foreach (NCLMetaTable item in from table in tables
			select NavGlobal.NCLMetadata.GetMetaTableById(table.Record.TableID, -1, requireCompiled: false) into objectMetadataTable
			where LanguageProvider.Provider.HasData(languageAppGroupId)
			select objectMetadataTable)
		{
			pageDefinition.MergeMultiLanguage(ObjectType.Table, item.ObjectId, languageAppGroupId);
		}
		return CreatePage(pageDefinition, null, 9598, PageType.FilterPage);
	}

	public MasterPage GetRequestPageForReport(int reportId)
	{
		return GetReportMetadata(reportId).RequestFormMetadata;
	}

	public MasterPage GetRequestPageForXmlPort(int xmlPortId)
	{
		return GetXmlPortMetadata(xmlPortId).RequestFormMetadata;
	}

	public PageDependencyResult GetMetadataForPageAndAllItsDependencies(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, ICollection<int> tablesToSkip, string viewName)
	{
		PageDependencyResult pageDependencyResult = new PageDependencyResult();
		PageResponseDependencyEntry metadataForPage = GetMetadataForPage(pageId, dataSourceType, personalizationId, applyPersonalization, forceElementRemovalOptionIfApplicable: false, viewName);
		string personalizationId2 = metadataForPage.PersonalizationId;
		PageDependencyResult masterPageDependencies = GetMasterPageDependencies(metadataForPage.MasterPage, personalizationId2, applyPersonalization, tablesToSkip);
		pageDependencyResult.MasterPages.Add(metadataForPage);
		List<MasterPage> list = new List<MasterPage>();
		foreach (PageResponseDependencyEntry masterPage in masterPageDependencies.MasterPages)
		{
			pageDependencyResult.MasterPages.Add(masterPage);
			list.Add(masterPage.MasterPage);
		}
		foreach (MetaTable metaTable in masterPageDependencies.MetaTables)
		{
			pageDependencyResult.MetaTables.Add(metaTable);
		}
		if (elementRemovalOption != ElementRemovalOption.None)
		{
			RemoveItemsOnPageBasedOnLicenseAndApplicationArea(metadataForPage.MasterPage, list);
		}
		return pageDependencyResult;
	}

	public static PageDefinition ApplyPageViewCustomizations(PageDefinition pageDefinition, string viewName, bool applyPersonalization)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected Obj, but got Unknown
		if (!pageDefinition.Properties.IsViewSupported() || NavCurrentThread.Session.ClientConnectionType == ConnectionType.RoleTailoredClient)
		{
			return pageDefinition;
		}
		try
		{
			if (applyPersonalization)
			{
				ApplyPageCustomizations(pageDefinition, viewName);
			}
			PageViewDefinition viewFromPageDefinition = GetViewFromPageDefinition(pageDefinition, viewName);
			if (viewFromPageDefinition == null)
			{
				throw new NavNCLViewMetadataNotFoundException(CultureInfo.CurrentCulture, pageDefinition.ID, viewName);
			}
			ActionDefinition actionViewFromPageDefinition = GetActionViewFromPageDefinition(pageDefinition, viewName);
			if (actionViewFromPageDefinition != null)
			{
				actionViewFromPageDefinition.IsViewApplied = true;
			}
			pageDefinition.Properties.ViewName = viewName;
			NavAppObjectMetadataRuntimeDeltas viewDeltas = GetViewDeltas(viewFromPageDefinition);
			if (((NavAppObjectMetadataDeltaCreator<Delta>)(object)viewDeltas).AllDeltas.Any())
			{
				MetadataRuntimeDeltaApplicator.GetSharedInstanceWithAllAdapters(ObjectType.PageExtension).ApplyDeltas(new DeltaApplicationContext
				{
					SubjectId = new ApplicationObjectId(ObjectType.Page, pageDefinition.ID),
					Subject = pageDefinition,
					Deltas = new List<NavAppObjectMetadataRuntimeDeltas> { viewDeltas }
				}, new IDeltaValidationFilter[1]
				{
					new ControlAddDeltaValidationFilter()
				});
			}
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007NW", Category.Extensions, ex);
			throw NavAppObjectMetadataException.Create("Page View Metadata Application", new ApplicationObjectId(ObjectType.Page, pageDefinition.ID), ex);
		}
		return pageDefinition;
	}

	private static PageViewDefinition GetViewFromPageDefinition(PageDefinition pageDefinition, string viewName)
	{
		foreach (PageViewDefinition viewContainer in pageDefinition.ViewContainers)
		{
			if (SemanticFacts.IsSameName(viewContainer.Name, viewName))
			{
				return viewContainer;
			}
		}
		return null;
	}

	private static ActionDefinition GetActionViewFromPageDefinition(PageDefinition pageDefinition, string viewName)
	{
		foreach (ActionContainerDefinition actionContainer in pageDefinition.ActionContainers)
		{
			if (actionContainer.ActionContainerType != ActionContainerType.ViewActions)
			{
				continue;
			}
			foreach (ActionBaseDefinition action in actionContainer.Actions)
			{
				if (action is ActionDefinition actionDefinition && SemanticFacts.IsSameName(actionDefinition.RunObjectView, viewName))
				{
					return actionDefinition;
				}
			}
			return null;
		}
		return null;
	}

	private static NavAppObjectMetadataRuntimeDeltas GetViewDeltas(PageViewDefinition viewDefinition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected Obj, but got Unknown
		NavAppObjectMetadataRuntimeDeltas val = new NavAppObjectMetadataRuntimeDeltas();
		if (string.IsNullOrEmpty(viewDefinition.ViewLayoutDelta))
		{
			return val;
		}
		XElement root = XDocument.Parse(viewDefinition.ViewLayoutDelta).Root;
		foreach (XElement item in root.Elements("ControlChange"))
		{
			((NavAppObjectMetadataDeltaCreator<Delta>)(object)val).Add((Delta)(object)ContentChangeDelta.FromXml(item));
		}
		foreach (XElement item2 in root.Elements("ControlMove"))
		{
			((NavAppObjectMetadataDeltaCreator<Delta>)(object)val).Add((Delta)(object)ContentMoveDelta.FromXml(item2));
		}
		return val;
	}

	internal static NavAppObjectMetadataRuntimeDeltas AdjustPersonalizationDeltasForViews(NavAppObjectMetadataRuntimeDeltas deltas)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected Obj, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		NavAppObjectMetadataRuntimeDeltas val = new NavAppObjectMetadataRuntimeDeltas();
		foreach (Delta allDelta in ((NavAppObjectMetadataDeltaCreator<Delta>)(object)deltas).AllDeltas)
		{
			DeltaTargetKind targetKind = allDelta.TargetKind;
			if ((int)targetKind != 1)
			{
				((NavAppObjectMetadataDeltaCreator<Delta>)(object)val).Add(allDelta);
			}
		}
		return val;
	}

	public static PageDefinition ApplyPageCustomizations(PageDefinition pageDefinition, string viewName = null)
	{
		NavAppObjectMetadataRuntimeDeltas configurationDeltas = null;
		NavAppObjectMetadataRuntimeDeltas personalizationDeltas = null;
		try
		{
			configurationDeltas = GetProfilePageMetadataDeltas(pageDefinition.ID, NavCurrentThread.Session.ProfileKey);
			personalizationDeltas = GetUserPageMetadataDeltas(pageDefinition.ID);
			ApplyPageCustomizationDeltas(pageDefinition, configurationDeltas, personalizationDeltas, viewName);
			return pageDefinition;
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000112", Category.Extensions, ex);
			string pageCustomizationExceptionMessage = GetPageCustomizationExceptionMessage(pageDefinition, configurationDeltas, personalizationDeltas);
			throw NavAppObjectMetadataException.Create(pageCustomizationExceptionMessage, new ApplicationObjectId(ObjectType.Page, pageDefinition.ID), ex);
		}
	}

	internal static void ApplyPageCustomizationDeltas(PageDefinition pageDefinition, NavAppObjectMetadataRuntimeDeltas configurationDeltas, NavAppObjectMetadataRuntimeDeltas personalizationDeltas, string viewName)
	{
		List<NavAppObjectMetadataRuntimeDeltas> list = new List<NavAppObjectMetadataRuntimeDeltas>();
		HashSet<string> viewsToFlag = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (configurationDeltas != null)
		{
			list.Add(configurationDeltas);
			pageDefinition.Properties.ConfigurationTypeApplied = NavAppObjectMetadataDeltaCreatorExtensions.CustomizationType((NavAppObjectMetadataDeltaCreator<Delta>)(object)configurationDeltas);
		}
		if (personalizationDeltas != null && NavCurrentThread.Session.ClientConnectionType != ConnectionType.RoleTailoredClient)
		{
			if (!string.IsNullOrEmpty(viewName))
			{
				personalizationDeltas = AdjustPersonalizationDeltasForViews(personalizationDeltas);
			}
			list.Add(personalizationDeltas);
			pageDefinition.Properties.PersonalizationTypeApplied = NavAppObjectMetadataDeltaCreatorExtensions.CustomizationType((NavAppObjectMetadataDeltaCreator<Delta>)(object)personalizationDeltas);
			SetUserPersonalizationInformation(pageDefinition, personalizationDeltas, viewsToFlag, viewName);
		}
		ApplyPageCustomizationsDeltas(pageDefinition, list);
		SetFlagOnViewsInUserScope(pageDefinition, viewsToFlag);
	}

	private static void ApplyPageCustomizationsDeltas(PageDefinition pageDefinition, List<NavAppObjectMetadataRuntimeDeltas> deltas)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected Obj, but got Unknown
		if (deltas.Any())
		{
			MetadataRuntimeDeltaApplicator.GetSharedInstanceWithAllAdapters(ObjectType.PageExtension).ApplyDeltas(new DeltaApplicationContext
			{
				SubjectId = new ApplicationObjectId(ObjectType.Page, pageDefinition.ID),
				Subject = pageDefinition,
				Deltas = deltas
			}, new IDeltaValidationFilter[1]
			{
				new ControlAddDeltaValidationFilter()
			});
		}
	}

	private static string GetPageCustomizationExceptionMessage(PageDefinition pageDefinition, NavAppObjectMetadataRuntimeDeltas configurationDeltas, NavAppObjectMetadataRuntimeDeltas personalizationDeltas)
	{
		bool flag = configurationDeltas != null;
		bool flag2 = personalizationDeltas != null;
		if (!flag && !flag2)
		{
			return "Empty Page Metadata Application";
		}
		if (flag && !flag2)
		{
			return "Profile Page Metadata Application";
		}
		if (!flag & flag2)
		{
			return "User Page Metadata Application";
		}
		try
		{
			ApplyPageCustomizationsDeltas(pageDefinition, new List<NavAppObjectMetadataRuntimeDeltas> { configurationDeltas });
		}
		catch (Exception)
		{
			return "Profile Page Metadata Application";
		}
		try
		{
			ApplyPageCustomizationsDeltas(pageDefinition, new List<NavAppObjectMetadataRuntimeDeltas> { personalizationDeltas });
		}
		catch (Exception)
		{
			return "User Page Metadata Application";
		}
		return "Profile or User Page Metadata Application";
	}

	internal static void SetFlagOnViewsInUserScope(PageDefinition pageDefinition, HashSet<string> viewsToFlag)
	{
		if (!pageDefinition.Properties.IsViewSupported() || (viewsToFlag != null && viewsToFlag.Count == 0))
		{
			return;
		}
		foreach (ActionContainerDefinition actionContainer in pageDefinition.ActionContainers)
		{
			if (actionContainer.ActionContainerType != ActionContainerType.ViewActions)
			{
				continue;
			}
			{
				foreach (ActionBaseDefinition action in actionContainer.Actions)
				{
					if ((viewsToFlag == null || viewsToFlag.Contains(action.Name)) && action is ActionDefinition actionDefinition)
					{
						actionDefinition.IsViewInUserScope = true;
					}
				}
				break;
			}
		}
	}

	private static void SetUserPersonalizationInformation(PageDefinition pageDefinition, NavAppObjectMetadataRuntimeDeltas runtimeDeltas, HashSet<string> viewsToFlag, string viewName)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected I4, but got Unknown
		foreach (Delta allDelta in ((NavAppObjectMetadataDeltaCreator<Delta>)(object)runtimeDeltas).AllDeltas)
		{
			DeltaTargetKind targetKind = allDelta.TargetKind;
			switch ((int)targetKind)
			{
			case 0:
				pageDefinition.Properties.PersonalizationChangesApplied |= DesignerPersonalizationTypes.Actions;
				break;
			case 1:
			case 3:
				pageDefinition.Properties.PersonalizationChangesApplied |= DesignerPersonalizationTypes.Controls;
				break;
			case 2:
			{
				pageDefinition.Properties.PersonalizationChangesApplied |= DesignerPersonalizationTypes.Views;
				if (string.IsNullOrEmpty(viewName))
				{
					break;
				}
				ActionAddDelta val = (ActionAddDelta)(object)((allDelta is ActionAddDelta) ? allDelta : null);
				if (val != null)
				{
					viewsToFlag.Add(((ContentAddDelta<ActionBaseDefinition>)(object)val).Context.Content.Name);
					break;
				}
				ViewDefinitionAddDelta val2 = (ViewDefinitionAddDelta)(object)((allDelta is ViewDefinitionAddDelta) ? allDelta : null);
				if (val2 != null)
				{
					SetViewLayoutPersonalizationTypeApplied(pageDefinition, viewName, val2);
				}
				break;
			}
			}
			if (CanEarlyReturn(pageDefinition.Properties))
			{
				break;
			}
		}
	}

	private static void SetViewLayoutPersonalizationTypeApplied(PageDefinition pageDefinition, string viewName, ViewDefinitionAddDelta viewDefinitionDelta)
	{
		PageViewDefinition content = ((ContentAddDelta<PageViewDefinition>)(object)viewDefinitionDelta).Context.Content;
		if (SemanticFacts.IsSameName(content.Name, viewName))
		{
			NavAppObjectMetadataRuntimeDeltas viewDeltas = GetViewDeltas(content);
			if (((NavAppObjectMetadataDeltaCreator<Delta>)(object)viewDeltas).AllDeltas.Any())
			{
				pageDefinition.Properties.PersonalizationChangesApplied |= DesignerPersonalizationTypes.ViewControls;
			}
		}
	}

	private static bool CanEarlyReturn(PageProperties properties)
	{
		if (properties.IsViewSupported())
		{
			if (properties.PersonalizationChangesApplied.HasFlag(DesignerPersonalizationTypes.Actions) && properties.PersonalizationChangesApplied.HasFlag(DesignerPersonalizationTypes.Controls) && properties.PersonalizationChangesApplied.HasFlag(DesignerPersonalizationTypes.Views))
			{
				return properties.PersonalizationChangesApplied.HasFlag(DesignerPersonalizationTypes.ViewControls);
			}
			return false;
		}
		if (properties.PersonalizationChangesApplied.HasFlag(DesignerPersonalizationTypes.Actions))
		{
			return properties.PersonalizationChangesApplied.HasFlag(DesignerPersonalizationTypes.Controls);
		}
		return false;
	}

	private PageResponseDependencyEntry GetMetadataForPage(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, bool forceElementRemovalOptionIfApplicable, string viewName = null)
	{
		MasterPage masterPage = GetMasterPage(pageId, dataSourceType, personalizationId, applyPersonalization, forceElementRemovalOptionIfApplicable, viewName);
		if (masterPage != null && masterPage.PageProperties.PageType == PageType.RoleCenter)
		{
			personalizationId = "";
			masterPage = GetMasterPage(pageId, DataSourceType.Table, personalizationId, applyPersonalization, forceElementRemovalOptionIfApplicable);
		}
		return new PageResponseDependencyEntry
		{
			MasterPage = masterPage,
			PersonalizationId = personalizationId
		};
	}

	private PageDependencyResult GetMasterPageDependencies(MasterPage page, string personalizationId, bool applyPersonalization, ICollection<int> tablesToSkip)
	{
		PageDependencyResult pageDependencyResult = new PageDependencyResult();
		if (page.PageProperties.SourceObject.PageDataSource == DataSourceType.Query)
		{
			if (page.PageProperties.SourceObject.SourceTable != 0)
			{
				MetaTable queryTableMetadata = GetQueryTableMetadata(page.PageProperties.SourceObject.SourceTable);
				if (!tablesToSkip.Contains(queryTableMetadata.Id))
				{
					pageDependencyResult.MetaTables.Add(queryTableMetadata);
				}
			}
			return pageDependencyResult;
		}
		foreach (int visibleInfoPartId in GetVisibleInfoPartIds(page))
		{
			GetMetadataForPageAndAddToCollection(visibleInfoPartId, DataSourceType.Table, personalizationId, applyPersonalization, pageDependencyResult);
		}
		if (page.PageProperties.CardFormID != 0)
		{
			GetMetadataForPageAndAddToCollection(page.PageProperties.CardFormID, DataSourceType.Table, "", applyPersonalization, pageDependencyResult);
		}
		MetaTable metaTable = null;
		int sourceTable = page.PageProperties.SourceObject.SourceTable;
		if (sourceTable != 0)
		{
			metaTable = GetTableMetadata(sourceTable);
			if (!tablesToSkip.Contains(metaTable.Id) && !pageDependencyResult.MetaTables.Contains(metaTable))
			{
				pageDependencyResult.MetaTables.Add(metaTable);
			}
		}
		if (metaTable != null)
		{
			List<MetaField> list = new List<MetaField>();
			List<ElementDefinition> list2 = page.ContentArea.FindAll((ElementDefinition control) => control is ControlDataboundDefinition);
			foreach (ElementDefinition item in list2)
			{
				ControlDataboundDefinition controlDataboundDefinition = (ControlDataboundDefinition)item;
				int result = 0;
				if (int.TryParse(controlDataboundDefinition.DataColumnName, out result))
				{
					MetaField fieldsById = metaTable.GetFieldsById(result);
					if (fieldsById == null)
					{
						throw NavMetaFieldNotFoundException.Create(CultureInfo.CurrentCulture, result, metaTable.Id);
					}
					list.Add(fieldsById);
				}
			}
			foreach (MetaField item2 in list)
			{
				foreach (MetaFieldRelation relation in item2.Relations)
				{
					GetMetadataForTableAndAddToCollection(relation.TableId, pageDependencyResult, tablesToSkip);
				}
			}
		}
		foreach (int partId in GetVisibleInfoPartIds(page))
		{
			PageResponseDependencyEntry pageResponseDependencyEntry = pageDependencyResult.MasterPages.Find((PageResponseDependencyEntry entry) => entry.MasterPage.ID == partId && entry.PersonalizationId == personalizationId);
			MasterPage masterPage = pageResponseDependencyEntry.MasterPage;
			if (masterPage.PageProperties.SourceObject.SourceTable != 0)
			{
				GetMetadataForTableAndAddToCollection(masterPage.PageProperties.SourceObject.SourceTable, pageDependencyResult, tablesToSkip);
			}
		}
		if (page.Expressions != null)
		{
			foreach (DataFieldDefinition expression in page.Expressions)
			{
				if (expression.TableNo != 0 && expression.FieldNo != 0)
				{
					GetMetadataForTableAndAddToCollection(expression.TableNo, pageDependencyResult, tablesToSkip);
				}
			}
		}
		return pageDependencyResult;
	}

	private void GetMetadataForPageAndAddToCollection(int pageId, DataSourceType dataSourceType, string personalizationId, bool applyPersonalization, PageDependencyResult metadataCollection)
	{
		if (!metadataCollection.MasterPages.Exists((PageResponseDependencyEntry entry) => entry.MasterPage.ID == pageId && entry.PersonalizationId == personalizationId))
		{
			PageResponseDependencyEntry metadataForPage = GetMetadataForPage(pageId, dataSourceType, personalizationId, applyPersonalization, forceElementRemovalOptionIfApplicable: true);
			metadataCollection.MasterPages.Add(metadataForPage);
		}
	}

	private void GetMetadataForTableAndAddToCollection(int tableId, PageDependencyResult metadataCollection, ICollection<int> tablesToSkip)
	{
		if (!tablesToSkip.Contains(tableId) && !metadataCollection.MetaTables.Exists((MetaTable entry) => entry.Id == tableId))
		{
			MetaTable tableMetadata = GetTableMetadata(tableId);
			metadataCollection.MetaTables.Add(tableMetadata);
		}
	}

	private static List<int> GetVisibleInfoPartIds(MasterPage page)
	{
		List<int> list = new List<int>();
		List<ElementDefinition> list2 = page.FindAll((ElementDefinition element) => element is InfopartPageDefinition);
		foreach (InfopartPageDefinition item in list2)
		{
			switch (item.Visible.ToUpperInvariant().Trim())
			{
			case "NO":
			case "FALSE":
			case "0":
				continue;
			}
			if (!list.Contains(item.PagePartID))
			{
				list.Add(item.PagePartID);
			}
		}
		return list;
	}
}
