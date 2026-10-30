using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class PermissionSetGenerator
{
	private class Permission
	{
		private const string ObjectTypeTableData = "0";

		private static readonly Permission DefaultTableData = new Permission(null, null, "1", "1", "1", "1", "0");

		private static readonly Permission DefaultObject = new Permission(null, null, "0", "0", "0", "0", "1");

		public string ObjectType { get; }

		public string ObjectID { get; }

		public string ReadPermission { get; }

		public string InsertPermission { get; }

		public string ModifyPermission { get; }

		public string DeletePermission { get; }

		public string ExecutePermission { get; }

		public string SecurityFilter { get; }

		private static Permission GetDefaultPermissions(string objectType)
		{
			if (!string.Equals(objectType, "0", StringComparison.Ordinal))
			{
				return DefaultObject;
			}
			return DefaultTableData;
		}

		public Permission(string objectType, string objectId, string readPermission = null, string insertPermission = null, string modifyPermission = null, string deletePermission = null, string executePermission = null, string securityFilter = null)
		{
			Permission defaultPermissions = GetDefaultPermissions(objectType);
			ObjectType = objectType ?? string.Empty;
			ObjectID = objectId ?? string.Empty;
			ReadPermission = readPermission ?? defaultPermissions.ReadPermission;
			InsertPermission = insertPermission ?? defaultPermissions.InsertPermission;
			ModifyPermission = modifyPermission ?? defaultPermissions.ModifyPermission;
			DeletePermission = deletePermission ?? defaultPermissions.DeletePermission;
			ExecutePermission = executePermission ?? defaultPermissions.ExecutePermission;
			SecurityFilter = securityFilter ?? string.Empty;
		}
	}

	private static readonly ImmutableArray<SymbolKind> applicationObjectSymbolKinds = new SymbolKind[6]
	{
		SymbolKind.Codeunit,
		SymbolKind.Table,
		SymbolKind.Page,
		SymbolKind.Report,
		SymbolKind.XmlPort,
		SymbolKind.Query
	}.ToImmutableArrayOrEmpty();

	public static void Generate(Stream fileStream, Compilation compilation)
	{
		GetExistingPermissions(fileStream, out var existingPermissions, out var roleId, out var roleName);
		roleName = roleName ?? compilation.ModuleInfo.Name;
		roleId = roleId ?? roleName.ToUpperInvariant();
		IEnumerable<Permission> newPermissions = GetAllObjectFromExtensionAsPermissions(compilation);
		newPermissions = UseAlreadyExistingPermissionsIfPossible(existingPermissions, newPermissions);
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Encoding = Encoding.UTF8,
			Indent = true
		};
		fileStream.Position = 0L;
		fileStream.SetLength(0L);
		XmlWriter writer = XmlWriter.Create(fileStream, settings);
		try
		{
			writer.WriteStartEnd("PermissionSets", () =>
			{
				writer.WriteStartEnd("PermissionSet", () =>
				{
					writer.WriteAttributeString("RoleID", roleId);
					writer.WriteAttributeString("RoleName", roleName);
					foreach (Permission permission in newPermissions)
					{
						writer.WriteStartEnd("Permission", () =>
						{
							writer.WriteElementString("ObjectType", permission.ObjectType);
							writer.WriteElementString("ObjectID", permission.ObjectID);
							writer.WriteElementString("ReadPermission", permission.ReadPermission);
							writer.WriteElementString("InsertPermission", permission.InsertPermission);
							writer.WriteElementString("ModifyPermission", permission.ModifyPermission);
							writer.WriteElementString("DeletePermission", permission.DeletePermission);
							writer.WriteElementString("ExecutePermission", permission.ExecutePermission);
							writer.WriteElementString("SecurityFilter", permission.SecurityFilter);
						});
					}
				});
			});
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	private static IEnumerable<Permission> UseAlreadyExistingPermissionsIfPossible(List<Permission> existingPermissions, IEnumerable<Permission> newPermissions)
	{
		return newPermissions.Select((Permission newPermission) =>
		{
			Permission permission = existingPermissions.FirstOrDefault((Permission existing) => existing.ObjectID == newPermission.ObjectID && existing.ObjectType == newPermission.ObjectType);
			return (permission != null) ? permission : newPermission;
		});
	}

	private static void GetExistingPermissions(Stream fileStream, out List<Permission> existingPermissions, out string roleId, out string roleName)
	{
		try
		{
			XmlReaderSettings settings = new XmlReaderSettings
			{
				XmlResolver = null,
				CloseInput = false
			};
			fileStream.Position = 0L;
			using XmlReader reader = XmlReader.Create(fileStream, settings);
			XDocument xDocument = XDocument.Load(reader);
			IEnumerable<XElement> source = xDocument.Root.Descendants("Permission");
			existingPermissions = source.Select((XElement p) => new Permission(p.Element("ObjectType")?.Value, p.Element("ObjectID")?.Value, p.Element("ReadPermission")?.Value, p.Element("InsertPermission")?.Value, p.Element("ModifyPermission")?.Value, p.Element("DeletePermission")?.Value, p.Element("ExecutePermission")?.Value, p.Element("SecurityFilter")?.Value)).ToList();
			roleId = xDocument.Root.Element("PermissionSet")?.Attribute("RoleID")?.Value;
			roleName = xDocument.Root.Element("PermissionSet")?.Attribute("RoleName")?.Value;
		}
		catch (XmlException)
		{
			existingPermissions = new List<Permission>();
			roleId = null;
			roleName = null;
		}
	}

	private static IEnumerable<Permission> GetAllObjectFromExtensionAsPermissions(Compilation compilation)
	{
		return applicationObjectSymbolKinds.SelectMany((SymbolKind kind) => compilation.GetApplicationObjectTypeSymbols(kind)).SelectMany((IApplicationObjectTypeSymbol symbol) => GetPermissionsForSymbol(symbol));
	}

	private static IEnumerable<Permission> GetPermissionsForSymbol(IApplicationObjectTypeSymbol symbol)
	{
		return from kindNumber in GetSymbolKindNumbers(symbol.Kind)
			select new Permission(kindNumber, symbol.Id.ToString(CultureInfo.InvariantCulture));
	}

	private static IEnumerable<string> GetSymbolKindNumbers(SymbolKind symbolKind)
	{
		return symbolKind switch
		{
			SymbolKind.Table => (IEnumerable<string>)new string[2] { "0", "1" }, 
			SymbolKind.Report => new string[1] { "3" }, 
			SymbolKind.Codeunit => new string[1] { "5" }, 
			SymbolKind.XmlPort => new string[1] { "6" }, 
			SymbolKind.Page => new string[1] { "8" }, 
			SymbolKind.Query => new string[1] { "9" }, 
			_ => new string[0], 
		};
	}
}
