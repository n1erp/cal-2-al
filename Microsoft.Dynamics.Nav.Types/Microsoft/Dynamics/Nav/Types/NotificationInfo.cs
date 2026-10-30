using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NotificationInfo
{
	[DataMember]
	public Guid Id { get; set; }

	[DataMember]
	public string Message { get; set; } = string.Empty;

	[DataMember]
	public NotificationScope Scope { get; set; }

	[DataMember]
	public ICollection<NotificationInfoAction> Actions { get; set; } = new List<NotificationInfoAction>();

	[DataMember]
	public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

	[DataMember]
	public Guid SentFromServerFormHandleId { get; set; }

	public void AddAction(string caption, int codeunitId, string functionName)
	{
		Actions.Add(new NotificationInfoAction(Actions.Count, caption, codeunitId, functionName));
	}

	public NotificationInfo Clone()
	{
		return new NotificationInfo
		{
			Message = Message,
			Scope = Scope,
			SentFromServerFormHandleId = SentFromServerFormHandleId,
			Actions = ((Actions == null) ? null : new List<NotificationInfoAction>(Actions.Select((NotificationInfoAction t) => t.Clone()))),
			Data = Data.ToDictionary((KeyValuePair<string, string> t) => t.Key, (KeyValuePair<string, string> t) => t.Value),
			Id = Id
		};
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NotificationInfo notificationInfo))
		{
			return false;
		}
		if (Id.Equals(notificationInfo.Id) && Message.Equals(notificationInfo.Message))
		{
			return Data.SequenceEqual(notificationInfo.Data);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return new StringBuilder(string.Format(CultureInfo.CurrentCulture, "ID: {0}, SentFromServerFormHandleId: {1}, Message: {2}, Scope: {3}", Id, SentFromServerFormHandleId, Message, Scope)).ToString();
	}
}
