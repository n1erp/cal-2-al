using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableViewResolver : TableFilterResolver
{
	public List<SortOrder> SortOrder { get; private set; }

	public List<NCLMetaField> SortingFields { get; private set; }

	protected override string Description => Lang.TableView;

	public TableViewResolver(NCLMetaTable table, string input)
		: base(table, input)
	{
	}

	public virtual void AddSortingField(string fieldIdentifier)
	{
		NCLMetaField nCLMetaField = ResolveField(fieldIdentifier, trapError: true);
		if (nCLMetaField == null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000010G", Category.FilterParsing, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Sorting field {0} for table {1} (id={2}) does not exist.", fieldIdentifier, TableName, TableId);
		}
		else
		{
			if (SortingFields == null)
			{
				SortingFields = new List<NCLMetaField>();
			}
			SortingFields.Add(nCLMetaField);
		}
	}

	public virtual void AddSortOrder(SortOrder sortOrder)
	{
		if (SortOrder == null)
		{
			SortOrder = new List<SortOrder>();
		}
		SortOrder.Add(sortOrder);
	}

	protected override void ValidateFilterType(NCLMetaFilterType filterType)
	{
		if ((uint)filterType > 1u)
		{
			throw GetInvalidExpressionException();
		}
	}
}
