using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class ExtensionsImportSyncStateValidator
{
	private class AppSyncSnapshotRecordComparer : IEqualityComparer<AppSyncSnapshotRecord>
	{
		public bool Equals(AppSyncSnapshotRecord x, AppSyncSnapshotRecord y)
		{
			if (x == null || y == null)
			{
				return false;
			}
			if (x != y)
			{
				if ((WrappedComparable<Guid>)(object)x.AppId == (WrappedComparable<Guid>)(object)y.AppId)
				{
					return x.TargetTableName == y.TargetTableName;
				}
				return false;
			}
			return true;
		}

		public int GetHashCode(AppSyncSnapshotRecord obj)
		{
			return ((object)obj.AppId).GetHashCode() ^ obj.TargetTableId.GetHashCode();
		}
	}

	private class AppSyncTrackingRecordComparer : IEqualityComparer<AppSyncTrackingRecord>
	{
		public bool Equals(AppSyncTrackingRecord x, AppSyncTrackingRecord y)
		{
			if (x == null || y == null)
			{
				return false;
			}
			if (x != y)
			{
				return (WrappedComparable<Guid>)(object)x.AppId == (WrappedComparable<Guid>)(object)y.AppId;
			}
			return true;
		}

		public int GetHashCode(AppSyncTrackingRecord obj)
		{
			return ((object)obj.AppId).GetHashCode();
		}
	}

	public static void EnsureSyncStateIsAcceptableForImport(List<AppSyncTrackingRecord> exportedTrackingRecords, List<AppSyncSnapshotRecord> exportedSnapshotRecords, List<AppSyncTrackingRecord> databaseTrackingRecords, List<AppSyncSnapshotRecord> databaseSnapshotRecords)
	{
		VerifyTrackingRecords(exportedTrackingRecords, databaseTrackingRecords);
		VerifySnapshotRecords(exportedSnapshotRecords, databaseSnapshotRecords);
	}

	public static void EnsureInstallStateIsAcceptableForImport(IEnumerable<AppSyncTrackingRecord> databaseTrackingRecords, IEnumerable<INavAppMetadataReference> installedRecords)
	{
		VerifyInstallRecords(databaseTrackingRecords, installedRecords);
	}

	private static void VerifyTrackingRecords(List<AppSyncTrackingRecord> exportedTrackingRecords, List<AppSyncTrackingRecord> databaseTrackingRecords)
	{
		Dictionary<AppSyncTrackingRecord, AppSyncTrackingRecord> dictionary = databaseTrackingRecords.ToDictionary((AppSyncTrackingRecord _) => _, (AppSyncTrackingRecord _) => _, new AppSyncTrackingRecordComparer());
		foreach (AppSyncTrackingRecord exportedTrackingRecord in exportedTrackingRecords)
		{
			if (!dictionary.TryGetValue(exportedTrackingRecord, out var value))
			{
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedExcessiveExtension, exportedTrackingRecord.Name, exportedTrackingRecord.Publisher, exportedTrackingRecord.Version));
			}
			if (exportedTrackingRecord.Version != value.Version)
			{
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedWrongVersionExtension, value.Name, value.Publisher, value.Version, exportedTrackingRecord.Version));
			}
			dictionary.Remove(exportedTrackingRecord);
		}
		if (dictionary.Count > 0)
		{
			throw new NavNotSupportedException(string.Join(Environment.NewLine, dictionary.Select((KeyValuePair<AppSyncTrackingRecord, AppSyncTrackingRecord> kvp) => string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedMissingExtension, kvp.Key.Name, kvp.Key.Publisher, kvp.Key.Version))));
		}
	}

	private static void VerifySnapshotRecords(List<AppSyncSnapshotRecord> exportedSnapshotRecords, List<AppSyncSnapshotRecord> databaseSnapshotRecords)
	{
		HashSet<AppSyncSnapshotRecord> hashSet = new HashSet<AppSyncSnapshotRecord>(databaseSnapshotRecords, new AppSyncSnapshotRecordComparer());
		foreach (AppSyncSnapshotRecord exportedSnapshotRecord in exportedSnapshotRecords)
		{
			if (!hashSet.Contains(exportedSnapshotRecord))
			{
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedExcessiveTableIncludedInFile, exportedSnapshotRecord.TargetTableName));
			}
			hashSet.Remove(exportedSnapshotRecord);
		}
		if (hashSet.Count > 0)
		{
			throw new NavNotSupportedException(string.Join(Environment.NewLine, hashSet.Select((AppSyncSnapshotRecord record) => string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedMissingExtensionTableInFile, record.TargetTableName))));
		}
	}

	private static void VerifyInstallRecords(IEnumerable<AppSyncTrackingRecord> trackingRecords, IEnumerable<INavAppMetadataReference> installedRecords)
	{
		foreach (INavAppMetadataReference installed in installedRecords.Where((INavAppMetadataReference i) => i.ExtensionType == NavExtensionType.ModernDev))
		{
			AppSyncTrackingRecord appSyncTrackingRecord = trackingRecords.FirstOrDefault((AppSyncTrackingRecord t) => (WrappedComparable<Guid>)(object)t.AppId == (WrappedComparable<Guid>)(object)((INavAppReference)installed).AppId);
			if (appSyncTrackingRecord == null)
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedInstallRecordWithoutSync, ((INavAppReference)installed).Name, ((INavAppReference)installed).Publisher, ((INavAppReference)installed).Version);
				throw new NavNotSupportedException(message);
			}
			if (((INavAppReference)installed).Version < appSyncTrackingRecord.BaselineVersion || ((INavAppReference)installed).Version > appSyncTrackingRecord.Version)
			{
				string message2 = string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedInstallRecordWithInvalidSyncRange, ((INavAppReference)installed).Name, ((INavAppReference)installed).Publisher, ((INavAppReference)installed).Version, appSyncTrackingRecord.BaselineVersion, appSyncTrackingRecord.Version);
				throw new NavNotSupportedException(message2);
			}
		}
	}
}
