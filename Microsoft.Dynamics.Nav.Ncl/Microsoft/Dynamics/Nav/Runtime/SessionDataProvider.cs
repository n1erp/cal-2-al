using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SessionDataProvider : EagerVirtualDataProvider
{
	public static readonly SessionDataProvider Instance = new SessionDataProvider();

	public override int TableId => 2000000009;

	private SessionDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000110);
		navRecord.ALGet(NavInteger.Create(NavEnvironment.Instance.GetServiceInstanceId()), NavInteger.Create(session.Id));
		NavDateTime navDateTime = (NavDateTime)navRecord.GetFieldValue(9);
		ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(session.Id), navRecord.GetFieldValue(6), NavBoolean.True, navDateTime.TimePart, navDateTime.DatePart, navRecord.GetFieldValue(10), NavText.Create(navRecord.GetFieldValue(7).ToString()), GetOptionValue(18, (int)session.Authenticator.AuthenticationMethod), NavText.Create(DnsHelper.HostName));
		return new ReadOnlyRecordBuffer[1] { readOnlyRecordBuffer };
	}
}
