using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SourceChangeAddFieldGroupSymbol : ChangeAddSymbol
{
	private SymbolCompletionState state;

	private Symbol lazyAnchor;

	private ImmutableArray<Symbol> lazyAddSymbols;

	private readonly FieldGroupAddChangeSyntax syntax;

	public override ImmutableArray<Location> Locations { get; }

	public override Symbol ContainingSymbol { get; }

	public override ChangeKind ChangeKind { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public override SymbolKind ChangeTargetKind => SymbolKind.FieldGroup;

	internal override Symbol Anchor
	{
		get
		{
			LazyGetAnchor();
			return lazyAnchor;
		}
	}

	internal override ImmutableArray<Symbol> AddedSymbols
	{
		get
		{
			LazyGetAddedSymbols();
			return lazyAddSymbols;
		}
	}

	public SourceChangeAddFieldGroupSymbol(TableExtensionTypeSymbol owner, FieldGroupAddChangeSyntax syntax)
	{
		ContainingSymbol = owner;
		this.syntax = syntax;
		Locations = ImmutableArray.Create((Location)new SourceLocation(syntax));
		DeclaringSyntaxReferences = ImmutableArray.Create(this.syntax.GetReference());
		ChangeKind = syntax.ChangeKeyword.Kind.ToChangeKind();
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
				ForceCompleteMembers(locationOpt, cancellationToken);
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

	private void LazyGetAddedSymbols()
	{
		if (lazyAddSymbols != null)
		{
			return;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance(syntax.Fields.Count);
		try
		{
			foreach (IdentifierNameSyntax field in syntax.Fields)
			{
				TableTypeSymbol baseTable = (TableTypeSymbol)((TableExtensionTypeSymbol)ContainingSymbol).Target;
				List<Symbol> foundSymbols = this.GetTableFields(baseTable, field).ToList();
				Symbol targetSymbolFromList = ChangeSymbolExtensions.GetTargetSymbolFromList(this, foundSymbols, field, ErrorCode.ERR_FieldNotFound, ErrorCode.ERR_FieldNotFound, instance);
				instance2.Add(targetSymbolFromList);
			}
			ImmutableArray<Symbol> value = instance2.ToImmutable();
			AddDeclarationDiagnostics(instance);
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyAddSymbols, value, default);
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	private void LazyGetAnchor()
	{
		if (state.HasComplete(CompletionPart.EnumValues))
		{
			return;
		}
		lock (syntax.Parent)
		{
			LazySetSymbol((DiagnosticBag d) =>
			{
				lazyAnchor = LazyGetAnchor(d);
				Binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(d, lazyAnchor, syntax.Anchor, null);
			}, CompletionPart.EnumValues, ref state);
		}
	}

	private Symbol LazyGetAnchor(DiagnosticBag diagnostics)
	{
		TableTypeSymbol tableTypeSymbol = ((ApplicationObjectExtensionTypeSymbol)ContainingSymbol).Target as TableTypeSymbol;
		if (tableTypeSymbol == null)
		{
			return null;
		}
		IdentifierNameSyntax anchor = syntax.Anchor;
		List<Symbol> foundSymbols = this.GetTableFieldgroups(tableTypeSymbol, anchor).ToList();
		return ChangeSymbolExtensions.GetTargetSymbolFromList(this, foundSymbols, anchor, ErrorCode.ERR_FieldGroupNotFound, ErrorCode.ERR_FieldGroupNotFound, diagnostics);
	}

	private void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return;
		}
		try
		{
			foreach (Symbol addedSymbol in AddedSymbols)
			{
				cancellationToken.ThrowIfCancellationRequested();
				addedSymbol.ForceComplete(locationOpt, cancellationToken);
			}
		}
		finally
		{
			state.NotePartComplete(CompletionPart.Members);
		}
	}
}
