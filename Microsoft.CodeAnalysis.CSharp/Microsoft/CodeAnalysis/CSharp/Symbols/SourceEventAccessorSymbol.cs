using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceEventAccessorSymbol : SourceMemberMethodSymbol
{
	private readonly SourceEventSymbol _event;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	private TypeSymbol _lazyReturnType;

	public SourceEventSymbol AssociatedEvent => _event;

	public sealed override Symbol AssociatedSymbol => _event;

	public sealed override bool ReturnsVoid
	{
		get
		{
			LazyMethodChecks();
			return base.ReturnsVoid;
		}
	}

	public override RefKind RefKind => RefKind.None;

	public sealed override TypeSymbol ReturnType
	{
		get
		{
			LazyMethodChecks();
			return _lazyReturnType;
		}
	}

	public sealed override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public sealed override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
			return _lazyParameters;
		}
	}

	public sealed override bool IsVararg => false;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public sealed override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses => ImmutableArray<TypeParameterConstraintClause>.Empty;

	internal Location Location => Locations[0];

	internal override bool IsExpressionBodied => false;

	public SourceEventAccessorSymbol(SourceEventSymbol @event, SyntaxReference syntaxReference, ImmutableArray<Location> locations)
		: base(@event.containingType, syntaxReference, locations)
	{
		_event = @event;
	}

	protected sealed override void MethodChecks(DiagnosticBag diagnostics)
	{
		if ((object)_lazyReturnType != null)
		{
			return;
		}
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (_event.IsWindowsRuntimeEvent)
		{
			TypeSymbol wellKnownType = declaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationToken);
			Binder.ReportUseSiteDiagnostics(wellKnownType, diagnostics, Location);
			if (MethodKind == MethodKind.EventAdd)
			{
				_lazyReturnType = wellKnownType;
				SynthesizedAccessorValueParameterSymbol item = new SynthesizedAccessorValueParameterSymbol(this, _event.Type, 0, ImmutableArray<CustomModifier>.Empty);
				_lazyParameters = ImmutableArray.Create((ParameterSymbol)item);
				return;
			}
			TypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Void);
			Binder.ReportUseSiteDiagnostics(specialType, diagnostics, Location);
			_lazyReturnType = specialType;
			SetReturnsVoid(returnsVoid: true);
			SynthesizedAccessorValueParameterSymbol item2 = new SynthesizedAccessorValueParameterSymbol(this, wellKnownType, 0, ImmutableArray<CustomModifier>.Empty);
			_lazyParameters = ImmutableArray.Create((ParameterSymbol)item2);
		}
		else
		{
			TypeSymbol specialType2 = declaringCompilation.GetSpecialType(SpecialType.System_Void);
			Binder.ReportUseSiteDiagnostics(specialType2, diagnostics, Location);
			_lazyReturnType = specialType2;
			SetReturnsVoid(returnsVoid: true);
			SynthesizedAccessorValueParameterSymbol item3 = new SynthesizedAccessorValueParameterSymbol(this, _event.Type, 0, ImmutableArray<CustomModifier>.Empty);
			_lazyParameters = ImmutableArray.Create((ParameterSymbol)item3);
		}
	}

	protected string GetOverriddenAccessorName(SourceEventSymbol @event, bool isAdder)
	{
		if (IsOverride)
		{
			EventSymbol overriddenEvent = @event.OverriddenEvent;
			if ((object)overriddenEvent != null)
			{
				return overriddenEvent.GetOwnOrInheritedAccessor(isAdder)?.Name;
			}
		}
		return null;
	}
}
