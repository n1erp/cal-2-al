using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SourceEnumValueSymbol : EnumValueSymbol
{
	private readonly EnumValueSyntax enumValueSyntax;

	private SymbolCompletionState state;

	private ImmutableArray<PropertySymbol> lazyProperties;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override int Ordinal { get; }

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(enumValueSyntax.Parent.GetReference());

	internal override bool RequiresCompletion { get; } = true;

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return lazyProperties;
		}
	}

	public static SourceEnumValueSymbol Create(Symbol owner, EnumValueSyntax enumValueSyntax, int ordinal)
	{
		ImmutableArray<Location> locations = ImmutableArray.Create(enumValueSyntax.Name?.Location ?? enumValueSyntax.Location);
		return new SourceEnumValueSymbol(owner, ordinal, enumValueSyntax, locations);
	}

	private SourceEnumValueSymbol(Symbol containingSymbol, int ordinal, EnumValueSyntax enumValueSyntax, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		Name = enumValueSyntax.GetNameStringValue();
		Ordinal = ordinal;
		this.enumValueSyntax = enumValueSyntax;
		Type = containingSymbol.GetBaseApplicationObjectSymbol();
		Locations = locations;
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
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)131055);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
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
			ImmutableArray<PropertySymbol> value = enumValueSyntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyProperties, value, default);
		}
	}
}
