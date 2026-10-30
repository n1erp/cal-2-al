using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public static class NavDesignerALFunctions
{
	public static int CreateTableField(int pageId, string fieldName, int fieldType, int fieldLength = 0, Dictionary<object, object> properties = null)
	{
		int sourceTable = NavGlobal.NCLMetadata.GetMetaFormById(pageId, -1, requireCompiled: false).SourceTable;
		int num;
		if (NavCurrentThread.Session.CurrentDesignerExtension.Objects.Tables.ContainsKey(sourceTable))
		{
			NavDesignerTable navDesignerTable = NavCurrentThread.Session.CurrentDesignerExtension.Objects.Tables[sourceTable];
			num = navDesignerTable.AddField(fieldName, (NavType)fieldType, fieldLength);
		}
		else
		{
			string tableName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Table, sourceTable));
			NavDesignerTableExtension navDesignerTableExtension = NavCurrentThread.Session.CurrentDesignerExtension.Objects.TableExtensions.Values.SingleOrDefault((NavDesignerTableExtension t) => t.ExtendsTableName.Equals(tableName));
			if (navDesignerTableExtension == null)
			{
				int unusedObjectId = NavDesignerUtil.GetUnusedObjectId(ObjectType.TableExtension, NavCurrentThread.Session.CurrentDesignerExtension.Manifest.IdSpaces);
				string objectName = NavDesignerUtil.GenerateExtensionObjectName(ObjectType.TableExtension, unusedObjectId);
				navDesignerTableExtension = NavDesignerTableExtension.Create(unusedObjectId, objectName, tableName);
				NavCurrentThread.Session.CurrentDesignerExtension.Objects.TableExtensions.Add(unusedObjectId, navDesignerTableExtension);
			}
			num = navDesignerTableExtension.AddField(fieldName, (NavType)fieldType, fieldLength);
		}
		try
		{
			if (num != 0)
			{
				SetTableFieldProperties(pageId, num, properties);
				NavCurrentThread.Session.CurrentDesignerExtension?.Save();
				return num;
			}
		}
		catch (InvalidOperationException)
		{
			num = 0;
		}
		catch (NavDesignerCompilerException)
		{
			num = 0;
		}
		catch (Exception)
		{
			num = 0;
		}
		return num;
	}

	public static bool SetTableFieldProperty(int pageId, int fieldId, int property, object propertyValue)
	{
		int sourceTable = NavGlobal.NCLMetadata.GetMetaFormById(pageId, -1, requireCompiled: false).SourceTable;
		return FindDesignerTableField(sourceTable, fieldId)?.SetProperty((PropertyKind)property, propertyValue) ?? false;
	}

	public static bool SetTableFieldProperties(int pageId, int fieldId, Dictionary<object, object> properties)
	{
		int sourceTable = NavGlobal.NCLMetadata.GetMetaFormById(pageId, -1, requireCompiled: false).SourceTable;
		NavDesignerTableField navDesignerTableField = FindDesignerTableField(sourceTable, fieldId);
		if (properties != null)
		{
			foreach (KeyValuePair<object, object> property in properties)
			{
				if (navDesignerTableField == null || !navDesignerTableField.SetProperty((PropertyKind)property.Key, property.Value))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static NavDesignerTableField FindDesignerTableField(int tableId, int fieldId)
	{
		if (NavCurrentThread.Session.CurrentDesignerExtension.Objects.Tables.ContainsKey(tableId))
		{
			NavDesignerTable navDesignerTable = NavCurrentThread.Session.CurrentDesignerExtension.Objects.Tables[tableId];
			return navDesignerTable.Fields.SingleOrDefault((NavDesignerTableField f) => f.Number == fieldId);
		}
		string tableName = NavGlobal.NCLMetadata.GetObjectName(new ApplicationObjectId(ObjectType.Table, tableId));
		return NavCurrentThread.Session.CurrentDesignerExtension.Objects.TableExtensions.Values.SingleOrDefault((NavDesignerTableExtension t) => t.ExtendsTableName.Equals(tableName))?.Fields.SingleOrDefault((NavDesignerTableField f) => f.Number == fieldId);
	}

	public static int CreateNewPage(string pageName, string pageType = null, int tableId = 0)
	{
		string sourceTable = ((tableId > 0) ? NavGlobal.NCLMetadata.GetTableNameByTableId(tableId) : null);
		int unusedObjectId = NavDesignerUtil.GetUnusedObjectId(ObjectType.Page, NavCurrentThread.Session.CurrentDesignerExtension.Manifest.IdSpaces);
		NavDesignerPage navDesignerPage = NavDesignerPage.Create(unusedObjectId, pageName, pageType, sourceTable);
		if (tableId != 0)
		{
			navDesignerPage.AddPageControlGroup("General");
			MetaTable tableMetadata = NavCurrentThread.Session.MetadataProvider.GetTableMetadata(tableId);
			MetaField metaField = tableMetadata.Fields.First();
			navDesignerPage.AddPageField(metaField.Name, metaField.Name, "General", null);
		}
		NavCurrentThread.Session.CurrentDesignerExtension.Objects.Pages[unusedObjectId] = navDesignerPage;
		NavCurrentThread.Session.CurrentDesignerExtension.Save();
		return unusedObjectId;
	}

	public static string GetDesignerExtensionName()
	{
		if (NavCurrentThread.Session.CurrentDesignerExtension is NavDevelopmentDesignerExtension { PackageType: NavAppPackageType.Extension } navDevelopmentDesignerExtension)
		{
			return navDevelopmentDesignerExtension.Manifest.AppName;
		}
		return string.Empty;
	}

	public static string GetDesignerExtensionPublisher()
	{
		if (NavCurrentThread.Session.CurrentDesignerExtension is NavDevelopmentDesignerExtension { PackageType: NavAppPackageType.Extension } navDevelopmentDesignerExtension)
		{
			return navDevelopmentDesignerExtension.Manifest.AppPublisher;
		}
		return string.Empty;
	}

	public static bool GetDesignerExtensionShowMyCode()
	{
		if (NavCurrentThread.Session.CurrentDesignerExtension is NavDevelopmentDesignerExtension { PackageType: NavAppPackageType.Extension } navDevelopmentDesignerExtension)
		{
			return navDevelopmentDesignerExtension.Manifest.ShowMyCode;
		}
		return true;
	}

	public static bool ExtensionNameAndPublisherIsValid(string name, string publisher)
	{
		if (NavCurrentThread.Session.CurrentDesignerExtension is NavDevelopmentDesignerExtension { PackageType: var packageType })
		{
			switch (packageType)
			{
			case NavAppPackageType.Extension:
				return true;
			case NavAppPackageType.Designer:
				return !(from m in NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(NavCurrentThread.Session.Tenant.Id)
					where m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && m.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)
					select m).Any();
			}
		}
		return false;
	}

	public static void SaveDesignerExtension(string name, string publisher)
	{
		if (NavCurrentThread.Session.CurrentDesignerExtension is NavDevelopmentDesignerExtension navDevelopmentDesignerExtension)
		{
			navDevelopmentDesignerExtension.SaveDesignerExtensionAsFullExtension(name, publisher);
			if (NavCurrentThread.Session.ApplicationExtension == Guid.Empty)
			{
				NavCurrentThread.Session.CurrentDesignerExtension = null;
			}
		}
	}

	public static void GenerateDesignerPackageZipStreamByVersion(Stream outputStream, Guid appId, string version)
	{
		NavDesignerExportManager.GenerateZipStream(outputStream, AppId.FromGuid(appId), new Version(version));
	}

	public static void GenerateDesignerPackageZipStream(Stream outputStream, string publisher, string name)
	{
		NavDesignerExportManager.GenerateZipStream(outputStream, publisher, name, string.Empty, string.Empty, (Uri)null);
	}

	public static void GenerateDesignerPackageZipStream(Stream outputStream, string publisher, string name, string description, string eula, Uri url)
	{
		NavDesignerExportManager.GenerateZipStream(outputStream, publisher, name, description, eula, url);
	}

	public static string SanitizeDesignerPathName(string pathName)
	{
		pathName = pathName.TrimStart('\\');
		return NavFileHelper.SanitizePath(pathName);
	}

	public static string SanitizeDesignerFileName(string fileName, char replacementCharacter = '_')
	{
		return NavFileHelper.SanitizeFileName(fileName, replacementCharacter);
	}
}
