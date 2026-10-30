using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavComplexValue : NavValue, ITreeObject, IDisposable
{
	private TreeHandler tree;

	public TreeHandler Tree => tree;

	public override bool IsMutable => true;

	public override bool IsZeroOrEmpty => false;

	public bool IsDisposed => Tree.IsDisposed;

	protected NavComplexValue(ITreeObject parent)
	{
		tree = TreeHandler.CreateTreeHandler(parent, this);
	}

	public abstract void Clear();

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			Tree.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
