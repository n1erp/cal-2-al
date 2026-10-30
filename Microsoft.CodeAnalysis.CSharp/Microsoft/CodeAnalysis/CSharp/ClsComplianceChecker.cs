using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ClsComplianceChecker : CSharpSymbolVisitor
{
	private enum Compliance
	{
		DeclaredTrue,
		DeclaredFalse,
		InheritedTrue,
		InheritedFalse,
		ImpliedFalse
	}

	private readonly CSharpCompilation _compilation;

	private readonly SyntaxTree _filterTree;

	private readonly TextSpan? _filterSpanWithinTree;

	private readonly ConcurrentQueue<Diagnostic> _diagnostics;

	private readonly CancellationToken _cancellationToken;

	private readonly ConcurrentDictionary<Symbol, Compliance> _declaredOrInheritedCompliance;

	private readonly ConcurrentStack<Task> _compilerTasks;

	private bool ConcurrentAnalysis
	{
		get
		{
			if (_filterTree == null)
			{
				return _compilation.Options.ConcurrentBuild;
			}
			return false;
		}
	}

	private ClsComplianceChecker(CSharpCompilation compilation, SyntaxTree filterTree, TextSpan? filterSpanWithinTree, ConcurrentQueue<Diagnostic> diagnostics, CancellationToken cancellationToken)
	{
		_compilation = compilation;
		_filterTree = filterTree;
		_filterSpanWithinTree = filterSpanWithinTree;
		_diagnostics = diagnostics;
		_cancellationToken = cancellationToken;
		_declaredOrInheritedCompliance = new ConcurrentDictionary<Symbol, Compliance>();
		if (ConcurrentAnalysis)
		{
			_compilerTasks = new ConcurrentStack<Task>();
		}
	}

	public static void CheckCompliance(CSharpCompilation compilation, DiagnosticBag diagnostics, CancellationToken cancellationToken, SyntaxTree filterTree = null, TextSpan? filterSpanWithinTree = null)
	{
		ConcurrentQueue<Diagnostic> concurrentQueue = new ConcurrentQueue<Diagnostic>();
		ClsComplianceChecker clsComplianceChecker = new ClsComplianceChecker(compilation, filterTree, filterSpanWithinTree, concurrentQueue, cancellationToken);
		clsComplianceChecker.Visit(compilation.Assembly);
		clsComplianceChecker.WaitForWorkers();
		foreach (Diagnostic item in concurrentQueue)
		{
			diagnostics.Add(item);
		}
	}

	public override void VisitAssembly(AssemblySymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		Compliance declaredOrInheritedCompliance = GetDeclaredOrInheritedCompliance(symbol);
		if (declaredOrInheritedCompliance == Compliance.DeclaredFalse)
		{
			return;
		}
		bool flag = IsTrue(declaredOrInheritedCompliance);
		for (int i = 0; i < symbol.Modules.Length; i++)
		{
			ModuleSymbol moduleSymbol = symbol.Modules[i];
			bool? declaredCompliance = GetDeclaredCompliance(moduleSymbol, out var attributeLocation);
			Location location = ((i == 0) ? attributeLocation : moduleSymbol.Locations[0]);
			if (declaredCompliance.HasValue)
			{
				if (location != null)
				{
					if (!IsDeclared(declaredOrInheritedCompliance))
					{
						AddDiagnostic(ErrorCode.WRN_CLS_NotOnModules, location);
					}
					else if (flag != (declaredCompliance == true))
					{
						AddDiagnostic(ErrorCode.WRN_CLS_NotOnModules2, location);
					}
				}
			}
			else
			{
				if (!flag || i <= 0)
				{
					continue;
				}
				bool flag2 = false;
				PEModuleSymbol pEModuleSymbol = (PEModuleSymbol)moduleSymbol;
				foreach (CSharpAttributeData assemblyAttribute in pEModuleSymbol.GetAssemblyAttributes())
				{
					if (assemblyAttribute.IsTargetAttribute(pEModuleSymbol, AttributeDescription.CLSCompliantAttribute))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					AddDiagnostic(ErrorCode.WRN_CLS_ModuleMissingCLS, location);
				}
			}
		}
		if (flag)
		{
			CheckForAttributeWithArrayArgument(symbol);
		}
		ModuleSymbol symbol2 = symbol.Modules[0];
		if (IsTrue(GetDeclaredOrInheritedCompliance(symbol2)))
		{
			CheckForAttributeWithArrayArgument(symbol2);
		}
		Visit(symbol.GlobalNamespace);
	}

	private void WaitForWorkers()
	{
		ConcurrentStack<Task> compilerTasks = _compilerTasks;
		if (compilerTasks != null)
		{
			Task result;
			while (compilerTasks.TryPop(out result))
			{
				result.GetAwaiter().GetResult();
			}
		}
	}

	public override void VisitNamespace(NamespaceSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (!DoNotVisit(symbol))
		{
			if (IsTrue(GetDeclaredOrInheritedCompliance(symbol)))
			{
				CheckName(symbol);
				CheckMemberDistinctness(symbol);
			}
			if (ConcurrentAnalysis)
			{
				VisitNamespaceMembersAsTasks(symbol);
			}
			else
			{
				VisitNamespaceMembers(symbol);
			}
		}
	}

	private void VisitNamespaceMembersAsTasks(NamespaceSymbol symbol)
	{
		foreach (Symbol m in symbol.GetMembersUnordered())
		{
			_compilerTasks.Push(Task.Run(UICultureUtilities.WithCurrentUICulture(() =>
			{
				try
				{
					Visit(m);
				}
				catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
				{
					throw ExceptionUtilities.Unreachable;
				}
			}), _cancellationToken));
		}
	}

	private void VisitNamespaceMembers(NamespaceSymbol symbol)
	{
		foreach (Symbol item in symbol.GetMembersUnordered())
		{
			Visit(item);
		}
	}

	public override void VisitNamedType(NamedTypeSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (DoNotVisit(symbol))
		{
			return;
		}
		Compliance declaredOrInheritedCompliance = GetDeclaredOrInheritedCompliance(symbol);
		if (VisitTypeOrMember(symbol, declaredOrInheritedCompliance) && IsTrue(declaredOrInheritedCompliance))
		{
			CheckBaseTypeCompliance(symbol);
			CheckTypeParameterCompliance(symbol.TypeParameters, symbol);
			if (symbol.TypeKind == TypeKind.Delegate)
			{
				CheckParameterCompliance(symbol.DelegateInvokeMethod.Parameters, symbol);
			}
			else if (_compilation.IsAttributeType(symbol) && !HasAcceptableAttributeConstructor(symbol))
			{
				AddDiagnostic(ErrorCode.WRN_CLS_BadAttributeType, symbol.Locations[0], symbol);
			}
		}
		foreach (Symbol item in symbol.GetMembersUnordered())
		{
			Visit(item);
		}
	}

	private bool HasAcceptableAttributeConstructor(NamedTypeSymbol attributeType)
	{
		foreach (MethodSymbol instanceConstructor in attributeType.InstanceConstructors)
		{
			if (!IsTrue(GetDeclaredOrInheritedCompliance(instanceConstructor)) || !IsAccessibleIfContainerIsAccessible(instanceConstructor))
			{
				continue;
			}
			bool flag = false;
			foreach (TypeSymbol parameterType in instanceConstructor.ParameterTypes)
			{
				if (parameterType.TypeKind == TypeKind.Array || parameterType.GetAttributeParameterTypedConstantKind(_compilation) == TypedConstantKind.Error)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	public override void VisitMethod(MethodSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (DoNotVisit(symbol))
		{
			return;
		}
		Compliance declaredOrInheritedCompliance = GetDeclaredOrInheritedCompliance(symbol);
		if (symbol.IsAccessor())
		{
			CheckForAttributeOnAccessor(symbol);
			CheckForMeaninglessOnParameter(symbol.Parameters);
			CheckForMeaninglessOnReturn(symbol);
			if (IsTrue(declaredOrInheritedCompliance))
			{
				CheckForAttributeWithArrayArgument(symbol);
			}
		}
		else if (VisitTypeOrMember(symbol, declaredOrInheritedCompliance) && IsTrue(declaredOrInheritedCompliance))
		{
			CheckParameterCompliance(symbol.Parameters, symbol.ContainingType);
			CheckTypeParameterCompliance(symbol.TypeParameters, symbol.ContainingType);
			if (symbol.IsVararg)
			{
				AddDiagnostic(ErrorCode.WRN_CLS_NoVarArgs, symbol.Locations[0]);
			}
		}
	}

	private void CheckForAttributeOnAccessor(MethodSymbol symbol)
	{
		foreach (CSharpAttributeData attribute in symbol.GetAttributes())
		{
			if (attribute.IsTargetAttribute(symbol, AttributeDescription.CLSCompliantAttribute) && TryGetAttributeWarningLocation(attribute, out var location))
			{
				AttributeUsageInfo attributeUsageInfo = attribute.AttributeClass.GetAttributeUsageInfo();
				AddDiagnostic(ErrorCode.ERR_AttributeNotOnAccessor, location, attribute.AttributeClass.Name, attributeUsageInfo.GetValidTargetsErrorArgument());
				break;
			}
		}
	}

	public override void VisitProperty(PropertySymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (!DoNotVisit(symbol))
		{
			Compliance declaredOrInheritedCompliance = GetDeclaredOrInheritedCompliance(symbol);
			if (VisitTypeOrMember(symbol, declaredOrInheritedCompliance) && IsTrue(declaredOrInheritedCompliance))
			{
				CheckParameterCompliance(symbol.Parameters, symbol.ContainingType);
			}
		}
	}

	public override void VisitEvent(EventSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (!DoNotVisit(symbol))
		{
			Compliance declaredOrInheritedCompliance = GetDeclaredOrInheritedCompliance(symbol);
			VisitTypeOrMember(symbol, declaredOrInheritedCompliance);
		}
	}

	public override void VisitField(FieldSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (!DoNotVisit(symbol))
		{
			Compliance declaredOrInheritedCompliance = GetDeclaredOrInheritedCompliance(symbol);
			if (VisitTypeOrMember(symbol, declaredOrInheritedCompliance) && IsTrue(declaredOrInheritedCompliance) && symbol.IsVolatile)
			{
				AddDiagnostic(ErrorCode.WRN_CLS_VolatileField, symbol.Locations[0], symbol);
			}
		}
	}

	private bool VisitTypeOrMember(Symbol symbol, Compliance compliance)
	{
		SymbolKind kind = symbol.Kind;
		if (!CheckForDeclarationWithoutAssemblyDeclaration(symbol, compliance))
		{
			return false;
		}
		bool flag = IsTrue(compliance);
		bool flag2 = IsAccessibleOutsideAssembly(symbol);
		if (flag2)
		{
			if (flag)
			{
				CheckName(symbol);
				CheckForCompliantWithinNonCompliant(symbol);
				CheckReturnTypeCompliance(symbol);
				if (symbol.Kind == SymbolKind.NamedType)
				{
					CheckMemberDistinctness((NamedTypeSymbol)symbol);
				}
			}
			else if (GetDeclaredOrInheritedCompliance(symbol.ContainingAssembly) == Compliance.DeclaredTrue && IsTrue(GetInheritedCompliance(symbol)))
			{
				CheckForNonCompliantAbstractMember(symbol);
			}
		}
		else if (IsDeclared(compliance))
		{
			AddDiagnostic(ErrorCode.WRN_CLS_MeaninglessOnPrivateType, symbol.Locations[0], symbol);
			return false;
		}
		if (flag)
		{
			CheckForAttributeWithArrayArgument(symbol);
		}
		switch (kind)
		{
		case SymbolKind.NamedType:
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
			if (namedTypeSymbol.TypeKind == TypeKind.Delegate)
			{
				MethodSymbol delegateInvokeMethod = namedTypeSymbol.DelegateInvokeMethod;
				CheckForMeaninglessOnParameter(delegateInvokeMethod.Parameters);
				CheckForMeaninglessOnReturn(delegateInvokeMethod);
			}
			break;
		}
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)symbol;
			CheckForMeaninglessOnParameter(methodSymbol.Parameters);
			CheckForMeaninglessOnReturn(methodSymbol);
			break;
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)symbol;
			CheckForMeaninglessOnParameter(propertySymbol.Parameters);
			break;
		}
		}
		return flag2;
	}

	private void CheckForNonCompliantAbstractMember(Symbol symbol)
	{
		NamedTypeSymbol containingType = symbol.ContainingType;
		if ((object)containingType != null && containingType.IsInterface)
		{
			AddDiagnostic(ErrorCode.WRN_CLS_BadInterfaceMember, symbol.Locations[0], symbol);
		}
		else if (symbol.IsAbstract && symbol.Kind != SymbolKind.NamedType)
		{
			AddDiagnostic(ErrorCode.WRN_CLS_NoAbstractMembers, symbol.Locations[0], symbol);
		}
	}

	private void CheckBaseTypeCompliance(NamedTypeSymbol symbol)
	{
		if (symbol.IsInterface)
		{
			foreach (NamedTypeSymbol item in symbol.InterfacesNoUseSiteDiagnostics())
			{
				if (!IsCompliantType(item, symbol))
				{
					AddDiagnostic(ErrorCode.WRN_CLS_BadInterface, symbol.Locations[0], symbol, item);
				}
			}
		}
		else
		{
			NamedTypeSymbol namedTypeSymbol = symbol.EnumUnderlyingType ?? symbol.BaseTypeNoUseSiteDiagnostics;
			if ((object)namedTypeSymbol != null && !IsCompliantType(namedTypeSymbol, symbol))
			{
				AddDiagnostic(ErrorCode.WRN_CLS_BadBase, symbol.Locations[0], symbol, namedTypeSymbol);
			}
		}
	}

	private void CheckForCompliantWithinNonCompliant(Symbol symbol)
	{
		NamedTypeSymbol containingType = symbol.ContainingType;
		if ((object)containingType != null && !IsTrue(GetDeclaredOrInheritedCompliance(containingType)))
		{
			AddDiagnostic(ErrorCode.WRN_CLS_IllegalTrueInFalse, symbol.Locations[0], symbol, containingType);
		}
	}

	private void CheckTypeParameterCompliance(ImmutableArray<TypeParameterSymbol> typeParameters, NamedTypeSymbol context)
	{
		foreach (TypeParameterSymbol item in typeParameters)
		{
			foreach (TypeSymbol constraintTypesNoUseSiteDiagnostic in item.ConstraintTypesNoUseSiteDiagnostics)
			{
				if (!IsCompliantType(constraintTypesNoUseSiteDiagnostic, context))
				{
					AddDiagnostic(ErrorCode.WRN_CLS_BadTypeVar, item.Locations[0], constraintTypesNoUseSiteDiagnostic);
				}
			}
		}
	}

	private void CheckParameterCompliance(ImmutableArray<ParameterSymbol> parameters, NamedTypeSymbol context)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (!IsCompliantType(item.Type, context))
			{
				AddDiagnostic(ErrorCode.WRN_CLS_BadArgType, item.Locations[0], item.Type);
			}
		}
	}

	private void CheckForAttributeWithArrayArgument(Symbol symbol)
	{
		CheckForAttributeWithArrayArgumentInternal(symbol.GetAttributes());
		if (symbol.Kind == SymbolKind.Method)
		{
			CheckForAttributeWithArrayArgumentInternal(((MethodSymbol)symbol).GetReturnTypeAttributes());
		}
	}

	private void CheckForAttributeWithArrayArgumentInternal(ImmutableArray<CSharpAttributeData> attributes)
	{
		foreach (CSharpAttributeData item in attributes)
		{
			foreach (TypedConstant constructorArgument in item.ConstructorArguments)
			{
				if (constructorArgument.Type.TypeKind == TypeKind.Array && TryGetAttributeWarningLocation(item, out var location))
				{
					AddDiagnostic(ErrorCode.WRN_CLS_ArrayArgumentToAttribute, location);
					return;
				}
			}
			foreach (KeyValuePair<string, TypedConstant> namedArgument in item.NamedArguments)
			{
				if (namedArgument.Value.Type.TypeKind == TypeKind.Array && TryGetAttributeWarningLocation(item, out var location2))
				{
					AddDiagnostic(ErrorCode.WRN_CLS_ArrayArgumentToAttribute, location2);
					return;
				}
			}
			if ((object)item.AttributeConstructor == null)
			{
				continue;
			}
			foreach (TypeSymbol parameterType in item.AttributeConstructor.ParameterTypes)
			{
				if (parameterType.TypeKind == TypeKind.Array && TryGetAttributeWarningLocation(item, out var location3))
				{
					AddDiagnostic(ErrorCode.WRN_CLS_ArrayArgumentToAttribute, location3);
					return;
				}
			}
		}
	}

	private bool TryGetAttributeWarningLocation(CSharpAttributeData attribute, out Location location)
	{
		SyntaxReference applicationSyntaxReference = attribute.ApplicationSyntaxReference;
		if (applicationSyntaxReference == null && _filterTree == null)
		{
			location = NoLocation.Singleton;
			return true;
		}
		if (_filterTree == null || (applicationSyntaxReference != null && applicationSyntaxReference.SyntaxTree == _filterTree))
		{
			location = new SourceLocation(applicationSyntaxReference);
			return true;
		}
		location = null;
		return false;
	}

	private void CheckForMeaninglessOnParameter(ImmutableArray<ParameterSymbol> parameters)
	{
		if (parameters.IsEmpty)
		{
			return;
		}
		int num = 0;
		Symbol containingSymbol = parameters[0].ContainingSymbol;
		if (containingSymbol.Kind == SymbolKind.Method)
		{
			Symbol associatedSymbol = ((MethodSymbol)containingSymbol).AssociatedSymbol;
			if ((object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Property)
			{
				num = ((PropertySymbol)associatedSymbol).ParameterCount;
			}
		}
		for (int i = num; i < parameters.Length; i++)
		{
			if (TryGetClsComplianceAttributeLocation(parameters[i].GetAttributes(), parameters[i], out var attributeLocation))
			{
				AddDiagnostic(ErrorCode.WRN_CLS_MeaninglessOnParam, attributeLocation);
			}
		}
	}

	private void CheckForMeaninglessOnReturn(MethodSymbol method)
	{
		if (TryGetClsComplianceAttributeLocation(method.GetReturnTypeAttributes(), method, out var attributeLocation))
		{
			AddDiagnostic(ErrorCode.WRN_CLS_MeaninglessOnReturn, attributeLocation);
		}
	}

	private void CheckReturnTypeCompliance(Symbol symbol)
	{
		ErrorCode code;
		TypeSymbol type;
		switch (symbol.Kind)
		{
		case SymbolKind.Field:
			code = ErrorCode.WRN_CLS_BadFieldPropType;
			type = ((FieldSymbol)symbol).Type;
			break;
		case SymbolKind.Property:
			code = ErrorCode.WRN_CLS_BadFieldPropType;
			type = ((PropertySymbol)symbol).Type;
			break;
		case SymbolKind.Event:
			code = ErrorCode.WRN_CLS_BadFieldPropType;
			type = ((EventSymbol)symbol).Type;
			break;
		case SymbolKind.Method:
		{
			code = ErrorCode.WRN_CLS_BadReturnType;
			MethodSymbol methodSymbol = (MethodSymbol)symbol;
			type = methodSymbol.ReturnType;
			if (methodSymbol.MethodKind == MethodKind.DelegateInvoke)
			{
				symbol = methodSymbol.ContainingType;
			}
			break;
		}
		case SymbolKind.NamedType:
			symbol = ((NamedTypeSymbol)symbol).DelegateInvokeMethod;
			if ((object)symbol == null)
			{
				return;
			}
			goto case SymbolKind.Method;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
		if (!IsCompliantType(type, symbol.ContainingType))
		{
			AddDiagnostic(code, symbol.Locations[0], symbol);
		}
	}

	private bool TryGetClsComplianceAttributeLocation(ImmutableArray<CSharpAttributeData> attributes, Symbol targetSymbol, out Location attributeLocation)
	{
		foreach (CSharpAttributeData item in attributes)
		{
			if (item.IsTargetAttribute(targetSymbol, AttributeDescription.CLSCompliantAttribute) && TryGetAttributeWarningLocation(item, out attributeLocation))
			{
				return true;
			}
		}
		attributeLocation = null;
		return false;
	}

	private bool CheckForDeclarationWithoutAssemblyDeclaration(Symbol symbol, Compliance compliance)
	{
		if (IsDeclared(compliance) && !IsDeclared(GetDeclaredOrInheritedCompliance(symbol.ContainingAssembly)))
		{
			ErrorCode code = (IsTrue(compliance) ? ErrorCode.WRN_CLS_AssemblyNotCLS : ErrorCode.WRN_CLS_AssemblyNotCLS2);
			AddDiagnostic(code, symbol.Locations[0], symbol);
			return false;
		}
		return true;
	}

	private void CheckMemberDistinctness(NamespaceOrTypeSymbol symbol)
	{
		MultiDictionary<string, Symbol> multiDictionary = new MultiDictionary<string, Symbol>(CaseInsensitiveComparison.Comparer);
		if (symbol.Kind != SymbolKind.Namespace)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
			foreach (NamedTypeSymbol interfacesAndTheirBaseInterfacesNoUseSiteDiagnostic in namedTypeSymbol.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics)
			{
				if (!IsAccessibleOutsideAssembly(interfacesAndTheirBaseInterfacesNoUseSiteDiagnostic))
				{
					continue;
				}
				foreach (Symbol item in interfacesAndTheirBaseInterfacesNoUseSiteDiagnostic.GetMembersUnordered())
				{
					if (IsAccessibleIfContainerIsAccessible(item) && (!item.IsOverride || (item.Kind != SymbolKind.Method && item.Kind != SymbolKind.Property)))
					{
						multiDictionary.Add(item.Name, item);
					}
				}
			}
			NamedTypeSymbol baseTypeNoUseSiteDiagnostics = namedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
			while ((object)baseTypeNoUseSiteDiagnostics != null)
			{
				foreach (Symbol item2 in baseTypeNoUseSiteDiagnostics.GetMembersUnordered())
				{
					if (IsAccessibleOutsideAssembly(item2) && IsTrue(GetDeclaredOrInheritedCompliance(item2)) && (!item2.IsOverride || (item2.Kind != SymbolKind.Method && item2.Kind != SymbolKind.Property)))
					{
						multiDictionary.Add(item2.Name, item2);
					}
				}
				baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
			}
		}
		foreach (Symbol member in symbol.GetMembers())
		{
			if (!DoNotVisit(member) && IsAccessibleIfContainerIsAccessible(member) && IsTrue(GetDeclaredOrInheritedCompliance(member)) && !member.IsOverride)
			{
				string name = member.Name;
				MultiDictionary<string, Symbol>.ValueSet sameNameSymbols = multiDictionary[name];
				if (sameNameSymbols.Count > 0)
				{
					CheckSymbolDistinctness(member, name, sameNameSymbols);
				}
				multiDictionary.Add(name, member);
			}
		}
	}

	private void CheckSymbolDistinctness(Symbol symbol, string symbolName, MultiDictionary<string, Symbol>.ValueSet sameNameSymbols)
	{
		bool flag = symbol.Kind == SymbolKind.Method || symbol.Kind == SymbolKind.Property;
		foreach (Symbol item in sameNameSymbols)
		{
			if (item.Name != symbolName && (!flag || item.Kind != symbol.Kind))
			{
				AddDiagnostic(ErrorCode.WRN_CLS_BadIdentifierCase, symbol.Locations[0], symbol);
				return;
			}
		}
		if (!flag)
		{
			return;
		}
		foreach (Symbol item2 in sameNameSymbols)
		{
			if (symbol.Kind == item2.Kind && !symbol.IsAccessor() && !item2.IsAccessor() && TryGetCollisionErrorCode(symbol, item2, out var code))
			{
				AddDiagnostic(code, symbol.Locations[0], symbol);
				break;
			}
			if (item2.Name != symbolName)
			{
				AddDiagnostic(ErrorCode.WRN_CLS_BadIdentifierCase, symbol.Locations[0], symbol);
				break;
			}
		}
	}

	private void CheckName(Symbol symbol)
	{
		if (symbol.CanBeReferencedByName && !symbol.IsOverride)
		{
			string name = symbol.Name;
			if (name.Length > 0 && name[0] == '_')
			{
				AddDiagnostic(ErrorCode.WRN_CLS_BadIdentifier, symbol.Locations[0], name);
			}
		}
	}

	private bool DoNotVisit(Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Namespace)
		{
			return false;
		}
		if (symbol.DeclaringCompilation == _compilation && !symbol.IsImplicitlyDeclared)
		{
			return IsSyntacticallyFilteredOut(symbol);
		}
		return true;
	}

	private bool IsSyntacticallyFilteredOut(Symbol symbol)
	{
		if (_filterTree != null)
		{
			return !symbol.IsDefinedInSourceTree(_filterTree, _filterSpanWithinTree);
		}
		return false;
	}

	private bool IsCompliantType(TypeSymbol type, NamedTypeSymbol context)
	{
		switch (type.TypeKind)
		{
		case TypeKind.Array:
			return IsCompliantType(((ArrayTypeSymbol)type).ElementType, context);
		case TypeKind.Dynamic:
			return true;
		case TypeKind.Pointer:
			return false;
		case TypeKind.Error:
		case TypeKind.TypeParameter:
			return true;
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Interface:
		case TypeKind.Struct:
		case TypeKind.Submission:
			return IsCompliantType((NamedTypeSymbol)type, context);
		default:
			throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
		}
	}

	private bool IsCompliantType(NamedTypeSymbol type, NamedTypeSymbol context)
	{
		switch (type.SpecialType)
		{
		case SpecialType.System_UIntPtr:
		case SpecialType.System_TypedReference:
			return false;
		case SpecialType.System_SByte:
		case SpecialType.System_UInt16:
		case SpecialType.System_UInt32:
		case SpecialType.System_UInt64:
			return false;
		default:
			if (type.TypeKind == TypeKind.Error)
			{
				return true;
			}
			if (!IsTrue(GetDeclaredOrInheritedCompliance(type.OriginalDefinition)))
			{
				return false;
			}
			if (type.IsTupleType)
			{
				return IsCompliantType(type.TupleUnderlyingType, context);
			}
			foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in type.TypeArgumentsNoUseSiteDiagnostics)
			{
				if (!IsCompliantType(typeArgumentsNoUseSiteDiagnostic, context))
				{
					return false;
				}
			}
			return !IsInaccessibleBecauseOfConstruction(type, context);
		}
	}

	private static bool IsInaccessibleBecauseOfConstruction(NamedTypeSymbol type, NamedTypeSymbol context)
	{
		bool flag = type.DeclaredAccessibility.HasProtected();
		bool flag2 = false;
		Dictionary<NamedTypeSymbol, NamedTypeSymbol> dictionary = null;
		NamedTypeSymbol containingType = type.ContainingType;
		while ((object)containingType != null)
		{
			if (dictionary == null)
			{
				dictionary = new Dictionary<NamedTypeSymbol, NamedTypeSymbol>();
			}
			flag = flag || containingType.DeclaredAccessibility.HasProtected();
			flag2 = flag2 || containingType.Arity > 0;
			dictionary.Add(containingType.OriginalDefinition, containingType);
			containingType = containingType.ContainingType;
		}
		if (!flag || !flag2 || dictionary == null)
		{
			return false;
		}
		while ((object)context != null)
		{
			NamedTypeSymbol namedTypeSymbol = context;
			while ((object)namedTypeSymbol != null)
			{
				if (dictionary.TryGetValue(namedTypeSymbol.OriginalDefinition, out var value))
				{
					return value != namedTypeSymbol;
				}
				namedTypeSymbol = namedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
			}
			context = context.ContainingType;
		}
		return false;
	}

	private Compliance GetDeclaredOrInheritedCompliance(Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Namespace)
		{
			return GetDeclaredOrInheritedCompliance(symbol.ContainingAssembly);
		}
		if (symbol.Kind == SymbolKind.Method)
		{
			Symbol associatedSymbol = ((MethodSymbol)symbol).AssociatedSymbol;
			if ((object)associatedSymbol != null)
			{
				return GetDeclaredOrInheritedCompliance(associatedSymbol);
			}
		}
		if (_declaredOrInheritedCompliance.TryGetValue(symbol, out var value))
		{
			return value;
		}
		value = (Compliance)(((int?)(!GetDeclaredCompliance(symbol, out var _))) ?? ((symbol.Kind != SymbolKind.Assembly) ? (IsTrue(GetInheritedCompliance(symbol)) ? 2 : 3) : 4));
		if (symbol.Kind != SymbolKind.Assembly && symbol.Kind != SymbolKind.NamedType)
		{
			return value;
		}
		return _declaredOrInheritedCompliance.GetOrAdd(symbol, value);
	}

	private Compliance GetInheritedCompliance(Symbol symbol)
	{
		Symbol symbol2 = (Symbol)(((object)symbol.ContainingType) ?? ((object)symbol.ContainingAssembly));
		return GetDeclaredOrInheritedCompliance(symbol2);
	}

	private bool? GetDeclaredCompliance(Symbol symbol, out Location attributeLocation)
	{
		attributeLocation = null;
		foreach (CSharpAttributeData attribute in symbol.GetAttributes())
		{
			if (!attribute.IsTargetAttribute(symbol, AttributeDescription.CLSCompliantAttribute))
			{
				continue;
			}
			NamedTypeSymbol attributeClass = attribute.AttributeClass;
			if ((object)attributeClass != null)
			{
				DiagnosticInfo useSiteDiagnostic = attributeClass.GetUseSiteDiagnostic();
				if (useSiteDiagnostic != null)
				{
					Location location = (symbol.Locations.IsEmpty ? NoLocation.Singleton : symbol.Locations[0]);
					_diagnostics.Enqueue(new CSDiagnostic(useSiteDiagnostic, location));
					if (useSiteDiagnostic.Severity >= DiagnosticSeverity.Error)
					{
						continue;
					}
				}
			}
			if (!attribute.HasErrors)
			{
				if (!TryGetAttributeWarningLocation(attribute, out attributeLocation))
				{
					attributeLocation = null;
				}
				return (bool)attribute.CommonConstructorArguments[0].Value;
			}
		}
		return null;
	}

	private static bool IsAccessibleOutsideAssembly(Symbol symbol)
	{
		while ((object)symbol != null && !IsImplicitClass(symbol))
		{
			if (!IsAccessibleIfContainerIsAccessible(symbol))
			{
				return false;
			}
			symbol = symbol.ContainingType;
		}
		return true;
	}

	private static bool IsAccessibleIfContainerIsAccessible(Symbol symbol)
	{
		switch (symbol.DeclaredAccessibility)
		{
		case Accessibility.Protected:
		case Accessibility.ProtectedOrInternal:
		case Accessibility.Public:
			return true;
		case Accessibility.Private:
		case Accessibility.ProtectedAndInternal:
		case Accessibility.Internal:
			return false;
		case Accessibility.NotApplicable:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.DeclaredAccessibility);
		}
	}

	private void AddDiagnostic(ErrorCode code, Location location)
	{
		CSDiagnostic item = new CSDiagnostic(new CSDiagnosticInfo(code), location);
		_diagnostics.Enqueue(item);
	}

	private void AddDiagnostic(ErrorCode code, Location location, params object[] args)
	{
		CSDiagnostic item = new CSDiagnostic(new CSDiagnosticInfo(code, args), location);
		_diagnostics.Enqueue(item);
	}

	private static bool IsImplicitClass(Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.NamedType)
		{
			return ((NamedTypeSymbol)symbol).IsImplicitClass;
		}
		return false;
	}

	private static bool IsTrue(Compliance compliance)
	{
		switch (compliance)
		{
		case Compliance.DeclaredTrue:
		case Compliance.InheritedTrue:
			return true;
		case Compliance.DeclaredFalse:
		case Compliance.InheritedFalse:
		case Compliance.ImpliedFalse:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(compliance);
		}
	}

	private static bool IsDeclared(Compliance compliance)
	{
		switch (compliance)
		{
		case Compliance.DeclaredTrue:
		case Compliance.DeclaredFalse:
			return true;
		case Compliance.InheritedTrue:
		case Compliance.InheritedFalse:
		case Compliance.ImpliedFalse:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(compliance);
		}
	}

	private static bool TryGetCollisionErrorCode(Symbol x, Symbol y, out ErrorCode code)
	{
		code = ErrorCode.Void;
		ImmutableArray<TypeSymbol> parameterTypes;
		ImmutableArray<RefKind> parameterRefKinds;
		ImmutableArray<TypeSymbol> parameterTypes2;
		ImmutableArray<RefKind> parameterRefKinds2;
		switch (x.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)x;
			parameterTypes = methodSymbol.ParameterTypes;
			parameterRefKinds = methodSymbol.ParameterRefKinds;
			MethodSymbol methodSymbol2 = (MethodSymbol)y;
			parameterTypes2 = methodSymbol2.ParameterTypes;
			parameterRefKinds2 = methodSymbol2.ParameterRefKinds;
			break;
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)x;
			parameterTypes = propertySymbol.ParameterTypes;
			parameterRefKinds = propertySymbol.ParameterRefKinds;
			PropertySymbol propertySymbol2 = (PropertySymbol)y;
			parameterTypes2 = propertySymbol2.ParameterTypes;
			parameterRefKinds2 = propertySymbol2.ParameterRefKinds;
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(x.Kind);
		}
		int length = parameterTypes.Length;
		if (parameterTypes2.Length != length)
		{
			return false;
		}
		bool flag = parameterRefKinds.IsDefault != parameterRefKinds2.IsDefault;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < length; i++)
		{
			TypeSymbol typeSymbol = parameterTypes[i];
			TypeSymbol typeSymbol2 = parameterTypes2[i];
			TypeKind typeKind = typeSymbol.TypeKind;
			if (typeSymbol2.TypeKind != typeKind)
			{
				return false;
			}
			if (typeKind == TypeKind.Array)
			{
				ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)typeSymbol;
				ArrayTypeSymbol arrayTypeSymbol2 = (ArrayTypeSymbol)typeSymbol2;
				flag2 = flag2 || arrayTypeSymbol.Rank != arrayTypeSymbol2.Rank;
				bool flag4 = arrayTypeSymbol.ElementType != arrayTypeSymbol2.ElementType;
				if (IsArrayOfArrays(arrayTypeSymbol) || IsArrayOfArrays(arrayTypeSymbol2))
				{
					flag3 |= flag4;
				}
				else if (flag4)
				{
					return false;
				}
			}
			else if (typeSymbol != typeSymbol2)
			{
				return false;
			}
			if (!parameterRefKinds.IsDefault)
			{
				flag = flag || parameterRefKinds[i] != parameterRefKinds2[i];
			}
		}
		ref ErrorCode reference = ref code;
		int num;
		if (flag3)
		{
			num = 3007;
		}
		else if (flag2)
		{
			num = 3006;
		}
		else
		{
			num = (flag ? 3006 : (-2));
		}
		reference = (ErrorCode)num;
		return code != ErrorCode.Void;
	}

	private static bool IsArrayOfArrays(ArrayTypeSymbol arrayType)
	{
		return arrayType.ElementType.Kind == SymbolKind.ArrayType;
	}
}
