using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavTypeKindExtensions
{
	internal static ObjectType ToLicensePermissionObjectType(this NavTypeKind navTypeKind)
	{
		switch (navTypeKind)
		{
		case NavTypeKind.PageExtension:
		case NavTypeKind.PageCustomization:
			return ObjectType.Page;
		case NavTypeKind.TableExtension:
		case NavTypeKind.EnumExtension:
		case NavTypeKind.Enum:
			return ObjectType.Table;
		default:
			return navTypeKind.ToObjectType();
		}
	}

	internal static bool IsInFreePublishingRange(this IApplicationObjectTypeSymbol applicationObject)
	{
		if (applicationObject.Id >= 50000)
		{
			return applicationObject.Id <= 99000;
		}
		return false;
	}

	internal static bool IsSubjectToLicenseCheck(this IApplicationObjectTypeSymbol applicationObject)
	{
		return applicationObject.NavTypeKind.IsSubjectToLicenseCheck();
	}

	internal static bool IsExtensionObject(this IApplicationObjectTypeSymbol applicationObject)
	{
		return applicationObject.NavTypeKind.IsExtensionObject();
	}

	internal static bool IsExtensionObject(this NavTypeKind navTypeKind)
	{
		if ((uint)(navTypeKind - 106) <= 1u || navTypeKind == NavTypeKind.EnumExtension)
		{
			return true;
		}
		return false;
	}

	internal static bool IsSubjectToLicenseCheck(this NavTypeKind navTypeKind)
	{
		return navTypeKind != NavTypeKind.PageCustomization;
	}

	public static NavAppObjectFormat ToObjectFormat(this NavTypeKind navTypeKind)
	{
		switch (navTypeKind)
		{
		case NavTypeKind.PageExtension:
		case NavTypeKind.TableExtension:
		case NavTypeKind.PageCustomization:
		case NavTypeKind.EnumExtension:
			return (NavAppObjectFormat)2;
		default:
			return (NavAppObjectFormat)0;
		}
	}

	public static ObjectType ToObjectType(this NavTypeKind navTypeKind)
	{
		switch (navTypeKind)
		{
		case NavTypeKind.Codeunit:
			return ObjectType.CodeUnit;
		case NavTypeKind.Record:
			return ObjectType.Table;
		case NavTypeKind.Page:
			return ObjectType.Page;
		case NavTypeKind.PageExtension:
		case NavTypeKind.PageCustomization:
			return ObjectType.PageExtension;
		case NavTypeKind.TableExtension:
			return ObjectType.TableExtension;
		case NavTypeKind.Report:
			return ObjectType.Report;
		case NavTypeKind.XmlPort:
			return ObjectType.XmlPort;
		case NavTypeKind.Query:
			return ObjectType.Query;
		case NavTypeKind.Enum:
			return ObjectType.Enum;
		case NavTypeKind.EnumExtension:
			return ObjectType.EnumExtension;
		default:
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unsupported symbol type {0}", navTypeKind));
		}
	}
}
