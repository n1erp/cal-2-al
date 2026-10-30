using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavNclTypeHelper
{
	private static NavType[] nclToNavType;

	private static NavValue[] fixedDefaultValues;

	private static Dictionary<NavType, NavNclType> navToNclType;

	private static Dictionary<Type, NavNclType> typeToNclType;

	public static NavType GetNavTypeFromNclType(NavNclType navNclType)
	{
		if (nclToNavType == null)
		{
			InitializeStaticInfo();
		}
		return nclToNavType[(int)navNclType];
	}

	public static NavNclType GetNclTypeFromNavType(NavType navType)
	{
		if (navType == NavType.NotSupported_Binary)
		{
			throw NavNCLNotSupportedTypeException.CreateUnsupportedType(CultureInfo.CurrentUICulture, "Binary");
		}
		if (navToNclType == null)
		{
			InitializeStaticInfo();
		}
		if (navToNclType.TryGetValue(navType, out var value))
		{
			return value;
		}
		throw new NavNCLTypeMappingException();
	}

	public static NavNclType GetNclTypeFromObject(object obj)
	{
		return GetNclTypeFromObject(obj, forDotNet: false);
	}

	public static NavNclType GetNclTypeFromObject(object value, bool forDotNet)
	{
		if (value == null)
		{
			return NavNclType.None;
		}
		if (value is NavObject navObject)
		{
			return navObject.NclType;
		}
		if (value is CurrencyWrapper)
		{
			return NavNclType.NavDecimal;
		}
		return GetNclTypeFromType(value.GetType(), forDotNet);
	}

	public static NavNclType GetNclTypeFromType(Type type)
	{
		return GetNclTypeFromType(type, forDotNet: false);
	}

	public static NavNclType GetNclTypeFromType(Type type, bool forDotNet)
	{
		if (typeToNclType == null)
		{
			InitializeStaticInfo();
		}
		if (typeToNclType.TryGetValue(type, out var value))
		{
			return value;
		}
		if (type.IsSubclassOf(typeof(NavApplicationObjectBase)))
		{
			Type baseType = type.BaseType;
			while (baseType != typeof(NavApplicationObjectBase))
			{
				if (typeToNclType.TryGetValue(baseType, out value))
				{
					return value;
				}
				baseType = baseType.BaseType;
			}
		}
		if (type.IsCOMObject || type == typeof(NavAutomationHandle))
		{
			return NavNclType.NavAutomation;
		}
		if (forDotNet)
		{
			return NavNclType.NavDotNet;
		}
		if (TryGetNclTypeFromGenericType(type, out value))
		{
			return value;
		}
		throw NavNCLNotSupportedTypeException.CreateUnknownType(CultureInfo.CurrentCulture, type.ToString());
	}

	public static NavType GetNavTypeFromType(Type type, bool forDotNet)
	{
		return GetNavTypeFromNclType(GetNclTypeFromType(type, forDotNet));
	}

	public static NavType GetNavTypeFromType(Type type)
	{
		return GetNavTypeFromType(type, forDotNet: false);
	}

	public static bool HasMaxLength(NavNclType nclType)
	{
		if ((uint)(nclType - 8) <= 1u || nclType == NavNclType.NavDateFormula)
		{
			return true;
		}
		return false;
	}

	public static NavValue GetFixedDefaultNavValue(INavValueMetadata metadata)
	{
		switch (metadata.NclType)
		{
		case NavNclType.NavCode:
			return NavCode.Default(metadata.NavDefinedLengthMetadata);
		case NavNclType.NavOemText:
			return NavOemText.Default(metadata.NavDefinedLengthMetadata);
		case NavNclType.NavOemCode:
			return NavOemCode.Default(metadata.NavDefinedLengthMetadata);
		case NavNclType.NavText:
			return NavText.Default(metadata.NavDefinedLengthMetadata);
		default:
			if (fixedDefaultValues == null)
			{
				InitializeStaticInfo();
			}
			return fixedDefaultValues[(int)metadata.NclType];
		}
	}

	private static bool TryGetNclTypeFromGenericType(Type type, out NavNclType nclType)
	{
		if (type.IsSubClassOfGenericType(typeof(NavList<>)))
		{
			nclType = NavNclType.NavList;
			return true;
		}
		if (type.IsSubClassOfGenericType(typeof(NavDictionary<, >)))
		{
			nclType = NavNclType.NavDictionary;
			return true;
		}
		nclType = NavNclType.None;
		return false;
	}

	private static void InitializeStaticInfo()
	{
		int[] array = (int[])Enum.GetValues(typeof(NavNclType));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != i)
			{
				throw new NavNCLEnumerationNotSequencedException();
			}
		}
		NavType[] array2 = new NavType[array.Length];
		NavValue[] tempFixedDefaultValues = new NavValue[array.Length];
		Dictionary<NavType, NavNclType> tempNavToNclType = new Dictionary<NavType, NavNclType>(array2.Length);
		Dictionary<Type, NavNclType> tempTypeToNclType = new Dictionary<Type, NavNclType>(array2.Length + 10);
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.None, NavType.None);
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavBoolean, NavType.Boolean, NavBoolean.Default, typeof(NavBoolean), typeof(bool));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavByte, NavType.Byte, NavByte.Default, typeof(NavByte), typeof(byte));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavInteger, NavType.Integer, NavInteger.Default, typeof(NavInteger), typeof(short), typeof(int), typeof(ushort), typeof(sbyte));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavBigInteger, NavType.BigInteger, NavBigInteger.Default, typeof(NavBigInteger), typeof(long), typeof(uint));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDecimal, NavType.Decimal, NavDecimal.Default, typeof(NavDecimal), typeof(decimal), typeof(Decimal18), typeof(float), typeof(double), typeof(ulong));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavOption, NavType.Option, typeof(NavOption));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDuration, NavType.Duration, NavDuration.Default, typeof(NavDuration), typeof(TimeSpan));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavOemText, NavType.OemText, typeof(NavOemText));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavOemCode, NavType.OemCode, typeof(NavOemCode));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTextConstant, NavType.TextConst, typeof(NavTextConstant));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavBigText, NavType.BigText, typeof(NavBigText));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavText, NavType.Text, typeof(NavText), typeof(string));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavCode, NavType.Code, typeof(NavCode));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavChar, NavType.Char, NavChar.Default, typeof(NavChar), typeof(char));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDate, NavType.Date, NavDate.Default, typeof(NavDate));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTime, NavType.Time, NavTime.Default, typeof(NavTime));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDateTime, NavType.DateTime, NavDateTime.Default, typeof(NavDateTime), typeof(DateTime));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavGuid, NavType.GUID, NavGuid.Default, typeof(NavGuid), typeof(Guid));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavRecordId, NavType.RecordID, NavRecordId.Default, typeof(NavRecordId));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDateFormula, NavType.DateFormula, typeof(NavDateFormula));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTableFilter, NavType.TableFilter, NavTableFilter.Default, typeof(NavTableFilter));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFormResult, NavType.Action, typeof(NavFormResult));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFieldClass, NavType.FieldClass, typeof(NavFieldClass));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFieldType, NavType.TypeId, typeof(NavFieldType));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTransactionType, NavType.TransactionType, typeof(NavTransactionType));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTableConnectionType, NavType.TableConnectionType, typeof(NavTableConnectionType));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavKeyRef, NavType.KeyRef, typeof(NavKeyRef));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFieldRef, NavType.FieldRef, typeof(NavFieldRef));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavVariant, NavType.Variant, typeof(NavVariant));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavStream, NavType.Stream, typeof(NavStream), typeof(Stream));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavInStream, NavType.InStream, typeof(NavInStream));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavOutStream, NavType.OutStream, typeof(NavOutStream));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavBlob, NavType.BLOB, typeof(NavBLOB));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFile, NavType.File, typeof(NavFile));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavRecordRef, NavType.RecordRef, typeof(NavRecordRef));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDialog, NavType.Dialog, typeof(NavDialog));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavAutomation, NavType.Automation, typeof(NavAutomation));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDotNet, NavType.DotNet, typeof(NavDotNet));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavRecord, NavType.Record, typeof(NavRecord));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavCodeunit, NavType.Codeunit, typeof(NavCodeunit));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavForm, NavType.Form, typeof(NavForm));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavReport, NavType.Report, typeof(NavReport));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlPort, NavType.XmlPort, typeof(NavXmlPort));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavQuery, NavType.Query, typeof(NavQuery));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTestPage, NavType.TestPage, typeof(NavTestPage));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavClrObject, NavType.ClrByteArray, typeof(byte[]));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavRecordReference, NavType.RecordReference, typeof(NavRecordHandle), typeof(INavRecordHandle));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavCodeunitReference, NavType.CodeunitReference, typeof(NavCodeunitHandle));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFormReference, NavType.FormReference, typeof(NavFormHandle));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavReportReference, NavType.ReportReference, typeof(NavReportHandle));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlPortReference, NavType.XmlPortReference, typeof(NavXmlPortHandle));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavQueryReference, NavType.QueryReference, typeof(NavQueryHandle));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTestPageReference, NavType.TestPageReference, typeof(NavTestPageHandle));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTestAction, NavType.TestAction, typeof(NavTestAction));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTestField, NavType.TestField, typeof(NavTestField));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavExecutionMode, NavType.ExecutionMode, typeof(NavExecutionMode));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavExecutionContext, NavType.ExecutionContext, typeof(NavExecutionContext));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavSecurityFiltering, NavType.SecurityFiltering, typeof(NavSecurityFiltering));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavClientType, NavType.ClientType, typeof(NavEnumClientType));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFilterPageBuilder, NavType.FilterPageBuilder, typeof(NavFilterPageBuilder));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavObjectType, NavType.ObjectType, typeof(NavEnumObjectType));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavVerbosity, NavType.Verbosity, typeof(NavVerbosity));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTestTransactionModel, NavType.TestTransactionModel, typeof(NavTestTransactionModel));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTextEncoding, NavType.TextEncoding, typeof(NavEnumTextEncoding));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavReportFormat, NavType.ReportFormat, typeof(NavEnumReportFormat));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDefaultLayout, NavType.DefaultLayout, typeof(NavEnumDefaultLayout));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavNotification, NavType.Notification, typeof(NavNotification));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavNotificationScope, NavType.NotificationScope, typeof(NavNotificationScope));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTestPermissions, NavType.TestPermissions, typeof(NavTestPermissions));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDataClassification, NavType.DataClassification, typeof(DataClassification));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavMedia, NavType.Media, typeof(NavMedia));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavMediaSet, NavType.MediaSet, typeof(NavMediaSet));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavFormExtension, NavType.PageExtension, typeof(NavFormExtension));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavRecordExtension, NavType.TableExtension, typeof(NavRecordExtension));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavHttpClient, NavType.HttpClient, typeof(NavHttpClient));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavHttpRequestMessage, NavType.HttpRequestMessage, typeof(NavHttpRequestMessage));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavHttpResponseMessage, NavType.HttpResponseMessage, typeof(NavHttpResponseMessage));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavSessionSettings, NavType.SessionSettings, typeof(NavSessionSettings));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavHttpContent, NavType.HttpContent, typeof(NavHttpContent));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavHttpHeaders, NavType.HttpHeaders, typeof(NavHttpHeaders));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavTextBuilder, NavType.TextBuilder, typeof(NavTextBuilder));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavJsonToken, NavType.JsonToken, typeof(NavJsonToken));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavJsonValue, NavType.JsonValue, typeof(NavJsonValue));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavJsonArray, NavType.JsonArray, typeof(NavJsonArray));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavJsonObject, NavType.JsonObject, typeof(NavJsonObject));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavVersion, NavType.Version, typeof(NavVersion));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavInstalledAppInfo, NavType.InstalledAppInfo, typeof(NavModuleInfo));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavAppDependencyInfo, NavType.AppDependencyInfo, typeof(NavModuleDependencyInfo));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlAttribute, NavType.XmlAttribute, typeof(NavXmlAttribute));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlAttributeCollection, NavType.XmlAttributeCollection, typeof(NavXmlAttributeCollection));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlComment, NavType.XmlComment, typeof(NavXmlComment));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlCData, NavType.XmlCData, typeof(NavXmlCData));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlDeclaration, NavType.XmlDeclaration, typeof(NavXmlDeclaration));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlDocument, NavType.XmlDocument, typeof(NavXmlDocument));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlDocumentType, NavType.XmlDocumentType, typeof(NavXmlDocumentType));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlElement, NavType.XmlElement, typeof(NavXmlElement));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlNamespaceManager, NavType.XmlNamespaceManager, typeof(NavXmlNamespaceManager));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlNameTable, NavType.XmlNameTable, typeof(NavXmlNameTable));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlNode, NavType.XmlNode, typeof(NavXmlNode));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlNodeList, NavType.XmlNodeList, typeof(NavXmlNodeList));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlProcessingInstruction, NavType.XmlProcessingInstruction, typeof(NavXmlProcessingInstruction));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlText, NavType.XmlText, typeof(NavXmlText));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlWriteOptions, NavType.XmlWriteOptions, typeof(NavXmlWriteOptions));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavXmlReadOptions, NavType.XmlReadOptions, typeof(NavXmlReadOptions));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavList, NavType.List, typeof(NavList<object>));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDictionary, NavType.Dictionary, typeof(NavDictionary<object, object>));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavWebServiceActionContext, NavType.WebServiceActionContext, typeof(NavWebServiceActionContext));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavWebServiceActionResultCode, NavType.WebServiceActionResultCode, typeof(NavWebServiceActionResultCode));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavDataScope, NavType.DataScope, typeof(DataScope));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavALErrorType, NavType.ALErrorType, typeof(ALErrorType));
		AddInfo(array2, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, NavNclType.NavErrorInfo, NavType.ALErrorInfo, typeof(NavALErrorInfo));
		nclToNavType = array2;
		fixedDefaultValues = tempFixedDefaultValues;
		navToNclType = tempNavToNclType;
		typeToNclType = tempTypeToNclType;
		for (int j = 0; j < nclToNavType.Length; j++)
		{
			if (j != 0 && nclToNavType[j] == NavType.None)
			{
				throw new NavNCLEnumerationNotMappedException();
			}
		}
	}

	private static void AddInfo(NavType[] tempNclToNavType, NavValue[] tempFixedDefaultValues, Dictionary<NavType, NavNclType> tempNavToNclType, Dictionary<Type, NavNclType> tempTypeToNclType, NavNclType nclType, NavType navType, params Type[] compatibleTypes)
	{
		AddInfo(tempNclToNavType, tempFixedDefaultValues, tempNavToNclType, tempTypeToNclType, nclType, navType, null, compatibleTypes);
	}

	private static void AddInfo(NavType[] tempNclToNavType, NavValue[] tempFixedDefaultValues, Dictionary<NavType, NavNclType> tempNavToNclType, Dictionary<Type, NavNclType> tempTypeToNclType, NavNclType nclType, NavType navType, NavValue fixedDefaultValue, params Type[] compatibleTypes)
	{
		tempFixedDefaultValues[(int)nclType] = fixedDefaultValue;
		tempNclToNavType[(int)nclType] = navType;
		tempNavToNclType.Add(navType, nclType);
		for (int i = 0; i < compatibleTypes.Length; i++)
		{
			tempTypeToNclType.Add(compatibleTypes[i], nclType);
		}
	}
}
