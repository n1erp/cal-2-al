using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TreeObjectReference : TreeObject, ITreeObjectReference, ITreeObject
{
	public ITreeObject Target
	{
		get
		{
			ITreeObject treeObject = Tree.GetReferenceTarget();
			if (treeObject == null && !Tree.IsDisposed)
			{
				treeObject = CreateTarget();
				Tree.SetReferenceTarget(treeObject);
			}
			return treeObject;
		}
		set
		{
			Tree.SetReferenceTarget(value);
		}
	}

	public TreeObjectReference(ITreeObject parent)
		: base(parent)
	{
	}

	public TreeObjectReference(ITreeObject parent, ITreeObject initialTarget)
		: base(parent)
	{
		if (initialTarget == null)
		{
			throw new ArgumentNullException("initialTarget");
		}
		Target = initialTarget;
	}

	public virtual ITreeSharedObject CreateTarget()
	{
		return null;
	}
}
