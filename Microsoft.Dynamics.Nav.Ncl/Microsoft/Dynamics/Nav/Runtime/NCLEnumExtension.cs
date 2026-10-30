using System;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLEnumExtension : NCLApplicationObjectExtension<MetaEnum>
{
	protected NCLEnumExtension(INCLMetaApplicationObjectLoader loader, int objectId, NavAppRuntimeMetadata runtimeAppMetadata)
		: base(loader, ObjectType.EnumExtension, objectId, runtimeAppMetadata)
	{
	}

	internal static NCLEnumExtension CreateEmptyNCLEnumExtension(INCLMetaApplicationObjectLoader loader, int enumId, NavAppRuntimeMetadata runtimeAppMetadata)
	{
		return new NCLEnumExtension(loader, enumId, runtimeAppMetadata);
	}

	protected override Type LoadClrType()
	{
		return null;
	}
}
