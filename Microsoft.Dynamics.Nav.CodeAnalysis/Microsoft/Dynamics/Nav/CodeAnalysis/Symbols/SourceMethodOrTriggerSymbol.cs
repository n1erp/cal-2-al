using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class SourceMethodOrTriggerSymbol : MethodSymbol
{
	private readonly Location location;

	private readonly SyntaxReference methodOrTriggerSyntaxReference;

	private readonly SyntaxReference bodyReference;

	private ImmutableArray<VariableSymbol> lazyLocalVariables;

	private ImmutableArray<ParameterSymbol> lazyParameters;

	private ReturnValueSymbol lazyReturnValue;

	private ImmutableArray<AttributeSymbol> lazyAttributes;

	private bool lazyIsTryFunction;

	protected SymbolCompletionState state;

	private ImmutableArray<Diagnostic> cachedDiagnostics;

	internal ImmutableArray<Diagnostic> Diagnostics => cachedDiagnostics;

	public override Symbol ContainingSymbol { get; }

	public override bool IsStatic => false;

	public override bool IsTryFunction
	{
		get
		{
			LazyMethodChecks();
			return lazyIsTryFunction;
		}
	}

	internal SyntaxNode BodySyntax
	{
		get
		{
			if (bodyReference != null)
			{
				return bodyReference.GetSyntax();
			}
			return null;
		}
	}

	internal SyntaxReference SyntaxRef => methodOrTriggerSyntaxReference;

	internal SyntaxNode SyntaxNode
	{
		get
		{
			if (methodOrTriggerSyntaxReference != null)
			{
				return methodOrTriggerSyntaxReference.GetSyntax();
			}
			return null;
		}
	}

	internal override int LocalVariableCount
	{
		get
		{
			if (lazyLocalVariables.IsDefault)
			{
				return GetSyntax().Variables.Variables.Count;
			}
			return lazyLocalVariables.Length;
		}
	}

	public override ImmutableArray<VariableSymbol> LocalVariables
	{
		get
		{
			LazyMethodChecks();
			return lazyLocalVariables;
		}
	}

	internal SyntaxTree SyntaxTree
	{
		get
		{
			if (methodOrTriggerSyntaxReference != null)
			{
				return methodOrTriggerSyntaxReference.SyntaxTree;
			}
			return null;
		}
	}

	public override ImmutableArray<Location> Locations => ImmutableArray.Create(location);

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(methodOrTriggerSyntaxReference);

	protected virtual object MethodChecksLockObject => methodOrTriggerSyntaxReference;

	internal sealed override bool RequiresCompletion => true;

	internal override int ParameterCount
	{
		get
		{
			if (lazyParameters.IsDefault)
			{
				return GetSyntax().ParameterList.Parameters.Count;
			}
			return lazyParameters.Length;
		}
	}

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyMethodChecks();
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
			LazyMethodChecks();
			return ImmutableArray<AttributeSymbol>.CastUp(lazyAttributes);
		}
	}

	public override ReturnValueSymbol ReturnValue
	{
		get
		{
			LazyMethodChecks();
			return lazyReturnValue;
		}
	}

	internal ImmutableArray<Diagnostic> SetDiagnostics(ImmutableArray<Diagnostic> newSet, out bool diagsWritten)
	{
		diagsWritten = ImmutableInterlocked.InterlockedInitialize(ref cachedDiagnostics, newSet);
		return cachedDiagnostics;
	}

	internal SourceMethodOrTriggerSymbol(Symbol containingSymbol, MethodOrTriggerDeclarationSyntax methodOrTriggerDeclarationSyntax)
	{
		ContainingSymbol = containingSymbol;
		location = new SourceLocation(methodOrTriggerDeclarationSyntax.Name.Identifier);
		methodOrTriggerSyntaxReference = methodOrTriggerDeclarationSyntax.GetReference();
		bodyReference = ((methodOrTriggerDeclarationSyntax.Body != null) ? methodOrTriggerDeclarationSyntax.Body.GetReference() : null);
	}

	internal MethodOrTriggerDeclarationSyntax GetSyntax()
	{
		DebugAssertHelper.Assert(methodOrTriggerSyntaxReference != null);
		return (MethodOrTriggerDeclarationSyntax)methodOrTriggerSyntaxReference.GetSyntax();
	}

	private void VariablesCheck()
	{
		bool flag = this.IsCodeunitOnRunTrigger() && ((SourceCodeunitTypeSymbol)ContainingSymbol).RelatedTable != null;
		if (GetSyntax().Variables == null && !flag)
		{
			lazyLocalVariables = ImmutableArray<VariableSymbol>.Empty;
			return;
		}
		ArrayBuilder<VariableSymbol> instance = ArrayBuilder<VariableSymbol>.GetInstance();
		if (flag)
		{
			TableTypeSymbol relatedTable = ((SourceCodeunitTypeSymbol)ContainingSymbol).RelatedTable;
			RecordTypeSymbol record = ContainingSymbol.ContainingModule.GetRecord(relatedTable, isTemporary: false);
			instance.Add(SynthesizedLocalVariableSymbol.Create(this, record, "Rec", hasImplicitWith: true));
		}
		VarSectionSyntax variables = GetSyntax().Variables;
		if (variables != null)
		{
			foreach (VariableDeclarationSyntax variable in variables.Variables)
			{
				DebugAssertHelper.Assert(variable.Type != null);
				SourceLocalVariableSymbol item = SourceLocalVariableSymbol.Create(this, variable);
				instance.Add(item);
			}
		}
		lazyLocalVariables = instance.ToImmutableAndFree();
	}

	private void NameConflictCheck(DiagnosticBag diagnostics)
	{
		if (ParameterCount == 0 && LocalVariableCount == 0 && ReturnValue == null)
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
		if (ReturnValue != null && ReturnValue.IsNamed)
		{
			ValidateName(ReturnValue, instance, diagnostics, ErrorCode.ERR_DuplicateReturnValueName);
		}
		if (!LocalVariables.IsDefaultOrEmpty)
		{
			foreach (VariableSymbol localVariable in LocalVariables)
			{
				ValidateName(localVariable, instance, diagnostics, ErrorCode.ERR_DuplicateVariableName);
			}
		}
		instance?.Free();
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

	protected void LazyMethodChecks()
	{
		if (state.HasComplete(CompletionPart.FinishMethodChecks))
		{
			return;
		}
		object methodChecksLockObject = MethodChecksLockObject;
		DebugAssertHelper.Assert(methodChecksLockObject != null);
		lock (methodChecksLockObject)
		{
			if (state.NotePartComplete(CompletionPart.StartMethodChecks))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				try
				{
					ParametersChecks(instance);
					VariablesCheck();
					NameConflictCheck(instance);
					AttributeChecks(instance);
					PerformAdditionalMethodChecks(instance);
					ApplyAttributeModificationsToMethod();
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

	private void ApplyAttributeModificationsToMethod()
	{
		if (this.HasAttribute(AttributeKind.TryFunction) && MethodKind == MethodKind.Method)
		{
			lazyIsTryFunction = true;
			if (lazyReturnValue == null)
			{
				lazyReturnValue = Microsoft.Dynamics.Nav.CodeAnalysis.Symbols.ReturnValueSymbol.Boolean;
			}
		}
	}

	protected virtual void PerformAdditionalMethodChecks(DiagnosticBag diagnostics)
	{
	}

	private void LazyGetAttributes()
	{
		if (!state.HasComplete(CompletionPart.Attributes))
		{
			ImmutableInterlocked.InterlockedInitialize(ref lazyAttributes, ((IEnumerable<AttributeSymbol>)GetSyntax().Attributes.Select((MemberAttributeSyntax attribute) => SourceAttributeSymbol.Create(this, attribute))).ToImmutableArray());
		}
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	[SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "unusedType")]
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
				DebugAssertHelper.Assert(state.HasComplete(CompletionPart.Attributes));
				break;
			case CompletionPart.ReturnType:
				_ = ReturnValue;
				state.NotePartComplete(CompletionPart.ReturnType);
				break;
			case CompletionPart.Parameters:
				foreach (ParameterSymbol parameter in Parameters)
				{
					parameter.ForceComplete(locationOpt, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.Parameters);
				break;
			case CompletionPart.Variables:
				foreach (VariableSymbol localVariable in LocalVariables)
				{
					localVariable.ForceComplete(locationOpt, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.Variables);
				break;
			case CompletionPart.StartMethodChecks:
			case CompletionPart.FinishMethodChecks:
			{
				LazyMethodChecks();
				CompletionPart part = CompletionPart.MethodSymbolAll;
				state.SpinWaitComplete(part, cancellationToken);
				return;
			}
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete((CompletionPart)106466);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void ForceCompleteAttributes(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		LazyGetAttributes();
		foreach (AttributeSymbol lazyAttribute in lazyAttributes)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lazyAttribute.ForceComplete(locationOpt, cancellationToken);
		}
		state.NotePartComplete(CompletionPart.Attributes);
	}

	protected void ParametersChecks(DiagnosticBag diagnostics)
	{
		MethodOrTriggerDeclarationSyntax syntax = GetSyntax();
		Binder withTypeParamsBinder = GetWithTypeParamsBinder(syntax);
		LazyGetParameters(syntax, withTypeParamsBinder, diagnostics);
		LazyGetReturnValueBeforeAttributeEffects(syntax, withTypeParamsBinder, diagnostics);
	}

	private Binder GetWithTypeParamsBinder(MethodOrTriggerDeclarationSyntax syntax)
	{
		BinderFactory binderFactory = DeclaringCompilation.GetBinderFactory(syntax.SyntaxTree);
		Binder binder = ((syntax.ReturnValue != null) ? binderFactory.GetBinder(syntax.ReturnValue) : null);
		if (binder == null && syntax.ParameterList.Parameters.Count > 0)
		{
			binder = binderFactory.GetBinder(syntax.ParameterList.Parameters[0]);
		}
		return binder;
	}

	private void AttributeChecks(DiagnosticBag diagnostics)
	{
		LazyGetAttributes();
		AttributeValidatorHelper.CheckMutuallyExclusiveAndDuplicateAttributes(this, diagnostics);
		AttributeValidatorHelper.CheckMethodSignature(this, diagnostics);
	}

	private void LazyGetParameters(MethodOrTriggerDeclarationSyntax syntax, Binder signatureBinder, DiagnosticBag diagnostics)
	{
		lazyParameters = ParameterHelpers.MakeParameters(signatureBinder, this, syntax.ParameterList.Parameters, diagnostics);
	}

	private void LazyGetReturnValueBeforeAttributeEffects(MethodOrTriggerDeclarationSyntax syntax, Binder signatureBinder, DiagnosticBag diagnostics)
	{
		if (syntax.ReturnValue != null)
		{
			lazyReturnValue = SourceReturnValueSymbol.Create(this, signatureBinder.BindReturnValueType(this, syntax.ReturnValue.DataType, diagnostics), syntax.ReturnValue);
		}
	}
}
