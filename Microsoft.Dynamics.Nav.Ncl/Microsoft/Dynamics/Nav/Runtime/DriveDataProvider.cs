using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DriveDataProvider : EagerVirtualDataProvider
{
	public static readonly DriveDataProvider Instance = new DriveDataProvider();

	public override int TableId => 2000000020;

	private DriveDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		if (!NavEnvironment.Topology.IsServiceRunningInLocalEnvironment && !FeatureSwitches.Instance.DisableFilePathLockdown)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007IU", Category.Runtime, new NavNCLFileLoadException(Lang.ServerFileAccessNotAllowed));
		}
		IEnumerable<DriveInfo> drivesSortedByTheName = GetDrivesSortedByTheName();
		return GetAllDrives(drivesSortedByTheName);
	}

	private static IEnumerable<DriveInfo> GetDrivesSortedByTheName()
	{
		List<DriveInfo> list = new List<DriveInfo>();
		for (char c = 'A'; c <= 'Z'; c = (char)(c + 1))
		{
			try
			{
				DriveInfo driveInfo = new DriveInfo(new string(c, 1));
				if (driveInfo.DriveType != DriveType.Unknown && driveInfo.DriveType != DriveType.NoRootDirectory)
				{
					list.Add(new DriveInfo(new string(c, 1)));
				}
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (IOException)
			{
			}
		}
		return list;
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetAllDrives(IEnumerable<DriveInfo> allDrives)
	{
		ReadOnlyRecordBuffer[] array = new ReadOnlyRecordBuffer[allDrives.Count()];
		int num = 0;
		foreach (DriveInfo allDrife in allDrives)
		{
			long num2 = -1L;
			long num3 = -1L;
			try
			{
				if (allDrife.IsReady)
				{
					num2 = allDrife.TotalSize / 1024;
					num3 = allDrife.AvailableFreeSpace / 1024;
				}
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (IOException)
			{
			}
			array[num++] = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, GetTruncatedCodeValue(1, allDrife.Name.TrimEnd('\\')), NavBoolean.Create(allDrife.DriveType == DriveType.Removable || allDrife.DriveType == DriveType.CDRom), NavInteger.Create((int)((num2 > (long)NavInteger.MaxValue) ? (-1) : num2)), NavInteger.Create((int)((num3 > (long)NavInteger.MaxValue) ? (-1) : num3)));
		}
		return array;
	}
}
