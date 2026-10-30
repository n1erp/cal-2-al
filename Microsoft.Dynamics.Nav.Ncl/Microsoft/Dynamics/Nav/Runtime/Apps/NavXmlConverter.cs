using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavXmlConverter : INavXmlConverter
{
	private readonly INavMetadataConverter deltaConverter;

	private readonly INavXmlConverterDeltaRetriever deltaRetriever;

	private readonly INavXmlConverterStorage deltaStorage;

	public NavXmlConverter(INavMetadataConverter deltaConverter, INavXmlConverterDeltaRetriever deltaRetriever, INavXmlConverterStorage deltaStorage)
	{
		this.deltaConverter = deltaConverter;
		this.deltaRetriever = deltaRetriever;
		this.deltaStorage = deltaStorage;
	}

	public void ConvertAndStore()
	{
		try
		{
			IEnumerable<IMetadataRecord> deltasToConvert = deltaRetriever.GetDeltasToConvert();
			NavXmlDeltaConversionResult navXmlDeltaConversionResult = deltaConverter.Convert(deltasToConvert);
			deltaStorage.SaveConvertedDeltas(navXmlDeltaConversionResult.ConvertedPages);
			ReportConversionResult(navXmlDeltaConversionResult);
		}
		catch (Exception ex) when (!(ex is NavException))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001M6", Category.Extensions, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("App Conversion :: Failure"));
			throw new NavAppConverterException(string.Format(CultureInfo.CurrentCulture, Lang.UnexpectedProcessError, ex.Message));
		}
	}

	private static void ReportConversionResult(NavXmlDeltaConversionResult result)
	{
		if (result.HadWarnings)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001M7", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Conversion of {0} completed with the following warnings(s): {1}", result.ConversionType, result.Warnings().FormatMessages());
		}
		if (result.HadErrors)
		{
			string text = result.Errors().FormatMessages();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001M8", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, "App Conversion Failure :: Type:{0}; Error(s):'{1}", result.ConversionType, text);
			throw new NavAppConverterException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppConversionError, result.ConversionType, text));
		}
	}
}
