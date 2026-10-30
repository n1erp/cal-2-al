using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal abstract class NavDesignerExtension
{
	internal const int DefaultRangeStart = 50000;

	internal const int DefaultRangeEnd = 99000;

	private readonly Dictionary<int, IEnumerable<NavAppObjectMetadataRuntimeDeltas>> pageExtensionDeltasFromInstalledExtensions = new Dictionary<int, IEnumerable<NavAppObjectMetadataRuntimeDeltas>>();

	protected INavPageCustomizationRepository pageCustomizationRepository;

	public virtual NavAppManifest Manifest { get; set; }

	public NavAppPackageType PackageType { get; set; }

	protected string DesignerExtensionPublisher { get; } = "Designer";

	public NavDesignerObjectCollection Objects { get; protected set; } = new NavDesignerObjectCollection();

	public IDictionary<string, byte[]> PackageFiles { get; protected set; }

	public IEnumerable<NavAppObjectMetadata> ObjectMetadata { get; protected set; }

	protected INavDesignerCompiler Compiler { get; }

	internal NavDesignerExtension(INavDesignerCompiler compiler, INavPageCustomizationRepository pageCustomizationRepository = null)
	{
		Compiler = compiler;
		this.pageCustomizationRepository = pageCustomizationRepository;
	}

	public abstract void Save();

	protected void SetExtensionProperties()
	{
		AppId val = AppId.op_Implicit(Guid.NewGuid());
		string appName = "Designer_" + ((object)val).ToString();
		Manifest = new NavAppManifest
		{
			AppName = appName,
			AppPublisher = DesignerExtensionPublisher,
			AppId = ((WrappedValue<Guid>)(object)val).Value,
			AppVersion = new Version(1, 0, 0, 0),
			AppCompatibilityId = new Version(1, 0, 0, 0),
			AppBrief = string.Empty,
			AppDescription = string.Empty,
			AppEula = string.Empty,
			AppHelp = string.Empty,
			AppLogo = string.Empty,
			AppPrivacyStatement = string.Empty,
			AppScreenshots = new List<string>(),
			AppUrl = string.Empty,
			Dependencies = NavDesignerUtil.GetAllInstalledV2ExtensionsAsDependencies(),
			IdSpaces = new IdSpaces(new IdSpace[1]
			{
				new IdSpace(50000, 99000)
			}),
			ShowMyCode = true
		};
		Manifest = Manifest.WithSystemReferences();
	}

	public PageDefinition GetPageDefinition(int pageId, string viewName = null)
	{
		PageDefinition designerBasePageDefinition = GetDesignerBasePageDefinition(pageId);
		ApplyRuntimeDeltasToPageDefinition(new ApplicationObjectId(ObjectType.Page, pageId), designerBasePageDefinition, viewName);
		ApplyViewDeltas(designerBasePageDefinition, viewName);
		return designerBasePageDefinition;
	}

	protected abstract void ApplyRuntimeDeltasToPageDefinition(ApplicationObjectId objectId, PageDefinition pageDefinition, string viewName);

	protected static void ApplyDeltas(ApplicationObjectId objectId, PageDefinition pageDefinition, IEnumerable<NavAppObjectMetadataRuntimeDeltas> deltas)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected Obj, but got Unknown
		if (deltas != null && deltas.Any())
		{
			MetadataRuntimeDeltaApplicator.GetSharedInstanceWithAllAdapters(ObjectType.PageExtension).ApplyDeltas(new DeltaApplicationContext
			{
				SubjectId = objectId,
				Subject = pageDefinition,
				Deltas = deltas
			}, Array.Empty<IDeltaValidationFilter>());
		}
	}

	protected IEnumerable<NavAppObjectMetadataRuntimeDeltas> GetPageExtensionObjectMetadataForObject(ApplicationObjectId objectId)
	{
		return GetPageExtensionObjectMetadataForObjectBase(objectId, null, null);
	}

	protected IEnumerable<NavAppObjectMetadataRuntimeDeltas> GetPageExtensionObjectMetadataForObject(ApplicationObjectId objectId, out HashSet<string> viewsToFlag)
	{
		viewsToFlag = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		return GetPageExtensionObjectMetadataForObjectBase(objectId, viewsToFlag, AddViewToListToFlag);
	}

	private void AddViewToListToFlag(AppId appId, NavAppObjectMetadataRuntimeDeltas extensionDelta, HashSet<string> viewsToFlag)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		if (!((WrappedComparable<Guid>)(object)appId != (WrappedComparable<Guid>)null) || !(((WrappedValue<Guid>)(object)appId).Value == Manifest.AppId))
		{
			return;
		}
		foreach (Delta allDelta in ((NavAppObjectMetadataDeltaCreator<Delta>)(object)extensionDelta).AllDeltas)
		{
			if ((int)allDelta.TargetKind == 2)
			{
				ActionAddDelta val = (ActionAddDelta)(object)((allDelta is ActionAddDelta) ? allDelta : null);
				if (val != null)
				{
					viewsToFlag.Add(((ContentAddDelta<ActionBaseDefinition>)(object)val).Context.Content.Name);
				}
			}
		}
	}

	private IEnumerable<NavAppObjectMetadataRuntimeDeltas> GetPageExtensionObjectMetadataForObjectBase(ApplicationObjectId objectId, HashSet<string> viewsToFlag, Action<AppId, NavAppObjectMetadataRuntimeDeltas, HashSet<string>> collectViewsInScope)
	{
		if (objectId.ObjectType != ObjectType.Page)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "The method parameter '{0}' is not valid. It must be of type '{1}'", "objectId", ObjectType.Page));
		}
		if (pageExtensionDeltasFromInstalledExtensions.ContainsKey(objectId.ObjectNumber))
		{
			return pageExtensionDeltasFromInstalledExtensions[objectId.ObjectNumber];
		}
		List<NavAppObjectMetadataRuntimeDeltas> list = new List<NavAppObjectMetadataRuntimeDeltas>();
		IEnumerable<SpecificNavAppObjectMetadataSummaryPair> extensionObjects = GetExtensionObjects(objectId);
		foreach (SpecificNavAppObjectMetadataSummaryPair item in extensionObjects)
		{
			NavAppObjectMetadataBlob objectMetadataBlob = NavEnvironment.Instance.NavAppMetadataRetriever.GetObjectMetadataBlob(new ApplicationObjectId(item.Summary.ObjectType, item.Summary.ObjectId), item.Summary.AppPackageId, (NavAppObjectMetadataBlobType)0);
			NavAppObjectMetadataRuntimeDeltas val = NavAppObjectMetadataRuntimeDeltas.FromXml(XDocument.Parse(objectMetadataBlob.Content));
			collectViewsInScope?.Invoke(item.RuntimeMetadata.AppId, val, viewsToFlag);
			list.Add(val);
		}
		pageExtensionDeltasFromInstalledExtensions[objectId.ObjectNumber] = list;
		return pageExtensionDeltasFromInstalledExtensions[objectId.ObjectNumber];
	}

	protected void ClearPageExtensionDeltasFromInstalledExtensionsCache()
	{
		pageExtensionDeltasFromInstalledExtensions.Clear();
	}

	internal abstract void SaveExtensionData();

	public abstract INavDesignerPageOperations GetPageOperationInterfaceForPage(int pageId);

	public abstract NavDesignerPageModifications GetPageModifications(int pageId);

	public abstract NavDesignerObject GetDesignerPageObject(int pageId, bool createIfNotFound);

	private PageDefinition GetDesignerBasePageDefinition(int pageId)
	{
		var (pageDefinition, val) = NavCurrentThread.Session.MetadataProvider.GetBasePageDefinition(pageId);
		if ((WrappedComparable<Guid>)(object)((val != null) ? val.AppId : null) != (WrappedComparable<Guid>)null && Manifest.AppId == ((WrappedValue<Guid>)(object)val.AppId).Value)
		{
			MetadataProvider.SetFlagOnViewsInUserScope(pageDefinition, null);
		}
		if (PackageType == NavAppPackageType.Extension && (WrappedComparable<Guid>)(object)((val != null) ? val.AppId : null) != (WrappedComparable<Guid>)null && !GetAllowedAppPackageIds().Contains(((WrappedValue<Guid>)(object)val.AppId).Value))
		{
			throw new NavMetadataNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.PageNotInDesignerExtensionDependencies, pageId, val.Name, val.Publisher, Manifest.AppName));
		}
		return pageDefinition;
	}

	private static PageDefinition ApplyViewDeltas(PageDefinition pageDefinition, string viewName)
	{
		if (!string.IsNullOrEmpty(viewName))
		{
			return MetadataProvider.ApplyPageViewCustomizations(pageDefinition, viewName, applyPersonalization: false);
		}
		return pageDefinition;
	}

	private IEnumerable<SpecificNavAppObjectMetadataSummaryPair> GetExtensionObjects(ApplicationObjectId objectId)
	{
		if (PackageType == NavAppPackageType.Extension)
		{
			HashSet<Guid> allowedAppPackageIds = GetAllowedAppPackageIds();
			return from o in NavCurrentThread.ResolveAppGroup().GetExtensionObjects()
				where allowedAppPackageIds.Contains(((WrappedValue<Guid>)(object)o.RuntimeMetadata.AppId).Value) && o.Summary.TargetObjectType == objectId.ObjectType && o.Summary.TargetObjectId == objectId.ObjectNumber
				select o;
		}
		return from o in NavCurrentThread.ResolveAppGroup().GetExtensionObjects()
			where o.Summary.TargetObjectType == objectId.ObjectType && o.Summary.TargetObjectId == objectId.ObjectNumber
			select o;
	}

	private HashSet<Guid> GetAllowedAppPackageIds()
	{
		HashSet<Guid> hashSet = new HashSet<Guid>(from x in Manifest.GetAllReferences()
			select x.AppId);
		hashSet.Add(Manifest.AppId);
		return hashSet;
	}
}
