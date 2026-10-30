using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NCLMetaApplicationObject
{
	[Flags]
	public enum StateTypes
	{
		MetadataLoaded = 2,
		ClrTypeLoaded = 4,
		Stale = 8,
		RemovedFromCache = 0x10
	}

	private const StateTypes PopulatedAndStaleFlags = StateTypes.MetadataLoaded | StateTypes.Stale;

	public const int LastReleaseCompatibleMetadataVersion = 70008;

	public const int CurrentMetadataVersion = 130000;

	public static readonly IEqualityComparer<NCLMetaApplicationObject> AppGroupAndApplicationObjectIdComparer = new GroupAndApplicationObjectIdComparer();

	private readonly int objectId;

	private readonly int appDependencyOrder;

	private readonly NavAppGroup metadataAppGroup;

	private NavAppRuntimeMetadata owningApp;

	private readonly Lazy<NavEventPublisherReflectionWrapper> publisherReflectionWrapper;

	private StateTypes state;

	private Delegate applicationObjectConstructor;

	private bool applicationObjectLegacyConstructor;

	private Type applicationObjectClrType;

	private static readonly ReadOnlyCollection<NCLMetaApplicationObject> EmptyExtensionObjectList = new ReadOnlyCollection<NCLMetaApplicationObject>(new List<NCLMetaApplicationObject>());

	public long Timestamp { get; protected set; }

	public MethodNumberCounter MethodNumberCounter { get; internal set; } = new MethodNumberCounter();

	public virtual IList<NCLMetaApplicationObject> ExtensionObjects => EmptyExtensionObjectList;

	public ObjectType ApplicationObjectType { get; }

	protected INCLMetaApplicationObjectLoader ObjectLoader { get; }

	internal bool MetadataLoaded => (state & StateTypes.MetadataLoaded) != 0;

	internal bool PopulatedAndStale => (state & (StateTypes.MetadataLoaded | StateTypes.Stale)) == (StateTypes.MetadataLoaded | StateTypes.Stale);

	internal bool Stale
	{
		get
		{
			return (state & StateTypes.Stale) == StateTypes.Stale;
		}
		set
		{
			if (value)
			{
				lock (this)
				{
					state |= StateTypes.Stale;
					return;
				}
			}
			throw new InvalidOperationException("NCLMetaApplicationObject.Stale can only be set to true.");
		}
	}

	internal bool ClrTypeLoaded => (state & StateTypes.ClrTypeLoaded) == StateTypes.ClrTypeLoaded;

	internal bool RemovedFromCache
	{
		get
		{
			return (state & StateTypes.RemovedFromCache) == StateTypes.RemovedFromCache;
		}
		set
		{
			if (value)
			{
				lock (this)
				{
					state |= StateTypes.RemovedFromCache;
					return;
				}
			}
			throw new InvalidOperationException("NCLMetaApplicationObject.RemovedFromCache can only be set to true.");
		}
	}

	public Type ApplicationObjectClrType
	{
		get
		{
			if (!ClrTypeLoaded)
			{
				lock (this)
				{
					CompileAndLoadClrObject();
				}
			}
			return applicationObjectClrType;
		}
	}

	public int ObjectId => objectId;

	public int AppDependencyOrder => appDependencyOrder;

	public NavAppGroup MetadataAppGroup => metadataAppGroup;

	public NavAppRuntimeMetadata OwningApp
	{
		get
		{
			if (MetadataAppGroup.GroupId != 0 && owningApp == null)
			{
				owningApp = ResolveOwningApp();
			}
			return owningApp;
		}
	}

	public ApplicationObjectId ApplicationObjectId => new ApplicationObjectId(ApplicationObjectType, objectId);

	internal virtual NavTriggerEventHandler TriggerEventHandler => null;

	public IEnumerable<ByRef<NavEventScope>> EventScopeReferences
	{
		get
		{
			Type type = ApplicationObjectClrType;
			if (type != null)
			{
				return from t in type.GetNestedTypes(BindingFlags.NonPublic)
					select t.GetField("NavEventScope", BindingFlags.Static) into fi
					where fi != null && typeof(NavEventScope).IsAssignableFrom(fi.FieldType)
					select new ByRef<NavEventScope>(fi, null);
			}
			return Enumerable.Empty<ByRef<NavEventScope>>();
		}
	}

	protected virtual Type ApplicationObjectConstructorDelegateType => null;

	protected virtual Type ApplicationObjectLegacyConstructorDelegateType => null;

	protected Delegate ApplicationObjectConstructor
	{
		get
		{
			if (!ClrTypeLoaded)
			{
				lock (this)
				{
					CompileAndLoadClrObject();
				}
			}
			return applicationObjectConstructor;
		}
	}

	protected bool ApplicationObjectLegacyConstructor => applicationObjectLegacyConstructor;

	public NavEventPublisherReflectionWrapper PublisherReflectionWrapper => publisherReflectionWrapper.Value;

	protected NCLMetaApplicationObject(INCLMetaApplicationObjectLoader loader, ObjectType objectType, int objectId, NavAppGroup metadataAppGroup, int appDependencyOrder)
	{
		ObjectLoader = loader;
		this.objectId = objectId;
		this.appDependencyOrder = appDependencyOrder;
		ApplicationObjectType = objectType;
		this.metadataAppGroup = metadataAppGroup;
		owningApp = this.metadataAppGroup.GetObjectOwner(ApplicationObjectType, objectId);
		publisherReflectionWrapper = new Lazy<NavEventPublisherReflectionWrapper>(() => new NavEventPublisherReflectionWrapper(ApplicationObjectClrType), LazyThreadSafetyMode.PublicationOnly);
	}

	internal void Populate()
	{
		if (!MetadataLoaded)
		{
			if (MetadataAppGroup != NavAppGroup.BaseGroup && NavCurrentThread.ResolveAppGroup() == NavAppGroup.BaseGroup)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007YP", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Object metadata load attempted with no thread app group context defined -- Type:{0}; Id:{1}; MetadataGroup:{2}{3}{4}", ApplicationObjectType, ObjectId, MetadataAppGroup.GroupId, Environment.NewLine, new StackTrace());
			}
			LoadMetadata();
			state |= StateTypes.MetadataLoaded;
		}
	}

	protected virtual void LoadMetadata()
	{
	}

	public virtual void ValidateMetadata()
	{
	}

	protected virtual NCLObjectXmlMetadata GetMetadataFromLoader()
	{
		return ObjectLoader.XmlMetadataLoader.GetMetaObjectAsXmlDocument(ApplicationObjectId, MetadataAppGroup);
	}

	protected virtual Type LoadClrType()
	{
		if (OwningApp == null)
		{
			return ObjectLoader.CodeLoader.GetBaseObjectClrType(ApplicationObjectType, ObjectId);
		}
		return ObjectLoader.AppClrTypeRetriever.GetClrType(OwningApp.PackageId, ApplicationObjectId);
	}

	protected virtual void OnClrTypeLoaded()
	{
	}

	private Delegate CreateConstructorDelegate(Type objectClrType)
	{
		if (objectClrType == null)
		{
			return null;
		}
		Type applicationObjectConstructorDelegateType = ApplicationObjectConstructorDelegateType;
		Type applicationObjectLegacyConstructorDelegateType = ApplicationObjectLegacyConstructorDelegateType;
		if (applicationObjectConstructorDelegateType == null)
		{
			return null;
		}
		return NCLCodeLoader.CreateDelegateToConstructor(applicationObjectConstructorDelegateType, applicationObjectLegacyConstructorDelegateType, objectClrType, out applicationObjectLegacyConstructor);
	}

	protected IEnumerable<T> GetExtensionObjects<T>(NavAppGroup appGroup) where T : NCLApplicationObjectExtension
	{
		List<T> result = new List<T>();
		NavAppGroup group = appGroup ?? NavCurrentThread.ResolveAppGroup();
		if (group != NavAppGroup.BaseGroup && ObjectLoader.MetadataCache != null && ObjectLoader.MetadataCache.TryGetExtensionApplicationObjects(out var extensionObjects, ApplicationObjectId, group.GroupId))
		{
			result = (from T r in extensionObjects
				orderby @group.GetAppDependencyOrderForExtensionObject(r.RuntimeAppId)
				select r).ToList();
		}
		return result;
	}

	internal virtual bool CheckIsStale()
	{
		return false;
	}

	protected virtual NavAppRuntimeMetadata ResolveOwningApp()
	{
		return MetadataAppGroup.GetObjectOwner(ApplicationObjectType, objectId);
	}

	public override int GetHashCode()
	{
		return ((int)ApplicationObjectType << 27) ^ ObjectId;
	}

	public static void VerifyMetadataVersion(int objectId, ObjectType navObjectType, int version)
	{
		if (version != 130000 && version != 70008 && (navObjectType != ObjectType.Table || (!NavRecord.IsTableVirtual(objectId) && objectId != 2000000001)))
		{
			throw new NavNCLMetadataVersionMismatchException(CultureInfo.CurrentCulture, navObjectType, objectId);
		}
	}

	protected void ApplyExtensionObjects<T>(IEnumerable<INCLApplicationObject<T>> extensions, T subject)
	{
		foreach (INCLApplicationObject<T> extension in extensions)
		{
			extension.ApplyRuntimeDeltas(ApplicationObjectId, subject);
		}
	}

	public bool IsEventSubscribed(NavTriggerEventType eventType)
	{
		return TriggerEventHandler?.IsEventSubscribed(eventType) ?? false;
	}

	public bool IsEventSubscribed(NavTriggerEventIdentity eventIdentity)
	{
		return TriggerEventHandler?.IsEventSubscribed(eventIdentity) ?? false;
	}

	internal virtual void CreateAndSetTriggerEventHandler()
	{
		throw new InvalidOperationException();
	}

	internal NavTriggerEventHandler GetOrCreateTriggerEventHandler()
	{
		NavTriggerEventHandler triggerEventHandler = TriggerEventHandler;
		if (triggerEventHandler == null)
		{
			lock (this)
			{
				if (TriggerEventHandler == null)
				{
					CreateAndSetTriggerEventHandler();
				}
				triggerEventHandler = TriggerEventHandler;
			}
		}
		return triggerEventHandler;
	}

	internal NavEventScope GetEventScope(NavTriggerEventType triggerEventType, EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		NavTriggerEventHandler navTriggerEventHandler;
		if (getOption == EventScopeGetOption.CreateIfNotFound)
		{
			navTriggerEventHandler = GetOrCreateTriggerEventHandler();
		}
		else
		{
			navTriggerEventHandler = TriggerEventHandler;
			if (navTriggerEventHandler == null)
			{
				return null;
			}
		}
		return navTriggerEventHandler.GetEventScope(triggerEventType, getOption);
	}

	public void CheckApplicationObjectIsValid(NavApplicationObjectBase applicationObject)
	{
		if (applicationObject.IsStale)
		{
			throw new NavAdministratorMadeChangesException();
		}
		NavSession session = applicationObject.Session;
		NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup(session);
		if (applicationObject.RuntimeAppGroupId != navAppGroup.GroupId)
		{
			if (!session.NCLMetadata.TryGetMetaApplicationObject(ApplicationObjectId, out var metaApplicationObject, navAppGroup.GroupId) || this != metaApplicationObject)
			{
				applicationObject.IsStale = true;
				throw new NavAdministratorMadeChangesException();
			}
			applicationObject.RuntimeAppGroupId = navAppGroup.GroupId;
		}
	}

	public void SetClrObject(Type clrObject)
	{
		if (ClrTypeLoaded)
		{
			return;
		}
		lock (this)
		{
			if (!ClrTypeLoaded)
			{
				if (Stale)
				{
					throw new NavObjectDefinitionChangedException(CultureInfo.CurrentCulture, NavCurrentThread.Session.GetObjectName(ApplicationObjectId), ApplicationObjectType);
				}
				applicationObjectClrType = clrObject;
				applicationObjectConstructor = CreateConstructorDelegate(applicationObjectClrType);
				state |= StateTypes.ClrTypeLoaded;
				OnClrTypeLoaded();
			}
		}
	}

	internal void CompileAndLoadClrObject()
	{
		if (!ClrTypeLoaded)
		{
			if (Stale)
			{
				throw new NavObjectDefinitionChangedException(CultureInfo.CurrentCulture, NavCurrentThread.Session.GetObjectName(ApplicationObjectId), ApplicationObjectType);
			}
			applicationObjectClrType = LoadClrType();
			applicationObjectConstructor = CreateConstructorDelegate(applicationObjectClrType);
			state |= StateTypes.ClrTypeLoaded;
			OnClrTypeLoaded();
		}
	}

	internal void CopyClrType(NCLMetaApplicationObject other)
	{
		applicationObjectClrType = other.ApplicationObjectClrType;
		applicationObjectConstructor = other.applicationObjectConstructor;
		applicationObjectLegacyConstructor = other.applicationObjectLegacyConstructor;
		state |= StateTypes.ClrTypeLoaded;
	}

	internal bool IsTriggerImplemented<T>(string triggerName, bool isPublic)
	{
		Type type = ApplicationObjectClrType;
		if (type == null)
		{
			return false;
		}
		BindingFlags bindingFlags = (isPublic ? BindingFlags.Public : BindingFlags.NonPublic);
		MethodInfo method = type.GetMethod(triggerName, BindingFlags.Instance | bindingFlags);
		if (method == null)
		{
			throw new InvalidOperationException(triggerName + " missing on " + type.ToString());
		}
		return method.DeclaringType != typeof(T);
	}
}
