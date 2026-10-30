using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LoweredDynamicOperationFactory
{
	[Flags]
	private enum CSharpBinderFlags
	{
		None = 0,
		CheckedContext = 1,
		InvokeSimpleName = 2,
		InvokeSpecialName = 4,
		BinaryOperationLogical = 8,
		ConvertExplicit = 0x10,
		ConvertArrayIndex = 0x20,
		ResultIndexed = 0x40,
		ValueFromCompoundAssignment = 0x80,
		ResultDiscarded = 0x100
	}

	[Flags]
	private enum CSharpArgumentInfoFlags
	{
		None = 0,
		UseCompileTimeType = 1,
		Constant = 2,
		NamedArgument = 4,
		IsRef = 8,
		IsOut = 0x10,
		IsStaticType = 0x20
	}

	private readonly SyntheticBoundNodeFactory _factory;

	private readonly int _methodOrdinal;

	private NamedTypeSymbol _currentDynamicCallSiteContainer;

	private int _callSiteIdDispenser;

	internal LoweredDynamicOperationFactory(SyntheticBoundNodeFactory factory, int methodOrdinal)
	{
		_factory = factory;
		_methodOrdinal = methodOrdinal;
	}

	internal LoweredDynamicOperation MakeDynamicConversion(BoundExpression loweredOperand, bool isExplicit, bool isArrayIndex, bool isChecked, TypeSymbol resultType)
	{
		_factory.Syntax = loweredOperand.Syntax;
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		if (isChecked)
		{
			cSharpBinderFlags |= CSharpBinderFlags.CheckedContext;
		}
		if (isExplicit)
		{
			cSharpBinderFlags |= CSharpBinderFlags.ConvertExplicit;
		}
		if (isArrayIndex)
		{
			cSharpBinderFlags |= CSharpBinderFlags.ConvertArrayIndex;
		}
		ImmutableArray<BoundExpression> loweredArguments = ImmutableArray.Create(loweredOperand);
		BoundExpression binderConstruction = MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__Convert, new BoundExpression[3]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.Typeof(resultType),
			_factory.TypeofDynamicOperationContextType()
		});
		return MakeDynamicOperation(binderConstruction, null, RefKind.None, loweredArguments, default, null, resultType);
	}

	internal LoweredDynamicOperation MakeDynamicUnaryOperator(UnaryOperatorKind operatorKind, BoundExpression loweredOperand, TypeSymbol resultType)
	{
		_factory.Syntax = loweredOperand.Syntax;
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		if (operatorKind.IsChecked())
		{
			cSharpBinderFlags |= CSharpBinderFlags.CheckedContext;
		}
		ImmutableArray<BoundExpression> loweredArguments = ImmutableArray.Create(loweredOperand);
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__UnaryOperation, new BoundExpression[4]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.Literal((int)operatorKind.ToExpressionType()),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, loweredArguments)
		}) : null);
		return MakeDynamicOperation(binderConstruction, null, RefKind.None, loweredArguments, default, null, resultType);
	}

	internal LoweredDynamicOperation MakeDynamicBinaryOperator(BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, bool isCompoundAssignment, TypeSymbol resultType)
	{
		_factory.Syntax = loweredLeft.Syntax;
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		if (operatorKind.IsChecked())
		{
			cSharpBinderFlags |= CSharpBinderFlags.CheckedContext;
		}
		if (operatorKind.IsLogical())
		{
			cSharpBinderFlags |= CSharpBinderFlags.BinaryOperationLogical;
		}
		ImmutableArray<BoundExpression> loweredArguments = ImmutableArray.Create(loweredLeft, loweredRight);
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__BinaryOperation, new BoundExpression[4]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.Literal((int)operatorKind.ToExpressionType(isCompoundAssignment)),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, loweredArguments)
		}) : null);
		return MakeDynamicOperation(binderConstruction, null, RefKind.None, loweredArguments, default, null, resultType);
	}

	internal LoweredDynamicOperation MakeDynamicMemberInvocation(string name, BoundExpression loweredReceiver, ImmutableArray<TypeSymbol> typeArguments, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> refKinds, bool hasImplicitReceiver, bool resultDiscarded)
	{
		_factory.Syntax = loweredReceiver.Syntax;
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		if (hasImplicitReceiver && !_factory.TopLevelMethod.IsStatic)
		{
			cSharpBinderFlags |= CSharpBinderFlags.InvokeSimpleName;
		}
		TypeSymbol resultType;
		if (resultDiscarded)
		{
			cSharpBinderFlags |= CSharpBinderFlags.ResultDiscarded;
			resultType = _factory.SpecialType(SpecialType.System_Void);
		}
		else
		{
			resultType = AssemblySymbol.DynamicType;
		}
		RefKind receiverRefKind;
		bool receiverIsStaticType;
		if (loweredReceiver.Kind == BoundKind.TypeExpression)
		{
			loweredReceiver = _factory.Typeof(((BoundTypeExpression)loweredReceiver).Type);
			receiverRefKind = RefKind.None;
			receiverIsStaticType = true;
		}
		else
		{
			receiverRefKind = GetReceiverRefKind(loweredReceiver);
			receiverIsStaticType = false;
		}
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__InvokeMember, new BoundExpression[5]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.Literal(name),
			typeArguments.IsDefaultOrEmpty ? _factory.Null(_factory.WellKnownArrayType(WellKnownType.System_Type)) : _factory.ArrayOrEmpty(_factory.WellKnownType(WellKnownType.System_Type), _factory.TypeOfs(typeArguments)),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, loweredArguments, argumentNames, refKinds, loweredReceiver, receiverRefKind, receiverIsStaticType)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, receiverRefKind, loweredArguments, refKinds, null, resultType);
	}

	internal LoweredDynamicOperation MakeDynamicEventAccessorInvocation(string accessorName, BoundExpression loweredReceiver, BoundExpression loweredHandler)
	{
		_factory.Syntax = loweredReceiver.Syntax;
		CSharpBinderFlags value = CSharpBinderFlags.InvokeSpecialName | CSharpBinderFlags.ResultDiscarded;
		ImmutableArray<BoundExpression> empty = ImmutableArray<BoundExpression>.Empty;
		TypeSymbol dynamicType = AssemblySymbol.DynamicType;
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__InvokeMember, new BoundExpression[5]
		{
			_factory.Literal((int)value),
			_factory.Literal(accessorName),
			_factory.Null(_factory.WellKnownArrayType(WellKnownType.System_Type)),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, empty, default, default, loweredReceiver, RefKind.None, receiverIsStaticType: false, loweredHandler)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, RefKind.None, empty, default, loweredHandler, dynamicType);
	}

	internal LoweredDynamicOperation MakeDynamicInvocation(BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> refKinds, bool resultDiscarded)
	{
		_factory.Syntax = loweredReceiver.Syntax;
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		TypeSymbol resultType;
		if (resultDiscarded)
		{
			cSharpBinderFlags |= CSharpBinderFlags.ResultDiscarded;
			resultType = _factory.SpecialType(SpecialType.System_Void);
		}
		else
		{
			resultType = AssemblySymbol.DynamicType;
		}
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__Invoke, new BoundExpression[3]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, loweredArguments, argumentNames, refKinds, loweredReceiver)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, RefKind.None, loweredArguments, refKinds, null, resultType);
	}

	internal LoweredDynamicOperation MakeDynamicConstructorInvocation(SyntaxNode syntax, TypeSymbol type, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> refKinds)
	{
		_factory.Syntax = syntax;
		BoundExpression loweredReceiver = _factory.Typeof(type);
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__InvokeConstructor, new BoundExpression[3]
		{
			_factory.Literal(0),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, loweredArguments, argumentNames, refKinds, loweredReceiver, RefKind.None, receiverIsStaticType: true)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, RefKind.None, loweredArguments, refKinds, null, type);
	}

	internal LoweredDynamicOperation MakeDynamicGetMember(BoundExpression loweredReceiver, string name, bool resultIndexed)
	{
		_factory.Syntax = loweredReceiver.Syntax;
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		if (resultIndexed)
		{
			cSharpBinderFlags |= CSharpBinderFlags.ResultIndexed;
		}
		ImmutableArray<BoundExpression> empty = ImmutableArray<BoundExpression>.Empty;
		DynamicTypeSymbol instance = DynamicTypeSymbol.Instance;
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__GetMember, new BoundExpression[4]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.Literal(name),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, empty, default, default, loweredReceiver)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, RefKind.None, empty, default, null, instance);
	}

	internal LoweredDynamicOperation MakeDynamicSetMember(BoundExpression loweredReceiver, string name, BoundExpression loweredRight, bool isCompoundAssignment = false, bool isChecked = false)
	{
		_factory.Syntax = loweredReceiver.Syntax;
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		if (isCompoundAssignment)
		{
			cSharpBinderFlags |= CSharpBinderFlags.ValueFromCompoundAssignment;
			if (isChecked)
			{
				cSharpBinderFlags |= CSharpBinderFlags.CheckedContext;
			}
		}
		ImmutableArray<BoundExpression> empty = ImmutableArray<BoundExpression>.Empty;
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__SetMember, new BoundExpression[4]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.Literal(name),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, empty, default, default, loweredReceiver, RefKind.None, receiverIsStaticType: false, loweredRight)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, RefKind.None, empty, default, loweredRight, AssemblySymbol.DynamicType);
	}

	internal LoweredDynamicOperation MakeDynamicGetIndex(BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> refKinds)
	{
		_factory.Syntax = loweredReceiver.Syntax;
		DynamicTypeSymbol instance = DynamicTypeSymbol.Instance;
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__GetIndex, new BoundExpression[3]
		{
			_factory.Literal(0),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, loweredArguments, argumentNames, refKinds, loweredReceiver)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, RefKind.None, loweredArguments, refKinds, null, instance);
	}

	internal LoweredDynamicOperation MakeDynamicSetIndex(BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> refKinds, BoundExpression loweredRight, bool isCompoundAssignment = false, bool isChecked = false)
	{
		CSharpBinderFlags cSharpBinderFlags = CSharpBinderFlags.None;
		if (isCompoundAssignment)
		{
			cSharpBinderFlags |= CSharpBinderFlags.ValueFromCompoundAssignment;
			if (isChecked)
			{
				cSharpBinderFlags |= CSharpBinderFlags.CheckedContext;
			}
		}
		RefKind receiverRefKind = GetReceiverRefKind(loweredReceiver);
		DynamicTypeSymbol instance = DynamicTypeSymbol.Instance;
		MethodSymbol argumentInfoFactory = GetArgumentInfoFactory();
		BoundExpression binderConstruction = (((object)argumentInfoFactory != null) ? MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__SetIndex, new BoundExpression[3]
		{
			_factory.Literal((int)cSharpBinderFlags),
			_factory.TypeofDynamicOperationContextType(),
			MakeCallSiteArgumentInfos(argumentInfoFactory, loweredArguments, argumentNames, refKinds, loweredReceiver, receiverRefKind, receiverIsStaticType: false, loweredRight)
		}) : null);
		return MakeDynamicOperation(binderConstruction, loweredReceiver, receiverRefKind, loweredArguments, refKinds, loweredRight, instance);
	}

	internal LoweredDynamicOperation MakeDynamicIsEventTest(string name, BoundExpression loweredReceiver)
	{
		_factory.Syntax = loweredReceiver.Syntax;
		NamedTypeSymbol resultType = _factory.SpecialType(SpecialType.System_Boolean);
		BoundExpression binderConstruction = MakeBinderConstruction(WellKnownMember.Microsoft_CSharp_RuntimeBinder_Binder__IsEvent, new BoundExpression[3]
		{
			_factory.Literal(0),
			_factory.Literal(name),
			_factory.TypeofDynamicOperationContextType()
		});
		return MakeDynamicOperation(binderConstruction, loweredReceiver, RefKind.None, ImmutableArray<BoundExpression>.Empty, default, null, resultType);
	}

	private MethodSymbol GetArgumentInfoFactory()
	{
		return _factory.WellKnownMethod(WellKnownMember.Microsoft_CSharp_RuntimeBinder_CSharpArgumentInfo__Create);
	}

	private BoundExpression MakeBinderConstruction(WellKnownMember factoryMethod, BoundExpression[] args)
	{
		Symbol symbol = _factory.WellKnownMember(factoryMethod);
		if ((object)symbol == null)
		{
			return null;
		}
		return _factory.Call(null, (MethodSymbol)symbol, args.AsImmutableOrNull());
	}

	internal static RefKind GetReceiverRefKind(BoundExpression loweredReceiver)
	{
		if (!loweredReceiver.Type.IsValueType)
		{
			return RefKind.None;
		}
		switch (loweredReceiver.Kind)
		{
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.RefValueOperator:
		case BoundKind.ArrayAccess:
		case BoundKind.ThisReference:
		case BoundKind.Local:
		case BoundKind.Parameter:
			return RefKind.Ref;
		case BoundKind.TypeExpression:
		case BoundKind.BaseReference:
			throw ExceptionUtilities.UnexpectedValue(loweredReceiver.Kind);
		default:
			return RefKind.None;
		}
	}

	internal BoundExpression MakeCallSiteArgumentInfos(MethodSymbol argumentInfoFactory, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames = default(ImmutableArray<string>), ImmutableArray<RefKind> refKinds = default(ImmutableArray<RefKind>), BoundExpression loweredReceiver = null, RefKind receiverRefKind = RefKind.None, bool receiverIsStaticType = false, BoundExpression loweredRight = null)
	{
		BoundExpression[] array = new BoundExpression[((loweredReceiver != null) ? 1 : 0) + loweredArguments.Length + ((loweredRight != null) ? 1 : 0)];
		int num = 0;
		if (loweredReceiver != null)
		{
			array[num++] = GetArgumentInfo(argumentInfoFactory, loweredReceiver, null, receiverRefKind, receiverIsStaticType);
		}
		for (int i = 0; i < loweredArguments.Length; i++)
		{
			array[num++] = GetArgumentInfo(argumentInfoFactory, loweredArguments[i], argumentNames.IsDefaultOrEmpty ? null : argumentNames[i], (!refKinds.IsDefault) ? refKinds[i] : RefKind.None, isStaticType: false);
		}
		if (loweredRight != null)
		{
			array[num] = GetArgumentInfo(argumentInfoFactory, loweredRight, null, RefKind.None, isStaticType: false);
		}
		return _factory.ArrayOrEmpty(argumentInfoFactory.ContainingType, array);
	}

	internal LoweredDynamicOperation MakeDynamicOperation(BoundExpression binderConstruction, BoundExpression loweredReceiver, RefKind receiverRefKind, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<RefKind> refKinds, BoundExpression loweredRight, TypeSymbol resultType)
	{
		NamedTypeSymbol delegateType = GetDelegateType(loweredReceiver, receiverRefKind, loweredArguments, refKinds, loweredRight, resultType);
		NamedTypeSymbol namedTypeSymbol = _factory.WellKnownType(WellKnownType.System_Runtime_CompilerServices_CallSite_T);
		MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Runtime_CompilerServices_CallSite_T__Create);
		FieldSymbol fieldSymbol = (FieldSymbol)_factory.WellKnownMember(WellKnownMember.System_Runtime_CompilerServices_CallSite_T__Target);
		MethodSymbol delegateInvokeMethod;
		if (binderConstruction == null || (object)delegateType == null || delegateType.IsErrorType() || (object)(delegateInvokeMethod = delegateType.DelegateInvokeMethod) == null || namedTypeSymbol.IsErrorType() || (object)methodSymbol == null || (object)fieldSymbol == null)
		{
			_factory.Diagnostics.Add(ErrorCode.ERR_DynamicRequiredTypesMissing, NoLocation.Singleton);
			return LoweredDynamicOperation.Bad(loweredReceiver, loweredArguments, loweredRight, resultType);
		}
		if ((object)_currentDynamicCallSiteContainer == null)
		{
			_currentDynamicCallSiteContainer = CreateCallSiteContainer(_factory, _methodOrdinal);
		}
		SynthesizedContainer synthesizedContainer = (SynthesizedContainer)_currentDynamicCallSiteContainer.OriginalDefinition;
		TypeMap typeMap = synthesizedContainer.TypeMap;
		ImmutableArray<LocalSymbol> temps = MakeTempsForDiscardArguments(ref loweredArguments);
		TypeSymbol[] typeArguments = new NamedTypeSymbol[1] { delegateType };
		NamedTypeSymbol newOwner = namedTypeSymbol.Construct(typeArguments);
		MethodSymbol method = methodSymbol.AsMember(newOwner);
		FieldSymbol f = fieldSymbol.AsMember(newOwner);
		FieldSymbol fieldSymbol2 = DefineCallSiteStorageSymbol(synthesizedContainer, delegateType, typeMap);
		BoundFieldAccess boundFieldAccess = _factory.Field(null, fieldSymbol2);
		ImmutableArray<BoundExpression> callSiteArguments = GetCallSiteArguments(boundFieldAccess, loweredReceiver, loweredArguments, loweredRight);
		BoundExpression boundExpression = _factory.Null(fieldSymbol2.Type);
		BoundExpression siteInitialization = _factory.Conditional(_factory.ObjectEqual(boundFieldAccess, boundExpression), _factory.AssignmentExpression(boundFieldAccess, _factory.Call(null, method, binderConstruction)), boundExpression, fieldSymbol2.Type);
		BoundCall siteInvocation = _factory.Call(_factory.Field(boundFieldAccess, f), delegateInvokeMethod, callSiteArguments);
		return new LoweredDynamicOperation(_factory, siteInitialization, siteInvocation, resultType, temps);
	}

	private ImmutableArray<LocalSymbol> MakeTempsForDiscardArguments(ref ImmutableArray<BoundExpression> loweredArguments)
	{
		int num = loweredArguments.Count((BoundExpression a) => a.Kind == BoundKind.DiscardExpression);
		if (num == 0)
		{
			return ImmutableArray<LocalSymbol>.Empty;
		}
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance(num);
		loweredArguments = _factory.MakeTempsForDiscardArguments(loweredArguments, instance);
		return instance.ToImmutableAndFree();
	}

	private static NamedTypeSymbol CreateCallSiteContainer(SyntheticBoundNodeFactory factory, int methodOrdinal)
	{
		int currentGenerationOrdinal = factory.CompilationState.ModuleBuilderOpt.CurrentGenerationOrdinal;
		DynamicSiteContainer dynamicSiteContainer = new DynamicSiteContainer(GeneratedNames.MakeDynamicCallSiteContainerName(methodOrdinal, currentGenerationOrdinal), factory.TopLevelMethod);
		factory.AddNestedType(dynamicSiteContainer);
		if (factory.TopLevelMethod.IsGenericMethod)
		{
			return dynamicSiteContainer.Construct(factory.TopLevelMethod.TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>());
		}
		return dynamicSiteContainer;
	}

	internal FieldSymbol DefineCallSiteStorageSymbol(NamedTypeSymbol containerDefinition, NamedTypeSymbol delegateTypeOverMethodTypeParameters, TypeMap methodToContainerTypeParametersMap)
	{
		string name = GeneratedNames.MakeDynamicCallSiteFieldName(_callSiteIdDispenser++);
		NamedTypeSymbol namedTypeSymbol = methodToContainerTypeParametersMap.SubstituteNamedType(delegateTypeOverMethodTypeParameters);
		NamedTypeSymbol wellKnownType = _factory.Compilation.GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_CallSite_T);
		TypeSymbol[] typeArguments = new NamedTypeSymbol[1] { namedTypeSymbol };
		NamedTypeSymbol type = wellKnownType.Construct(typeArguments);
		SynthesizedFieldSymbol synthesizedFieldSymbol = new SynthesizedFieldSymbol(containerDefinition, type, name, isPublic: true, isReadOnly: false, isStatic: true);
		_factory.AddField(containerDefinition, synthesizedFieldSymbol);
		if (!_currentDynamicCallSiteContainer.IsGenericType)
		{
			return synthesizedFieldSymbol;
		}
		return synthesizedFieldSymbol.AsMember(_currentDynamicCallSiteContainer);
	}

	internal NamedTypeSymbol GetDelegateType(BoundExpression loweredReceiver, RefKind receiverRefKind, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<RefKind> refKinds, BoundExpression loweredRight, TypeSymbol resultType)
	{
		NamedTypeSymbol namedTypeSymbol = _factory.WellKnownType(WellKnownType.System_Runtime_CompilerServices_CallSite);
		if (namedTypeSymbol.IsErrorType())
		{
			return null;
		}
		TypeSymbol[] array = MakeCallSiteDelegateSignature(namedTypeSymbol, loweredReceiver, loweredArguments, loweredRight, resultType);
		bool flag = resultType.SpecialType == SpecialType.System_Void;
		bool flag2 = receiverRefKind != RefKind.None || !refKinds.IsDefaultOrEmpty;
		if (!flag2)
		{
			WellKnownType wellKnownType = (flag ? WellKnownTypes.GetWellKnownActionDelegate(array.Length) : WellKnownTypes.GetWellKnownFunctionDelegate(array.Length - 1));
			if (wellKnownType != WellKnownType.Unknown)
			{
				NamedTypeSymbol wellKnownType2 = _factory.Compilation.GetWellKnownType(wellKnownType);
				if (!wellKnownType2.HasUseSiteError)
				{
					return wellKnownType2.Construct(array);
				}
			}
		}
		BitVector byRefParameters;
		if (flag2)
		{
			byRefParameters = BitVector.Create(1 + ((loweredReceiver != null) ? 1 : 0) + loweredArguments.Length + ((loweredRight != null) ? 1 : 0));
			int num = 1;
			if (loweredReceiver != null)
			{
				byRefParameters[num++] = receiverRefKind != RefKind.None;
			}
			if (!refKinds.IsDefault)
			{
				int num2 = 0;
				while (num2 < refKinds.Length)
				{
					if (refKinds[num2] != RefKind.None)
					{
						byRefParameters[num] = true;
					}
					num2++;
					num++;
				}
			}
		}
		else
		{
			byRefParameters = default;
		}
		int parameterCount = array.Length - ((!flag) ? 1 : 0);
		int currentGenerationOrdinal = _factory.CompilationState.ModuleBuilderOpt.CurrentGenerationOrdinal;
		return _factory.Compilation.AnonymousTypeManager.SynthesizeDelegate(parameterCount, byRefParameters, flag, currentGenerationOrdinal).Construct(array);
	}

	internal BoundExpression GetArgumentInfo(MethodSymbol argumentInfoFactory, BoundExpression boundArgument, string name, RefKind refKind, bool isStaticType)
	{
		CSharpArgumentInfoFlags cSharpArgumentInfoFlags = CSharpArgumentInfoFlags.None;
		if (isStaticType)
		{
			cSharpArgumentInfoFlags |= CSharpArgumentInfoFlags.IsStaticType;
		}
		if (name != null)
		{
			cSharpArgumentInfoFlags |= CSharpArgumentInfoFlags.NamedArgument;
		}
		switch (refKind)
		{
		case RefKind.Out:
			cSharpArgumentInfoFlags |= CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsOut;
			break;
		case RefKind.Ref:
			cSharpArgumentInfoFlags |= CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsRef;
			break;
		}
		TypeSymbol type = boundArgument.Type;
		if (boundArgument.ConstantValue != null)
		{
			cSharpArgumentInfoFlags |= CSharpArgumentInfoFlags.Constant;
		}
		if ((object)type != null && !type.IsDynamic())
		{
			cSharpArgumentInfoFlags |= CSharpArgumentInfoFlags.UseCompileTimeType;
		}
		return _factory.Call(null, argumentInfoFactory, _factory.Literal((int)cSharpArgumentInfoFlags), _factory.Literal(name));
	}

	internal static ImmutableArray<BoundExpression> GetCallSiteArguments(BoundExpression callSiteFieldAccess, BoundExpression receiver, ImmutableArray<BoundExpression> arguments, BoundExpression right)
	{
		BoundExpression[] array = new BoundExpression[1 + ((receiver != null) ? 1 : 0) + arguments.Length + ((right != null) ? 1 : 0)];
		int num = 0;
		array[num++] = callSiteFieldAccess;
		if (receiver != null)
		{
			array[num++] = receiver;
		}
		arguments.CopyTo(array, num);
		num += arguments.Length;
		if (right != null)
		{
			array[num] = right;
		}
		return array.AsImmutableOrNull();
	}

	internal TypeSymbol[] MakeCallSiteDelegateSignature(TypeSymbol callSiteType, BoundExpression receiver, ImmutableArray<BoundExpression> arguments, BoundExpression right, TypeSymbol resultType)
	{
		NamedTypeSymbol namedTypeSymbol = _factory.SpecialType(SpecialType.System_Object);
		TypeSymbol[] array = new TypeSymbol[1 + ((receiver != null) ? 1 : 0) + arguments.Length + ((right != null) ? 1 : 0) + ((resultType.SpecialType != SpecialType.System_Void) ? 1 : 0)];
		int num = 0;
		array[num++] = callSiteType;
		if (receiver != null)
		{
			array[num++] = receiver.Type ?? namedTypeSymbol;
		}
		for (int i = 0; i < arguments.Length; i++)
		{
			array[num++] = arguments[i].Type ?? namedTypeSymbol;
		}
		if (right != null)
		{
			array[num++] = right.Type ?? namedTypeSymbol;
		}
		if (num < array.Length)
		{
			array[num] = resultType ?? namedTypeSymbol;
		}
		return array;
	}
}
