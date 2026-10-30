using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.Configuration;

internal class NavXmlConfigurationConverter : NavXmlAbstractConverter
{
	protected override ConversionType ConversionType => ConversionType.Configuration;

	protected override string ObjectPrefix => "Configuration";

	public NavXmlConfigurationConverter(INavXmlConverterMetadataProvider metadataProvider, INavXmlDeltaConverter deltaConverter)
		: base(metadataProvider, deltaConverter)
	{
	}

	public override NavXmlDeltaConversionResult Convert(IEnumerable<IMetadataRecord> records)
	{
		records = records ?? new IMetadataRecord[0];
		List<NavXmlDeltaConversionDiagnostics> list = new List<NavXmlDeltaConversionDiagnostics>();
		foreach (IGrouping<string, IMetadataRecord> deltasByProfile in from r in records
			group r by ((ProfileMetadataRecord)r).ProfileId)
		{
			MasterNavigation profileNavigationFrame = null;
			NavXmlAbstractConverter.HandleGenericErrors(list, () =>
			{
				profileNavigationFrame = metadataProvider.GetMasterNavigationFrame(deltasByProfile.Key);
			});
			ConvertInternal(from d in deltasByProfile
				group d by d.PageId, profileNavigationFrame);
		}
		return NavXmlDeltaConversionResult.Create(ConversionType, convertedPages, list);
	}
}
