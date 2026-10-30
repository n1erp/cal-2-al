using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemOperatingSystem
{
	private class UriComponents
	{
		internal string Protocol { get; set; }

		internal string HostName { get; set; }

		internal string Port { get; set; }

		internal string InstanceName { get; set; }

		internal Uri OriginalUrl { get; set; }

		internal UriComponents(string url)
		{
			try
			{
				OriginalUrl = new Uri(url);
				string[] segments = OriginalUrl.Segments;
				Protocol = OriginalUrl.Scheme;
				HostName = OriginalUrl.Host;
				Port = OriginalUrl.Port.ToString(CultureInfo.InvariantCulture);
				InstanceName = ((segments.Length >= 2) ? OriginalUrl.Segments[1].TrimEnd('/') : string.Empty);
			}
			catch (UriFormatException innerException)
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.GetUrlIllegalUrlSyntax, url, ProductNames.ProductNameServer);
				throw new NavNCLGetUrlException(message, innerException);
			}
		}

		internal UriComponents(string protocol, string hostName, string port, string instanceName)
		{
			Protocol = protocol;
			HostName = hostName;
			Port = port;
			InstanceName = instanceName;
		}
	}

	private static UriComponents publicODataUrl;

	private static UriComponents publicSoapUrl;

	private static UriComponents publicWebUrl;

	private static UriComponents publicWinUrl;

	public static string ALApplicationPath
	{
		get
		{
			Assembly assembly = Assembly.GetEntryAssembly();
			if (assembly == null)
			{
				assembly = Assembly.GetExecutingAssembly();
			}
			return Path.GetDirectoryName(assembly.Location) + "\\";
		}
	}

	public static string ALTemporaryPath => NavFile.GetUserDirectoryAndEnsureExists(DataError.ThrowError, tempPath: true);

	public static bool ALGuiAllowed => NavCurrentThread.Session.CallbackAllowed;

	private static bool IsFixedTenantWebServiceEndpointUrlEnabled => !string.IsNullOrWhiteSpace(ServerUserSettings.Instance.FixedTenantWebServiceEndpointBaseUri.Value?.OriginalString);

	private static UriComponents PublicODataUrl
	{
		get
		{
			if (publicODataUrl == null)
			{
				string value = ServerUserSettings.Instance.PublicODataBaseUrl.Value;
				if (!string.IsNullOrEmpty(value))
				{
					publicODataUrl = new UriComponents(value);
					if (string.IsNullOrEmpty(publicODataUrl.InstanceName))
					{
						publicODataUrl.InstanceName = ServerUserSettings.Instance.ServerInstance.Value;
					}
				}
				else
				{
					string port = ServerUserSettings.Instance.ODataServicesPort.Value.ToString(CultureInfo.InvariantCulture);
					string protocol = (ServerUserSettings.Instance.ODataServicesSSLEnabled.Value ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);
					publicODataUrl = new UriComponents(protocol, DnsHelper.MachineName, port, ServerUserSettings.Instance.ServerInstance.Value);
				}
			}
			return publicODataUrl;
		}
	}

	private static UriComponents PublicSoapUrl
	{
		get
		{
			if (publicSoapUrl == null)
			{
				string value = ServerUserSettings.Instance.PublicSOAPBaseUrl.Value;
				if (!string.IsNullOrEmpty(value))
				{
					publicSoapUrl = new UriComponents(value);
					if (string.IsNullOrEmpty(publicSoapUrl.InstanceName))
					{
						publicSoapUrl.InstanceName = ServerUserSettings.Instance.ServerInstance.Value;
					}
				}
				else
				{
					string port = ServerUserSettings.Instance.SOAPServicesPort.Value.ToString(CultureInfo.InvariantCulture);
					string protocol = (ServerUserSettings.Instance.SOAPServicesSSLEnabled.Value ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);
					publicSoapUrl = new UriComponents(protocol, DnsHelper.MachineName, port, ServerUserSettings.Instance.ServerInstance.Value);
				}
			}
			return publicSoapUrl;
		}
	}

	private static UriComponents PublicWebUrl
	{
		get
		{
			if (publicWebUrl == null)
			{
				string value = ServerUserSettings.Instance.PublicWebBaseUrl.Value;
				if (!string.IsNullOrEmpty(value))
				{
					publicWebUrl = new UriComponents(value);
				}
				else
				{
					string port = "48900";
					string uriSchemeHttp = Uri.UriSchemeHttp;
					string text = DnsHelper.MachineName;
					string[] array = text.Split('.');
					if (array.Length >= 1)
					{
						text = array[0];
					}
					publicWebUrl = new UriComponents(uriSchemeHttp, text, port, ServerUserSettings.Instance.ServerInstance.Value);
				}
			}
			return publicWebUrl;
		}
	}

	private static UriComponents PublicTabletUrl
	{
		get
		{
			string value = ServerUserSettings.Instance.AppProtocolName.Value;
			return new UriComponents(value, PublicWebUrl.HostName, PublicWebUrl.Port, PublicWebUrl.InstanceName);
		}
	}

	private static UriComponents PublicWinUrl
	{
		get
		{
			if (publicWinUrl == null)
			{
				string value = ServerUserSettings.Instance.PublicWinBaseUrl.Value;
				if (!string.IsNullOrEmpty(value))
				{
					publicWinUrl = new UriComponents(value);
					if (string.IsNullOrEmpty(publicWinUrl.InstanceName))
					{
						publicWinUrl.InstanceName = ServerUserSettings.Instance.ServerInstance.Value;
					}
					if (publicWinUrl.Port == "-1")
					{
						publicWinUrl.Port = ServerUserSettings.Instance.ClientServicesPort.Value.ToString(CultureInfo.InvariantCulture);
					}
				}
				else
				{
					string port = ServerUserSettings.Instance.ClientServicesPort.Value.ToString(CultureInfo.InvariantCulture);
					publicWinUrl = new UriComponents("dynamicsnav", DnsHelper.MachineName, port, ServerUserSettings.Instance.ServerInstance.Value);
				}
			}
			return publicWinUrl;
		}
	}

	public static bool IsFixedClientEndpointEnabled => !string.IsNullOrWhiteSpace(ServerUserSettings.Instance.FixedWebClientEndpointBaseUri.Value?.OriginalString);

	public static void ALHyperlink(string hyperlink, Guid automationId)
	{
		if (!NavCurrentThread.Session.TestExecution.TestHandleHyperlink(hyperlink) && NavCurrentThread.Session.CallbackAllowed)
		{
			NavCurrentThread.Session.ClientCallback.DialogHyperlink(hyperlink, automationId);
		}
	}

	public static string ALGetUrl(NavClientType clientType)
	{
		return ALGetUrl(clientType, null, NavObjectType.TableData, 0, null, useFilter: false);
	}

	public static string ALGetUrl(NavClientType clientType, string company)
	{
		return ALGetUrl(clientType, company, NavObjectType.TableData, 0, null, useFilter: false);
	}

	public static string ALGetUrl(NavClientType clientType, string company, NavObjectType objectType)
	{
		return ALGetUrl(clientType, company, objectType, -1, null, useFilter: false);
	}

	public static string ALGetUrl(NavClientType clientType, string company, NavObjectType objectType, int objectId)
	{
		return ALGetUrl(clientType, company, objectType, objectId, null, useFilter: false);
	}

	public static string ALGetUrl(NavClientType clientType, string company, NavObjectType objectType, int objectId, object record)
	{
		return ALGetUrl(clientType, company, objectType, objectId, record, useFilter: false);
	}

	public static string ALGetUrl(NavClientType clientType, string company, NavObjectType objectType, int objectId, object record, bool useFilter)
	{
		try
		{
			return GetUrlCore(clientType, company, objectType, objectId, record, useFilter);
		}
		catch (NavBaseException)
		{
			return string.Empty;
		}
	}

	public static string GetUrlCore(NavClientType clientType, string company, NavObjectType objectType, int objectId, object record, bool useFilter)
	{
		if (clientType == NavClientType.Current)
		{
			clientType = ALSession.ALCurrentClientType;
		}
		if (clientType == NavClientType.Default)
		{
			clientType = ALSession.ALDefaultClientType;
		}
		Uri uri = GetTenantAgnosticUrl(clientType, company, objectType, objectId, record, useFilter);
		bool flag;
		switch (clientType)
		{
		case NavClientType.SOAP:
		case NavClientType.OData:
		case NavClientType.ODataV4:
		case NavClientType.Api:
			flag = !IsFixedTenantWebServiceEndpointUrlEnabled;
			break;
		case NavClientType.Web:
		case NavClientType.Tablet:
		case NavClientType.Phone:
			flag = !IsFixedClientEndpointEnabled;
			break;
		default:
			flag = true;
			break;
		}
		if (flag)
		{
			uri = NavEnvironment.Instance.Tenants.AddTenantInfo(NavCurrentThread.Session.Tenant.Id, uri, doNotUpdateUriWithAlternateIdInfo: false);
		}
		return uri.OriginalString;
	}

	private static object UnpackObject(object record)
	{
		if (record == null)
		{
			return null;
		}
		if (record is NavRecord || record is NavRecordRef)
		{
			return record;
		}
		if (!(record is NavVariant navVariant))
		{
			throw new NavNCLArgumentException(Lang.IllegalGetUrlArgument);
		}
		if (navVariant.ALIsRecordRef)
		{
			return navVariant.InnerValue;
		}
		if (navVariant.ALIsRecord)
		{
			if (navVariant.InnerValue is NavRecord result)
			{
				return result;
			}
			if (navVariant.InnerValue is NavRecordHandle navRecordHandle)
			{
				return navRecordHandle.Target;
			}
		}
		throw new NavNCLArgumentException(Lang.IllegalGetUrlArgument);
	}

	public static Uri GetTenantAgnosticUrl(NavClientType clientType, string company, NavObjectType objectType, int objectId, object record, bool useFilter)
	{
		object obj = UnpackObject(record);
		if (clientType == NavClientType.Current)
		{
			clientType = ALSession.ALCurrentClientType;
		}
		if (clientType == NavClientType.Default)
		{
			clientType = ALSession.ALDefaultClientType;
		}
		if (objectType == NavObjectType.TableData)
		{
			if (objectId != 0)
			{
				throw new NavNCLGetUrlException(Lang.GetUrlIllegalObjectId);
			}
		}
		else if (objectId < 1)
		{
			throw new NavNCLGetUrlException(Lang.GetUrlIllegalObjectId);
		}
		string uriString;
		switch (clientType)
		{
		case NavClientType.Windows:
			uriString = BuildWindowsUrl(company, objectType, objectId, obj, useFilter);
			break;
		case NavClientType.Tablet:
		case NavClientType.Phone:
		case NavClientType.Desktop:
			uriString = BuildTabletUrl(company, objectType, objectId, obj, useFilter);
			break;
		case NavClientType.Web:
			uriString = BuildWebUrl(company, objectType, objectId, obj, useFilter);
			break;
		case NavClientType.SOAP:
			uriString = BuildWSUrl(company, objectType, objectId, obj, useFilter);
			break;
		case NavClientType.OData:
			uriString = BuildODataV3Url(company, objectType, objectId, obj, useFilter);
			break;
		case NavClientType.ODataV4:
			uriString = BuildODataV4Url(company, objectType, objectId, obj, useFilter);
			break;
		case NavClientType.Api:
			uriString = BuildApiUrl(company, objectType, objectId, obj, useFilter);
			break;
		default:
			throw new NavNCLGetUrlException(Lang.GetUrlUnknownClientType);
		}
		return new Uri(uriString);
	}

	public static Uri GetOpenDynamicFileHandlerUrl(string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			throw new NavNCLArgumentException("token");
		}
		string uriString = BuildBaseWebUrl() + "/OpenDynamicFileHandler.axd?token=" + token;
		return NavEnvironment.Instance.Tenants.AddTenantInfo(NavCurrentThread.Session.Tenant.Id, new Uri(uriString), ServerUserSettings.Instance.FixedWebClientEndpointBaseUri?.Value != null);
	}

	private static string BuildWindowsUrl(string company, NavObjectType objectType, int objectId, object record, bool useFilter)
	{
		string text = PublicWinUrl.Protocol + "://" + PublicWinUrl.HostName + ":" + PublicWinUrl.Port + "/" + PublicWinUrl.InstanceName + "/";
		if (!string.IsNullOrEmpty(company))
		{
			company = Uri.EscapeDataString(company);
			text += company;
		}
		text += "/";
		string text2 = string.Empty;
		string text3 = objectId.ToString(CultureInfo.InvariantCulture);
		switch (objectType)
		{
		case NavObjectType.Table:
			text2 = "runtable?table=" + text3;
			break;
		case NavObjectType.Page:
			text2 = "runpage?page=" + text3;
			break;
		case NavObjectType.Report:
			text2 = "runreport?report=" + text3;
			break;
		case NavObjectType.Codeunit:
			text2 = "runcodeunit?codeunit=" + text3;
			break;
		case NavObjectType.Query:
			text2 = "runquery?query=" + text3;
			break;
		case NavObjectType.XmlPort:
			text2 = "runxmlport?xmlport=" + text3;
			break;
		default:
			throw new NavNCLGetUrlException(Lang.GetUrlIllegalObjectType);
		case NavObjectType.TableData:
			break;
		}
		text += text2;
		if (record != null)
		{
			if (objectType != NavObjectType.Page)
			{
				throw new NavNCLGetUrlException(Lang.GetUrlIllegalRecordParam);
			}
			if (useFilter)
			{
				string text4 = BuildFilter(record);
				if (!string.IsNullOrWhiteSpace(text4))
				{
					text = text + "&$filter=" + Uri.EscapeUriString(text4);
				}
			}
			else
			{
				string str = BuildBookmark(record);
				text = text + "&bookmark=" + HttpUtility.UrlEncode(str);
			}
		}
		return text;
	}

	private static string BuildWebUrl(string company, NavObjectType objectType, int objectId, object record, bool useFilter)
	{
		string text = BuildBaseWebUrl();
		string text2 = AddWebQueryString(company, objectType, objectId, record, useFilter);
		if (string.IsNullOrEmpty(text2))
		{
			return text;
		}
		return text.TrimEnd('/') + "/" + text2;
	}

	private static string BuildBaseWebUrl()
	{
		return WebBaseUrlWithFixedEndpoint(PublicWebUrl, preserveProtocol: true);
	}

	private static string WebBaseUrlWithFixedEndpoint(UriComponents uriComponents, bool preserveProtocol)
	{
		string text = ServerUserSettings.Instance.FixedWebClientEndpointBaseUri.Value?.OriginalString.TrimEnd('/');
		UriBuilder uriBuilder;
		if (!string.IsNullOrWhiteSpace(text))
		{
			uriBuilder = new UriBuilder(text);
			uriBuilder.Port = -1;
			NavSession session = NavCurrentThread.Session;
			Guid? guid = session.Authenticator?.SecurityToken?.IdentityProviderTenantId;
			string aadTenantId = session.Tenant.TenantSettings.AadTenantId;
			string urlName = session.Tenant.TenantSettings.UrlName;
			List<string> list = uriBuilder.Path.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (guid.HasValue && guid.Value != Guid.Parse(aadTenantId))
			{
				list.Add(aadTenantId);
			}
			if (!string.IsNullOrWhiteSpace(urlName))
			{
				list.Add(urlName);
			}
			uriBuilder.Path = "/" + string.Join("/", list);
			if (!preserveProtocol)
			{
				uriBuilder.Scheme = uriComponents.Protocol;
			}
		}
		else
		{
			if (!int.TryParse(uriComponents.Port, out var result))
			{
				result = -1;
			}
			uriBuilder = new UriBuilder(uriComponents.Protocol, uriComponents.HostName, result);
			if (!string.IsNullOrEmpty(uriComponents.InstanceName))
			{
				uriBuilder.Path += uriComponents.InstanceName;
			}
		}
		return uriBuilder.ToString();
	}

	private static string AddWebQueryString(string company, NavObjectType objectType, int objectId, object record, bool useFilter)
	{
		string text = null;
		string text2 = "?";
		if (!string.IsNullOrEmpty(company))
		{
			company = Uri.EscapeDataString(company);
			text = text + "?company=" + company;
			text2 = "&";
		}
		string text3 = string.Empty;
		string text4 = objectId.ToString(CultureInfo.InvariantCulture);
		switch (objectType)
		{
		case NavObjectType.Page:
			text3 = text2 + "page=" + text4;
			break;
		case NavObjectType.Report:
			text3 = text2 + "report=" + text4;
			break;
		case NavObjectType.Table:
			text3 = text2 + "table=" + text4;
			break;
		case NavObjectType.Query:
			text3 = text2 + "query=" + text4;
			break;
		default:
			throw new NavNCLGetUrlException(Lang.GetUrlIllegalObjectType);
		case NavObjectType.TableData:
			break;
		}
		text += text3;
		if (record != null)
		{
			if (objectType != NavObjectType.Page)
			{
				throw new NavNCLGetUrlException(Lang.GetUrlIllegalRecordParam);
			}
			if (useFilter)
			{
				string text5 = BuildFilter(record);
				if (!string.IsNullOrWhiteSpace(text5))
				{
					text = text + "&filter=" + Uri.EscapeUriString(text5);
				}
			}
			else
			{
				string str = BuildBookmark(record);
				text = text + "&bookmark=" + HttpUtility.UrlEncode(str);
			}
		}
		return text;
	}

	private static string BuildTabletUrl(string company, NavObjectType objectType, int objectId, object record, bool useFilter)
	{
		string text = WebBaseUrlWithFixedEndpoint(PublicTabletUrl, preserveProtocol: false);
		return text + AddWebQueryString(company, objectType, objectId, record, useFilter);
	}

	private static string BuildWSUrl(string company, NavObjectType objectType, int objectId, object recordValue, bool useFilter)
	{
		NavRecord record = recordValue as NavRecord;
		if (useFilter || (recordValue != null && !IsWebServiceRecord(record)))
		{
			throw new NavNCLGetUrlException(Lang.GetUrlIllegalRecordParamWS);
		}
		string text = (IsFixedTenantWebServiceEndpointUrlEnabled ? BuildFixedTenantWebServiceEndpointUrlBase("/WS") : (PublicSoapUrl.Protocol + "://" + PublicSoapUrl.HostName + ":" + PublicSoapUrl.Port + "/" + PublicSoapUrl.InstanceName + "/WS/"));
		if (!string.IsNullOrEmpty(company))
		{
			company = Uri.EscapeDataString(company);
			text = text + company + "/";
		}
		_ = string.Empty;
		return text + objectType switch
		{
			NavObjectType.TableData => "Services", 
			NavObjectType.Page => "Page/" + MetaBase.ExternalizeName(WebServiceHelper.FindWebServiceName(objectType, objectId, record)), 
			NavObjectType.Codeunit => "Codeunit/" + MetaBase.ExternalizeName(WebServiceHelper.FindWebServiceName(objectType, objectId, record)), 
			_ => throw new NavNCLGetUrlException(Lang.GetUrlIllegalObjectType), 
		};
	}

	private static string BuildODataUrlCore(string company, NavObjectType objectType, int objectId, object recordValue, bool useFilter, string oDataRouteName, WebServiceHelper.ODataFilterConverter oDataFilterConverter, WebServiceHelper.PublishedServiceNameFinder publishedServiceNameFinder, WebServiceHelper.CompanyNameGenerator companyNameGenerator)
	{
		if (string.IsNullOrWhiteSpace(oDataRouteName))
		{
			throw new ArgumentNullException("oDataRouteName");
		}
		NavRecord navRecord = recordValue as NavRecord;
		_ = string.Empty;
		if (NavObjectType.Page != objectType && recordValue != null && !IsWebServiceRecord(navRecord))
		{
			throw new NavNCLGetUrlException(Lang.GetUrlIllegalRecordParam);
		}
		oDataRouteName = (oDataRouteName.StartsWith("/", StringComparison.OrdinalIgnoreCase) ? oDataRouteName : ("/" + oDataRouteName));
		string text = (IsFixedTenantWebServiceEndpointUrlEnabled ? BuildFixedTenantWebServiceEndpointUrlBase(oDataRouteName) : (PublicODataUrl.Protocol + "://" + PublicODataUrl.HostName + ":" + PublicODataUrl.Port + "/" + PublicODataUrl.InstanceName + oDataRouteName));
		if (!string.IsNullOrEmpty(company))
		{
			text += companyNameGenerator(company);
		}
		string text2 = string.Empty;
		switch (objectType)
		{
		case NavObjectType.Page:
		case NavObjectType.Query:
			text2 = "/" + MetaBase.ExternalizeName(publishedServiceNameFinder(objectType, objectId, navRecord));
			break;
		default:
			throw new NavNCLGetUrlException(Lang.GetUrlIllegalObjectType);
		case NavObjectType.TableData:
			break;
		}
		text += text2;
		if (NavObjectType.Page == objectType && navRecord != null && !IsWebServiceRecord(navRecord))
		{
			ReadOnlyCollection<NavValue> fields = navRecord.ALRecordId.Fields;
			bool flag = true;
			bool flag2 = fields.Count > 1;
			int index = 0;
			NCLMetaKey nCLMetaKey = navRecord.MetaTable.Keys[0];
			ReadOnlyCollection<NCLMetaField> keyFieldsList = nCLMetaKey.KeyFieldsList;
			foreach (NavValue item in fields)
			{
				string text3 = string.Empty;
				string text4 = item.ToString();
				if (flag2)
				{
					text3 = keyFieldsList[index].FieldName;
					text3 = MetaBase.ExternalizeName(text3) + "=";
				}
				string text5 = string.Empty;
				if (!IsNumeric(keyFieldsList[index++].FieldNclType))
				{
					text5 = "'";
					text4 = HttpUtility.UrlEncode(text4);
				}
				if (flag)
				{
					flag = false;
					text = text + "(" + text3 + text5 + text4 + text5;
				}
				else
				{
					text = text + "," + text3 + text5 + text4 + text5;
				}
			}
			text += ")";
		}
		if (useFilter && navRecord != null)
		{
			switch (objectType)
			{
			case NavObjectType.Page:
			{
				string text6 = oDataFilterConverter(navRecord);
				text = text + "?$filter=" + text6;
				break;
			}
			case NavObjectType.Query:
				throw new NavNotSupportedException("Filters are not supported yet for queries.");
			default:
				throw new NavNCLGetUrlException(Lang.GetUrlIllegalObjectType);
			case NavObjectType.TableData:
				break;
			}
		}
		return text;
	}

	private static string BuildFixedTenantWebServiceEndpointUrlBase(string protocolType)
	{
		string aadTenantId = NavCurrentThread.Session.Tenant.TenantSettings.AadTenantId;
		string urlName = NavCurrentThread.Session.Tenant.TenantSettings.UrlName;
		if (string.Equals(protocolType, "/odata", StringComparison.OrdinalIgnoreCase))
		{
			protocolType = "/ODataV3";
		}
		string text = aadTenantId;
		if (!string.IsNullOrWhiteSpace(urlName))
		{
			text = text + "/" + urlName;
		}
		text += (protocolType.Equals("/ws", StringComparison.OrdinalIgnoreCase) ? EnsureEndsWithSlash(protocolType) : protocolType);
		return EnsureEndsWithSlash(NavCurrentThread.Session.Tenant.FixedTenantWebServiceEndpointBaseUri) + text;
	}

	private static string EnsureEndsWithSlash(string uri)
	{
		if (!uri.EndsWith("/", StringComparison.OrdinalIgnoreCase))
		{
			return uri + "/";
		}
		return uri;
	}

	private static string BuildFilter(object recordValue)
	{
		if (recordValue is NavRecord navRecord)
		{
			return navRecord.GetUrlFilters;
		}
		if (recordValue is NavRecordRef navRecordRef)
		{
			return navRecordRef.GetUrlFilters;
		}
		return string.Empty;
	}

	private static string BuildBookmark(object recordValue)
	{
		if (recordValue is NavRecord navRecord)
		{
			return NavFormatEvaluateHelper.Format(navRecord.ALRecordId, 0, 10);
		}
		if (recordValue is NavRecordRef navRecordRef)
		{
			return NavFormatEvaluateHelper.Format(navRecordRef.ALRecordId, 0, 10);
		}
		return string.Empty;
	}

	private static string BuildODataV3Url(string company, NavObjectType objectType, int objectId, object recordValue, bool useFilter)
	{
		return BuildODataUrlCore(company, objectType, objectId, recordValue, useFilter, "OData", WebServiceHelper.ODataV3FilterConverter, WebServiceHelper.FindWebServiceName, WebServiceHelper.GenerateODataCompanySegment);
	}

	private static string BuildODataV4Url(string company, NavObjectType objectType, int objectId, object recordValue, bool useFilter)
	{
		return BuildODataUrlCore(company, objectType, objectId, recordValue, useFilter, "ODataV4", WebServiceHelper.ODataV4FilterConverter, WebServiceHelper.FindWebServiceName, WebServiceHelper.GenerateODataCompanySegment);
	}

	private static string BuildApiUrl(string company, NavObjectType objectType, int objectId, object recordValue, bool useFilter)
	{
		string oDataRouteName = "api/" + WebServiceHelper.GetApiWebServiceLatestVersionPath((ObjectType)objectType, objectId);
		return BuildODataUrlCore(company, objectType, objectId, recordValue, useFilter, oDataRouteName, WebServiceHelper.FilterNotSupportedConverter, WebServiceHelper.FindApiServiceName, WebServiceHelper.GenerateApiCompanySegment);
	}

	private static bool IsNumeric(NavNclType type)
	{
		if ((uint)(type - 2) <= 3u)
		{
			return true;
		}
		return false;
	}

	private static bool IsWebServiceRecord(NavRecord record)
	{
		bool result = false;
		if (record != null)
		{
			result = record.TableID == 2000000076 || record.TableID == 2000000168;
		}
		return result;
	}
}
