using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class EventSymbol : Symbol, IEventDefinition, ITypeDefinitionMember, ITypeMemberReference, IReference, INamedEntity, IDefinition, IEventSymbol, ISymbol, IEquatable<ISymbol>
{
	IMethodReference IEventDefinition.Adder => AddMethod;

	IMethodReference IEventDefinition.Remover => RemoveMethod;

	bool IEventDefinition.IsRuntimeSpecial => HasRuntimeSpecialName;

	internal virtual bool HasRuntimeSpecialName => false;

	bool IEventDefinition.IsSpecialName => HasSpecialName;

	IMethodReference IEventDefinition.Caller => null;

	ITypeDefinition ITypeDefinitionMember.ContainingTypeDefinition => ContainingType;

	TypeMemberVisibility ITypeDefinitionMember.Visibility => PEModuleBuilder.MemberVisibility(this);

	string INamedEntity.Name => MetadataName;

	public new virtual EventSymbol OriginalDefinition => this;

	protected sealed override Symbol OriginalSymbolDefinition => OriginalDefinition;

	public abstract TypeSymbol Type { get; }

	public abstract MethodSymbol AddMethod { get; }

	public abstract MethodSymbol RemoveMethod { get; }

	internal bool HasAssociatedField => (object)AssociatedField != null;

	public abstract bool IsWindowsRuntimeEvent { get; }

	internal virtual bool IsDirectlyExcludedFromCodeCoverage => false;

	internal abstract bool HasSpecialName { get; }

	internal virtual FieldSymbol AssociatedField => null;

	public EventSymbol OverriddenEvent
	{
		get
		{
			if (IsOverride)
			{
				if (IsDefinition)
				{
					return (EventSymbol)OverriddenOrHiddenMembers.GetOverriddenMember();
				}
				return (EventSymbol)OverriddenOrHiddenMembersResult.GetOverriddenMember(this, OriginalDefinition.OverriddenEvent);
			}
			return null;
		}
	}

	internal virtual OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers => this.MakeOverriddenOrHiddenMembers();

	internal bool HidesBaseEventsByName => (AddMethod ?? RemoveMethod)?.HidesBaseMethodsByName ?? false;

	internal virtual bool IsExplicitInterfaceImplementation => ExplicitInterfaceImplementations.Any();

	public abstract ImmutableArray<EventSymbol> ExplicitInterfaceImplementations { get; }

	public sealed override SymbolKind Kind => SymbolKind.Event;

	internal abstract bool MustCallMethodsDirectly { get; }

	protected override int HighestPriorityUseSiteError => 570;

	public sealed override bool HasUnsupportedMetadata
	{
		get
		{
			DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null)
			{
				return useSiteDiagnostic.Code == 570;
			}
			return false;
		}
	}

	public virtual bool IsTupleEvent => false;

	public virtual EventSymbol TupleUnderlyingEvent => null;

	ITypeSymbol IEventSymbol.Type => Type;

	IMethodSymbol IEventSymbol.AddMethod => AddMethod;

	IMethodSymbol IEventSymbol.RemoveMethod => RemoveMethod;

	IMethodSymbol IEventSymbol.RaiseMethod => null;

	IEventSymbol IEventSymbol.OriginalDefinition => OriginalDefinition;

	IEventSymbol IEventSymbol.OverriddenEvent => OverriddenEvent;

	ImmutableArray<IEventSymbol> IEventSymbol.ExplicitInterfaceImplementations => ExplicitInterfaceImplementations.Cast<EventSymbol, IEventSymbol>();

	IEnumerable<IMethodReference> IEventDefinition.GetAccessors(EmitContext context)
	{
		MethodSymbol addMethod = AddMethod;
		if (addMethod.ShouldInclude(context))
		{
			yield return addMethod;
		}
		MethodSymbol removeMethod = RemoveMethod;
		if (removeMethod.ShouldInclude(context))
		{
			yield return removeMethod;
		}
	}

	ITypeReference IEventDefinition.GetType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(Type, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		return ContainingType;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return this;
	}

	internal EventSymbol()
	{
	}

	public ImmutableArray<CSharpAttributeData> GetFieldAttributes()
	{
		if ((object)AssociatedField != null)
		{
			return AssociatedField.GetAttributes();
		}
		return ImmutableArray<CSharpAttributeData>.Empty;
	}

	internal EventSymbol GetLeastOverriddenEvent(NamedTypeSymbol accessingTypeOpt)
	{
		NamedTypeSymbol originalDefinition = (((object)accessingTypeOpt == null) ? ContainingType : accessingTypeOpt).OriginalDefinition;
		EventSymbol eventSymbol = this;
		while (eventSymbol.IsOverride && !eventSymbol.HidesBaseEventsByName)
		{
			EventSymbol overriddenEvent = eventSymbol.OverriddenEvent;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if ((object)overriddenEvent == null || !AccessCheck.IsSymbolAccessible(overriddenEvent, originalDefinition, ref useSiteDiagnostics))
			{
				break;
			}
			eventSymbol = overriddenEvent;
		}
		return eventSymbol;
	}

	internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEvent(this, argument);
	}

	public override void Accept(CSharpSymbolVisitor visitor)
	{
		visitor.VisitEvent(this);
	}

	public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
	{
		return visitor.VisitEvent(this);
	}

	internal EventSymbol AsMember(NamedTypeSymbol newOwner)
	{
		if (!(newOwner == ContainingSymbol))
		{
			return new SubstitutedEventSymbol(newOwner as SubstitutedNamedTypeSymbol, this);
		}
		return this;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (IsDefinition)
		{
			return base.GetUseSiteDiagnostic();
		}
		return OriginalDefinition.GetUseSiteDiagnostic();
	}

	internal bool CalculateUseSiteDiagnostic(ref DiagnosticInfo result)
	{
		if (DeriveUseSiteDiagnosticFromType(ref result, Type))
		{
			return true;
		}
		if (ContainingModule.HasUnifiedReferences)
		{
			HashSet<TypeSymbol> checkedTypes = null;
			if (Type.GetUnificationUseSiteDiagnosticRecursive(ref result, this, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitEvent(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitEvent(this);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is EventSymbol eventSymbol))
		{
			return false;
		}
		if ((object)this == eventSymbol)
		{
			return true;
		}
		if (ContainingType == eventSymbol.ContainingType)
		{
			return (object)OriginalDefinition == eventSymbol.OriginalDefinition;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int currentKey = 1;
		currentKey = Hash.Combine(ContainingType, currentKey);
		return Hash.Combine(Name, currentKey);
	}
}
