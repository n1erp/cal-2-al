using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal static class ScaffoldingFactory
{
	public static Scaffolding GetApplicationObjectScaffolding(string name, Type targetType, ValueBinder valueBinder, ObjectType objectType, bool promotedChildren, bool hideGlobalsNode)
	{
		switch (objectType)
		{
		case ObjectType.Table:
			if (!hideGlobalsNode)
			{
				return new RecordScaffolding(name, targetType, valueBinder, promotedChildren);
			}
			break;
		case ObjectType.Report:
			return new ReportScaffolding(name, targetType, valueBinder, promotedChildren, hideGlobalsNode);
		case ObjectType.XmlPort:
			return new XmlPortScaffolding(name, targetType, valueBinder, promotedChildren, hideGlobalsNode);
		case ObjectType.Query:
			return new QueryScaffolding(name, targetType, valueBinder, promotedChildren);
		}
		return new ApplicationObjectScaffolding<NavApplicationObjectBase>(name, targetType, valueBinder, promotedChildren, hideGlobalsNode);
	}

	public static Scaffolding GetNavValueScaffolding(string name, Type targetType, NavNclType nclType, ValueBinder valueBinder, object parent, bool promotedChildren, params ScaffoldingValueCoercerBase[] valueCoercers)
	{
		object value = valueBinder.GetValue(parent);
		if (value == null)
		{
			return new NavValueScaffolding(name, targetType, nclType, valueBinder);
		}
		switch (nclType)
		{
		case NavNclType.NavRecord:
		case NavNclType.NavCodeunit:
		case NavNclType.NavForm:
		case NavNclType.NavReport:
		case NavNclType.NavXmlPort:
		case NavNclType.NavQuery:
		case NavNclType.NavFormExtension:
			return GetApplicationObjectScaffolding(name, targetType, valueBinder, NclTypeToObjectType(nclType), promotedChildren, hideGlobalsNode: false);
		case NavNclType.NavRecordReference:
		case NavNclType.NavCodeunitReference:
		case NavNclType.NavFormReference:
		case NavNclType.NavReportReference:
		case NavNclType.NavXmlPortReference:
		case NavNclType.NavQueryReference:
		{
			INavApplicationObjectBaseHandle navApplicationObjectBaseHandle2 = (INavApplicationObjectBaseHandle)Scaffolding.ApplyValueCoercers(valueBinder.GetValue(parent), valueCoercers);
			if (navApplicationObjectBaseHandle2.HasTarget)
			{
				Scaffolding applicationObjectScaffolding = GetApplicationObjectScaffolding(name, targetType, valueBinder, navApplicationObjectBaseHandle2.Target.ObjectId.ObjectType, promotedChildren, hideGlobalsNode: false);
				applicationObjectScaffolding.ValueCoercers.Add(new HandleScaffoldingValueCoercer(nclType));
				return applicationObjectScaffolding;
			}
			return new UninitializedHandleScaffolding(name, targetType, valueBinder, promotedChildren);
		}
		case NavNclType.NavTestPage:
			return new TestPageScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavTestPageReference:
		{
			INavApplicationObjectBaseHandle navApplicationObjectBaseHandle = (INavApplicationObjectBaseHandle)Scaffolding.ApplyValueCoercers(valueBinder.GetValue(parent), valueCoercers);
			if (navApplicationObjectBaseHandle.HasTarget)
			{
				Scaffolding scaffolding = new TestPageScaffolding(name, targetType, valueBinder, promotedChildren);
				scaffolding.ValueCoercers.Add(new HandleScaffoldingValueCoercer(nclType));
				return scaffolding;
			}
			return new UninitializedTestPageScaffolding(name, targetType, valueBinder, promotedChildren);
		}
		case NavNclType.NavRecordRef:
		{
			NavRecordRef navRecordRef = (NavRecordRef)Scaffolding.ApplyValueCoercers(valueBinder.GetValue(parent), valueCoercers);
			if (navRecordRef.Record != null)
			{
				RecordScaffolding recordScaffolding = new RecordScaffolding(name, navRecordRef.Target.GetType(), valueBinder, promotedChildren);
				recordScaffolding.ValueCoercers.Add(new RecordRefScaffoldingValueCoercer());
				return recordScaffolding;
			}
			return new UninitializedRecordRefScaffolding(name, targetType, valueBinder, promotedChildren);
		}
		case NavNclType.NavFieldRef:
			return new FieldRefScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavKeyRef:
			return new KeyRefScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavVariant:
			return new VariantScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavBlob:
			return new BlobScaffolding(name, targetType, valueBinder);
		case NavNclType.NavBigText:
			return new BigTextScaffolding(name, targetType, valueBinder);
		case NavNclType.NavDotNet:
			return new DotNetScaffolding(name, targetType, valueBinder);
		case NavNclType.NavNotification:
			return new NotificationScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavSessionSettings:
			return new SessionSettingsScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavHttpClient:
			return new HttpClientScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavHttpHeaders:
			return new HttpHeadersScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavHttpResponseMessage:
			return new HttpResponseMessageScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavHttpRequestMessage:
			return new HttpRequestMessageScaffolding(name, targetType, valueBinder, promotedChildren);
		case NavNclType.NavJsonToken:
		case NavNclType.NavJsonValue:
		case NavNclType.NavJsonArray:
		case NavNclType.NavJsonObject:
			return new JsonScaffolding(name, targetType, nclType, valueBinder, promotedChildren);
		case NavNclType.NavXmlAttribute:
		case NavNclType.NavXmlComment:
		case NavNclType.NavXmlCData:
		case NavNclType.NavXmlDocument:
		case NavNclType.NavXmlElement:
		case NavNclType.NavXmlNode:
		case NavNclType.NavXmlText:
			return new XmlScaffolding(name, targetType, nclType, valueBinder, promotedChildren);
		default:
			return new NavValueScaffolding(name, targetType, nclType, valueBinder);
		}
	}

	public static Scaffolding GetScaffolding(string name, Type targetType, ValueBinder valueBinder, object parent, bool promotedChildren, params ScaffoldingValueCoercerBase[] valueCoercers)
	{
		if (targetType.IsEnum)
		{
			return new EnumScaffolding(name, targetType, valueBinder);
		}
		TypeCode typeCode = Type.GetTypeCode(targetType);
		if ((uint)(typeCode - 2) <= 14u || typeCode == TypeCode.String)
		{
			return new TypeScaffolding(name, targetType, valueBinder);
		}
		if (targetType == typeof(Decimal18) || targetType == typeof(Guid) || targetType == typeof(object) || targetType == typeof(EventArgs))
		{
			return new TypeScaffolding(name, targetType, valueBinder);
		}
		if (typeof(IArrayAccess).IsAssignableFrom(targetType))
		{
			return new ArrayScaffolding(name, targetType, valueBinder, promotedChildren);
		}
		if (typeof(IByRef).IsAssignableFrom(targetType))
		{
			Type targetType2 = targetType.GetGenericArguments()[0];
			ByRefScaffoldingValueCoercer byRefScaffoldingValueCoercer = new ByRefScaffoldingValueCoercer(targetType);
			Array.Resize(ref valueCoercers, valueCoercers.Length + 1);
			valueCoercers[valueCoercers.Length - 1] = byRefScaffoldingValueCoercer;
			Scaffolding scaffolding = GetScaffolding(name, targetType2, valueBinder, parent, promotedChildren, valueCoercers);
			scaffolding.ValueCoercers.Add(byRefScaffoldingValueCoercer);
			return scaffolding;
		}
		NavNclType nclTypeFromType = NavNclTypeHelper.GetNclTypeFromType(targetType);
		return GetNavValueScaffolding(name, targetType, nclTypeFromType, valueBinder, parent, promotedChildren, valueCoercers);
	}

	private static ObjectType NclTypeToObjectType(NavNclType nclType)
	{
		return nclType switch
		{
			NavNclType.NavRecord => ObjectType.Table, 
			NavNclType.NavCodeunit => ObjectType.CodeUnit, 
			NavNclType.NavForm => ObjectType.Page, 
			NavNclType.NavXmlPort => ObjectType.XmlPort, 
			NavNclType.NavReport => ObjectType.Report, 
			NavNclType.NavQuery => ObjectType.Query, 
			NavNclType.NavFormExtension => ObjectType.PageExtension, 
			NavNclType.NavRecordExtension => ObjectType.TableExtension, 
			_ => throw new ArgumentOutOfRangeException(nclType.ToString()), 
		};
	}
}
