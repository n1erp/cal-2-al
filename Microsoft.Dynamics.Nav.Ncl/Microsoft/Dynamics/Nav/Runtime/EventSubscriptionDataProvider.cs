using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class EventSubscriptionDataProvider : EagerVirtualDataProvider
{
	public static readonly EventSubscriptionDataProvider Instance = new EventSubscriptionDataProvider();

	private const string ManualBindingText = "Manual";

	private const string StaticBindingText = "Static-Automatic";

	public override int TableId => 2000000140;

	private EventSubscriptionDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		int fieldDefinedLength = MetaTable.GetFieldByNo(2).FieldDefinedLength;
		NCLOptionMetadata fieldOptionMetadata = MetaTable.GetFieldByNo(3).FieldOptionMetadata;
		NCLOptionMetadata fieldOptionMetadata2 = MetaTable.GetFieldByNo(4).FieldOptionMetadata;
		int fieldDefinedLength2 = MetaTable.GetFieldByNo(6).FieldDefinedLength;
		int fieldDefinedLength3 = MetaTable.GetFieldByNo(9).FieldDefinedLength;
		int fieldDefinedLength4 = MetaTable.GetFieldByNo(11).FieldDefinedLength;
		int fieldDefinedLength5 = MetaTable.GetFieldByNo(12).FieldDefinedLength;
		NavSession session = NavCurrentThread.Session;
		HashSet<NavEventSubscription> distinctEventSubscriptions = GetDistinctEventSubscriptions(NavCurrentThread.ResolveAppGroup(session)?.GroupId, NavGlobal.EventSubscriptionMetadata.GetSubscriptionsCopy());
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>(distinctEventSubscriptions.Count);
		foreach (NavEventSubscription subscription in distinctEventSubscriptions)
		{
			string value = (subscription.Resolved ? string.Empty : (subscription.Status & NavEventSubscriptionStatus.ErrorMask).ToString());
			string text = subscription.OriginalMethodALName;
			if (NavEventSubscription.TriggerEventTypeIsFieldBased(subscription.OriginalTriggerEventType))
			{
				text = text + " - " + ((subscription.OriginalTriggerEventFieldId != 0) ? subscription.OriginalTriggerEventFieldId.ToString(CultureInfo.InvariantCulture) : subscription.OriginalTriggerEventFieldName);
			}
			NavGuid navGuid = NavGuid.Null;
			NavText navText = NavText.Empty;
			if (subscription.SubscriberNavAppGroup.GroupId != 0)
			{
				NavAppRuntimeMetadata val = subscription.SubscriberNavAppGroup.OrderedAppMetadata.FirstOrDefault((NavAppRuntimeMetadata appmd) => appmd.ContainsNewObject(new ApplicationObjectId(subscription.Subscriber.ApplicationObjectId.ObjectType, subscription.SubscriberId)));
				if (val != null)
				{
					navGuid = new NavGuid(((WrappedValue<Guid>)(object)val.PackageId).Value);
					navText = new NavText(fieldDefinedLength4, val.Name);
				}
			}
			int value2 = 0;
			if (subscription.IsEventManualBinding && session != null && session.Tenant != null)
			{
				value2 = session.EventBindings.Count((NavCodeunit cu) => cu.ObjectId.ObjectNumber == subscription.SubscriberId);
			}
			ReadOnlyRecordBuffer item = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(subscription.SubscriberId), NavText.CreateTruncated(fieldDefinedLength, subscription.SubscriberMethodALName), fieldOptionMetadata.CreateNavOption((int)subscription.OriginalEventType), fieldOptionMetadata2.CreateNavOption((int)subscription.OriginalApplicationObjectId.ObjectType), NavInteger.Create(subscription.OriginalApplicationObjectId.ObjectNumber), NavText.CreateTruncated(fieldDefinedLength2, text), NavBoolean.Create(subscription.Resolved), NavBigInteger.Create(subscription.NoOfCalls), NavText.CreateTruncated(fieldDefinedLength3, value), navGuid, navText, new NavText(fieldDefinedLength5, subscription.IsEventManualBinding ? "Manual" : "Static-Automatic"), NavInteger.Create(value2));
			list.Add(item);
		}
		return list;
	}

	internal static HashSet<NavEventSubscription> GetDistinctEventSubscriptions(int? appGroupId, IEnumerable<NavEventSubscription> allSubscriptions)
	{
		HashSet<NavEventSubscription> hashSet = new HashSet<NavEventSubscription>(allSubscriptions.Where((NavEventSubscription es) => appGroupId.HasValue && es.SubscriberNavAppGroup.GroupId == appGroupId), new NavEventSubscriptionDistinctEqualityComparer());
		foreach (NavEventSubscription item in allSubscriptions.Where((NavEventSubscription es) => es.SubscriberNavAppGroup.GroupId == 0))
		{
			hashSet.Add(item);
		}
		return hashSet;
	}
}
