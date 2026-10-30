using System;
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

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceControlSymbol : ControlSymbol
{
	private static readonly ImmutableArray<PropertyKind> ApiPageMatchingPropertyKinds = ImmutableArray.Create(new PropertyKind[3]
	{
		PropertyKind.APIPublisher,
		PropertyKind.APIGroup,
		PropertyKind.APIVersion
	});

	private static readonly ImmutableArray<PropertyKind> ApiPartMatchingPropertyKinds = ImmutableArray.Create(new PropertyKind[2]
	{
		PropertyKind.EntityName,
		PropertyKind.EntitySetName
	});

	private SymbolCompletionState state;

	private BoundExpression lazyBoundSourceExpression;

	private PageTypeSymbol lazyPart;

	private ControlAddInTypeSymbol lazyControlAddIn;

	private FieldSymbol lazyRelatedFieldSymbol;

	private readonly SyntaxToken identifierToken;

	private ImmutableArray<PropertySymbol> properties;

	private ImmutableArray<ControlSymbol> controls;

	private ImmutableArray<ActionSymbol> actions;

	private ImmutableArray<MethodSymbol> triggers;

	private readonly ApplicationObjectTypeSymbol applicationObject;

	private ImmutableArray<Symbol> lazyMembers;

	public override ControlKind ControlKind { get; }

	public override Symbol ContainingSymbol { get; }

	public override string MetadataName { get; }

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

	internal ControlBaseSyntax ControlBaseSyntax { get; }

	public override ImmutableArray<ControlSymbol> Controls
	{
		get
		{
			LazyGetMembers();
			return controls;
		}
	}

	public override ImmutableArray<ActionSymbol> Actions
	{
		get
		{
			LazyGetMembers();
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

	public override TypeSymbol Type
	{
		get
		{
			LazyGetType();
			if (lazyBoundSourceExpression == null)
			{
				return NavCorLib.None;
			}
			return lazyBoundSourceExpression.Type;
		}
	}

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(identifierToken.Parent.Parent.GetReference());

	internal override bool RequiresCompletion => true;

	internal override TypeSymbol BaseType => RelatedControlAddInSymbol;

	public override PageTypeSymbol RelatedPartSymbol
	{
		get
		{
			LazyGetType();
			return lazyPart;
		}
	}

	public override ControlAddInTypeSymbol RelatedControlAddInSymbol
	{
		get
		{
			LazyGetType();
			return lazyControlAddIn;
		}
	}

	internal BoundExpression SourceExpression
	{
		get
		{
			LazyGetType();
			return lazyBoundSourceExpression;
		}
	}

	internal bool IsSourceRecordField => GetSourceRecordFieldAccess() != null;

	public override FieldSymbol RelatedFieldSymbol
	{
		get
		{
			LazyGetType();
			return lazyRelatedFieldSymbol;
		}
	}

	public static SourceControlSymbol Create(Symbol owner, ControlBaseSyntax syntax)
	{
		string name = syntax.Name.Unquoted();
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceControlSymbol(owner, syntax, syntax.Name.Identifier, name, locations);
	}

	private SourceControlSymbol(Symbol containingSymbol, ControlBaseSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
	{
		applicationObject = containingSymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		ContainingSymbol = containingSymbol;
		ControlBaseSyntax = syntax;
		identifierToken = identifier;
		Name = name;
		MetadataName = name.MangleUnquotedIdentifierName();
		Id = IdSpace.GetMemberId(applicationObject.Id, Name);
		Locations = locations;
		ControlKind = syntax.ControlKeyword.Kind.ToControlKind();
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
			case CompletionPart.ReturnType:
				LazyGetType();
				break;
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
			case CompletionPart.FieldNameListSymbol:
				ForceCompleteTriggers(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FieldNameListSymbol));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckControl();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.StartMethodChecks));
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FinishMethodChecks));
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)72487);
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

	private void ForceCompleteMembers(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetMembers();
		if (!state.HasComplete(CompletionPart.MembersCompleted))
		{
			foreach (ControlSymbol control in controls)
			{
				cancellationToken.ThrowIfCancellationRequested();
				control.ForceComplete(locationOpt, cancellationToken);
			}
			foreach (ActionSymbol action in actions)
			{
				cancellationToken.ThrowIfCancellationRequested();
				action.ForceComplete(locationOpt, cancellationToken);
			}
			state.NotePartComplete(CompletionPart.MembersCompleted);
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

	private void LazyGetType()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		lock (identifierToken.Parent)
		{
			if (state.HasComplete(CompletionPart.ReturnType))
			{
				return;
			}
			LazySetSymbol((DiagnosticBag diagnostics) =>
			{
				if (ControlBaseSyntax is PageFieldSyntax { Expression: not null } pageFieldSyntax)
				{
					Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(ControlBaseSyntax.SyntaxTree).GetBinder(pageFieldSyntax.Expression);
					lazyBoundSourceExpression = binder.BindExpression(pageFieldSyntax.Expression, diagnostics);
					lazyRelatedFieldSymbol = (lazyBoundSourceExpression as BoundFieldAccess)?.FieldSymbol;
				}
				else
				{
					if (ControlBaseSyntax is PagePartSyntax { PartName: not null } pagePartSyntax)
					{
						PageTypeSymbol value = ContainingSymbol.DeclaringCompilation.GetBinderFactory(ControlBaseSyntax.SyntaxTree).GetBinder(pagePartSyntax.PartName).GetApplicationObjectTypeSymbol(SymbolKind.Page, pagePartSyntax.PartName, diagnostics) as PageTypeSymbol;
						Interlocked.CompareExchange(ref lazyPart, value, null);
					}
					if (ControlBaseSyntax is PageUserControlSyntax { ControlAddIn: not null } pageUserControlSyntax)
					{
						ControlAddInTypeSymbol controlAddInTypeSymbol = DeclaringCompilation.GetControlAddInTypeSymbol(pageUserControlSyntax.ControlAddIn, diagnostics);
						Interlocked.CompareExchange(ref lazyControlAddIn, controlAddInTypeSymbol, null);
					}
				}
			}, CompletionPart.ReturnType);
		}
	}

	private void LazyGetProperties()
	{
		if (!(properties != null))
		{
			ImmutableArray<PropertySymbol> value = ControlBaseSyntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref properties, value, default);
		}
	}

	private void LazyGetMembers()
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return;
		}
		LazySetSymbol((DiagnosticBag d) =>
		{
			ControlGroupBaseSyntax controlGroupBaseSyntax = ControlBaseSyntax as ControlGroupBaseSyntax;
			controls = ((controlGroupBaseSyntax != null) ? ((IEnumerable<ControlSymbol>)controlGroupBaseSyntax.Controls.Select((ControlBaseSyntax control) => Create(this, control))).ToImmutableArray() : ImmutableArray<ControlSymbol>.Empty);
			PageGroupSyntax pageGroupSyntax = ControlBaseSyntax as PageGroupSyntax;
			ImmutableArray<ActionSymbol>? immutableArray;
			if (pageGroupSyntax == null)
			{
				immutableArray = null;
			}
			else
			{
				GroupActionListSyntax actionList = pageGroupSyntax.ActionList;
				immutableArray = ((actionList != null) ? new ImmutableArray<ActionSymbol>?(((IEnumerable<ActionSymbol>)actionList.Actions.Select((ActionBaseSyntax action) => SourceActionSymbol.Create(this, action))).ToImmutableArray()) : ((ImmutableArray<ActionSymbol>?)null));
			}
			actions = immutableArray ?? ImmutableArray<ActionSymbol>.Empty;
		}, CompletionPart.Members);
	}

	private void LazyGetTriggers()
	{
		if (triggers != null)
		{
			return;
		}
		ImmutableArray<MethodSymbol> value = ImmutableArray<MethodSymbol>.Empty;
		if (ControlBaseSyntax.Kind == SyntaxKind.PageField)
		{
			value = ((IEnumerable<MethodSymbol>)((PageFieldSyntax)ControlBaseSyntax).Triggers.Select((TriggerDeclarationSyntax trigger) => SourceTriggerSymbol.Create(this, trigger))).ToImmutableArray();
		}
		if (ControlBaseSyntax.Kind == SyntaxKind.PageUserControl)
		{
			value = ((IEnumerable<MethodSymbol>)((PageUserControlSyntax)ControlBaseSyntax).Triggers.Select((TriggerDeclarationSyntax trigger) => SourceTriggerSymbol.Create(this, trigger))).ToImmutableArray();
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref triggers, value, default);
	}

	private void CheckControl()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (ControlBaseSyntax.Parent)
		{
			if (!state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				return;
			}
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				if (lazyBoundSourceExpression != null && !lazyBoundSourceExpression.HasAnyErrors && !Type.IsValidPageFieldType())
				{
					instance.Add(ErrorCode.ERR_InvalidFieldType, lazyBoundSourceExpression.Syntax.Location, lazyBoundSourceExpression.Syntax);
				}
				if (ControlKind == ControlKind.Part)
				{
					if (RelatedPartSymbol == null)
					{
						PagePartSyntax pagePartSyntax = (PagePartSyntax)ControlBaseSyntax;
						instance.Add(ErrorCode.ERR_PageNotFound, pagePartSyntax.PartName.Location, pagePartSyntax.PartName);
					}
					else
					{
						switch (RelatedPartSymbol.PageType)
						{
						case PageTypeKind.API:
							CheckSubpageOfTypeApi(instance);
							break;
						case PageTypeKind.HeadlinePart:
							if (applicationObject.GetContainingPage().PageType != PageTypeKind.RoleCenter)
							{
								PagePartSyntax pagePartSyntax3 = (PagePartSyntax)ControlBaseSyntax;
								instance.Add(ErrorCode.ERR_HeadlinePartMustUsedInRoleCenter, pagePartSyntax3.PartName.Location, pagePartSyntax3.PartName);
							}
							break;
						default:
							if (!RelatedPartSymbol.PageType.IsPartPageType())
							{
								PagePartSyntax pagePartSyntax2 = (PagePartSyntax)ControlBaseSyntax;
								if (applicationObject.GetContainingPage().PageType != PageTypeKind.API)
								{
									instance.Add(ErrorCode.WRN_ERR_PagePartMustBePart, pagePartSyntax2.PartName.Location, pagePartSyntax2.PartName);
								}
								else
								{
									instance.Add(ErrorCode.ERR_ApiPagePartMustBePartOrApi, pagePartSyntax2.PartName.Location, pagePartSyntax2.PartName);
								}
							}
							break;
						}
					}
				}
				if (ControlKind == ControlKind.UserControl)
				{
					CheckUserControl(instance);
				}
				if (ControlKind == ControlKind.Repeater)
				{
					CheckRepeaterControl(instance);
				}
				if (ControlKind == ControlKind.Field)
				{
					CheckFieldControlImages(instance);
				}
				TriggerValidationHelper.CheckTriggers(this, Triggers, GetTriggerTypeInfo, instance);
				AddDeclarationDiagnostics(instance);
			}
			finally
			{
				state.NotePartComplete(CompletionPart.FinishMethodChecks);
				instance.Free();
			}
		}
	}

	private void CheckSubpageOfTypeApi(DiagnosticBag diagnostics)
	{
		IPageBaseTypeSymbol containingPage = applicationObject.GetContainingPage();
		if (containingPage.PageType != PageTypeKind.API)
		{
			PagePartSyntax pagePartSyntax = (PagePartSyntax)ControlBaseSyntax;
			diagnostics.Add(ErrorCode.ERR_ApiPageCouldOnlyBePartOfAnotherApiPage, pagePartSyntax.PartName.Location, pagePartSyntax.PartName);
		}
		else
		{
			CheckMatchingPageOfTypeApiAndSubpageOfTypeApiProperties(containingPage, diagnostics);
			CheckMatchingPageOfTypeApiAndControlOfKindPartProperties(containingPage, diagnostics);
		}
	}

	private void CheckMatchingPageOfTypeApiAndSubpageOfTypeApiProperties(IPageBaseTypeSymbol containingPage, DiagnosticBag diagnostics)
	{
		foreach (PropertyKind apiPageMatchingPropertyKind in ApiPageMatchingPropertyKinds)
		{
			IPropertySymbol property = containingPage.GetProperty(apiPageMatchingPropertyKind);
			PropertySymbol property2 = RelatedPartSymbol.GetProperty(apiPageMatchingPropertyKind);
			if (property != null && property2 != null && property.ValueText != property2.ValueText)
			{
				PagePartSyntax pagePartSyntax = (PagePartSyntax)ControlBaseSyntax;
				diagnostics.Add(ErrorCode.ERR_ApiPageAndSubpagePropertyMismatch, pagePartSyntax.PartName.Location, pagePartSyntax.Name, containingPage.Name, RelatedPartSymbol.Name, apiPageMatchingPropertyKind);
			}
		}
	}

	private void CheckMatchingPageOfTypeApiAndControlOfKindPartProperties(IPageBaseTypeSymbol containingPage, DiagnosticBag diagnostics)
	{
		foreach (PropertyKind apiPartMatchingPropertyKind in ApiPartMatchingPropertyKinds)
		{
			PropertySymbol property = GetProperty(apiPartMatchingPropertyKind);
			PropertySymbol property2 = RelatedPartSymbol.GetProperty(apiPartMatchingPropertyKind);
			if (property != null && property2 != null && property.ValueText != property2.ValueText)
			{
				PagePartSyntax pagePartSyntax = (PagePartSyntax)ControlBaseSyntax;
				diagnostics.Add(ErrorCode.ERR_ApiPagePartAndSubpagePropertyMismatch, property.Locations.IsEmpty ? Location.None : property.Locations[0], pagePartSyntax.Name, containingPage.Name, apiPartMatchingPropertyKind);
			}
		}
	}

	private void CheckFieldControlImages(DiagnosticBag diagnostics)
	{
		PropertySymbol property = GetProperty(PropertyKind.Image);
		if (property == null)
		{
			return;
		}
		PropertySyntax propertySyntax = (PropertySyntax)property.GetFirstSyntax();
		if (!PageMemberSymbolHelpers.IsContainedInControlOfKind(this, ControlKind.CueGroup))
		{
			diagnostics.Add(ErrorCode.WRN_ERR_FieldImageOnlyOnFieldInCueGroup, propertySyntax.Location);
			return;
		}
		string valueText = property.ValueText;
		if (!ImageResources.GetFieldCueGroupImageNames().Contains(valueText))
		{
			diagnostics.Add(ErrorCode.WRN_ImageNotValid, propertySyntax.Value.Location, valueText);
		}
	}

	private void CheckRepeaterControl(DiagnosticBag diagnostics)
	{
		IPageBaseTypeSymbol containingPage = this.GetContainingPage();
		if (containingPage != null && containingPage.RelatedTable == null)
		{
			diagnostics.Add(ErrorCode.ERR_RepeaterCanOnlyBeAddedToPagesThatHaveASourceTable, ControlBaseSyntax.Location);
		}
	}

	private void CheckUserControl(DiagnosticBag diagnostics)
	{
		if (RelatedControlAddInSymbol == null)
		{
			PageUserControlSyntax pageUserControlSyntax = (PageUserControlSyntax)ControlBaseSyntax;
			diagnostics.Add(ErrorCode.ERR_ControlAddInNotFound, pageUserControlSyntax.ControlAddIn.Location, pageUserControlSyntax.ControlAddIn);
			return;
		}
		foreach (SourceTriggerSymbol item in Triggers.OfType<SourceTriggerSymbol>())
		{
			IEnumerable<EventSymbol> source = RelatedControlAddInSymbol.GetMembers(item.Name).OfType<EventSymbol>();
			if (source.Count() != 1)
			{
				diagnostics.Add(ErrorCode.ERR_EventNotFound, item.GetSyntax().Name.Location, item.Name);
			}
			else
			{
				source.First().CheckSubscriberParameters(item, diagnostics, enforceExactVarMatch: false);
			}
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
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		instance.AddRange(Controls);
		instance.AddRange(Actions);
		instance.AddRange(Triggers);
		instance.AddRange(Properties);
		if (RelatedPartSymbol != null)
		{
			instance.AddRange(new SynthesizedPropertySymbol(this, "Page", PageReferenceTypeSymbol.Create(RelatedPartSymbol)));
		}
		if (RelatedControlAddInSymbol != null)
		{
			IEnumerable<SynthesizedMethodSymbol> items = from x in RelatedControlAddInSymbol.GetMembers()
				where x.Kind == SymbolKind.Event
				where !Triggers.Any((MethodSymbol methodSymbol) => SemanticFacts.IsSameName(x.Name, methodSymbol.Name))
				select new SynthesizedMethodSymbol(this, x.Name, parameters: x.GetParameters(), metadataName: SourceTriggerSymbol.GetFullTriggerName(this, x.MetadataName).MangleUnquotedIdentifierName());
			instance.AddRange(items);
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, instance.ToImmutableAndFree(), default);
		return lazyMembers;
	}

	internal BoundFieldAccess GetSourceRecordFieldAccess()
	{
		BoundFieldAccess recordFieldAccess = GetRecordFieldAccess();
		if (recordFieldAccess == null || recordFieldAccess.ReceiverOpt?.Kind != BoundKind.Global)
		{
			return null;
		}
		VariableSymbol globalSymbol = ((BoundGlobal)recordFieldAccess.ReceiverOpt).GlobalSymbol;
		if ((object)globalSymbol == null || globalSymbol.VariableKind != VariableKind.Synthesized || !string.Equals(globalSymbol.Name, "Rec"))
		{
			return null;
		}
		return recordFieldAccess;
	}

	internal BoundFieldAccess GetRecordFieldAccess()
	{
		if (this.GetContainingPage()?.RelatedTable == null)
		{
			return null;
		}
		return SourceExpression as BoundFieldAccess;
	}
}
