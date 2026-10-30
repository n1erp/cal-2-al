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
internal sealed class SourceReportColumnSymbol : ReportColumnSymbol
{
	private readonly SyntaxToken identifierToken;

	private BoundExpression lazyBoundSourceExpression;

	private ImmutableArray<PropertySymbol> properties;

	private SymbolCompletionState state;

	public override int Id { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return properties;
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

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			SyntaxNode parent = identifierToken.Parent.Parent;
			DebugAssertHelper.Assert(parent is ReportColumnSyntax);
			return ImmutableArray.Create(parent.GetReference());
		}
	}

	public BoundExpression SourceExpression
	{
		get
		{
			LazyGetType();
			return lazyBoundSourceExpression;
		}
	}

	internal ReportColumnSyntax Syntax { get; }

	internal override bool RequiresCompletion => true;

	public static SourceReportColumnSymbol Create(Symbol owner, ReportColumnSyntax syntax)
	{
		string name = syntax.Name.Unquoted();
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceReportColumnSymbol(owner, syntax, syntax.Name.Identifier, name, locations);
	}

	private SourceReportColumnSymbol(Symbol containingSymbol, ReportColumnSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
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
				LazyGetType();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.ReturnType));
				break;
			case CompletionPart.Variables:
				ForceCompleteProperties(locationOpt, cancellationToken);
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Variables));
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckColumn();
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
				if (Syntax?.SourceExpression != null)
				{
					Binder binder = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree).GetBinder(Syntax.SourceExpression);
					lazyBoundSourceExpression = binder.BindExpression(Syntax.SourceExpression, diagnostics);
				}
			}, CompletionPart.ReturnType, ref state);
		}
	}

	private void CheckColumn()
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
				if (lazyBoundSourceExpression != null && !lazyBoundSourceExpression.HasAnyErrors)
				{
					if (!SemanticFacts.IsValidColumnType(Type))
					{
						instance.Add(ErrorCode.ERR_InvalidColumnType, lazyBoundSourceExpression.Syntax.Location, lazyBoundSourceExpression.Syntax);
					}
					CheckColumnIncludeCaptionProperty(instance);
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

	private void CheckColumnIncludeCaptionProperty(DiagnosticBag diagnostics)
	{
		if (lazyBoundSourceExpression.Kind != BoundKind.FieldAccess)
		{
			PropertySymbol symbol = Properties.FirstOrDefault((PropertySymbol prop) => prop.PropertyKind == PropertyKind.IncludeCaption);
			if (this.GetBooleanPropertyValue(PropertyKind.IncludeCaption) ?? false)
			{
				diagnostics.Add(ErrorCode.ERR_IncludeCaptionCannotBeSetForNonField, symbol.GetLocation());
			}
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
}
