using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal abstract class SourceChangeModifySymbol : ChangeModifySymbol
{
	private SymbolCompletionState state;

	private readonly PropertyListSyntax propertyListSyntax;

	private ImmutableArray<PropertySymbol> lazyProperties;

	private readonly SyntaxList<TriggerDeclarationSyntax> triggers;

	private ImmutableArray<MethodSymbol> lazyTriggers;

	private ISymbol lazyTarget;

	private ImmutableArray<Symbol> lazyMembers;

	public override ChangeKind ChangeKind => ChangeKind.Modify;

	public override string Name { get; }

	public override string MetadataName { get; }

	protected SyntaxNode Syntax { get; }

	protected abstract SyntaxNode TargetSyntax { get; }

	public override Symbol ContainingSymbol { get; }

	internal override ISymbol Target
	{
		get
		{
			LazyGetTarget();
			return lazyTarget;
		}
	}

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return lazyProperties;
		}
	}

	public override ImmutableArray<MethodSymbol> Triggers
	{
		get
		{
			LazyGetTriggers();
			return lazyTriggers;
		}
	}

	internal override bool RequiresCompletion => true;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => (Target as Symbol)?.PropertyTypeInfos;

	protected SourceChangeModifySymbol(Symbol owner, SyntaxNode syntax, IdentifierNameSyntax identifierName, PropertyListSyntax propertyListSyntax, SyntaxList<TriggerDeclarationSyntax> triggers = default(SyntaxList<TriggerDeclarationSyntax>))
	{
		ContainingSymbol = owner;
		Syntax = syntax;
		string text = identifierName.Identifier.Text;
		Name = text.UnquoteIdentifier();
		MetadataName = text.MangleIdentifierName();
		this.propertyListSyntax = propertyListSyntax;
		this.triggers = triggers;
		Locations = ImmutableArray.Create((Location)new SourceLocation(syntax));
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
	}

	internal override Location GetNameLocation()
	{
		return Syntax.Kind switch
		{
			SyntaxKind.ControlModifyChange => ((ControlModifyChangeSyntax)Syntax).Name.Identifier.GetLocation(), 
			SyntaxKind.ActionModifyChange => ((ActionModifyChangeSyntax)Syntax).Name.Identifier.GetLocation(), 
			SyntaxKind.FieldModification => ((FieldModificationSyntax)Syntax).Name.Identifier.GetLocation(), 
			SyntaxKind.ViewModifyChange => ((ViewModifyChangeSyntax)Syntax).Name.Identifier.GetLocation(), 
			_ => throw ExceptionUtilities.UnexpectedValue(Syntax.Kind), 
		};
	}

	private void LazyGetTarget()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		lock (Syntax.Parent)
		{
			LazySetSymbol((DiagnosticBag d) =>
			{
				lazyTarget = LazyGetTarget(d);
				Binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(d, lazyTarget as Symbol, TargetSyntax, null);
			}, CompletionPart.ReturnType);
		}
	}

	protected abstract ISymbol LazyGetTarget(DiagnosticBag diagnostics);

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
			case CompletionPart.ReturnType:
				LazyGetTarget();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.ReturnType));
				break;
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.FieldNameListSymbol:
				ForceCompleteTriggers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FieldNameListSymbol));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckChangeModifySymbol();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.StartMethodChecks));
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FinishMethodChecks));
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)72679);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void CheckChangeModifySymbol()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (Syntax)
		{
			if (!state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				return;
			}
			DiagnosticBag diagnostics = DiagnosticBag.GetInstance();
			try
			{
				CheckModifySymbol(diagnostics);
				if (ContainingType.Kind == SymbolKind.PageCustomization)
				{
					Triggers.Do((MethodSymbol t) =>
					{
						diagnostics.Add(ErrorCode.ERR_CannotHaveMethodsOnPageCustomization, t.GetLocation());
					});
				}
			}
			finally
			{
				AddDeclarationDiagnostics(diagnostics);
				diagnostics.Free();
				state.NotePartComplete(CompletionPart.FinishMethodChecks);
			}
		}
	}

	protected virtual void CheckModifySymbol(DiagnosticBag diagnostics)
	{
	}

	private void ForceCompleteProperties(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetProperties();
		foreach (PropertySymbol lazyProperty in lazyProperties)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lazyProperty.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.Variables);
	}

	private void LazyGetProperties()
	{
		if (!(lazyProperties != null))
		{
			ImmutableArray<PropertySymbol> value = propertyListSyntax.Properties.CreatePropertySymbols(this);
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyProperties, value, default);
		}
	}

	private void ForceCompleteTriggers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetTriggers();
		foreach (MethodSymbol lazyTrigger in lazyTriggers)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lazyTrigger.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.FieldNameListSymbol);
	}

	private void LazyGetTriggers()
	{
		if (!(lazyTriggers != null))
		{
			ImmutableArray<MethodSymbol> value = ((IEnumerable<MethodSymbol>)triggers.Select((TriggerDeclarationSyntax trigger) => SourceTriggerSymbol.Create(this, trigger))).ToImmutableArray();
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyTriggers, value, default);
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

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableArray<MethodSymbol> immutableArray = Triggers;
		ImmutableArray<PropertySymbol> properties = Properties;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(immutableArray.Length + properties.Length);
		instance.AddRange(immutableArray);
		instance.AddRange(properties);
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, instance.ToImmutableAndFree(), default);
		return lazyMembers;
	}
}
