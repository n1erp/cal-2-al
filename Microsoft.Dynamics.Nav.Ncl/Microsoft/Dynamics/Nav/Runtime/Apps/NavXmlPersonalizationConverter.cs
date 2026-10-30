using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavXmlPersonalizationConverter : NavXmlAbstractConverter
{
	protected override ConversionType ConversionType => ConversionType.Personalization;

	protected override string ObjectPrefix => "Personalization";

	public NavXmlPersonalizationConverter(INavXmlConverterMetadataProvider metadataProvider, INavXmlDeltaConverter deltaConverter)
		: base(metadataProvider, deltaConverter)
	{
	}

	public override NavXmlDeltaConversionResult Convert(IEnumerable<IMetadataRecord> records)
	{
		records = records ?? new IMetadataRecord[0];
		List<NavXmlDeltaConversionDiagnostics> list = new List<NavXmlDeltaConversionDiagnostics>();
		foreach (IGrouping<Guid, IMetadataRecord> deltasByUser in from r in records
			group r by ((UserMetadataRecord)r).UserSid)
		{
			MasterNavigation profileNavigationFrame = null;
			NavXmlAbstractConverter.HandleGenericErrors(list, () =>
			{
				profileNavigationFrame = metadataProvider.GetMasterNavigationFrame(deltasByUser.Key);
			});
			ConvertInternal(from d in deltasByUser
				group d by d.PageId, profileNavigationFrame);
		}
		return NavXmlDeltaConversionResult.Create(ConversionType, convertedPages, list);
	}
}
