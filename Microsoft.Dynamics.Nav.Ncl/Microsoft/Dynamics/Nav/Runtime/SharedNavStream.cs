namespace Microsoft.Dynamics.Nav.Runtime;

internal class SharedNavStream : TreeObject, ITreeSharedObject, ITreeObject
{
	private INavStreamProvider navStreamProvider;

	private long position;

	private TextEncoding textEncoding;

	internal INavStreamProvider SharedStream
	{
		get
		{
			return navStreamProvider;
		}
		set
		{
			if (navStreamProvider != value)
			{
				ReleaseSharedStream();
				navStreamProvider = value;
			}
		}
	}

	public long Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	internal TextEncoding TextEncoding
	{
		get
		{
			return textEncoding;
		}
		set
		{
			textEncoding = value;
		}
	}

	internal SharedNavStream()
		: base(NavCurrentThread.Session.Company.SharedObjects)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			ReleaseSharedStream();
		}
		base.Dispose(disposing);
	}

	private void ReleaseSharedStream()
	{
		if (navStreamProvider != null && navStreamProvider is ITreeObject treeObject && !treeObject.Tree.IsDisposed && treeObject.Tree.Parent == this)
		{
			treeObject.Tree.InternalHostDispose();
			position = 0L;
		}
	}
}
