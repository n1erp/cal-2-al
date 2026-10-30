using System;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLTableExtension : NCLApplicationObjectExtension<MetaTable, NavRecord, NavRecordExtension>
{
	protected delegate NavRecordExtension CreateTableExtensionObjectInstance(ITreeObject parent);

	protected override Type ApplicationObjectConstructorDelegateType { get; } = typeof(CreateTableExtensionObjectInstance);

	internal static NCLTableExtension CreateEmptyNCLTableExtension(INCLMetaApplicationObjectLoader loader, int tableId, NavAppRuntimeMetadata runtimeAppMetadata)
	{
		return new NCLTableExtension(loader, tableId, runtimeAppMetadata);
	}

	protected NCLTableExtension(INCLMetaApplicationObjectLoader loader, int tableId, NavAppRuntimeMetadata runtimeAppMetadata)
		: base(loader, ObjectType.TableExtension, tableId, runtimeAppMetadata)
	{
	}

	protected override void AddExtensionType(NavRecordExtension extension, NavRecord parent)
	{
		parent.RegisterTableExtension(extension);
	}

	protected override NavRecordExtension CreateExtensionObject(NavRecord parent, NavRecord record = null)
	{
		return ((CreateTableExtensionObjectInstance)ApplicationObjectConstructor)(parent);
	}
}
