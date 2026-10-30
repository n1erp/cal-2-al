using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavApplicationObjectBaseHandle<T> : NavComplexValue, ITreeObjectReference, ITreeObject, INavApplicationObjectBaseHandle where T : NavApplicationObjectBase
{
	private readonly ApplicationObjectId objectId;

	public sealed override int GetBytesSize => 0;

	public bool HasTarget => Tree.GetReferenceTarget() != null;

	public ApplicationObjectId ObjectId => objectId;

	public virtual T Target
	{
		get
		{
			T val = (T)Tree.GetReferenceTarget();
			if (val == null && !Tree.IsDisposed)
			{
				val = CreateTarget();
				Tree.SetReferenceTarget(val);
			}
			return val;
		}
		set
		{
			Tree.SetReferenceTarget(value);
		}
	}

	bool INavApplicationObjectBaseHandle.HasTarget => HasTarget;

	NavApplicationObjectBase INavApplicationObjectBaseHandle.Target => Target;

	protected NavApplicationObjectBaseHandle(ITreeObject parent, ApplicationObjectId id)
		: base(parent)
	{
		objectId = id;
	}

	protected NavApplicationObjectBaseHandle(ITreeObject parent, T initialTarget)
		: base(parent)
	{
		if (initialTarget == null)
		{
			throw new ArgumentNullException("initialTarget");
		}
		objectId = initialTarget.ObjectId;
		Target = initialTarget;
	}

	public override void Clear()
	{
		if (HasTarget)
		{
			Target.Clear();
		}
	}

	public sealed override int GetHashCode()
	{
		return 0;
	}

	protected abstract T CreateTarget();
}
