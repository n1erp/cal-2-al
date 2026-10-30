using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventSubscriptionMetadata : IDisposable
{
	private readonly List<NavEventExclusionFilter> dynamicExclusionFilters;

	private readonly List<NavEventExclusionFilter> staticExclusionFilters;

	private readonly List<NavEventSubscriptionSettingsEntry> staticSettingsEntries;

	private readonly List<NavEventSubscriptionSettingsEntry> dynamicSettingsEntries;

	private readonly List<NavEventErrorReporter> staticErrorReporters;

	private readonly List<NavEventErrorReporter> dynamicErrorReporters;

	private readonly Dictionary<Type, INavEventSubscriptionObserver> staticSubscriptionObservers;

	private readonly NavEventSubscriptionFactory subscriptionFactory;

	private readonly List<NavEventSubscription> eventSubscriptions = new List<NavEventSubscription>(128);

	private readonly Dictionary<int, NavAppGroup> initializedAppGroups = new Dictionary<int, NavAppGroup>();

	private readonly object syncLock = new object();

	private IEnumerable<NavEventExclusionFilter> AllExclusionFilters => staticExclusionFilters.Concat(dynamicExclusionFilters).ToList();

	private IEnumerable<NavEventSubscriptionSettingsEntry> AllSettingsEntries => staticSettingsEntries.Concat(dynamicSettingsEntries).ToList();

	private IEnumerable<NavEventErrorReporter> AllErrorReporters => staticErrorReporters.Concat(dynamicErrorReporters).ToList();

	static NavEventSubscriptionMetadata()
	{
		MetaTable.GetValidateEventSubscriptionsOnTable = GetValidateEventSubscriptionsOnTable;
		MasterPage.GetControlsAndActionsOnPage = GetControlsAndActionsOnPage;
	}

	private static IEnumerable<int> GetValidateEventSubscriptionsOnTable(int tableId, int appGroupId)
	{
		if (!NavGlobal.NCLMetadata.TryGetMetaTableById(tableId, out var metaTable, appGroupId))
		{
			return Enumerable.Empty<int>();
		}
		return from f in metaTable.NormalFields
			where (f.HasSubscribers && f.IsEventSubscribed(NavTriggerEventType.OnBeforeValidateEvent)) || f.IsEventSubscribed(NavTriggerEventType.OnAfterValidateEvent)
			select f.FieldNo;
	}

	private static void GetControlsAndActionsOnPage(int pageId, int appGroupId, out bool onAfterGetCurrentRecordEnabled, out ControlDataboundDefinition[] controls, out ActionDefinition[] actions)
	{
		if (SystemPages.IsServerSystemPageId(pageId) || !NavGlobal.NCLMetadata.TryGetMetaFormById(pageId, out var metaForm, appGroupId, requireCompiled: false))
		{
			onAfterGetCurrentRecordEnabled = false;
			controls = new ControlDataboundDefinition[0];
			actions = new ActionDefinition[0];
			return;
		}
		MetadataExtension<PageDefinition> pageDefinitionWithExtensions = metaForm.GetPageDefinitionWithExtensions(metaForm.MetadataAppGroup.GroupId);
		if (pageDefinitionWithExtensions.Item.Properties.SourceObject.SourceTable != 0)
		{
			NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.Table, pageDefinitionWithExtensions.Item.Properties.SourceObject.SourceTable, out var metaApplicationObject, appGroupId);
			if (metaApplicationObject != null)
			{
				IEnumerable<string> validateEventFieldIds = from x in GetValidateEventSubscriptionsOnTable(metaApplicationObject.ObjectId, appGroupId)
					select x.ToString(CultureInfo.CurrentCulture);
				foreach (ControlDataboundDefinition item in metaForm.Fields.Where((ControlDataboundDefinition field) => validateEventFieldIds.Contains(field.DataColumnName)))
				{
					item.HasOnValidateEventSubscriber = true;
				}
			}
		}
		onAfterGetCurrentRecordEnabled = pageDefinitionWithExtensions.Item.Properties.OnAfterGetCurrentRecordEnabled || metaForm.IsEventSubscribed(NavTriggerEventType.OnAfterGetCurrRecordEvent) || metaForm.IsAfterGetCurrRecordTriggerDefined;
		controls = metaForm.Fields.ToArray();
		actions = metaForm.Actions.ToArray();
	}

	internal NavEventSubscriptionMetadata(Func<ITenantSessionHandler> getSessionHandler, IEnumerable<NavEventExclusionFilter> staticExclusionFilters, IEnumerable<NavEventSubscriptionSettingsEntry> staticSettingsEntries, IEnumerable<NavEventErrorReporter> staticErrorReporters, IEnumerable<INavEventSubscriptionObserver> staticSubscriptionObservers)
	{
		if (getSessionHandler == null)
		{
			throw new ArgumentNullException("getSessionHandler");
		}
		this.staticExclusionFilters = staticExclusionFilters?.ToList() ?? new List<NavEventExclusionFilter>();
		dynamicExclusionFilters = new List<NavEventExclusionFilter>();
		this.staticSettingsEntries = staticSettingsEntries?.ToList() ?? new List<NavEventSubscriptionSettingsEntry>();
		dynamicSettingsEntries = new List<NavEventSubscriptionSettingsEntry>();
		this.staticErrorReporters = staticErrorReporters?.ToList() ?? new List<NavEventErrorReporter>();
		dynamicErrorReporters = new List<NavEventErrorReporter>();
		this.staticSubscriptionObservers = staticSubscriptionObservers?.ToDictionary((INavEventSubscriptionObserver o) => o.GetType()) ?? new Dictionary<Type, INavEventSubscriptionObserver>();
		subscriptionFactory = new NavEventSubscriptionFactory(getSessionHandler);
	}

	internal void InitializeBaseEvents()
	{
		if (NavEnvironment.Instance.StandaloneInstance || NavEnvironment.Instance.InEventsDisabledMode)
		{
			return;
		}
		lock (syncLock)
		{
			if (eventSubscriptions.Any())
			{
				return;
			}
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000PS", DiagnosticParameter.SystemMetadata("Initializing base events"), MonitorType.None, Category.EventSubscription, createNewActivity: false))
			{
				NavEventSubscriptionModifiers subscriptionModifiers = new NavEventSubscriptionModifiers(AllExclusionFilters, AllSettingsEntries, AllErrorReporters);
				IList<NavEventSubscription> list = subscriptionFactory.FindAndCreateSubscriptionInstances(NavAppGroup.BaseGroup, subscriptionModifiers);
				AddEventSubscriptions(list);
				BindSubscriptions(list);
			}
		}
	}

	public T GetSubscriptionObserverInstance<T>() where T : INavEventSubscriptionObserver
	{
		return staticSubscriptionObservers.Values.OfType<T>().Single();
	}

	public void AddDynamicExclusionFilter(NavEventExclusionFilter filter)
	{
		lock (syncLock)
		{
			dynamicExclusionFilters.Add(filter);
		}
	}

	public void RemoveDynamicExclusionFilter(NavEventExclusionFilter filter)
	{
		lock (syncLock)
		{
			dynamicExclusionFilters.Remove(filter);
		}
	}

	public void AddDynamicSettingsEntry(NavEventSubscriptionSettingsEntry settingsEntry)
	{
		lock (syncLock)
		{
			dynamicSettingsEntries.Add(settingsEntry);
		}
	}

	public void RemoveDynamicSettingsEntry(NavEventSubscriptionSettingsEntry settingsEntry)
	{
		lock (syncLock)
		{
			dynamicSettingsEntries.Remove(settingsEntry);
		}
	}

	public void AddDynamicErrorReporter(NavEventErrorReporter errorReporter)
	{
		lock (syncLock)
		{
			dynamicErrorReporters.Add(errorReporter);
		}
	}

	public void RemoveDynamicErrorReporter(NavEventErrorReporter errorReporter)
	{
		lock (syncLock)
		{
			dynamicErrorReporters.Remove(errorReporter);
		}
	}

	public void InitializeAppGroupEvents(NavAppGroup appGroup)
	{
		if (NavEnvironment.Instance.StandaloneInstance || NavEnvironment.Instance.InEventsDisabledMode)
		{
			return;
		}
		lock (syncLock)
		{
			if (initializedAppGroups.ContainsKey(appGroup.GroupId) || appGroup == NavAppGroup.BaseGroup)
			{
				return;
			}
			string content = string.Format(CultureInfo.InvariantCulture, "Initializing extension events -- Group:{0}", appGroup.GroupId);
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000PT", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.EventSubscription, createNewActivity: false))
			{
				NavEventSubscriptionModifiers subscriptionModifiers = new NavEventSubscriptionModifiers(AllExclusionFilters, AllSettingsEntries, AllErrorReporters);
				IList<NavEventSubscription> list = subscriptionFactory.FindAndCreateSubscriptionInstances(appGroup, subscriptionModifiers);
				AddEventSubscriptions(list);
				BindSubscriptions(list);
				initializedAppGroups.Add(appGroup.GroupId, appGroup);
			}
		}
	}

	private void BindSubscriptions(IEnumerable<NavEventSubscription> subscriptionsToBind)
	{
		if (subscriptionsToBind == null)
		{
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<NavEventSubscription> list = subscriptionsToBind.Where((NavEventSubscription es) => !es.HasError).ToList();
		foreach (NavEventSubscription item in list)
		{
			item.BindSubscription();
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PU", Category.EventSubscription, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Finished binding subscriptions -- Subscriptions:{0}; Elapsed:{1}", list.Count, stopwatch.ElapsedMilliseconds);
	}

	public void RemoveForNavAppGroup(NavAppGroup appGroup)
	{
		lock (syncLock)
		{
			if (!initializedAppGroups.ContainsKey(appGroup.GroupId))
			{
				return;
			}
			string content = string.Format(CultureInfo.InvariantCulture, "Removing extension events -- Group:{0}", appGroup.GroupId);
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000PV", DiagnosticParameter.SystemMetadata(content), MonitorType.None, Category.EventSubscription, createNewActivity: false))
			{
				initializedAppGroups.Remove(appGroup.GroupId);
				RebindAllEvents();
			}
		}
	}

	public void RebindEventSubscriptionsIfNeeded(Func<IEnumerable<ApplicationObjectId>> staleFunc)
	{
		IList<NavEventSubscription> subscriptionCopy = GetSubscriptionsCopy();
		List<ApplicationObjectId> source = new List<ApplicationObjectId>(staleFunc());
		if (source.Any((ApplicationObjectId to) => to.ObjectType == ObjectType.CodeUnit) || source.Any((ApplicationObjectId to) => subscriptionCopy.Any((NavEventSubscription subscription) => subscription.OriginalApplicationObjectId == to)))
		{
			subscriptionFactory.InvalidateBaseSubscriberCodeunitIdCache();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PW", Category.EventSubscription, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Triggering rebind from change listener");
			RebindAllEvents();
		}
	}

	private void RebindAllEvents()
	{
		lock (syncLock)
		{
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000PX", DiagnosticParameter.SystemMetadata("Rebinding all events"), MonitorType.None, Category.EventSubscription, createNewActivity: false))
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				List<NavEventSubscription> list = eventSubscriptions.ToList();
				List<NavEventSubscription> list2 = eventSubscriptions.Where((NavEventSubscription es) => !es.HasError).ToList();
				ClearEventSubscriptions();
				List<NavAppGroup> list3 = initializedAppGroups.Values.ToList();
				HashSet<PackageId> allPublishedPackageIds = (from m in NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata()
					select m.PackageId).ToHashSet();
				List<NavAppGroup> list4 = list3.Where((NavAppGroup g) => g.OrderedAppMetadata.All((NavAppRuntimeMetadata meta) => allPublishedPackageIds.Contains(meta.PackageId))).ToList();
				NavEventSubscriptionModifiers subscriptionModifiers = new NavEventSubscriptionModifiers(AllExclusionFilters, AllSettingsEntries, AllErrorReporters);
				IList<NavEventSubscription> subscriptions = subscriptionFactory.FindAndCreateSubscriptionInstances(NavAppGroup.BaseGroup, subscriptionModifiers);
				AddEventSubscriptions(subscriptions);
				foreach (NavAppGroup item in list4)
				{
					IList<NavEventSubscription> subscriptions2 = subscriptionFactory.FindAndCreateSubscriptionInstances(item, subscriptionModifiers);
					AddEventSubscriptions(subscriptions2);
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PY", Category.EventSubscription, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Rebind all -- OldSubscriptions:{0}; NewSubscriptions:{1}; AllGroups:{2}; ActiveGroups:{3}; DormantGroups:{4}", list.Count, eventSubscriptions.Count, list3.Count, list4.Count, list3.Count - list4.Count);
				IEnumerable<IGrouping<NCLMetaApplicationObject, NavEventSubscription>> enumerable = list2.GroupBy((NavEventSubscription es) => es.OriginalApplicationObject, NCLMetaApplicationObject.AppGroupAndApplicationObjectIdComparer);
				foreach (IGrouping<NCLMetaApplicationObject, NavEventSubscription> item2 in enumerable)
				{
					NCLMetaApplicationObject key = item2.Key;
					ObjectType objectType = key.ApplicationObjectId.ObjectType;
					NCLMetaApplicationObject metaApplicationObject;
					if ((uint)(objectType - 14) <= 1u)
					{
						metaApplicationObject = NavGlobal.NCLMetadata.GetExtensionApplicationObject(key.OwningApp.RuntimeAppId, key.ApplicationObjectId);
					}
					else
					{
						NavGlobal.NCLMetadata.TryGetMetaApplicationObject(key.ApplicationObjectId, out metaApplicationObject, key.MetadataAppGroup.GroupId);
					}
					if (key == metaApplicationObject)
					{
						continue;
					}
					key.Stale = true;
					foreach (ByRef<NavEventScope> eventScopeReference in key.EventScopeReferences)
					{
						eventScopeReference.Value?.ClearEventScope();
						eventScopeReference.Value = NavEventScope.StaleEventScope;
					}
					foreach (NavEventSubscription item3 in item2.Where((NavEventSubscription es) => es.OriginalEventType == NavEventType.Trigger))
					{
						item3.GetEventScope(EventScopeGetOption.SetStaleIfFound);
					}
				}
				IDictionary<long, long> dictionary = list2.DistinctBy((NavEventSubscription es) => es.NoOfCallsKeyHash).ToDictionary((NavEventSubscription es) => es.NoOfCallsKeyHash, (NavEventSubscription es) => es.NoOfCalls);
				foreach (NavEventSubscription item4 in eventSubscriptions.Where((NavEventSubscription es) => !es.HasError))
				{
					if (dictionary.TryGetValue(item4.NoOfCallsKeyHash, out var value))
					{
						item4.NoOfCalls = value;
					}
				}
				foreach (NavEventSubscription item5 in list2)
				{
					item5.GetEventScope(EventScopeGetOption.RemoveIfFound);
				}
				BindSubscriptions(eventSubscriptions);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000212", Category.EventSubscription, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Rebind all finished -- OldSubscriptions:{0}; NewSubscriptions:{1}; AllGroups:{2}; ActiveGroups:{3}; DormantGroups:{4}; Elapsed:{5}", list.Count, eventSubscriptions.Count, list3.Count, list4.Count, list3.Count - list4.Count, stopwatch.Elapsed);
			}
		}
	}

	private void ClearAll()
	{
		lock (syncLock)
		{
			ClearEventSubscriptions();
			initializedAppGroups.Clear();
		}
	}

	private void ClearEventSubscriptions()
	{
		eventSubscriptions.Clear();
		staticSubscriptionObservers.Values.ForEach((INavEventSubscriptionObserver obs) =>
		{
			obs.NotifyAboutClear();
		});
	}

	private void AddEventSubscriptions(IEnumerable<NavEventSubscription> subscriptions)
	{
		eventSubscriptions.AddRange(subscriptions);
		foreach (NavEventSubscription subscription in subscriptions)
		{
			staticSubscriptionObservers.Values.ForEach((INavEventSubscriptionObserver obs) =>
			{
				obs.NotifyAboutAdd(subscription);
			});
		}
	}

	public IList<NavEventSubscription> GetSubscriptionsCopy()
	{
		lock (syncLock)
		{
			return new List<NavEventSubscription>(eventSubscriptions);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			ClearAll();
		}
	}
}
