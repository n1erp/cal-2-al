using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLPageExtension : NCLApplicationObjectExtension<PageDefinition, NavForm, NavFormExtension>
{
	internal static NCLPageExtension CreateEmptyNCLPageExtension(INCLMetaApplicationObjectLoader loader, int pageId, NavAppRuntimeMetadata runtimeAppMetadata)
	{
		return new NCLPageExtension(loader, pageId, runtimeAppMetadata);
	}

	protected NCLPageExtension(INCLMetaApplicationObjectLoader loader, int pageId, NavAppRuntimeMetadata runtimeAppMetadata)
		: base(loader, ObjectType.PageExtension, pageId, runtimeAppMetadata)
	{
	}

	protected override void AddExtensionType(NavFormExtension extension, NavForm parent)
	{
		parent.PageExtensions.Add(extension);
	}

	internal virtual NavAppObjectMetadataRuntimeDeltas GetDeltas()
	{
		return Deltas;
	}
}
