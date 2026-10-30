using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaForm : NCLMetaApplicationObject
{
	[Flags]
	private enum PageTriggers
	{
		OnInit = 1,
		OnOpenPage = 2,
		OnClosePage = 4,
		OnFindRecord = 8,
		OnNextRecord = 0x10,
		OnAfterGetRecord = 0x20,
		OnNewRecord = 0x40,
		OnInsertRecord = 0x80,
		OnModifyRecord = 0x100,
		OnDeleteRecord = 0x200,
		OnQueryClosePage = 0x400,
		OnAfterGetCurrRecord = 0x800,
		Unknown = 0x1000
	}

	private delegate NavForm CreateNavForm(ITreeObject parent, NavRecord record);

	private class PageLoadResult
	{
		public PageDefinition PageDef { get; set; }

		public List<NCLPageExtension> Extensions { get; set; }

		public long Timestamp { get; set; }
	}

	private readonly Dictionary<int, MetadataExtension<PageDefinition>> customizedPageDefinitionsByAppGroup = new Dictionary<int, MetadataExtension<PageDefinition>>();

	private readonly object pageDefinitionLookupLockObj = new object();

	private List<NCLPageExtension> orderedExtensionObjects = new List<NCLPageExtension>();

	private List<NCLMetaApplicationObject> orderedExtensionObjectsBaseType = new List<NCLMetaApplicationObject>();

	private ReadOnlyCollection<string> apiVersionList;

	private NavPageTriggerEventHandler pageTriggerEventHandler;

	private MetadataExtension<PageDefinition> metadataAppGroupPageDefinition;

	private PageTriggers definedTriggers = PageTriggers.Unknown;

	protected override Type ApplicationObjectConstructorDelegateType => typeof(CreateNavForm);

	public override IList<NCLMetaApplicationObject> ExtensionObjects => orderedExtensionObjectsBaseType;

	public int FormId => ObjectId;

	public string Name => metadataAppGroupPageDefinition.Item.Name;

	public string CaptionML => metadataAppGroupPageDefinition.Item.CaptionML;

	public string Caption
	{
		get
		{
			string languageValue = MultiLanguage.GetLanguageValue(NavCurrentThread.Session.AbbreviatedLanguageName, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.LCID)), CaptionML);
			if (!string.IsNullOrEmpty(languageValue))
			{
				return languageValue;
			}
			return Name;
		}
	}

	public string ApiPublisher => metadataAppGroupPageDefinition.Item.Properties.APIPublisher;

	public string ApiGroup => metadataAppGroupPageDefinition.Item.Properties.APIGroup;

	public ReadOnlyCollection<string> ApiVersionList => apiVersionList;

	public string EntityName => metadataAppGroupPageDefinition.Item.Properties.EntityName;

	public string EntitySetName => metadataAppGroupPageDefinition.Item.Properties.EntitySetName;

	public PageType PageType => metadataAppGroupPageDefinition.Item.Properties.PageType;

	public int SourceTable => metadataAppGroupPageDefinition.Item.Properties.SourceObject?.SourceTable ?? 0;

	public bool SourceTableTemporary => metadataAppGroupPageDefinition.Item.Properties.SourceObject?.SourceTableTemporary ?? false;

	public ViewDefinition SourceTableView => metadataAppGroupPageDefinition.Item.Properties.SourceObject?.SourceTableView;

	public string ODataKeyFields => metadataAppGroupPageDefinition.Item.Properties.SourceObject?.ODataKeyFields ?? string.Empty;

	public IEnumerable<ControlDataboundDefinition> Fields
	{
		get
		{
			PageDefinition pageDefinition = GetPageDefinitionWithExtensions(MetadataAppGroup.GroupId)?.Item;
			if (pageDefinition == null)
			{
				return Enumerable.Empty<ControlDataboundDefinition>();
			}
			return pageDefinition.FindAll((ElementDefinition e) => e is ControlDataboundDefinition).Cast<ControlDataboundDefinition>();
		}
	}

	public IEnumerable<ActionDefinition> Actions
	{
		get
		{
			PageDefinition pageDefinition = GetPageDefinitionWithExtensions(MetadataAppGroup.GroupId)?.Item;
			if (pageDefinition == null)
			{
				return Enumerable.Empty<ActionDefinition>();
			}
			return pageDefinition.FindAll((ElementDefinition e) => e is ActionDefinition).Cast<ActionDefinition>();
		}
	}

	internal NavPageTriggerEventHandler PageTriggerEventHandler => pageTriggerEventHandler;

	internal override NavTriggerEventHandler TriggerEventHandler => pageTriggerEventHandler;

	public bool IsInitTriggerDefined => (DefinedTriggers & PageTriggers.OnInit) != 0;

	public bool IsOpenPageTriggerDefined => (DefinedTriggers & PageTriggers.OnOpenPage) != 0;

	public bool IsClosePageTriggerDefined => (DefinedTriggers & PageTriggers.OnClosePage) != 0;

	public bool IsFindRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnFindRecord) != 0;

	public bool IsNextRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnNextRecord) != 0;

	public bool IsAfterGetRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnAfterGetRecord) != 0;

	public bool IsNewRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnNewRecord) != 0;

	public bool IsInsertRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnInsertRecord) != 0;

	public bool IsModifyRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnModifyRecord) != 0;

	public bool IsDeleteRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnDeleteRecord) != 0;

	public bool IsQueryClosePageTriggerDefined => (DefinedTriggers & PageTriggers.OnQueryClosePage) != 0;

	public bool IsAfterGetCurrRecordTriggerDefined => (DefinedTriggers & PageTriggers.OnAfterGetCurrRecord) != 0;

	private PageTriggers DefinedTriggers
	{
		get
		{
			if (definedTriggers == PageTriggers.Unknown)
			{
				PageTriggers pageTriggers = (PageTriggers)0;
				PageTriggers[] array = (PageTriggers[])Enum.GetValues(typeof(PageTriggers));
				for (int i = 0; i < array.Length; i++)
				{
					PageTriggers pageTriggers2 = array[i];
					if (pageTriggers2 == PageTriggers.Unknown)
					{
						continue;
					}
					string triggerName = pageTriggers2.ToString();
					pageTriggers |= (IsTriggerImplemented<NavForm>(triggerName, isPublic: false) ? pageTriggers2 : ((PageTriggers)0));
					if (pageTriggers2 == PageTriggers.OnFindRecord || pageTriggers2 == PageTriggers.OnNextRecord || pageTriggers2 == PageTriggers.OnInit)
					{
						continue;
					}
					foreach (NCLPageExtension orderedExtensionObject in orderedExtensionObjects)
					{
						if ((pageTriggers & pageTriggers2) == 0 && orderedExtensionObject.IsTriggerImplemented<NavFormExtension>(triggerName, isPublic: true))
						{
							pageTriggers |= pageTriggers2;
						}
					}
				}
				definedTriggers = pageTriggers;
			}
			return definedTriggers;
		}
	}

	private NCLMetaForm(INCLMetaApplicationObjectLoader loader, int formId, NavAppGroup metadataAppGroup, int appDependencyOrder)
		: base(loader, ObjectType.Page, formId, metadataAppGroup, appDependencyOrder)
	{
	}

	internal static NCLMetaForm CreateEmptyNCLMetaForm(INCLMetaApplicationObjectLoader loader, int formId, NavAppGroup appGroup, int appDependencyOrder = -1)
	{
		return new NCLMetaForm(loader, formId, appGroup, appDependencyOrder);
	}

	public NavForm CreateObjectInstance()
	{
		return CreateObjectInstance((NavRecord)null);
	}

	public NavForm CreateObjectInstance(NavRecord record)
	{
		CreateNavForm createNavForm = (CreateNavForm)ApplicationObjectConstructor;
		NavForm navForm = createNavForm(NavCurrentThread.Session.Company.SharedObjects, record);
		foreach (NCLPageExtension orderedExtensionObject in orderedExtensionObjects)
		{
			orderedExtensionObject.CreateExtensionInstanceAndBindToParent(navForm, record);
		}
		navForm.CallOnMetadataLoadedExtensionMethod();
		return navForm;
	}

	public NavForm CreateObjectInstance(string personalizationId)
	{
		string formPersonalizationId = NavCurrentThread.FormPersonalizationId;
		try
		{
			NavCurrentThread.FormPersonalizationId = personalizationId;
			return CreateObjectInstance((NavRecord)null);
		}
		finally
		{
			NavCurrentThread.FormPersonalizationId = formPersonalizationId;
		}
	}

	protected override void LoadMetadata()
	{
		base.LoadMetadata();
		PageLoadResult pageLoadResult = LoadPageMetadata();
		orderedExtensionObjects = pageLoadResult.Extensions;
		orderedExtensionObjectsBaseType = pageLoadResult.Extensions.Cast<NCLMetaApplicationObject>().ToList();
		metadataAppGroupPageDefinition = new MetadataExtension<PageDefinition>(pageLoadResult.PageDef);
		apiVersionList = WebServiceHelper.ParseApiVersion(metadataAppGroupPageDefinition.Item.Properties.APIVersion, Name, ObjectId, ApiPublisher, ApiGroup);
		Timestamp = pageLoadResult.Timestamp;
	}

	public override void ValidateMetadata()
	{
		base.ValidateMetadata();
		LoadPageMetadata();
	}

	private static void EnsureAppChangesHaveNotMadeFatalChangesToMetaPage(PageDefinition page)
	{
		EnsureNoControlIdAppearsMoreThanOnce(page);
		EnsureNoExpressionNameAppearsMoreThanOnce(page);
	}

	private static void EnsureNoControlIdAppearsMoreThanOnce(PageDefinition page)
	{
		Dictionary<int, string> ids = new Dictionary<int, string>();
		foreach (ControlContainerDefinition container in page.Content.Containers)
		{
			foreach (ControlBaseDefinition control in container.Controls)
			{
				EnsureNoControlIdAppearsMoreThanOnce(page, control, ids);
			}
		}
	}

	private static void EnsureNoControlIdAppearsMoreThanOnce(PageDefinition page, ControlBaseDefinition control, IDictionary<int, string> ids)
	{
		if (ids.TryGetValue(control.ID, out var value))
		{
			string message = string.Format(CultureInfo.InvariantCulture, "The controls '{1}' and '{2}' on page '{3}' have the same ID: '{0}'. A control ID can only appear once on a page.", control.ID, control.Name, value, page.Name);
			throw NavAppObjectMetadataException.Create(message);
		}
		ids.Add(control.ID, control.Name);
		List<ControlBaseDefinition> list = null;
		if (control is ControlContainerPlaceHolder controlContainerPlaceHolder)
		{
			list = controlContainerPlaceHolder.Controls;
		}
		else if (control is ControlGroupBaseDefinition controlGroupBaseDefinition)
		{
			list = controlGroupBaseDefinition.Controls;
		}
		if (list == null)
		{
			return;
		}
		foreach (ControlBaseDefinition item in list)
		{
			EnsureNoControlIdAppearsMoreThanOnce(page, item, ids);
		}
	}

	private static void EnsureNoExpressionNameAppearsMoreThanOnce(PageDefinition page)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (DataFieldDefinition expression in page.Expressions)
		{
			if (hashSet.Contains(expression.Name))
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The expression name '{0}' can only appear once in the page '{1}'.", expression.Name, page.Name);
				throw NavAppObjectMetadataException.Create(message);
			}
			hashSet.Add(expression.Name);
		}
	}

	public PageDefinition GetBasePageDefinition(int tenantAppGroupId)
	{
		return ObjectLoader.XmlMetadataLoader.GetMetaPage(ObjectId, MetadataAppGroup, tenantAppGroupId).Value;
	}

	public MetadataExtension<PageDefinition> GetPageDefinitionWithExtensions(int tenantAppGroupId)
	{
		bool flag = MetadataAppGroup.GroupId == tenantAppGroupId;
		bool flag2 = orderedExtensionObjects.Any();
		bool flag3 = LanguageProvider.Provider.HasAppDataFor(ObjectType.Page, ObjectId, tenantAppGroupId);
		MetadataExtension<PageDefinition> value;
		if (!flag && (flag3 | flag2))
		{
			lock (pageDefinitionLookupLockObj)
			{
				if (!customizedPageDefinitionsByAppGroup.TryGetValue(tenantAppGroupId, out value))
				{
					PageDefinition value2 = ObjectLoader.XmlMetadataLoader.GetMetaPage(ObjectId, MetadataAppGroup, tenantAppGroupId).Value;
					value = new MetadataExtension<PageDefinition>(ApplyAppGroupAwareEnumMetadataToPageExpressions(ApplyPageExtensions(value2, orderedExtensionObjects)));
					customizedPageDefinitionsByAppGroup.Add(tenantAppGroupId, value);
				}
			}
		}
		else
		{
			value = metadataAppGroupPageDefinition;
		}
		return value;
	}

	internal static PageDefinition ApplyAppGroupAwareEnumMetadataToPageExpressions(PageDefinition pageDefinition)
	{
		for (int i = 0; i < pageDefinition.Expressions.Count; i++)
		{
			DataFieldDefinition dataFieldDefinition = pageDefinition.Expressions[i];
			if (dataFieldDefinition.EnumIdSpecified)
			{
				if (!NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Enum, dataFieldDefinition.EnumId, out var metaApplicationObject, -1, requireCompiled: false))
				{
					throw new NavMetadataNotFoundException(ObjectType.Enum, dataFieldDefinition.EnumId);
				}
				NCLOptionMetadata nCLEnumMetadata = ((NCLMetaEnum)metaApplicationObject).GetNCLEnumMetadata();
				dataFieldDefinition.OptionString = nCLEnumMetadata.OptionString;
				dataFieldDefinition.OptionCaption = nCLEnumMetadata.OptionCaption;
				dataFieldDefinition.OptionValues = string.Join(','.ToString(), nCLEnumMetadata.OrdinalValues.Select((int v) => v.ToString(CultureInfo.InvariantCulture)));
			}
		}
		return pageDefinition;
	}

	private PageDefinition ApplyPageExtensions(PageDefinition definition, IEnumerable<NCLPageExtension> extensions)
	{
		if (definition != null && extensions != null)
		{
			ApplyExtensionObjects(extensions, definition);
		}
		return definition;
	}

	internal override void CreateAndSetTriggerEventHandler()
	{
		pageTriggerEventHandler = new NavPageTriggerEventHandler(this);
	}

	private PageLoadResult LoadPageMetadata()
	{
		base.LoadMetadata();
		TimestampedObject<PageDefinition> metaPage = ObjectLoader.XmlMetadataLoader.GetMetaPage(ObjectId, MetadataAppGroup, MetadataAppGroup.GroupId);
		List<NCLPageExtension> extensions = GetExtensionObjects<NCLPageExtension>(MetadataAppGroup).ToList();
		ApplyPageExtensions(metaPage.Value, extensions);
		EnsureAppChangesHaveNotMadeFatalChangesToMetaPage(metaPage.Value);
		return new PageLoadResult
		{
			PageDef = metaPage.Value,
			Extensions = extensions,
			Timestamp = metaPage.Timestamp
		};
	}
}
