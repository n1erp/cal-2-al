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
internal sealed class SourceQueryDataItemSymbol : QueryDataItemSymbol
{
	private readonly SyntaxToken identifierToken;

	private ImmutableArray<SourceQueryColumnSymbol> columns;

	private ImmutableArray<SourceQueryDataItemSymbol> dataItems;

	private ImmutableArray<SourceQueryFilterSymbol> filters;

	private ImmutableArray<Symbol> lazyMembers;

	private ImmutableArray<PropertySymbol> properties;

	private TableTypeSymbol relatedTable;

	private SymbolCompletionState state;

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

	public override TableTypeSymbol RelatedTable
	{
		get
		{
			LazyGetMembers();
			return relatedTable;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(Syntax.GetReference());

	public ImmutableArray<SourceQueryColumnSymbol> Columns
	{
		get
		{
			LazyGetMembers();
			return columns;
		}
	}

	public ImmutableArray<SourceQueryDataItemSymbol> DataItems
	{
		get
		{
			LazyGetMembers();
			return dataItems;
		}
	}

	public ImmutableArray<SourceQueryFilterSymbol> Filters
	{
		get
		{
			LazyGetMembers();
			return filters;
		}
	}

	internal int Indentation { get; }

	internal QueryDataItemSyntax Syntax { get; }

	internal override bool RequiresCompletion => true;

	public static SourceQueryDataItemSymbol Create(Symbol containingSymbol, QueryDataItemSyntax syntax, int indentation)
	{
		string valueText = syntax.Name.Identifier.ValueText;
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceQueryDataItemSymbol(containingSymbol, syntax, syntax.Name.Identifier, valueText, locations, indentation);
	}

	private SourceQueryDataItemSymbol(Symbol containingSymbol, QueryDataItemSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations, int indentation)
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

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.AddRange(Properties);
		instance.AddRange(DataItems);
		instance.AddRange(Columns);
		instance.AddRange(Filters);
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, instance.ToImmutableAndFree(), default);
		return lazyMembers;
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
				state.NotePartComplete((CompletionPart)73519);
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
			foreach (SourceQueryDataItemSymbol dataItem in dataItems)
			{
				cancellationToken.ThrowIfCancellationRequested();
				dataItem.ForceComplete(locationOpt, cancellationToken);
			}
			foreach (SourceQueryColumnSymbol column in columns)
			{
				cancellationToken.ThrowIfCancellationRequested();
				column.ForceComplete(locationOpt, cancellationToken);
			}
			foreach (SourceQueryFilterSymbol filter in filters)
			{
				cancellationToken.ThrowIfCancellationRequested();
				filter.ForceComplete(locationOpt, cancellationToken);
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
				dataItems = (from dataItem in Syntax.Elements.OfType<QueryDataItemSyntax>()
					select Create(this, dataItem, Indentation + 1)).ToImmutableArray();
				columns = (from column in Syntax.Elements.OfType<QueryColumnSyntax>()
					select SourceQueryColumnSymbol.Create(this, column)).ToImmutableArray();
				filters = (from filter in Syntax.Elements.OfType<QueryFilterSyntax>()
					select SourceQueryFilterSymbol.Create(this, filter)).ToImmutableArray();
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
			if (!state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				return;
			}
			LazySetSymbol((DiagnosticBag diagnostics) =>
			{
				CheckForUnion(diagnostics);
				if (!Name.IsValidClsIdentifier())
				{
					diagnostics.Add(ErrorCode.ERR_NameIsNotValidClsIdentifier, Syntax.Name.GetLocation(), Name);
				}
			}, CompletionPart.FinishMethodChecks, ref state);
		}
	}

	private void CheckForUnion(DiagnosticBag diagnostics)
	{
		ImmutableArray<Symbol> immutableArray = GetMembers().WhereAsArray((Symbol m) => m.Kind == SymbolKind.QueryDataItem);
		if (immutableArray.Length > 1)
		{
			foreach (Symbol item in immutableArray)
			{
				diagnostics.Add(ErrorCode.ERR_UnionsAreNotSupportedInQueries, item.GetLocation());
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

	private void TrySetSourceTable(DiagnosticBag diagnostics)
	{
		Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(Syntax.DataItemTable);
		string identifierOrLiteralValue = Syntax.DataItemTable.Identifier.GetIdentifierOrLiteralValue();
		if (identifierOrLiteralValue != null)
		{
			relatedTable = (TableTypeSymbol)binder.GetApplicationObjectTypeSymbol(SymbolKind.Table, Syntax.DataItemTable, diagnostics);
			if (relatedTable == null)
			{
				diagnostics.Add(ErrorCode.ERR_MissingApplicationObject, Syntax.DataItemTable.Location, SymbolKind.Table.ToString(), identifierOrLiteralValue);
			}
			else if (!relatedTable.CanCompileWithTarget(DeclaringCompilation.Options.Target))
			{
				diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, Syntax.DataItemTable.Location, identifierOrLiteralValue, DeclaringCompilation.Options.Target.ToString());
			}
			else
			{
				binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, relatedTable, Syntax.DataItemTable);
			}
		}
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitQueryDataItem(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItem(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitQueryDataItem(this, a);
	}
}
