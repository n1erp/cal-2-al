using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class NotificationScaffolding : Scaffolding<NavNotification>
{
	private class DataScaffolding : Scaffolding<NotificationInfo>
	{
		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public DataScaffolding(string name, ValueBinder valueBinder)
			: base(name, typeof(NotificationInfo), valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NotificationInfo instance)
		{
			foreach (string name in instance.Data.Keys)
			{
				yield return new TypeScaffolding(name, typeof(string), new DelegateValueBinder((object input) => ((NotificationInfo)input).Data[name], typeof(NotificationInfo)));
			}
		}

		protected override string GetStringValue(NotificationInfo instance)
		{
			return "Count = " + instance.Data.Count;
		}
	}

	private class ActionsScaffolding : Scaffolding<NotificationInfo>
	{
		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public ActionsScaffolding(string name, ValueBinder valueBinder)
			: base(name, typeof(NotificationInfo), valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NotificationInfo instance)
		{
			foreach (NotificationInfoAction action in instance.Actions)
			{
				yield return new NotificationInfoActionScaffolding(action.Caption, new DelegateValueBinder((object input) => action, typeof(NotificationInfo)));
			}
		}

		protected override string GetStringValue(NotificationInfo instance)
		{
			return "Count = " + instance.Actions.Count;
		}
	}

	private class NotificationInfoActionScaffolding : Scaffolding<NotificationInfoAction>
	{
		private const string CodeunitidFieldName = "CodeunitID";

		private const string FunctionNameFieldName = "FunctionName";

		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public NotificationInfoActionScaffolding(string name, ValueBinder valueBinder)
			: base(name, typeof(Dictionary<string, string>), valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NotificationInfoAction instance)
		{
			Type notificationInfoActionType = instance.GetType();
			PropertyInfo property = notificationInfoActionType.GetProperty("CodeunitId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			yield return ScaffoldingFactory.GetScaffolding("CodeunitID", typeof(int), new PropertyInfoValueBinder(property), instance, false);
			PropertyInfo property2 = notificationInfoActionType.GetProperty("FunctionName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			yield return ScaffoldingFactory.GetScaffolding("FunctionName", typeof(string), new PropertyInfoValueBinder(property2), instance, false);
		}

		protected override string GetStringValue(NotificationInfoAction instance)
		{
			return instance.Caption;
		}
	}

	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string IdFieldName = "ID";

	private const string MessageFieldName = "Message";

	private const string ScopeFieldName = "Scope";

	private const string DataFieldName = "Data";

	private const string ActionsFieldName = "Actions";

	private const string CountPrefix = "Count = ";

	private string typeString;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => true;

	public NotificationScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, true, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return instance is NavNotification;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavNotification instance)
	{
		Type notificationType = instance.GetType();
		PropertyInfo property = notificationType.GetProperty("ALId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("ID", typeof(Guid), new PropertyInfoValueBinder(property), instance, false);
		PropertyInfo property2 = notificationType.GetProperty("ALMessage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("Message", typeof(string), new PropertyInfoValueBinder(property2), instance, false);
		PropertyInfo property3 = notificationType.GetProperty("ALScope", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("Scope", typeof(NotificationScope), new PropertyInfoValueBinder(property3), instance, false);
		PropertyInfo notificationInfoDataProperty = notificationType.GetProperty("NotificationInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return new DataScaffolding("Data", new PropertyInfoValueBinder(notificationInfoDataProperty));
		yield return new ActionsScaffolding("Actions", new PropertyInfoValueBinder(notificationInfoDataProperty));
	}

	protected override string GetStringValue(NavNotification instance)
	{
		return instance.ALMessage;
	}

	protected override void ReadMetadata(NavNotification instance)
	{
		typeString = ((instance != null) ? ScaffoldingHelper.GetTypeString(instance.NclType, instance) : string.Empty);
	}
}
