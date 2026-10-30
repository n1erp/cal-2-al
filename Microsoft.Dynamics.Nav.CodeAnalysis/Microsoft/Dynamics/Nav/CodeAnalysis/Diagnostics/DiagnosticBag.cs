using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
[DebuggerTypeProxy(typeof(DebuggerProxy))]
internal class DiagnosticBag
{
	private class NullDiagnosticBag : DiagnosticBag
	{
		public static NullDiagnosticBag Instance { get; } = new NullDiagnosticBag();

		public override bool IsEmptyWithoutResolution => false;

		private NullDiagnosticBag()
		{
		}

		public override void Add(Diagnostic diag)
		{
		}

		public override void AddRange(DiagnosticBag bag)
		{
		}

		public override void AddRange(IEnumerable<Diagnostic> diagnostics)
		{
		}

		public override void AddRange<T>(ImmutableArray<T> diagnostics)
		{
		}

		public override void AddRangeAndFree(DiagnosticBag bag)
		{
		}

		public override IEnumerable<Diagnostic> AsEnumerable()
		{
			return SpecializedCollections.EmptyEnumerable<Diagnostic>();
		}

		internal override IEnumerable<Diagnostic> AsEnumerableWithoutResolution()
		{
			return SpecializedCollections.EmptyEnumerable<Diagnostic>();
		}

		internal override void Clear()
		{
		}

		public override bool HasAnyErrors()
		{
			return false;
		}

		internal override bool HasAnyResolvedErrors()
		{
			return false;
		}

		public override ImmutableArray<Diagnostic> ToReadOnly()
		{
			return ImmutableArray<Diagnostic>.Empty;
		}

		public override ImmutableArray<TDiagnostic> ToReadOnly<TDiagnostic>()
		{
			return ImmutableArray<TDiagnostic>.Empty;
		}

		public override ImmutableArray<Diagnostic> ToReadOnlyAndFree()
		{
			return ImmutableArray<Diagnostic>.Empty;
		}

		public override ImmutableArray<TDiagnostic> ToReadOnlyAndFree<TDiagnostic>()
		{
			return ImmutableArray<TDiagnostic>.Empty;
		}

		internal override void Free()
		{
		}

		public override string ToString()
		{
			return string.Empty;
		}
	}

	internal sealed class DebuggerProxy
	{
		private readonly DiagnosticBag diagnosticBag;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Diagnostics
		{
			get
			{
				ConcurrentQueue<Diagnostic> lazyConcurrentQueue = diagnosticBag.lazyConcurrentQueue;
				if (lazyConcurrentQueue != null)
				{
					return lazyConcurrentQueue.ToArray();
				}
				return SpecializedCollections.EmptyObjects;
			}
		}

		public DebuggerProxy(DiagnosticBag bag)
		{
			diagnosticBag = bag;
		}
	}

	private ConcurrentQueue<Diagnostic> lazyConcurrentQueue;

	private static readonly ObjectPool<DiagnosticBag> PoolInstance = CreatePool(128);

	public virtual bool IsEmptyWithoutResolution => lazyConcurrentQueue?.IsEmpty ?? true;

	private ConcurrentQueue<Diagnostic> Bag
	{
		get
		{
			ConcurrentQueue<Diagnostic> concurrentQueue = lazyConcurrentQueue;
			if (concurrentQueue != null)
			{
				return concurrentQueue;
			}
			ConcurrentQueue<Diagnostic> concurrentQueue2 = new ConcurrentQueue<Diagnostic>();
			return Interlocked.CompareExchange(ref lazyConcurrentQueue, concurrentQueue2, null) ?? concurrentQueue2;
		}
	}

	public virtual bool HasAnyErrors()
	{
		if (IsEmptyWithoutResolution)
		{
			return false;
		}
		foreach (Diagnostic item in Bag)
		{
			if (item.Severity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}

	internal virtual bool HasAnyResolvedErrors()
	{
		if (IsEmptyWithoutResolution)
		{
			return false;
		}
		foreach (Diagnostic item in Bag)
		{
			if (item is DiagnosticWithInfo { HasLazyInfo: false } && item.Severity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void Add(Diagnostic diag)
	{
		Bag.Enqueue(diag);
	}

	public virtual void AddRange<T>(ImmutableArray<T> diagnostics) where T : Diagnostic
	{
		if (!diagnostics.IsDefaultOrEmpty)
		{
			ConcurrentQueue<Diagnostic> bag = Bag;
			for (int i = 0; i < diagnostics.Length; i++)
			{
				bag.Enqueue(diagnostics[i]);
			}
		}
	}

	public virtual void AddRange(IEnumerable<Diagnostic> diagnostics)
	{
		foreach (Diagnostic diagnostic in diagnostics)
		{
			Bag.Enqueue(diagnostic);
		}
	}

	public virtual void AddRange(DiagnosticBag bag)
	{
		if (!bag.IsEmptyWithoutResolution)
		{
			AddRange(bag.Bag);
		}
	}

	public virtual void AddRangeAndFree(DiagnosticBag bag)
	{
		AddRange(bag);
		bag.Free();
	}

	public virtual ImmutableArray<TDiagnostic> ToReadOnlyAndFree<TDiagnostic>() where TDiagnostic : Diagnostic
	{
		ConcurrentQueue<Diagnostic> oldBag = lazyConcurrentQueue;
		Free();
		return ToReadOnlyCore<TDiagnostic>(oldBag);
	}

	public virtual ImmutableArray<Diagnostic> ToReadOnlyAndFree()
	{
		return ToReadOnlyAndFree<Diagnostic>();
	}

	public virtual ImmutableArray<TDiagnostic> ToReadOnly<TDiagnostic>() where TDiagnostic : Diagnostic
	{
		return ToReadOnlyCore<TDiagnostic>(lazyConcurrentQueue);
	}

	public virtual ImmutableArray<Diagnostic> ToReadOnly()
	{
		return ToReadOnly<Diagnostic>();
	}

	private static ImmutableArray<TDiagnostic> ToReadOnlyCore<TDiagnostic>(ConcurrentQueue<Diagnostic> oldBag) where TDiagnostic : Diagnostic
	{
		if (oldBag == null)
		{
			return ImmutableArray<TDiagnostic>.Empty;
		}
		ArrayBuilder<TDiagnostic> instance = ArrayBuilder<TDiagnostic>.GetInstance();
		foreach (TDiagnostic item in oldBag)
		{
			if (item.Severity != (DiagnosticSeverity)(-2))
			{
				DebugAssertHelper.Assert(item.Severity != (DiagnosticSeverity)(-1));
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public virtual IEnumerable<Diagnostic> AsEnumerable()
	{
		ConcurrentQueue<Diagnostic> bag = Bag;
		bool flag = false;
		foreach (Diagnostic item in bag)
		{
			if (item.Severity == (DiagnosticSeverity)(-2))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return bag;
		}
		return AsEnumerableFiltered();
	}

	private IEnumerable<Diagnostic> AsEnumerableFiltered()
	{
		foreach (Diagnostic item in Bag)
		{
			if (item.Severity != (DiagnosticSeverity)(-2))
			{
				DebugAssertHelper.Assert(item.Severity != (DiagnosticSeverity)(-1));
				yield return item;
			}
		}
	}

	internal virtual IEnumerable<Diagnostic> AsEnumerableWithoutResolution()
	{
		IEnumerable<Diagnostic> enumerable = lazyConcurrentQueue;
		return enumerable ?? SpecializedCollections.EmptyEnumerable<Diagnostic>();
	}

	public override string ToString()
	{
		if (IsEmptyWithoutResolution)
		{
			return "<no errors>";
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Diagnostic item in Bag)
		{
			stringBuilder.AppendLine(item.ToString());
		}
		return stringBuilder.ToString();
	}

	internal virtual void Clear()
	{
		if (lazyConcurrentQueue != null)
		{
			lazyConcurrentQueue = null;
		}
	}

	internal static DiagnosticBag GetInstance()
	{
		return PoolInstance.Allocate();
	}

	internal static DiagnosticBag GetNullInstance()
	{
		return NullDiagnosticBag.Instance;
	}

	internal virtual void Free()
	{
		Clear();
		PoolInstance.Free(this);
	}

	private static ObjectPool<DiagnosticBag> CreatePool(int size)
	{
		return new ObjectPool<DiagnosticBag>(() => new DiagnosticBag(), size);
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return "Count = " + ((lazyConcurrentQueue != null) ? lazyConcurrentQueue.Count : 0);
	}
}
