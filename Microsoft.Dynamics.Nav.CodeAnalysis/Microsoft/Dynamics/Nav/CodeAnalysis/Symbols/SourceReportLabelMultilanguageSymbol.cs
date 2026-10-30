using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceReportLabelMultilanguageSymbol : ReportLabelSymbol
{
	private readonly SyntaxToken identifierToken;

	private ImmutableArray<BoundMultilanguageExpression> lazyMultilanguagePropertyValue;

	private SymbolCompletionState state;

	public override bool IsMultilanguageReportLabel => true;

	public override ImmutableArray<Location> Locations { get; }

	public override string Name { get; }

	public override int Id { get; }

	public override Symbol ContainingSymbol { get; }

	private CommaSeparatedIdentifierEqualsStringListSyntax CaptionSyntax { get; }

	public ImmutableArray<BoundMultilanguageExpression> Captions
	{
		get
		{
			LazyGetLabelValue();
			return lazyMultilanguagePropertyValue;
		}
	}

	private ReportLabelMultilanguageSyntax ReportLabelSyntax { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			SyntaxNode parent = identifierToken.Parent.Parent;
			DebugAssertHelper.Assert(parent is ReportLabelBaseSyntax);
			return ImmutableArray.Create(parent.GetReference());
		}
	}

	private SourceReportLabelMultilanguageSymbol(Symbol containingSymbol, ReportLabelMultilanguageSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		ReportLabelSyntax = syntax;
		identifierToken = identifier;
		Name = name;
		Id = IdSpace.GetMemberId(Name);
		Locations = locations;
		CaptionSyntax = syntax.Caption;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public static SourceReportLabelMultilanguageSymbol Create(Symbol owner, ReportLabelMultilanguageSyntax syntax)
	{
		string name = syntax.Name.Unquoted();
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceReportLabelMultilanguageSymbol(owner, syntax, syntax.Name.Identifier, name, locations);
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitReportLabel(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabel(this);
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
				LazyGetLabelValue();
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
				CheckLabel();
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion(ref state);
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)73719);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitReportLabel(this, a);
	}

	private void CheckLabel()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		lock (ReportLabelSyntax.Parent)
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
					instance.Add(ErrorCode.ERR_NameIsNotValidClsIdentifier, ReportLabelSyntax.Name.GetLocation(), Name);
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

	private void LazyGetLabelValue()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		lock (identifierToken.Parent)
		{
			if (!state.HasComplete(CompletionPart.ReturnType))
			{
				LazySetSymbol((DiagnosticBag diagnostics) =>
				{
					ImmutableArray<BoundMultilanguageExpression> immutableArray = Binder.BindMultiLanguageValue(CaptionSyntax, DeclaringCompilation.Options.CompilerFeatures, diagnostics, out var hasErrors);
					lazyMultilanguagePropertyValue = ((!hasErrors) ? immutableArray : ImmutableArray<BoundMultilanguageExpression>.Empty);
				}, CompletionPart.ReturnType, ref state);
			}
		}
	}
}
