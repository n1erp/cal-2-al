namespace Microsoft.Dynamics.Nav.Runtime;

public class RecordDataItemLink
{
	private FilterFieldDictionary filterFieldDictionary;

	public string Link { get; private set; }

	public NCLMetaTable FromMetaTable { get; private set; }

	public NCLMetaTable ToMetaTable { get; private set; }

	public RecordDataItemLink(NCLMetaTable fromMetaTable, NCLMetaTable toMetaTable, string link)
	{
		FromMetaTable = fromMetaTable;
		ToMetaTable = toMetaTable;
		Link = link;
	}

	internal FilterFieldDictionary GetFilterFieldDictionary()
	{
		if (filterFieldDictionary == null)
		{
			DataItemLinkResolver dataItemLinkResolver = new DataItemLinkResolver(ToMetaTable, FromMetaTable, Link);
			TableViewParser.ParseTableFilters(Link, dataItemLinkResolver);
			filterFieldDictionary = ((dataItemLinkResolver.Filters.Count == 0) ? FilterFieldDictionary.Empty : new FilterFieldDictionary(dataItemLinkResolver.Filters));
		}
		return filterFieldDictionary;
	}
}
