using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class SourceQueryFilterSymbol : QueryFilterSymbol
{
	private readonly SourceQueryDataItemSymbol containingSymbol;

	private readonly SyntaxToken identifierToken;

	private ImmutableArray<PropertySymbol> properties;

	private FieldSymbol relatedField;

	private SymbolCompletionState state;

	public override FieldSymbol RelatedField
	{
		get
		{
			LazyGetRelatedField();
			return relatedField;
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

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(identifierToken.Parent.Parent.GetReference());

	public override Symbol ContainingSymbol => containingSymbol;

	public override string Name { get; }

	public override string MetadataName { get; }

	public override int Id { get; }

	internal override bool RequiresCompletion => true;

	private QueryFilterSyntax Syntax { get; }

	public static SourceQueryFilterSymbol Create(SourceQueryDataItemSymbol containingSymbol, QueryFilterSyntax syntax)
	{
		string name = syntax.Name.Unquoted();
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceQueryFilterSymbol(containingSymbol, syntax, syntax.Name.Identifier, name, locations);
	}

	[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
	public SourceQueryFilterSymbol(SourceQueryDataItemSymbol containingSymbol, QueryFilterSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
	{
		this.containingSymbol = containingSymbol;
		Syntax = syntax;
		identifierToken = identifier;
		Name = name.UnquoteIdentifier();
		MetadataName = name.MangleIdentifierName();
		Id = IdSpace.GetMemberId(Name);
		Locations = locations;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return Properties.CastArray<Symbol>();
	}

	internal override bool HasComplete(CompletionPart part)
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
				LazyGetRelatedField();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.ReturnType));
				break;
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckFilter();
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)73703);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void LazyGetRelatedField()
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
				if (Syntax?.RelatedField != null)
				{
					relatedField = GetRelatedFieldSymbol(diagnostics);
				}
			}, CompletionPart.ReturnType, ref state);
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

	private void LazyGetProperties()
	{
		if (!(properties != null))
		{
			ImmutableArray<PropertySymbol> value = Syntax.PropertyList?.Properties.CreatePropertySymbols(this) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref properties, value, default);
		}
	}

	private void CheckFilter()
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
				if (!Name.IsValidClsIdentifier())
				{
					instance.Add(ErrorCode.ERR_NameIsNotValidClsIdentifier, Syntax.Name.GetLocation(), Name);
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

	private FieldSymbol GetRelatedFieldSymbol(DiagnosticBag diagnostics)
	{
		Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(Syntax);
		string identifierOrLiteralValue = Syntax.RelatedField.GetIdentifierOrLiteralValue();
		if (string.IsNullOrEmpty(identifierOrLiteralValue))
		{
			diagnostics.Add(ErrorCode.ERR_QueryElementSourceMustBeFieldOnTable, Syntax.RelatedField.GetLocation());
			return null;
		}
		identifierOrLiteralValue = identifierOrLiteralValue.UnquoteIdentifier();
		FieldSymbol fieldSymbol = (from symbol in binder.FindSymbols(identifierOrLiteralValue, LookupOptions.MemberAccess)
			where symbol.Kind == SymbolKind.Field
			select symbol).Cast<FieldSymbol>().FirstIfSingle();
		if (fieldSymbol == null)
		{
			diagnostics.Add(ErrorCode.ERR_QueryElementSourceMustBeFieldOnTable, Syntax.RelatedField.GetLocation());
			return null;
		}
		return fieldSymbol;
	}
}
