using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class NavValueFormatter
{
	private static readonly NavValueFormatter navCharFormatter = new NavCharFormatter();

	private static readonly NavValueFormatter navTextFormatter = new NavTextFormatter();

	private static readonly NavValueFormatter navTextBuilderFormatter = new NavTextBuilderFormatter();

	private static readonly NavValueFormatter navTimeFormatter = new NavTimeFormatter();

	private static readonly NavValueFormatter navDateFormatter = new NavDateFormatter();

	private static readonly NavValueFormatter navDateTimeFormatter = new NavDateTimeFormatter();

	private static readonly NavValueFormatter navDurationFormatter = new NavDurationFormatter();

	private static readonly NavValueFormatter navDecimalFormatter = new NavDecimalFormatter();

	private static readonly NavValueFormatter navOptionFormatter = new NavOptionFormatter();

	private static readonly NavValueFormatter navBooleanFormatter = new NavBooleanFormatter();

	private static readonly NavValueFormatter navByteFormatter = new NavByteFormatter();

	private static readonly NavValueFormatter navIntegerFormatter = new NavIntegerFormatter();

	private static readonly NavValueFormatter navBigIntegerFormatter = new NavBigIntegerFormatter();

	private static readonly NavValueFormatter navTestPageFormatter = new NavTestPageFormatter();

	private static readonly NavTableFilterFormatter navTableFilterFormatter = new NavTableFilterFormatter();

	private static readonly NavValueFormatter navDateFormulaFormatter = new NavDateFormulaFormatter();

	private static readonly NavValueFormatter navGenericFormatter = new NavGenericFormatter();

	private static readonly NavValueFormatter navRecordIdFormatter = new NavRecordIdFormatter();

	private static readonly NavValueFormatter navJsonFormatter = new NavJsonFormatter();

	private static readonly NavValueFormatter navXmlNodeFormatter = new NavXmlNodeFormatter();

	private static readonly NavValueFormatter navFilterPageBuilderFormatter = new NavFilterPageBuilderFormatter();

	private static readonly NavValueFormatter navVersionFormatter = new NavVersionFormatter();

	internal static NavValueFormatter GetFormatter(NavValue value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		switch (value.NclType)
		{
		case NavNclType.NavChar:
			return navCharFormatter;
		case NavNclType.NavOemText:
		case NavNclType.NavOemCode:
		case NavNclType.NavTextConstant:
		case NavNclType.NavText:
		case NavNclType.NavCode:
			return navTextFormatter;
		case NavNclType.NavTime:
			return navTimeFormatter;
		case NavNclType.NavDate:
			return navDateFormatter;
		case NavNclType.NavDateFormula:
			return navDateFormulaFormatter;
		case NavNclType.NavDateTime:
			return navDateTimeFormatter;
		case NavNclType.NavDuration:
			return navDurationFormatter;
		case NavNclType.NavDecimal:
			return navDecimalFormatter;
		case NavNclType.NavOption:
			return navOptionFormatter;
		case NavNclType.NavBoolean:
			return navBooleanFormatter;
		case NavNclType.NavByte:
			return navByteFormatter;
		case NavNclType.NavInteger:
			return navIntegerFormatter;
		case NavNclType.NavBigInteger:
			return navBigIntegerFormatter;
		case NavNclType.NavRecordId:
			return navRecordIdFormatter;
		case NavNclType.NavTestField:
		case NavNclType.NavTestAction:
		case NavNclType.NavTestPage:
		case NavNclType.NavTestPageReference:
			return navTestPageFormatter;
		case NavNclType.NavTableFilter:
			return navTableFilterFormatter;
		case NavNclType.NavFilterPageBuilder:
			return navFilterPageBuilderFormatter;
		case NavNclType.NavVersion:
			return navVersionFormatter;
		case NavNclType.NavJsonToken:
		case NavNclType.NavJsonValue:
		case NavNclType.NavJsonArray:
		case NavNclType.NavJsonObject:
			return navJsonFormatter;
		case NavNclType.NavXmlAttribute:
		case NavNclType.NavXmlComment:
		case NavNclType.NavXmlCData:
		case NavNclType.NavXmlDeclaration:
		case NavNclType.NavXmlDocument:
		case NavNclType.NavXmlDocumentType:
		case NavNclType.NavXmlElement:
		case NavNclType.NavXmlNode:
		case NavNclType.NavXmlProcessingInstruction:
		case NavNclType.NavXmlText:
			return navXmlNodeFormatter;
		case NavNclType.NavTextBuilder:
			return navTextBuilderFormatter;
		case NavNclType.NavBigText:
		case NavNclType.NavMedia:
		case NavNclType.NavMediaSet:
		case NavNclType.NavNotification:
		case NavNclType.NavNotificationScope:
		case NavNclType.NavGuid:
		case NavNclType.NavFormResult:
		case NavNclType.NavFieldClass:
		case NavNclType.NavFieldType:
		case NavNclType.NavTransactionType:
		case NavNclType.NavExecutionMode:
		case NavNclType.NavExecutionContext:
		case NavNclType.NavSecurityFiltering:
		case NavNclType.NavClientType:
		case NavNclType.NavTableConnectionType:
		case NavNclType.NavObjectType:
		case NavNclType.NavTextEncoding:
		case NavNclType.NavDataScope:
		case NavNclType.NavReportFormat:
		case NavNclType.NavDefaultLayout:
		case NavNclType.NavTestPermissions:
		case NavNclType.NavSessionSettings:
		case NavNclType.NavDataClassification:
		case NavNclType.NavVerbosity:
		case NavNclType.NavTestTransactionModel:
		case NavNclType.NavWebServiceActionResultCode:
		case NavNclType.NavALErrorType:
		case NavNclType.NavKeyRef:
		case NavNclType.NavFieldRef:
		case NavNclType.NavVariant:
		case NavNclType.NavErrorInfo:
		case NavNclType.NavBlob:
		case NavNclType.NavFile:
		case NavNclType.NavRecordRef:
		case NavNclType.NavDotNet:
		case NavNclType.NavRecord:
		case NavNclType.NavCodeunit:
		case NavNclType.NavForm:
		case NavNclType.NavReport:
		case NavNclType.NavXmlPort:
		case NavNclType.NavQuery:
		case NavNclType.NavRecordReference:
		case NavNclType.NavCodeunitReference:
		case NavNclType.NavFormReference:
		case NavNclType.NavReportReference:
		case NavNclType.NavXmlPortReference:
		case NavNclType.NavHttpClient:
		case NavNclType.NavHttpRequestMessage:
		case NavNclType.NavHttpResponseMessage:
		case NavNclType.NavHttpContent:
		case NavNclType.NavHttpHeaders:
		case NavNclType.NavXmlAttributeCollection:
		case NavNclType.NavXmlNamespaceManager:
		case NavNclType.NavXmlNameTable:
		case NavNclType.NavXmlNodeList:
		case NavNclType.NavXmlReadOptions:
		case NavNclType.NavXmlWriteOptions:
		case NavNclType.NavList:
		case NavNclType.NavDictionary:
		case NavNclType.NavWebServiceActionContext:
			return navGenericFormatter;
		default:
			throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, value.NclType.ToString());
		}
	}

	internal string Format(NavValue value, int length, int formatNumber, FormatSettings formatsetting)
	{
		return AdjustResultLength(FormatWithFormatNumber(value, length, formatNumber, formatsetting), length, value);
	}

	internal virtual string Format(NavValue value, int length, string format, FormatSettings formatSetting)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int number = -1;
		format = NavFormatEvaluateHelper.EvaluateFormatString(value, format ?? string.Empty, out number);
		return NavFormatEvaluateHelper.FormatWithParsedFormatString(value, length, format, number, formatSetting);
	}

	internal abstract string FormatWithFormatNumber(NavValue value, int length, int formatNumber, FormatSettings formatsetting);

	internal abstract string GetStandardFormat(int formatNumber);

	internal abstract bool PadLeft(NavValue value);

	internal virtual ReadOnlyCollection<string> GetFieldNames()
	{
		return null;
	}

	internal virtual string FormatField(NavValue value, FormatOptions options, FormatSettings settings, string format, ref int pos)
	{
		throw new NotSupportedException(value.NclType.ToString());
	}

	internal static string AdjustResultLength(string result, int length, NavValue value)
	{
		if (result != null && length != 0)
		{
			if (Math.Abs(length) < result.Length)
			{
				return result.Remove(Math.Abs(length));
			}
			if (length > result.Length)
			{
				if (!GetFormatter(value).PadLeft(value))
				{
					return result.PadRight(length, ' ');
				}
				return result.PadLeft(length, ' ');
			}
		}
		return result;
	}

	protected static void AppendValueQuoteIfNecessary(StringBuilder stringBuilder, string value, char quoteIfContains, bool quoteEmpty)
	{
		if (value != null && (quoteEmpty || !string.IsNullOrEmpty(value)))
		{
			if (string.IsNullOrEmpty(value) || value.IndexOf(quoteIfContains) >= 0 || value[0] == '"' || value[0] == ' ' || value[value.Length - 1] == ' ')
			{
				stringBuilder.Append("\"");
				int length = stringBuilder.Length;
				stringBuilder.Append(value);
				stringBuilder.Replace("\"", "\"\"", length, value.Length);
				stringBuilder.Append("\"");
			}
			else
			{
				stringBuilder.Append(value);
			}
		}
	}
}
