using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class RecordState : IDisposable
{
	public abstract void BackupTableAndRecordHandle();

	public abstract void BackupRecordHandle();

	public abstract void RevertBackupHandles();

	public abstract void RestoreRecord();

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected abstract void Dispose(bool disposing);
}
