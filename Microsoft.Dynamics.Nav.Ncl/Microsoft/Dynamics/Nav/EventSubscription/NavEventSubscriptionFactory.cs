using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.EventSubscription;

internal class NavEventSubscriptionFactory
{
	private readonly Func<ITenantSessionHandler> getSessionHandler;

	private List<int> cachedBaseSubscriberCodeunitIds;

	public NavEventSubscriptionFactory(Func<ITenantSessionHandler> getSessionHandler)
	{
		if (getSessionHandler == null)
		{
			throw new ArgumentNullException("getSessionHandler");
		}
		this.getSessionHandler = getSessionHandler;
	}

	public IList<NavEventSubscription> FindAndCreateSubscriptionInstances(NavAppGroup appGroup, NavEventSubscriptionModifiers subscriptionModifiers)
	{
		if (appGroup == null)
		{
			throw new ArgumentNullException("appGroup");
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<int> list = new List<int>(128);
		List<int> baseSubscriberCodeunitIdsFromCacheOrDatabase = GetBaseSubscriberCodeunitIdsFromCacheOrDatabase();
		list.AddRange(baseSubscriberCodeunitIdsFromCacheOrDatabase);
		foreach (NavAppRuntimeMetadata orderedAppMetadatum in appGroup.OrderedAppMetadata)
		{
			IEnumerable<int> collection = from a in orderedAppMetadatum.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary a) => a.ObjectType == ObjectType.CodeUnit).Where((NavAppObjectMetadataSummary a) =>
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					return ((Enum)a.ObjectFlags).HasFlag((Enum)(object)(NavAppObjectFlags)1);
				})
				select a.ObjectId;
			list.AddRange(collection);
		}
		List<NavEventSubscription> list2 = CreateEventSubscriptionsFromCodeunitIds(list, appGroup, subscriptionModifiers);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000PQ", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Created app group subscription instances -- Group:{0}; Subscriptions:{1}; Elapsed:{2}", appGroup.GroupId, list2.Count, stopwatch.ElapsedMilliseconds);
		return list2;
	}

	private List<NavEventSubscription> CreateEventSubscriptionsFromCodeunitIds(List<int> codeunitIds, NavAppGroup appGroup, NavEventSubscriptionModifiers subscriptionModifiers)
	{
		List<NavEventSubscription> list = new List<NavEventSubscription>(128);
		foreach (int codeunitId in codeunitIds)
		{
			try
			{
				if (NavGlobal.NCLMetadata.TryGetMetaApplicationObject(ObjectType.CodeUnit, codeunitId, out var metaApplicationObject, appGroup.GroupId))
				{
					NCLMetaCodeunit subscriber = (NCLMetaCodeunit)metaApplicationObject;
					IEnumerable<NavEventSubscription> collection = CreateEventSubscriptionsFromSubscriber(subscriber, appGroup, subscriptionModifiers);
					list.AddRange(collection);
				}
			}
			catch (NavNCLMetadataCompileErrorException ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000PR", Category.EventSubscription, ex);
				LogWriterHelper.LogEvent(EventLogEntryType.Error, string.Format(CultureInfo.CurrentCulture, Lang.ErrorEventingStartup, codeunitId, ex.Message, ProductNames.ProductNameServer), isExceptionEvent: true);
				throw;
			}
			catch (ObjectDisposedException ex2)
			{
				if (NavEnvironment.Instance.InServerShutdown || NavEnvironment.Instance.IsDisposed)
				{
					return list;
				}
				LogWriterHelper.LogEvent(EventLogEntryType.Information, string.Format(CultureInfo.CurrentCulture, Lang.InformationEventingStartupAborted, ex2.Message, ProductNames.ProductNameServer), isExceptionEvent: true);
				throw;
			}
		}
		return list;
	}

	private static IEnumerable<NavEventSubscription> CreateEventSubscriptionsFromSubscriber(INavEventSubscriber subscriber, NavAppGroup appGroup, NavEventSubscriptionModifiers subscriptionModifiers)
	{
		List<NavEventSubscription> list = new List<NavEventSubscription>();
		foreach (NavEventSubscriberMethodInfo item in subscriber.SubscriberReflectionWrapper.EventSubscriberMethodInfo)
		{
			ApplicationObjectId targetObjectId = item.Attribute.TargetObjectId;
			bool flag;
			if (appGroup == NavAppGroup.BaseGroup)
			{
				flag = subscriber.OriginatesFromBase;
			}
			else
			{
				flag = !subscriber.OriginatesFromBase;
				if (!flag)
				{
					flag = subscriber.OriginatesFromBase && appGroup.ContainsAppExtensionsForObject(targetObjectId.ObjectType, targetObjectId.ObjectNumber);
				}
			}
			if (flag)
			{
				try
				{
					list.Add(new NavEventSubscription(subscriber, item, appGroup, subscriptionModifiers, item.Attribute.MemberId));
				}
				catch (Exception exception)
				{
					string content = string.Format(CultureInfo.InvariantCulture, "Unexpected event subscriber failure -- SubscriberType:{0}; SubscriberId:{1}; TargetType:{2}; TargetId:{3}; SubscriberCLRMethod:{4}; SubscriberALName:{5}; TargetMethod:{6};", subscriber.ApplicationObjectId.ObjectType, subscriber.ApplicationObjectId.ObjectNumber, item.Attribute.TargetObjectId.ObjectType, item.Attribute.TargetObjectId.ObjectNumber, item.RawMethodInfo.Name, item.ALName, item.Attribute.TargetMethodName);
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000020D", Category.EventSubscription, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(content));
					throw;
				}
			}
		}
		return list;
	}

	public void InvalidateBaseSubscriberCodeunitIdCache()
	{
		cachedBaseSubscriberCodeunitIds = null;
	}

	private List<int> GetBaseSubscriberCodeunitIdsFromCacheOrDatabase()
	{
		List<int> list = cachedBaseSubscriberCodeunitIds;
		if (list == null)
		{
			list = (cachedBaseSubscriberCodeunitIds = GetBaseSubscriberCodeunitIdsFromDatabase());
		}
		return list;
	}

	private List<int> GetBaseSubscriberCodeunitIdsFromDatabase()
	{
		List<int> eventSubscriberCodeunits = new List<int>(100);
		getSessionHandler().RunTenantActionInSystemSession((NavTenant tenant, NavSession session) =>
		{
			using NavRecord navRecord = new NavRecord(session, 2000000071);
			navRecord.ALSetRange(3, NavInteger.Create(5));
			navRecord.ALSetRange(34, NavBoolean.True);
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					eventSubscriberCodeunits.Add(navRecord.GetFieldValue(6).ToInt32());
				}
				while (navRecord.ALNext(1) != 0);
			}
		}, throwExceptions: true);
		return eventSubscriberCodeunits;
	}
}
