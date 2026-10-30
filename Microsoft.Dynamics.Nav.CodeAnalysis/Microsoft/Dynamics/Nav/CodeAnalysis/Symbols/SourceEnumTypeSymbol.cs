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
internal class SourceEnumTypeSymbol : EnumTypeSymbol
{
	private SymbolCompletionState state;

	private ImmutableArray<PropertySymbol> lazyProperties;

	private ImmutableArray<EnumValueSymbol> lazyEnumValues;

	internal EnumTypeSyntax Syntax { get; }

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

	public static SourceEnumTypeSymbol Create(ModuleSymbol owner, EnumTypeSyntax syntax)
	{
		return new SourceEnumTypeSymbol(owner, syntax);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		LazyGetEnumValues();
		return StaticCast<Symbol>.From(lazyEnumValues);
	}

	private SourceEnumTypeSymbol(ModuleSymbol owner, EnumTypeSyntax syntax)
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
				state.NotePartComplete((CompletionPart)69615);
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
		if (!(lazyEnumValues != null))
		{
			ArrayBuilder<EnumValueSymbol> instance = ArrayBuilder<EnumValueSymbol>.GetInstance();
			int num = 0;
			for (int i = 0; i < Syntax.Values.Count; i++)
			{
				EnumValueSyntax enumValueSyntax = Syntax.Values[i];
				num = ((enumValueSyntax.Id.Value != null) ? ((int)enumValueSyntax.Id.Value) : num);
				instance.Add(SourceEnumValueSymbol.Create(this, enumValueSyntax, num++));
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyEnumValues, instance.ToImmutableAndFree(), default);
		}
	}

	private void CheckEnumValues()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (Syntax.Parent)
		{
			if (state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				try
				{
					ObjectMembers<EnumTypeSymbol>.CheckMembersForDuplicates(GetMembers(), instance, SymbolKind.EnumValue, ErrorCode.ERR_DuplicateMemberName, ErrorCode.ERR_DuplicateMemberId);
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

	internal override ICodeEmitter<MemberDeclarationSyntax> GetEmitter(ModuleBuilder moduleBuilder)
	{
		return null;
	}

	internal override ObjectMetadataEmitter GetMetadataEmitter(ModuleBuilder moduleBuilder)
	{
		return new EnumTypeMetadataEmitter(this, moduleBuilder);
	}
}
