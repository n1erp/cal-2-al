using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceReportDataItemSymbol : ReportDataItemSymbol
{
	private readonly SyntaxToken identifierToken;

	private ImmutableArray<Symbol> lazyMembers;

	private ImmutableArray<SourceReportColumnSymbol> columns;

	private ImmutableArray<SourceReportDataItemSymbol> dataItems;

	private ImmutableArray<PropertySymbol> properties;

	private SymbolCompletionState state;

	private ImmutableArray<MethodSymbol> triggers;

	private TableTypeSymbol relatedTable;

	public override int Id { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return properties;
		}
	}

	public override ImmutableArray<MethodSymbol> Triggers
	{
		get
		{
			LazyGetTriggers();
			return triggers;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			SyntaxNode parent = identifierToken.Parent.Parent;
			DebugAssertHelper.Assert(parent is ReportDataItemSyntax);
			return ImmutableArray.Create(parent.GetReference());
		}
	}

	public ImmutableArray<SourceReportColumnSymbol> Columns
	{
		get
		{
			LazyGetMembers();
			return columns;
		}
	}

	public ImmutableArray<SourceReportDataItemSymbol> DataItems
	{
		get
		{
			LazyGetMembers();
			return dataItems;
		}
	}

	public override TableTypeSymbol RelatedTable
	{
		get
		{
			LazyGetMembers();
			return relatedTable;
		}
	}

	internal ReportDataItemSyntax Syntax { get; }

	internal int Indentation { get; }

	internal override bool RequiresCompletion => true;

	private SourceReportDataItemSymbol(Symbol containingSymbol, ReportDataItemSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations, int indentation)
	{
		ContainingSymbol = containingSymbol;
		Syntax = syntax;
		identifierToken = identifier;
		Name = name.UnquoteIdentifier();
		MetadataName = name.MangleIdentifierName();
		Id = IdSpace.GetMemberId(Name);
		Locations = locations;
		Indentation = indentation;
	}

	public static SourceReportDataItemSymbol Create(Symbol owner, ReportDataItemSyntax syntax, int indentation)
	{
		string valueText = syntax.Name.Identifier.ValueText;
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceReportDataItemSymbol(owner, syntax, syntax.Name.Identifier, valueText, locations, indentation);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.AddRange(Triggers);
		instance.AddRange(Properties);
		instance.AddRange(Columns);
		instance.AddRange(DataItems);
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, instance.ToImmutableAndFree(), default);
		return lazyMembers;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override bool HasComplete(CompletionPart part)
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
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.FieldNameListSymbol:
				ForceCompleteTriggers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FieldNameListSymbol));
				break;
			case CompletionPart.Members:
				LazyGetMembers();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Members));
				break;
			case CompletionPart.MembersCompleted:
				ForceCompleteMembers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.MembersCompleted));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckDataItem();
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)72495);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetMembers();
		if (!state.HasComplete(CompletionPart.MembersCompleted))
		{
			foreach (SourceReportColumnSymbol column in columns)
			{
				cancellationToken.ThrowIfCancellationRequested();
				column.ForceComplete(locationOpt, cancellationToken);
			}
			foreach (SourceReportDataItemSymbol dataItem in dataItems)
			{
				cancellationToken.ThrowIfCancellationRequested();
				dataItem.ForceComplete(locationOpt, cancellationToken);
			}
			state.NotePartComplete(CompletionPart.MembersCompleted);
		}
	}

	private void LazyGetMembers()
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return;
		}
		lock (identifierToken.Parent)
		{
			if (state.HasComplete(CompletionPart.Members))
			{
				return;
			}
			LazySetSymbol((DiagnosticBag diagnostics) =>
			{
				ReportDataItemSyntax syntax = Syntax;
				columns = ((syntax != null) ? (from column in syntax.Elements.OfType<ReportColumnSyntax>()
					select SourceReportColumnSymbol.Create(this, column)).ToImmutableArray() : ImmutableArray<SourceReportColumnSymbol>.Empty);
				ReportDataItemSyntax syntax2 = Syntax;
				dataItems = ((syntax2 != null) ? (from dataItem in syntax2.Elements.OfType<ReportDataItemSyntax>()
					select Create(this, dataItem, Indentation + 1)).ToImmutableArray() : ImmutableArray<SourceReportDataItemSymbol>.Empty);
				TrySetSourceTable(diagnostics);
			}, CompletionPart.Members, ref state);
		}
	}

	private void CheckDataItem()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (Syntax.Parent)
		{
			if (state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				LazySetSymbol((DiagnosticBag diagnostics) =>
				{
					TriggerValidationHelper.CheckTriggers(this, Triggers, GetTriggerTypeInfo, diagnostics);
				}, CompletionPart.FinishMethodChecks, ref state);
			}
		}
	}

	private void ForceCompleteProperties(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetProperties();
		foreach (PropertySymbol property in properties)
		{
			cancellationToken.ThrowIfCancellationRequested();
			property.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.Variables);
	}

	private void LazyGetProperties()
	{
		if (!(properties != null))
		{
			ImmutableArray<PropertySymbol> value = Syntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref properties, value, default);
		}
	}

	private void ForceCompleteTriggers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetTriggers();
		foreach (MethodSymbol trigger in triggers)
		{
			cancellationToken.ThrowIfCancellationRequested();
			trigger.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.FieldNameListSymbol);
	}

	private void LazyGetTriggers()
	{
		if (!(triggers != null))
		{
			ImmutableArray<MethodSymbol> value = ((IEnumerable<MethodSymbol>)Syntax.Triggers.Select((TriggerDeclarationSyntax trigger) => SourceTriggerSymbol.Create(this, trigger))).ToImmutableArray();
			ImmutableInterlocked.InterlockedCompareExchange(ref triggers, value, default);
		}
	}

	private void TrySetSourceTable(DiagnosticBag diagnostics)
	{
		Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(Syntax.DataItemTable);
		TableTypeSymbol tableTypeSymbol = (TableTypeSymbol)binder.GetApplicationObjectTypeSymbol(SymbolKind.Table, Syntax.DataItemTable, diagnostics);
		if (tableTypeSymbol == null)
		{
			string text = Syntax.DataItemTable.Identifier.GetIdentifierOrLiteralValue() ?? string.Empty;
			diagnostics.Add(ErrorCode.ERR_MissingApplicationObject, Syntax.DataItemTable.Location, SymbolKind.Table.ToString(), text);
		}
		else if (!tableTypeSymbol.CanCompileWithTarget(DeclaringCompilation.Options.Target))
		{
			diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, Syntax.DataItemTable.Location, tableTypeSymbol.Name, DeclaringCompilation.Options.Target.ToString());
		}
		else
		{
			binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, tableTypeSymbol, Syntax.DataItemTable);
			relatedTable = tableTypeSymbol;
		}
	}
}
