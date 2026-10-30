using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal abstract class SourceVariableSymbol : VariableSymbol
{
	private SymbolCompletionState state;

	private TypeSymbol lazyTypeSymbol;

	private readonly VariableDeclarationSyntax syntax;

	private readonly SyntaxToken identifierToken;

	private readonly ImmutableArray<Location> locations;

	private ImmutableArray<AttributeSymbol> lazyAttributes;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public override ImmutableArray<AttributeSymbol> Attributes
	{
		get
		{
			LazyGetAttributes();
			return lazyAttributes;
		}
	}

	public override TypeSymbol Type
	{
		get
		{
			LazyGetType();
			return lazyTypeSymbol;
		}
	}

	internal SyntaxToken IdentifierToken => identifierToken;

	public override ImmutableArray<Location> Locations => locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(identifierToken.Parent.Parent.GetReference());

	internal override bool RequiresCompletion => true;

	protected internal SourceVariableSymbol(Symbol containingSymbol, VariableDeclarationSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		this.syntax = syntax;
		identifierToken = identifier;
		Name = name.UnquoteIdentifier();
		MetadataName = name.GetLowercasedMetadataName();
		this.locations = locations;
	}

	protected void LazyGetType()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		lock (identifierToken.Parent)
		{
			if (state.NotePartComplete(CompletionPart.Parameters))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				try
				{
					Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree).GetBinder(syntax.Type.Parent);
					lazyTypeSymbol = binder.BindVariableType(ContainingSymbol, syntax.Type, instance);
					AddDeclarationDiagnostics(instance);
					return;
				}
				finally
				{
					state.NotePartComplete(CompletionPart.ReturnType);
					instance.Free();
				}
			}
		}
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
			case CompletionPart.Attributes:
				ForceCompleteAttributes(locationOpt, cancellationToken);
				break;
			case CompletionPart.Parameters:
			case CompletionPart.ReturnType:
				LazyGetType();
				break;
			case CompletionPart.Variables:
				VariableCheck();
				state.NotePartComplete(CompletionPart.Variables);
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)98275);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	protected virtual void VariableCheck()
	{
	}

	private void LazyGetAttributes()
	{
		if (!(lazyAttributes != null))
		{
			ImmutableArray<AttributeSymbol> value = ((IEnumerable<AttributeSymbol>)syntax.Attributes.Select((MemberAttributeSyntax attribute) => SourceAttributeSymbol.Create(this, attribute))).ToImmutableArray();
			ImmutableInterlocked.InterlockedInitialize(ref lazyAttributes, value);
		}
	}

	private void ForceCompleteAttributes(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		if (state.HasComplete(CompletionPart.Attributes))
		{
			return;
		}
		try
		{
			LazyGetAttributes();
			foreach (AttributeSymbol lazyAttribute in lazyAttributes)
			{
				cancellationToken.ThrowIfCancellationRequested();
				lazyAttribute.ForceComplete(locationOpt, cancellationToken);
			}
		}
		finally
		{
			state.NotePartComplete(CompletionPart.Attributes);
		}
	}
}
