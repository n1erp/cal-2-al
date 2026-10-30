using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal abstract class DiagnosticQueue
{
	private sealed class SimpleDiagnosticQueue : DiagnosticQueue
	{
		private readonly AsyncQueue<Diagnostic> diagnosticQueue;

		public SimpleDiagnosticQueue()
		{
			diagnosticQueue = new AsyncQueue<Diagnostic>();
		}

		public SimpleDiagnosticQueue(Diagnostic diagnostic)
		{
			diagnosticQueue = new AsyncQueue<Diagnostic>();
			diagnosticQueue.Enqueue(diagnostic);
		}

		public override void Enqueue(Diagnostic diagnostic)
		{
			diagnosticQueue.Enqueue(diagnostic);
		}

		public override void EnqueueLocal(Diagnostic diagnostic, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic)
		{
			diagnosticQueue.Enqueue(diagnostic);
		}

		public override void EnqueueNonLocal(Diagnostic diagnostic, DiagnosticAnalyzer analyzer)
		{
			diagnosticQueue.Enqueue(diagnostic);
		}

		public override ImmutableArray<Diagnostic> DequeueLocalSemanticDiagnostics(DiagnosticAnalyzer analyzer)
		{
			throw new NotImplementedException();
		}

		public override ImmutableArray<Diagnostic> DequeueLocalSyntaxDiagnostics(DiagnosticAnalyzer analyzer)
		{
			throw new NotImplementedException();
		}

		public override ImmutableArray<Diagnostic> DequeueNonLocalDiagnostics(DiagnosticAnalyzer analyzer)
		{
			throw new NotImplementedException();
		}

		public override bool TryComplete()
		{
			return diagnosticQueue.TryComplete();
		}

		public override bool TryDequeue(out Diagnostic d)
		{
			return diagnosticQueue.TryDequeue(out d);
		}
	}

	private sealed class CategorizedDiagnosticQueue : DiagnosticQueue
	{
		private readonly object gate = new object();

		private Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> lazyLocalSemanticDiagnostics;

		private Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> lazyLocalSyntaxDiagnostics;

		private Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> lazyNonLocalDiagnostics;

		public CategorizedDiagnosticQueue()
		{
			lazyLocalSemanticDiagnostics = null;
			lazyLocalSyntaxDiagnostics = null;
			lazyNonLocalDiagnostics = null;
		}

		public override void Enqueue(Diagnostic diagnostic)
		{
			throw new InvalidOperationException();
		}

		public override void EnqueueLocal(Diagnostic diagnostic, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic)
		{
			if (isSyntaxDiagnostic)
			{
				EnqueueCore(ref lazyLocalSyntaxDiagnostics, diagnostic, analyzer);
			}
			else
			{
				EnqueueCore(ref lazyLocalSemanticDiagnostics, diagnostic, analyzer);
			}
		}

		public override void EnqueueNonLocal(Diagnostic diagnostic, DiagnosticAnalyzer analyzer)
		{
			EnqueueCore(ref lazyNonLocalDiagnostics, diagnostic, analyzer);
		}

		private void EnqueueCore(ref Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> lazyDiagnosticsMap, Diagnostic diagnostic, DiagnosticAnalyzer analyzer)
		{
			lock (gate)
			{
				lazyDiagnosticsMap = lazyDiagnosticsMap ?? new Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue>();
				EnqueueCore_NoLock(lazyDiagnosticsMap, diagnostic, analyzer);
			}
		}

		private static void EnqueueCore_NoLock(Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> diagnosticsMap, Diagnostic diagnostic, DiagnosticAnalyzer analyzer)
		{
			if (diagnosticsMap.TryGetValue(analyzer, out var value))
			{
				value.Enqueue(diagnostic);
			}
			else
			{
				diagnosticsMap[analyzer] = new SimpleDiagnosticQueue(diagnostic);
			}
		}

		public override bool TryComplete()
		{
			return true;
		}

		public override bool TryDequeue(out Diagnostic d)
		{
			lock (gate)
			{
				return TryDequeue_NoLock(out d);
			}
		}

		private bool TryDequeue_NoLock(out Diagnostic d)
		{
			if (!TryDequeue_NoLock(lazyLocalSemanticDiagnostics, out d) && !TryDequeue_NoLock(lazyLocalSyntaxDiagnostics, out d))
			{
				return TryDequeue_NoLock(lazyNonLocalDiagnostics, out d);
			}
			return true;
		}

		private static bool TryDequeue_NoLock(Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> lazyDiagnosticsMap, out Diagnostic d)
		{
			Diagnostic diag = null;
			if (lazyDiagnosticsMap != null && lazyDiagnosticsMap.Any((KeyValuePair<DiagnosticAnalyzer, SimpleDiagnosticQueue> kvp) => kvp.Value.TryDequeue(out diag)))
			{
				d = diag;
				return true;
			}
			d = null;
			return false;
		}

		public override ImmutableArray<Diagnostic> DequeueLocalSyntaxDiagnostics(DiagnosticAnalyzer analyzer)
		{
			return DequeueDiagnosticsCore(analyzer, lazyLocalSyntaxDiagnostics);
		}

		public override ImmutableArray<Diagnostic> DequeueLocalSemanticDiagnostics(DiagnosticAnalyzer analyzer)
		{
			return DequeueDiagnosticsCore(analyzer, lazyLocalSemanticDiagnostics);
		}

		public override ImmutableArray<Diagnostic> DequeueNonLocalDiagnostics(DiagnosticAnalyzer analyzer)
		{
			return DequeueDiagnosticsCore(analyzer, lazyNonLocalDiagnostics);
		}

		private ImmutableArray<Diagnostic> DequeueDiagnosticsCore(DiagnosticAnalyzer analyzer, Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> diagnosticsMap)
		{
			if (TryGetDiagnosticsQueue(analyzer, diagnosticsMap, out var queue))
			{
				ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
				Diagnostic d;
				while (queue.TryDequeue(out d))
				{
					builder.Add(d);
				}
				return builder.ToImmutable();
			}
			return ImmutableArray<Diagnostic>.Empty;
		}

		private bool TryGetDiagnosticsQueue(DiagnosticAnalyzer analyzer, Dictionary<DiagnosticAnalyzer, SimpleDiagnosticQueue> diagnosticsMap, out SimpleDiagnosticQueue queue)
		{
			queue = null;
			lock (gate)
			{
				return diagnosticsMap?.TryGetValue(analyzer, out queue) ?? false;
			}
		}
	}

	public abstract bool TryComplete();

	public abstract bool TryDequeue(out Diagnostic d);

	public abstract void Enqueue(Diagnostic diagnostic);

	public abstract void EnqueueLocal(Diagnostic diagnostic, DiagnosticAnalyzer analyzer, bool isSyntaxDiagnostic);

	public abstract void EnqueueNonLocal(Diagnostic diagnostic, DiagnosticAnalyzer analyzer);

	public abstract ImmutableArray<Diagnostic> DequeueLocalSyntaxDiagnostics(DiagnosticAnalyzer analyzer);

	public abstract ImmutableArray<Diagnostic> DequeueLocalSemanticDiagnostics(DiagnosticAnalyzer analyzer);

	public abstract ImmutableArray<Diagnostic> DequeueNonLocalDiagnostics(DiagnosticAnalyzer analyzer);

	public static DiagnosticQueue Create(bool categorized = false)
	{
		if (!categorized)
		{
			return new SimpleDiagnosticQueue();
		}
		return new CategorizedDiagnosticQueue();
	}
}
