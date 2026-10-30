using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppPersonalizationMetadataOutputter : NavAppPackageMetadataOutputter
{
	public override void AddApplicationObject(IApplicationObjectTypeSymbol symbol, byte[] code, string metadata, string debugCode)
	{
	}

	public override void AddProfileObject(ISymbol symbol, byte[] code, string metadata, string debugCode)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected Obj, but got Unknown
		if (symbol.Kind == SymbolKind.PageCustomization)
		{
			IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbol = symbol as IApplicationObjectExtensionTypeSymbol;
			ObjectMetadata.Add(new NavAppObjectMetadata
			{
				ObjectType = ObjectType.PageExtension,
				ObjectId = applicationObjectExtensionTypeSymbol.Id,
				Format = (NavAppObjectFormat)2,
				MetadataVersion = 130000,
				ObjectSubtype = applicationObjectExtensionTypeSymbol.Target?.Id.ToString(CultureInfo.InvariantCulture),
				ObjectName = symbol.Name,
				Metadata = metadata,
				UserALCode = debugCode
			});
		}
	}

	public override void FinalizeModule()
	{
	}
}
