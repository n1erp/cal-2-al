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
internal sealed class SourceFieldSymbol : FieldSymbol
{
	private SymbolCompletionState state;

	private TypeSymbol lazyTypeSymbol;

	private readonly SyntaxToken identifierToken;

	private ImmutableArray<PropertySymbol> properties;

	private ImmutableArray<MethodSymbol> triggers;

	private const int MissingFieldId = -1;

	private const int ReservedFieldId = 0;

	internal FieldSyntax DeclaringSyntax { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public override int Id { get; }

	public override TypeSymbol Type
	{
		get
		{
			LazyGetType();
			return lazyTypeSymbol;
		}
	}

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

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(identifierToken.Parent.Parent.GetReference());

	internal override bool RequiresCompletion => true;

	public static SourceFieldSymbol Create(Symbol owner, FieldSyntax syntax)
	{
		string valueText = syntax.Name.Identifier.ValueText;
		int id = ((syntax.No.IsKind(SyntaxKind.Int32LiteralToken) && syntax.No.Value != null) ? ((int)syntax.No.Value) : (-1));
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceFieldSymbol(owner, syntax, syntax.Name.Identifier, valueText, id, locations);
	}

	private SourceFieldSymbol(Symbol containingSymbol, FieldSyntax syntax, SyntaxToken identifier, string name, int id, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		DeclaringSyntax = syntax;
		identifierToken = identifier;
		Name = name.UnquoteIdentifier();
		MetadataName = name.MangleIdentifierName();
		Id = id;
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
			case CompletionPart.ReturnType:
				LazyGetType();
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
				CheckField();
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
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			TypeSymbol typeSymbol = ContainingSymbol.DeclaringCompilation.GetBinderFactory(DeclaringSyntax.SyntaxTree).GetBinder(DeclaringSyntax.Type.Parent).BindFieldType(this, DeclaringSyntax, instance);
			if (Interlocked.CompareExchange(ref lazyTypeSymbol, typeSymbol, null) != typeSymbol)
			{
				state.NotePartComplete(CompletionPart.ReturnType);
				AddDeclarationDiagnostics(instance);
			}
		}
		finally
		{
			instance.Free();
		}
		state.SpinWaitComplete(CompletionPart.ReturnType, CancellationToken.None);
	}

	private void LazyGetProperties()
	{
		if (!(properties != null))
		{
			ImmutableArray<PropertySymbol> value = DeclaringSyntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref properties, value, default);
		}
	}

	private void LazyGetTriggers()
	{
		if (!(triggers != null))
		{
			ImmutableArray<MethodSymbol> value = ((IEnumerable<MethodSymbol>)DeclaringSyntax.Triggers.Select((TriggerDeclarationSyntax trigger) => SourceTriggerSymbol.Create(this, trigger))).ToImmutableArray();
			ImmutableInterlocked.InterlockedCompareExchange(ref triggers, value, default);
		}
	}

	private void CheckField()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (DeclaringSyntax.Parent)
		{
			if (!state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				return;
			}
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				TriggerValidationHelper.CheckTriggers(this, Triggers, GetTriggerTypeInfo, instance);
				if (Id == 0)
				{
					instance.Add(ErrorCode.ERR_InvalidSymbolId, DeclaringSyntax.No.GetLocation(), Id);
				}
				if (Name.Length > 30)
				{
					instance.Add(ErrorCode.WRN_ERR_FieldNameIdentifierTooLong, this.GetLocation(), 30);
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
		return Triggers.CastArray<Symbol>().Concat(Properties.CastArray<Symbol>());
	}
}
