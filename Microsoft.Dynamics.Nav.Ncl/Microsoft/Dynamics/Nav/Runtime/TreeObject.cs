using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TreeObject : ITreeObject, IDisposable
{
	private TreeHandler tree;

	public TreeHandler Tree => tree;

	public bool IsDisposed => tree.IsDisposed;

	public TreeObject(ITreeObject parent)
	{
		tree = TreeHandler.CreateTreeHandler(parent, this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !IsDisposed)
		{
			Tree.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
