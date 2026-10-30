using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class QueryScaffolding : Scaffolding<NavQuery>
{
	private class FieldsScaffolding : Scaffolding<NavQuery>
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

		protected override IEnumerable<Scaffolding> CreateChildren(NavQuery instance)
		{
			return from column in instance.NCLMetaQuery.QueryDefinition.DefinedColumns
				where !column.SourceTableField.FieldIsTimestamp
				select ScaffoldingFactory.GetNavValueScaffolding(column.Name, column.SourceTableField.FieldClrType, column.SourceTableField.FieldNclType, new QueryFieldValueBinder(column), instance, false);
		}

		protected override string GetStringValue(NavQuery instance)
		{
			return "Count = " + instance.NCLMetaQuery.QueryDefinition.DefinedColumns.Count;
		}
	}

	private ApplicationObjectId objectId;

	private string typeString;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => true;

	public QueryScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, true, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavQuery navQuery)
		{
			return navQuery.ObjectId == objectId;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavQuery instance)
	{
		instance.EnsureGlobalVariablesInitialized();
		Type type = instance.GetType();
		yield return new FieldsScaffolding("Fields", type, new IdentityValueBinder());
		yield return new TypeScaffolding("Filters", typeof(string), new PropertyAccessValueBinder<NavQuery>((NavQuery qry) => qry.ALGetFilters, "ALGetFilters"));
	}

	protected override string GetStringValue(NavQuery instance)
	{
		return string.Empty;
	}

	protected override void ReadMetadata(NavQuery instance)
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
}
