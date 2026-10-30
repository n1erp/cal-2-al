using System;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

internal class ControlAddDeltaValidationFilter : IDeltaValidationFilter
{
	public Type Type => typeof(ControlAddDelta);

	public bool IsValidDelta(object subject, Delta delta)
	{
		if (subject is PageDefinition pageDefinition)
		{
			MetaTable tableMetadata = NavCurrentThread.Session.MetadataProvider.GetTableMetadata(pageDefinition.Properties.SourceObject.SourceTable);
			return tableMetadata.Fields.FirstOrDefault((MetaField f) =>
			{
				string name = f.Name;
				Delta val = delta;
				return name.Equals(((ContentAddDelta<ControlBaseDefinition>)(object)((val is ControlAddDelta) ? val : null))?.Context.Content.Name);
			}) != null;
		}
		return false;
	}
}
