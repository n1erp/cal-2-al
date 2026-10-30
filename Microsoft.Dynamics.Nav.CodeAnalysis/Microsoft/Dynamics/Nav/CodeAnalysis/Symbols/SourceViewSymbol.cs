using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceViewSymbol : ViewSymbol
{
	private SymbolCompletionState state;

	private readonly ApplicationObjectTypeSymbol applicationObject;

	private readonly SyntaxToken identifierToken;

	private ImmutableArray<PropertySymbol> properties;

	private ImmutableArray<Symbol> changeSymbols;

	private ImmutableArray<ChangeModifySymbol> modifySymbols;

	private ImmutableArray<ChangeMoveSymbol> moveSymbols;

	private ImmutableArray<Symbol> lazyMembers;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override int Id { get; }

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return properties;
		}
	}

	internal PageViewSyntax ViewSyntax { get; }

	internal override ImmutableArray<ChangeModifySymbol> ModifySymbols
	{
		get
		{
			LazyGetModifiedSymbols();
			return modifySymbols;
		}
	}

	internal override ImmutableArray<ChangeMoveSymbol> MoveSymbols
	{
		get
		{
			LazyGetMovedSymbols();
			return moveSymbols;
		}
	}

	private ImmutableArray<Symbol> ChangeSymbols
	{
		get
		{
			LazyGetChangedSymbols();
			return changeSymbols;
		}
	}

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(identifierToken.Parent.Parent.GetReference());

	internal override bool RequiresCompletion => true;

	public static SourceViewSymbol Create(Symbol owner, PageViewSyntax syntax)
	{
		string name = syntax.Name.Unquoted();
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceViewSymbol(owner, syntax, syntax.Name.Identifier, name, locations);
	}

	private SourceViewSymbol(Symbol containingSymbol, PageViewSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
	{
		applicationObject = containingSymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		ContainingSymbol = containingSymbol;
		ViewSyntax = syntax;
		identifierToken = identifier;
		Name = name;
		Id = IdSpace.GetMemberId(applicationObject.Id, Name);
		Locations = locations;
	}

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
				CheckView();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.StartMethodChecks));
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FinishMethodChecks));
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

	private void ForceCompleteProperties(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		foreach (PropertySymbol property in Properties)
		{
			cancellationToken.ThrowIfCancellationRequested();
			property.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.Variables);
	}

	private void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetMembers();
		if (!state.HasComplete(CompletionPart.MembersCompleted))
		{
			foreach (Symbol changeSymbol in ChangeSymbols)
			{
				cancellationToken.ThrowIfCancellationRequested();
				changeSymbol.ForceComplete(locationOpt, cancellationToken);
			}
			state.NotePartComplete(CompletionPart.MembersCompleted);
		}
	}

	private void LazyGetMembers()
	{
		if (!state.HasComplete(CompletionPart.Members))
		{
			LazyGetChangedSymbols();
			state.NotePartComplete(CompletionPart.Members);
		}
	}

	private void LazyGetProperties()
	{
		if (!(properties != null))
		{
			ImmutableArray<PropertySymbol> value = ViewSyntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref properties, value, default);
		}
	}

	private void LazyGetChangedSymbols()
	{
		if (!(changeSymbols != null))
		{
			ImmutableArray<Symbol> changeSymbolsList = GetChangeSymbolsList();
			ImmutableInterlocked.InterlockedCompareExchange(ref changeSymbols, changeSymbolsList, default);
		}
	}

	private ImmutableArray<Symbol> GetChangeSymbolsList()
	{
		if (ViewSyntax.Layout == null || ViewSyntax.Layout.Changes.Count == 0)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance(ViewSyntax.Layout.Changes.Count);
		try
		{
			foreach (ControlChangeBaseSyntax change in ViewSyntax.Layout.Changes)
			{
				switch (change.Kind)
				{
				case SyntaxKind.ControlMoveChange:
					instance2.Add(SourceChangeMoveControlSymbol.Create(this, (ControlMoveChangeSyntax)change));
					break;
				case SyntaxKind.ControlModifyChange:
					instance2.Add(SourceChangeModifyControlSymbol.Create(this, (ControlModifyChangeSyntax)change));
					break;
				case SyntaxKind.ControlAddChange:
					instance.Add(ErrorCode.ERR_AddChangeNotSupportedForViews, change.GetFirstToken().GetLocation());
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(change.Kind);
				}
			}
			AddDeclarationDiagnostics(instance);
			return instance2.ToImmutableArray();
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	private void LazyGetModifiedSymbols()
	{
		if (!(modifySymbols != null))
		{
			ImmutableArray<ChangeModifySymbol> value = ChangeSymbols.OfType<ChangeModifySymbol>().ToImmutableArray();
			ImmutableInterlocked.InterlockedCompareExchange(ref modifySymbols, value, default);
		}
	}

	private void LazyGetMovedSymbols()
	{
		if (!(moveSymbols != null))
		{
			ImmutableArray<ChangeMoveSymbol> value = ChangeSymbols.OfType<ChangeMoveSymbol>().ToImmutableArray();
			ImmutableInterlocked.InterlockedCompareExchange(ref moveSymbols, value, default);
		}
	}

	private void CheckView()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (ViewSyntax.Parent)
		{
			if (!state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				return;
			}
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				if (string.IsNullOrWhiteSpace(Name))
				{
					Location location = ViewSyntax.Name.GetLocation();
					instance.Add(ErrorCode.ERR_InvalidViewName, location, Name);
				}
				AddDeclarationDiagnostics(instance);
			}
			finally
			{
				state.NotePartComplete(CompletionPart.FinishMethodChecks);
				instance.Free();
			}
		}
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(Properties.Length + ModifySymbols.Length + MoveSymbols.Length);
		ImmutableArray<Symbol> value;
		try
		{
			instance.AddRange(Properties);
			instance.AddRange(ModifySymbols);
			instance.AddRange(MoveSymbols);
			value = instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, value, default);
		return lazyMembers;
	}
}
