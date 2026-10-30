using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavGenericFormatter : NavValueFormatter
{
	internal override string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			switch (value.NclType)
			{
			case NavNclType.NavBigText:
				return ((NavBigText)value).Value;
			case NavNclType.NavKeyRef:
				return ((NavKeyRef)value).Value;
			case NavNclType.NavGuid:
				return ((NavGuid)value).Value.ToString("B").ToUpperInvariant();
			case NavNclType.NavMedia:
				return ((NavMedia)value).ToGuid().ToString("B").ToUpperInvariant();
			case NavNclType.NavMediaSet:
				return ((NavMediaSet)value).ToGuid().ToString("B").ToUpperInvariant();
			case NavNclType.NavCodeunitReference:
				return ((NavCodeunitHandle)value).Target.ToString();
			case NavNclType.NavRecordReference:
				return ((NavRecordHandle)value).Target.ToString();
			case NavNclType.NavFormReference:
				return ((NavFormHandle)value).Target.ToString();
			case NavNclType.NavXmlPortReference:
				return ((NavXmlPortHandle)value).Target.ToString();
			case NavNclType.NavReportReference:
				return ((NavReportHandle)value).Target.ToString();
			case NavNclType.NavBlob:
				if (!((NavBLOB)value).ALHasValue)
				{
					return " ";
				}
				return "*";
			case NavNclType.NavFieldRef:
			case NavNclType.NavVariant:
				if (!value.IsInitialized)
				{
					return string.Empty;
				}
				return NavFormatEvaluateHelper.Format(value.InnerValue, length, formatNumber);
			case NavNclType.NavRecordRef:
			{
				NavRecordRef navRecordRef = (NavRecordRef)value;
				if (!navRecordRef.HasTarget)
				{
					return string.Empty;
				}
				return NavFormatEvaluateHelper.Format(navRecordRef.Record, length, formatNumber);
			}
			case NavNclType.NavRecord:
			{
				NavRecord navRecord = (NavRecord)value;
				if (!navRecord.IsOpen)
				{
					return navRecord.GetType().FullName;
				}
				return navRecord.RecordImplementation.Format();
			}
			case NavNclType.NavQuery:
			{
				NavQuery navQuery = (NavQuery)value;
				return navQuery.Caption;
			}
			case NavNclType.NavDotNet:
				return FormatDotNet(value, formatNumber);
			case NavNclType.NavCodeunit:
			{
				NavCodeunit navCodeunit = (NavCodeunit)value;
				if (navCodeunit.ObjectId.ObjectNumber == 0)
				{
					return string.Empty;
				}
				return navCodeunit.ObjectId.ObjectNumber.ToString(CultureInfo.InvariantCulture);
			}
			case NavNclType.NavReport:
			{
				NavReport navReport = (NavReport)value;
				if (navReport.ReportId == 0)
				{
					return string.Empty;
				}
				return navReport.ReportId.ToString(CultureInfo.InvariantCulture);
			}
			case NavNclType.NavXmlPort:
			{
				NavXmlPort navXmlPort = (NavXmlPort)value;
				if (navXmlPort.XmlPortId == 0)
				{
					return string.Empty;
				}
				return navXmlPort.XmlPortId.ToString(CultureInfo.InvariantCulture);
			}
			case NavNclType.NavForm:
			{
				NavForm navForm = (NavForm)value;
				if (navForm.FormId == 0)
				{
					return string.Empty;
				}
				return navForm.FormId.ToString(CultureInfo.InvariantCulture);
			}
			case NavNclType.NavFile:
			{
				NavFile navFile = (NavFile)value;
				if (string.IsNullOrEmpty(navFile.ALName))
				{
					return string.Empty;
				}
				return navFile.ALName;
			}
			case NavNclType.NavNotification:
				return (value as NavNotification)?.ALMessage ?? string.Empty;
			case NavNclType.NavErrorInfo:
				return (value as NavALErrorInfo)?.ToString();
			case NavNclType.NavHttpClient:
				return (value as NavHttpClient)?.ALGetBaseAddress ?? string.Empty;
			case NavNclType.NavHttpRequestMessage:
				return (value as NavHttpRequestMessage)?.ALMethod ?? string.Empty;
			case NavNclType.NavHttpResponseMessage:
				if ((value as NavHttpResponseMessage)?.ALContent == null)
				{
					return string.Empty;
				}
				return "*";
			case NavNclType.NavSessionSettings:
				return (value as NavSessionSettings)?.Value.ToString();
			case NavNclType.NavHttpContent:
				if ((value as NavHttpContent)?.Content == null)
				{
					return string.Empty;
				}
				return "*";
			case NavNclType.NavHttpHeaders:
				if ((value as NavHttpHeaders)?.Headers == null)
				{
					return string.Empty;
				}
				return "*";
			case NavNclType.NavXmlAttributeCollection:
			case NavNclType.NavXmlNamespaceManager:
			case NavNclType.NavXmlNameTable:
			case NavNclType.NavXmlNodeList:
				if (!(value as IXmlValueContainer<object>).HasValue)
				{
					return string.Empty;
				}
				return "*";
			case NavNclType.NavXmlReadOptions:
			case NavNclType.NavXmlWriteOptions:
			case NavNclType.NavList:
			case NavNclType.NavDictionary:
			case NavNclType.NavWebServiceActionContext:
				return string.Empty;
			case NavNclType.NavSecurityFiltering:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavSecurityFiltering)value).Value.ToString("D");
			case NavNclType.NavExecutionMode:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavExecutionMode)value).Value.ToString("D");
			case NavNclType.NavExecutionContext:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavExecutionContext)value).Value.ToString("D");
			case NavNclType.NavTransactionType:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavTransactionType)value).Value.ToString("D");
			case NavNclType.NavClientType:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavEnumClientType)value).Value.ToString("D");
			case NavNclType.NavObjectType:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavEnumObjectType)value).Value.ToString("D");
			case NavNclType.NavTextEncoding:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavEnumTextEncoding)value).Value.ToString("D");
			case NavNclType.NavReportFormat:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavEnumReportFormat)value).Value.ToString("D");
			case NavNclType.NavDefaultLayout:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavEnumDefaultLayout)value).Value.ToString("D");
			case NavNclType.NavTableConnectionType:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavTableConnectionType)value).Value.ToString("D");
			case NavNclType.NavNotificationScope:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavNotificationScope)value).Value.ToString("D");
			case NavNclType.NavTestPermissions:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavEnumTestPermissions)value).Value.ToString("D");
			case NavNclType.NavDataClassification:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavDataClassification)value).Value.ToString("D");
			case NavNclType.NavVerbosity:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavVerbosity)value).Value.ToString("D");
			case NavNclType.NavTestTransactionModel:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavTestTransactionModel)value).Value.ToString("D");
			case NavNclType.NavWebServiceActionResultCode:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavWebServiceActionResultCode)value).Value.ToString("D");
			case NavNclType.NavDataScope:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavEnumDataScope)value).Value.ToString("D");
			case NavNclType.NavALErrorType:
				if (formatNumber != 2 && formatNumber != 9)
				{
					return value.ToString();
				}
				return ((NavALErrorType)value).Value.ToString("D");
			case NavNclType.NavFieldClass:
				return ((NavFieldClass)value).ToString();
			case NavNclType.NavFieldType:
				return ((NavFieldType)value).ToString();
			case NavNclType.NavFormResult:
				return ((NavFormResult)value).ToString();
			}
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavNclTypeHelper.GetNavTypeFromNclType(value.NclType));
	}

	internal override string GetStandardFormat(int formatNumber)
	{
		if ((uint)formatNumber <= 2u || formatNumber == 9)
		{
			return FormatSettings.GenericStdFormatStrings[formatNumber];
		}
		throw NavNCLInvalidStandardFormatException.Create(CultureInfo.CurrentCulture, formatNumber, NavType.None);
	}

	internal override bool PadLeft(NavValue value)
	{
		return false;
	}

	internal override string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		return Format(value, options.Length, format[pos++], settings);
	}

	private static string FormatDotNet(NavValue value, int formatNumber)
	{
		if (value is NavDotNet navDotNet)
		{
			if (navDotNet.RunOnClient)
			{
				if (navDotNet.ValidRemoteObject && !NavAutomationHelper.HasScriptableTag(navDotNet.GetRemoteHandle().Handle))
				{
					return navDotNet.InvokeMethod<string>("ToString", 0u, Array.Empty<object>());
				}
				return "null";
			}
			if (navDotNet.Value != null)
			{
				return FormatDotNetObject(navDotNet.Value, formatNumber);
			}
			return "null";
		}
		return string.Empty;
	}

	internal static string FormatDotNetObject(object dotNetObject, int formatNumber)
	{
		CultureInfo provider = ((formatNumber == 9) ? CultureInfo.InvariantCulture : NavCurrentThread.Session.WindowsCulture);
		return string.Format(provider, "{0}", dotNetObject);
	}
}
