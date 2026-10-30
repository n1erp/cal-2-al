using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class SourceEnumExtensionTypeSymbol : EnumExtensionTypeSymbol
{
	private SymbolCompletionState state;

	private ImmutableArray<PropertySymbol> lazyProperties;

	private ImmutableArray<EnumValueSymbol> lazyEnumValues;

	private EnumTypeSymbol lazyTarget;

	internal EnumExtensionTypeSyntax Syntax { get; }

	public override int Id { get; }

	public override string Name { get; }

	internal override bool RequiresCompletion { get; } = true;

	public override Symbol ContainingSymbol { get; }

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

	public override ImmutableArray<EnumValueSymbol> Values
	{
		get
		{
			LazyGetEnumValues();
			return lazyEnumValues;
		}
	}

	internal override ApplicationObjectTypeSymbol Target
	{
		get
		{
			LazyGetTarget();
			return lazyTarget;
		}
	}

	public static SourceEnumExtensionTypeSymbol Create(ModuleSymbol owner, EnumExtensionTypeSyntax syntax)
	{
		return new SourceEnumExtensionTypeSymbol(owner, syntax);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		LazyGetEnumValues();
		return StaticCast<Symbol>.From(lazyEnumValues);
	}

	private SourceEnumExtensionTypeSymbol(ModuleSymbol owner, EnumExtensionTypeSyntax syntax)
		: base(owner)
	{
		ContainingSymbol = owner;
		Id = syntax.ObjectId.GetObjectIdOrDefault();
		Name = ((syntax.Name != null) ? syntax.Name.Identifier.ValueText.UnquoteIdentifier() : string.Empty);
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
		Locations = ImmutableArray.Create(syntax.Name?.Location ?? syntax.Location);
		Syntax = syntax;
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
				break;
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.EnumValues:
				ForceCompleteEnumValues(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.EnumValues));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckEnumValues();
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)69607);
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
			ImmutableArray<PropertySymbol> value = Syntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyProperties, value, default);
		}
	}

	private void ForceCompleteEnumValues(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetEnumValues();
		foreach (EnumValueSymbol lazyEnumValue in lazyEnumValues)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lazyEnumValue.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.EnumValues);
	}

	private void LazyGetEnumValues()
	{
		if (lazyEnumValues != null)
		{
			return;
		}
		ArrayBuilder<EnumValueSymbol> instance = ArrayBuilder<EnumValueSymbol>.GetInstance();
		int num = 0;
		for (int i = 0; i < Syntax.Values.Count; i++)
		{
			EnumValueSyntax enumValueSyntax = Syntax.Values[i];
			if (enumValueSyntax.Id.IsKind(SyntaxKind.Int32LiteralToken) && enumValueSyntax.Id.Value is int num2)
			{
				num = num2;
			}
			instance.Add(SourceEnumValueSymbol.Create(this, enumValueSyntax, num++));
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyEnumValues, instance.ToImmutableAndFree(), default);
	}

	private void CheckEnumValues()
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
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				if (Target != null && Target.GetSimplePropertyValue<bool>(PropertyKind.Extensible) != true)
				{
					instance.Add(ErrorCode.ERR_EnumNotExtensible, Syntax.BaseObject.GetLocation(), Target.Name);
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
				if (Syntax.BaseObject != null)
				{
					Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(Syntax.BaseObject);
					lazyTarget = binder.GetApplicationObjectTypeSymbol(SymbolKind.Enum, Syntax.BaseObject, d) as EnumTypeSymbol;
					if (lazyTarget != null)
					{
						binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(d, lazyTarget, Syntax.BaseObject);
					}
					else
					{
						d.Add(ErrorCode.ERR_ExtensionTargetNotFound, Syntax.BaseObject.Location, SymbolKind.Enum, Syntax.BaseObject.Identifier);
					}
				}
			}, CompletionPart.ReturnType, ref state);
		}
	}

	internal override ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return null;
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new EnumExtensionTypeMetadataEmitter(this, moduleBuilder);
	}
}
