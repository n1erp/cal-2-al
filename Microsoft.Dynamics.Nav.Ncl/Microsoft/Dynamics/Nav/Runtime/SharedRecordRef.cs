namespace Microsoft.Dynamics.Nav.Runtime;

internal class SharedRecordRef : TreeObject, ITreeSharedObject, ITreeObject
{
	private NavRecord record;

	internal NavRecord Record
	{
		get
		{
			return record;
		}
		set
		{
			if (record != value)
			{
				if (record != null && !record.Tree.IsDisposed && record.Tree.Parent == this)
				{
					record.Dispose();
				}
				record = value;
			}
		}
	}

	internal SharedRecordRef()
		: base(NavCurrentThread.Session.Company.SharedObjects)
	{
	}
}
