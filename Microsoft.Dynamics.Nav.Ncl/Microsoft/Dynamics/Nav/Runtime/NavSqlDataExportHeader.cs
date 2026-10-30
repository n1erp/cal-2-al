using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlDataExportHeader
{
	public const int PreviousSupportedVersion = 3;

	public const int CurrentVersion = 4;

	private readonly int exportVersion;

	private readonly int navDatabaseVersion;

	private readonly string description;

	private readonly bool hasApplication;

	private readonly bool hasAppDatabaseTables;

	private readonly int tenantVersion;

	private readonly string tenantId;

	private readonly bool hasGlobalTables;

	private readonly string[] companyNames;

	private readonly DateTime exportDateTime;

	private readonly string applicationVersion;

	public int ExportVersion => exportVersion;

	public int NavDatabaseVersion => navDatabaseVersion;

	public string Description => description;

	public DateTime ExportDateTime => exportDateTime;

	public bool HasApplication => hasApplication;

	public bool HasAppDatabaseTables => hasAppDatabaseTables;

	public int TenantVersion => tenantVersion;

	public string TenantId => tenantId;

	public bool HasGlobalTables => hasGlobalTables;

	public IEnumerable<string> CompanyNames => companyNames;

	public string ApplicationVersion => applicationVersion;

	public NavSqlDataExportHeader(BinaryReader reader)
	{
		try
		{
			exportVersion = reader.ReadInt32();
			if (exportVersion != 3 && exportVersion != 4)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.ExportVersionNotSupported, ProductNames.ProductNameShort));
			}
			navDatabaseVersion = reader.ReadInt32();
			description = reader.ReadString();
			exportDateTime = DateTime.FromBinary(reader.ReadInt64());
			hasApplication = reader.ReadBoolean();
			hasAppDatabaseTables = reader.ReadBoolean();
			tenantVersion = reader.ReadInt32();
			tenantId = reader.ReadString();
			hasGlobalTables = reader.ReadBoolean();
			int num = reader.ReadInt32();
			if (num > 0)
			{
				string[] array = new string[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = reader.ReadString();
				}
				companyNames = array.ToArray();
			}
			try
			{
				applicationVersion = reader.ReadString();
			}
			catch (EndOfStreamException)
			{
			}
		}
		catch (EndOfStreamException)
		{
			throw new NavNotSupportedException(Lang.UnexpectedReadErrorDuringImport);
		}
		catch (IOException ex3)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.IOErrorDuringImport, ex3.Message), ex3);
		}
	}

	public NavSqlDataExportHeader(int navDatabaseVersion, string description, bool hasApplication, bool hasAppDatabaseTables, int tenantVersion, string tenantId, bool hasGlobalTables, IEnumerable<string> companyNames, string applicationVersion)
	{
		this.navDatabaseVersion = navDatabaseVersion;
		exportVersion = 4;
		this.description = description;
		this.tenantVersion = tenantVersion;
		this.tenantId = tenantId;
		this.hasApplication = hasApplication;
		this.hasAppDatabaseTables = hasAppDatabaseTables;
		this.hasGlobalTables = hasGlobalTables;
		this.companyNames = companyNames?.ToArray();
		this.applicationVersion = applicationVersion;
	}

	public NavSqlDataExportHeader(int navDatabaseVersion, string description, bool hasApplication, bool hasAppDatabaseTables, int tenantVersion, string tenantId, bool hasGlobalTables, IEnumerable<string> companyNames, string applicationVersion, int exportVersion)
	{
		this.navDatabaseVersion = navDatabaseVersion;
		this.exportVersion = exportVersion;
		this.description = description;
		this.tenantVersion = tenantVersion;
		this.tenantId = tenantId;
		this.hasApplication = hasApplication;
		this.hasAppDatabaseTables = hasAppDatabaseTables;
		this.hasGlobalTables = hasGlobalTables;
		this.companyNames = companyNames?.ToArray();
		this.applicationVersion = applicationVersion;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(exportVersion);
		writer.Write(navDatabaseVersion);
		writer.Write(description ?? string.Empty);
		writer.Write(DateTime.UtcNow.ToBinary());
		writer.Write(hasApplication);
		writer.Write(hasAppDatabaseTables);
		writer.Write(tenantVersion);
		writer.Write(tenantId);
		writer.Write(hasGlobalTables);
		if (companyNames == null)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(companyNames.Count());
			string[] array = companyNames;
			foreach (string value in array)
			{
				writer.Write(value);
			}
		}
		writer.Write(applicationVersion);
	}
}
