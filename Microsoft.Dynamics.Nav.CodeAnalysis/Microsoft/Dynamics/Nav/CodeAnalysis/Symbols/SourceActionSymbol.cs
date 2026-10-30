using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceActionSymbol : ActionSymbol
{
	private SymbolCompletionState state;

	private readonly SyntaxToken identifierToken;

	private ImmutableArray<PropertySymbol> properties;

	private ImmutableArray<ActionSymbol> actions;

	private ImmutableArray<MethodSymbol> triggers;

	private ImmutableArray<Symbol> lazyMembers;

	public override ActionKind ActionKind { get; }

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

	internal ActionBaseSyntax ActionBaseSyntax { get; }

	public override ImmutableArray<ActionSymbol> Actions
	{
		get
		{
			LazyGetActions();
			return actions;
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

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(identifierToken.Parent.Parent.GetReference());

	internal override bool RequiresCompletion => true;

	public override TypeSymbol Type => NavCorLib.None;

	public static SourceActionSymbol Create(Symbol owner, ActionBaseSyntax syntax)
	{
		string name = syntax.Name.Unquoted();
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceActionSymbol(owner, syntax, syntax.Name.Identifier, name, locations);
	}

	private SourceActionSymbol(Symbol containingSymbol, ActionBaseSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
	{
		ApplicationObjectTypeSymbol containingSymbolOfType = containingSymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		ContainingSymbol = containingSymbol;
		ActionBaseSyntax = syntax;
		identifierToken = identifier;
		Name = name;
		Id = IdSpace.GetMemberId(containingSymbolOfType.Id, Name);
		Locations = locations;
		ActionKind = syntax.ActionKeyword.Kind.ToActionKind();
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
				ForceCompleteActions(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Members));
				break;
			case CompletionPart.FieldNameListSymbol:
				ForceCompleteTriggers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FieldNameListSymbol));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckAction();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.StartMethodChecks));
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FinishMethodChecks));
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)72623);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
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

	private void ForceCompleteActions(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetActions();
		foreach (ActionSymbol action in actions)
		{
			cancellationToken.ThrowIfCancellationRequested();
			action.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.Members);
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

	private void LazyGetProperties()
	{
		if (!(properties != null))
		{
			ImmutableArray<PropertySymbol> value = ActionBaseSyntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref properties, value, default);
		}
	}

	private void LazyGetActions()
	{
		if (!(actions != null))
		{
			ImmutableArray<ActionSymbol> value = ((ActionBaseSyntax is ActionGroupBaseSyntax) ? ((IEnumerable<ActionSymbol>)((ActionGroupBaseSyntax)ActionBaseSyntax).Actions.Select((ActionBaseSyntax trigger) => Create(this, trigger))).ToImmutableArrayOrEmpty() : ImmutableArray<ActionSymbol>.Empty);
			ImmutableInterlocked.InterlockedCompareExchange(ref actions, value, default);
		}
	}

	private void LazyGetTriggers()
	{
		if (!(triggers != null))
		{
			ImmutableArray<MethodSymbol> value = ((ActionBaseSyntax.Kind == SyntaxKind.PageAction) ? ((IEnumerable<MethodSymbol>)((PageActionSyntax)ActionBaseSyntax).Triggers.Select((TriggerDeclarationSyntax trigger) => SourceTriggerSymbol.Create(this, trigger))).ToImmutableArrayOrEmpty() : ImmutableArray<MethodSymbol>.Empty);
			ImmutableInterlocked.InterlockedCompareExchange(ref triggers, value, default);
		}
	}

	private void CheckAction()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (ActionBaseSyntax.Parent)
		{
			if (state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				try
				{
					TriggerValidationHelper.CheckTriggers(this, Triggers, GetTriggerTypeInfo, instance);
					CheckActionImages(instance);
					AddDeclarationDiagnostics(instance);
					return;
				}
				finally
				{
					state.NotePartComplete(CompletionPart.FinishMethodChecks);
					instance.Free();
				}
			}
		}
	}

	private void CheckActionImages(DiagnosticBag diagnostics)
	{
		PropertySymbol property = GetProperty(PropertyKind.Image);
		if (property == null)
		{
			return;
		}
		PropertySyntax propertySyntax = (PropertySyntax)property.GetFirstSyntax();
		Location location = propertySyntax.Value.Location;
		string valueText = property.ValueText;
		if (ActionKind == ActionKind.Action)
		{
			if (PageMemberSymbolHelpers.IsContainedInControlOfKind(this, ControlKind.CueGroup))
			{
				if (!ImageResources.GetActionCueGroupImageNames().Contains(valueText))
				{
					diagnostics.Add(ErrorCode.WRN_ImageNotValid, location, valueText);
				}
			}
			else if (!ImageResources.GetActionImageNames().Contains(valueText))
			{
				diagnostics.Add(ErrorCode.WRN_ImageNotValid, location, valueText);
			}
		}
		if (ActionKind != ActionKind.Group)
		{
			return;
		}
		if (PageMemberSymbolHelpers.IsDirectlyContainedInGivenActionArea(this, ActionAreaKind.Sections))
		{
			if (!ImageResources.GetRoleCenterActionGroupImageNames().Contains(valueText))
			{
				diagnostics.Add(ErrorCode.WRN_ImageNotValid, location, valueText);
			}
		}
		else if (PageMemberSymbolHelpers.IsContainedInGroupInGivenActionArea(this, ActionAreaKind.Sections))
		{
			diagnostics.Add(ErrorCode.WRN_ImageOnNestedActionGroupInSectionsArea, propertySyntax.Location);
		}
		else if (!ImageResources.GetActionImageNames().Contains(valueText))
		{
			diagnostics.Add(ErrorCode.WRN_ImageNotValid, location, valueText);
		}
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(Actions.Length + Triggers.Length + Properties.Length);
		ImmutableArray<Symbol> value;
		try
		{
			instance.AddRange(Actions);
			instance.AddRange(Triggers);
			instance.AddRange(Properties);
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
