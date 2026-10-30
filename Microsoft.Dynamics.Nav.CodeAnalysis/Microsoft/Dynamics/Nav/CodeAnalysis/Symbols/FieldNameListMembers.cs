using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class FieldNameListMembers
{
	private readonly FieldNameListSymbol containingSymbol;

	private readonly KeyFieldGroupBaseSyntax fieldNameListSyntax;

	private readonly Action<FieldSymbol, IdentifierNameSyntax, ApplicationObjectTypeSymbol, DiagnosticBag> validateFieldMember;

	private readonly Action<KeyFieldGroupBaseSyntax, DiagnosticBag> validateDeclaration;

	private SymbolCompletionState state;

	private ImmutableArray<PropertySymbol> lazyProperties;

	private ImmutableArray<FieldSymbol> fields;

	public ImmutableArray<PropertySymbol> Properties
	{
		get
		{
			LazyGetProperties();
			return lazyProperties;
		}
	}

	public ImmutableArray<FieldSymbol> Fields
	{
		get
		{
			LazyGetFields();
			return fields;
		}
	}

	public FieldNameListMembers(FieldNameListSymbol containingSymbol, KeyFieldGroupBaseSyntax fieldNameListSyntax, Action<FieldSymbol, IdentifierNameSyntax, ApplicationObjectTypeSymbol, DiagnosticBag> validateFieldMember = null, Action<KeyFieldGroupBaseSyntax, DiagnosticBag> validateDeclaration = null)
	{
		this.containingSymbol = containingSymbol;
		this.fieldNameListSyntax = fieldNameListSyntax;
		this.validateFieldMember = validateFieldMember;
		this.validateDeclaration = validateDeclaration;
	}

	internal bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	internal void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
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
			case CompletionPart.FieldNameListSymbol:
				ForceCompleteFields();
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.FieldNameListSymbol));
				break;
			case CompletionPart.SymbolCompleted:
				NotifyCompilationAboutSymbolCompletion();
				break;
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)97263);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
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

	private void ForceCompleteFields()
	{
		LazyGetFields();
		state.NotePartComplete(CompletionPart.FieldNameListSymbol);
	}

	private void LazyGetProperties()
	{
		if (!(lazyProperties != null))
		{
			ImmutableArray<PropertySymbol> value = fieldNameListSyntax.PropertyList?.Properties.CreatePropertySymbols(containingSymbol) ?? ImmutableArray<PropertySymbol>.Empty;
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyProperties, value, default);
		}
	}

	private void LazyGetFields()
	{
		if (state.HasComplete(CompletionPart.FieldNameListSymbol))
		{
			return;
		}
		lock (fieldNameListSyntax.Parent)
		{
			LazySetSymbol(CreateAndValidateFieldReferences, CompletionPart.FieldNameListSymbol);
		}
	}

	private void CreateAndValidateFieldReferences(DiagnosticBag diagnostics)
	{
		validateDeclaration?.Invoke(fieldNameListSyntax, diagnostics);
		ApplicationObjectTypeSymbol containingSymbolOfKind = containingSymbol.GetContainingSymbolOfKind<ApplicationObjectTypeSymbol>(SymbolKind.Table, SymbolKind.TableExtension);
		if (containingSymbolOfKind == null || (containingSymbolOfKind.Kind != SymbolKind.Table && containingSymbolOfKind.Kind != SymbolKind.TableExtension))
		{
			fields = ImmutableArray<FieldSymbol>.Empty;
			return;
		}
		fields = CreateAndValidateFieldReferences(containingSymbolOfKind, diagnostics);
		if (!fields.Any())
		{
			diagnostics.Add(ErrorCode.ERR_FieldListCannotBeEmpty, fieldNameListSyntax.Name.Location);
		}
	}

	private ImmutableArray<FieldSymbol> CreateAndValidateFieldReferences(ApplicationObjectTypeSymbol applicationObject, DiagnosticBag d)
	{
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(fieldNameListSyntax.Fields.Count);
		try
		{
			foreach (IdentifierNameSyntax field in fieldNameListSyntax.Fields)
			{
				FieldSymbol fieldSymbol = CreateAndValidateFieldReference(applicationObject, field, d);
				if (fieldSymbol != null)
				{
					instance.Add(fieldSymbol);
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	private FieldSymbol CreateAndValidateFieldReference(ApplicationObjectTypeSymbol applicationObject, IdentifierNameSyntax name, DiagnosticBag diagnostics)
	{
		if (name.IsMissing || string.IsNullOrEmpty(name.Identifier.ValueText))
		{
			return null;
		}
		FieldSymbol fieldSymbol = applicationObject.GetMembers(name.Identifier.ValueText.UnquoteIdentifier()).FirstOrDefault() as FieldSymbol;
		if (fieldSymbol == null)
		{
			diagnostics.Add(ErrorCode.ERR_MissingTypeReference, name.Location, name, applicationObject.Name);
			return null;
		}
		validateFieldMember?.Invoke(fieldSymbol, name, applicationObject, diagnostics);
		return fieldSymbol;
	}

	private void LazySetSymbol(Action<DiagnosticBag> setSymbol, CompletionPart completionValue)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			setSymbol(instance);
			containingSymbol.AddDeclarationDiagnostics(instance);
		}
		finally
		{
			state.NotePartComplete(completionValue);
			instance.Free();
		}
	}

	protected void NotifyCompilationAboutSymbolCompletion()
	{
		try
		{
			containingSymbol.DeclaringCompilation.SymbolDeclaredEvent(containingSymbol);
		}
		finally
		{
			state.NotePartComplete(CompletionPart.SymbolCompleted);
		}
	}
}
