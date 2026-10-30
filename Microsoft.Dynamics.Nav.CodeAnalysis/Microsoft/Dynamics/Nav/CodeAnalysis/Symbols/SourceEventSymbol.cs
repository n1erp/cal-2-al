using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceEventSymbol : EventSymbol
{
	private ImmutableArray<ParameterSymbol> lazyParameters;

	private ImmutableArray<AttributeSymbol> lazyAttributes;

	private SymbolCompletionState state;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override string MetadataName { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(Syntax.GetReference());

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			CheckEvent();
			if (!lazyParameters.IsDefault)
			{
				return lazyParameters;
			}
			return ImmutableArray<ParameterSymbol>.Empty;
		}
	}

	public override ImmutableArray<AttributeSymbol> Attributes
	{
		get
		{
			CheckEvent();
			if (!lazyAttributes.IsDefault)
			{
				return ImmutableArray<AttributeSymbol>.CastUp(lazyAttributes);
			}
			return ImmutableArray<AttributeSymbol>.Empty;
		}
	}

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	internal EventDeclarationSyntax Syntax { get; }

	internal sealed override bool RequiresCompletion => true;

	public SourceEventSymbol(ObjectTypeSymbol containingSymbol, EventDeclarationSyntax eventDeclarationSyntax)
	{
		ContainingSymbol = containingSymbol;
		Name = eventDeclarationSyntax.Name.Unquoted();
		MetadataName = eventDeclarationSyntax.Name.Identifier.Text.MangleIdentifierName();
		Locations = ImmutableArray.Create((Location)new SourceLocation(eventDeclarationSyntax.Name.Identifier));
		Syntax = eventDeclarationSyntax;
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
			case CompletionPart.Parameters:
				foreach (ParameterSymbol parameter in Parameters)
				{
					parameter.ForceComplete(locationOpt, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.Parameters);
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckEvent();
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)73723);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void CheckEvent()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (Syntax)
		{
			if (state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				LazySetSymbol((DiagnosticBag diagnostics) =>
				{
					Binder binder = DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(Syntax);
					lazyParameters = ParameterHelpers.MakeParameters(binder, this, Syntax.ParameterList.Parameters, diagnostics);
					CheckNameConflicts(diagnostics);
				}, CompletionPart.FinishMethodChecks, ref state);
			}
		}
	}

	private void CheckNameConflicts(DiagnosticBag diagnostics)
	{
		if (Parameters.Length == 0)
		{
			return;
		}
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		if (!Parameters.IsDefaultOrEmpty)
		{
			foreach (ParameterSymbol parameter in Parameters)
			{
				ValidateName(parameter, instance, diagnostics, ErrorCode.ERR_DuplicateParamName);
			}
		}
		instance.Free();
	}

	private static void ValidateName(Symbol symbol, PooledNameComparisonHashSet names, DiagnosticBag diagnostics, ErrorCode errorCode)
	{
		string name = symbol.Name;
		if (name != null)
		{
			if (names.Contains(name))
			{
				diagnostics.Add(errorCode, symbol.GetLocation(), name);
			}
			else
			{
				names.Add(name);
			}
		}
	}
}
