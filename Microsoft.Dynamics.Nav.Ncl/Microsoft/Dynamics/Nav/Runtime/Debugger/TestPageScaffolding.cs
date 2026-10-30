using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class TestPageScaffolding : Scaffolding<NavTestPageBase>
{
	private class FieldsScaffolding : Scaffolding<NavTestPageBase>
	{
		private ApplicationObjectId objectId;

		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public FieldsScaffolding(string name, Type type, ValueBinder valueBinder)
			: base(name, type, valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			if (instance is NavTestPageBase navTestPageBase)
			{
				return navTestPageBase.ObjectId == objectId;
			}
			return false;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NavTestPageBase instance)
		{
			if (!instance.IsOpened)
			{
				yield break;
			}
			foreach (TestFieldScaffolding item in from field in instance.MetaPage.FindAll((ElementDefinition element) => element is ControlDefinition)
				let controlDef = (ControlDefinition)field
				let testField = instance.GetField(controlDef.ID, throwIfNotFound: false)
				where testField != null && testField.FieldType != NavType.None
				select new TestFieldScaffolding(testField.Name, typeof(NavText), new DelegateValueBinder((object p) => ((NavTestPageBase)p).GetField(controlDef.ID), typeof(NavTestField))))
			{
				yield return item;
			}
		}

		protected override string GetStringValue(NavTestPageBase instance)
		{
			return "Count = " + instance.MetaPage.FindAll((ElementDefinition element) => element is ControlDefinition).Count;
		}

		protected override void ReadMetadata(NavTestPageBase instance)
		{
			if (instance != null)
			{
				objectId = instance.ObjectId;
			}
			else
			{
				objectId = default;
			}
		}
	}

	private class TestFieldScaffolding : Scaffolding<NavTestField>
	{
		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public TestFieldScaffolding(string name, Type type, ValueBinder valueBinder)
			: base(name, type, valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NavTestField instance)
		{
			yield return ScaffoldingFactory.GetScaffolding("VALUE", typeof(string), new PropertyAccessValueBinder<NavTestField>((NavTestField field) => field.ALValue, "ALValue"), instance, false);
			yield return ScaffoldingFactory.GetScaffolding("CAPTION", typeof(string), new PropertyAccessValueBinder<NavTestField>((NavTestField field) => field.Name, "ALCaption"), instance, false);
		}

		protected override string GetStringValue(NavTestField instance)
		{
			return new NavText(instance.ALValue);
		}
	}

	private class PartsScaffolding : Scaffolding<NavTestPageBase>
	{
		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public PartsScaffolding(string name, Type type, ValueBinder valueBinder)
			: base(name, type, valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NavTestPageBase instance)
		{
			if (!instance.IsOpened)
			{
				yield break;
			}
			foreach (TestPageScaffolding item in from part in instance.MetaPage.FindAll((ElementDefinition element) => element is InfopartPageDefinition)
				let partDef = (InfopartPageDefinition)part
				where instance.GetPart(partDef.ID, throwIfNotFound: false) != null
				select new TestPageScaffolding(partDef.Name ?? NavCurrentThread.Session.GetObjectName(new ApplicationObjectId(ObjectType.Page, partDef.PagePartID)), typeof(NavTestPage), new DelegateValueBinder((object page) => ((NavTestPage)page).GetPart(partDef.ID), typeof(NavTestPart)), promotedChildren: true))
			{
				yield return item;
			}
		}

		protected override string GetStringValue(NavTestPageBase instance)
		{
			return "Count = " + instance.MetaPage.FindAll((ElementDefinition element) => element is InfopartPageDefinition).Count;
		}
	}

	private class FilterScaffolding : Scaffolding<NavTestPageBase>
	{
		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public FilterScaffolding(string name, Type type, ValueBinder valueBinder)
			: base(name, type, valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NavTestPageBase instance)
		{
			yield return ScaffoldingFactory.GetScaffolding("ASCENDING", typeof(bool), new PropertyAccessValueBinder<NavTestPageBase>((NavTestPageBase page) => page.ALFilter.ALAscending, "Ascending"), instance, false);
			yield return ScaffoldingFactory.GetScaffolding("CURRENTKEY", typeof(string), new PropertyAccessValueBinder<NavTestPageBase>((NavTestPageBase page) => page.ALFilter.ALCurrentKey, "CurrentKey"), instance, false);
			yield return ScaffoldingFactory.GetScaffolding("FILTER", typeof(string), new DelegateValueBinder((object page) => ((NavTestPageBase)page).GetFilter(), typeof(string)), instance, false);
		}

		protected override string GetStringValue(NavTestPageBase instance)
		{
			if (!instance.IsOpened)
			{
				return string.Empty;
			}
			return (instance.ALFilter.ALAscending ? "Ascending " : "Descending ") + instance.ALFilter.ALCurrentKey;
		}
	}

	private const string ClosedValueString = "<Closed>";

	private const string HiddenValueString = "<Hidden>";

	internal const string TypeNameString = "TestPage";

	private ApplicationObjectId objectId;

	private string typeString;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => true;

	public TestPageScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, true, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavTestPageBase navTestPageBase && navTestPageBase.ObjectId == objectId)
		{
			return navTestPageBase.IsOpened;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavTestPageBase instance)
	{
		if (instance != null && instance.IsOpened)
		{
			Type testPageType = instance.GetType();
			yield return new FilterScaffolding("Filter", testPageType, new IdentityValueBinder());
			yield return new FieldsScaffolding("Fields", testPageType, new IdentityValueBinder());
			yield return new PartsScaffolding("Parts", testPageType, new IdentityValueBinder());
		}
	}

	protected override string GetStringValue(NavTestPageBase instance)
	{
		if (instance == null)
		{
			return "<Hidden>";
		}
		if (!instance.IsOpened)
		{
			return "<Closed>";
		}
		return instance.ALCaption;
	}

	protected override void ReadMetadata(NavTestPageBase instance)
	{
		if (instance != null)
		{
			objectId = instance.ObjectId;
			typeString = ScaffoldingHelper.GetTypeString("TestPage", instance.ObjectName, instance.ObjectId.ObjectNumber);
		}
		else
		{
			typeString = string.Empty;
		}
	}
}
