using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class ContentHashFixer
{
	public static void EnsureHashesAreCalculatedForPublishedArtifacts(NavDatabase appDatabase)
	{
		try
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(appDatabase, updateOrlockingActivity: true);
			int num = FixNavAppHashes(conScope);
			int num2 = FixNavAppObjectMetadataHashes(conScope);
			num2 += FixNavAppObjectUserCodeHashes(conScope);
			int num3 = FixNavAppResourceHashes(conScope);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007MR", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Hashes Fixed -- NavApp:{0}; NavAppObjectMetadata:{1}; NavAppResource:{2}; Elapsed:{3}", num, num2, num3, stopwatch.Elapsed.ToStringWithMilliseconds());
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007MS", Category.Extensions, exception);
		}
	}

	private static int FixNavAppHashes(NavSqlConnectionScope conScope)
	{
		int num = 0;
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		string statement = "SELECT [Package ID], [Blob] FROM [NAV App] " + FormattableString.Invariant($"WHERE ([Package Hash] = '' OR LEN([Package Hash]) <> {64}) AND [Blob] IS NOT NULL");
		using SqlDataReader sqlDataReader = navSqlCommand.ExecuteReader(statement);
		while (sqlDataReader.Read())
		{
			Guid guid = sqlDataReader.GetGuid(0);
			byte[] array = ReadCompressedBlob(navSqlCommand, 1);
			if (array != null)
			{
				using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
				Sha256Hash sha256Hash = Sha256Hash.ComputeHashForContent(array);
				string statement2 = FormattableString.Invariant($"UPDATE [NAV App] SET [Package Hash] = '{sha256Hash.HexString}' ") + FormattableString.Invariant($"WHERE [Package ID] = '{guid}'");
				navSqlCommand2.ExecuteNonQuery(statement2);
				num++;
			}
		}
		return num;
	}

	private static int FixNavAppObjectMetadataHashes(NavSqlConnectionScope conScope)
	{
		int num = 0;
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		string statement = "SELECT [App Package ID], [Object Type], [Object ID], [Metadata] FROM [NAV App Object Metadata] " + FormattableString.Invariant($"WHERE ([Metadata Hash] = '' OR LEN([Metadata Hash]) <> {64}) AND [Metadata] IS NOT NULL");
		using SqlDataReader sqlDataReader = navSqlCommand.ExecuteReader(statement);
		while (sqlDataReader.Read())
		{
			Guid guid = sqlDataReader.GetGuid(0);
			int @int = sqlDataReader.GetInt32(1);
			int int2 = sqlDataReader.GetInt32(2);
			byte[] array = ReadCompressedBlob(navSqlCommand, 3);
			if (array != null)
			{
				using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
				Sha256Hash sha256Hash = Sha256Hash.ComputeHashForContent(array);
				string statement2 = FormattableString.Invariant($"UPDATE [NAV App Object Metadata] SET [Metadata Hash] = '{sha256Hash.HexString}' ") + FormattableString.Invariant($"WHERE [App Package ID] = '{guid}' AND [Object Type] = {@int} AND [Object ID] = {int2}");
				navSqlCommand2.ExecuteNonQuery(statement2);
				num++;
			}
		}
		return num;
	}

	private static int FixNavAppObjectUserCodeHashes(NavSqlConnectionScope conScope)
	{
		int num = 0;
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		string statement = "SELECT [App Package ID], [Object Type], [Object ID], [User Code] FROM [NAV App Object Metadata] " + FormattableString.Invariant($"WHERE ([User Code Hash] = '' OR LEN([User Code Hash]) <> {64}) AND [User Code] IS NOT NULL");
		using SqlDataReader sqlDataReader = navSqlCommand.ExecuteReader(statement);
		while (sqlDataReader.Read())
		{
			Guid guid = sqlDataReader.GetGuid(0);
			int @int = sqlDataReader.GetInt32(1);
			int int2 = sqlDataReader.GetInt32(2);
			byte[] array = ReadCompressedBlob(navSqlCommand, 3);
			if (array != null)
			{
				using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
				Sha256Hash sha256Hash = Sha256Hash.ComputeHashForContent(array);
				string statement2 = FormattableString.Invariant($"UPDATE [NAV App Object Metadata] SET [User Code Hash] = '{sha256Hash.HexString}' ") + FormattableString.Invariant($"WHERE [App Package ID] = '{guid}' AND [Object Type] = {@int} AND [Object ID] = {int2}");
				navSqlCommand2.ExecuteNonQuery(statement2);
				num++;
			}
		}
		return num;
	}

	private static int FixNavAppResourceHashes(NavSqlConnectionScope conScope)
	{
		int num = 0;
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		string statement = "SELECT [Package ID], [Type], [Name], [Content] FROM [NAV App Resource] " + FormattableString.Invariant($"WHERE ([Hash] = '' OR LEN([Hash]) <> {64}) AND [Content] IS NOT NULL");
		using SqlDataReader sqlDataReader = navSqlCommand.ExecuteReader(statement);
		while (sqlDataReader.Read())
		{
			Guid guid = sqlDataReader.GetGuid(0);
			int @int = sqlDataReader.GetInt32(1);
			string text = sqlDataReader.GetString(2);
			byte[] array = ReadCompressedBlob(navSqlCommand, 3);
			if (array != null)
			{
				using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
				Sha256Hash sha256Hash = Sha256Hash.ComputeHashForContent(array);
				string statement2 = FormattableString.Invariant($"UPDATE [NAV App Resource] SET [Hash] = '{sha256Hash.HexString}' ") + FormattableString.Invariant($"WHERE [Package ID] = '{guid}' AND [Type] = {@int} AND [Name] = '{text}'");
				navSqlCommand2.ExecuteNonQuery(statement2);
				num++;
			}
		}
		return num;
	}

	private static byte[] ReadCompressedBlob(NavSqlCommand command, int ordinal)
	{
		using NavBLOB navBLOB = new NavBLOB();
		command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
		if (!navBLOB.IsZeroOrEmpty)
		{
			return navBLOB.GetBytes();
		}
		return null;
	}
}
