using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

public static class ScaffoldingHelper
{
	public static string AlName(this MemberInfo member)
	{
		string text = member.NavNameAttributeValue();
		if (text == null)
		{
			text = member.Name;
			if (text.Length > 0)
			{
				text = char.ToUpper(text[0], CultureInfo.InvariantCulture) + ((text.Length > 1) ? text.Substring(1) : string.Empty);
			}
		}
		return text;
	}

	public static bool IsDebuggerAvailable(this MemberInfo memberInfo)
	{
		if (memberInfo.GetCustomAttribute<DebuggerNonUserCodeAttribute>() != null)
		{
			return false;
		}
		DebuggerBrowsableAttribute customAttribute = memberInfo.GetCustomAttribute<DebuggerBrowsableAttribute>();
		if (customAttribute != null)
		{
			return customAttribute.State != DebuggerBrowsableState.Never;
		}
		return true;
	}

	public static string GetTypeString(ApplicationObjectId objectId)
	{
		NavSession session = NavCurrentThread.Session;
		if (objectId.ObjectType == ObjectType.Table)
		{
			string tableNameByTableId = session.NCLMetadata.GetTableNameByTableId(objectId.ObjectNumber, onlySearchCache: true);
			if (tableNameByTableId != null)
			{
				return GetTypeString(objectId, tableNameByTableId);
			}
		}
		string objectName;
		try
		{
			objectName = session.GetObjectName(objectId);
		}
		catch (NavNCLObjectNotFoundException)
		{
			objectName = null;
		}
		return GetTypeString(objectId, objectName);
	}

	public static string GetTypeString(Type type, object instance)
	{
		if (type.IsEnum)
		{
			if (type == typeof(TransactionType))
			{
				return "TransactionType";
			}
			if (type == typeof(TableConnectionType))
			{
				return "TableConnectionType";
			}
			if (type == typeof(FormResult))
			{
				return "Action";
			}
			if (type == typeof(ExecutionMode))
			{
				return "ExecutionMode";
			}
			if (type == typeof(ExecutionContext))
			{
				return "ExecutionContext";
			}
			if (type == typeof(NavClientType))
			{
				return "ClientType";
			}
			if (type == typeof(NavObjectType))
			{
				return "ObjectType";
			}
			if (type == typeof(NavDefaultLayout))
			{
				return "DefaultLayout";
			}
			if (type == typeof(NavReportFormat))
			{
				return "ReportFormat";
			}
			if (type == typeof(TextEncoding))
			{
				return "TextEncoding";
			}
			if (type == typeof(NotificationScope))
			{
				return "NotificationScope";
			}
			if (type == typeof(DataClassification))
			{
				return "DataClassification";
			}
			if (type == typeof(SecurityFiltering))
			{
				return "SecurityFilter";
			}
			if (type == typeof(TestTransactionModel))
			{
				return "TransactionModel";
			}
			if (type == typeof(ALVerbosity))
			{
				return "Verbosity";
			}
			if (type == typeof(NavType))
			{
				return "FieldType";
			}
			if (type == typeof(FieldClass))
			{
				return "FieldClass";
			}
			if (type == typeof(CompressionType))
			{
				return "CompressionType";
			}
		}
		NavNclType nclTypeFromType = NavNclTypeHelper.GetNclTypeFromType(type);
		return GetTypeString(nclTypeFromType, instance);
	}

	public static string GetTypeString(NavNclType navNclType, object instance)
	{
		switch (navNclType)
		{
		case NavNclType.NavBoolean:
			return "Boolean";
		case NavNclType.NavByte:
			return "Byte";
		case NavNclType.NavChar:
			return "Char";
		case NavNclType.NavInteger:
			return "Integer";
		case NavNclType.NavBigInteger:
			return "BigInteger";
		case NavNclType.NavDecimal:
			return "Decimal";
		case NavNclType.NavOption:
			return "Option";
		case NavNclType.NavDuration:
			return "Duration";
		case NavNclType.NavOemText:
			return GetTypeAndLength("OemText", instance);
		case NavNclType.NavText:
			return GetTypeAndLength("Text", instance);
		case NavNclType.NavOemCode:
			return GetTypeAndLength("OemCode", instance);
		case NavNclType.NavCode:
			return GetTypeAndLength("Code", instance);
		case NavNclType.NavBigText:
			return "BigText";
		case NavNclType.NavDate:
			return "Date";
		case NavNclType.NavTime:
			return "Time";
		case NavNclType.NavDateTime:
			return "DateTime";
		case NavNclType.NavGuid:
			return "GUID";
		case NavNclType.NavRecordId:
			return "RecordID";
		case NavNclType.NavDateFormula:
			return "DateFormula";
		case NavNclType.NavFormResult:
			return "Action";
		case NavNclType.NavTransactionType:
			return "TransactionType";
		case NavNclType.NavTableConnectionType:
			return "TableConnectionType";
		case NavNclType.NavClientType:
			return "ClientType";
		case NavNclType.NavObjectType:
			return "ObjectType";
		case NavNclType.NavDefaultLayout:
			return "DefaultLayout";
		case NavNclType.NavSecurityFiltering:
			return "SecurityFilter";
		case NavNclType.NavReportFormat:
			return "ReportFormat";
		case NavNclType.NavTextEncoding:
			return "TextEncoding";
		case NavNclType.NavFieldClass:
			return "FieldClass";
		case NavNclType.NavFieldType:
			return "FieldType";
		case NavNclType.NavKeyRef:
			return "KeyRef";
		case NavNclType.NavFieldRef:
			return "FieldRef";
		case NavNclType.NavVariant:
			return "Variant";
		case NavNclType.NavInStream:
			return "InStream";
		case NavNclType.NavOutStream:
			return "OutStream";
		case NavNclType.NavFile:
			return "File";
		case NavNclType.NavRecordRef:
			return "RecordRef";
		case NavNclType.NavDialog:
			return "Dialog";
		case NavNclType.NavAutomation:
			if (instance is NavAutomation { AutomationComponentType: var automationComponentType })
			{
				return automationComponentType.ToString();
			}
			return NavAutomation.ComponentType.Automation.ToString();
		case NavNclType.NavDotNet:
			return "DotNet";
		case NavNclType.NavRecord:
		case NavNclType.NavRecordReference:
			return "Record";
		case NavNclType.NavCodeunit:
		case NavNclType.NavCodeunitReference:
			return "Codeunit";
		case NavNclType.NavForm:
		case NavNclType.NavFormReference:
			return "Page";
		case NavNclType.NavReport:
		case NavNclType.NavReportReference:
			return "Report";
		case NavNclType.NavXmlPort:
		case NavNclType.NavXmlPortReference:
			return "XMLPort";
		case NavNclType.NavQuery:
		case NavNclType.NavQueryReference:
			return "Query";
		case NavNclType.NavTestPage:
		case NavNclType.NavTestPageReference:
			return "TestPage";
		case NavNclType.NavBlob:
			return "BLOB";
		case NavNclType.NavTableFilter:
			return "TableFilter";
		case NavNclType.NavTextConstant:
			return "Text Constant";
		case NavNclType.NavExecutionMode:
			return "ExecutionMode";
		case NavNclType.NavExecutionContext:
			return "ExecutionContext";
		case NavNclType.NavFilterPageBuilder:
			return "FilterPageBuilder";
		case NavNclType.NavMedia:
			return "Media";
		case NavNclType.NavMediaSet:
			return "MediaSet";
		case NavNclType.NavNotification:
			return "Notification";
		case NavNclType.NavNotificationScope:
			return "NotificationScope";
		case NavNclType.NavTestPermissions:
			return "TestPermissions";
		case NavNclType.NavDataClassification:
			return "DataClassification";
		case NavNclType.NavVerbosity:
			return "Verbosity";
		case NavNclType.NavTestTransactionModel:
			return "TransactionModel";
		case NavNclType.NavHttpClient:
			return "HttpClient";
		case NavNclType.NavHttpRequestMessage:
			return "HttpRequestMessage";
		case NavNclType.NavHttpResponseMessage:
			return "HttpResponseMessage";
		case NavNclType.NavSessionSettings:
			return "SessionSettings";
		case NavNclType.NavHttpContent:
			return "HttpContent";
		case NavNclType.NavHttpHeaders:
			return "HttpHeaders";
		case NavNclType.NavJsonToken:
			return "JsonToken";
		case NavNclType.NavJsonValue:
			return "JsonValue";
		case NavNclType.NavJsonArray:
			return "JsonArray";
		case NavNclType.NavJsonObject:
			return "JsonObject";
		case NavNclType.NavVersion:
			return "Version";
		case NavNclType.NavInstalledAppInfo:
			return "InstalledAppInfo";
		case NavNclType.NavAppDependencyInfo:
			return "AppDependencyInfo";
		case NavNclType.NavXmlAttribute:
			return "XmlAttribute";
		case NavNclType.NavXmlAttributeCollection:
			return "XmlAttributeCollection";
		case NavNclType.NavXmlComment:
			return "XmlComment";
		case NavNclType.NavXmlCData:
			return "XmlCData";
		case NavNclType.NavXmlDeclaration:
			return "XmlDeclaration";
		case NavNclType.NavXmlDocument:
			return "XmlDocument";
		case NavNclType.NavXmlDocumentType:
			return "XmlDocumentType";
		case NavNclType.NavXmlElement:
			return "XmlElement";
		case NavNclType.NavXmlNamespaceManager:
			return "XmlNamespaceManager";
		case NavNclType.NavXmlNameTable:
			return "XmlNameTable";
		case NavNclType.NavXmlNode:
			return "XmlNode";
		case NavNclType.NavXmlNodeList:
			return "XmlNodeList";
		case NavNclType.NavXmlProcessingInstruction:
			return "XmlProcessingInstruction";
		case NavNclType.NavXmlText:
			return "XmlText";
		case NavNclType.NavXmlWriteOptions:
			return "XmlWriteOptions";
		case NavNclType.NavXmlReadOptions:
			return "XmlReadOptions";
		case NavNclType.NavList:
			return "List";
		case NavNclType.NavDictionary:
			return "Dictionary";
		case NavNclType.NavTextBuilder:
			return "TextBuilder";
		case NavNclType.NavWebServiceActionContext:
			return "WebServiceActionContext";
		case NavNclType.NavWebServiceActionResultCode:
			return "WebServiceResultCode";
		case NavNclType.NavDataScope:
			return "DataScope";
		case NavNclType.NavALErrorType:
			return "ALErrorType";
		case NavNclType.NavErrorInfo:
			return "ALErrorInfo";
		default:
			throw new ArgumentOutOfRangeException("navNclType");
		}
	}

	public static string GetTypeString(ObjectType objectType)
	{
		return objectType switch
		{
			ObjectType.TableData => "TableData", 
			ObjectType.Table => "Table", 
			ObjectType.Form => "Form", 
			ObjectType.Report => "Report", 
			ObjectType.Dataport => "Dataport", 
			ObjectType.CodeUnit => "Codeunit", 
			ObjectType.XmlPort => "XMLport", 
			ObjectType.MenuSuite => "MenuSuite", 
			ObjectType.Page => "Page", 
			ObjectType.Query => "Query", 
			ObjectType.System => "System", 
			ObjectType.FieldNumber => "FieldNumber", 
			ObjectType.PageExtension => "PageExtension", 
			ObjectType.TableExtension => "TableExtension", 
			_ => throw new ArgumentOutOfRangeException("objectType"), 
		};
	}

	public static string GetTypeString(NavApplicationObjectBase applicationObjectBase)
	{
		return GetTypeString(applicationObjectBase.ObjectId, applicationObjectBase.ObjectName);
	}

	public static string GetTypeString(ApplicationObjectId objectId, string objectName)
	{
		return GetTypeString(GetTypeString(objectId.ObjectType), objectName, objectId.ObjectNumber);
	}

	public static string GetTypeString(string objectType, string objectName, int objectId)
	{
		return string.Format(CultureInfo.CurrentCulture, "{0} {1} ({2})", objectType, objectName, objectId);
	}

	public static string GetTypeAndLength(string typeName, object instance)
	{
		NavValue navValue = ((!(instance is IArrayAccess arrayAccess)) ? (instance as NavValue) : (arrayAccess[new int[arrayAccess.Dimensions.Length]] as NavValue));
		if (navValue == null || !(navValue is NavStringValue) || navValue.NavDefinedLengthMetadata == 0)
		{
			return typeName;
		}
		return typeName + "[" + navValue.NavDefinedLengthMetadata + "]";
	}

	public static string NavNameAttributeValue(this ICustomAttributeProvider attributeProvider)
	{
		object[] customAttributes = attributeProvider.GetCustomAttributes(typeof(NavNameAttribute), inherit: false);
		int num = 0;
		if (num < customAttributes.Length)
		{
			NavNameAttribute navNameAttribute = (NavNameAttribute)customAttributes[num];
			return navNameAttribute.Name;
		}
		object[] customAttributes2 = attributeProvider.GetCustomAttributes(typeof(ReturnValueAttribute), inherit: false);
		int num2 = 0;
		if (num2 < customAttributes2.Length)
		{
			ReturnValueAttribute returnValueAttribute = (ReturnValueAttribute)customAttributes2[num2];
			if (!string.IsNullOrEmpty(returnValueAttribute.VariableName))
			{
				return returnValueAttribute.VariableName;
			}
			return "<Return Value>";
		}
		return null;
	}

	internal static string ByteToStringValue(byte instance)
	{
		CultureInfo windowsCulture = NavCurrentThread.Session.WindowsCulture;
		StringBuilder stringBuilder = new StringBuilder(instance.ToString(windowsCulture));
		char c = NCLManagedAdapter.ByteToTextChar(instance);
		if (!char.IsControl(c))
		{
			stringBuilder.AppendFormat(windowsCulture, " ('{0}')", c);
		}
		return stringBuilder.ToString();
	}
}
