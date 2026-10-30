using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TreeHandler
{
	private class TreeSharedObjectHandler : TreeHandler
	{
		private int refCount;

		public override int ReferenceCount => refCount;

		internal TreeSharedObjectHandler(ITreeObject parent, ITreeObject hostObject)
			: base(parent, hostObject)
		{
		}

		protected override void InternalClearAll()
		{
			refCount = 0;
			base.InternalClearAll();
		}

		protected override void InternalAddReference(ITreeObjectReference reference)
		{
			if (!IsDisposed)
			{
				Interlocked.Increment(ref refCount);
			}
		}

		protected override void InternalRemoveReferenceDisposeIfLast(ITreeObjectReference reference)
		{
			if (!IsDisposed && Interlocked.Decrement(ref refCount) == 0)
			{
				InternalHostDispose();
			}
		}
	}

	private class TreeObjectReferenceHandler : TreeHandler
	{
		private ITreeObject referencedObject;

		internal TreeObjectReferenceHandler(ITreeObject parent, ITreeObject hostObject)
			: base(parent, hostObject)
		{
		}

		public override ITreeObject GetReferenceTarget()
		{
			return referencedObject;
		}

		public override void SetReferenceTarget(ITreeObject newReference)
		{
			if (newReference != referencedObject)
			{
				if (referencedObject != null)
				{
					referencedObject.Tree.InternalRemoveReferenceDisposeIfLast((ITreeObjectReference)hostObject);
				}
				referencedObject = newReference;
				newReference?.Tree.InternalAddReference((ITreeObjectReference)hostObject);
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if ((!IsDisposed & disposing) && referencedObject != null)
				{
					ITreeObject treeObject = referencedObject;
					referencedObject = null;
					treeObject.Tree.InternalRemoveReferenceDisposeIfLast((ITreeObjectReference)hostObject);
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}

	private static readonly TreeHandler defaultHandler = new TreeHandler();

	private ITreeObject hostObject;

	private TreeHandler parentHandler;

	private TreeHandler fistChildHandler;

	private TreeHandler previousSiblingHandler;

	private TreeHandler nextSiblingHandler;

	public static TreeHandler DefaultHandler => defaultHandler;

	public bool IsDisposed => hostObject == null;

	public ITreeObject Parent
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Tree");
			}
			if (parentHandler != null)
			{
				return parentHandler.hostObject;
			}
			return null;
		}
	}

	public int ChildCount
	{
		get
		{
			lock (this)
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException("Tree");
				}
				int num = 0;
				for (TreeHandler treeHandler = fistChildHandler; treeHandler != null; treeHandler = treeHandler.nextSiblingHandler)
				{
					num++;
				}
				return num;
			}
		}
	}

	public IEnumerable<ITreeObject> Children
	{
		get
		{
			lock (this)
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException("Tree");
				}
				TreeHandler treeHandler = fistChildHandler;
				while (treeHandler != null)
				{
					TreeHandler next = treeHandler.nextSiblingHandler;
					yield return treeHandler.hostObject;
					treeHandler = next;
				}
			}
		}
	}

	public virtual int ReferenceCount => -1;

	public NavSession Session
	{
		get
		{
			for (ITreeObject parent = Parent; parent != null; parent = parent.Tree.Parent)
			{
				if (parent is ISessionProvider sessionProvider)
				{
					return sessionProvider.Session;
				}
			}
			return null;
		}
	}

	private TreeHandler()
	{
	}

	protected TreeHandler(ITreeObject parent, ITreeObject hostObject)
	{
		if (hostObject == null)
		{
			throw new ArgumentNullException("hostObject");
		}
		this.hostObject = hostObject;
		if (this.hostObject.Tree != null)
		{
			throw new InvalidOperationException();
		}
		if (parent != null)
		{
			if (parent.Tree == null)
			{
				throw new ArgumentException("parent.Tree");
			}
			parentHandler = parent.Tree;
			parentHandler.InternalAddChild(this);
		}
	}

	public static TreeHandler CreateTreeRoot(ITreeObject hostRootObject)
	{
		if (hostRootObject == null)
		{
			throw new ArgumentNullException("hostRootObject");
		}
		return new TreeHandler(null, hostRootObject);
	}

	public static TreeHandler CreateTreeHandler(ITreeObject parent, ITreeObject hostObject)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (hostObject == null)
		{
			throw new ArgumentNullException("hostObject");
		}
		if (hostObject is ITreeSharedObject)
		{
			return new TreeSharedObjectHandler(parent, hostObject);
		}
		if (hostObject is ITreeObjectReference)
		{
			return new TreeObjectReferenceHandler(parent, hostObject);
		}
		return new TreeHandler(parent, hostObject);
	}

	public IEnumerable<T> ChildrenOfTypeWhere<T>(Func<T, bool> predicate) where T : class, ITreeObject
	{
		lock (this)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Tree");
			}
			TreeHandler treeHandler = fistChildHandler;
			while (treeHandler != null)
			{
				TreeHandler next = treeHandler.nextSiblingHandler;
				if (treeHandler.hostObject is T val && predicate(val))
				{
					yield return val;
				}
				treeHandler = next;
			}
		}
	}

	public T ChildOfTypeFirstOrDefault<T>(Func<T, bool> predicate) where T : class, ITreeObject
	{
		lock (this)
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("Tree");
			}
			for (TreeHandler treeHandler = fistChildHandler; treeHandler != null; treeHandler = treeHandler.nextSiblingHandler)
			{
				if (treeHandler.hostObject is T val && predicate(val))
				{
					return val;
				}
			}
			return null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void DisposeAllChildren()
	{
		TreeHandler treeHandler = Interlocked.Exchange(ref fistChildHandler, null);
		while (treeHandler != null)
		{
			TreeHandler treeHandler2 = treeHandler.nextSiblingHandler;
			treeHandler.InternalHostDispose();
			treeHandler = treeHandler2;
		}
	}

	public void DisposeAllChildrenExcept(ITreeObject objectNotToDispose)
	{
		lock (this)
		{
			if (fistChildHandler == null)
			{
				return;
			}
			if (objectNotToDispose == null || objectNotToDispose.Tree.Parent != hostObject)
			{
				DisposeAllChildren();
				return;
			}
			InternalRemoveChild(objectNotToDispose.Tree);
			try
			{
				DisposeAllChildren();
			}
			finally
			{
				InternalAddChild(objectNotToDispose.Tree);
			}
		}
	}

	public T FindParentType<T>() where T : class, ITreeObject
	{
		for (ITreeObject parent = Parent; parent != null; parent = parent.Tree.Parent)
		{
			if (parent is T result)
			{
				return result;
			}
		}
		return null;
	}

	public virtual ITreeObject GetReferenceTarget()
	{
		throw new InvalidOperationException();
	}

	public virtual void SetReferenceTarget(ITreeObject newTarget)
	{
		throw new InvalidOperationException();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing || IsDisposed)
		{
			return;
		}
		try
		{
			DisposeAllChildren();
		}
		finally
		{
			try
			{
				if (parentHandler != null)
				{
					parentHandler.InternalRemoveChild(this);
				}
			}
			finally
			{
				InternalClearAll();
			}
		}
	}

	protected virtual void InternalClearAll()
	{
		parentHandler = null;
		fistChildHandler = null;
		nextSiblingHandler = null;
		previousSiblingHandler = null;
		hostObject = null;
	}

	protected virtual void InternalAddReference(ITreeObjectReference reference)
	{
	}

	protected virtual void InternalRemoveReferenceDisposeIfLast(ITreeObjectReference reference)
	{
		if (!IsDisposed)
		{
			ITreeObject parent = Parent;
			if (parent != null && parent == reference)
			{
				InternalHostDispose();
			}
		}
	}

	private void InternalAddChild(TreeHandler childToAdd)
	{
		lock (this)
		{
			if (fistChildHandler == null)
			{
				fistChildHandler = childToAdd;
				return;
			}
			fistChildHandler.previousSiblingHandler = childToAdd;
			childToAdd.nextSiblingHandler = fistChildHandler;
			fistChildHandler = childToAdd;
		}
	}

	private void InternalRemoveChild(TreeHandler childToRemove)
	{
		lock (this)
		{
			if (fistChildHandler != null && childToRemove.parentHandler == this)
			{
				TreeHandler treeHandler = childToRemove.previousSiblingHandler;
				TreeHandler treeHandler2 = childToRemove.nextSiblingHandler;
				childToRemove.previousSiblingHandler = null;
				childToRemove.nextSiblingHandler = null;
				if (treeHandler2 != null)
				{
					treeHandler2.previousSiblingHandler = treeHandler;
				}
				if (treeHandler != null)
				{
					treeHandler.nextSiblingHandler = treeHandler2;
				}
				else
				{
					fistChildHandler = treeHandler2;
				}
			}
		}
	}

	internal void InternalHostDispose()
	{
		if (!IsDisposed)
		{
			if (hostObject is IDisposable disposable)
			{
				disposable.Dispose();
			}
			else
			{
				Dispose();
			}
		}
	}

	[Conditional("DEBUG")]
	private void Dump(ref string result, string comment, bool entireTree)
	{
		ITreeObject parent = hostObject;
		if (entireTree)
		{
			while (parent.Tree.Parent != null)
			{
				parent = parent.Tree.Parent;
			}
		}
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		stringWriter.WriteLine("ITreeObject dump - Time: " + DateTime.Now.ToString());
		if (!string.IsNullOrEmpty(comment))
		{
			stringWriter.WriteLine("Comment: " + comment);
		}
		stringWriter.WriteLine();
		stringWriter.WriteLine("Dump complete");
		result = stringWriter.ToString();
	}

	[Conditional("DEBUG")]
	private static void InternalDump(StringWriter sw, int indent, ITreeObject obj)
	{
		if (indent >= 5)
		{
			return;
		}
		sw.Write(new string(' ', indent));
		sw.Write("[" + obj.GetType()?.ToString() + "]");
		if (obj is ITreeSharedObject)
		{
			sw.Write(" refCount=" + obj.Tree.ReferenceCount + " ");
		}
		string text = obj.ToString();
		if (text.Length > 50)
		{
			text = text.Substring(0, 47) + "...";
		}
		sw.WriteLine(" " + text);
		foreach (ITreeObject child in obj.Tree.Children)
		{
			_ = child;
		}
	}
}
