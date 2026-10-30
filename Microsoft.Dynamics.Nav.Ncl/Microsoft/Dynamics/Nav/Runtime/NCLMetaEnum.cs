using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaEnum : NCLMetaApplicationObject
{
	private class EnumLoadResult
	{
		public NCLEnumMetadata EnumMetadata { get; set; }

		public long Timestamp { get; set; }
	}

	private NCLOptionMetadata enumMetadata;

	private NCLMetaEnum(INCLMetaApplicationObjectLoader loader, int objectId, NavAppGroup metadataAppGroup, int appDependencyOrder = -1)
		: base(loader, ObjectType.Enum, objectId, metadataAppGroup, appDependencyOrder)
	{
	}

	internal static NCLMetaEnum CreateEmptyNCLMetaEnum(INCLMetaApplicationObjectLoader loader, int enumId, NavAppGroup appGroup, int appDependencyOrder = -1)
	{
		return new NCLMetaEnum(loader, enumId, appGroup, appDependencyOrder);
	}

	protected override void LoadMetadata()
	{
		base.LoadMetadata();
		EnumLoadResult enumLoadResult = LoadEnumMetadata();
		enumMetadata = enumLoadResult.EnumMetadata;
		Timestamp = enumLoadResult.Timestamp;
	}

	public override void ValidateMetadata()
	{
		base.ValidateMetadata();
		LoadEnumMetadata();
	}

	protected override NCLObjectXmlMetadata GetMetadataFromLoader()
	{
		return NavGlobal.MetadataProvider.GetLegacyEnumMetadata(ObjectId) ?? base.GetMetadataFromLoader();
	}

	protected override Type LoadClrType()
	{
		return null;
	}

	private void ApplyExtensions(MetaEnum definition, IEnumerable<NCLEnumExtension> extensions)
	{
		if (definition != null && extensions != null)
		{
			ApplyExtensionObjects(extensions, definition);
		}
	}

	internal NCLOptionMetadata GetNCLEnumMetadata()
	{
		return enumMetadata;
	}

	private EnumLoadResult LoadEnumMetadata()
	{
		try
		{
			NCLObjectXmlMetadata metadataFromLoader = GetMetadataFromLoader();
			MetaEnum metaEnum = new MetaEnum(metadataFromLoader.Document.DocumentElement);
			IEnumerable<NCLEnumExtension> extensionObjects = GetExtensionObjects<NCLEnumExtension>(MetadataAppGroup);
			ApplyExtensions(metaEnum, extensionObjects);
			string[] option = metaEnum.Values.Select((MetaEnumValue m) => m.Name).ToArray();
			int[] indexes = metaEnum.Values.Select((MetaEnumValue m) => m.Ordinal).ToArray();
			NCLCaptionStrings[] optionCaptions = metaEnum.Values.Select((MetaEnumValue m) => NCLCaptionStrings.CreateNCLCaptionStrings(m.CaptionML ?? string.Empty, m.Name)).ToArray();
			return new EnumLoadResult
			{
				EnumMetadata = NCLEnumMetadata.Create(option, indexes, optionCaptions),
				Timestamp = metadataFromLoader.Timestamp
			};
		}
		catch (NavMetadataNotFoundException ex)
		{
			throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, ApplicationObjectId.ObjectType, ApplicationObjectId.ObjectNumber, ex.InnerException);
		}
	}
}
