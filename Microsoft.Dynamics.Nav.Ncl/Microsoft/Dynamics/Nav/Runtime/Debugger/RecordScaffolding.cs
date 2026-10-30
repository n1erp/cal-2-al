using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class RecordScaffolding : Scaffolding<NavRecord>
{
	private class FieldsScaffolding : Scaffolding<NavRecord>
	{
		public override string TypeString => null;

		protected override bool ProbablyHasChildren => true;

		public FieldsScaffolding(string name, Type type, ValueBinder valueBinder)
			: base(name, type, valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NavRecord instance)
		{
			return from field in instance.MetaTable.Fields
				where !field.FieldIsTimestamp
				select ScaffoldingFactory.GetNavValueScaffolding(field.FieldName, field.FieldClrType, field.FieldNclType, new RecordFieldValueBinder(field), instance, false);
		}

		protected override string GetStringValue(NavRecord instance)
		{
			return "Count = " + (instance.MetaTable.FieldCount - 1);
		}
	}

	private class KeysScaffolding : Scaffolding<NavRecord>
	{
		public override string TypeString => string.Empty;

		protected override bool ProbablyHasChildren => true;

		public KeysScaffolding(string name, Type targetType, ValueBinder valueBinder)
			: base(name, targetType, valueBinder, false, false)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NavRecord instance)
		{
			int noOfKeys = instance.MetaTable.KeyCount;
			string sorting = (instance.ALAscending ? Lang.SortingTypeAscending : Lang.SortingTypeDescending);
			for (int i = 0; i < noOfKeys; i++)
			{
				NCLMetaKey key = instance.MetaTable.GetKeyByIndex(i);
				if (!string.IsNullOrWhiteSpace(key.GetKeyName()))
				{
					yield return new KeyScaffolding(key.GetKeyName(), new DelegateValueBinder((object parent) => (!(key.GetKeyName() == ((NavRecord)parent).ALCurrentKey)) ? string.Empty : sorting, null, typeof(string)));
				}
			}
		}

		protected override string GetStringValue(NavRecord instance)
		{
			return instance.ALCurrentKey;
		}
	}

	private class KeyScaffolding : TypeScaffolding
	{
		public override string TypeString => string.Empty;

		public KeyScaffolding(string name, ValueBinder valueBinder)
			: base(name, typeof(string), valueBinder)
		{
		}
	}

	private class GlobalsScaffolding : ApplicationObjectScaffolding<NavRecord>
	{
		public override string TypeString => string.Empty;

		public GlobalsScaffolding(string name, Type type, ValueBinder valueBinder)
			: base(name, type, valueBinder, false, true)
		{
		}

		protected override bool IsCompatibleOverride(object instance)
		{
			return true;
		}

		protected override IEnumerable<Scaffolding> CreateChildren(NavRecord instance)
		{
			return from c in base.CreateChildren(instance)
				where (!(c.Name == "Rec") && !(c.Name == "xRec")) || !(c is RecordScaffolding)
				select c;
		}
	}

	private readonly bool showGlobalVariables;

	private ApplicationObjectId objectId;

	private string typeString;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => true;

	public RecordScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: this(name, type, valueBinder, promotedChildren, showGlobalVariables: true)
	{
	}

	public RecordScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren, bool showGlobalVariables)
		: base(name, type, valueBinder, true, promotedChildren)
	{
		this.showGlobalVariables = showGlobalVariables;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavRecord navRecord)
		{
			return navRecord.ObjectId == objectId;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavRecord instance)
	{
		if (instance != null)
		{
			instance.EnsureGlobalVariablesInitialized();
			Type recordType = instance.GetType();
			yield return new FieldsScaffolding("Fields", recordType, new IdentityValueBinder());
			yield return new TypeScaffolding("Filter Group", typeof(int), new PropertyAccessValueBinder<NavRecord>((NavRecord rec) => rec.ALFilterGroup, "ALFilterGroup", isSetSupported: true));
			yield return new TypeScaffolding("Filters", typeof(string), new PropertyAccessValueBinder<NavRecord>((NavRecord rec) => rec.ALGetFilters, "ALGetFilters"));
			yield return new KeysScaffolding("Keys", recordType, new IdentityValueBinder());
			if (showGlobalVariables)
			{
				yield return new GlobalsScaffolding("<Globals>", recordType, new IdentityValueBinder());
			}
		}
	}

	protected override string GetStringValue(NavRecord instance)
	{
		NCLMetaKey keyByIndex = instance.MetaTable.GetKeyByIndex(0);
		return string.Join(", ", (from f in keyByIndex.KeyFieldsList
			select instance.GetFieldValue(f) into v
			select FormatString(v)).ToArray());
	}

	protected override void ReadMetadata(NavRecord instance)
	{
		if (instance != null)
		{
			objectId = instance.ObjectId;
			typeString = ScaffoldingHelper.GetTypeString(instance);
		}
		else
		{
			typeString = string.Empty;
		}
	}

	private static string FormatString(NavValue value)
	{
		string text = ((value == null) ? string.Empty : value.ToString());
		if (value.NclType == NavNclType.NavOemText || value.NclType == NavNclType.NavText || value.NclType == NavNclType.NavOemCode || value.NclType == NavNclType.NavCode)
		{
			text = "'" + text + "'";
		}
		return text;
	}
}
