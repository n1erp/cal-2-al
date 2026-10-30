using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal abstract class SourceChangeMoveSymbol : ChangeMoveSymbol
{
	private SymbolCompletionState state;

	private Symbol lazyAnchor;

	private ImmutableArray<Symbol> lazyMovedSymbols;

	internal SyntaxNode Syntax { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(Syntax.GetReference());

	internal override Symbol Anchor
	{
		get
		{
			LazyGetAnchor();
			return lazyAnchor;
		}
	}

	internal override ImmutableArray<Symbol> MovedSymbols
	{
		get
		{
			LazyGetMovedSymbols();
			return lazyMovedSymbols;
		}
	}

	internal override bool RequiresCompletion => true;

	protected SourceChangeMoveSymbol(Symbol owner, SyntaxNode syntax)
	{
		ContainingSymbol = owner;
		Syntax = syntax;
		Locations = ImmutableArray.Create((Location)new SourceLocation(syntax));
	}

	private void LazyGetAnchor()
	{
		if (state.HasComplete(CompletionPart.EnumValues))
		{
			return;
		}
		lock (Syntax.Parent)
		{
			LazySetSymbol((DiagnosticBag d) =>
			{
				lazyAnchor = LazyGetAnchor(d);
			}, CompletionPart.EnumValues);
		}
	}

	private void LazyGetMovedSymbols()
	{
		if (lazyMovedSymbols != null)
		{
			return;
		}
		lock (Syntax.Parent)
		{
			LazySetSymbol((DiagnosticBag d) =>
			{
				ImmutableArray<Symbol> value = LazyGetMovedSymbols(d);
				ImmutableInterlocked.InterlockedCompareExchange(ref lazyMovedSymbols, value, default);
			}, CompletionPart.Members);
		}
	}

	protected abstract Symbol LazyGetAnchor(DiagnosticBag diagnostics);

	protected abstract ImmutableArray<Symbol> LazyGetMovedSymbols(DiagnosticBag diagnostics);

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.EnumValues:
				LazyGetAnchor();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.EnumValues));
				break;
			case CompletionPart.Members:
				LazyGetMovedSymbols();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Members));
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)94143);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	public void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		if (HasComplete(CompletionPart.Members))
		{
			return;
		}
		try
		{
			foreach (Symbol movedSymbol in MovedSymbols)
			{
				cancellationToken.ThrowIfCancellationRequested();
				movedSymbol.ForceComplete(locationOpt, cancellationToken);
			}
		}
		finally
		{
			state.NotePartComplete(CompletionPart.Members);
		}
	}

	private void LazySetSymbol(Action<DiagnosticBag> setSymbol, CompletionPart completionValue)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			setSymbol(instance);
			AddDeclarationDiagnostics(instance);
		}
		finally
		{
			state.NotePartComplete(completionValue);
			instance.Free();
		}
	}
}
