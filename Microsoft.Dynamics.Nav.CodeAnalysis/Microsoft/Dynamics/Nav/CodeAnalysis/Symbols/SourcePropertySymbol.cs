using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SourcePropertySymbol : PropertySymbol
{
	private readonly string name;

	private BoundProperty lazyProperty;

	private SymbolCompletionState state;

	private readonly object mutex = new object();

	internal PropertySyntax PropertySyntax { get; }

	public SyntaxNode Syntax => PropertySyntax;

	public override Symbol ContainingSymbol { get; }

	public override string Name => name;

	public override PropertyKind PropertyKind { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	internal override BoundProperty Property
	{
		get
		{
			LazyGetProperty();
			return lazyProperty;
		}
	}

	internal override object Value
	{
		get
		{
			if (Property == null || Property.PropertyValue == null)
			{
				return null;
			}
			return Property.PropertyValue.ValueObject;
		}
	}

	public override string ValueText
	{
		get
		{
			if (Property == null || string.IsNullOrEmpty(Property.PropertyValue?.ValueText))
			{
				return string.Empty;
			}
			return Property.PropertyValue.ValueText;
		}
	}

	public static SourcePropertySymbol Create(Symbol owner, PropertySyntax syntax)
	{
		DebugAssertHelper.Assert(syntax.Name != null);
		ImmutableArray<Location> locations = ImmutableArray.Create(syntax.Location);
		return new SourcePropertySymbol(owner, syntax, locations);
	}

	private SourcePropertySymbol(Symbol containingSymbol, PropertySyntax syntax, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		PropertySyntax = syntax;
		string valueText = syntax.Name.Identifier.ValueText;
		name = valueText;
		Locations = locations;
		DeclaringSyntaxReferences = ImmutableArray.Create(PropertySyntax.GetReference());
		PropertyKind = SyntaxFacts.GetPropertyKind(name);
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
			case CompletionPart.ReturnType:
				LazyGetProperty();
				break;
			case CompletionPart.Variables:
				CheckProperties();
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

	private void LazyGetProperty()
	{
		if (state.HasComplete(CompletionPart.ReturnType))
		{
			return;
		}
		lock (mutex)
		{
			if (state.NotePartComplete(CompletionPart.Parameters))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				try
				{
					Binder binder = GetBinder();
					GlobalLogger.LogVerbose("Binding property '{0}' declared on '{1}'.", Syntax, ContainingSymbol);
					lazyProperty = binder.BindProperty(PropertySyntax, instance);
					binder.ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(instance, this, PropertySyntax.Name);
					AddDeclarationDiagnostics(instance);
					return;
				}
				finally
				{
					instance.Free();
					state.NotePartComplete(CompletionPart.ReturnType);
				}
			}
		}
	}

	private Binder GetBinder()
	{
		BinderFactory binderFactory = ContainingSymbol.DeclaringCompilation.GetBinderFactory(Syntax.SyntaxTree);
		if (PropertySyntax.Value != null)
		{
			SyntaxKind kind = PropertySyntax.Value.Kind;
			if (kind == SyntaxKind.QualifiedObjectReferencePropertyValue)
			{
				return binderFactory.GetBinder(Syntax.Parent);
			}
		}
		return binderFactory.GetBinder(Syntax.Parent, Syntax.SpanStart);
	}

	private void CheckProperties()
	{
		if (state.HasComplete(CompletionPart.Variables) || Property == null || Property.HasErrors)
		{
			return;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			CompilationTarget target = DeclaringCompilation.Options.Target;
			if (!this.CanCompileWithTarget(target))
			{
				this.CreateInsufficientScopeDiagnostic(target, instance);
			}
			AddDeclarationDiagnostics(instance);
		}
		finally
		{
			instance.Free();
		}
	}
}
