using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavApplicationObjectBaseExtensions
{
	public static AppId GetOwningAppId(this NavApplicationObjectBase navObject)
	{
		AppId val = null;
		switch (navObject.NavType)
		{
		case NavType.Codeunit:
		{
			NavCodeunit obj5 = navObject as NavCodeunit;
			object obj6;
			if (obj5 == null)
			{
				obj6 = null;
			}
			else
			{
				NCLMetaCodeunit metaCodeunit = obj5.MetaCodeunit;
				if (metaCodeunit == null)
				{
					obj6 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp3 = metaCodeunit.OwningApp;
					obj6 = ((owningApp3 != null) ? owningApp3.AppId : null);
				}
			}
			val = (AppId)obj6;
			break;
		}
		case NavType.Form:
		{
			NavForm obj11 = navObject as NavForm;
			object obj12;
			if (obj11 == null)
			{
				obj12 = null;
			}
			else
			{
				NCLMetaForm nclMetaForm = obj11.NclMetaForm;
				if (nclMetaForm == null)
				{
					obj12 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp6 = nclMetaForm.OwningApp;
					obj12 = ((owningApp6 != null) ? owningApp6.AppId : null);
				}
			}
			val = (AppId)obj12;
			break;
		}
		case NavType.Record:
		{
			NavRecord obj9 = navObject as NavRecord;
			object obj10;
			if (obj9 == null)
			{
				obj10 = null;
			}
			else
			{
				NCLMetaTable metaTable = obj9.MetaTable;
				if (metaTable == null)
				{
					obj10 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp5 = metaTable.OwningApp;
					obj10 = ((owningApp5 != null) ? owningApp5.AppId : null);
				}
			}
			val = (AppId)obj10;
			break;
		}
		case NavType.XmlPort:
		{
			NavXmlPort obj13 = navObject as NavXmlPort;
			object obj14;
			if (obj13 == null)
			{
				obj14 = null;
			}
			else
			{
				NCLMetaXmlPort nclMetaXmlPort = obj13.NclMetaXmlPort;
				if (nclMetaXmlPort == null)
				{
					obj14 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp7 = nclMetaXmlPort.OwningApp;
					obj14 = ((owningApp7 != null) ? owningApp7.AppId : null);
				}
			}
			val = (AppId)obj14;
			break;
		}
		case NavType.Report:
		{
			NavReport obj7 = navObject as NavReport;
			object obj8;
			if (obj7 == null)
			{
				obj8 = null;
			}
			else
			{
				NCLMetaReport nclMetaReport = obj7.NclMetaReport;
				if (nclMetaReport == null)
				{
					obj8 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp4 = nclMetaReport.OwningApp;
					obj8 = ((owningApp4 != null) ? owningApp4.AppId : null);
				}
			}
			val = (AppId)obj8;
			break;
		}
		case NavType.Query:
		{
			NavQuery obj3 = navObject as NavQuery;
			object obj4;
			if (obj3 == null)
			{
				obj4 = null;
			}
			else
			{
				NCLMetaQuery nCLMetaQuery = obj3.NCLMetaQuery;
				if (nCLMetaQuery == null)
				{
					obj4 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp2 = nCLMetaQuery.OwningApp;
					obj4 = ((owningApp2 != null) ? owningApp2.AppId : null);
				}
			}
			val = (AppId)obj4;
			break;
		}
		case NavType.PageExtension:
		{
			NavFormExtension obj15 = navObject as NavFormExtension;
			object obj16;
			if (obj15 == null)
			{
				obj16 = null;
			}
			else
			{
				NCLPageExtension nclPageExtension = obj15.NclPageExtension;
				if (nclPageExtension == null)
				{
					obj16 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp8 = nclPageExtension.OwningApp;
					obj16 = ((owningApp8 != null) ? owningApp8.AppId : null);
				}
			}
			val = (AppId)obj16;
			break;
		}
		case NavType.TableExtension:
		{
			NavRecordExtension obj = navObject as NavRecordExtension;
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				NCLTableExtension nclTableExtension = obj.NclTableExtension;
				if (nclTableExtension == null)
				{
					obj2 = null;
				}
				else
				{
					NavAppRuntimeMetadata owningApp = nclTableExtension.OwningApp;
					obj2 = ((owningApp != null) ? owningApp.AppId : null);
				}
			}
			val = (AppId)obj2;
			break;
		}
		}
		return val ?? AppId.None;
	}
}
