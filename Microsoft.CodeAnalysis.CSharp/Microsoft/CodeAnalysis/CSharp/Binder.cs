using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit.NoPia;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class Binder
{
	protected enum OverflowChecks
	{
		Implicit,
		Disabled,
		Enabled
	}

	private class QueryTranslationState
	{
		public BoundExpression fromExpression;

		public RangeVariableSymbol rangeVariable;

		public readonly Stack<QueryClauseSyntax> clauses = new Stack<QueryClauseSyntax>();

		public SelectOrGroupClauseSyntax selectOrGroup;

		public readonly Dictionary<RangeVariableSymbol, ArrayBuilder<string>> allRangeVariables = new Dictionary<RangeVariableSymbol, ArrayBuilder<string>>();

		private int _nextTransparentIdentifierNumber;

		public static RangeVariableMap RangeVariableMap(params RangeVariableSymbol[] parameters)
		{
			RangeVariableMap rangeVariableMap = new RangeVariableMap();
			foreach (RangeVariableSymbol key in parameters)
			{
				rangeVariableMap.Add(key, ImmutableArray<string>.Empty);
			}
			return rangeVariableMap;
		}

		public RangeVariableMap RangeVariableMap()
		{
			RangeVariableMap rangeVariableMap = new RangeVariableMap();
			foreach (RangeVariableSymbol key in allRangeVariables.Keys)
			{
				rangeVariableMap.Add(key, allRangeVariables[key].ToImmutable());
			}
			return rangeVariableMap;
		}

		internal RangeVariableSymbol AddRangeVariable(Binder binder, SyntaxToken identifier, DiagnosticBag diagnostics)
		{
			string valueText = identifier.ValueText;
			RangeVariableSymbol rangeVariableSymbol = new RangeVariableSymbol(valueText, binder.ContainingMemberOrLambda, identifier.GetLocation());
			bool flag = false;
			foreach (RangeVariableSymbol key in allRangeVariables.Keys)
			{
				if (key.Name == valueText)
				{
					diagnostics.Add(ErrorCode.ERR_QueryDuplicateRangeVariable, identifier.GetLocation(), valueText);
					flag = true;
				}
			}
			if (!flag)
			{
				new LocalScopeBinder(binder).ValidateDeclarationNameConflictsInScope(rangeVariableSymbol, diagnostics);
			}
			allRangeVariables.Add(rangeVariableSymbol, ArrayBuilder<string>.GetInstance());
			return rangeVariableSymbol;
		}

		internal void AddTransparentIdentifier(string name)
		{
			foreach (ArrayBuilder<string> value in allRangeVariables.Values)
			{
				value.Add(name);
			}
		}

		internal string TransparentRangeVariableName()
		{
			return "<>h__TransparentIdentifier" + _nextTransparentIdentifierNumber++;
		}

		internal RangeVariableSymbol TransparentRangeVariable(Binder binder)
		{
			return new RangeVariableSymbol(TransparentRangeVariableName(), binder.ContainingMemberOrLambda, null, isTransparent: true);
		}

		public void Clear()
		{
			fromExpression = null;
			rangeVariable = null;
			selectOrGroup = null;
			foreach (ArrayBuilder<string> value in allRangeVariables.Values)
			{
				value.Free();
			}
			allRangeVariables.Clear();
			clauses.Clear();
		}

		public void Free()
		{
			Clear();
		}
	}

	private delegate BoundBlock LambdaBodyFactory(LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag diagnostics);

	private class QueryUnboundLambdaState : UnboundLambdaState
	{
		private readonly ImmutableArray<RangeVariableSymbol> _parameters;

		private readonly LambdaBodyFactory _bodyFactory;

		private readonly RangeVariableMap _rangeVariableMap;

		public override bool HasSignature => true;

		public override bool HasExplicitlyTypedParameterList => false;

		public override int ParameterCount => _parameters.Length;

		public override bool IsAsync => false;

		public override MessageID MessageID => MessageID.IDS_FeatureQueryExpression;

		public QueryUnboundLambdaState(Binder binder, RangeVariableMap rangeVariableMap, ImmutableArray<RangeVariableSymbol> parameters, LambdaBodyFactory bodyFactory)
			: base(binder, null)
		{
			_parameters = parameters;
			_rangeVariableMap = rangeVariableMap;
			_bodyFactory = bodyFactory;
		}

		public override string ParameterName(int index)
		{
			return _parameters[index].Name;
		}

		public override RefKind RefKind(int index)
		{
			return Microsoft.CodeAnalysis.RefKind.None;
		}

		public override Location ParameterLocation(int index)
		{
			return _parameters[index].Locations[0];
		}

		public override TypeSymbol ParameterType(int index)
		{
			throw new ArgumentException();
		}

		public override void GenerateAnonymousFunctionConversionError(DiagnosticBag diagnostics, TypeSymbol targetType)
		{
			base.GenerateAnonymousFunctionConversionError(diagnostics, targetType);
		}

		public override Binder ParameterBinder(LambdaSymbol lambdaSymbol, Binder binder)
		{
			return new WithQueryLambdaParametersBinder(lambdaSymbol, _rangeVariableMap, binder);
		}

		protected override BoundBlock BindLambdaBody(LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag diagnostics)
		{
			return _bodyFactory(lambdaSymbol, lambdaBodyBinder, diagnostics);
		}
	}

	private class RangeVariableMap : Dictionary<RangeVariableSymbol, ImmutableArray<string>>
	{
	}

	[Flags]
	internal enum BindValueKind : ushort
	{
		RValue = 4,
		Assignable = 8,
		RefersToLocation = 0x10,
		RefAssignable = 0x20,
		RValueOrMethodGroup = 5,
		CompoundAssignment = RValue | Assignable,
		IncrementDecrement = RValueOrMethodGroup | Assignable,
		ReadonlyRef = RValue | RefersToLocation,
		AddressOf = RValueOrMethodGroup | RefersToLocation,
		FixedReceiver = 0x16,
		RefOrOut = CompoundAssignment | RefersToLocation,
		RefReturn = IncrementDecrement | RefersToLocation
	}

	private sealed class WithQueryLambdaParametersBinder : WithLambdaParametersBinder
	{
		private readonly RangeVariableMap _rangeVariableMap;

		private readonly MultiDictionary<string, RangeVariableSymbol> _parameterMap;

		public WithQueryLambdaParametersBinder(LambdaSymbol lambdaSymbol, RangeVariableMap rangeVariableMap, Binder next)
			: base(lambdaSymbol, next)
		{
			_rangeVariableMap = rangeVariableMap;
			_parameterMap = new MultiDictionary<string, RangeVariableSymbol>();
			foreach (RangeVariableSymbol key in rangeVariableMap.Keys)
			{
				_parameterMap.Add(key.Name, key);
			}
		}

		protected override BoundExpression BindRangeVariable(SimpleNameSyntax node, RangeVariableSymbol qv, DiagnosticBag diagnostics)
		{
			if (_rangeVariableMap.TryGetValue(qv, out var value))
			{
				BoundExpression boundExpression;
				if (value.IsEmpty)
				{
					boundExpression = new BoundParameter(node, parameterMap[qv.Name].Single());
				}
				else
				{
					boundExpression = new BoundParameter(node, lambdaSymbol.Parameters[0]);
					for (int num = value.Length - 1; num >= 0; num--)
					{
						boundExpression.WasCompilerGenerated = true;
						string name = value[num];
						boundExpression = SelectField(node, boundExpression, name, diagnostics);
					}
				}
				return new BoundRangeVariable(node, qv, boundExpression, boundExpression.Type);
			}
			return base.BindRangeVariable(node, qv, diagnostics);
		}

		private BoundExpression SelectField(SimpleNameSyntax node, BoundExpression receiver, string name, DiagnosticBag diagnostics)
		{
			NamedTypeSymbol namedTypeSymbol = receiver.Type as NamedTypeSymbol;
			if ((object)namedTypeSymbol == null || !namedTypeSymbol.IsAnonymousType)
			{
				CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_UnsupportedTransparentIdentifierAccess, name, receiver.ExpressionSymbol ?? namedTypeSymbol);
				TypeSymbol type = receiver.Type;
				if ((object)type == null || !type.IsErrorType())
				{
					Error(diagnostics, cSDiagnosticInfo, node);
				}
				return new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray.Create(receiver.ExpressionSymbol), ImmutableArray.Create(receiver), new ExtendedErrorTypeSymbol(Compilation, "", 0, cSDiagnosticInfo));
			}
			LookupResult instance = LookupResult.GetInstance();
			LookupOptions options = LookupOptions.MustBeInstance;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			LookupMembersWithFallback(instance, receiver.Type, name, 0, ref useSiteDiagnostics, null, options);
			diagnostics.Add(node, useSiteDiagnostics);
			BoundExpression result = BindMemberOfType(node, node, name, 0, indexed: false, receiver, default, default, instance, BoundMethodGroupFlags.None, diagnostics);
			instance.Free();
			return result;
		}

		internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
		{
			if ((options & LookupOptions.NamespaceAliasesOnly) != 0)
			{
				return;
			}
			foreach (RangeVariableSymbol item in _parameterMap[name])
			{
				result.MergeEqual(originalBinder.CheckViability(item, arity, options, null, diagnose, ref useSiteDiagnostics));
			}
		}

		protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
		{
			if (!options.CanConsiderMembers())
			{
				return;
			}
			foreach (KeyValuePair<string, MultiDictionary<string, RangeVariableSymbol>.ValueSet> item in _parameterMap)
			{
				result.AddSymbol(null, item.Key, 0);
			}
		}
	}

	private struct AttributeExpressionVisitor(Binder binder)
	{
		private readonly Binder _binder = binder;

		public ImmutableArray<TypedConstant> VisitArguments(ImmutableArray<BoundExpression> arguments, DiagnosticBag diagnostics, ref bool attrHasErrors, bool parentHasErrors = false)
		{
			ImmutableArray<TypedConstant> result = ImmutableArray<TypedConstant>.Empty;
			int length = arguments.Length;
			if (length > 0)
			{
				ArrayBuilder<TypedConstant> instance = ArrayBuilder<TypedConstant>.GetInstance(length);
				foreach (BoundExpression item in arguments)
				{
					bool curArgumentHasErrors = parentHasErrors || item.HasAnyErrors;
					instance.Add(VisitExpression(item, diagnostics, ref attrHasErrors, curArgumentHasErrors));
				}
				result = instance.ToImmutableAndFree();
			}
			return result;
		}

		public ImmutableArray<KeyValuePair<string, TypedConstant>> VisitNamedArguments(ImmutableArray<BoundExpression> arguments, DiagnosticBag diagnostics, ref bool attrHasErrors)
		{
			ArrayBuilder<KeyValuePair<string, TypedConstant>> arrayBuilder = null;
			foreach (BoundExpression item in arguments)
			{
				KeyValuePair<string, TypedConstant>? keyValuePair = VisitNamedArgument(item, diagnostics, ref attrHasErrors);
				if (keyValuePair.HasValue)
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<KeyValuePair<string, TypedConstant>>.GetInstance();
					}
					arrayBuilder.Add(keyValuePair.Value);
				}
			}
			return arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
		}

		private KeyValuePair<string, TypedConstant>? VisitNamedArgument(BoundExpression argument, DiagnosticBag diagnostics, ref bool attrHasErrors)
		{
			KeyValuePair<string, TypedConstant>? result = null;
			BoundKind kind = argument.Kind;
			if (kind == BoundKind.AssignmentOperator)
			{
				BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)argument;
				switch (boundAssignmentOperator.Left.Kind)
				{
				case BoundKind.FieldAccess:
				{
					BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundAssignmentOperator.Left;
					result = new KeyValuePair<string, TypedConstant>(boundFieldAccess.FieldSymbol.Name, VisitExpression(boundAssignmentOperator.Right, diagnostics, ref attrHasErrors, argument.HasAnyErrors));
					break;
				}
				case BoundKind.PropertyAccess:
				{
					BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)boundAssignmentOperator.Left;
					result = new KeyValuePair<string, TypedConstant>(boundPropertyAccess.PropertySymbol.Name, VisitExpression(boundAssignmentOperator.Right, diagnostics, ref attrHasErrors, argument.HasAnyErrors));
					break;
				}
				}
			}
			return result;
		}

		private TypedConstant VisitExpression(BoundExpression node, DiagnosticBag diagnostics, ref bool attrHasErrors, bool curArgumentHasErrors)
		{
			TypedConstantKind attributeParameterTypedConstantKind = node.Type.GetAttributeParameterTypedConstantKind(_binder.Compilation);
			return VisitExpression(node, attributeParameterTypedConstantKind, diagnostics, ref attrHasErrors, curArgumentHasErrors || attributeParameterTypedConstantKind == TypedConstantKind.Error);
		}

		private TypedConstant VisitExpression(BoundExpression node, TypedConstantKind typedConstantKind, DiagnosticBag diagnostics, ref bool attrHasErrors, bool curArgumentHasErrors)
		{
			ConstantValue constantValue = node.ConstantValue;
			if (constantValue != null)
			{
				if (constantValue.IsBad)
				{
					typedConstantKind = TypedConstantKind.Error;
				}
				return CreateTypedConstant(node, typedConstantKind, diagnostics, ref attrHasErrors, curArgumentHasErrors, node.ConstantValue.Value);
			}
			return node.Kind switch
			{
				BoundKind.Conversion => VisitConversion((BoundConversion)node, diagnostics, ref attrHasErrors, curArgumentHasErrors), 
				BoundKind.TypeOfOperator => VisitTypeOfExpression((BoundTypeOfOperator)node, diagnostics, ref attrHasErrors, curArgumentHasErrors), 
				BoundKind.ArrayCreation => VisitArrayCreation((BoundArrayCreation)node, diagnostics, ref attrHasErrors, curArgumentHasErrors), 
				_ => CreateTypedConstant(node, TypedConstantKind.Error, diagnostics, ref attrHasErrors, curArgumentHasErrors), 
			};
		}

		private TypedConstant VisitConversion(BoundConversion node, DiagnosticBag diagnostics, ref bool attrHasErrors, bool curArgumentHasErrors)
		{
			TypeSymbol type = node.Type;
			BoundExpression operand = node.Operand;
			TypeSymbol type2 = operand.Type;
			if ((object)type != null && (object)type2 != null && (type.SpecialType == SpecialType.System_Object || (type2.IsArray() && type.IsArray() && ((ArrayTypeSymbol)type).ElementType.SpecialType == SpecialType.System_Object)))
			{
				TypedConstantKind attributeParameterTypedConstantKind = type2.GetAttributeParameterTypedConstantKind(_binder.Compilation);
				return VisitExpression(operand, attributeParameterTypedConstantKind, diagnostics, ref attrHasErrors, curArgumentHasErrors);
			}
			return CreateTypedConstant(node, TypedConstantKind.Error, diagnostics, ref attrHasErrors, curArgumentHasErrors);
		}

		private static TypedConstant VisitTypeOfExpression(BoundTypeOfOperator node, DiagnosticBag diagnostics, ref bool attrHasErrors, bool curArgumentHasErrors)
		{
			TypeSymbol type = node.SourceType.Type;
			if ((object)type != null)
			{
				SymbolKind kind = type.Kind;
				bool flag = kind != SymbolKind.TypeParameter && (type.IsUnboundGenericType() || !type.ContainsTypeParameter());
				if (!flag && !curArgumentHasErrors)
				{
					Error(diagnostics, ErrorCode.ERR_AttrArgWithTypeVars, node.Syntax, type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
					curArgumentHasErrors = true;
					attrHasErrors = true;
				}
			}
			return CreateTypedConstant(node, TypedConstantKind.Type, diagnostics, ref attrHasErrors, curArgumentHasErrors, node.SourceType.Type);
		}

		private TypedConstant VisitArrayCreation(BoundArrayCreation node, DiagnosticBag diagnostics, ref bool attrHasErrors, bool curArgumentHasErrors)
		{
			ImmutableArray<BoundExpression> bounds = node.Bounds;
			int length = bounds.Length;
			if (length > 1)
			{
				return CreateTypedConstant(node, TypedConstantKind.Error, diagnostics, ref attrHasErrors, curArgumentHasErrors);
			}
			TypedConstantKind attributeParameterTypedConstantKind = ((ArrayTypeSymbol)node.Type).GetAttributeParameterTypedConstantKind(_binder.Compilation);
			ImmutableArray<TypedConstant> arrayValue;
			if (node.InitializerOpt != null)
			{
				arrayValue = VisitArguments(node.InitializerOpt.Initializers, diagnostics, ref attrHasErrors, curArgumentHasErrors);
			}
			else if (length != 0)
			{
				arrayValue = ((!bounds[0].IsDefaultValue()) ? ImmutableArray.Create(CreateTypedConstant(node, TypedConstantKind.Error, diagnostics, ref attrHasErrors, curArgumentHasErrors)) : ImmutableArray<TypedConstant>.Empty);
			}
			else
			{
				arrayValue = ImmutableArray<TypedConstant>.Empty;
			}
			return CreateTypedConstant(node, attributeParameterTypedConstantKind, diagnostics, ref attrHasErrors, curArgumentHasErrors, null, arrayValue);
		}

		private static TypedConstant CreateTypedConstant(BoundExpression node, TypedConstantKind typedConstantKind, DiagnosticBag diagnostics, ref bool attrHasErrors, bool curArgumentHasErrors, object simpleValue = null, ImmutableArray<TypedConstant> arrayValue = default(ImmutableArray<TypedConstant>))
		{
			TypeSymbol type = node.Type;
			if (typedConstantKind != TypedConstantKind.Error && type.ContainsTypeParameter())
			{
				typedConstantKind = TypedConstantKind.Error;
			}
			switch (typedConstantKind)
			{
			case TypedConstantKind.Error:
				if (!curArgumentHasErrors)
				{
					Error(diagnostics, ErrorCode.ERR_BadAttributeArgument, node.Syntax);
					attrHasErrors = true;
				}
				return new TypedConstant(type, TypedConstantKind.Error, null);
			case TypedConstantKind.Array:
				return new TypedConstant(type, arrayValue);
			default:
				return new TypedConstant(type, typedConstantKind, simpleValue);
			}
		}
	}

	private struct AnalyzedAttributeArguments
	{
		internal readonly AnalyzedArguments ConstructorArguments;

		internal readonly ImmutableArray<BoundExpression> NamedArguments;

		internal AnalyzedAttributeArguments(AnalyzedArguments constructorArguments, ImmutableArray<BoundExpression> namedArguments)
		{
			ConstructorArguments = constructorArguments;
			NamedArguments = namedArguments;
		}
	}

	[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
	internal class DeconstructionVariable
	{
		internal readonly BoundExpression Single;

		internal readonly ArrayBuilder<DeconstructionVariable> NestedVariables;

		internal readonly CSharpSyntaxNode Syntax;

		internal bool HasNestedVariables => NestedVariables != null;

		internal DeconstructionVariable(BoundExpression variable, SyntaxNode syntax)
		{
			Single = variable;
			NestedVariables = null;
			Syntax = (CSharpSyntaxNode)syntax;
		}

		internal DeconstructionVariable(ArrayBuilder<DeconstructionVariable> variables, SyntaxNode syntax)
		{
			Single = null;
			NestedVariables = variables;
			Syntax = (CSharpSyntaxNode)syntax;
		}

		internal static void FreeDeconstructionVariables(ArrayBuilder<DeconstructionVariable> variables)
		{
			foreach (DeconstructionVariable variable in variables)
			{
				if (variable.HasNestedVariables)
				{
					FreeDeconstructionVariables(variable.NestedVariables);
				}
			}
			variables.Free();
		}

		private string GetDebuggerDisplay()
		{
			if (Single != null)
			{
				return Single.GetDebuggerDisplay();
			}
			return $"Nested variables ({NestedVariables.Count})";
		}
	}

	private sealed class BinderWithContainingMemberOrLambda : Binder
	{
		private readonly Symbol _containingMemberOrLambda;

		internal override Symbol ContainingMemberOrLambda => _containingMemberOrLambda;

		internal BinderWithContainingMemberOrLambda(Binder next, Symbol containingMemberOrLambda)
			: base(next)
		{
			_containingMemberOrLambda = containingMemberOrLambda;
		}

		internal BinderWithContainingMemberOrLambda(Binder next, BinderFlags flags, Symbol containingMemberOrLambda)
			: base(next, flags)
		{
			_containingMemberOrLambda = containingMemberOrLambda;
		}
	}

	private sealed class BinderWithConditionalReceiver : Binder
	{
		private readonly BoundExpression _receiverExpression;

		internal override BoundExpression ConditionalReceiverExpression => _receiverExpression;

		internal BinderWithConditionalReceiver(Binder next, BoundExpression receiverExpression)
			: base(next)
		{
			_receiverExpression = receiverExpression;
		}
	}

	internal struct ProcessedFieldInitializers
	{
		internal ImmutableArray<BoundInitializer> BoundInitializers { get; set; }

		internal BoundStatement LoweredInitializers { get; set; }

		internal bool HasErrors { get; set; }

		internal ImportChain FirstImportChain { get; set; }
	}

	private class ConsistentSymbolOrder : IComparer<Symbol>
	{
		public static readonly ConsistentSymbolOrder Instance = new ConsistentSymbolOrder();

		public int Compare(Symbol fst, Symbol snd)
		{
			if (snd == fst)
			{
				return 0;
			}
			if ((object)fst == null)
			{
				return -1;
			}
			if ((object)snd == null)
			{
				return 1;
			}
			if (snd.Name != fst.Name)
			{
				return string.CompareOrdinal(fst.Name, snd.Name);
			}
			if (snd.Kind != fst.Kind)
			{
				return fst.Kind - snd.Kind;
			}
			int num = ((!snd.Locations.IsDefault) ? snd.Locations.Length : 0);
			int length = fst.Locations.Length;
			if (num != length)
			{
				return num - length;
			}
			if (num == 0 && length == 0)
			{
				return Compare(fst.ContainingSymbol, snd.ContainingSymbol);
			}
			Location location = snd.Locations[0];
			Location location2 = fst.Locations[0];
			if (location.IsInSource != location2.IsInSource)
			{
				if (!location.IsInSource)
				{
					return -1;
				}
				return 1;
			}
			int num2 = Compare(fst.ContainingSymbol, snd.ContainingSymbol);
			if (!location.IsInSource)
			{
				return num2;
			}
			if (num2 == 0 && location.SourceTree == location2.SourceTree)
			{
				return location2.SourceSpan.Start - location.SourceSpan.Start;
			}
			return num2;
		}
	}

	[Flags]
	private enum BestSymbolLocation
	{
		None = 0,
		FromSourceModule = 1,
		FromAddedModule = 2,
		FromReferencedAssembly = FromSourceModule | FromAddedModule,
		FromCorLibrary = 4
	}

	[DebuggerDisplay("Location = {_location}, Index = {_index}")]
	private struct BestSymbolInfo
	{
		private readonly BestSymbolLocation _location;

		private readonly int _index;

		public int Index
		{
			get
			{
				if (!IsNone)
				{
					return _index;
				}
				return -1;
			}
		}

		public bool IsFromSourceModule => _location == BestSymbolLocation.FromSourceModule;

		public bool IsFromAddedModule => _location == BestSymbolLocation.FromAddedModule;

		public bool IsFromCompilation
		{
			get
			{
				if (_location != BestSymbolLocation.FromSourceModule)
				{
					return _location == BestSymbolLocation.FromAddedModule;
				}
				return true;
			}
		}

		public bool IsNone => _location == BestSymbolLocation.None;

		public bool IsFromCorLibrary => _location == BestSymbolLocation.FromCorLibrary;

		public BestSymbolInfo(BestSymbolLocation location, int index)
		{
			_location = location;
			_index = index;
		}

		public static bool Sort(ref BestSymbolInfo first, ref BestSymbolInfo second)
		{
			if (IsSecondLocationBetter(first._location, second._location))
			{
				BestSymbolInfo bestSymbolInfo = first;
				first = second;
				second = bestSymbolInfo;
				return true;
			}
			return false;
		}

		public static bool IsSecondLocationBetter(BestSymbolLocation firstLocation, BestSymbolLocation secondLocation)
		{
			if (firstLocation != BestSymbolLocation.None)
			{
				return firstLocation > secondLocation;
			}
			return true;
		}
	}

	private readonly Binder _next;

	internal readonly BinderFlags Flags;

	private Conversions _lazyConversions;

	private OverloadResolution _lazyOverloadResolution;

	internal const uint ExternalScope = 0u;

	internal const uint TopLevelScope = 1u;

	private const int ValueKindInsignificantBits = 2;

	private const BindValueKind ValueKindSignificantBitsMask = (BindValueKind)65532;

	private static readonly Func<PropertySymbol, bool> s_isIndexedPropertyWithNonOptionalArguments = (PropertySymbol property) =>
	{
		if (property.IsIndexer || !property.IsIndexedProperty)
		{
			return false;
		}
		ParameterSymbol parameterSymbol = property.Parameters[0];
		return !parameterSymbol.IsOptional && !parameterSymbol.IsParams;
	};

	private static readonly SymbolDisplayFormat s_propertyGroupFormat = new SymbolDisplayFormat(SymbolDisplayGlobalNamespaceStyle.Omitted, SymbolDisplayTypeQualificationStyle.NameOnly, SymbolDisplayGenericsOptions.None, SymbolDisplayMemberOptions.IncludeContainingType, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayParameterOptions.None, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

	internal const int MaxParameterListsForErrorRecovery = 10;

	private const string transparentIdentifierPrefix = "<>h__TransparentIdentifier";

	private static readonly Func<Symbol, MethodSymbol> s_toMethodSymbolFunc = (Symbol s) => (MethodSymbol)s;

	private static readonly Func<Symbol, PropertySymbol> s_toPropertySymbolFunc = (Symbol s) => (PropertySymbol)s;

	internal CSharpCompilation Compilation { get; }

	internal bool IsSemanticModelBinder => Flags.Includes(BinderFlags.SemanticModel);

	internal bool IsEarlyAttributeBinder => Flags.Includes(BinderFlags.EarlyAttributeBinding);

	protected virtual SyntaxNode EnclosingNameofArgument => null;

	private bool IsInsideNameof => EnclosingNameofArgument != null;

	protected internal Binder Next => _next;

	protected OverflowChecks CheckOverflow
	{
		get
		{
			if (!Flags.Includes(BinderFlags.CheckedRegion))
			{
				if (!Flags.Includes(BinderFlags.UncheckedRegion))
				{
					return OverflowChecks.Implicit;
				}
				return OverflowChecks.Disabled;
			}
			return OverflowChecks.Enabled;
		}
	}

	protected bool CheckOverflowAtRuntime => CheckOverflow switch
	{
		OverflowChecks.Implicit => Compilation.Options.CheckOverflow, 
		OverflowChecks.Enabled => true, 
		_ => false, 
	};

	internal bool CheckOverflowAtCompileTime => CheckOverflow != OverflowChecks.Disabled;

	internal virtual SyntaxNode ScopeDesignator => null;

	internal virtual bool IsLocalFunctionsScopeBinder => false;

	internal virtual bool IsLabelsScopeBinder => false;

	internal virtual Symbol ContainingMemberOrLambda => Next.ContainingMemberOrLambda;

	internal virtual bool IsInMethodBody => Next.IsInMethodBody;

	internal virtual bool IsDirectlyInIterator => Next.IsDirectlyInIterator;

	internal virtual bool IsIndirectlyInIterator => Next.IsIndirectlyInIterator;

	internal virtual GeneratedLabelSymbol BreakLabel => Next.BreakLabel;

	internal virtual GeneratedLabelSymbol ContinueLabel => Next.ContinueLabel;

	internal virtual ImportChain ImportChain => _next.ImportChain;

	internal virtual QuickAttributeChecker QuickAttributeChecker => _next.QuickAttributeChecker;

	internal NamedTypeSymbol ContainingType
	{
		get
		{
			Symbol containingMemberOrLambda = ContainingMemberOrLambda;
			if ((object)containingMemberOrLambda != null)
			{
				if (containingMemberOrLambda.Kind != SymbolKind.NamedType)
				{
					return containingMemberOrLambda.ContainingType;
				}
				return (NamedTypeSymbol)containingMemberOrLambda;
			}
			return null;
		}
	}

	internal bool BindingTopLevelScriptCode
	{
		get
		{
			Symbol containingMemberOrLambda = ContainingMemberOrLambda;
			return containingMemberOrLambda?.Kind switch
			{
				SymbolKind.Method => ((MethodSymbol)containingMemberOrLambda).IsScriptInitializer, 
				SymbolKind.NamedType => ((NamedTypeSymbol)containingMemberOrLambda).IsScriptClass, 
				_ => false, 
			};
		}
	}

	internal virtual ConstantFieldsInProgress ConstantFieldsInProgress => Next.ConstantFieldsInProgress;

	internal virtual ConsList<FieldSymbol> FieldsBeingBound => Next.FieldsBeingBound;

	internal virtual LocalSymbol LocalInProgress => Next.LocalInProgress;

	internal virtual BoundExpression ConditionalReceiverExpression => Next.ConditionalReceiverExpression;

	internal Conversions Conversions
	{
		get
		{
			if (_lazyConversions == null)
			{
				Interlocked.CompareExchange(ref _lazyConversions, new Conversions(this), null);
			}
			return _lazyConversions;
		}
	}

	internal OverloadResolution OverloadResolution
	{
		get
		{
			if (_lazyOverloadResolution == null)
			{
				Interlocked.CompareExchange(ref _lazyOverloadResolution, new OverloadResolution(this), null);
			}
			return _lazyOverloadResolution;
		}
	}

	private bool ContextForbidsAwait
	{
		get
		{
			if (!Flags.Includes(BinderFlags.InCatchFilter))
			{
				return Flags.Includes(BinderFlags.InLockBody);
			}
			return true;
		}
	}

	internal bool InFieldInitializer => Flags.Includes(BinderFlags.FieldInitializer);

	internal bool InParameterDefaultValue => Flags.Includes(BinderFlags.ParameterDefaultValue);

	protected bool InConstructorInitializer => Flags.Includes(BinderFlags.ConstructorInitializer);

	internal bool InAttributeArgument => Flags.Includes(BinderFlags.AttributeArgument);

	internal bool InCref => Flags.Includes(BinderFlags.Cref);

	protected bool InCrefButNotParameterOrReturnType
	{
		get
		{
			if (InCref)
			{
				return !Flags.Includes(BinderFlags.CrefParameterOrReturnType);
			}
			return false;
		}
	}

	internal virtual bool SupportsExtensionMethods => false;

	internal virtual ImmutableHashSet<Symbol> LockedOrDisposedVariables => _next.LockedOrDisposedVariables;

	internal virtual uint LocalScopeDepth => Next.LocalScopeDepth;

	internal virtual ImmutableArray<LocalSymbol> Locals => ImmutableArray<LocalSymbol>.Empty;

	internal virtual ImmutableArray<LocalFunctionSymbol> LocalFunctions => ImmutableArray<LocalFunctionSymbol>.Empty;

	internal virtual ImmutableArray<LabelSymbol> Labels => ImmutableArray<LabelSymbol>.Empty;

	private bool ShouldCheckConstraints => !Flags.Includes(BinderFlags.SuppressConstraintChecks);

	internal bool InUnsafeRegion => Flags.Includes(BinderFlags.UnsafeRegion);

	internal Binder(CSharpCompilation compilation)
	{
		Flags = compilation.Options.TopLevelBinderFlags;
		Compilation = compilation;
	}

	internal Binder(Binder next)
	{
		_next = next;
		Flags = next.Flags;
		Compilation = next.Compilation;
	}

	protected Binder(Binder next, BinderFlags flags)
	{
		_next = next;
		Flags = flags;
		Compilation = next.Compilation;
	}

	internal virtual Binder GetBinder(SyntaxNode node)
	{
		return Next.GetBinder(node);
	}

	internal virtual ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		return Next.GetDeclaredLocalsForScope(scopeDesignator);
	}

	internal virtual ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		return Next.GetDeclaredLocalFunctionsForScope(scopeDesignator);
	}

	internal virtual TypeSymbol GetIteratorElementType(YieldStatementSyntax node, DiagnosticBag diagnostics)
	{
		return Next.GetIteratorElementType(node, diagnostics);
	}

	internal virtual Imports GetImports(ConsList<Symbol> basesBeingResolved)
	{
		return _next.GetImports(basesBeingResolved);
	}

	internal static void Error(DiagnosticBag diagnostics, DiagnosticInfo info, SyntaxNode syntax)
	{
		diagnostics.Add(new CSDiagnostic(info, syntax.Location));
	}

	internal static void Error(DiagnosticBag diagnostics, DiagnosticInfo info, Location location)
	{
		diagnostics.Add(new CSDiagnostic(info, location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, CSharpSyntaxNode syntax)
	{
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(code), syntax.Location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, CSharpSyntaxNode syntax, params object[] args)
	{
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(code, args), syntax.Location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxToken token)
	{
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(code), token.GetLocation()));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxToken token, params object[] args)
	{
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(code, args), token.GetLocation()));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxNodeOrToken syntax)
	{
		Error(diagnostics, code, syntax.GetLocation());
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxNodeOrToken syntax, params object[] args)
	{
		Error(diagnostics, code, syntax.GetLocation(), args);
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, Location location)
	{
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(code), location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, Location location, params object[] args)
	{
		diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(code, args), location));
	}

	internal void ReportDiagnosticsIfObsolete(DiagnosticBag diagnostics, Symbol symbol, SyntaxNodeOrToken node, bool hasBaseReceiver)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Method:
		case SymbolKind.NamedType:
		case SymbolKind.Property:
			ReportDiagnosticsIfObsolete(diagnostics, symbol, node, hasBaseReceiver, ContainingMemberOrLambda, ContainingType, Flags);
			break;
		}
	}

	internal void ReportDiagnosticsIfObsolete(DiagnosticBag diagnostics, Conversion conversion, SyntaxNodeOrToken node, bool hasBaseReceiver)
	{
		if (conversion.IsValid && (object)conversion.Method != null)
		{
			ReportDiagnosticsIfObsolete(diagnostics, conversion.Method, node, hasBaseReceiver);
		}
	}

	internal static void ReportDiagnosticsIfObsolete(DiagnosticBag diagnostics, Symbol symbol, SyntaxNodeOrToken node, bool hasBaseReceiver, Symbol containingMember, NamedTypeSymbol containingType, BinderFlags location)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			symbol = ((MethodSymbol)symbol).ConstructedFrom;
		}
		Symbol leastOverriddenMember = symbol.GetLeastOverriddenMember(containingType);
		bool flag = hasBaseReceiver && (object)symbol != leastOverriddenMember;
		if (flag)
		{
			leastOverriddenMember.GetAttributes();
		}
		ObsoleteDiagnosticKind obsoleteDiagnosticKind = ReportDiagnosticsIfObsoleteInternal(diagnostics, leastOverriddenMember, node, containingMember, location);
		if ((obsoleteDiagnosticKind == ObsoleteDiagnosticKind.NotObsolete || obsoleteDiagnosticKind == ObsoleteDiagnosticKind.Lazy) && flag)
		{
			ReportDiagnosticsIfObsoleteInternal(diagnostics, symbol, node, containingMember, location);
		}
	}

	internal static ObsoleteDiagnosticKind ReportDiagnosticsIfObsoleteInternal(DiagnosticBag diagnostics, Symbol symbol, SyntaxNodeOrToken node, Symbol containingMember, BinderFlags location)
	{
		ObsoleteDiagnosticKind obsoleteDiagnosticKind = ObsoleteAttributeHelpers.GetObsoleteDiagnosticKind(symbol, containingMember);
		DiagnosticInfo diagnosticInfo = null;
		switch (obsoleteDiagnosticKind)
		{
		case ObsoleteDiagnosticKind.Diagnostic:
			diagnosticInfo = ObsoleteAttributeHelpers.CreateObsoleteDiagnostic(symbol, location);
			break;
		case ObsoleteDiagnosticKind.Lazy:
		case ObsoleteDiagnosticKind.LazyPotentiallySuppressed:
			diagnosticInfo = new LazyObsoleteDiagnosticInfo(symbol, containingMember, location);
			break;
		}
		if (diagnosticInfo != null)
		{
			diagnostics.Add(diagnosticInfo, node.GetLocation());
		}
		return obsoleteDiagnosticKind;
	}

	internal static bool IsSymbolAccessibleConditional(Symbol symbol, AssemblySymbol within, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return AccessCheck.IsSymbolAccessible(symbol, within, ref useSiteDiagnostics);
	}

	internal bool IsSymbolAccessibleConditional(Symbol symbol, NamedTypeSymbol within, ref HashSet<DiagnosticInfo> useSiteDiagnostics, TypeSymbol throughTypeOpt = null)
	{
		if (!Flags.Includes(BinderFlags.IgnoreAccessibility))
		{
			return AccessCheck.IsSymbolAccessible(symbol, within, ref useSiteDiagnostics, throughTypeOpt);
		}
		return true;
	}

	internal bool IsSymbolAccessibleConditional(Symbol symbol, NamedTypeSymbol within, TypeSymbol throughTypeOpt, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		if (Flags.Includes(BinderFlags.IgnoreAccessibility))
		{
			failedThroughTypeCheck = false;
			return true;
		}
		return AccessCheck.IsSymbolAccessible(symbol, within, throughTypeOpt, out failedThroughTypeCheck, ref useSiteDiagnostics, basesBeingResolved);
	}

	internal static void ReportUseSiteDiagnosticForSynthesizedAttribute(CSharpCompilation compilation, WellKnownMember attributeMember, DiagnosticBag diagnostics, Location location = null, CSharpSyntaxNode syntax = null)
	{
		bool isOptional = WellKnownMembers.IsSynthesizedAttributeOptional(attributeMember);
		GetWellKnownTypeMember(compilation, attributeMember, diagnostics, location, syntax, isOptional);
	}

	internal BoundExpression WrapWithVariablesIfAny(CSharpSyntaxNode scopeDesignator, BoundExpression expression)
	{
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetDeclaredLocalsForScope(scopeDesignator);
		if (!declaredLocalsForScope.IsEmpty)
		{
			return new BoundSequence(scopeDesignator, declaredLocalsForScope, ImmutableArray<BoundExpression>.Empty, expression, expression.Type)
			{
				WasCompilerGenerated = true
			};
		}
		return expression;
	}

	internal BoundStatement WrapWithVariablesIfAny(CSharpSyntaxNode scopeDesignator, BoundStatement statement)
	{
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetDeclaredLocalsForScope(scopeDesignator);
		if (declaredLocalsForScope.IsEmpty)
		{
			return statement;
		}
		return new BoundBlock(statement.Syntax, declaredLocalsForScope, ImmutableArray.Create(statement))
		{
			WasCompilerGenerated = true
		};
	}

	internal BoundStatement WrapWithVariablesAndLocalFunctionsIfAny(CSharpSyntaxNode scopeDesignator, BoundStatement statement)
	{
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetDeclaredLocalsForScope(scopeDesignator);
		ImmutableArray<LocalFunctionSymbol> declaredLocalFunctionsForScope = GetDeclaredLocalFunctionsForScope(scopeDesignator);
		if (declaredLocalsForScope.IsEmpty && declaredLocalFunctionsForScope.IsEmpty)
		{
			return statement;
		}
		return new BoundBlock(statement.Syntax, declaredLocalsForScope, declaredLocalFunctionsForScope, ImmutableArray.Create(statement))
		{
			WasCompilerGenerated = true
		};
	}

	internal string Dump()
	{
		return TreeDumper.DumpCompact(DumpAncestors());
		TreeDumperNode DumpAncestors()
		{
			TreeDumperNode treeDumperNode = null;
			for (Binder binder = this; binder != null; binder = binder.Next)
			{
				(string description, string snippet, string locals) tuple = Print(binder);
				string item = tuple.description;
				string item2 = tuple.snippet;
				string item3 = tuple.locals;
				List<TreeDumperNode> list = new List<TreeDumperNode>();
				if (!item3.IsEmpty())
				{
					list.Add(new TreeDumperNode("locals", item3, null));
				}
				Symbol containingMemberOrLambda = binder.ContainingMemberOrLambda;
				if (containingMemberOrLambda != null && containingMemberOrLambda != binder.Next?.ContainingMemberOrLambda)
				{
					list.Add(new TreeDumperNode("containing symbol", containingMemberOrLambda.ToDisplayString(), null));
				}
				if (item2 != null)
				{
					list.Add(new TreeDumperNode("scope", $"{item2} ({binder.ScopeDesignator.Kind()})", null));
				}
				if (treeDumperNode != null)
				{
					list.Add(treeDumperNode);
				}
				treeDumperNode = new TreeDumperNode(item, null, list);
			}
			return treeDumperNode;
		}
		static (string description, string snippet, string locals) Print(Binder scope)
		{
			string item = string.Join(", ", scope.Locals.SelectAsArray((LocalSymbol s) => s.Name));
			string item2 = null;
			if (scope.ScopeDesignator != null)
			{
				string[] array = scope.ScopeDesignator.ToString().Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 1)
				{
					item2 = array[0];
				}
				else
				{
					string text = array[0];
					string text2 = array[^1].Trim();
					int num = Math.Min(text2.Length, 12);
					item2 = text.Substring(0, Math.Min(text.Length, 12)) + " ... " + text2.Substring(text2.Length - num, num);
				}
				item2 = (item2.IsEmpty() ? null : item2);
			}
			return (description: scope.GetType().Name, snippet: item2, locals: item);
		}
	}

	private static bool RequiresRValueOnly(BindValueKind kind)
	{
		return (kind & (BindValueKind)0xFFFC) == BindValueKind.RValue;
	}

	private static bool RequiresAssignmentOnly(BindValueKind kind)
	{
		return (kind & (BindValueKind)0xFFFC) == BindValueKind.Assignable;
	}

	private static bool RequiresVariable(BindValueKind kind)
	{
		return !RequiresRValueOnly(kind);
	}

	private static bool RequiresReferenceToLocation(BindValueKind kind)
	{
		return (kind & BindValueKind.RefersToLocation) != 0;
	}

	private static bool RequiresAssignableVariable(BindValueKind kind)
	{
		return (kind & BindValueKind.Assignable) != 0;
	}

	private static bool RequiresRefAssignableVariable(BindValueKind kind)
	{
		return (kind & BindValueKind.RefAssignable) != 0;
	}

	private static bool RequiresRefOrOut(BindValueKind kind)
	{
		return (kind & BindValueKind.RefOrOut) == BindValueKind.RefOrOut;
	}

	private BoundExpression CheckValue(BoundExpression expr, BindValueKind valueKind, DiagnosticBag diagnostics)
	{
		switch (expr.Kind)
		{
		case BoundKind.PropertyGroup:
			expr = BindIndexedPropertyAccess((BoundPropertyGroup)expr, mustHaveAllOptionalParameters: false, diagnostics);
			break;
		case BoundKind.OutVariablePendingInference:
		case BoundKind.OutDeconstructVarPendingInference:
			return expr;
		case BoundKind.DiscardExpression:
			return expr;
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			if (valueKind == BindValueKind.Assignable && !boundIndexerAccess.Indexer.ReturnsByRef)
			{
				expr = boundIndexerAccess.Update(useSetterForDefaultArgumentGeneration: true);
			}
			break;
		}
		}
		bool flag = false;
		if (expr.Kind == BoundKind.MethodGroup && valueKind != BindValueKind.RValueOrMethodGroup)
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)expr;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			MethodGroupResolution methodGroupResolution = ResolveMethodGroup(boundMethodGroup, null, isMethodGroupConversion: false, ref useSiteDiagnostics);
			diagnostics.Add(expr.Syntax, useSiteDiagnostics);
			Symbol symbol = null;
			bool flag2 = methodGroupResolution.MethodGroup != null;
			if (!expr.HasAnyErrors)
			{
				diagnostics.AddRange(methodGroupResolution.Diagnostics);
			}
			flag = methodGroupResolution.HasAnyErrors;
			if (flag)
			{
				symbol = methodGroupResolution.OtherSymbol;
			}
			methodGroupResolution.Free();
			if (!flag2)
			{
				BoundExpression boundExpression = boundMethodGroup.ReceiverOpt;
				if ((object)symbol != null && boundExpression != null && boundExpression.Kind == BoundKind.TypeOrValueExpression)
				{
					BoundTypeOrValueExpression boundTypeOrValueExpression = (BoundTypeOrValueExpression)boundExpression;
					boundExpression = (symbol.IsStatic ? null : boundTypeOrValueExpression.Data.ValueExpression);
				}
				return new BoundBadExpression(expr.Syntax, boundMethodGroup.ResultKind, ((object)symbol == null) ? ImmutableArray<Symbol>.Empty : ImmutableArray.Create(symbol), (boundExpression == null) ? ImmutableArray<BoundExpression>.Empty : ImmutableArray.Create(boundExpression), GetNonMethodMemberType(symbol));
			}
		}
		if ((!flag && CheckValueKind(expr.Syntax, expr, valueKind, checkingReceiver: false, diagnostics)) || (expr.HasAnyErrors && valueKind == BindValueKind.RValueOrMethodGroup))
		{
			return expr;
		}
		LookupResultKind resultKind = ((valueKind == BindValueKind.RValue || valueKind == BindValueKind.RValueOrMethodGroup) ? LookupResultKind.NotAValue : LookupResultKind.NotAVariable);
		return ToBadExpression(expr, resultKind);
	}

	internal static bool IsTypeOrValueExpression(BoundExpression expression)
	{
		BoundKind? boundKind = expression?.Kind;
		if (boundKind.HasValue)
		{
			BoundKind valueOrDefault = boundKind.GetValueOrDefault();
			byte b = (byte)valueOrDefault;
			if (b == 11 || (b == 138 && ((BoundQueryClause)expression).Value.Kind == BoundKind.TypeOrValueExpression))
			{
				return true;
			}
		}
		return false;
	}

	internal bool CheckValueKind(SyntaxNode node, BoundExpression expr, BindValueKind valueKind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		if (expr.HasAnyErrors)
		{
			return false;
		}
		switch (expr.Kind)
		{
		case BoundKind.PropertyAccess:
		case BoundKind.IndexerAccess:
			return CheckPropertyValueKind(node, expr, valueKind, checkingReceiver, diagnostics);
		case BoundKind.EventAccess:
			return CheckEventValueKind((BoundEventAccess)expr, valueKind, diagnostics);
		default:
			if (RequiresRValueOnly(valueKind))
			{
				return CheckNotNamespaceOrType(expr, diagnostics);
			}
			if (expr.ConstantValue != null || expr.Type.GetSpecialTypeSafe() == SpecialType.System_Void)
			{
				Error(diagnostics, GetStandardLvalueError(valueKind), node);
				return false;
			}
			switch (expr.Kind)
			{
			case BoundKind.NamespaceExpression:
			{
				BoundNamespaceExpression boundNamespaceExpression = (BoundNamespaceExpression)expr;
				Error(diagnostics, ErrorCode.ERR_BadSKknown, node, boundNamespaceExpression.NamespaceSymbol, MessageID.IDS_SK_NAMESPACE.Localize(), MessageID.IDS_SK_VARIABLE.Localize());
				return false;
			}
			case BoundKind.TypeExpression:
			{
				BoundTypeExpression boundTypeExpression = (BoundTypeExpression)expr;
				Error(diagnostics, ErrorCode.ERR_BadSKknown, node, boundTypeExpression.Type, MessageID.IDS_SK_TYPE.Localize(), MessageID.IDS_SK_VARIABLE.Localize());
				return false;
			}
			case BoundKind.Lambda:
			case BoundKind.UnboundLambda:
				Error(diagnostics, GetStandardLvalueError(valueKind), node);
				return false;
			case BoundKind.MethodGroup:
			{
				BoundMethodGroup boundMethodGroup = (BoundMethodGroup)expr;
				Error(diagnostics, GetMethodGroupLvalueError(valueKind), node, boundMethodGroup.Name, MessageID.IDS_MethodGroup.Localize());
				return false;
			}
			case BoundKind.RangeVariable:
			{
				BoundRangeVariable boundRangeVariable = (BoundRangeVariable)expr;
				Error(diagnostics, GetRangeLvalueError(valueKind), node, boundRangeVariable.RangeVariableSymbol.Name);
				return false;
			}
			case BoundKind.Conversion:
				if (((BoundConversion)expr).ConversionKind == ConversionKind.Unboxing)
				{
					Error(diagnostics, ErrorCode.ERR_UnboxNotLValue, node);
					return false;
				}
				break;
			case BoundKind.PointerIndirectionOperator:
			case BoundKind.RefValueOperator:
			case BoundKind.ArrayAccess:
			case BoundKind.DynamicMemberAccess:
			case BoundKind.DynamicIndexerAccess:
				if (RequiresRefAssignableVariable(valueKind))
				{
					Error(diagnostics, ErrorCode.ERR_RefLocalOrParamExpected, node);
					return false;
				}
				return true;
			case BoundKind.PointerElementAccess:
				if (RequiresRefAssignableVariable(valueKind))
				{
					Error(diagnostics, ErrorCode.ERR_RefLocalOrParamExpected, node);
					return false;
				}
				if (((BoundPointerElementAccess)expr).Expression is BoundFieldAccess boundFieldAccess && boundFieldAccess.FieldSymbol.IsFixed)
				{
					return CheckValueKind(node, boundFieldAccess.ReceiverOpt, valueKind, checkingReceiver: true, diagnostics);
				}
				return true;
			case BoundKind.Parameter:
			{
				BoundParameter parameter = (BoundParameter)expr;
				return CheckParameterValueKind(node, parameter, valueKind, checkingReceiver, diagnostics);
			}
			case BoundKind.Local:
			{
				BoundLocal local = (BoundLocal)expr;
				return CheckLocalValueKind(node, local, valueKind, checkingReceiver, diagnostics);
			}
			case BoundKind.ThisReference:
			{
				BoundThisReference boundThisReference = (BoundThisReference)expr;
				if (RequiresRefAssignableVariable(valueKind))
				{
					Error(diagnostics, ErrorCode.ERR_RefLocalOrParamExpected, node);
					return false;
				}
				if (boundThisReference.Type.IsValueType)
				{
					if (RequiresAssignableVariable(valueKind) && boundThisReference.Type.IsReadOnly)
					{
						MethodSymbol obj = ContainingMemberOrLambda as MethodSymbol;
						if ((object)obj == null || obj.MethodKind != MethodKind.Constructor)
						{
							goto IL_03b7;
						}
					}
					return true;
				}
				goto IL_03b7;
			}
			case BoundKind.Call:
			{
				BoundCall call = (BoundCall)expr;
				return CheckCallValueKind(call, node, valueKind, checkingReceiver, diagnostics);
			}
			case BoundKind.ConditionalOperator:
			{
				BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
				if (boundConditionalOperator.IsRef && (CheckValueKind(boundConditionalOperator.Consequence.Syntax, boundConditionalOperator.Consequence, valueKind, checkingReceiver: false, diagnostics) & CheckValueKind(boundConditionalOperator.Alternative.Syntax, boundConditionalOperator.Alternative, valueKind, checkingReceiver: false, diagnostics)))
				{
					return true;
				}
				break;
			}
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess fieldAccess = (BoundFieldAccess)expr;
				return CheckFieldValueKind(node, fieldAccess, valueKind, checkingReceiver, diagnostics);
			}
			case BoundKind.AssignmentOperator:
				{
					BoundAssignmentOperator assignment = (BoundAssignmentOperator)expr;
					return CheckSimpleAssignmentValueKind(node, assignment, valueKind, diagnostics);
				}
				IL_03b7:
				Error(diagnostics, GetThisLvalueError(valueKind), node, "this");
				return false;
			}
			Error(diagnostics, GetStandardLvalueError(valueKind), node);
			return false;
		}
	}

	private static bool CheckNotNamespaceOrType(BoundExpression expr, DiagnosticBag diagnostics)
	{
		switch (expr.Kind)
		{
		case BoundKind.NamespaceExpression:
			Error(diagnostics, ErrorCode.ERR_BadSKknown, expr.Syntax, ((BoundNamespaceExpression)expr).NamespaceSymbol, MessageID.IDS_SK_NAMESPACE.Localize(), MessageID.IDS_SK_VARIABLE.Localize());
			return false;
		case BoundKind.TypeExpression:
			Error(diagnostics, ErrorCode.ERR_BadSKunknown, expr.Syntax, expr.Type, MessageID.IDS_SK_TYPE.Localize());
			return false;
		default:
			return true;
		}
	}

	private bool CheckLocalValueKind(SyntaxNode node, BoundLocal local, BindValueKind valueKind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		LocalSymbol localSymbol = local.LocalSymbol;
		if (RequiresAssignableVariable(valueKind))
		{
			if (LockedOrDisposedVariables.Contains(localSymbol))
			{
				diagnostics.Add(ErrorCode.WRN_AssignmentToLockOrDispose, local.Syntax.Location, localSymbol);
			}
			if (localSymbol.RefKind == RefKind.In || (localSymbol.RefKind == RefKind.None && !localSymbol.IsWritableVariable))
			{
				ReportReadonlyLocalError(node, localSymbol, valueKind, checkingReceiver, diagnostics);
				return false;
			}
		}
		else if (RequiresRefAssignableVariable(valueKind))
		{
			if (localSymbol.RefKind == RefKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_RefLocalOrParamExpected, node.Location, localSymbol);
				return false;
			}
			if (!localSymbol.IsWritableVariable)
			{
				ReportReadonlyLocalError(node, localSymbol, valueKind, checkingReceiver, diagnostics);
				return false;
			}
		}
		return true;
	}

	private static bool CheckLocalRefEscape(SyntaxNode node, BoundLocal local, uint escapeTo, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		LocalSymbol localSymbol = local.LocalSymbol;
		if (localSymbol.RefEscapeScope <= escapeTo)
		{
			return true;
		}
		if (escapeTo == 0)
		{
			if (localSymbol.RefKind == RefKind.None)
			{
				if (checkingReceiver)
				{
					Error(diagnostics, ErrorCode.ERR_RefReturnLocal2, local.Syntax, localSymbol);
				}
				else
				{
					Error(diagnostics, ErrorCode.ERR_RefReturnLocal, node, localSymbol);
				}
				return false;
			}
			if (checkingReceiver)
			{
				Error(diagnostics, ErrorCode.ERR_RefReturnNonreturnableLocal2, local.Syntax, localSymbol);
			}
			else
			{
				Error(diagnostics, ErrorCode.ERR_RefReturnNonreturnableLocal, node, localSymbol);
			}
			return false;
		}
		Error(diagnostics, ErrorCode.ERR_EscapeLocal, node, localSymbol);
		return false;
	}

	private bool CheckParameterValueKind(SyntaxNode node, BoundParameter parameter, BindValueKind valueKind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		ParameterSymbol parameterSymbol = parameter.ParameterSymbol;
		if (parameterSymbol.RefKind == RefKind.In && RequiresAssignableVariable(valueKind))
		{
			ReportReadOnlyError(parameterSymbol, node, valueKind, checkingReceiver, diagnostics);
			return false;
		}
		if (parameterSymbol.RefKind == RefKind.None && RequiresRefAssignableVariable(valueKind))
		{
			Error(diagnostics, ErrorCode.ERR_RefLocalOrParamExpected, node);
			return false;
		}
		if (LockedOrDisposedVariables.Contains(parameterSymbol))
		{
			diagnostics.Add(ErrorCode.WRN_AssignmentToLockOrDispose, parameter.Syntax.Location, parameterSymbol.Name);
		}
		return true;
	}

	private static bool CheckParameterRefEscape(SyntaxNode node, BoundParameter parameter, uint escapeTo, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		ParameterSymbol parameterSymbol = parameter.ParameterSymbol;
		if (escapeTo == 0 && parameterSymbol.RefKind == RefKind.None)
		{
			if (checkingReceiver)
			{
				Error(diagnostics, ErrorCode.ERR_RefReturnParameter2, parameter.Syntax, parameterSymbol.Name);
			}
			else
			{
				Error(diagnostics, ErrorCode.ERR_RefReturnParameter, node, parameterSymbol.Name);
			}
			return false;
		}
		return true;
	}

	private bool CheckFieldValueKind(SyntaxNode node, BoundFieldAccess fieldAccess, BindValueKind valueKind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		bool isStatic = fieldSymbol.IsStatic;
		if (RequiresAssignableVariable(valueKind))
		{
			if (fieldSymbol.IsReadOnly)
			{
				bool flag = false;
				Symbol containingMemberOrLambda = ContainingMemberOrLambda;
				if ((object)containingMemberOrLambda != null && isStatic == containingMemberOrLambda.IsStatic && (isStatic || fieldAccess.ReceiverOpt.Kind == BoundKind.ThisReference) && (Compilation.FeatureStrictEnabled ? (fieldSymbol.ContainingType == containingMemberOrLambda.ContainingType) : (fieldSymbol.ContainingType.OriginalDefinition == containingMemberOrLambda.ContainingType.OriginalDefinition)))
				{
					if (containingMemberOrLambda.Kind == SymbolKind.Method)
					{
						MethodSymbol methodSymbol = (MethodSymbol)containingMemberOrLambda;
						MethodKind methodKind = ((!isStatic) ? MethodKind.Constructor : MethodKind.StaticConstructor);
						flag = methodSymbol.MethodKind == methodKind;
					}
					else if (containingMemberOrLambda.Kind == SymbolKind.Field)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					ReportReadOnlyFieldError(fieldSymbol, node, valueKind, checkingReceiver, diagnostics);
					return false;
				}
			}
			if (fieldSymbol.IsFixed)
			{
				Error(diagnostics, GetStandardLvalueError(valueKind), node);
				return false;
			}
		}
		if (RequiresRefAssignableVariable(valueKind))
		{
			Error(diagnostics, ErrorCode.ERR_RefLocalOrParamExpected, node);
			return false;
		}
		if (isStatic || fieldSymbol.ContainingType.IsReferenceType)
		{
			return true;
		}
		return CheckIsValidReceiverForVariable(node, fieldAccess.ReceiverOpt, valueKind, diagnostics);
	}

	private bool CheckSimpleAssignmentValueKind(SyntaxNode node, BoundAssignmentOperator assignment, BindValueKind valueKind, DiagnosticBag diagnostics)
	{
		if (assignment.IsRef)
		{
			return CheckValueKind(node, assignment.Left, valueKind, checkingReceiver: false, diagnostics);
		}
		Error(diagnostics, GetStandardLvalueError(valueKind), node);
		return false;
	}

	private static bool CheckFieldRefEscape(SyntaxNode node, BoundFieldAccess fieldAccess, uint escapeFrom, uint escapeTo, DiagnosticBag diagnostics)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		if (fieldSymbol.IsStatic || fieldSymbol.ContainingType.IsReferenceType)
		{
			return true;
		}
		return CheckRefEscape(node, fieldAccess.ReceiverOpt, escapeFrom, escapeTo, checkingReceiver: true, diagnostics);
	}

	private static bool CheckFieldLikeEventRefEscape(SyntaxNode node, BoundEventAccess eventAccess, uint escapeFrom, uint escapeTo, DiagnosticBag diagnostics)
	{
		EventSymbol eventSymbol = eventAccess.EventSymbol;
		if (eventSymbol.IsStatic || eventSymbol.ContainingType.IsReferenceType)
		{
			return true;
		}
		return CheckRefEscape(node, eventAccess.ReceiverOpt, escapeFrom, escapeTo, checkingReceiver: true, diagnostics);
	}

	private bool CheckEventValueKind(BoundEventAccess boundEvent, BindValueKind valueKind, DiagnosticBag diagnostics)
	{
		BoundExpression receiverOpt = boundEvent.ReceiverOpt;
		SyntaxNode eventName = GetEventName(boundEvent);
		EventSymbol eventSymbol = boundEvent.EventSymbol;
		if (valueKind == BindValueKind.CompoundAssignment)
		{
			if (receiverOpt != null && receiverOpt.Kind == BoundKind.BaseReference && eventSymbol.IsAbstract)
			{
				Error(diagnostics, ErrorCode.ERR_AbstractBaseCall, boundEvent.Syntax, eventSymbol);
				return false;
			}
			if (ReportUseSiteDiagnostics(eventSymbol, diagnostics, eventName))
			{
				return false;
			}
			return true;
		}
		if (!boundEvent.IsUsableAsField)
		{
			Error(diagnostics, GetBadEventUsageDiagnosticInfo(eventSymbol), eventName);
			return false;
		}
		if (ReportUseSiteDiagnostics(eventSymbol, diagnostics, eventName))
		{
			if (!CheckIsValidReceiverForVariable(eventName, receiverOpt, BindValueKind.Assignable, diagnostics))
			{
				return false;
			}
		}
		else if (RequiresVariable(valueKind))
		{
			if (eventSymbol.IsWindowsRuntimeEvent && valueKind != BindValueKind.Assignable)
			{
				ErrorCode code = ((valueKind == BindValueKind.RefOrOut) ? ErrorCode.ERR_WinRtEventPassedByRef : GetStandardLvalueError(valueKind));
				Error(diagnostics, code, eventName, eventSymbol);
				return false;
			}
			if (RequiresVariableReceiver(receiverOpt, eventSymbol.AssociatedField) && !CheckIsValidReceiverForVariable(eventName, receiverOpt, valueKind, diagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckIsValidReceiverForVariable(SyntaxNode node, BoundExpression receiver, BindValueKind kind, DiagnosticBag diagnostics)
	{
		if (!Flags.Includes(BinderFlags.ObjectInitializerMember) || receiver.Kind != BoundKind.ImplicitReceiver)
		{
			return CheckValueKind(node, receiver, kind, checkingReceiver: true, diagnostics);
		}
		return true;
	}

	private static bool RequiresVariableReceiver(BoundExpression receiver, Symbol symbol)
	{
		if (!symbol.IsStatic && symbol.Kind != SymbolKind.Event)
		{
			if (receiver == null)
			{
				return false;
			}
			return receiver.Type?.IsValueType == true;
		}
		return false;
	}

	private bool CheckCallValueKind(BoundCall call, SyntaxNode node, BindValueKind valueKind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		return CheckMethodReturnValueKind(call.Method, call.Syntax, node, valueKind, checkingReceiver, diagnostics);
	}

	protected bool CheckMethodReturnValueKind(MethodSymbol methodSymbol, SyntaxNode callSyntaxOpt, SyntaxNode node, BindValueKind valueKind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		if (RequiresVariable(valueKind) && methodSymbol.RefKind == RefKind.None)
		{
			if (checkingReceiver)
			{
				Error(diagnostics, ErrorCode.ERR_ReturnNotLValue, callSyntaxOpt, methodSymbol);
			}
			else
			{
				Error(diagnostics, GetStandardLvalueError(valueKind), node);
			}
			return false;
		}
		if (RequiresAssignableVariable(valueKind) && methodSymbol.RefKind == RefKind.In)
		{
			ReportReadOnlyError(methodSymbol, node, valueKind, checkingReceiver, diagnostics);
			return false;
		}
		if (RequiresRefAssignableVariable(valueKind))
		{
			Error(diagnostics, ErrorCode.ERR_RefLocalOrParamExpected, node);
			return false;
		}
		return true;
	}

	private bool CheckPropertyValueKind(SyntaxNode node, BoundExpression expr, BindValueKind valueKind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = GetPropertySymbol(expr, out var receiver, out var propertySyntax);
		if ((RequiresReferenceToLocation(valueKind) | checkingReceiver) && propertySymbol.RefKind == RefKind.None)
		{
			if (checkingReceiver)
			{
				Error(diagnostics, ErrorCode.ERR_ReturnNotLValue, expr.Syntax, propertySymbol);
			}
			else
			{
				Error(diagnostics, (valueKind == BindValueKind.RefOrOut) ? ErrorCode.ERR_RefProperty : GetStandardLvalueError(valueKind), node, propertySymbol);
			}
			return false;
		}
		if (RequiresAssignableVariable(valueKind) && propertySymbol.RefKind == RefKind.In)
		{
			ReportReadOnlyError(propertySymbol, node, valueKind, checkingReceiver, diagnostics);
			return false;
		}
		if (RequiresAssignableVariable(valueKind) && propertySymbol.RefKind == RefKind.None)
		{
			MethodSymbol ownOrInheritedSetMethod = propertySymbol.GetOwnOrInheritedSetMethod();
			if ((object)ownOrInheritedSetMethod == null)
			{
				Symbol containingMemberOrLambda = ContainingMemberOrLambda;
				if (!AccessingAutoPropertyFromConstructor(receiver, propertySymbol, containingMemberOrLambda))
				{
					Error(diagnostics, ErrorCode.ERR_AssgReadonlyProp, node, propertySymbol);
					return false;
				}
			}
			else
			{
				if (receiver != null && receiver.Kind == BoundKind.BaseReference && ownOrInheritedSetMethod.IsAbstract)
				{
					Error(diagnostics, ErrorCode.ERR_AbstractBaseCall, node, propertySymbol);
					return false;
				}
				if (!object.Equals(ownOrInheritedSetMethod.GetUseSiteDiagnostic(), propertySymbol.GetUseSiteDiagnostic()) && ReportUseSiteDiagnostics(ownOrInheritedSetMethod, diagnostics, propertySyntax))
				{
					return false;
				}
				TypeSymbol accessThroughType = GetAccessThroughType(receiver);
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				bool flag = IsAccessible(ownOrInheritedSetMethod, accessThroughType, out var failedThroughTypeCheck, ref useSiteDiagnostics);
				diagnostics.Add(node, useSiteDiagnostics);
				if (!flag)
				{
					if (failedThroughTypeCheck)
					{
						Error(diagnostics, ErrorCode.ERR_BadProtectedAccess, node, propertySymbol, accessThroughType, ContainingType);
					}
					else
					{
						Error(diagnostics, ErrorCode.ERR_InaccessibleSetter, node, propertySymbol);
					}
					return false;
				}
				ReportDiagnosticsIfObsolete(diagnostics, ownOrInheritedSetMethod, node, receiver != null && receiver.Kind == BoundKind.BaseReference);
				if (RequiresVariableReceiver(receiver, ownOrInheritedSetMethod) && !CheckIsValidReceiverForVariable(node, receiver, BindValueKind.Assignable, diagnostics))
				{
					return false;
				}
			}
		}
		if (!RequiresAssignmentOnly(valueKind) || propertySymbol.RefKind != RefKind.None)
		{
			MethodSymbol ownOrInheritedGetMethod = propertySymbol.GetOwnOrInheritedGetMethod();
			if ((object)ownOrInheritedGetMethod == null)
			{
				Error(diagnostics, ErrorCode.ERR_PropertyLacksGet, node, propertySymbol);
				return false;
			}
			if (receiver != null && receiver.Kind == BoundKind.BaseReference && ownOrInheritedGetMethod.IsAbstract)
			{
				Error(diagnostics, ErrorCode.ERR_AbstractBaseCall, node, propertySymbol);
				return false;
			}
			if (!object.Equals(ownOrInheritedGetMethod.GetUseSiteDiagnostic(), propertySymbol.GetUseSiteDiagnostic()) && ReportUseSiteDiagnostics(ownOrInheritedGetMethod, diagnostics, propertySyntax))
			{
				return false;
			}
			TypeSymbol accessThroughType2 = GetAccessThroughType(receiver);
			HashSet<DiagnosticInfo> useSiteDiagnostics2 = null;
			bool flag2 = IsAccessible(ownOrInheritedGetMethod, accessThroughType2, out var failedThroughTypeCheck2, ref useSiteDiagnostics2);
			diagnostics.Add(node, useSiteDiagnostics2);
			if (!flag2)
			{
				if (failedThroughTypeCheck2)
				{
					Error(diagnostics, ErrorCode.ERR_BadProtectedAccess, node, propertySymbol, accessThroughType2, ContainingType);
				}
				else
				{
					Error(diagnostics, ErrorCode.ERR_InaccessibleGetter, node, propertySymbol);
				}
				return false;
			}
			ReportDiagnosticsIfObsolete(diagnostics, ownOrInheritedGetMethod, node, receiver != null && receiver.Kind == BoundKind.BaseReference);
		}
		if (RequiresRefAssignableVariable(valueKind))
		{
			Error(diagnostics, ErrorCode.ERR_RefLocalOrParamExpected, node);
			return false;
		}
		return true;
	}

	internal static uint GetInvocationEscapeScope(Symbol symbol, BoundExpression receiverOpt, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<BoundExpression> argsOpt, ImmutableArray<RefKind> argRefKindsOpt, ImmutableArray<int> argsToParamsOpt, uint scopeOfTheContainingExpression, bool isRefEscape)
	{
		if (symbol.IsStatic)
		{
			receiverOpt = null;
		}
		uint num = 0u;
		ArrayBuilder<bool> inParametersMatchedWithArgs = null;
		if (!argsOpt.IsDefault)
		{
			while (true)
			{
				BoundExpression boundExpression;
				for (int i = 0; i < argsOpt.Length; i++)
				{
					boundExpression = argsOpt[i];
					if (boundExpression.Kind == BoundKind.ArgListOperator)
					{
						goto IL_0032;
					}
					uint val = (((GetEffectiveRefKindAndMarkMatchedInParameter(i, argRefKindsOpt, parameters, argsToParamsOpt, ref inParametersMatchedWithArgs) != RefKind.None) & isRefEscape) ? GetRefEscape(boundExpression, scopeOfTheContainingExpression) : GetValEscape(boundExpression, scopeOfTheContainingExpression));
					num = Math.Max(num, val);
					if (num >= scopeOfTheContainingExpression)
					{
						inParametersMatchedWithArgs?.Free();
						return num;
					}
				}
				break;
				IL_0032:
				argsOpt = ((BoundArgListOperator)boundExpression).Arguments;
				argRefKindsOpt = default;
				parameters = ImmutableArray<ParameterSymbol>.Empty;
				argsToParamsOpt = default;
			}
		}
		if ((TryGetunmatchedInParameterAndFreeMatchedArgs(parameters, ref inParametersMatchedWithArgs) != null) & isRefEscape)
		{
			return scopeOfTheContainingExpression;
		}
		if (receiverOpt != null && receiverOpt.Type?.IsByRefLikeType == true)
		{
			num = Math.Max(num, GetValEscape(receiverOpt, scopeOfTheContainingExpression));
		}
		return num;
	}

	private static bool CheckInvocationEscape(SyntaxNode syntax, Symbol symbol, BoundExpression receiverOpt, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<BoundExpression> argsOpt, ImmutableArray<RefKind> argRefKindsOpt, ImmutableArray<int> argsToParamsOpt, bool checkingReceiver, uint escapeFrom, uint escapeTo, DiagnosticBag diagnostics, bool isRefEscape)
	{
		if (symbol.IsStatic)
		{
			receiverOpt = null;
		}
		ArrayBuilder<bool> inParametersMatchedWithArgs = null;
		if (!argsOpt.IsDefault)
		{
			while (true)
			{
				BoundExpression boundExpression;
				for (int i = 0; i < argsOpt.Length; i++)
				{
					boundExpression = argsOpt[i];
					if (boundExpression.Kind == BoundKind.ArgListOperator)
					{
						goto IL_0033;
					}
					if (!(((GetEffectiveRefKindAndMarkMatchedInParameter(i, argRefKindsOpt, parameters, argsToParamsOpt, ref inParametersMatchedWithArgs) != RefKind.None) & isRefEscape) ? CheckRefEscape(boundExpression.Syntax, boundExpression, escapeFrom, escapeTo, checkingReceiver: false, diagnostics) : CheckValEscape(boundExpression.Syntax, boundExpression, escapeFrom, escapeTo, checkingReceiver: false, diagnostics)))
					{
						inParametersMatchedWithArgs?.Free();
						ErrorCode standardCallEscapeError = GetStandardCallEscapeError(checkingReceiver);
						string text;
						if (parameters.Length > 0)
						{
							int index = (argsToParamsOpt.IsDefault ? i : argsToParamsOpt[i]);
							text = parameters[index].Name;
						}
						else
						{
							text = "__arglist";
						}
						Error(diagnostics, standardCallEscapeError, syntax, symbol, text);
						return false;
					}
				}
				break;
				IL_0033:
				argsOpt = ((BoundArgListOperator)boundExpression).Arguments;
				argRefKindsOpt = default;
				parameters = ImmutableArray<ParameterSymbol>.Empty;
				argsToParamsOpt = default;
			}
		}
		ParameterSymbol parameterSymbol = TryGetunmatchedInParameterAndFreeMatchedArgs(parameters, ref inParametersMatchedWithArgs);
		if ((parameterSymbol != null) & isRefEscape)
		{
			Error(diagnostics, GetStandardCallEscapeError(checkingReceiver), syntax, symbol, parameterSymbol.Name);
			return false;
		}
		if (receiverOpt != null && receiverOpt.Type?.IsByRefLikeType == true)
		{
			return CheckValEscape(receiverOpt.Syntax, receiverOpt, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		return true;
	}

	private static bool CheckInvocationArgMixing(SyntaxNode syntax, Symbol symbol, BoundExpression receiverOpt, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<BoundExpression> argsOpt, ImmutableArray<int> argsToParamsOpt, uint scopeOfTheContainingExpression, DiagnosticBag diagnostics)
	{
		if (symbol.IsStatic)
		{
			receiverOpt = null;
		}
		uint num = scopeOfTheContainingExpression;
		TypeSymbol typeSymbol = receiverOpt?.Type;
		if ((object)typeSymbol != null && typeSymbol.IsByRefLikeType && (object)typeSymbol != null && !typeSymbol.IsReadOnly)
		{
			num = GetValEscape(receiverOpt, scopeOfTheContainingExpression);
		}
		if (!argsOpt.IsDefault)
		{
			BoundArgListOperator boundArgListOperator = null;
			for (int i = 0; i < argsOpt.Length; i++)
			{
				BoundExpression boundExpression = argsOpt[i];
				if (boundExpression.Kind == BoundKind.ArgListOperator)
				{
					boundArgListOperator = (BoundArgListOperator)boundExpression;
					break;
				}
				int index = (argsToParamsOpt.IsDefault ? i : argsToParamsOpt[i]);
				if (parameters[index].RefKind.IsWritableReference())
				{
					TypeSymbol type = boundExpression.Type;
					if ((object)type != null && type.IsByRefLikeType)
					{
						num = Math.Min(num, GetValEscape(boundExpression, scopeOfTheContainingExpression));
					}
				}
			}
			if (boundArgListOperator != null)
			{
				ImmutableArray<BoundExpression> arguments = boundArgListOperator.Arguments;
				ImmutableArray<RefKind> argumentRefKindsOpt = boundArgListOperator.ArgumentRefKindsOpt;
				for (int j = 0; j < arguments.Length; j++)
				{
					BoundExpression boundExpression2 = arguments[j];
					if (((!argumentRefKindsOpt.IsDefault) ? argumentRefKindsOpt[j] : RefKind.None).IsWritableReference())
					{
						TypeSymbol type2 = boundExpression2.Type;
						if ((object)type2 != null && type2.IsByRefLikeType)
						{
							num = Math.Min(num, GetValEscape(boundExpression2, scopeOfTheContainingExpression));
						}
					}
				}
			}
		}
		if (num == scopeOfTheContainingExpression)
		{
			return true;
		}
		if (!argsOpt.IsDefault)
		{
			while (true)
			{
				BoundExpression boundExpression3;
				for (int k = 0; k < argsOpt.Length; k++)
				{
					boundExpression3 = argsOpt[k];
					if (boundExpression3.Kind == BoundKind.ArgListOperator)
					{
						goto IL_016f;
					}
					if (!CheckValEscape(boundExpression3.Syntax, boundExpression3, scopeOfTheContainingExpression, num, checkingReceiver: false, diagnostics))
					{
						string text;
						if (parameters.Length > 0)
						{
							int index2 = (argsToParamsOpt.IsDefault ? k : argsToParamsOpt[k]);
							text = parameters[index2].Name;
						}
						else
						{
							text = "__arglist";
						}
						Error(diagnostics, ErrorCode.ERR_CallArgMixing, syntax, symbol, text);
						return false;
					}
				}
				break;
				IL_016f:
				argsOpt = ((BoundArgListOperator)boundExpression3).Arguments;
				parameters = ImmutableArray<ParameterSymbol>.Empty;
				argsToParamsOpt = default;
			}
		}
		if (receiverOpt != null && receiverOpt.Type?.IsByRefLikeType == true)
		{
			return CheckValEscape(receiverOpt.Syntax, receiverOpt, scopeOfTheContainingExpression, num, checkingReceiver: false, diagnostics);
		}
		return true;
	}

	private static RefKind GetEffectiveRefKindAndMarkMatchedInParameter(int argIndex, ImmutableArray<RefKind> argRefKindsOpt, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<int> argsToParamsOpt, ref ArrayBuilder<bool> inParametersMatchedWithArgs)
	{
		RefKind refKind = ((!argRefKindsOpt.IsDefault) ? argRefKindsOpt[argIndex] : RefKind.None);
		if ((refKind == RefKind.None || refKind == RefKind.In) && argIndex < parameters.Length)
		{
			int index = (argsToParamsOpt.IsDefault ? argIndex : argsToParamsOpt[argIndex]);
			if (parameters[index].RefKind == RefKind.In)
			{
				refKind = RefKind.In;
				inParametersMatchedWithArgs = inParametersMatchedWithArgs ?? ArrayBuilder<bool>.GetInstance(parameters.Length, fillWithValue: false);
				inParametersMatchedWithArgs[index] = true;
			}
		}
		return refKind;
	}

	private static ParameterSymbol TryGetunmatchedInParameterAndFreeMatchedArgs(ImmutableArray<ParameterSymbol> parameters, ref ArrayBuilder<bool> inParametersMatchedWithArgs)
	{
		try
		{
			if (!parameters.IsDefault)
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					ParameterSymbol parameterSymbol = parameters[i];
					if (parameterSymbol.IsParams)
					{
						break;
					}
					if (parameterSymbol.RefKind == RefKind.In)
					{
						ArrayBuilder<bool> obj = inParametersMatchedWithArgs;
						if ((obj == null || !obj[i]) && !parameterSymbol.Type.IsByRefLikeType)
						{
							return parameterSymbol;
						}
					}
				}
			}
			return null;
		}
		finally
		{
			inParametersMatchedWithArgs?.Free();
			inParametersMatchedWithArgs = null;
		}
	}

	private static ErrorCode GetStandardCallEscapeError(bool checkingReceiver)
	{
		if (!checkingReceiver)
		{
			return ErrorCode.ERR_EscapeCall;
		}
		return ErrorCode.ERR_EscapeCall2;
	}

	private static void ReportReadonlyLocalError(SyntaxNode node, LocalSymbol local, BindValueKind kind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		MessageID id;
		if (local.IsForEach)
		{
			id = MessageID.IDS_FOREACHLOCAL;
		}
		else if (local.IsUsing)
		{
			id = MessageID.IDS_USINGLOCAL;
		}
		else
		{
			if (!local.IsFixed)
			{
				Error(diagnostics, GetStandardLvalueError(kind), node);
				return;
			}
			id = MessageID.IDS_FIXEDLOCAL;
		}
		ErrorCode[] array = new ErrorCode[4]
		{
			ErrorCode.ERR_RefReadonlyLocalCause,
			ErrorCode.ERR_AssgReadonlyLocalCause,
			ErrorCode.ERR_RefReadonlyLocal2Cause,
			ErrorCode.ERR_AssgReadonlyLocal2Cause
		};
		int num = (checkingReceiver ? 2 : 0) + ((!RequiresRefOrOut(kind)) ? 1 : 0);
		Error(diagnostics, array[num], node, local, id.Localize());
	}

	private static ErrorCode GetThisLvalueError(BindValueKind kind)
	{
		switch (kind)
		{
		case BindValueKind.Assignable:
		case BindValueKind.CompoundAssignment:
			return ErrorCode.ERR_AssgReadonlyLocal;
		case BindValueKind.RefOrOut:
			return ErrorCode.ERR_RefReadonlyLocal;
		case BindValueKind.AddressOf:
			return ErrorCode.ERR_InvalidAddrOp;
		case BindValueKind.IncrementDecrement:
			return ErrorCode.ERR_IncrementLvalueExpected;
		case BindValueKind.ReadonlyRef:
		case BindValueKind.RefReturn:
			return ErrorCode.ERR_RefReturnThis;
		case BindValueKind.RefAssignable:
			return ErrorCode.ERR_RefLocalOrParamExpected;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private static ErrorCode GetRangeLvalueError(BindValueKind kind)
	{
		switch (kind)
		{
		case BindValueKind.Assignable:
		case BindValueKind.CompoundAssignment:
		case BindValueKind.IncrementDecrement:
			return ErrorCode.ERR_QueryRangeVariableReadOnly;
		case BindValueKind.AddressOf:
			return ErrorCode.ERR_InvalidAddrOp;
		case BindValueKind.ReadonlyRef:
		case BindValueKind.RefReturn:
			return ErrorCode.ERR_RefReturnRangeVariable;
		case BindValueKind.RefAssignable:
			return ErrorCode.ERR_RefLocalOrParamExpected;
		default:
			if (RequiresReferenceToLocation(kind))
			{
				return ErrorCode.ERR_QueryOutRefRangeVariable;
			}
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private static ErrorCode GetMethodGroupLvalueError(BindValueKind valueKind)
	{
		if (valueKind == BindValueKind.AddressOf)
		{
			return ErrorCode.ERR_InvalidAddrOp;
		}
		if (RequiresReferenceToLocation(valueKind))
		{
			return ErrorCode.ERR_RefReadonlyLocalCause;
		}
		return ErrorCode.ERR_AssgReadonlyLocalCause;
	}

	private static ErrorCode GetStandardLvalueError(BindValueKind kind)
	{
		switch (kind)
		{
		case BindValueKind.Assignable:
		case BindValueKind.CompoundAssignment:
			return ErrorCode.ERR_AssgLvalueExpected;
		case BindValueKind.AddressOf:
			return ErrorCode.ERR_InvalidAddrOp;
		case BindValueKind.IncrementDecrement:
			return ErrorCode.ERR_IncrementLvalueExpected;
		case BindValueKind.FixedReceiver:
			return ErrorCode.ERR_FixedNeedsLvalue;
		case BindValueKind.ReadonlyRef:
		case BindValueKind.RefReturn:
			return ErrorCode.ERR_RefReturnLvalueExpected;
		case BindValueKind.RefAssignable:
			return ErrorCode.ERR_RefLocalOrParamExpected;
		default:
			if (RequiresReferenceToLocation(kind))
			{
				return ErrorCode.ERR_RefLvalueExpected;
			}
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private static ErrorCode GetStandardRValueRefEscapeError(uint escapeTo)
	{
		if (escapeTo == 0)
		{
			return ErrorCode.ERR_RefReturnLvalueExpected;
		}
		return ErrorCode.ERR_EscapeOther;
	}

	private static void ReportReadOnlyFieldError(FieldSymbol field, SyntaxNode node, BindValueKind kind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		if (kind == BindValueKind.AddressOf)
		{
			Error(diagnostics, ErrorCode.ERR_InvalidAddrOp, node);
			return;
		}
		ErrorCode[] array = new ErrorCode[12]
		{
			ErrorCode.ERR_RefReturnReadonly,
			ErrorCode.ERR_RefReadonly,
			ErrorCode.ERR_AssgReadonly,
			ErrorCode.ERR_RefReturnReadonlyStatic,
			ErrorCode.ERR_RefReadonlyStatic,
			ErrorCode.ERR_AssgReadonlyStatic,
			ErrorCode.ERR_RefReturnReadonly2,
			ErrorCode.ERR_RefReadonly2,
			ErrorCode.ERR_AssgReadonly2,
			ErrorCode.ERR_RefReturnReadonlyStatic2,
			ErrorCode.ERR_RefReadonlyStatic2,
			ErrorCode.ERR_AssgReadonlyStatic2
		};
		int num = (checkingReceiver ? 6 : 0) + (field.IsStatic ? 3 : 0) + ((kind != BindValueKind.RefReturn) ? (RequiresRefOrOut(kind) ? 1 : 2) : 0);
		if (checkingReceiver)
		{
			Error(diagnostics, array[num], node, field);
		}
		else
		{
			Error(diagnostics, array[num], node);
		}
	}

	private static void ReportReadOnlyError(Symbol symbol, SyntaxNode node, BindValueKind kind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		if (kind == BindValueKind.AddressOf)
		{
			Error(diagnostics, ErrorCode.ERR_InvalidAddrOp, node);
			return;
		}
		LocalizableErrorArgument localizableErrorArgument = symbol.Kind.Localize();
		ErrorCode[] array = new ErrorCode[6]
		{
			ErrorCode.ERR_RefReturnReadonlyNotField,
			ErrorCode.ERR_RefReadonlyNotField,
			ErrorCode.ERR_AssignReadonlyNotField,
			ErrorCode.ERR_RefReturnReadonlyNotField2,
			ErrorCode.ERR_RefReadonlyNotField2,
			ErrorCode.ERR_AssignReadonlyNotField2
		};
		int num = (checkingReceiver ? 3 : 0) + ((kind != BindValueKind.RefReturn) ? (RequiresRefOrOut(kind) ? 1 : 2) : 0);
		Error(diagnostics, array[num], node, localizableErrorArgument, symbol);
	}

	internal BoundExpression ValidateEscape(BoundExpression expr, uint escapeTo, bool isByRef, DiagnosticBag diagnostics)
	{
		if (isByRef)
		{
			if (CheckRefEscape(expr.Syntax, expr, LocalScopeDepth, escapeTo, checkingReceiver: false, diagnostics))
			{
				return expr;
			}
		}
		else if (CheckValEscape(expr.Syntax, expr, LocalScopeDepth, escapeTo, checkingReceiver: false, diagnostics))
		{
			return expr;
		}
		return ToBadExpression(expr);
	}

	internal static uint GetRefEscape(BoundExpression expr, uint scopeOfTheContainingExpression)
	{
		if (expr.HasAnyErrors)
		{
			return 0u;
		}
		TypeSymbol type = expr.Type;
		if ((object)type != null && type.GetSpecialTypeSafe() == SpecialType.System_Void)
		{
			return 0u;
		}
		if (expr.ConstantValue != null)
		{
			return scopeOfTheContainingExpression;
		}
		switch (expr.Kind)
		{
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.ArrayAccess:
			return 0u;
		case BoundKind.RefValueOperator:
			return 1u;
		case BoundKind.Parameter:
			if (((BoundParameter)expr).ParameterSymbol.RefKind != RefKind.None)
			{
				return 0u;
			}
			return 1u;
		case BoundKind.Local:
			return ((BoundLocal)expr).LocalSymbol.RefEscapeScope;
		case BoundKind.ThisReference:
			if (((BoundThisReference)expr).Type.IsValueType)
			{
				return 1u;
			}
			break;
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			if (boundConditionalOperator.IsRef)
			{
				return Math.Max(GetRefEscape(boundConditionalOperator.Consequence, scopeOfTheContainingExpression), GetRefEscape(boundConditionalOperator.Alternative, scopeOfTheContainingExpression));
			}
			break;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (fieldSymbol.IsStatic || fieldSymbol.ContainingType.IsReferenceType)
			{
				return 0u;
			}
			return GetRefEscape(boundFieldAccess.ReceiverOpt, scopeOfTheContainingExpression);
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			if (boundEventAccess.IsUsableAsField)
			{
				EventSymbol eventSymbol = boundEventAccess.EventSymbol;
				if (eventSymbol.IsStatic || eventSymbol.ContainingType.IsReferenceType)
				{
					return 0u;
				}
				return GetRefEscape(boundEventAccess.ReceiverOpt, scopeOfTheContainingExpression);
			}
			break;
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expr;
			MethodSymbol method = boundCall.Method;
			if (method.RefKind != RefKind.None)
			{
				return GetInvocationEscapeScope(boundCall.Method, boundCall.ReceiverOpt, method.Parameters, boundCall.Arguments, boundCall.ArgumentRefKindsOpt, boundCall.ArgsToParamsOpt, scopeOfTheContainingExpression, isRefEscape: true);
			}
			break;
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			return GetInvocationEscapeScope(indexer, boundIndexerAccess.ReceiverOpt, indexer.Parameters, boundIndexerAccess.Arguments, boundIndexerAccess.ArgumentRefKindsOpt, boundIndexerAccess.ArgsToParamsOpt, scopeOfTheContainingExpression, isRefEscape: true);
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			return GetInvocationEscapeScope(boundPropertyAccess.PropertySymbol, boundPropertyAccess.ReceiverOpt, default, default, default, default, scopeOfTheContainingExpression, isRefEscape: true);
		}
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			if (boundAssignmentOperator.IsRef)
			{
				return GetRefEscape(boundAssignmentOperator.Left, scopeOfTheContainingExpression);
			}
			break;
		}
		}
		return scopeOfTheContainingExpression;
	}

	internal static bool CheckRefEscape(SyntaxNode node, BoundExpression expr, uint escapeFrom, uint escapeTo, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		if (escapeTo >= escapeFrom)
		{
			return true;
		}
		if (expr.HasAnyErrors)
		{
			return true;
		}
		TypeSymbol type = expr.Type;
		if ((object)type != null && type.GetSpecialTypeSafe() == SpecialType.System_Void)
		{
			return true;
		}
		if (expr.ConstantValue != null)
		{
			Error(diagnostics, GetStandardRValueRefEscapeError(escapeTo), node);
			return false;
		}
		switch (expr.Kind)
		{
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.ArrayAccess:
			return true;
		case BoundKind.RefValueOperator:
			if (escapeTo != 0)
			{
				return true;
			}
			break;
		case BoundKind.Parameter:
		{
			BoundParameter parameter = (BoundParameter)expr;
			return CheckParameterRefEscape(node, parameter, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.Local:
		{
			BoundLocal local = (BoundLocal)expr;
			return CheckLocalRefEscape(node, local, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.ThisReference:
			if (((BoundThisReference)expr).Type.IsValueType)
			{
				if (escapeTo == 0)
				{
					Error(diagnostics, ErrorCode.ERR_RefReturnStructThis, node, "this");
					return false;
				}
				return true;
			}
			break;
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			if (boundConditionalOperator.IsRef)
			{
				if (CheckRefEscape(boundConditionalOperator.Consequence.Syntax, boundConditionalOperator.Consequence, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
				{
					return CheckRefEscape(boundConditionalOperator.Alternative.Syntax, boundConditionalOperator.Alternative, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
				}
				return false;
			}
			break;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess fieldAccess = (BoundFieldAccess)expr;
			return CheckFieldRefEscape(node, fieldAccess, escapeFrom, escapeTo, diagnostics);
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			if (boundEventAccess.IsUsableAsField)
			{
				return CheckFieldLikeEventRefEscape(node, boundEventAccess, escapeFrom, escapeTo, diagnostics);
			}
			break;
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expr;
			MethodSymbol method = boundCall.Method;
			if (method.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundCall.Syntax, method, boundCall.ReceiverOpt, method.Parameters, boundCall.Arguments, boundCall.ArgumentRefKindsOpt, boundCall.ArgsToParamsOpt, checkingReceiver, escapeFrom, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			if (indexer.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundIndexerAccess.Syntax, indexer, boundIndexerAccess.ReceiverOpt, indexer.Parameters, boundIndexerAccess.Arguments, boundIndexerAccess.ArgumentRefKindsOpt, boundIndexerAccess.ArgsToParamsOpt, checkingReceiver, escapeFrom, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			if (propertySymbol.RefKind != RefKind.None)
			{
				return CheckInvocationEscape(boundPropertyAccess.Syntax, propertySymbol, boundPropertyAccess.ReceiverOpt, default, default, default, default, checkingReceiver, escapeFrom, escapeTo, diagnostics, isRefEscape: true);
			}
			break;
		}
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			if (boundAssignmentOperator.IsRef)
			{
				return CheckRefEscape(node, boundAssignmentOperator.Left, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
			}
			break;
		}
		}
		Error(diagnostics, GetStandardRValueRefEscapeError(escapeTo), node);
		return false;
	}

	internal static uint GetBroadestValEscape(BoundTupleExpression expr, uint scopeOfTheContainingExpression)
	{
		uint num = scopeOfTheContainingExpression;
		foreach (BoundExpression argument in expr.Arguments)
		{
			uint val = ((argument.Kind != BoundKind.TupleLiteral) ? GetValEscape(argument, scopeOfTheContainingExpression) : GetBroadestValEscape((BoundTupleExpression)argument, scopeOfTheContainingExpression));
			num = Math.Min(num, val);
		}
		return num;
	}

	internal static uint GetValEscape(BoundExpression expr, uint scopeOfTheContainingExpression)
	{
		if (expr.HasAnyErrors)
		{
			return 0u;
		}
		if (expr.ConstantValue != null)
		{
			return 0u;
		}
		TypeSymbol type = expr.Type;
		if ((object)type == null || !type.IsByRefLikeType)
		{
			return 0u;
		}
		switch (expr.Kind)
		{
		case BoundKind.DefaultExpression:
		case BoundKind.ThisReference:
		case BoundKind.Parameter:
			return 0u;
		case BoundKind.TupleLiteral:
			return GetTupleValEscape(((BoundTupleLiteral)expr).Arguments, scopeOfTheContainingExpression);
		case BoundKind.ConvertedTupleLiteral:
			return GetTupleValEscape(((BoundConvertedTupleLiteral)expr).Arguments, scopeOfTheContainingExpression);
		case BoundKind.MakeRefOperator:
		case BoundKind.RefValueOperator:
			return 0u;
		case BoundKind.DiscardExpression:
			return 0u;
		case BoundKind.DeconstructValuePlaceholder:
			return ((BoundDeconstructValuePlaceholder)expr).ValEscape;
		case BoundKind.Local:
			return ((BoundLocal)expr).LocalSymbol.ValEscapeScope;
		case BoundKind.StackAllocArrayCreation:
		case BoundKind.ConvertedStackAllocExpression:
			return 1u;
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			uint valEscape = GetValEscape(boundConditionalOperator.Consequence, scopeOfTheContainingExpression);
			if (boundConditionalOperator.IsRef)
			{
				return valEscape;
			}
			return Math.Max(valEscape, GetValEscape(boundConditionalOperator.Alternative, scopeOfTheContainingExpression));
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (fieldSymbol.IsStatic || !fieldSymbol.ContainingType.IsByRefLikeType)
			{
				return 0u;
			}
			return GetValEscape(boundFieldAccess.ReceiverOpt, scopeOfTheContainingExpression);
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expr;
			return GetInvocationEscapeScope(boundCall.Method, boundCall.ReceiverOpt, boundCall.Method.Parameters, boundCall.Arguments, boundCall.ArgumentRefKindsOpt, boundCall.ArgsToParamsOpt, scopeOfTheContainingExpression, isRefEscape: false);
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			return GetInvocationEscapeScope(indexer, boundIndexerAccess.ReceiverOpt, indexer.Parameters, boundIndexerAccess.Arguments, boundIndexerAccess.ArgumentRefKindsOpt, boundIndexerAccess.ArgsToParamsOpt, scopeOfTheContainingExpression, isRefEscape: false);
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			return GetInvocationEscapeScope(boundPropertyAccess.PropertySymbol, boundPropertyAccess.ReceiverOpt, default, default, default, default, scopeOfTheContainingExpression, isRefEscape: false);
		}
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expr;
			MethodSymbol constructor = boundObjectCreationExpression.Constructor;
			uint num = GetInvocationEscapeScope(constructor, null, constructor.Parameters, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.ArgumentRefKindsOpt, boundObjectCreationExpression.ArgsToParamsOpt, scopeOfTheContainingExpression, isRefEscape: false);
			BoundObjectInitializerExpressionBase initializerExpressionOpt = boundObjectCreationExpression.InitializerExpressionOpt;
			if (initializerExpressionOpt != null)
			{
				num = Math.Max(num, GetValEscape(initializerExpressionOpt, scopeOfTheContainingExpression));
			}
			return num;
		}
		case BoundKind.UnaryOperator:
			return GetValEscape(((BoundUnaryOperator)expr).Operand, scopeOfTheContainingExpression);
		case BoundKind.Conversion:
			return GetValEscape(((BoundConversion)expr).Operand, scopeOfTheContainingExpression);
		case BoundKind.AssignmentOperator:
			return GetValEscape(((BoundAssignmentOperator)expr).Right, scopeOfTheContainingExpression);
		case BoundKind.IncrementOperator:
			return GetValEscape(((BoundIncrementOperator)expr).Operand, scopeOfTheContainingExpression);
		case BoundKind.CompoundAssignmentOperator:
		{
			BoundCompoundAssignmentOperator boundCompoundAssignmentOperator = (BoundCompoundAssignmentOperator)expr;
			return Math.Max(GetValEscape(boundCompoundAssignmentOperator.Left, scopeOfTheContainingExpression), GetValEscape(boundCompoundAssignmentOperator.Right, scopeOfTheContainingExpression));
		}
		case BoundKind.BinaryOperator:
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)expr;
			return Math.Max(GetValEscape(boundBinaryOperator.Left, scopeOfTheContainingExpression), GetValEscape(boundBinaryOperator.Right, scopeOfTheContainingExpression));
		}
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)expr;
			return Math.Max(GetValEscape(boundUserDefinedConditionalLogicalOperator.Left, scopeOfTheContainingExpression), GetValEscape(boundUserDefinedConditionalLogicalOperator.Right, scopeOfTheContainingExpression));
		}
		case BoundKind.QueryClause:
			return GetValEscape(((BoundQueryClause)expr).Value, scopeOfTheContainingExpression);
		case BoundKind.RangeVariable:
			return GetValEscape(((BoundRangeVariable)expr).Value, scopeOfTheContainingExpression);
		case BoundKind.ObjectInitializerExpression:
			return GetValEscapeOfObjectInitializer((BoundObjectInitializerExpression)expr, scopeOfTheContainingExpression);
		case BoundKind.CollectionInitializerExpression:
			return GetValEscape(((BoundCollectionInitializerExpression)expr).Initializers, scopeOfTheContainingExpression);
		case BoundKind.CollectionElementInitializer:
			return GetValEscape(((BoundCollectionElementInitializer)expr).Arguments, scopeOfTheContainingExpression);
		case BoundKind.ObjectInitializerMember:
			return scopeOfTheContainingExpression;
		case BoundKind.ImplicitReceiver:
			return scopeOfTheContainingExpression;
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
			return 0u;
		case BoundKind.NullCoalescingOperator:
		case BoundKind.ArrayAccess:
		case BoundKind.AwaitExpression:
		case BoundKind.AsOperator:
		case BoundKind.ConditionalAccess:
			return scopeOfTheContainingExpression;
		default:
			return scopeOfTheContainingExpression;
		}
	}

	private static uint GetTupleValEscape(ImmutableArray<BoundExpression> elements, uint scopeOfTheContainingExpression)
	{
		uint num = scopeOfTheContainingExpression;
		foreach (BoundExpression item in elements)
		{
			num = Math.Max(num, GetValEscape(item, scopeOfTheContainingExpression));
		}
		return num;
	}

	private static uint GetValEscapeOfObjectInitializer(BoundObjectInitializerExpression initExpr, uint scopeOfTheContainingExpression)
	{
		uint num = 0u;
		foreach (BoundExpression initializer in initExpr.Initializers)
		{
			if (initializer.Kind == BoundKind.AssignmentOperator)
			{
				BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)initializer;
				num = Math.Max(num, GetValEscape(boundAssignmentOperator.Right, scopeOfTheContainingExpression));
				BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)boundAssignmentOperator.Left;
				num = Math.Max(num, GetValEscape(boundObjectInitializerMember.Arguments, scopeOfTheContainingExpression));
			}
			else
			{
				num = Math.Max(num, GetValEscape(initializer, scopeOfTheContainingExpression));
			}
		}
		return num;
	}

	private static uint GetValEscape(ImmutableArray<BoundExpression> expressions, uint scopeOfTheContainingExpression)
	{
		uint num = 0u;
		foreach (BoundExpression item in expressions)
		{
			num = Math.Max(num, GetValEscape(item, scopeOfTheContainingExpression));
		}
		return num;
	}

	internal static bool CheckValEscape(SyntaxNode node, BoundExpression expr, uint escapeFrom, uint escapeTo, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		if (escapeTo >= escapeFrom)
		{
			return true;
		}
		if (expr.HasAnyErrors)
		{
			return true;
		}
		if (expr.ConstantValue != null)
		{
			return true;
		}
		TypeSymbol type = expr.Type;
		if ((object)type == null || !type.IsByRefLikeType)
		{
			return true;
		}
		switch (expr.Kind)
		{
		case BoundKind.DefaultExpression:
		case BoundKind.ThisReference:
		case BoundKind.Parameter:
			return true;
		case BoundKind.TupleLiteral:
			return CheckTupleValEscape(((BoundTupleLiteral)expr).Arguments, escapeFrom, escapeTo, diagnostics);
		case BoundKind.ConvertedTupleLiteral:
			return CheckTupleValEscape(((BoundConvertedTupleLiteral)expr).Arguments, escapeFrom, escapeTo, diagnostics);
		case BoundKind.MakeRefOperator:
		case BoundKind.RefValueOperator:
			return true;
		case BoundKind.DiscardExpression:
			return true;
		case BoundKind.DeconstructValuePlaceholder:
		{
			BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder = (BoundDeconstructValuePlaceholder)expr;
			if (boundDeconstructValuePlaceholder.ValEscape > escapeTo)
			{
				Error(diagnostics, ErrorCode.ERR_EscapeLocal, node, boundDeconstructValuePlaceholder.Syntax);
				return false;
			}
			return true;
		}
		case BoundKind.Local:
		{
			LocalSymbol localSymbol = ((BoundLocal)expr).LocalSymbol;
			if (localSymbol.ValEscapeScope > escapeTo)
			{
				Error(diagnostics, ErrorCode.ERR_EscapeLocal, node, localSymbol);
				return false;
			}
			return true;
		}
		case BoundKind.StackAllocArrayCreation:
		case BoundKind.ConvertedStackAllocExpression:
			if (escapeTo < 1)
			{
				Error(diagnostics, ErrorCode.ERR_EscapeStackAlloc, node, expr.Type);
				return false;
			}
			return true;
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expr;
			bool flag = CheckValEscape(boundConditionalOperator.Consequence.Syntax, boundConditionalOperator.Consequence, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
			if (!flag || boundConditionalOperator.IsRef)
			{
				return flag;
			}
			return CheckValEscape(boundConditionalOperator.Alternative.Syntax, boundConditionalOperator.Alternative, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (fieldSymbol.IsStatic || !fieldSymbol.ContainingType.IsByRefLikeType)
			{
				return true;
			}
			return CheckValEscape(node, boundFieldAccess.ReceiverOpt, escapeFrom, escapeTo, checkingReceiver: true, diagnostics);
		}
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expr;
			MethodSymbol method = boundCall.Method;
			return CheckInvocationEscape(boundCall.Syntax, method, boundCall.ReceiverOpt, method.Parameters, boundCall.Arguments, boundCall.ArgumentRefKindsOpt, boundCall.ArgsToParamsOpt, checkingReceiver, escapeFrom, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			return CheckInvocationEscape(boundIndexerAccess.Syntax, indexer, boundIndexerAccess.ReceiverOpt, indexer.Parameters, boundIndexerAccess.Arguments, boundIndexerAccess.ArgumentRefKindsOpt, boundIndexerAccess.ArgsToParamsOpt, checkingReceiver, escapeFrom, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			return CheckInvocationEscape(boundPropertyAccess.Syntax, boundPropertyAccess.PropertySymbol, boundPropertyAccess.ReceiverOpt, default, default, default, default, checkingReceiver, escapeFrom, escapeTo, diagnostics, isRefEscape: false);
		}
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expr;
			MethodSymbol constructor = boundObjectCreationExpression.Constructor;
			bool flag2 = CheckInvocationEscape(boundObjectCreationExpression.Syntax, constructor, null, constructor.Parameters, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.ArgumentRefKindsOpt, boundObjectCreationExpression.ArgsToParamsOpt, checkingReceiver, escapeFrom, escapeTo, diagnostics, isRefEscape: false);
			BoundObjectInitializerExpressionBase initializerExpressionOpt = boundObjectCreationExpression.InitializerExpressionOpt;
			if (initializerExpressionOpt != null)
			{
				flag2 = flag2 && CheckValEscape(initializerExpressionOpt.Syntax, initializerExpressionOpt, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
			}
			return flag2;
		}
		case BoundKind.UnaryOperator:
		{
			BoundUnaryOperator boundUnaryOperator = (BoundUnaryOperator)expr;
			return CheckValEscape(node, boundUnaryOperator.Operand, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			return CheckValEscape(node, boundConversion.Operand, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			return CheckValEscape(node, boundAssignmentOperator.Left, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.IncrementOperator:
		{
			BoundIncrementOperator boundIncrementOperator = (BoundIncrementOperator)expr;
			return CheckValEscape(node, boundIncrementOperator.Operand, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.CompoundAssignmentOperator:
		{
			BoundCompoundAssignmentOperator boundCompoundAssignmentOperator = (BoundCompoundAssignmentOperator)expr;
			if (CheckValEscape(boundCompoundAssignmentOperator.Left.Syntax, boundCompoundAssignmentOperator.Left, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
			{
				return CheckValEscape(boundCompoundAssignmentOperator.Right.Syntax, boundCompoundAssignmentOperator.Right, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.BinaryOperator:
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)expr;
			if (CheckValEscape(boundBinaryOperator.Left.Syntax, boundBinaryOperator.Left, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
			{
				return CheckValEscape(boundBinaryOperator.Right.Syntax, boundBinaryOperator.Right, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)expr;
			if (CheckValEscape(boundUserDefinedConditionalLogicalOperator.Left.Syntax, boundUserDefinedConditionalLogicalOperator.Left, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
			{
				return CheckValEscape(boundUserDefinedConditionalLogicalOperator.Right.Syntax, boundUserDefinedConditionalLogicalOperator.Right, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
			}
			return false;
		}
		case BoundKind.QueryClause:
		{
			BoundExpression value2 = ((BoundQueryClause)expr).Value;
			return CheckValEscape(value2.Syntax, value2, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.RangeVariable:
		{
			BoundExpression value = ((BoundRangeVariable)expr).Value;
			return CheckValEscape(value.Syntax, value, escapeFrom, escapeTo, checkingReceiver: false, diagnostics);
		}
		case BoundKind.ObjectInitializerExpression:
			return CheckValEscapeOfObjectInitializer((BoundObjectInitializerExpression)expr, escapeFrom, escapeTo, diagnostics);
		case BoundKind.CollectionInitializerExpression:
			return CheckValEscape(((BoundCollectionInitializerExpression)expr).Initializers, escapeFrom, escapeTo, diagnostics);
		case BoundKind.CollectionElementInitializer:
			return CheckValEscape(((BoundCollectionElementInitializer)expr).Arguments, escapeFrom, escapeTo, diagnostics);
		case BoundKind.PointerElementAccess:
		{
			BoundExpression expression = ((BoundPointerElementAccess)expr).Expression;
			return CheckValEscape(expression.Syntax, expression, escapeFrom, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.PointerIndirectionOperator:
		{
			BoundExpression operand = ((BoundPointerIndirectionOperator)expr).Operand;
			return CheckValEscape(operand.Syntax, operand, escapeFrom, escapeTo, checkingReceiver, diagnostics);
		}
		case BoundKind.NullCoalescingOperator:
		case BoundKind.ArrayAccess:
		case BoundKind.AwaitExpression:
		case BoundKind.AsOperator:
		case BoundKind.ConditionalAccess:
			return false;
		default:
			return false;
		}
	}

	private static bool CheckTupleValEscape(ImmutableArray<BoundExpression> elements, uint escapeFrom, uint escapeTo, DiagnosticBag diagnostics)
	{
		foreach (BoundExpression item in elements)
		{
			if (!CheckValEscape(item.Syntax, item, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private static bool CheckValEscapeOfObjectInitializer(BoundObjectInitializerExpression initExpr, uint escapeFrom, uint escapeTo, DiagnosticBag diagnostics)
	{
		foreach (BoundExpression initializer in initExpr.Initializers)
		{
			if (initializer.Kind == BoundKind.AssignmentOperator)
			{
				BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)initializer;
				if (!CheckValEscape(initializer.Syntax, boundAssignmentOperator.Right, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
				{
					return false;
				}
				if (!CheckValEscape(((BoundObjectInitializerMember)boundAssignmentOperator.Left).Arguments, escapeFrom, escapeTo, diagnostics))
				{
					return false;
				}
			}
			else if (!CheckValEscape(initializer.Syntax, initializer, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private static bool CheckValEscape(ImmutableArray<BoundExpression> expressions, uint escapeFrom, uint escapeTo, DiagnosticBag diagnostics)
	{
		foreach (BoundExpression item in expressions)
		{
			if (!CheckValEscape(item.Syntax, item, escapeFrom, escapeTo, checkingReceiver: false, diagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private BoundExpression BindAnonymousObjectCreation(AnonymousObjectCreationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		SeparatedSyntaxList<AnonymousObjectMemberDeclaratorSyntax> initializers = node.Initializers;
		int count = initializers.Count;
		bool hasError = false;
		BoundExpression[] array = new BoundExpression[count];
		AnonymousTypeField[] array2 = new AnonymousTypeField[count];
		CSharpSyntaxNode[] array3 = new CSharpSyntaxNode[count];
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		for (int i = 0; i < count; i++)
		{
			AnonymousObjectMemberDeclaratorSyntax anonymousObjectMemberDeclaratorSyntax = initializers[i];
			NameEqualsSyntax nameEquals = anonymousObjectMemberDeclaratorSyntax.NameEquals;
			ExpressionSyntax expression = anonymousObjectMemberDeclaratorSyntax.Expression;
			SyntaxToken token;
			if (nameEquals != null)
			{
				token = nameEquals.Name.Identifier;
			}
			else
			{
				if (!IsAnonymousTypeMemberExpression(expression))
				{
					hasError = true;
					diagnostics.Add(ErrorCode.ERR_InvalidAnonymousTypeMemberDeclarator, expression.GetLocation());
				}
				token = expression.ExtractAnonymousTypeMemberName();
			}
			hasError |= expression.HasErrors;
			array[i] = BindValue(expression, diagnostics, BindValueKind.RValue);
			string text = null;
			if (token.Kind() == SyntaxKind.IdentifierToken)
			{
				text = token.ValueText;
				if (!instance.Add(text))
				{
					Error(diagnostics, ErrorCode.ERR_AnonymousTypeDuplicatePropertyName, anonymousObjectMemberDeclaratorSyntax);
					hasError = true;
					text = null;
				}
			}
			else
			{
				hasError = true;
			}
			TypeSymbol anonymousTypeFieldType = GetAnonymousTypeFieldType(array[i], anonymousObjectMemberDeclaratorSyntax, diagnostics, ref hasError);
			array3[i] = ((token.Kind() == SyntaxKind.IdentifierToken) ? ((CSharpSyntaxNode)token.Parent) : anonymousObjectMemberDeclaratorSyntax);
			array2[i] = new AnonymousTypeField((text == null) ? ("$" + i) : text, array3[i].Location, anonymousTypeFieldType);
		}
		instance.Free();
		AnonymousTypeManager anonymousTypeManager = Compilation.AnonymousTypeManager;
		AnonymousTypeDescriptor typeDescr = new AnonymousTypeDescriptor(array2.AsImmutableOrNull(), node.NewKeyword.GetLocation());
		NamedTypeSymbol namedTypeSymbol = anonymousTypeManager.ConstructAnonymousTypeSymbol(typeDescr);
		ArrayBuilder<BoundAnonymousPropertyDeclaration> instance2 = ArrayBuilder<BoundAnonymousPropertyDeclaration>.GetInstance();
		for (int j = 0; j < count; j++)
		{
			if (initializers[j].NameEquals == null)
			{
				continue;
			}
			AnonymousTypeField anonymousTypeField = array2[j];
			if (anonymousTypeField.Name == null)
			{
				continue;
			}
			foreach (Symbol member in namedTypeSymbol.GetMembers(anonymousTypeField.Name))
			{
				if (member.Kind == SymbolKind.Property)
				{
					instance2.Add(new BoundAnonymousPropertyDeclaration(array3[j], (PropertySymbol)member, anonymousTypeField.Type));
					break;
				}
			}
		}
		if (!IsAnonymousTypesAllowed())
		{
			Error(diagnostics, ErrorCode.ERR_AnonymousTypeNotAvailable, node.NewKeyword);
			hasError = true;
		}
		return new BoundAnonymousObjectCreationExpression(node, namedTypeSymbol.InstanceConstructors[0], array.AsImmutableOrNull(), instance2.ToImmutableAndFree(), namedTypeSymbol, hasError);
	}

	private static bool IsAnonymousTypeMemberExpression(ExpressionSyntax expr)
	{
		while (true)
		{
			switch (expr.Kind())
			{
			case SyntaxKind.QualifiedName:
				expr = ((QualifiedNameSyntax)expr).Right;
				break;
			case SyntaxKind.ConditionalAccessExpression:
				expr = ((ConditionalAccessExpressionSyntax)expr).WhenNotNull;
				if (expr.Kind() == SyntaxKind.MemberBindingExpression)
				{
					return true;
				}
				break;
			case SyntaxKind.IdentifierName:
			case SyntaxKind.SimpleMemberAccessExpression:
				return true;
			default:
				return false;
			}
		}
	}

	private bool IsAnonymousTypesAllowed()
	{
		Symbol containingMemberOrLambda = ContainingMemberOrLambda;
		if ((object)containingMemberOrLambda == null)
		{
			return false;
		}
		return containingMemberOrLambda.Kind switch
		{
			SymbolKind.Method => true, 
			SymbolKind.Field => !((FieldSymbol)containingMemberOrLambda).IsConst, 
			SymbolKind.NamedType => ((NamedTypeSymbol)containingMemberOrLambda).IsScriptClass, 
			_ => false, 
		};
	}

	private TypeSymbol GetAnonymousTypeFieldType(BoundExpression expression, CSharpSyntaxNode errorSyntax, DiagnosticBag diagnostics, ref bool hasError)
	{
		object obj = null;
		TypeSymbol typeSymbol = expression.Type;
		if (!expression.HasAnyErrors)
		{
			if (expression.HasExpressionType())
			{
				if (typeSymbol.SpecialType == SpecialType.System_Void)
				{
					obj = typeSymbol;
					typeSymbol = CreateErrorType(SyntaxFacts.GetText(SyntaxKind.VoidKeyword));
				}
				else if (typeSymbol.IsUnsafe())
				{
					obj = typeSymbol;
				}
				else if (typeSymbol.IsRestrictedType())
				{
					obj = typeSymbol;
				}
			}
			else
			{
				obj = expression.Display;
			}
		}
		if ((object)typeSymbol == null)
		{
			typeSymbol = CreateErrorType("error");
		}
		if (obj != null)
		{
			hasError = true;
			Error(diagnostics, ErrorCode.ERR_AnonymousTypePropertyAssignedBadValue, errorSyntax, obj);
		}
		return typeSymbol;
	}

	internal static void BindAttributeTypes(ImmutableArray<Binder> binders, ImmutableArray<AttributeSyntax> attributesToBind, Symbol ownerSymbol, NamedTypeSymbol[] boundAttributeTypes, DiagnosticBag diagnostics)
	{
		for (int i = 0; i < attributesToBind.Length; i++)
		{
			if ((object)boundAttributeTypes[i] == null)
			{
				Binder binder = binders[i];
				boundAttributeTypes[i] = (NamedTypeSymbol)binder.BindType(attributesToBind[i].Name, diagnostics);
			}
		}
	}

	internal static void GetAttributes(ImmutableArray<Binder> binders, ImmutableArray<AttributeSyntax> attributesToBind, ImmutableArray<NamedTypeSymbol> boundAttributeTypes, CSharpAttributeData[] attributesBuilder, DiagnosticBag diagnostics)
	{
		for (int i = 0; i < attributesToBind.Length; i++)
		{
			AttributeSyntax attributeSyntax = attributesToBind[i];
			NamedTypeSymbol boundAttributeType = boundAttributeTypes[i];
			Binder binder = binders[i];
			SourceAttributeData sourceAttributeData = (SourceAttributeData)attributesBuilder[i];
			if (sourceAttributeData == null)
			{
				attributesBuilder[i] = binder.GetAttribute(attributeSyntax, boundAttributeType, diagnostics);
				continue;
			}
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			bool isConditionallyOmitted = binder.IsAttributeConditionallyOmitted(sourceAttributeData.AttributeClass, attributeSyntax.SyntaxTree, ref useSiteDiagnostics);
			diagnostics.Add(attributeSyntax, useSiteDiagnostics);
			attributesBuilder[i] = sourceAttributeData.WithOmittedCondition(isConditionallyOmitted);
		}
	}

	internal CSharpAttributeData GetAttribute(AttributeSyntax node, NamedTypeSymbol boundAttributeType, DiagnosticBag diagnostics)
	{
		BoundAttribute boundAttribute = new ExecutableCodeBinder(node, ContainingMemberOrLambda, this).BindAttribute(node, boundAttributeType, diagnostics);
		return GetAttribute(boundAttribute, diagnostics);
	}

	internal BoundAttribute BindAttribute(AttributeSyntax node, NamedTypeSymbol attributeType, DiagnosticBag diagnostics)
	{
		return GetBinder(node).BindAttributeCore(node, attributeType, diagnostics);
	}

	private Binder SkipSemanticModelBinder()
	{
		Binder binder = this;
		while (binder.IsSemanticModelBinder)
		{
			binder = binder.Next;
		}
		return binder;
	}

	private BoundAttribute BindAttributeCore(AttributeSyntax node, NamedTypeSymbol attributeType, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = attributeType;
		LookupResultKind resultKind = LookupResultKind.Viable;
		if (namedTypeSymbol.IsErrorType())
		{
			ErrorTypeSymbol errorTypeSymbol = (ErrorTypeSymbol)namedTypeSymbol;
			resultKind = errorTypeSymbol.ResultKind;
			if (errorTypeSymbol.CandidateSymbols.Length == 1 && errorTypeSymbol.CandidateSymbols[0] is NamedTypeSymbol)
			{
				namedTypeSymbol = (NamedTypeSymbol)errorTypeSymbol.CandidateSymbols[0];
			}
		}
		AttributeArgumentListSyntax argumentList = node.ArgumentList;
		AnalyzedAttributeArguments analyzedAttributeArguments = WithAdditionalFlags(BinderFlags.AttributeArgument).BindAttributeArguments(argumentList, namedTypeSymbol, diagnostics);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		MethodSymbol methodSymbol = (namedTypeSymbol.IsErrorType() ? null : BindAttributeConstructor(node, namedTypeSymbol, analyzedAttributeArguments.ConstructorArguments, diagnostics, ref resultKind, attributeType.IsErrorType(), ref useSiteDiagnostics));
		diagnostics.Add(node, useSiteDiagnostics);
		if ((object)methodSymbol != null)
		{
			ReportDiagnosticsIfObsolete(diagnostics, methodSymbol, node, hasBaseReceiver: false);
		}
		if ((object)methodSymbol != null && methodSymbol.Parameters.Any((ParameterSymbol p) => p.RefKind == RefKind.In))
		{
			Error(diagnostics, ErrorCode.ERR_AttributeCtorInParameter, node, methodSymbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
		}
		AnalyzedArguments constructorArguments = analyzedAttributeArguments.ConstructorArguments;
		ImmutableArray<BoundExpression> constructorArguments2 = constructorArguments.Arguments.ToImmutableAndFree();
		ImmutableArray<string> names = constructorArguments.GetNames();
		ImmutableArray<BoundExpression> namedArguments = analyzedAttributeArguments.NamedArguments;
		constructorArguments.Free();
		return new BoundAttribute(node, methodSymbol, constructorArguments2, names, namedArguments, resultKind, attributeType, resultKind != LookupResultKind.Viable);
	}

	private CSharpAttributeData GetAttribute(BoundAttribute boundAttribute, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)boundAttribute.Type;
		MethodSymbol constructor = boundAttribute.Constructor;
		bool hasErrors = boundAttribute.HasAnyErrors;
		if (namedTypeSymbol.IsErrorType() || namedTypeSymbol.IsAbstract || (object)constructor == null)
		{
			return new SourceAttributeData(boundAttribute.Syntax.GetReference(), namedTypeSymbol, constructor, hasErrors);
		}
		ValidateTypeForAttributeParameters(constructor.Parameters, ((AttributeSyntax)boundAttribute.Syntax).Name, diagnostics, ref hasErrors);
		AttributeExpressionVisitor attributeExpressionVisitor = new AttributeExpressionVisitor(this);
		ImmutableArray<BoundExpression> constructorArguments = boundAttribute.ConstructorArguments;
		ImmutableArray<TypedConstant> immutableArray = attributeExpressionVisitor.VisitArguments(constructorArguments, diagnostics, ref hasErrors);
		ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments = attributeExpressionVisitor.VisitNamedArguments(boundAttribute.NamedArguments, diagnostics, ref hasErrors);
		ImmutableArray<int> constructorArgumentsSourceIndices;
		ImmutableArray<TypedConstant> constructorArguments2;
		if (hasErrors || constructor.ParameterCount == 0)
		{
			constructorArgumentsSourceIndices = default;
			constructorArguments2 = immutableArray;
		}
		else
		{
			constructorArguments2 = GetRewrittenAttributeConstructorArguments(out constructorArgumentsSourceIndices, constructor, immutableArray, boundAttribute.ConstructorArgumentNamesOpt, (AttributeSyntax)boundAttribute.Syntax, diagnostics, ref hasErrors);
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		bool isConditionallyOmitted = IsAttributeConditionallyOmitted(namedTypeSymbol, boundAttribute.SyntaxTree, ref useSiteDiagnostics);
		diagnostics.Add(boundAttribute.Syntax, useSiteDiagnostics);
		return new SourceAttributeData(boundAttribute.Syntax.GetReference(), namedTypeSymbol, constructor, constructorArguments2, constructorArgumentsSourceIndices, namedArguments, hasErrors, isConditionallyOmitted);
	}

	private void ValidateTypeForAttributeParameters(ImmutableArray<ParameterSymbol> parameters, CSharpSyntaxNode syntax, DiagnosticBag diagnostics, ref bool hasErrors)
	{
		foreach (ParameterSymbol item in parameters)
		{
			TypeSymbol type = item.Type;
			if (!type.IsValidAttributeParameterType(Compilation))
			{
				Error(diagnostics, ErrorCode.ERR_BadAttributeParamType, syntax, item.Name, type);
				hasErrors = true;
			}
		}
	}

	protected bool IsAttributeConditionallyOmitted(NamedTypeSymbol attributeType, SyntaxTree syntaxTree, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (IsEarlyAttributeBinder)
		{
			return false;
		}
		if (attributeType.IsConditional)
		{
			ImmutableArray<string> appliedConditionalSymbols = attributeType.GetAppliedConditionalSymbols();
			if (syntaxTree.IsAnyPreprocessorSymbolDefined(appliedConditionalSymbols))
			{
				return false;
			}
			NamedTypeSymbol namedTypeSymbol = attributeType.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
			if ((object)namedTypeSymbol != null && namedTypeSymbol.IsConditional)
			{
				return IsAttributeConditionallyOmitted(namedTypeSymbol, syntaxTree, ref useSiteDiagnostics);
			}
			return true;
		}
		return false;
	}

	private AnalyzedAttributeArguments BindAttributeArguments(AttributeArgumentListSyntax attributeArgumentList, NamedTypeSymbol attributeType, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		ImmutableArray<BoundExpression> namedArguments = ImmutableArray<BoundExpression>.Empty;
		if (attributeArgumentList != null)
		{
			ArrayBuilder<BoundExpression> arrayBuilder = null;
			HashSet<string> hashSet = null;
			bool hadLangVersionError = false;
			bool flag = false;
			foreach (AttributeArgumentSyntax argument in attributeArgumentList.Arguments)
			{
				if (argument.NameEquals == null)
				{
					if (flag)
					{
						diagnostics.Add(ErrorCode.ERR_NamedArgumentExpected, argument.Expression.GetLocation());
					}
					BindArgumentAndName(instance, diagnostics, ref hadLangVersionError, argument, BindArgumentExpression(diagnostics, argument.Expression, RefKind.None, allowArglist: false), argument.NameColon, RefKind.None);
					continue;
				}
				flag = true;
				string valueText = argument.NameEquals.Name.Identifier.ValueText;
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance();
					hashSet = new HashSet<string>();
				}
				else if (hashSet.Contains(valueText))
				{
					Error(diagnostics, ErrorCode.ERR_DuplicateNamedAttributeArgument, argument, valueText);
				}
				BoundExpression item = BindNamedAttributeArgument(argument, attributeType, diagnostics);
				arrayBuilder.Add(item);
				hashSet.Add(valueText);
			}
			if (arrayBuilder != null)
			{
				namedArguments = arrayBuilder.ToImmutableAndFree();
			}
		}
		return new AnalyzedAttributeArguments(instance, namedArguments);
	}

	private BoundExpression BindNamedAttributeArgument(AttributeArgumentSyntax namedArgument, NamedTypeSymbol attributeType, DiagnosticBag diagnostics)
	{
		Symbol symbol = BindNamedAttributeArgumentName(namedArgument, attributeType, diagnostics, out var wasError, out var resultKind);
		ReportDiagnosticsIfObsolete(diagnostics, symbol, namedArgument, hasBaseReceiver: false);
		TypeSymbol typeSymbol = ((!wasError) ? BindNamedAttributeArgumentType(namedArgument, symbol, attributeType, diagnostics) : CreateErrorType());
		BoundExpression expression = BindValue(namedArgument.Expression, diagnostics, BindValueKind.RValue);
		expression = GenerateConversionForAssignment(typeSymbol, expression, diagnostics);
		FieldSymbol fieldSymbol = symbol as FieldSymbol;
		IdentifierNameSyntax name = namedArgument.NameEquals.Name;
		BoundExpression left;
		if ((object)fieldSymbol == null)
		{
			left = ((!(symbol is PropertySymbol propertySymbol)) ? ((BoundExpression)BadExpression(name, resultKind)) : ((BoundExpression)new BoundPropertyAccess(name, null, propertySymbol, resultKind, typeSymbol)));
		}
		else
		{
			(fieldSymbol.ContainingAssembly as SourceAssemblySymbol)?.NoteFieldAccess(fieldSymbol, read: true, write: true);
			left = new BoundFieldAccess(name, null, fieldSymbol, null, resultKind, fieldSymbol.Type);
		}
		return new BoundAssignmentOperator(namedArgument, left, expression, typeSymbol);
	}

	private Symbol BindNamedAttributeArgumentName(AttributeArgumentSyntax namedArgument, NamedTypeSymbol attributeType, DiagnosticBag diagnostics, out bool wasError, out LookupResultKind resultKind)
	{
		IdentifierNameSyntax name = namedArgument.NameEquals.Name;
		string valueText = name.Identifier.ValueText;
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupMembersWithFallback(instance, attributeType, valueText, 0, ref useSiteDiagnostics);
		diagnostics.Add(name, useSiteDiagnostics);
		Symbol result = ResultSymbol(instance, valueText, 0, name, diagnostics, suppressUseSiteDiagnostics: false, out wasError);
		resultKind = instance.Kind;
		instance.Free();
		return result;
	}

	private TypeSymbol BindNamedAttributeArgumentType(AttributeArgumentSyntax namedArgument, Symbol namedArgumentNameSymbol, NamedTypeSymbol attributeType, DiagnosticBag diagnostics)
	{
		if (namedArgumentNameSymbol.Kind == SymbolKind.ErrorType)
		{
			return (TypeSymbol)namedArgumentNameSymbol;
		}
		bool flag = false;
		TypeSymbol typeSymbol = null;
		flag |= namedArgumentNameSymbol.DeclaredAccessibility != Accessibility.Public;
		flag |= namedArgumentNameSymbol.IsStatic;
		if (!flag)
		{
			switch (namedArgumentNameSymbol.Kind)
			{
			case SymbolKind.Field:
			{
				FieldSymbol fieldSymbol = (FieldSymbol)namedArgumentNameSymbol;
				typeSymbol = fieldSymbol.Type;
				flag |= fieldSymbol.IsReadOnly;
				flag |= fieldSymbol.IsConst;
				break;
			}
			case SymbolKind.Property:
			{
				PropertySymbol leastOverriddenProperty = ((PropertySymbol)namedArgumentNameSymbol).GetLeastOverriddenProperty(ContainingType);
				typeSymbol = leastOverriddenProperty.Type;
				flag |= leastOverriddenProperty.IsReadOnly;
				MethodSymbol getMethod = leastOverriddenProperty.GetMethod;
				MethodSymbol setMethod = leastOverriddenProperty.SetMethod;
				flag = flag || (object)getMethod == null || (object)setMethod == null;
				if (!flag)
				{
					flag = getMethod.DeclaredAccessibility != Accessibility.Public || setMethod.DeclaredAccessibility != Accessibility.Public;
				}
				break;
			}
			default:
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return new ExtendedErrorTypeSymbol(attributeType, namedArgumentNameSymbol, LookupResultKind.NotAVariable, diagnostics.Add(ErrorCode.ERR_BadNamedAttributeArgument, namedArgument.NameEquals.Name.Location, namedArgumentNameSymbol.Name));
		}
		if (!typeSymbol.IsValidAttributeParameterType(Compilation))
		{
			return new ExtendedErrorTypeSymbol(attributeType, namedArgumentNameSymbol, LookupResultKind.NotAVariable, diagnostics.Add(ErrorCode.ERR_BadNamedAttributeArgumentType, namedArgument.NameEquals.Name.Location, namedArgumentNameSymbol.Name));
		}
		return typeSymbol;
	}

	protected virtual MethodSymbol BindAttributeConstructor(AttributeSyntax node, NamedTypeSymbol attributeType, AnalyzedArguments boundConstructorArguments, DiagnosticBag diagnostics, ref LookupResultKind resultKind, bool suppressErrors, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!TryPerformConstructorOverloadResolution(attributeType, boundConstructorArguments, attributeType.Name, node.Location, suppressErrors, diagnostics, out var memberResolutionResult, out var _, allowProtectedConstructorsOfBaseType: true))
		{
			resultKind = resultKind.WorseResultKind((memberResolutionResult.IsValid && !IsConstructorAccessible(memberResolutionResult.Member, ref useSiteDiagnostics)) ? LookupResultKind.Inaccessible : LookupResultKind.OverloadResolutionFailure);
		}
		return memberResolutionResult.Member;
	}

	private ImmutableArray<TypedConstant> GetRewrittenAttributeConstructorArguments(out ImmutableArray<int> constructorArgumentsSourceIndices, MethodSymbol attributeConstructor, ImmutableArray<TypedConstant> constructorArgsArray, ImmutableArray<string> constructorArgumentNamesOpt, AttributeSyntax syntax, DiagnosticBag diagnostics, ref bool hasErrors)
	{
		int length = constructorArgsArray.Length;
		int argsConsumedCount = 0;
		bool flag = !constructorArgumentNamesOpt.IsDefault;
		int num = -1;
		ImmutableArray<ParameterSymbol> parameters = attributeConstructor.Parameters;
		int length2 = parameters.Length;
		TypedConstant[] array = new TypedConstant[length2];
		int[] array2 = null;
		for (int i = 0; i < length2; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			TypedConstant typedConstant;
			if (parameterSymbol.IsParams && parameterSymbol.Type.IsSZArray() && i + 1 == length2)
			{
				typedConstant = GetParamArrayArgument(parameterSymbol, constructorArgsArray, constructorArgumentNamesOpt, length, argsConsumedCount, Conversions, out var foundNamed);
				if (!foundNamed)
				{
					array2 = array2 ?? CreateSourceIndicesArray(i, length2);
				}
			}
			else if (argsConsumedCount < length)
			{
				if (!flag || constructorArgumentNamesOpt[argsConsumedCount] == null)
				{
					typedConstant = constructorArgsArray[argsConsumedCount];
					if (array2 != null)
					{
						array2[i] = argsConsumedCount;
					}
					argsConsumedCount++;
				}
				else
				{
					if (num == -1)
					{
						num = argsConsumedCount;
					}
					typedConstant = GetMatchingNamedOrOptionalConstructorArgument(out var matchingArgumentIndex, constructorArgsArray, constructorArgumentNamesOpt, parameterSymbol, num, length, ref argsConsumedCount, syntax, diagnostics);
					array2 = array2 ?? CreateSourceIndicesArray(i, length2);
					array2[i] = matchingArgumentIndex;
				}
			}
			else
			{
				typedConstant = GetDefaultValueArgument(parameterSymbol, syntax, diagnostics);
				array2 = array2 ?? CreateSourceIndicesArray(i, length2);
			}
			if (!hasErrors)
			{
				if (typedConstant.Kind == TypedConstantKind.Error)
				{
					hasErrors = true;
				}
				else if (typedConstant.Kind == TypedConstantKind.Array && parameterSymbol.Type.TypeKind == TypeKind.Array && (TypeSymbol)typedConstant.Type != parameterSymbol.Type)
				{
					diagnostics.Add(ErrorCode.ERR_BadAttributeArgument, syntax.Location);
					hasErrors = true;
				}
			}
			array[i] = typedConstant;
		}
		constructorArgumentsSourceIndices = array2?.AsImmutableOrNull() ?? default(ImmutableArray<int>);
		return array.AsImmutableOrNull();
	}

	private static int[] CreateSourceIndicesArray(int paramIndex, int parameterCount)
	{
		int[] array = new int[parameterCount];
		for (int i = 0; i < paramIndex; i++)
		{
			array[i] = i;
		}
		for (int j = paramIndex; j < parameterCount; j++)
		{
			array[j] = -1;
		}
		return array;
	}

	private TypedConstant GetMatchingNamedOrOptionalConstructorArgument(out int matchingArgumentIndex, ImmutableArray<TypedConstant> constructorArgsArray, ImmutableArray<string> constructorArgumentNamesOpt, ParameterSymbol parameter, int startIndex, int argumentsCount, ref int argsConsumedCount, AttributeSyntax syntax, DiagnosticBag diagnostics)
	{
		int matchingNamedConstructorArgumentIndex = GetMatchingNamedConstructorArgumentIndex(parameter.Name, constructorArgumentNamesOpt, startIndex, argumentsCount);
		if (matchingNamedConstructorArgumentIndex < argumentsCount)
		{
			argsConsumedCount++;
			matchingArgumentIndex = matchingNamedConstructorArgumentIndex;
			return constructorArgsArray[matchingNamedConstructorArgumentIndex];
		}
		matchingArgumentIndex = -1;
		return GetDefaultValueArgument(parameter, syntax, diagnostics);
	}

	private static int GetMatchingNamedConstructorArgumentIndex(string parameterName, ImmutableArray<string> argumentNamesOpt, int startIndex, int argumentsCount)
	{
		if (parameterName.IsEmpty() || !argumentNamesOpt.Any())
		{
			return argumentsCount;
		}
		int i;
		for (i = startIndex; i < argumentsCount && !string.Equals(argumentNamesOpt[i], parameterName, StringComparison.Ordinal); i++)
		{
		}
		return i;
	}

	private TypedConstant GetDefaultValueArgument(ParameterSymbol parameter, AttributeSyntax syntax, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = parameter.Type;
		ConstantValue constantValue = (parameter.IsOptional ? parameter.ExplicitDefaultConstantValue : null);
		object value = null;
		TypedConstantKind typedConstantKind;
		if (!IsEarlyAttributeBinder && parameter.IsCallerLineNumber)
		{
			int displayLineNumber = syntax.SyntaxTree.GetDisplayLineNumber(syntax.Name.Span);
			typedConstantKind = TypedConstantKind.Primitive;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion callerLineNumberConversion = Conversions.GetCallerLineNumberConversion(typeSymbol, ref useSiteDiagnostics);
			diagnostics.Add(syntax, useSiteDiagnostics);
			if (callerLineNumberConversion.IsNumeric || callerLineNumberConversion.IsConstantExpression)
			{
				value = ((typeSymbol.SpecialType == SpecialType.System_Single) ? ((object)(float)displayLineNumber) : DoUncheckedConversion(typeSymbol.SpecialType, ConstantValue.Create(displayLineNumber)));
			}
			else
			{
				typeSymbol = Compilation.GetSpecialType(SpecialType.System_Int32);
				value = displayLineNumber;
			}
		}
		else if (!IsEarlyAttributeBinder && parameter.IsCallerFilePath)
		{
			typeSymbol = Compilation.GetSpecialType(SpecialType.System_String);
			typedConstantKind = TypedConstantKind.Primitive;
			value = syntax.SyntaxTree.GetDisplayPath(syntax.Name.Span, Compilation.Options.SourceReferenceResolver);
		}
		else if (!IsEarlyAttributeBinder && parameter.IsCallerMemberName && (object)((ContextualAttributeBinder)this).AttributedMember != null)
		{
			typeSymbol = Compilation.GetSpecialType(SpecialType.System_String);
			typedConstantKind = TypedConstantKind.Primitive;
			value = ((ContextualAttributeBinder)this).AttributedMember.GetMemberCallerName();
		}
		else if (constantValue == null)
		{
			if (typeSymbol.SpecialType == SpecialType.System_Object)
			{
				diagnostics.Add(ErrorCode.ERR_BadAttributeParamDefaultArgument, syntax.Name.Location, parameter.Name);
				typedConstantKind = TypedConstantKind.Error;
			}
			else
			{
				typedConstantKind = TypedConstant.GetTypedConstantKind(typeSymbol, Compilation);
				constantValue = typeSymbol.GetDefaultValue();
				if (constantValue != null)
				{
					value = constantValue.Value;
				}
			}
		}
		else if (constantValue.IsBad)
		{
			typedConstantKind = TypedConstantKind.Error;
		}
		else if (typeSymbol.SpecialType == SpecialType.System_Object && !constantValue.IsNull)
		{
			diagnostics.Add(ErrorCode.ERR_NotNullRefDefaultParameter, syntax.Location, parameter.Name, typeSymbol);
			typedConstantKind = TypedConstantKind.Error;
		}
		else
		{
			typedConstantKind = TypedConstant.GetTypedConstantKind(typeSymbol, Compilation);
			value = constantValue.Value;
		}
		if (typedConstantKind == TypedConstantKind.Array)
		{
			return new TypedConstant(typeSymbol, default);
		}
		return new TypedConstant(typeSymbol, typedConstantKind, value);
	}

	private static TypedConstant GetParamArrayArgument(ParameterSymbol parameter, ImmutableArray<TypedConstant> constructorArgsArray, ImmutableArray<string> constructorArgumentNamesOpt, int argumentsCount, int argsConsumedCount, Conversions conversions, out bool foundNamed)
	{
		if (!constructorArgumentNamesOpt.IsDefault)
		{
			int num = constructorArgumentNamesOpt.IndexOf(parameter.Name);
			if (num >= 0)
			{
				foundNamed = true;
				if (TryGetNormalParamValue(parameter, constructorArgsArray, num, conversions, out var result))
				{
					return result;
				}
				return new TypedConstant(parameter.Type, ImmutableArray.Create(constructorArgsArray[num]));
			}
		}
		int num2 = argumentsCount - argsConsumedCount;
		foundNamed = false;
		switch (num2)
		{
		case 0:
			return new TypedConstant(parameter.Type, ImmutableArray<TypedConstant>.Empty);
		case 1:
		{
			if (TryGetNormalParamValue(parameter, constructorArgsArray, argsConsumedCount, conversions, out var result2))
			{
				return result2;
			}
			break;
		}
		}
		TypedConstant[] array = new TypedConstant[num2];
		for (int i = 0; i < num2; i++)
		{
			array[i] = constructorArgsArray[argsConsumedCount++];
		}
		return new TypedConstant(parameter.Type, array.AsImmutableOrNull());
	}

	private static bool TryGetNormalParamValue(ParameterSymbol parameter, ImmutableArray<TypedConstant> constructorArgsArray, int argIndex, Conversions conversions, out TypedConstant result)
	{
		TypedConstant typedConstant = constructorArgsArray[argIndex];
		if (typedConstant.Kind != TypedConstantKind.Array)
		{
			result = default;
			return false;
		}
		TypeSymbol typeSymbol = (TypeSymbol)typedConstant.Type;
		if (typeSymbol == parameter.Type)
		{
			result = typedConstant;
			return true;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = conversions.ClassifyBuiltInConversion(typeSymbol, parameter.Type, ref useSiteDiagnostics);
		if (conversion.IsValid && conversion.Kind == ConversionKind.ImplicitReference)
		{
			result = typedConstant;
			return true;
		}
		result = default;
		return false;
	}

	private BoundExpression BindAwait(AwaitExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression expression = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		return BindAwait(expression, node, diagnostics);
	}

	private BoundExpression BindAwait(BoundExpression expression, SyntaxNode node, DiagnosticBag diagnostics)
	{
		bool flag = ReportBadAwaitWithoutAsync(node, diagnostics) | ReportBadAwaitContext(node, diagnostics) | !GetAwaitableExpressionInfo(expression, out var getAwaiter, out var isCompleted, out var getResult, out var _, node, diagnostics);
		TypeSymbol type;
		if ((object)getResult != null)
		{
			type = getResult.ReturnType;
		}
		else
		{
			type = (flag ? CreateErrorType() : Compilation.DynamicType);
		}
		return new BoundAwaitExpression(node, expression, getAwaiter, isCompleted, getResult, type, flag);
	}

	private bool CouldBeAwaited(BoundExpression expression)
	{
		if (expression.Kind != BoundKind.Call)
		{
			return false;
		}
		TypeSymbol type = expression.Type;
		if ((object)type == null || type.IsDynamic() || type.SpecialType == SpecialType.System_Void)
		{
			return false;
		}
		BoundCall boundCall = (BoundCall)expression;
		if ((object)boundCall.Method != null && boundCall.Method.IsAsync)
		{
			return true;
		}
		if (ImplementsWinRTAsyncInterface(boundCall.Type))
		{
			return true;
		}
		if (!(ContainingMemberOrLambda is MethodSymbol { IsAsync: not false }))
		{
			return false;
		}
		if (ContextForbidsAwait)
		{
			return false;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		BoundExpression boundExpression = BindAwait(expression, expression.Syntax, instance);
		instance.Free();
		return !boundExpression.HasAnyErrors;
	}

	private bool ReportBadAwaitWithoutAsync(SyntaxNode node, DiagnosticBag diagnostics)
	{
		DiagnosticInfo diagnosticInfo = null;
		Symbol containingMemberOrLambda = ContainingMemberOrLambda;
		if ((object)containingMemberOrLambda != null)
		{
			switch (containingMemberOrLambda.Kind)
			{
			case SymbolKind.Field:
				if (containingMemberOrLambda.ContainingType.IsScriptClass)
				{
					if (!((FieldSymbol)containingMemberOrLambda).IsStatic)
					{
						return false;
					}
					diagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_BadAwaitInStaticVariableInitializer);
				}
				break;
			case SymbolKind.Method:
			{
				MethodSymbol methodSymbol = (MethodSymbol)containingMemberOrLambda;
				if (methodSymbol.IsAsync)
				{
					return false;
				}
				if (methodSymbol.MethodKind != MethodKind.AnonymousFunction)
				{
					diagnosticInfo = (methodSymbol.ReturnsVoid ? new CSDiagnosticInfo(ErrorCode.ERR_BadAwaitWithoutVoidAsyncMethod) : new CSDiagnosticInfo(ErrorCode.ERR_BadAwaitWithoutAsyncMethod, methodSymbol.ReturnType));
				}
				else
				{
					diagnosticInfo = (methodSymbol.IsImplicitlyDeclared ? new CSDiagnosticInfo(ErrorCode.ERR_BadAwaitInQuery) : new CSDiagnosticInfo(ErrorCode.ERR_BadAwaitWithoutAsyncLambda, ((LambdaSymbol)methodSymbol).MessageID.Localize()));
				}
				break;
			}
			}
		}
		if (diagnosticInfo == null)
		{
			diagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_BadAwaitWithoutAsync);
		}
		Error(diagnostics, diagnosticInfo, node);
		return true;
	}

	private bool ReportBadAwaitContext(SyntaxNode node, DiagnosticBag diagnostics)
	{
		if (InUnsafeRegion && !Flags.Includes(BinderFlags.AllowAwaitInUnsafeContext))
		{
			Error(diagnostics, ErrorCode.ERR_AwaitInUnsafeContext, node);
			return true;
		}
		if (Flags.Includes(BinderFlags.InLockBody))
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaitInLock, node);
			return true;
		}
		if (Flags.Includes(BinderFlags.InCatchFilter))
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaitInCatchFilter, node);
			return true;
		}
		if (Flags.Includes(BinderFlags.InFinallyBlock))
		{
			CSharpSyntaxTree obj = node.SyntaxTree as CSharpSyntaxTree;
			if (obj != null && obj.Options?.IsFeatureEnabled(MessageID.IDS_AwaitInCatchAndFinally) == false)
			{
				Error(diagnostics, ErrorCode.ERR_BadAwaitInFinally, node);
				return true;
			}
		}
		if (Flags.Includes(BinderFlags.InCatchBlock))
		{
			CSharpSyntaxTree obj2 = node.SyntaxTree as CSharpSyntaxTree;
			if (obj2 != null && obj2.Options?.IsFeatureEnabled(MessageID.IDS_AwaitInCatchAndFinally) == false)
			{
				Error(diagnostics, ErrorCode.ERR_BadAwaitInCatch, node);
				return true;
			}
		}
		return false;
	}

	internal bool GetAwaitableExpressionInfo(BoundExpression expression, out MethodSymbol getAwaiter, out PropertySymbol isCompleted, out MethodSymbol getResult, out BoundExpression getAwaiterGetResultCall, SyntaxNode node, DiagnosticBag diagnostics)
	{
		getAwaiter = null;
		isCompleted = null;
		getResult = null;
		getAwaiterGetResultCall = null;
		if (!ValidateAwaitedExpression(expression, node, diagnostics))
		{
			return false;
		}
		if (expression.HasDynamicType())
		{
			return true;
		}
		BoundExpression getAwaiterCall = null;
		if (!GetGetAwaiterMethod(expression, node, diagnostics, out getAwaiter, out getAwaiterCall))
		{
			return false;
		}
		TypeSymbol returnType = getAwaiter.ReturnType;
		if (GetIsCompletedProperty(returnType, node, expression.Type, diagnostics, out isCompleted) && AwaiterImplementsINotifyCompletion(returnType, node, diagnostics))
		{
			return GetGetResultMethod(getAwaiterCall, node, expression.Type, diagnostics, out getResult, out getAwaiterGetResultCall);
		}
		return false;
	}

	private static bool ValidateAwaitedExpression(BoundExpression expression, SyntaxNode node, DiagnosticBag diagnostics)
	{
		if (expression.HasAnyErrors)
		{
			return false;
		}
		if ((object)expression.Type == null)
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaitArgIntrinsic, node, expression.Display);
			return false;
		}
		return true;
	}

	private bool GetGetAwaiterMethod(BoundExpression expression, SyntaxNode node, DiagnosticBag diagnostics, out MethodSymbol getAwaiterMethod, out BoundExpression getAwaiterCall)
	{
		if (expression.Type.SpecialType == SpecialType.System_Void)
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaitArgVoidCall, node);
			getAwaiterMethod = null;
			getAwaiterCall = null;
			return false;
		}
		getAwaiterCall = MakeInvocationExpression(node, expression, "GetAwaiter", ImmutableArray<BoundExpression>.Empty, diagnostics);
		if (getAwaiterCall.HasAnyErrors)
		{
			getAwaiterMethod = null;
			getAwaiterCall = null;
			return false;
		}
		if (getAwaiterCall.Kind != BoundKind.Call)
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaitArg, node, expression.Type);
			getAwaiterMethod = null;
			getAwaiterCall = null;
			return false;
		}
		getAwaiterMethod = ((BoundCall)getAwaiterCall).Method;
		if (getAwaiterMethod is ErrorMethodSymbol || HasOptionalOrVariableParameters(getAwaiterMethod) || getAwaiterMethod.ReturnsVoid)
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaitArg, node, expression.Type);
			getAwaiterMethod = null;
			getAwaiterCall = null;
			return false;
		}
		return true;
	}

	private bool GetIsCompletedProperty(TypeSymbol awaiterType, SyntaxNode node, TypeSymbol awaitedExpressionType, DiagnosticBag diagnostics, out PropertySymbol isCompletedProperty)
	{
		BoundLiteral boundLeft = new BoundLiteral(node, ConstantValue.Null, awaiterType);
		string rightName = "IsCompleted";
		BoundExpression boundExpression = BindInstanceMemberAccess(node, node, boundLeft, rightName, 0, default, default, invoked: false, indexed: false, diagnostics);
		if (boundExpression.HasAnyErrors)
		{
			isCompletedProperty = null;
			return false;
		}
		if (boundExpression.Kind != BoundKind.PropertyAccess)
		{
			Error(diagnostics, ErrorCode.ERR_NoSuchMember, node, awaiterType, "IsCompleted");
			isCompletedProperty = null;
			return false;
		}
		isCompletedProperty = ((BoundPropertyAccess)boundExpression).PropertySymbol;
		if (isCompletedProperty.IsWriteOnly)
		{
			Error(diagnostics, ErrorCode.ERR_PropertyLacksGet, node, isCompletedProperty);
			isCompletedProperty = null;
			return false;
		}
		if (isCompletedProperty.Type.SpecialType != SpecialType.System_Boolean)
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaiterPattern, node, awaiterType, awaitedExpressionType);
			isCompletedProperty = null;
			return false;
		}
		return true;
	}

	private bool AwaiterImplementsINotifyCompletion(TypeSymbol awaiterType, SyntaxNode node, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol wellKnownType = GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_INotifyCompletion, diagnostics, node);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!Conversions.ClassifyImplicitConversionFromType(awaiterType, wellKnownType, ref useSiteDiagnostics).IsImplicit)
		{
			diagnostics.Add(node, useSiteDiagnostics);
			Error(diagnostics, ErrorCode.ERR_DoesntImplementAwaitInterface, node, awaiterType, wellKnownType);
			return false;
		}
		return true;
	}

	private bool GetGetResultMethod(BoundExpression awaiterExpression, SyntaxNode node, TypeSymbol awaitedExpressionType, DiagnosticBag diagnostics, out MethodSymbol getResultMethod, out BoundExpression getAwaiterGetResultCall)
	{
		TypeSymbol type = awaiterExpression.Type;
		getAwaiterGetResultCall = MakeInvocationExpression(node, awaiterExpression, "GetResult", ImmutableArray<BoundExpression>.Empty, diagnostics);
		if (getAwaiterGetResultCall.HasAnyErrors)
		{
			getResultMethod = null;
			getAwaiterGetResultCall = null;
			return false;
		}
		if (getAwaiterGetResultCall.Kind != BoundKind.Call)
		{
			Error(diagnostics, ErrorCode.ERR_NoSuchMember, node, type, "GetResult");
			getResultMethod = null;
			getAwaiterGetResultCall = null;
			return false;
		}
		getResultMethod = ((BoundCall)getAwaiterGetResultCall).Method;
		if (getResultMethod.IsExtensionMethod)
		{
			Error(diagnostics, ErrorCode.ERR_NoSuchMember, node, type, "GetResult");
			getResultMethod = null;
			getAwaiterGetResultCall = null;
			return false;
		}
		if (HasOptionalOrVariableParameters(getResultMethod) || getResultMethod.IsConditional)
		{
			Error(diagnostics, ErrorCode.ERR_BadAwaiterPattern, node, type, awaitedExpressionType);
			getResultMethod = null;
			getAwaiterGetResultCall = null;
			return false;
		}
		return true;
	}

	private static bool HasOptionalOrVariableParameters(MethodSymbol method)
	{
		if (method.ParameterCount != 0)
		{
			ParameterSymbol parameterSymbol = method.Parameters[method.ParameterCount - 1];
			if (!parameterSymbol.IsOptional)
			{
				return parameterSymbol.IsParams;
			}
			return true;
		}
		return false;
	}

	internal ImmutableArray<TypeParameterConstraintClause> BindTypeParameterConstraintClauses(Symbol containingSymbol, ImmutableArray<TypeParameterSymbol> typeParameters, SyntaxList<TypeParameterConstraintClauseSyntax> clauses, DiagnosticBag diagnostics)
	{
		int length = typeParameters.Length;
		Dictionary<string, int> dictionary = new Dictionary<string, int>(length, StringOrdinalComparer.Instance);
		foreach (TypeParameterSymbol item in typeParameters)
		{
			string name = item.Name;
			if (!dictionary.ContainsKey(name))
			{
				dictionary.Add(name, dictionary.Count);
			}
		}
		TypeParameterConstraintClause[] array = new TypeParameterConstraintClause[length];
		foreach (TypeParameterConstraintClauseSyntax item2 in clauses)
		{
			string valueText = item2.Name.Identifier.ValueText;
			if (dictionary.TryGetValue(valueText, out var value))
			{
				TypeParameterConstraintClause typeParameterConstraintClause = BindTypeParameterConstraints(valueText, item2.Constraints, diagnostics);
				if (array[value] == null)
				{
					array[value] = typeParameterConstraintClause;
					continue;
				}
				diagnostics.Add(ErrorCode.ERR_DuplicateConstraintClause, item2.Name.Location, valueText);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_TyVarNotFoundInConstraint, item2.Name.Location, valueText, containingSymbol.ConstructedFrom());
			}
		}
		return array.AsImmutableOrNull();
	}

	private TypeParameterConstraintClause BindTypeParameterConstraints(string name, SeparatedSyntaxList<TypeParameterConstraintSyntax> constraintsSyntax, DiagnosticBag diagnostics)
	{
		TypeParameterConstraintKind typeParameterConstraintKind = TypeParameterConstraintKind.None;
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		int i = 0;
		for (int count = constraintsSyntax.Count; i < count; i++)
		{
			TypeParameterConstraintSyntax typeParameterConstraintSyntax = constraintsSyntax[i];
			switch (typeParameterConstraintSyntax.Kind())
			{
			case SyntaxKind.ClassConstraint:
				if (i != 0)
				{
					diagnostics.Add(ErrorCode.ERR_RefValBoundMustBeFirst, typeParameterConstraintSyntax.GetFirstToken().GetLocation());
				}
				typeParameterConstraintKind |= TypeParameterConstraintKind.ReferenceType;
				break;
			case SyntaxKind.StructConstraint:
				if (i != 0)
				{
					diagnostics.Add(ErrorCode.ERR_RefValBoundMustBeFirst, typeParameterConstraintSyntax.GetFirstToken().GetLocation());
				}
				typeParameterConstraintKind |= TypeParameterConstraintKind.ValueType;
				break;
			case SyntaxKind.ConstructorConstraint:
				if ((typeParameterConstraintKind & TypeParameterConstraintKind.ValueType) != 0)
				{
					diagnostics.Add(ErrorCode.ERR_NewBoundWithVal, typeParameterConstraintSyntax.GetFirstToken().GetLocation());
				}
				if ((typeParameterConstraintKind & TypeParameterConstraintKind.Unmanaged) != 0)
				{
					diagnostics.Add(ErrorCode.ERR_NewBoundWithUnmanaged, typeParameterConstraintSyntax.GetFirstToken().GetLocation());
				}
				if (i != count - 1)
				{
					diagnostics.Add(ErrorCode.ERR_NewBoundMustBeLast, typeParameterConstraintSyntax.GetFirstToken().GetLocation());
				}
				typeParameterConstraintKind |= TypeParameterConstraintKind.Constructor;
				break;
			case SyntaxKind.TypeConstraint:
			{
				TypeConstraintSyntax typeConstraintSyntax = (TypeConstraintSyntax)typeParameterConstraintSyntax;
				TypeSyntax type = typeConstraintSyntax.Type;
				SyntaxKind syntaxKind = type.Kind();
				if (syntaxKind != SyntaxKind.PredefinedType && syntaxKind != SyntaxKind.PointerType && !SyntaxFacts.IsName(type.Kind()))
				{
					diagnostics.Add(ErrorCode.ERR_BadConstraintType, type.GetLocation());
				}
				TypeSymbol typeSymbol = BindTypeOrUnmanagedKeyword(type, diagnostics, out var isUnmanaged);
				if (isUnmanaged)
				{
					if (typeParameterConstraintKind != TypeParameterConstraintKind.None || instance.Any())
					{
						diagnostics.Add(ErrorCode.ERR_UnmanagedConstraintMustBeFirst, type.GetLocation());
						break;
					}
					GetWellKnownType(WellKnownType.System_Runtime_InteropServices_UnmanagedType, diagnostics, type);
					GetSpecialType(SpecialType.System_ValueType, diagnostics, type);
					typeParameterConstraintKind |= TypeParameterConstraintKind.Unmanaged;
				}
				else
				{
					if (!IsValidConstraintType(typeConstraintSyntax, typeSymbol, diagnostics))
					{
						break;
					}
					if (instance.Contains(typeSymbol))
					{
						Error(diagnostics, ErrorCode.ERR_DuplicateBound, typeParameterConstraintSyntax, typeSymbol, name);
						break;
					}
					if (typeSymbol.TypeKind == TypeKind.Class)
					{
						if (instance.Count > 0)
						{
							Error(diagnostics, ErrorCode.ERR_ClassBoundNotFirst, typeParameterConstraintSyntax, typeSymbol);
							break;
						}
						if ((typeParameterConstraintKind & TypeParameterConstraintKind.ReferenceType) != 0)
						{
							SpecialType specialType = typeSymbol.SpecialType;
							if ((uint)(specialType - 2) > 2u)
							{
								Error(diagnostics, ErrorCode.ERR_RefValBoundWithClass, typeParameterConstraintSyntax, typeSymbol);
								break;
							}
						}
						else if (typeSymbol.SpecialType != SpecialType.System_Enum)
						{
							if ((typeParameterConstraintKind & TypeParameterConstraintKind.ValueType) != 0)
							{
								Error(diagnostics, ErrorCode.ERR_RefValBoundWithClass, typeParameterConstraintSyntax, typeSymbol);
								break;
							}
							if ((typeParameterConstraintKind & TypeParameterConstraintKind.Unmanaged) != 0)
							{
								Error(diagnostics, ErrorCode.ERR_UnmanagedBoundWithClass, typeParameterConstraintSyntax, typeSymbol);
								break;
							}
						}
					}
					instance.Add(typeSymbol);
				}
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(typeParameterConstraintSyntax.Kind());
			}
		}
		return new TypeParameterConstraintClause(typeParameterConstraintKind, instance.ToImmutableAndFree());
	}

	private static bool IsValidConstraintType(TypeConstraintSyntax syntax, TypeSymbol type, DiagnosticBag diagnostics)
	{
		switch (type.SpecialType)
		{
		case SpecialType.System_Enum:
			CheckFeatureAvailability(syntax, MessageID.IDS_FeatureEnumGenericTypeConstraint, diagnostics);
			break;
		case SpecialType.System_MulticastDelegate:
		case SpecialType.System_Delegate:
			CheckFeatureAvailability(syntax, MessageID.IDS_FeatureDelegateGenericTypeConstraint, diagnostics);
			break;
		case SpecialType.System_Object:
		case SpecialType.System_ValueType:
		case SpecialType.System_Array:
			Error(diagnostics, ErrorCode.ERR_SpecialTypeAsBound, syntax, type);
			return false;
		}
		switch (type.TypeKind)
		{
		case TypeKind.Error:
		case TypeKind.TypeParameter:
			return true;
		case TypeKind.Dynamic:
			Error(diagnostics, ErrorCode.ERR_DynamicTypeAsBound, syntax);
			return false;
		case TypeKind.Class:
			if (!type.IsSealed)
			{
				if (type.IsStatic)
				{
					Error(diagnostics, ErrorCode.ERR_ConstraintIsStaticClass, syntax, type);
					return false;
				}
				break;
			}
			goto case TypeKind.Delegate;
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Struct:
			Error(diagnostics, ErrorCode.ERR_BadBoundType, syntax, type);
			return false;
		case TypeKind.Array:
		case TypeKind.Pointer:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
		case TypeKind.Interface:
			break;
		}
		if (type.ContainsDynamic())
		{
			Error(diagnostics, ErrorCode.ERR_ConstructedDynamicTypeAsBound, syntax, type);
			return false;
		}
		return true;
	}

	internal BoundExpression CreateConversion(BoundExpression source, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(source, destination, ref useSiteDiagnostics);
		diagnostics.Add(source.Syntax, useSiteDiagnostics);
		return CreateConversion(source.Syntax, source, conversion, isCast: false, destination, diagnostics);
	}

	internal BoundExpression CreateConversion(BoundExpression source, Conversion conversion, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		return CreateConversion(source.Syntax, source, conversion, isCast: false, destination, diagnostics);
	}

	internal BoundExpression CreateConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		return CreateConversion(syntax, source, conversion, isCast, source.WasCompilerGenerated, destination, diagnostics);
	}

	protected BoundExpression CreateConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, bool wasCompilerGenerated, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		if (conversion.IsIdentity)
		{
			if (source.Kind == BoundKind.TupleLiteral)
			{
				BoundTupleLiteral boundTupleLiteral = (BoundTupleLiteral)source;
				TupleTypeSymbol.ReportNamesMismatchesIfAny(destination, boundTupleLiteral, diagnostics);
				source = new BoundConvertedTupleLiteral(boundTupleLiteral.Syntax, boundTupleLiteral.Type, boundTupleLiteral.Arguments, boundTupleLiteral.Type, boundTupleLiteral.HasErrors);
			}
			if (!isCast && source.Type == destination)
			{
				return source;
			}
		}
		ReportDiagnosticsIfObsolete(diagnostics, conversion, syntax, hasBaseReceiver: false);
		if (conversion.IsMethodGroup)
		{
			return CreateMethodGroupConversion(syntax, source, conversion, isCast, destination, diagnostics);
		}
		if (conversion.IsAnonymousFunction && source.Kind == BoundKind.UnboundLambda)
		{
			return CreateAnonymousFunctionConversion(syntax, source, conversion, isCast, destination, diagnostics);
		}
		if (conversion.IsStackAlloc)
		{
			return CreateStackAllocConversion(syntax, source, conversion, isCast, destination, diagnostics);
		}
		if (conversion.IsTupleLiteralConversion || (conversion.IsNullable && conversion.UnderlyingConversions[0].IsTupleLiteralConversion))
		{
			return CreateTupleLiteralConversion(syntax, (BoundTupleLiteral)source, conversion, isCast, destination, diagnostics);
		}
		if (conversion.IsUserDefined)
		{
			return CreateUserDefinedConversion(syntax, source, conversion, isCast, destination, diagnostics);
		}
		ConstantValue constantValueOpt = FoldConstantConversion(syntax, source, conversion, destination, diagnostics);
		if (conversion.Kind == ConversionKind.DefaultOrNullLiteral && source.Kind == BoundKind.DefaultExpression)
		{
			source = ((BoundDefaultExpression)source).Update(constantValueOpt, destination);
		}
		return new BoundConversion(syntax, source, conversion, CheckOverflowAtRuntime, isCast && !wasCompilerGenerated, constantValueOpt, destination)
		{
			WasCompilerGenerated = wasCompilerGenerated
		};
	}

	protected BoundExpression CreateUserDefinedConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		if (!conversion.IsValid)
		{
			GenerateImplicitConversionError(diagnostics, syntax, conversion, source, destination);
			return new BoundConversion(syntax, source, conversion, CheckOverflowAtRuntime, isCast, null, destination, hasErrors: true)
			{
				WasCompilerGenerated = source.WasCompilerGenerated
			};
		}
		BoundExpression boundExpression = CreateConversion(source.Syntax, source, conversion.UserDefinedFromConversion, isCast: false, wasCompilerGenerated: true, conversion.BestUserDefinedConversionAnalysis.FromType, diagnostics);
		TypeSymbol typeSymbol = conversion.BestUserDefinedConversionAnalysis.Operator.ParameterTypes[0];
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (conversion.BestUserDefinedConversionAnalysis.Kind == UserDefinedConversionAnalysisKind.ApplicableInNormalForm && conversion.BestUserDefinedConversionAnalysis.FromType != typeSymbol)
		{
			boundExpression = CreateConversion(syntax, boundExpression, Conversions.ClassifyStandardConversion(null, boundExpression.Type, typeSymbol, ref useSiteDiagnostics), isCast: false, wasCompilerGenerated: true, typeSymbol, diagnostics);
		}
		TypeSymbol returnType = conversion.BestUserDefinedConversionAnalysis.Operator.ReturnType;
		TypeSymbol toType = conversion.BestUserDefinedConversionAnalysis.ToType;
		Conversion conversion2 = conversion.UserDefinedToConversion;
		BoundExpression source2;
		if (conversion.BestUserDefinedConversionAnalysis.Kind == UserDefinedConversionAnalysisKind.ApplicableInNormalForm && toType != returnType)
		{
			source2 = new BoundConversion(syntax, boundExpression, conversion, @checked: false, isCast, null, returnType)
			{
				WasCompilerGenerated = true
			};
			if (toType.IsNullableType() && toType.GetNullableUnderlyingType() == returnType)
			{
				conversion2 = Conversions.ClassifyConversionFromType(returnType, destination, ref useSiteDiagnostics);
			}
			else
			{
				source2 = CreateConversion(syntax, source2, Conversions.ClassifyStandardConversion(null, returnType, conversion.BestUserDefinedConversionAnalysis.ToType, ref useSiteDiagnostics), isCast: false, wasCompilerGenerated: true, conversion.BestUserDefinedConversionAnalysis.ToType, diagnostics);
			}
		}
		else
		{
			source2 = new BoundConversion(syntax, boundExpression, conversion, @checked: false, isCast, null, conversion.BestUserDefinedConversionAnalysis.ToType)
			{
				WasCompilerGenerated = true
			};
		}
		diagnostics.Add(syntax, useSiteDiagnostics);
		BoundExpression boundExpression2 = CreateConversion(syntax, source2, conversion2, isCast: false, wasCompilerGenerated: true, destination, diagnostics);
		boundExpression2.ResetCompilerGenerated(source.WasCompilerGenerated);
		return boundExpression2;
	}

	private static BoundExpression CreateAnonymousFunctionConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		BoundLambda boundLambda = ((UnboundLambda)source).Bind((NamedTypeSymbol)destination);
		diagnostics.AddRange(boundLambda.Diagnostics);
		return new BoundConversion(syntax, boundLambda, conversion, @checked: false, isCast, null, destination)
		{
			WasCompilerGenerated = source.WasCompilerGenerated
		};
	}

	private BoundExpression CreateMethodGroupConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		BoundMethodGroup boundMethodGroup = FixMethodGroupWithTypeOrValue((BoundMethodGroup)source, conversion, diagnostics);
		BoundExpression receiverOpt = boundMethodGroup.ReceiverOpt;
		MethodSymbol method = conversion.Method;
		bool hasErrors = false;
		if (receiverOpt != null && receiverOpt.Kind == BoundKind.BaseReference && method.IsAbstract)
		{
			Error(diagnostics, ErrorCode.ERR_AbstractBaseCall, syntax, method);
			hasErrors = true;
		}
		NamedTypeSymbol delegateType = (NamedTypeSymbol)destination;
		if (MethodGroupConversionHasErrors(syntax, conversion, boundMethodGroup.ReceiverOpt, conversion.IsExtensionMethod, delegateType, diagnostics))
		{
			hasErrors = true;
		}
		return new BoundConversion(syntax, boundMethodGroup, conversion, @checked: false, isCast, null, destination, hasErrors)
		{
			WasCompilerGenerated = source.WasCompilerGenerated
		};
	}

	private BoundExpression CreateStackAllocConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		BoundStackAllocArrayCreation boundStackAllocArrayCreation = (BoundStackAllocArrayCreation)source;
		TypeSymbol elementType = boundStackAllocArrayCreation.ElementType;
		TypeSymbol type;
		switch (conversion.Kind)
		{
		case ConversionKind.StackAllocToPointerType:
			ReportUnsafeIfNotAllowed(syntax.Location, diagnostics);
			type = new PointerTypeSymbol(elementType);
			break;
		case ConversionKind.StackAllocToSpanType:
			CheckFeatureAvailability(syntax, MessageID.IDS_FeatureRefStructs, diagnostics);
			type = Compilation.GetWellKnownType(WellKnownType.System_Span_T).Construct(elementType);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(conversion.Kind);
		}
		BoundConvertedStackAllocExpression source2 = new BoundConvertedStackAllocExpression(syntax, elementType, boundStackAllocArrayCreation.Count, boundStackAllocArrayCreation.InitializerOpt, type, boundStackAllocArrayCreation.HasErrors);
		Conversion conversion2 = conversion.UnderlyingConversions.Single();
		return CreateConversion(syntax, source2, conversion2, isCast, destination, diagnostics);
	}

	private BoundExpression CreateTupleLiteralConversion(SyntaxNode syntax, BoundTupleLiteral sourceTuple, Conversion conversion, bool isCast, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = destination;
		Conversion conversion2 = conversion;
		if (conversion.IsNullable)
		{
			typeSymbol = destination.GetNullableUnderlyingType();
			conversion2 = conversion.UnderlyingConversions[0];
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)typeSymbol;
		if (namedTypeSymbol.IsTupleType)
		{
			TupleTypeSymbol tupleTypeSymbol = (TupleTypeSymbol)namedTypeSymbol;
			TupleTypeSymbol.ReportNamesMismatchesIfAny(namedTypeSymbol, sourceTuple, diagnostics);
			if (sourceTuple.Type is TupleTypeSymbol tupleTypeSymbol2)
			{
				namedTypeSymbol = tupleTypeSymbol2.WithUnderlyingType(tupleTypeSymbol.UnderlyingNamedType);
			}
			else
			{
				TupleExpressionSyntax tupleExpressionSyntax = (TupleExpressionSyntax)sourceTuple.Syntax;
				ArrayBuilder<Location> instance = ArrayBuilder<Location>.GetInstance();
				foreach (ArgumentSyntax argument in tupleExpressionSyntax.Arguments)
				{
					instance.Add(argument.NameColon?.Name.Location);
				}
				namedTypeSymbol = tupleTypeSymbol.WithElementNames(sourceTuple.ArgumentNamesOpt, tupleExpressionSyntax.Location, instance.ToImmutableAndFree());
			}
		}
		ImmutableArray<BoundExpression> arguments = sourceTuple.Arguments;
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance(arguments.Length);
		ImmutableArray<TypeSymbol> elementTypesOfTupleOrCompatible = namedTypeSymbol.GetElementTypesOfTupleOrCompatible();
		ImmutableArray<Conversion> underlyingConversions = conversion2.UnderlyingConversions;
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression boundExpression = arguments[i];
			TypeSymbol destination2 = elementTypesOfTupleOrCompatible[i];
			Conversion conversion3 = underlyingConversions[i];
			instance2.Add(CreateConversion(boundExpression.Syntax, boundExpression, conversion3, isCast, destination2, diagnostics));
		}
		BoundExpression boundExpression2 = new BoundConvertedTupleLiteral(sourceTuple.Syntax, sourceTuple.Type, instance2.ToImmutableAndFree(), namedTypeSymbol);
		if (sourceTuple.Type != destination)
		{
			boundExpression2 = new BoundConversion(sourceTuple.Syntax, boundExpression2, conversion, @checked: false, isCast, null, destination);
		}
		if (isCast)
		{
			boundExpression2 = new BoundConversion(syntax, boundExpression2, Conversion.Identity, @checked: false, isCast, null, destination);
		}
		return boundExpression2;
	}

	private static bool IsMethodGroupWithTypeOrValueReceiver(BoundNode node)
	{
		if (node.Kind != BoundKind.MethodGroup)
		{
			return false;
		}
		return IsTypeOrValueExpression(((BoundMethodGroup)node).ReceiverOpt);
	}

	private BoundMethodGroup FixMethodGroupWithTypeOrValue(BoundMethodGroup group, Conversion conversion, DiagnosticBag diagnostics)
	{
		if (!IsMethodGroupWithTypeOrValueReceiver(group))
		{
			return group;
		}
		BoundExpression receiverOpt = group.ReceiverOpt;
		receiverOpt = ReplaceTypeOrValueReceiver(receiverOpt, conversion.Method.IsStatic && !conversion.IsExtensionMethod, diagnostics);
		return group.Update(group.TypeArgumentsOpt, group.Name, group.Methods, group.LookupSymbolOpt, group.LookupError, group.Flags, receiverOpt, group.ResultKind);
	}

	private bool MemberGroupFinalValidation(BoundExpression receiverOpt, MethodSymbol methodSymbol, SyntaxNode node, DiagnosticBag diagnostics, bool invokedAsExtensionMethod)
	{
		if (MemberGroupFinalValidationAccessibilityChecks(receiverOpt, methodSymbol, node, diagnostics, invokedAsExtensionMethod))
		{
			return true;
		}
		return !methodSymbol.CheckConstraints(Conversions, node, Compilation, diagnostics);
	}

	private bool MemberGroupFinalValidationAccessibilityChecks(BoundExpression receiverOpt, Symbol memberSymbol, SyntaxNode node, DiagnosticBag diagnostics, bool invokedAsExtensionMethod)
	{
		if (!IsTypeOrValueExpression(receiverOpt))
		{
			if (memberSymbol.IsStatic)
			{
				if (invokedAsExtensionMethod)
				{
					if (IsMemberAccessedThroughType(receiverOpt))
					{
						if (receiverOpt.Kind == BoundKind.QueryClause)
						{
							diagnostics.Add(ErrorCode.ERR_QueryNoProvider, node.Location, receiverOpt.Type, memberSymbol.Name);
						}
						else
						{
							diagnostics.Add(ErrorCode.ERR_ObjectRequired, node.Location, memberSymbol);
						}
						return true;
					}
				}
				else if (!WasImplicitReceiver(receiverOpt) && IsMemberAccessedThroughVariableOrValue(receiverOpt))
				{
					if (Flags.Includes(BinderFlags.CollectionInitializerAddMethod))
					{
						diagnostics.Add(ErrorCode.ERR_InitializerAddHasWrongSignature, node.Location, memberSymbol);
					}
					else if (node.Kind() == SyntaxKind.AwaitExpression && memberSymbol.Name == "GetAwaiter")
					{
						diagnostics.Add(ErrorCode.ERR_BadAwaitArg, node.Location, receiverOpt.Type);
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_ObjectProhibited, node.Location, memberSymbol);
					}
					return true;
				}
			}
			else
			{
				if (IsMemberAccessedThroughType(receiverOpt))
				{
					diagnostics.Add(ErrorCode.ERR_ObjectRequired, node.Location, memberSymbol);
					return true;
				}
				if (WasImplicitReceiver(receiverOpt))
				{
					if ((InFieldInitializer && !ContainingType.IsScriptClass) || InConstructorInitializer || InAttributeArgument)
					{
						SyntaxNode syntaxNode = node;
						if (node.Parent != null && node.Parent.Kind() == SyntaxKind.InvocationExpression)
						{
							syntaxNode = node.Parent;
						}
						ErrorCode code = (InFieldInitializer ? ErrorCode.ERR_FieldInitRefNonstatic : ErrorCode.ERR_ObjectRequired);
						diagnostics.Add(code, syntaxNode.Location, memberSymbol);
						return true;
					}
					if (receiverOpt == null || ContainingMember().IsStatic)
					{
						Error(diagnostics, ErrorCode.ERR_ObjectRequired, node, memberSymbol);
						return true;
					}
				}
			}
		}
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			bool flag = IsSymbolAccessibleConditional(memberSymbol.GetTypeOrReturnType(), containingType, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			if (!flag)
			{
				Error(diagnostics, ErrorCode.ERR_BadAccess, node, memberSymbol);
				return true;
			}
		}
		return false;
	}

	private static bool IsMemberAccessedThroughVariableOrValue(BoundExpression receiverOpt)
	{
		if (receiverOpt == null)
		{
			return false;
		}
		return !IsMemberAccessedThroughType(receiverOpt);
	}

	internal static bool IsMemberAccessedThroughType(BoundExpression receiverOpt)
	{
		if (receiverOpt == null)
		{
			return false;
		}
		while (receiverOpt.Kind == BoundKind.QueryClause)
		{
			receiverOpt = ((BoundQueryClause)receiverOpt).Value;
		}
		return receiverOpt.Kind == BoundKind.TypeExpression;
	}

	internal static bool WasImplicitReceiver(BoundExpression receiverOpt)
	{
		if (receiverOpt == null)
		{
			return true;
		}
		if (!receiverOpt.WasCompilerGenerated)
		{
			return false;
		}
		BoundKind kind = receiverOpt.Kind;
		if (kind - 85 <= BoundKind.ParameterEqualsValue)
		{
			return true;
		}
		return false;
	}

	internal bool MethodGroupIsCompatibleWithDelegate(BoundExpression receiverOpt, bool isExtensionMethod, MethodSymbol method, NamedTypeSymbol delegateType, Location errorLocation, DiagnosticBag diagnostics)
	{
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		ImmutableArray<ParameterSymbol> parameters = delegateInvokeMethod.Parameters;
		ImmutableArray<ParameterSymbol> parameters2 = method.Parameters;
		int length = parameters.Length;
		if (parameters2.Length != length + (isExtensionMethod ? 1 : 0))
		{
			Error(diagnostics, ErrorCode.ERR_MethDelegateMismatch, errorLocation, method, delegateType);
			return false;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		for (int i = 0; i < length; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			ParameterSymbol parameterSymbol2 = parameters2[isExtensionMethod ? (i + 1) : i];
			if (parameterSymbol.RefKind != parameterSymbol2.RefKind || !Conversions.HasIdentityOrImplicitReferenceConversion(parameterSymbol.Type, parameterSymbol2.Type, ref useSiteDiagnostics))
			{
				Error(diagnostics, ErrorCode.ERR_MethDelegateMismatch, errorLocation, method, delegateType);
				diagnostics.Add(errorLocation, useSiteDiagnostics);
				return false;
			}
		}
		if (delegateInvokeMethod.RefKind != method.RefKind)
		{
			Error(diagnostics, ErrorCode.ERR_DelegateRefMismatch, errorLocation, method, delegateType);
			diagnostics.Add(errorLocation, useSiteDiagnostics);
			return false;
		}
		bool num;
		if (delegateInvokeMethod.RefKind == RefKind.None)
		{
			if (method.ReturnsVoid && delegateInvokeMethod.ReturnsVoid)
			{
				goto IL_0187;
			}
			num = Conversions.HasIdentityOrImplicitReferenceConversion(method.ReturnType, delegateInvokeMethod.ReturnType, ref useSiteDiagnostics);
		}
		else
		{
			num = ConversionsBase.HasIdentityConversion(method.ReturnType, delegateInvokeMethod.ReturnType);
		}
		if (!num)
		{
			Error(diagnostics, ErrorCode.ERR_BadRetType, errorLocation, method, method.ReturnType);
			diagnostics.Add(errorLocation, useSiteDiagnostics);
			return false;
		}
		goto IL_0187;
		IL_0187:
		diagnostics.Add(errorLocation, useSiteDiagnostics);
		return true;
	}

	private bool MethodGroupConversionHasErrors(SyntaxNode syntax, Conversion conversion, BoundExpression receiverOpt, bool isExtensionMethod, NamedTypeSymbol delegateType, DiagnosticBag diagnostics)
	{
		MethodSymbol method = conversion.Method;
		if (!MethodGroupIsCompatibleWithDelegate(receiverOpt, isExtensionMethod, method, delegateType, syntax.Location, diagnostics) || MemberGroupFinalValidation(receiverOpt, method, syntax, diagnostics, isExtensionMethod))
		{
			return true;
		}
		if (method.IsConditional)
		{
			Error(diagnostics, ErrorCode.ERR_DelegateOnConditional, syntax.Location, method);
			return true;
		}
		if (method is SourceOrdinaryMethodSymbol { IsPartialWithoutImplementation: not false })
		{
			Error(diagnostics, ErrorCode.ERR_PartialMethodToDelegate, syntax.Location, method);
			return true;
		}
		if (method.HasUnsafeParameter() || method.ReturnType.IsUnsafe())
		{
			return ReportUnsafeIfNotAllowed(syntax, diagnostics);
		}
		return false;
	}

	private bool MethodGroupConversionDoesNotExistOrHasErrors(BoundMethodGroup boundMethodGroup, NamedTypeSymbol delegateType, Location delegateMismatchLocation, DiagnosticBag diagnostics, out Conversion conversion)
	{
		if (ReportDelegateInvokeUseSiteDiagnostic(diagnostics, delegateType, delegateMismatchLocation))
		{
			conversion = Conversion.NoConversion;
			return true;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		conversion = Conversions.GetMethodGroupConversion(boundMethodGroup, delegateType, ref useSiteDiagnostics);
		diagnostics.Add(delegateMismatchLocation, useSiteDiagnostics);
		if (!conversion.Exists)
		{
			if (!Conversions.ReportDelegateMethodGroupDiagnostics(this, boundMethodGroup, delegateType, diagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_MethDelegateMismatch, delegateMismatchLocation, boundMethodGroup.Name, delegateType);
			}
			return true;
		}
		return MethodGroupConversionHasErrors(boundMethodGroup.Syntax, conversion, boundMethodGroup.ReceiverOpt, conversion.IsExtensionMethod, delegateType, diagnostics);
	}

	public ConstantValue FoldConstantConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		if (source.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = source.ConstantValue;
		if (constantValue == null)
		{
			if (conversion.Kind == ConversionKind.DefaultOrNullLiteral && source.Kind == BoundKind.DefaultExpression)
			{
				return destination.GetDefaultValue();
			}
			return constantValue;
		}
		if (constantValue.IsBad)
		{
			return constantValue;
		}
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
			return destination.SpecialType switch
			{
				SpecialType.System_Single => ConstantValue.Create(constantValue.SingleValue), 
				SpecialType.System_Double => ConstantValue.Create(constantValue.DoubleValue), 
				_ => constantValue, 
			};
		case ConversionKind.DefaultOrNullLiteral:
			return constantValue;
		case ConversionKind.ImplicitConstant:
			return FoldConstantNumericConversion(syntax, constantValue, destination, diagnostics);
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ExplicitNumeric:
		case ConversionKind.ExplicitEnumeration:
			if (destination.IsNullableType())
			{
				return null;
			}
			return FoldConstantNumericConversion(syntax, constantValue, destination, diagnostics);
		case ConversionKind.ImplicitReference:
		case ConversionKind.ExplicitReference:
			if (!constantValue.IsNull)
			{
				return null;
			}
			return constantValue;
		default:
			return null;
		}
	}

	private ConstantValue FoldConstantNumericConversion(SyntaxNode syntax, ConstantValue sourceValue, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		SpecialType specialType = (((object)destination == null || !destination.IsEnumType()) ? destination.GetSpecialTypeSafe() : ((NamedTypeSymbol)destination).EnumUnderlyingType.SpecialType);
		if (sourceValue.IsDecimal)
		{
			if (!CheckConstantBounds(specialType, sourceValue))
			{
				Error(diagnostics, ErrorCode.ERR_ConstOutOfRange, syntax, string.Concat(sourceValue.Value, "M"), destination);
				return ConstantValue.Bad;
			}
		}
		else if (specialType == SpecialType.System_Decimal)
		{
			if (!CheckConstantBounds(specialType, sourceValue))
			{
				Error(diagnostics, ErrorCode.ERR_ConstOutOfRange, syntax, sourceValue.Value, destination);
				return ConstantValue.Bad;
			}
		}
		else if (CheckOverflowAtCompileTime && !CheckConstantBounds(specialType, sourceValue))
		{
			Error(diagnostics, ErrorCode.ERR_ConstOutOfRangeChecked, syntax, sourceValue.Value, destination);
			return ConstantValue.Bad;
		}
		return ConstantValue.Create(DoUncheckedConversion(specialType, sourceValue), specialType);
	}

	private static object DoUncheckedConversion(SpecialType destinationType, ConstantValue value)
	{
		switch (value.Discriminator)
		{
		case ConstantValueTypeDiscriminator.Byte:
		{
			byte byteValue = value.ByteValue;
			switch (destinationType)
			{
			case SpecialType.System_Byte:
				return byteValue;
			case SpecialType.System_Char:
				return (char)byteValue;
			case SpecialType.System_UInt16:
				return (ushort)byteValue;
			case SpecialType.System_UInt32:
				return (uint)byteValue;
			case SpecialType.System_UInt64:
				return (ulong)byteValue;
			case SpecialType.System_SByte:
				return (sbyte)byteValue;
			case SpecialType.System_Int16:
				return (short)byteValue;
			case SpecialType.System_Int32:
				return (int)byteValue;
			case SpecialType.System_Int64:
				return (long)byteValue;
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return (double)(int)byteValue;
			case SpecialType.System_Decimal:
				return (decimal)byteValue;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.Char:
		{
			char charValue = value.CharValue;
			switch (destinationType)
			{
			case SpecialType.System_Byte:
				return (byte)charValue;
			case SpecialType.System_Char:
				return charValue;
			case SpecialType.System_UInt16:
				return (ushort)charValue;
			case SpecialType.System_UInt32:
				return (uint)charValue;
			case SpecialType.System_UInt64:
				return (ulong)charValue;
			case SpecialType.System_SByte:
				return (sbyte)charValue;
			case SpecialType.System_Int16:
				return (short)charValue;
			case SpecialType.System_Int32:
				return (int)charValue;
			case SpecialType.System_Int64:
				return (long)charValue;
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return (double)(int)charValue;
			case SpecialType.System_Decimal:
				return (decimal)charValue;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.UInt16:
		{
			ushort uInt16Value = value.UInt16Value;
			switch (destinationType)
			{
			case SpecialType.System_Byte:
				return (byte)uInt16Value;
			case SpecialType.System_Char:
				return (char)uInt16Value;
			case SpecialType.System_UInt16:
				return uInt16Value;
			case SpecialType.System_UInt32:
				return (uint)uInt16Value;
			case SpecialType.System_UInt64:
				return (ulong)uInt16Value;
			case SpecialType.System_SByte:
				return (sbyte)uInt16Value;
			case SpecialType.System_Int16:
				return (short)uInt16Value;
			case SpecialType.System_Int32:
				return (int)uInt16Value;
			case SpecialType.System_Int64:
				return (long)uInt16Value;
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return (double)(int)uInt16Value;
			case SpecialType.System_Decimal:
				return (decimal)uInt16Value;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.UInt32:
		{
			uint uInt32Value = value.UInt32Value;
			return destinationType switch
			{
				SpecialType.System_Byte => (object)(byte)uInt32Value, 
				SpecialType.System_Char => (char)uInt32Value, 
				SpecialType.System_UInt16 => (ushort)uInt32Value, 
				SpecialType.System_UInt32 => uInt32Value, 
				SpecialType.System_UInt64 => (ulong)uInt32Value, 
				SpecialType.System_SByte => (sbyte)uInt32Value, 
				SpecialType.System_Int16 => (short)uInt32Value, 
				SpecialType.System_Int32 => (int)uInt32Value, 
				SpecialType.System_Int64 => (long)uInt32Value, 
				SpecialType.System_Single => (double)(float)uInt32Value, 
				SpecialType.System_Double => (double)uInt32Value, 
				SpecialType.System_Decimal => (decimal)uInt32Value, 
				_ => throw ExceptionUtilities.UnexpectedValue(destinationType), 
			};
		}
		case ConstantValueTypeDiscriminator.UInt64:
		{
			ulong uInt64Value = value.UInt64Value;
			return destinationType switch
			{
				SpecialType.System_Byte => (object)(byte)uInt64Value, 
				SpecialType.System_Char => (char)uInt64Value, 
				SpecialType.System_UInt16 => (ushort)uInt64Value, 
				SpecialType.System_UInt32 => (uint)uInt64Value, 
				SpecialType.System_UInt64 => uInt64Value, 
				SpecialType.System_SByte => (sbyte)uInt64Value, 
				SpecialType.System_Int16 => (short)uInt64Value, 
				SpecialType.System_Int32 => (int)uInt64Value, 
				SpecialType.System_Int64 => (long)uInt64Value, 
				SpecialType.System_Single => (double)(float)uInt64Value, 
				SpecialType.System_Double => (double)uInt64Value, 
				SpecialType.System_Decimal => (decimal)uInt64Value, 
				_ => throw ExceptionUtilities.UnexpectedValue(destinationType), 
			};
		}
		case ConstantValueTypeDiscriminator.SByte:
		{
			sbyte sByteValue = value.SByteValue;
			switch (destinationType)
			{
			case SpecialType.System_Byte:
				return (byte)sByteValue;
			case SpecialType.System_Char:
				return (char)sByteValue;
			case SpecialType.System_UInt16:
				return (ushort)sByteValue;
			case SpecialType.System_UInt32:
				return (uint)sByteValue;
			case SpecialType.System_UInt64:
				return (ulong)sByteValue;
			case SpecialType.System_SByte:
				return sByteValue;
			case SpecialType.System_Int16:
				return (short)sByteValue;
			case SpecialType.System_Int32:
				return (int)sByteValue;
			case SpecialType.System_Int64:
				return (long)sByteValue;
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return (double)sByteValue;
			case SpecialType.System_Decimal:
				return (decimal)sByteValue;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.Int16:
		{
			short int16Value = value.Int16Value;
			switch (destinationType)
			{
			case SpecialType.System_Byte:
				return (byte)int16Value;
			case SpecialType.System_Char:
				return (char)int16Value;
			case SpecialType.System_UInt16:
				return (ushort)int16Value;
			case SpecialType.System_UInt32:
				return (uint)int16Value;
			case SpecialType.System_UInt64:
				return (ulong)int16Value;
			case SpecialType.System_SByte:
				return (sbyte)int16Value;
			case SpecialType.System_Int16:
				return int16Value;
			case SpecialType.System_Int32:
				return (int)int16Value;
			case SpecialType.System_Int64:
				return (long)int16Value;
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return (double)int16Value;
			case SpecialType.System_Decimal:
				return (decimal)int16Value;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.Int32:
		{
			int int32Value = value.Int32Value;
			return destinationType switch
			{
				SpecialType.System_Byte => (object)(byte)int32Value, 
				SpecialType.System_Char => (char)int32Value, 
				SpecialType.System_UInt16 => (ushort)int32Value, 
				SpecialType.System_UInt32 => (uint)int32Value, 
				SpecialType.System_UInt64 => (ulong)int32Value, 
				SpecialType.System_SByte => (sbyte)int32Value, 
				SpecialType.System_Int16 => (short)int32Value, 
				SpecialType.System_Int32 => int32Value, 
				SpecialType.System_Int64 => (long)int32Value, 
				SpecialType.System_Single => (double)(float)int32Value, 
				SpecialType.System_Double => (double)int32Value, 
				SpecialType.System_Decimal => (decimal)int32Value, 
				_ => throw ExceptionUtilities.UnexpectedValue(destinationType), 
			};
		}
		case ConstantValueTypeDiscriminator.Int64:
		{
			long int64Value = value.Int64Value;
			return destinationType switch
			{
				SpecialType.System_Byte => (object)(byte)int64Value, 
				SpecialType.System_Char => (char)int64Value, 
				SpecialType.System_UInt16 => (ushort)int64Value, 
				SpecialType.System_UInt32 => (uint)int64Value, 
				SpecialType.System_UInt64 => (ulong)int64Value, 
				SpecialType.System_SByte => (sbyte)int64Value, 
				SpecialType.System_Int16 => (short)int64Value, 
				SpecialType.System_Int32 => (int)int64Value, 
				SpecialType.System_Int64 => int64Value, 
				SpecialType.System_Single => (double)(float)int64Value, 
				SpecialType.System_Double => (double)int64Value, 
				SpecialType.System_Decimal => (decimal)int64Value, 
				_ => throw ExceptionUtilities.UnexpectedValue(destinationType), 
			};
		}
		case ConstantValueTypeDiscriminator.Single:
		case ConstantValueTypeDiscriminator.Double:
		{
			double doubleValue = value.DoubleValue;
			return destinationType switch
			{
				SpecialType.System_Byte => (object)(byte)doubleValue, 
				SpecialType.System_Char => (char)doubleValue, 
				SpecialType.System_UInt16 => (ushort)doubleValue, 
				SpecialType.System_UInt32 => (uint)doubleValue, 
				SpecialType.System_UInt64 => (ulong)doubleValue, 
				SpecialType.System_SByte => (sbyte)doubleValue, 
				SpecialType.System_Int16 => (short)doubleValue, 
				SpecialType.System_Int32 => (int)doubleValue, 
				SpecialType.System_Int64 => (long)doubleValue, 
				SpecialType.System_Single => (double)(float)doubleValue, 
				SpecialType.System_Double => doubleValue, 
				SpecialType.System_Decimal => (value.Discriminator == ConstantValueTypeDiscriminator.Single) ? ((decimal)(float)doubleValue) : ((decimal)doubleValue), 
				_ => throw ExceptionUtilities.UnexpectedValue(destinationType), 
			};
		}
		case ConstantValueTypeDiscriminator.Decimal:
		{
			decimal num = (CheckConstantBounds(destinationType, value.DecimalValue) ? value.DecimalValue : 0m);
			return destinationType switch
			{
				SpecialType.System_Byte => (object)(byte)num, 
				SpecialType.System_Char => (char)num, 
				SpecialType.System_UInt16 => (ushort)num, 
				SpecialType.System_UInt32 => (uint)num, 
				SpecialType.System_UInt64 => (ulong)num, 
				SpecialType.System_SByte => (sbyte)num, 
				SpecialType.System_Int16 => (short)num, 
				SpecialType.System_Int32 => (int)num, 
				SpecialType.System_Int64 => (long)num, 
				SpecialType.System_Single => (double)(float)num, 
				SpecialType.System_Double => (double)num, 
				SpecialType.System_Decimal => num, 
				_ => throw ExceptionUtilities.UnexpectedValue(destinationType), 
			};
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(value.Discriminator);
		}
	}

	public static bool CheckConstantBounds(SpecialType destinationType, ConstantValue value)
	{
		if (value.IsBad)
		{
			return true;
		}
		object obj = CanonicalizeConstant(value);
		if (!(obj is decimal))
		{
			return CheckConstantBounds(destinationType, (double)obj);
		}
		return CheckConstantBounds(destinationType, (decimal)obj);
	}

	private static bool CheckConstantBounds(SpecialType destinationType, double value)
	{
		switch (destinationType)
		{
		case SpecialType.System_Byte:
			if (-1.0 < value)
			{
				return value < 256.0;
			}
			return false;
		case SpecialType.System_Char:
			if (-1.0 < value)
			{
				return value < 65536.0;
			}
			return false;
		case SpecialType.System_UInt16:
			if (-1.0 < value)
			{
				return value < 65536.0;
			}
			return false;
		case SpecialType.System_UInt32:
			if (-1.0 < value)
			{
				return value < 4294967296.0;
			}
			return false;
		case SpecialType.System_UInt64:
			if (-1.0 < value)
			{
				return value < 1.8446744073709552E+19;
			}
			return false;
		case SpecialType.System_SByte:
			if (-129.0 < value)
			{
				return value < 128.0;
			}
			return false;
		case SpecialType.System_Int16:
			if (-32769.0 < value)
			{
				return value < 32768.0;
			}
			return false;
		case SpecialType.System_Int32:
			if (-2147483649.0 < value)
			{
				return value < 2147483648.0;
			}
			return false;
		case SpecialType.System_Int64:
			if (-9.223372036854776E+18 <= value)
			{
				return value < 9.223372036854776E+18;
			}
			return false;
		case SpecialType.System_Decimal:
			if (-7.922816251426434E+28 < value)
			{
				return value < 7.922816251426434E+28;
			}
			return false;
		default:
			return true;
		}
	}

	private static bool CheckConstantBounds(SpecialType destinationType, decimal value)
	{
		switch (destinationType)
		{
		case SpecialType.System_Byte:
			if (-1m < value)
			{
				return value < 256m;
			}
			return false;
		case SpecialType.System_Char:
			if (-1m < value)
			{
				return value < 65536m;
			}
			return false;
		case SpecialType.System_UInt16:
			if (-1m < value)
			{
				return value < 65536m;
			}
			return false;
		case SpecialType.System_UInt32:
			if (-1m < value)
			{
				return value < 4294967296m;
			}
			return false;
		case SpecialType.System_UInt64:
			if (-1m < value)
			{
				return value < 18446744073709551616m;
			}
			return false;
		case SpecialType.System_SByte:
			if (-129m < value)
			{
				return value < 128m;
			}
			return false;
		case SpecialType.System_Int16:
			if (-32769m < value)
			{
				return value < 32768m;
			}
			return false;
		case SpecialType.System_Int32:
			if (-2147483649m < value)
			{
				return value < 2147483648m;
			}
			return false;
		case SpecialType.System_Int64:
			if (-9223372036854775809m < value)
			{
				return value < 9223372036854775808m;
			}
			return false;
		default:
			return true;
		}
	}

	private static object CanonicalizeConstant(ConstantValue value)
	{
		switch (value.Discriminator)
		{
		case ConstantValueTypeDiscriminator.SByte:
			return (decimal)value.SByteValue;
		case ConstantValueTypeDiscriminator.Int16:
			return (decimal)value.Int16Value;
		case ConstantValueTypeDiscriminator.Int32:
			return (decimal)value.Int32Value;
		case ConstantValueTypeDiscriminator.Int64:
			return (decimal)value.Int64Value;
		case ConstantValueTypeDiscriminator.Byte:
			return (decimal)value.ByteValue;
		case ConstantValueTypeDiscriminator.Char:
			return (decimal)value.CharValue;
		case ConstantValueTypeDiscriminator.UInt16:
			return (decimal)value.UInt16Value;
		case ConstantValueTypeDiscriminator.UInt32:
			return (decimal)value.UInt32Value;
		case ConstantValueTypeDiscriminator.UInt64:
			return (decimal)value.UInt64Value;
		case ConstantValueTypeDiscriminator.Single:
		case ConstantValueTypeDiscriminator.Double:
			return value.DoubleValue;
		case ConstantValueTypeDiscriminator.Decimal:
			return value.DecimalValue;
		default:
			throw ExceptionUtilities.UnexpectedValue(value.Discriminator);
		}
	}

	internal ImmutableArray<Symbol> BindCref(CrefSyntax syntax, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		return BindCrefInternal(syntax, out ambiguityWinner, diagnostics);
	}

	private ImmutableArray<Symbol> BindCrefInternal(CrefSyntax syntax, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		switch (syntax.Kind())
		{
		case SyntaxKind.TypeCref:
			return BindTypeCref((TypeCrefSyntax)syntax, out ambiguityWinner, diagnostics);
		case SyntaxKind.QualifiedCref:
			return BindQualifiedCref((QualifiedCrefSyntax)syntax, out ambiguityWinner, diagnostics);
		case SyntaxKind.NameMemberCref:
		case SyntaxKind.IndexerMemberCref:
		case SyntaxKind.OperatorMemberCref:
		case SyntaxKind.ConversionOperatorMemberCref:
			return BindMemberCref((MemberCrefSyntax)syntax, null, out ambiguityWinner, diagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
		}
	}

	private ImmutableArray<Symbol> BindTypeCref(TypeCrefSyntax syntax, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		NamespaceOrTypeSymbol namespaceOrTypeSymbol = BindNamespaceOrTypeSymbolInCref(syntax.Type);
		if (namespaceOrTypeSymbol.Kind == SymbolKind.ErrorType)
		{
			TypeCrefSyntax typeCrefSyntax = syntax.WithLeadingTrivia(null).WithTrailingTrivia(null);
			diagnostics.Add(ErrorCode.WRN_BadXMLRef, syntax.Location, typeCrefSyntax.ToFullString());
		}
		ambiguityWinner = null;
		return ImmutableArray.Create((Symbol)namespaceOrTypeSymbol);
	}

	private ImmutableArray<Symbol> BindQualifiedCref(QualifiedCrefSyntax syntax, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		NamespaceOrTypeSymbol containerOpt = BindNamespaceOrTypeSymbolInCref(syntax.Container);
		return BindMemberCref(syntax.Member, containerOpt, out ambiguityWinner, diagnostics);
	}

	private NamespaceOrTypeSymbol BindNamespaceOrTypeSymbolInCref(TypeSyntax syntax)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		NamespaceOrTypeSymbol result = BindNamespaceOrTypeSymbol(syntax, instance);
		instance.Free();
		return result;
	}

	private ImmutableArray<Symbol> BindMemberCref(MemberCrefSyntax syntax, NamespaceOrTypeSymbol containerOpt, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		if ((object)containerOpt != null && containerOpt.Kind == SymbolKind.TypeParameter)
		{
			CrefSyntax rootCrefSyntax = GetRootCrefSyntax(syntax);
			MemberCrefSyntax memberCrefSyntax = syntax.WithLeadingTrivia(null).WithTrailingTrivia(null);
			diagnostics.Add(ErrorCode.WRN_BadXMLRef, rootCrefSyntax.Location, memberCrefSyntax.ToFullString());
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		ImmutableArray<Symbol> immutableArray = syntax.Kind() switch
		{
			SyntaxKind.NameMemberCref => BindNameMemberCref((NameMemberCrefSyntax)syntax, containerOpt, out ambiguityWinner, diagnostics), 
			SyntaxKind.IndexerMemberCref => BindIndexerMemberCref((IndexerMemberCrefSyntax)syntax, containerOpt, out ambiguityWinner, diagnostics), 
			SyntaxKind.OperatorMemberCref => BindOperatorMemberCref((OperatorMemberCrefSyntax)syntax, containerOpt, out ambiguityWinner, diagnostics), 
			SyntaxKind.ConversionOperatorMemberCref => BindConversionOperatorMemberCref((ConversionOperatorMemberCrefSyntax)syntax, containerOpt, out ambiguityWinner, diagnostics), 
			_ => throw ExceptionUtilities.UnexpectedValue(syntax.Kind()), 
		};
		if (!immutableArray.Any())
		{
			CrefSyntax rootCrefSyntax2 = GetRootCrefSyntax(syntax);
			MemberCrefSyntax memberCrefSyntax2 = syntax.WithLeadingTrivia(null).WithTrailingTrivia(null);
			diagnostics.Add(ErrorCode.WRN_BadXMLRef, rootCrefSyntax2.Location, memberCrefSyntax2.ToFullString());
		}
		return immutableArray;
	}

	private ImmutableArray<Symbol> BindNameMemberCref(NameMemberCrefSyntax syntax, NamespaceOrTypeSymbol containerOpt, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		SimpleNameSyntax simpleNameSyntax = syntax.Name as SimpleNameSyntax;
		int num;
		string text;
		if (simpleNameSyntax != null)
		{
			num = simpleNameSyntax.Arity;
			text = simpleNameSyntax.Identifier.ValueText;
		}
		else
		{
			containerOpt = BindNamespaceOrTypeSymbolInCref(syntax.Name);
			num = 0;
			text = ".ctor";
		}
		if (string.IsNullOrEmpty(text))
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		ImmutableArray<Symbol> symbols = ComputeSortedCrefMembers(syntax, containerOpt, text, num, syntax.Parameters != null, diagnostics);
		if (symbols.IsEmpty)
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		return ProcessCrefMemberLookupResults(symbols, num, syntax, (num == 0) ? null : ((GenericNameSyntax)simpleNameSyntax).TypeArgumentList, syntax.Parameters, out ambiguityWinner, diagnostics);
	}

	private ImmutableArray<Symbol> BindIndexerMemberCref(IndexerMemberCrefSyntax syntax, NamespaceOrTypeSymbol containerOpt, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		ImmutableArray<Symbol> symbols = ComputeSortedCrefMembers(syntax, containerOpt, "this[]", 0, syntax.Parameters != null, diagnostics);
		if (symbols.IsEmpty)
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		return ProcessCrefMemberLookupResults(symbols, 0, syntax, null, syntax.Parameters, out ambiguityWinner, diagnostics);
	}

	private ImmutableArray<Symbol> BindOperatorMemberCref(OperatorMemberCrefSyntax syntax, NamespaceOrTypeSymbol containerOpt, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		CrefParameterListSyntax parameters = syntax.Parameters;
		SyntaxKind kind = syntax.OperatorToken.Kind();
		string text = ((parameters != null && parameters.Parameters.Count == 1) ? null : OperatorFacts.BinaryOperatorNameFromSyntaxKindIfAny(kind));
		text = text ?? OperatorFacts.UnaryOperatorNameFromSyntaxKindIfAny(kind);
		if (text == null)
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		ImmutableArray<Symbol> symbols = ComputeSortedCrefMembers(syntax, containerOpt, text, 0, syntax.Parameters != null, diagnostics);
		if (symbols.IsEmpty)
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		return ProcessCrefMemberLookupResults(symbols, 0, syntax, null, parameters, out ambiguityWinner, diagnostics);
	}

	private ImmutableArray<Symbol> BindConversionOperatorMemberCref(ConversionOperatorMemberCrefSyntax syntax, NamespaceOrTypeSymbol containerOpt, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		string memberName = ((syntax.ImplicitOrExplicitKeyword.Kind() == SyntaxKind.ImplicitKeyword) ? "op_Implicit" : "op_Explicit");
		ImmutableArray<Symbol> array = ComputeSortedCrefMembers(syntax, containerOpt, memberName, 0, syntax.Parameters != null, diagnostics);
		if (array.IsEmpty)
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		TypeSymbol returnType = BindCrefParameterOrReturnType(syntax.Type, syntax, diagnostics);
		array = array.WhereAsArray((Symbol symbol) => symbol.Kind != SymbolKind.Method || ((MethodSymbol)symbol).ReturnType == returnType);
		if (!array.Any())
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		return ProcessCrefMemberLookupResults(array, 0, syntax, null, syntax.Parameters, out ambiguityWinner, diagnostics);
	}

	private ImmutableArray<Symbol> ComputeSortedCrefMembers(CSharpSyntaxNode syntax, NamespaceOrTypeSymbol containerOpt, string memberName, int arity, bool hasParameterList, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		ImmutableArray<Symbol> result = ComputeSortedCrefMembers(containerOpt, memberName, arity, hasParameterList, ref useSiteDiagnostics);
		diagnostics.Add(syntax, useSiteDiagnostics);
		return result;
	}

	private ImmutableArray<Symbol> ComputeSortedCrefMembers(NamespaceOrTypeSymbol containerOpt, string memberName, int arity, bool hasParameterList, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		LookupResult instance = LookupResult.GetInstance();
		LookupSymbolsOrMembersInternal(instance, containerOpt, memberName, arity, null, LookupOptions.AllMethodsOnArityZero, diagnose: false, ref useSiteDiagnostics);
		ArrayBuilder<Symbol> instance2;
		if (instance.IsMultiViable)
		{
			instance2 = ArrayBuilder<Symbol>.GetInstance();
			instance2.AddRange(instance.Symbols);
			instance.Free();
		}
		else
		{
			instance.Free();
			NamedTypeSymbol namedTypeSymbol = null;
			if (arity == 0)
			{
				if (containerOpt is NamedTypeSymbol namedTypeSymbol2)
				{
					if (namedTypeSymbol2.Name == memberName && (hasParameterList || namedTypeSymbol2.Arity == 0 || ContainingType != namedTypeSymbol2.OriginalDefinition))
					{
						namedTypeSymbol = namedTypeSymbol2;
					}
				}
				else if (((object)containerOpt == null) & hasParameterList)
				{
					NamedTypeSymbol containingType = ContainingType;
					if ((object)containingType != null && memberName == containingType.Name)
					{
						namedTypeSymbol = containingType;
					}
				}
			}
			if ((object)namedTypeSymbol == null)
			{
				return ImmutableArray<Symbol>.Empty;
			}
			ImmutableArray<MethodSymbol> instanceConstructors = namedTypeSymbol.InstanceConstructors;
			int length = instanceConstructors.Length;
			if (length == 0)
			{
				return ImmutableArray<Symbol>.Empty;
			}
			instance2 = ArrayBuilder<Symbol>.GetInstance(length);
			instance2.AddRange(instanceConstructors);
		}
		if (instance2.Count > 1)
		{
			instance2.Sort(ConsistentSymbolOrder.Instance);
		}
		return instance2.ToImmutableAndFree();
	}

	private ImmutableArray<Symbol> ProcessCrefMemberLookupResults(ImmutableArray<Symbol> symbols, int arity, MemberCrefSyntax memberSyntax, TypeArgumentListSyntax typeArgumentListSyntax, BaseCrefParameterListSyntax parameterListSyntax, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		if (parameterListSyntax == null)
		{
			return ProcessParameterlessCrefMemberLookupResults(symbols, arity, memberSyntax, typeArgumentListSyntax, out ambiguityWinner, diagnostics);
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		GetCrefOverloadResolutionCandidates(symbols, arity, typeArgumentListSyntax, instance);
		ImmutableArray<ParameterSymbol> parameterSymbols = BindCrefParameters(parameterListSyntax, diagnostics);
		ImmutableArray<Symbol> result = PerformCrefOverloadResolution(instance, parameterSymbols, arity, memberSyntax, out ambiguityWinner, diagnostics);
		instance.Free();
		if (result.Length == 0)
		{
			for (int i = 0; i < parameterSymbols.Length; i++)
			{
				if (ContainsNestedTypeOfUnconstructedGenericType(parameterSymbols[i].Type))
				{
					diagnostics.Add(ErrorCode.WRN_UnqualifiedNestedTypeInCref, parameterListSyntax.Parameters[i].Location);
					break;
				}
			}
		}
		return result;
	}

	private static bool ContainsNestedTypeOfUnconstructedGenericType(TypeSymbol type)
	{
		switch (type.TypeKind)
		{
		case TypeKind.Array:
			return ContainsNestedTypeOfUnconstructedGenericType(((ArrayTypeSymbol)type).ElementType);
		case TypeKind.Pointer:
			return ContainsNestedTypeOfUnconstructedGenericType(((PointerTypeSymbol)type).PointedAtType);
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Error:
		case TypeKind.Interface:
		case TypeKind.Struct:
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			if (IsNestedTypeOfUnconstructedGenericType(namedTypeSymbol))
			{
				return true;
			}
			foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics)
			{
				if (ContainsNestedTypeOfUnconstructedGenericType(typeArgumentsNoUseSiteDiagnostic))
				{
					return true;
				}
			}
			return false;
		}
		case TypeKind.Dynamic:
		case TypeKind.TypeParameter:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
		}
	}

	private static bool IsNestedTypeOfUnconstructedGenericType(NamedTypeSymbol type)
	{
		NamedTypeSymbol containingType = type.ContainingType;
		while ((object)containingType != null)
		{
			if (containingType.Arity > 0 && containingType.IsDefinition)
			{
				return true;
			}
			containingType = containingType.ContainingType;
		}
		return false;
	}

	private ImmutableArray<Symbol> ProcessParameterlessCrefMemberLookupResults(ImmutableArray<Symbol> symbols, int arity, MemberCrefSyntax memberSyntax, TypeArgumentListSyntax typeArgumentListSyntax, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		if (symbols.Length > 1 && arity == 0)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (Symbol item in symbols)
			{
				if (item.Kind == SymbolKind.Method)
				{
					if (((MethodSymbol)item).Arity == 0)
					{
						flag = true;
					}
					else
					{
						flag2 = true;
					}
					if (flag2 & flag)
					{
						break;
					}
				}
			}
			if (flag & flag2)
			{
				symbols = symbols.WhereAsArray((Symbol s) => s.Kind != SymbolKind.Method || ((MethodSymbol)s).Arity == 0);
			}
		}
		Symbol symbol = symbols[0];
		if (symbols.Length > 1)
		{
			ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(symbols.Length);
			foreach (Symbol item2 in symbols)
			{
				instance.Add(UnwrapAliasNoDiagnostics(item2));
			}
			BestSymbolInfo bestSymbolInfo = GetBestSymbolInfo(instance, out var secondBest);
			instance.Free();
			int num = 0;
			if (bestSymbolInfo.IsFromCompilation)
			{
				num = bestSymbolInfo.Index;
				symbol = symbols[num];
			}
			if (symbol.Kind == SymbolKind.TypeParameter)
			{
				CrefSyntax rootCrefSyntax = GetRootCrefSyntax(memberSyntax);
				diagnostics.Add(ErrorCode.WRN_BadXMLRefTypeVar, rootCrefSyntax.Location, rootCrefSyntax.ToString());
			}
			else if (secondBest.IsFromCompilation == bestSymbolInfo.IsFromCompilation)
			{
				CrefSyntax rootCrefSyntax2 = GetRootCrefSyntax(memberSyntax);
				int index = ((num == 0) ? 1 : 0);
				diagnostics.Add(ErrorCode.WRN_AmbiguousXMLReference, rootCrefSyntax2.Location, rootCrefSyntax2.ToString(), symbol, symbols[index]);
				ambiguityWinner = ConstructWithCrefTypeParameters(arity, typeArgumentListSyntax, symbol);
				return symbols.SelectAsArray((Symbol sym) => ConstructWithCrefTypeParameters(arity, typeArgumentListSyntax, sym));
			}
		}
		else if (symbol.Kind == SymbolKind.TypeParameter)
		{
			CrefSyntax rootCrefSyntax3 = GetRootCrefSyntax(memberSyntax);
			diagnostics.Add(ErrorCode.WRN_BadXMLRefTypeVar, rootCrefSyntax3.Location, rootCrefSyntax3.ToString());
		}
		ambiguityWinner = null;
		return ImmutableArray.Create(ConstructWithCrefTypeParameters(arity, typeArgumentListSyntax, symbol));
	}

	private void GetCrefOverloadResolutionCandidates(ImmutableArray<Symbol> symbols, int arity, TypeArgumentListSyntax typeArgumentListSyntax, ArrayBuilder<Symbol> candidates)
	{
		foreach (Symbol item in symbols)
		{
			Symbol symbol = ConstructWithCrefTypeParameters(arity, typeArgumentListSyntax, item);
			if (!(symbol is NamedTypeSymbol namedTypeSymbol))
			{
				candidates.Add(symbol);
			}
			else
			{
				candidates.AddRange(namedTypeSymbol.InstanceConstructors);
			}
		}
	}

	private static ImmutableArray<Symbol> PerformCrefOverloadResolution(ArrayBuilder<Symbol> candidates, ImmutableArray<ParameterSymbol> parameterSymbols, int arity, MemberCrefSyntax memberSyntax, out Symbol ambiguityWinner, DiagnosticBag diagnostics)
	{
		ArrayBuilder<Symbol> arrayBuilder = null;
		foreach (Symbol candidate in candidates)
		{
			SymbolKind kind = candidate.Kind;
			Symbol member;
			if (kind != SymbolKind.Method)
			{
				if (kind == SymbolKind.NamedType)
				{
					throw ExceptionUtilities.UnexpectedValue(candidate.Kind);
				}
				if (kind != SymbolKind.Property)
				{
					continue;
				}
				member = new SignatureOnlyPropertySymbol(null, null, parameterSymbols, RefKind.None, null, ImmutableArray<CustomModifier>.Empty, ImmutableArray<CustomModifier>.Empty, isStatic: false, ImmutableArray<PropertySymbol>.Empty);
			}
			else
			{
				MethodSymbol methodSymbol = (MethodSymbol)candidate;
				MethodKind methodKind = methodSymbol.MethodKind;
				bool isVararg = methodSymbol.IsVararg;
				int count = ((methodKind != MethodKind.Constructor) ? ((arity == 0) ? methodSymbol.Arity : arity) : 0);
				member = new SignatureOnlyMethodSymbol(null, null, methodKind, typeParameters: IndexedTypeParameterSymbol.Take(count), parameters: parameterSymbols, callingConvention: isVararg ? CallingConvention.ExtraArguments : CallingConvention.HasThis, refKind: RefKind.None, returnType: null, returnTypeCustomModifiers: ImmutableArray<CustomModifier>.Empty, refCustomModifiers: ImmutableArray<CustomModifier>.Empty, explicitInterfaceImplementations: ImmutableArray<MethodSymbol>.Empty);
			}
			if (!MemberSignatureComparer.CrefComparer.Equals(member, candidate))
			{
				continue;
			}
			if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<Symbol>.GetInstance();
				arrayBuilder.Add(candidate);
				continue;
			}
			bool flag = arrayBuilder[0].GetMemberArity() == 0;
			bool flag2 = candidate.GetMemberArity() == 0;
			if (!flag | flag2)
			{
				if (!flag & flag2)
				{
					arrayBuilder.Clear();
				}
				arrayBuilder.Add(candidate);
			}
		}
		if (arrayBuilder == null)
		{
			ambiguityWinner = null;
			return ImmutableArray<Symbol>.Empty;
		}
		if (arrayBuilder.Count > 1)
		{
			ambiguityWinner = arrayBuilder[0];
			CrefSyntax rootCrefSyntax = GetRootCrefSyntax(memberSyntax);
			diagnostics.Add(ErrorCode.WRN_AmbiguousXMLReference, rootCrefSyntax.Location, rootCrefSyntax.ToString(), ambiguityWinner, arrayBuilder[1]);
		}
		else
		{
			ambiguityWinner = null;
		}
		return arrayBuilder.ToImmutableAndFree();
	}

	private Symbol ConstructWithCrefTypeParameters(int arity, TypeArgumentListSyntax typeArgumentListSyntax, Symbol symbol)
	{
		if (arity > 0)
		{
			SeparatedSyntaxList<TypeSyntax> arguments = typeArgumentListSyntax.Arguments;
			TypeSymbol[] array = new TypeSymbol[arity];
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			for (int i = 0; i < arity; i++)
			{
				TypeSyntax syntax = arguments[i];
				array[i] = BindType(syntax, instance);
				instance.Clear();
			}
			instance.Free();
			symbol = ((symbol.Kind != SymbolKind.Method) ? ((Symbol)((NamedTypeSymbol)symbol).Construct(array)) : ((Symbol)((MethodSymbol)symbol).Construct(array)));
		}
		return symbol;
	}

	private ImmutableArray<ParameterSymbol> BindCrefParameters(BaseCrefParameterListSyntax parameterListSyntax, DiagnosticBag diagnostics)
	{
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance(parameterListSyntax.Parameters.Count);
		foreach (CrefParameterSyntax parameter in parameterListSyntax.Parameters)
		{
			RefKind refKind = parameter.RefKindKeyword.Kind().GetRefKind();
			TypeSymbol type = BindCrefParameterOrReturnType(parameter.Type, (MemberCrefSyntax)parameterListSyntax.Parent, diagnostics);
			instance.Add(new SignatureOnlyParameterSymbol(type, ImmutableArray<CustomModifier>.Empty, ImmutableArray<CustomModifier>.Empty, isParams: false, refKind));
		}
		return instance.ToImmutableAndFree();
	}

	private TypeSymbol BindCrefParameterOrReturnType(TypeSyntax typeSyntax, MemberCrefSyntax memberCrefSyntax, DiagnosticBag diagnostics)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		TypeSymbol result = WithAdditionalFlags(BinderFlags.CrefParameterOrReturnType).BindType(typeSyntax, instance);
		if (instance.HasAnyErrors() && HasNonObsoleteError(instance))
		{
			ErrorCode code = ((typeSyntax.Parent.Kind() == SyntaxKind.ConversionOperatorMemberCref) ? ErrorCode.WRN_BadXMLRefReturnType : ErrorCode.WRN_BadXMLRefParamType);
			CrefSyntax rootCrefSyntax = GetRootCrefSyntax(memberCrefSyntax);
			diagnostics.Add(code, typeSyntax.Location, typeSyntax.ToString(), rootCrefSyntax.ToString());
		}
		instance.Free();
		return result;
	}

	private static bool HasNonObsoleteError(DiagnosticBag unusedDiagnostics)
	{
		foreach (Diagnostic item in unusedDiagnostics.AsEnumerable())
		{
			ErrorCode code = (ErrorCode)item.Code;
			if (code != ErrorCode.ERR_DeprecatedSymbolStr && code != ErrorCode.ERR_DeprecatedCollectionInitAddStr && item.Severity == DiagnosticSeverity.Error)
			{
				return true;
			}
		}
		return false;
	}

	private static CrefSyntax GetRootCrefSyntax(MemberCrefSyntax syntax)
	{
		SyntaxNode parent = syntax.Parent;
		if (parent != null && !parent.IsKind(SyntaxKind.XmlCrefAttribute))
		{
			return (CrefSyntax)parent;
		}
		return syntax;
	}

	internal BoundExpression BindDeconstruction(AssignmentExpressionSyntax node, DiagnosticBag diagnostics, bool resultIsUsedOverride = false)
	{
		ExpressionSyntax left = node.Left;
		ExpressionSyntax right = node.Right;
		DeclarationExpressionSyntax declaration = null;
		ExpressionSyntax expression = null;
		BoundDeconstructionAssignmentOperator result = BindDeconstruction(node, left, right, diagnostics, ref declaration, ref expression, resultIsUsedOverride);
		if (declaration != null)
		{
			switch (node.Parent?.Kind())
			{
			case null:
			case SyntaxKind.ExpressionStatement:
				if (expression != null)
				{
					Error(diagnostics, ErrorCode.ERR_MixedDeconstructionUnsupported, left);
				}
				break;
			case SyntaxKind.ForStatement:
				if (((ForStatementSyntax)node.Parent).Initializers.Contains(node))
				{
					if (expression != null)
					{
						Error(diagnostics, ErrorCode.ERR_MixedDeconstructionUnsupported, left);
					}
				}
				else
				{
					Error(diagnostics, ErrorCode.ERR_DeclarationExpressionNotPermitted, declaration);
				}
				break;
			default:
				Error(diagnostics, ErrorCode.ERR_DeclarationExpressionNotPermitted, declaration);
				break;
			}
		}
		return result;
	}

	internal BoundDeconstructionAssignmentOperator BindDeconstruction(CSharpSyntaxNode deconstruction, ExpressionSyntax left, ExpressionSyntax right, DiagnosticBag diagnostics, ref DeclarationExpressionSyntax declaration, ref ExpressionSyntax expression, bool resultIsUsedOverride = false, BoundDeconstructValuePlaceholder rightPlaceholder = null)
	{
		DeconstructionVariable deconstructionVariable = BindDeconstructionVariables(left, diagnostics, ref declaration, ref expression);
		DiagnosticBag diagnosticBag = new DiagnosticBag();
		BoundExpression boundRHS = rightPlaceholder ?? BindValue(right, diagnosticBag, BindValueKind.RValue);
		boundRHS = FixTupleLiteral(deconstructionVariable.NestedVariables, boundRHS, deconstruction, diagnosticBag);
		bool resultIsUsed = resultIsUsedOverride || IsDeconstructionResultUsed(left);
		BoundDeconstructionAssignmentOperator result = BindDeconstructionAssignment(deconstruction, left, boundRHS, deconstructionVariable.NestedVariables, resultIsUsed, diagnosticBag);
		DeconstructionVariable.FreeDeconstructionVariables(deconstructionVariable.NestedVariables);
		diagnostics.AddRange(diagnosticBag);
		return result;
	}

	private BoundDeconstructionAssignmentOperator BindDeconstructionAssignment(CSharpSyntaxNode node, ExpressionSyntax left, BoundExpression boundRHS, ArrayBuilder<DeconstructionVariable> checkedVariables, bool resultIsUsed, DiagnosticBag diagnostics)
	{
		uint valEscape = GetValEscape(boundRHS, LocalScopeDepth);
		if ((object)boundRHS.Type == null || boundRHS.Type.IsErrorType())
		{
			FailRemainingInferencesAndSetValEscape(checkedVariables, diagnostics, valEscape);
			NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Void, diagnostics, node);
			TypeSymbol type = boundRHS.Type ?? specialType;
			return new BoundDeconstructionAssignmentOperator(node, DeconstructionVariablesAsTuple(left, checkedVariables, diagnostics, ignoreDiagnosticsFromTuple: true), new BoundConversion(boundRHS.Syntax, boundRHS, Conversion.Deconstruction, @checked: false, explicitCastInCode: false, null, type, hasErrors: true), resultIsUsed, specialType, hasErrors: true);
		}
		bool flag = !MakeDeconstructionConversion(boundRHS.Type, node, boundRHS.Syntax, diagnostics, checkedVariables, out var conversion);
		FailRemainingInferencesAndSetValEscape(checkedVariables, diagnostics, valEscape);
		BoundTupleLiteral boundTupleLiteral = DeconstructionVariablesAsTuple(left, checkedVariables, diagnostics, diagnostics.HasAnyErrors() || !resultIsUsed);
		TypeSymbol type2 = (flag ? CreateErrorType() : boundTupleLiteral.Type);
		uint broadestValEscape = GetBroadestValEscape(boundTupleLiteral, LocalScopeDepth);
		boundRHS = ValidateEscape(boundRHS, broadestValEscape, isByRef: false, diagnostics);
		BoundConversion right = new BoundConversion(boundRHS.Syntax, boundRHS, conversion, @checked: false, explicitCastInCode: false, null, type2, flag)
		{
			WasCompilerGenerated = true
		};
		return new BoundDeconstructionAssignmentOperator(node, boundTupleLiteral, right, resultIsUsed, type2);
	}

	private static bool IsDeconstructionResultUsed(ExpressionSyntax left)
	{
		CSharpSyntaxNode parent = left.Parent;
		if (parent == null || parent.Kind() == SyntaxKind.ForEachVariableStatement)
		{
			return false;
		}
		CSharpSyntaxNode parent2 = parent.Parent;
		if (parent2 == null)
		{
			return false;
		}
		switch (parent2.Kind())
		{
		case SyntaxKind.ExpressionStatement:
			return ((ExpressionStatementSyntax)parent2).Expression != parent;
		case SyntaxKind.ForStatement:
		{
			ForStatementSyntax forStatementSyntax = (ForStatementSyntax)parent2;
			if (!forStatementSyntax.Incrementors.Contains(parent))
			{
				return !forStatementSyntax.Initializers.Contains(parent);
			}
			return false;
		}
		default:
			return true;
		}
	}

	private BoundExpression FixTupleLiteral(ArrayBuilder<DeconstructionVariable> checkedVariables, BoundExpression boundRHS, CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		if (boundRHS.Kind == BoundKind.TupleLiteral)
		{
			bool flag = diagnostics.HasAnyErrors();
			TypeSymbol typeSymbol = MakeMergedTupleType(checkedVariables, (BoundTupleLiteral)boundRHS, syntax, Compilation, flag ? null : diagnostics);
			if ((object)typeSymbol != null)
			{
				boundRHS = GenerateConversionForAssignment(typeSymbol, boundRHS, diagnostics);
			}
		}
		else if ((object)boundRHS.Type == null)
		{
			Error(diagnostics, ErrorCode.ERR_DeconstructRequiresExpression, boundRHS.Syntax);
		}
		return boundRHS;
	}

	private bool MakeDeconstructionConversion(TypeSymbol type, SyntaxNode syntax, SyntaxNode rightSyntax, DiagnosticBag diagnostics, ArrayBuilder<DeconstructionVariable> variables, out Conversion conversion)
	{
		conversion = Conversion.Deconstruction;
		DeconstructMethodInfo deconstructMethodInfo = default;
		ImmutableArray<TypeSymbol> foundTypes;
		if (type.IsTupleType)
		{
			foundTypes = type.TupleElementTypes;
			SetInferredTypes(variables, foundTypes, diagnostics);
			if (variables.Count != foundTypes.Length)
			{
				Error(diagnostics, ErrorCode.ERR_DeconstructWrongCardinality, syntax, foundTypes.Length, variables.Count);
				return false;
			}
		}
		else
		{
			BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder = new BoundDeconstructValuePlaceholder(syntax, LocalScopeDepth, type);
			BoundExpression boundExpression = MakeDeconstructInvocationExpression(variables.Count, boundDeconstructValuePlaceholder, rightSyntax, diagnostics, out var outPlaceholders);
			if (boundExpression.HasAnyErrors)
			{
				return false;
			}
			deconstructMethodInfo = new DeconstructMethodInfo(boundExpression, boundDeconstructValuePlaceholder, outPlaceholders);
			foundTypes = outPlaceholders.SelectAsArray((BoundDeconstructValuePlaceholder p) => p.Type);
			SetInferredTypes(variables, foundTypes, diagnostics);
		}
		bool flag = false;
		int count = variables.Count;
		ArrayBuilder<Conversion> instance = ArrayBuilder<Conversion>.GetInstance(count);
		for (int num = 0; num < count; num++)
		{
			DeconstructionVariable deconstructionVariable = variables[num];
			Conversion conversion2;
			if (deconstructionVariable.HasNestedVariables)
			{
				if (syntax.Kind() == SyntaxKind.TupleExpression)
				{
					_ = ((TupleExpressionSyntax)syntax).Arguments[num];
				}
				flag |= !MakeDeconstructionConversion(foundTypes[num], syntax, rightSyntax, diagnostics, deconstructionVariable.NestedVariables, out conversion2);
			}
			else
			{
				BoundExpression single = deconstructionVariable.Single;
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				conversion2 = Conversions.ClassifyConversionFromType(foundTypes[num], single.Type, ref useSiteDiagnostics);
				diagnostics.Add(single.Syntax, useSiteDiagnostics);
				if (!conversion2.IsImplicit)
				{
					flag = true;
					GenerateImplicitConversionError(diagnostics, Compilation, single.Syntax, conversion2, foundTypes[num], single.Type);
				}
			}
			instance.Add(conversion2);
		}
		conversion = new Conversion(ConversionKind.Deconstruction, deconstructMethodInfo, instance.ToImmutableAndFree());
		return !flag;
	}

	private void SetInferredTypes(ArrayBuilder<DeconstructionVariable> variables, ImmutableArray<TypeSymbol> foundTypes, DiagnosticBag diagnostics)
	{
		int num = Math.Min(variables.Count, foundTypes.Length);
		for (int i = 0; i < num; i++)
		{
			DeconstructionVariable deconstructionVariable = variables[i];
			if (!deconstructionVariable.HasNestedVariables)
			{
				BoundExpression single = deconstructionVariable.Single;
				if ((object)single.Type == null)
				{
					variables[i] = new DeconstructionVariable(SetInferredType(single, foundTypes[i], diagnostics), deconstructionVariable.Syntax);
				}
			}
		}
	}

	private BoundExpression SetInferredType(BoundExpression expression, TypeSymbol type, DiagnosticBag diagnostics)
	{
		return expression.Kind switch
		{
			BoundKind.DeconstructionVariablePendingInference => ((DeconstructionVariablePendingInference)expression).SetInferredType(type, this, diagnostics), 
			BoundKind.DiscardExpression => ((BoundDiscardExpression)expression).SetInferredType(type), 
			_ => throw ExceptionUtilities.UnexpectedValue(expression.Kind), 
		};
	}

	private void FailRemainingInferencesAndSetValEscape(ArrayBuilder<DeconstructionVariable> variables, DiagnosticBag diagnostics, uint rhsValEscape)
	{
		int count = variables.Count;
		for (int i = 0; i < count; i++)
		{
			DeconstructionVariable deconstructionVariable = variables[i];
			if (deconstructionVariable.HasNestedVariables)
			{
				FailRemainingInferencesAndSetValEscape(deconstructionVariable.NestedVariables, diagnostics, rhsValEscape);
				continue;
			}
			switch (deconstructionVariable.Single.Kind)
			{
			case BoundKind.Local:
			{
				BoundLocal boundLocal = (BoundLocal)deconstructionVariable.Single;
				if (boundLocal.IsDeclaration)
				{
					((SourceLocalSymbol)boundLocal.LocalSymbol).SetValEscape(rhsValEscape);
				}
				break;
			}
			case BoundKind.DeconstructionVariablePendingInference:
			{
				BoundExpression boundExpression = ((DeconstructionVariablePendingInference)deconstructionVariable.Single).FailInference(this, diagnostics);
				variables[i] = new DeconstructionVariable(boundExpression, boundExpression.Syntax);
				break;
			}
			case BoundKind.DiscardExpression:
			{
				BoundDiscardExpression boundDiscardExpression = (BoundDiscardExpression)deconstructionVariable.Single;
				if ((object)boundDiscardExpression.Type == null)
				{
					Error(diagnostics, ErrorCode.ERR_TypeInferenceFailedForImplicitlyTypedDeconstructionVariable, boundDiscardExpression.Syntax, "_");
					variables[i] = new DeconstructionVariable(boundDiscardExpression.FailInference(this, diagnostics), boundDiscardExpression.Syntax);
				}
				break;
			}
			}
		}
	}

	private static TypeSymbol MakeMergedTupleType(ArrayBuilder<DeconstructionVariable> lhsVariables, BoundTupleLiteral rhsLiteral, CSharpSyntaxNode syntax, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		int count = lhsVariables.Count;
		int length = rhsLiteral.Arguments.Length;
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(count);
		ArrayBuilder<Location> instance2 = ArrayBuilder<Location>.GetInstance(count);
		for (int i = 0; i < length; i++)
		{
			BoundExpression boundExpression = rhsLiteral.Arguments[i];
			TypeSymbol typeSymbol = boundExpression.Type;
			if (i < count)
			{
				DeconstructionVariable deconstructionVariable = lhsVariables[i];
				if (deconstructionVariable.HasNestedVariables)
				{
					if (boundExpression.Kind == BoundKind.TupleLiteral)
					{
						typeSymbol = MakeMergedTupleType(deconstructionVariable.NestedVariables, (BoundTupleLiteral)boundExpression, syntax, compilation, diagnostics);
					}
					else if ((object)typeSymbol == null)
					{
						Error(diagnostics, ErrorCode.ERR_DeconstructRequiresExpression, boundExpression.Syntax);
					}
				}
				else if ((object)deconstructionVariable.Single.Type != null)
				{
					typeSymbol = deconstructionVariable.Single.Type;
				}
			}
			else if ((object)typeSymbol == null)
			{
				Error(diagnostics, ErrorCode.ERR_DeconstructRequiresExpression, boundExpression.Syntax);
			}
			instance.Add(typeSymbol);
			instance2.Add(boundExpression.Syntax.Location);
		}
		if (instance.Any((TypeSymbol t) => t == null))
		{
			instance.Free();
			instance2.Free();
			return null;
		}
		return TupleTypeSymbol.Create(null, instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), default, compilation, shouldCheckConstraints: true, default, syntax, diagnostics);
	}

	private BoundTupleLiteral DeconstructionVariablesAsTuple(CSharpSyntaxNode syntax, ArrayBuilder<DeconstructionVariable> variables, DiagnosticBag diagnostics, bool ignoreDiagnosticsFromTuple)
	{
		int count = variables.Count;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(count);
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance(count);
		ArrayBuilder<Location> instance3 = ArrayBuilder<Location>.GetInstance(count);
		ArrayBuilder<string> inferredElementNames = ArrayBuilder<string>.GetInstance(count);
		foreach (DeconstructionVariable variable in variables)
		{
			if (variable.HasNestedVariables)
			{
				BoundTupleLiteral boundTupleLiteral = DeconstructionVariablesAsTuple(variable.Syntax, variable.NestedVariables, diagnostics, ignoreDiagnosticsFromTuple);
				instance.Add(boundTupleLiteral);
				instance2.Add(boundTupleLiteral.Type);
				inferredElementNames.Add(null);
			}
			else
			{
				BoundExpression single = variable.Single;
				instance.Add(single);
				instance2.Add(single.Type);
				inferredElementNames.Add(ExtractDeconstructResultElementName(single));
			}
			instance3.Add(variable.Syntax.Location);
		}
		ImmutableArray<BoundExpression> arguments = instance.ToImmutableAndFree();
		PooledHashSet<string> instance4 = PooledHashSet<string>.GetInstance();
		RemoveDuplicateInferredTupleNamesAndFreeIfEmptied(ref inferredElementNames, instance4);
		instance4.Free();
		ImmutableArray<string> immutableArray = inferredElementNames?.ToImmutableAndFree() ?? default(ImmutableArray<string>);
		ImmutableArray<bool> immutableArray2 = (immutableArray.IsDefault ? default(ImmutableArray<bool>) : immutableArray.SelectAsArray((string n) => n != null));
		bool flag = Compilation.LanguageVersion.DisallowInferredTupleElementNames();
		NamedTypeSymbol type = TupleTypeSymbol.Create(syntax.Location, instance2.ToImmutableAndFree(), instance3.ToImmutableAndFree(), immutableArray, Compilation, !ignoreDiagnosticsFromTuple, flag ? immutableArray2 : default(ImmutableArray<bool>), syntax, ignoreDiagnosticsFromTuple ? null : diagnostics);
		return new BoundTupleLiteral(syntax, immutableArray, immutableArray2, arguments, type);
	}

	private static string ExtractDeconstructResultElementName(BoundExpression expression)
	{
		if (expression.Kind == BoundKind.DiscardExpression)
		{
			return null;
		}
		return InferTupleElementName(expression.Syntax);
	}

	private BoundExpression MakeDeconstructInvocationExpression(int numCheckedVariables, BoundExpression receiver, SyntaxNode rightSyntax, DiagnosticBag diagnostics, out ImmutableArray<BoundDeconstructValuePlaceholder> outPlaceholders)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)receiver.Syntax;
		if (numCheckedVariables < 2)
		{
			Error(diagnostics, ErrorCode.ERR_DeconstructTooFewElements, cSharpSyntaxNode);
			outPlaceholders = default;
			return BadExpression(cSharpSyntaxNode, receiver);
		}
		if (receiver.Type.IsDynamic())
		{
			Error(diagnostics, ErrorCode.ERR_CannotDeconstructDynamic, rightSyntax);
			outPlaceholders = default;
			return BadExpression(cSharpSyntaxNode, receiver);
		}
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		ArrayBuilder<OutDeconstructVarPendingInference> instance2 = ArrayBuilder<OutDeconstructVarPendingInference>.GetInstance(numCheckedVariables);
		try
		{
			for (int i = 0; i < numCheckedVariables; i++)
			{
				OutDeconstructVarPendingInference item = new OutDeconstructVarPendingInference(cSharpSyntaxNode);
				instance.Arguments.Add(item);
				instance.RefKinds.Add(RefKind.Out);
				instance2.Add(item);
			}
			BoundExpression expr = BindInstanceMemberAccess(rightSyntax, cSharpSyntaxNode, receiver, "Deconstruct", 0, default, default, invoked: true, indexed: false, diagnostics);
			expr = CheckValue(expr, BindValueKind.RValueOrMethodGroup, diagnostics);
			expr.WasCompilerGenerated = true;
			if (expr.Kind != BoundKind.MethodGroup)
			{
				return MissingDeconstruct(receiver, rightSyntax, numCheckedVariables, diagnostics, out outPlaceholders, receiver);
			}
			BoundExpression boundExpression = BindMethodGroupInvocation(rightSyntax, rightSyntax, "Deconstruct", (BoundMethodGroup)expr, instance, diagnostics, null);
			boundExpression.WasCompilerGenerated = true;
			if (boundExpression.HasErrors && !receiver.HasAnyErrors)
			{
				return MissingDeconstruct(receiver, rightSyntax, numCheckedVariables, diagnostics, out outPlaceholders, boundExpression);
			}
			if (boundExpression.Kind != BoundKind.Call)
			{
				return MissingDeconstruct(receiver, rightSyntax, numCheckedVariables, diagnostics, out outPlaceholders, boundExpression);
			}
			MethodSymbol method = ((BoundCall)boundExpression).Method;
			ImmutableArray<ParameterSymbol> parameters = method.Parameters;
			for (int j = (method.IsExtensionMethod ? 1 : 0); j < parameters.Length; j++)
			{
				if (parameters[j].RefKind != RefKind.Out)
				{
					return MissingDeconstruct(receiver, rightSyntax, numCheckedVariables, diagnostics, out outPlaceholders, boundExpression);
				}
			}
			if (method.ReturnType.GetSpecialTypeSafe() != SpecialType.System_Void)
			{
				return MissingDeconstruct(receiver, rightSyntax, numCheckedVariables, diagnostics, out outPlaceholders, boundExpression);
			}
			if (instance2.Any((OutDeconstructVarPendingInference v) => v.Placeholder == null))
			{
				return MissingDeconstruct(receiver, rightSyntax, numCheckedVariables, diagnostics, out outPlaceholders, boundExpression);
			}
			outPlaceholders = instance2.SelectAsArray((OutDeconstructVarPendingInference v) => v.Placeholder);
			return boundExpression;
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private BoundBadExpression MissingDeconstruct(BoundExpression receiver, SyntaxNode rightSyntax, int numParameters, DiagnosticBag diagnostics, out ImmutableArray<BoundDeconstructValuePlaceholder> outPlaceholders, BoundExpression childNode)
	{
		Error(diagnostics, ErrorCode.ERR_MissingDeconstruct, rightSyntax, receiver.Type, numParameters);
		outPlaceholders = default;
		return BadExpression(rightSyntax, childNode);
	}

	private DeconstructionVariable BindDeconstructionVariables(ExpressionSyntax node, DiagnosticBag diagnostics, ref DeclarationExpressionSyntax declaration, ref ExpressionSyntax expression)
	{
		switch (node.Kind())
		{
		case SyntaxKind.DeclarationExpression:
		{
			DeclarationExpressionSyntax declarationExpressionSyntax = (DeclarationExpressionSyntax)node;
			if (declaration == null)
			{
				declaration = declarationExpressionSyntax;
			}
			bool isConst = false;
			TypeSymbol declType = BindVariableType(declarationExpressionSyntax.Designation, diagnostics, declarationExpressionSyntax.Type, ref isConst, out var isVar, out var _);
			if (declarationExpressionSyntax.Designation.Kind() == SyntaxKind.ParenthesizedVariableDesignation && !isVar)
			{
				Error(diagnostics, ErrorCode.ERR_DeconstructionVarFormDisallowsSpecificType, declarationExpressionSyntax.Designation);
			}
			return BindDeconstructionVariables(declType, declarationExpressionSyntax.Designation, declarationExpressionSyntax, diagnostics);
		}
		case SyntaxKind.TupleExpression:
		{
			TupleExpressionSyntax tupleExpressionSyntax = (TupleExpressionSyntax)node;
			ArrayBuilder<DeconstructionVariable> instance = ArrayBuilder<DeconstructionVariable>.GetInstance(tupleExpressionSyntax.Arguments.Count);
			foreach (ArgumentSyntax argument in tupleExpressionSyntax.Arguments)
			{
				if (argument.NameColon != null)
				{
					Error(diagnostics, ErrorCode.ERR_TupleElementNamesInDeconstruction, argument.NameColon);
				}
				instance.Add(BindDeconstructionVariables(argument.Expression, diagnostics, ref declaration, ref expression));
			}
			return new DeconstructionVariable(instance, node);
		}
		default:
		{
			BoundExpression expr = BindExpression(node, diagnostics, invoked: false, indexed: false);
			BoundExpression boundExpression = CheckValue(expr, BindValueKind.Assignable, diagnostics);
			if (expression == null && boundExpression.Kind != BoundKind.DiscardExpression)
			{
				expression = node;
			}
			return new DeconstructionVariable(boundExpression, node);
		}
		}
	}

	private DeconstructionVariable BindDeconstructionVariables(TypeSymbol declType, VariableDesignationSyntax node, CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.SingleVariableDesignation:
		{
			SingleVariableDesignationSyntax designation = (SingleVariableDesignationSyntax)node;
			return new DeconstructionVariable(BindDeconstructionVariable(declType, designation, syntax, diagnostics), syntax);
		}
		case SyntaxKind.DiscardDesignation:
			_ = (DiscardDesignationSyntax)node;
			return new DeconstructionVariable(BindDiscardExpression(syntax, declType), syntax);
		case SyntaxKind.ParenthesizedVariableDesignation:
		{
			ParenthesizedVariableDesignationSyntax parenthesizedVariableDesignationSyntax = (ParenthesizedVariableDesignationSyntax)node;
			ArrayBuilder<DeconstructionVariable> instance = ArrayBuilder<DeconstructionVariable>.GetInstance();
			foreach (VariableDesignationSyntax variable in parenthesizedVariableDesignationSyntax.Variables)
			{
				instance.Add(BindDeconstructionVariables(declType, variable, variable, diagnostics));
			}
			return new DeconstructionVariable(instance, syntax);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind());
		}
	}

	private static BoundDiscardExpression BindDiscardExpression(SyntaxNode syntax, TypeSymbol declType)
	{
		return new BoundDiscardExpression(syntax, declType);
	}

	private BoundExpression BindDeconstructionVariable(TypeSymbol declType, SingleVariableDesignationSyntax designation, CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		SourceLocalSymbol sourceLocalSymbol = LookupLocal(designation.Identifier);
		if ((object)sourceLocalSymbol != null)
		{
			bool hasErrors = sourceLocalSymbol.ScopeBinder.ValidateDeclarationNameConflictsInScope(sourceLocalSymbol, diagnostics);
			if ((object)declType != null)
			{
				return new BoundLocal(syntax, sourceLocalSymbol, isDeclaration: true, null, declType, hasErrors);
			}
			return new DeconstructionVariablePendingInference(syntax, sourceLocalSymbol, null);
		}
		GlobalExpressionVariable globalExpressionVariable = LookupDeclaredField(designation);
		if ((object)globalExpressionVariable == null)
		{
			throw ExceptionUtilities.Unreachable;
		}
		BoundThisReference boundThisReference = ThisReference(designation, ContainingType, hasErrors: false, wasCompilerGenerated: true);
		if ((object)declType != null)
		{
			TypeSymbol fieldType = globalExpressionVariable.GetFieldType(FieldsBeingBound);
			return new BoundFieldAccess(syntax, boundThisReference, globalExpressionVariable, null, LookupResultKind.Viable, isDeclaration: true, fieldType);
		}
		return new DeconstructionVariablePendingInference(syntax, globalExpressionVariable, boundThisReference);
	}

	internal bool HasThis(bool isExplicit, out bool inStaticContext)
	{
		Symbol symbol = ContainingMemberOrLambda?.ContainingNonLambdaMember();
		if ((object)symbol != null && symbol.IsStatic)
		{
			inStaticContext = symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Method || symbol.Kind == SymbolKind.Property;
			return false;
		}
		inStaticContext = false;
		if (InConstructorInitializer || InAttributeArgument)
		{
			return false;
		}
		bool flag = (symbol?.ContainingType)?.IsScriptClass ?? false;
		if (InFieldInitializer && !flag)
		{
			return false;
		}
		if (flag)
		{
			return !isExplicit;
		}
		return true;
	}

	protected virtual bool IsUnboundTypeAllowed(GenericNameSyntax syntax)
	{
		return _next.IsUnboundTypeAllowed(syntax);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax)
	{
		return BadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, BoundExpression childNode)
	{
		return BadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, childNode);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, ImmutableArray<BoundExpression> childNodes)
	{
		return BadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, childNodes);
	}

	protected BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind lookupResultKind)
	{
		return BadExpression(syntax, lookupResultKind, ImmutableArray<Symbol>.Empty);
	}

	protected BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind lookupResultKind, BoundExpression childNode)
	{
		return BadExpression(syntax, lookupResultKind, ImmutableArray<Symbol>.Empty, childNode);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols)
	{
		return new BoundBadExpression(syntax, resultKind, symbols, ImmutableArray<BoundExpression>.Empty, CreateErrorType());
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols, BoundExpression childNode)
	{
		return new BoundBadExpression(syntax, resultKind, symbols, ImmutableArray.Create(childNode), CreateErrorType());
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols, ImmutableArray<BoundExpression> childNodes)
	{
		return new BoundBadExpression(syntax, resultKind, symbols, childNodes, CreateErrorType());
	}

	private BoundExpression ToBadExpression(BoundExpression expr, LookupResultKind resultKind = LookupResultKind.Empty)
	{
		TypeSymbol type = expr.Type;
		BoundKind kind = expr.Kind;
		if (expr.HasAnyErrors && ((object)type != null || kind == BoundKind.UnboundLambda))
		{
			return expr;
		}
		if (kind == BoundKind.BadExpression)
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)expr;
			return boundBadExpression.Update(resultKind, boundBadExpression.Symbols, boundBadExpression.ChildBoundNodes, type);
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		expr.GetExpressionSymbols(instance, null, this);
		return new BoundBadExpression(expr.Syntax, resultKind, instance.ToImmutableAndFree(), ImmutableArray.Create(expr), type ?? CreateErrorType());
	}

	internal TypeSymbol CreateErrorType(string name = "")
	{
		return new ExtendedErrorTypeSymbol(Compilation, name, 0, null);
	}

	internal BoundExpression BindValue(ExpressionSyntax node, DiagnosticBag diagnostics, BindValueKind valueKind)
	{
		BoundExpression expr = BindExpression(node, diagnostics, invoked: false, indexed: false);
		return CheckValue(expr, valueKind, diagnostics);
	}

	internal BoundExpression BindValueAllowArgList(ExpressionSyntax node, DiagnosticBag diagnostics, BindValueKind valueKind)
	{
		BoundExpression expr = BindExpressionAllowArgList(node, diagnostics);
		return CheckValue(expr, valueKind, diagnostics);
	}

	internal BoundFieldEqualsValue BindFieldInitializer(FieldSymbol field, EqualsValueClauseSyntax initializerOpt, DiagnosticBag diagnostics)
	{
		if (initializerOpt == null)
		{
			return null;
		}
		Binder binder = GetBinder(initializerOpt);
		BoundExpression value = binder.BindVariableOrAutoPropInitializerValue(initializerOpt, RefKind.None, field.GetFieldType(binder.FieldsBeingBound), diagnostics);
		return new BoundFieldEqualsValue(initializerOpt, field, binder.GetDeclaredLocalsForScope(initializerOpt), value);
	}

	internal BoundExpression BindVariableOrAutoPropInitializerValue(EqualsValueClauseSyntax initializerOpt, RefKind refKind, TypeSymbol varType, DiagnosticBag diagnostics)
	{
		if (initializerOpt == null)
		{
			return null;
		}
		IsInitializerRefKindValid(initializerOpt, initializerOpt, refKind, diagnostics, out var valueKind, out var value);
		BoundExpression expression = BindPossibleArrayInitializer(value, varType, valueKind, diagnostics);
		return GenerateConversionForAssignment(varType, expression, diagnostics);
	}

	internal Binder CreateBinderForParameterDefaultValue(ParameterSymbol parameter, EqualsValueClauseSyntax defaultValueSyntax)
	{
		LocalScopeBinder next = new LocalScopeBinder(WithContainingMemberOrLambda(parameter.ContainingSymbol).WithAdditionalFlags(BinderFlags.ParameterDefaultValue));
		return new ExecutableCodeBinder(defaultValueSyntax, parameter.ContainingSymbol, next);
	}

	internal BoundParameterEqualsValue BindParameterDefaultValue(EqualsValueClauseSyntax defaultValueSyntax, ParameterSymbol parameter, DiagnosticBag diagnostics, out BoundExpression valueBeforeConversion)
	{
		Binder binder = GetBinder(defaultValueSyntax);
		valueBeforeConversion = binder.BindValue(defaultValueSyntax.Value, diagnostics, BindValueKind.RValue);
		return new BoundParameterEqualsValue(defaultValueSyntax, parameter, binder.GetDeclaredLocalsForScope(defaultValueSyntax), binder.GenerateConversionForAssignment(parameter.Type, valueBeforeConversion, diagnostics, isDefaultParameter: true));
	}

	internal BoundFieldEqualsValue BindEnumConstantInitializer(SourceEnumConstantSymbol symbol, EqualsValueClauseSyntax equalsValueSyntax, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(equalsValueSyntax);
		BoundExpression expression = binder.BindValue(equalsValueSyntax.Value, diagnostics, BindValueKind.RValue);
		expression = binder.GenerateConversionForAssignment(symbol.ContainingType.EnumUnderlyingType, expression, diagnostics);
		return new BoundFieldEqualsValue(equalsValueSyntax, symbol, binder.GetDeclaredLocalsForScope(equalsValueSyntax), expression);
	}

	public BoundExpression BindExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		return BindExpression(node, diagnostics, invoked: false, indexed: false);
	}

	protected BoundExpression BindExpression(ExpressionSyntax node, DiagnosticBag diagnostics, bool invoked, bool indexed)
	{
		BoundExpression boundExpression = BindExpressionInternal(node, diagnostics, invoked, indexed);
		VerifyUnchecked(node, diagnostics, boundExpression);
		if (boundExpression.Kind == BoundKind.ArgListOperator)
		{
			Error(diagnostics, ErrorCode.ERR_IllegalArglist, node);
			boundExpression = ToBadExpression(boundExpression);
		}
		return boundExpression;
	}

	protected BoundExpression BindExpressionAllowArgList(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindExpressionInternal(node, diagnostics, invoked: false, indexed: false);
		VerifyUnchecked(node, diagnostics, boundExpression);
		return boundExpression;
	}

	private void VerifyUnchecked(ExpressionSyntax node, DiagnosticBag diagnostics, BoundExpression expr)
	{
		if (!expr.HasAnyErrors && !IsInsideNameof)
		{
			TypeSymbol type = expr.Type;
			if ((object)type != null && type.IsUnsafe())
			{
				ReportUnsafeIfNotAllowed(node, diagnostics);
			}
		}
	}

	private BoundExpression BindExpressionInternal(ExpressionSyntax node, DiagnosticBag diagnostics, bool invoked, bool indexed)
	{
		if (IsEarlyAttributeBinder && !EarlyWellKnownAttributeBinder.CanBeValidAttributeArgument(node, this))
		{
			return BadExpression(node, LookupResultKind.NotAValue);
		}
		switch (node.Kind())
		{
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
			return BindAnonymousFunction(node, diagnostics);
		case SyntaxKind.ThisExpression:
			return BindThis((ThisExpressionSyntax)node, diagnostics);
		case SyntaxKind.BaseExpression:
			return BindBase((BaseExpressionSyntax)node, diagnostics);
		case SyntaxKind.InvocationExpression:
			return BindInvocationExpression((InvocationExpressionSyntax)node, diagnostics);
		case SyntaxKind.ArrayInitializerExpression:
			return BindUnexpectedArrayInitializer((InitializerExpressionSyntax)node, diagnostics, ErrorCode.ERR_ArrayInitInBadPlace);
		case SyntaxKind.ArrayCreationExpression:
			return BindArrayCreationExpression((ArrayCreationExpressionSyntax)node, diagnostics);
		case SyntaxKind.ImplicitArrayCreationExpression:
			return BindImplicitArrayCreationExpression((ImplicitArrayCreationExpressionSyntax)node, diagnostics);
		case SyntaxKind.StackAllocArrayCreationExpression:
			return BindStackAllocArrayCreationExpression((StackAllocArrayCreationExpressionSyntax)node, diagnostics);
		case SyntaxKind.ImplicitStackAllocArrayCreationExpression:
			return BindImplicitStackAllocArrayCreationExpression((ImplicitStackAllocArrayCreationExpressionSyntax)node, diagnostics);
		case SyntaxKind.ObjectCreationExpression:
			return BindObjectCreationExpression((ObjectCreationExpressionSyntax)node, diagnostics);
		case SyntaxKind.IdentifierName:
		case SyntaxKind.GenericName:
			return BindIdentifier((SimpleNameSyntax)node, invoked, indexed, diagnostics);
		case SyntaxKind.SimpleMemberAccessExpression:
		case SyntaxKind.PointerMemberAccessExpression:
			return BindMemberAccess((MemberAccessExpressionSyntax)node, invoked, indexed, diagnostics);
		case SyntaxKind.SimpleAssignmentExpression:
			return BindAssignment((AssignmentExpressionSyntax)node, diagnostics);
		case SyntaxKind.CastExpression:
			return BindCast((CastExpressionSyntax)node, diagnostics);
		case SyntaxKind.ElementAccessExpression:
			return BindElementAccess((ElementAccessExpressionSyntax)node, diagnostics);
		case SyntaxKind.AddExpression:
		case SyntaxKind.SubtractExpression:
		case SyntaxKind.MultiplyExpression:
		case SyntaxKind.DivideExpression:
		case SyntaxKind.ModuloExpression:
		case SyntaxKind.LeftShiftExpression:
		case SyntaxKind.RightShiftExpression:
		case SyntaxKind.BitwiseOrExpression:
		case SyntaxKind.BitwiseAndExpression:
		case SyntaxKind.ExclusiveOrExpression:
		case SyntaxKind.EqualsExpression:
		case SyntaxKind.NotEqualsExpression:
		case SyntaxKind.LessThanExpression:
		case SyntaxKind.LessThanOrEqualExpression:
		case SyntaxKind.GreaterThanExpression:
		case SyntaxKind.GreaterThanOrEqualExpression:
			return BindSimpleBinaryOperator((BinaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.LogicalOrExpression:
		case SyntaxKind.LogicalAndExpression:
			return BindConditionalLogicalOperator((BinaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.CoalesceExpression:
			return BindNullCoalescingOperator((BinaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.ConditionalAccessExpression:
			return BindConditionalAccessExpression((ConditionalAccessExpressionSyntax)node, diagnostics);
		case SyntaxKind.MemberBindingExpression:
			return BindMemberBindingExpression((MemberBindingExpressionSyntax)node, invoked, indexed, diagnostics);
		case SyntaxKind.ElementBindingExpression:
			return BindElementBindingExpression((ElementBindingExpressionSyntax)node, diagnostics);
		case SyntaxKind.IsExpression:
			return BindIsOperator((BinaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.AsExpression:
			return BindAsOperator((BinaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.UnaryPlusExpression:
		case SyntaxKind.UnaryMinusExpression:
		case SyntaxKind.BitwiseNotExpression:
		case SyntaxKind.LogicalNotExpression:
			return BindUnaryOperator((PrefixUnaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.AddressOfExpression:
			return BindAddressOfExpression((PrefixUnaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.PointerIndirectionExpression:
			return BindPointerIndirectionExpression((PrefixUnaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.PostIncrementExpression:
		case SyntaxKind.PostDecrementExpression:
			return BindIncrementOperator(node, ((PostfixUnaryExpressionSyntax)node).Operand, ((PostfixUnaryExpressionSyntax)node).OperatorToken, diagnostics);
		case SyntaxKind.PreIncrementExpression:
		case SyntaxKind.PreDecrementExpression:
			return BindIncrementOperator(node, ((PrefixUnaryExpressionSyntax)node).Operand, ((PrefixUnaryExpressionSyntax)node).OperatorToken, diagnostics);
		case SyntaxKind.ConditionalExpression:
			return BindConditionalOperator((ConditionalExpressionSyntax)node, diagnostics);
		case SyntaxKind.NumericLiteralExpression:
		case SyntaxKind.StringLiteralExpression:
		case SyntaxKind.CharacterLiteralExpression:
		case SyntaxKind.TrueLiteralExpression:
		case SyntaxKind.FalseLiteralExpression:
		case SyntaxKind.NullLiteralExpression:
			return BindLiteralConstant((LiteralExpressionSyntax)node, diagnostics);
		case SyntaxKind.DefaultLiteralExpression:
			return BindDefaultLiteral(node);
		case SyntaxKind.ParenthesizedExpression:
			return BindParenthesizedExpression(((ParenthesizedExpressionSyntax)node).Expression, diagnostics);
		case SyntaxKind.CheckedExpression:
		case SyntaxKind.UncheckedExpression:
			return BindCheckedExpression((CheckedExpressionSyntax)node, diagnostics);
		case SyntaxKind.DefaultExpression:
			return BindDefaultExpression((DefaultExpressionSyntax)node, diagnostics);
		case SyntaxKind.TypeOfExpression:
			return BindTypeOf((TypeOfExpressionSyntax)node, diagnostics);
		case SyntaxKind.SizeOfExpression:
			return BindSizeOf((SizeOfExpressionSyntax)node, diagnostics);
		case SyntaxKind.AddAssignmentExpression:
		case SyntaxKind.SubtractAssignmentExpression:
		case SyntaxKind.MultiplyAssignmentExpression:
		case SyntaxKind.DivideAssignmentExpression:
		case SyntaxKind.ModuloAssignmentExpression:
		case SyntaxKind.AndAssignmentExpression:
		case SyntaxKind.ExclusiveOrAssignmentExpression:
		case SyntaxKind.OrAssignmentExpression:
		case SyntaxKind.LeftShiftAssignmentExpression:
		case SyntaxKind.RightShiftAssignmentExpression:
			return BindCompoundAssignment((AssignmentExpressionSyntax)node, diagnostics);
		case SyntaxKind.AliasQualifiedName:
		case SyntaxKind.PredefinedType:
			return BindNamespaceOrType(node, diagnostics);
		case SyntaxKind.QueryExpression:
			return BindQuery((QueryExpressionSyntax)node, diagnostics);
		case SyntaxKind.AnonymousObjectCreationExpression:
			return BindAnonymousObjectCreation((AnonymousObjectCreationExpressionSyntax)node, diagnostics);
		case SyntaxKind.QualifiedName:
			return BindQualifiedName((QualifiedNameSyntax)node, diagnostics);
		case SyntaxKind.ComplexElementInitializerExpression:
			return BindUnexpectedComplexElementInitializer((InitializerExpressionSyntax)node, diagnostics);
		case SyntaxKind.ArgListExpression:
			return BindArgList(node, diagnostics);
		case SyntaxKind.RefTypeExpression:
			return BindRefType((RefTypeExpressionSyntax)node, diagnostics);
		case SyntaxKind.MakeRefExpression:
			return BindMakeRef((MakeRefExpressionSyntax)node, diagnostics);
		case SyntaxKind.RefValueExpression:
			return BindRefValue((RefValueExpressionSyntax)node, diagnostics);
		case SyntaxKind.AwaitExpression:
			return BindAwait((AwaitExpressionSyntax)node, diagnostics);
		case SyntaxKind.OmittedTypeArgument:
		case SyntaxKind.ObjectInitializerExpression:
		case SyntaxKind.OmittedArraySizeExpression:
			return BadExpression(node);
		case SyntaxKind.NullableType:
			return BadExpression(node);
		case SyntaxKind.InterpolatedStringExpression:
			return BindInterpolatedString((InterpolatedStringExpressionSyntax)node, diagnostics);
		case SyntaxKind.IsPatternExpression:
			return BindIsPatternExpression((IsPatternExpressionSyntax)node, diagnostics);
		case SyntaxKind.TupleExpression:
			return BindTupleExpression((TupleExpressionSyntax)node, diagnostics);
		case SyntaxKind.ThrowExpression:
			return BindThrowExpression((ThrowExpressionSyntax)node, diagnostics);
		case SyntaxKind.RefType:
			return BindRefType(node, diagnostics);
		case SyntaxKind.RefExpression:
			return BindRefExpression(node, diagnostics);
		case SyntaxKind.DeclarationExpression:
			return BindDeclarationExpression((DeclarationExpressionSyntax)node, diagnostics);
		default:
			return BadExpression(node);
		}
	}

	private static BoundExpression BindDefaultLiteral(ExpressionSyntax node)
	{
		return new BoundDefaultExpression(node, null, null);
	}

	private BoundExpression BindRefExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		SyntaxToken firstToken = node.GetFirstToken();
		diagnostics.Add(ErrorCode.ERR_UnexpectedToken, firstToken.GetLocation(), firstToken.ValueText);
		return new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, CreateErrorType("ref"));
	}

	private BoundExpression BindRefType(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		SyntaxToken firstToken = node.GetFirstToken();
		diagnostics.Add(ErrorCode.ERR_UnexpectedToken, firstToken.GetLocation(), firstToken.ValueText);
		return new BoundTypeExpression(node, null, CreateErrorType("ref"));
	}

	private BoundExpression BindThrowExpression(ThrowExpressionSyntax node, DiagnosticBag diagnostics)
	{
		bool hasErrors = node.HasErrors;
		if (!IsThrowExpressionInProperContext(node))
		{
			diagnostics.Add(ErrorCode.ERR_ThrowMisplaced, node.ThrowKeyword.GetLocation());
			hasErrors = true;
		}
		BoundExpression expression = BindThrownExpression(node.Expression, diagnostics, ref hasErrors);
		return new BoundThrowExpression(node, expression, null, hasErrors);
	}

	private static bool IsThrowExpressionInProperContext(ThrowExpressionSyntax node)
	{
		CSharpSyntaxNode parent = node.Parent;
		if (parent == null || node.HasErrors)
		{
			return true;
		}
		switch (node.Parent.Kind())
		{
		case SyntaxKind.ConditionalExpression:
		{
			ConditionalExpressionSyntax conditionalExpressionSyntax = (ConditionalExpressionSyntax)parent;
			if (node != conditionalExpressionSyntax.WhenTrue)
			{
				return node == conditionalExpressionSyntax.WhenFalse;
			}
			return true;
		}
		case SyntaxKind.CoalesceExpression:
		{
			BinaryExpressionSyntax binaryExpressionSyntax = (BinaryExpressionSyntax)parent;
			return node == binaryExpressionSyntax.Right;
		}
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
		case SyntaxKind.ArrowExpressionClause:
			return true;
		default:
			return false;
		}
	}

	private BoundExpression BindDeclarationExpression(DeclarationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		bool isConst = false;
		TypeSymbol declType = BindVariableType(node.Designation, diagnostics, node.Type, ref isConst, out var _, out var _);
		Error(diagnostics, ErrorCode.ERR_DeclarationExpressionNotPermitted, node);
		return BindDeclarationVariables(declType, node.Designation, node, diagnostics);
	}

	private BoundExpression BindDeclarationVariables(TypeSymbol declType, VariableDesignationSyntax node, CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		declType = declType ?? CreateErrorType("var");
		switch (node.Kind())
		{
		case SyntaxKind.SingleVariableDesignation:
		{
			SingleVariableDesignationSyntax designation = (SingleVariableDesignationSyntax)node;
			return BindDeconstructionVariable(declType, designation, syntax, diagnostics);
		}
		case SyntaxKind.DiscardDesignation:
			return BindDiscardExpression(syntax, declType);
		case SyntaxKind.ParenthesizedVariableDesignation:
		{
			ParenthesizedVariableDesignationSyntax parenthesizedVariableDesignationSyntax = (ParenthesizedVariableDesignationSyntax)node;
			int count = parenthesizedVariableDesignationSyntax.Variables.Count;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(count);
			ArrayBuilder<string> inferredElementNames = ArrayBuilder<string>.GetInstance(count);
			foreach (VariableDesignationSyntax variable in parenthesizedVariableDesignationSyntax.Variables)
			{
				instance.Add(BindDeclarationVariables(declType, variable, variable, diagnostics));
				inferredElementNames.Add(InferTupleElementName(variable));
			}
			ImmutableArray<BoundExpression> immutableArray = instance.ToImmutableAndFree();
			PooledHashSet<string> instance2 = PooledHashSet<string>.GetInstance();
			RemoveDuplicateInferredTupleNamesAndFreeIfEmptied(ref inferredElementNames, instance2);
			instance2.Free();
			ImmutableArray<string> immutableArray2 = inferredElementNames?.ToImmutableAndFree() ?? default(ImmutableArray<string>);
			ImmutableArray<bool> immutableArray3 = (immutableArray2.IsDefault ? default(ImmutableArray<bool>) : immutableArray2.SelectAsArray((string n) => n != null));
			bool flag = Compilation.LanguageVersion.DisallowInferredTupleElementNames();
			NamedTypeSymbol type = TupleTypeSymbol.Create(null, immutableArray.SelectAsArray((BoundExpression e) => e.Type), default, immutableArray2, Compilation, shouldCheckConstraints: false, flag ? immutableArray3 : default(ImmutableArray<bool>));
			return new BoundTupleLiteral(syntax, default, immutableArray3, immutableArray, type);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind());
		}
	}

	private BoundExpression BindTupleExpression(TupleExpressionSyntax node, DiagnosticBag diagnostics)
	{
		SeparatedSyntaxList<ArgumentSyntax> arguments = node.Arguments;
		int count = arguments.Count;
		if (count < 2)
		{
			ImmutableArray<BoundExpression> childNodes = ((count == 1) ? ImmutableArray.Create(BindValue(arguments[0].Expression, diagnostics, BindValueKind.RValue)) : ImmutableArray<BoundExpression>.Empty);
			return BadExpression(node, childNodes);
		}
		bool flag = true;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(arguments.Count);
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance(arguments.Count);
		ArrayBuilder<Location> instance3 = ArrayBuilder<Location>.GetInstance(arguments.Count);
		(ImmutableArray<string> elementNamesArray, ImmutableArray<bool> inferredArray, bool hasErrors) tuple = ExtractTupleElementNames(arguments, diagnostics);
		ImmutableArray<string> item = tuple.elementNamesArray;
		ImmutableArray<bool> item2 = tuple.inferredArray;
		bool item3 = tuple.hasErrors;
		for (int i = 0; i < count; i++)
		{
			ArgumentSyntax argumentSyntax = arguments[i];
			IdentifierNameSyntax identifierNameSyntax = argumentSyntax.NameColon?.Name;
			if (identifierNameSyntax != null)
			{
				instance3.Add(identifierNameSyntax.Location);
			}
			else
			{
				instance3.Add(argumentSyntax.Location);
			}
			BoundExpression boundExpression = BindValue(argumentSyntax.Expression, diagnostics, BindValueKind.RValue);
			TypeSymbol type = boundExpression.Type;
			if ((object)type != null && type.SpecialType == SpecialType.System_Void)
			{
				diagnostics.Add(ErrorCode.ERR_VoidInTuple, argumentSyntax.Location);
				boundExpression = new BoundBadExpression(argumentSyntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), CreateErrorType("void"));
			}
			instance.Add(boundExpression);
			TypeSymbol type2 = boundExpression.Type;
			instance2.Add(type2);
			if ((object)type2 == null)
			{
				flag = false;
			}
		}
		NamedTypeSymbol type3 = null;
		ImmutableArray<TypeSymbol> immutableArray = instance2.ToImmutableAndFree();
		ImmutableArray<Location> immutableArray2 = instance3.ToImmutableAndFree();
		if (flag)
		{
			bool flag2 = Compilation.LanguageVersion.DisallowInferredTupleElementNames();
			type3 = TupleTypeSymbol.Create(node.Location, immutableArray, immutableArray2, item, Compilation, shouldCheckConstraints: true, syntax: node, diagnostics: diagnostics, errorPositions: flag2 ? item2 : default(ImmutableArray<bool>));
		}
		else
		{
			TupleTypeSymbol.VerifyTupleTypePresent(immutableArray.Length, node, Compilation, diagnostics);
		}
		return new BoundTupleLiteral(node, item, item2, instance.ToImmutableAndFree(), type3, item3);
	}

	private static (ImmutableArray<string> elementNamesArray, ImmutableArray<bool> inferredArray, bool hasErrors) ExtractTupleElementNames(SeparatedSyntaxList<ArgumentSyntax> arguments, DiagnosticBag diagnostics)
	{
		bool item = false;
		int count = arguments.Count;
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		ArrayBuilder<string> elementNames = null;
		ArrayBuilder<string> elementNames2 = null;
		for (int i = 0; i < count; i++)
		{
			ArgumentSyntax argumentSyntax = arguments[i];
			IdentifierNameSyntax identifierNameSyntax = argumentSyntax.NameColon?.Name;
			string name = null;
			string name2 = null;
			if (identifierNameSyntax != null)
			{
				name = identifierNameSyntax.Identifier.ValueText;
				if (diagnostics != null && !CheckTupleMemberName(name, i, argumentSyntax.NameColon.Name, diagnostics, instance))
				{
					item = true;
				}
			}
			else
			{
				name2 = InferTupleElementName(argumentSyntax.Expression);
			}
			CollectTupleFieldMemberName(name, i, count, ref elementNames);
			CollectTupleFieldMemberName(name2, i, count, ref elementNames2);
		}
		RemoveDuplicateInferredTupleNamesAndFreeIfEmptied(ref elementNames2, instance);
		instance.Free();
		(ImmutableArray<string>, ImmutableArray<bool>) tuple = MergeTupleElementNames(elementNames, elementNames2);
		elementNames?.Free();
		elementNames2?.Free();
		return (elementNamesArray: tuple.Item1, inferredArray: tuple.Item2, hasErrors: item);
	}

	private static (ImmutableArray<string> names, ImmutableArray<bool> inferred) MergeTupleElementNames(ArrayBuilder<string> elementNames, ArrayBuilder<string> inferredElementNames)
	{
		if (elementNames == null)
		{
			if (inferredElementNames == null)
			{
				return (names: default, inferred: default);
			}
			ImmutableArray<string> immutableArray = inferredElementNames.ToImmutable();
			return (names: immutableArray, inferred: immutableArray.SelectAsArray((string n) => n != null));
		}
		if (inferredElementNames == null)
		{
			return (names: elementNames.ToImmutable(), inferred: default);
		}
		ArrayBuilder<bool> instance = ArrayBuilder<bool>.GetInstance(elementNames.Count);
		for (int num = 0; num < elementNames.Count; num++)
		{
			string text = inferredElementNames[num];
			if (elementNames[num] == null && text != null)
			{
				elementNames[num] = text;
				instance.Add(item: true);
			}
			else
			{
				instance.Add(item: false);
			}
		}
		return (names: elementNames.ToImmutable(), inferred: instance.ToImmutableAndFree());
	}

	private static void RemoveDuplicateInferredTupleNamesAndFreeIfEmptied(ref ArrayBuilder<string> inferredElementNames, HashSet<string> uniqueFieldNames)
	{
		if (inferredElementNames == null)
		{
			return;
		}
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		foreach (string inferredElementName in inferredElementNames)
		{
			if (inferredElementName != null && !uniqueFieldNames.Add(inferredElementName))
			{
				instance.Add(inferredElementName);
			}
		}
		for (int i = 0; i < inferredElementNames.Count; i++)
		{
			string text = inferredElementNames[i];
			if (text != null && instance.Contains(text))
			{
				inferredElementNames[i] = null;
			}
		}
		instance.Free();
		if (inferredElementNames.All((string n) => n == null))
		{
			inferredElementNames.Free();
			inferredElementNames = null;
		}
	}

	private static string InferTupleElementName(SyntaxNode syntax)
	{
		string text = syntax.TryGetInferredMemberName();
		if (text == null || TupleTypeSymbol.IsElementNameReserved(text) != -1)
		{
			return null;
		}
		return text;
	}

	private BoundExpression BindRefValue(RefValueExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		bool hasErrors = boundExpression.HasAnyErrors;
		TypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_TypedReference);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(boundExpression, specialType, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		if (!conversion.IsImplicit || !conversion.IsValid)
		{
			hasErrors = true;
			GenerateImplicitConversionError(diagnostics, node, conversion, boundExpression, specialType);
		}
		boundExpression = CreateConversion(boundExpression, conversion, specialType, diagnostics);
		TypeSymbol type = BindType(node.Type, diagnostics);
		return new BoundRefValueOperator(node, boundExpression, type, hasErrors);
	}

	private BoundExpression BindMakeRef(MakeRefExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Expression, diagnostics, BindValueKind.RefOrOut);
		bool hasErrors = boundExpression.HasAnyErrors;
		TypeSymbol specialType = GetSpecialType(SpecialType.System_TypedReference, diagnostics, node);
		if ((object)boundExpression.Type != null && boundExpression.Type.IsRestrictedType())
		{
			Error(diagnostics, ErrorCode.ERR_MethodArgCantBeRefAny, node, boundExpression.Type);
			hasErrors = true;
		}
		return new BoundMakeRefOperator(node, boundExpression, specialType, hasErrors);
	}

	private BoundExpression BindRefType(RefTypeExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		bool hasErrors = boundExpression.HasAnyErrors;
		TypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_TypedReference);
		TypeSymbol wellKnownType = Compilation.GetWellKnownType(WellKnownType.System_Type);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(boundExpression, specialType, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		if (!conversion.IsImplicit || !conversion.IsValid)
		{
			hasErrors = true;
			GenerateImplicitConversionError(diagnostics, node, conversion, boundExpression, specialType);
		}
		boundExpression = CreateConversion(boundExpression, conversion, specialType, diagnostics);
		return new BoundRefTypeOperator(node, boundExpression, null, wellKnownType, hasErrors);
	}

	private BoundExpression BindArgList(CSharpSyntaxNode node, DiagnosticBag diagnostics)
	{
		TypeSymbol specialType = GetSpecialType(SpecialType.System_RuntimeArgumentHandle, diagnostics, node);
		MethodSymbol methodSymbol = ContainingMember() as MethodSymbol;
		bool hasErrors = false;
		if ((object)methodSymbol == null || !methodSymbol.IsVararg)
		{
			Error(diagnostics, ErrorCode.ERR_ArgsInvalid, node);
			hasErrors = true;
		}
		else if (ContainingMemberOrLambda != methodSymbol)
		{
			Error(diagnostics, ErrorCode.ERR_SpecialByRefInLambda, node, specialType);
			hasErrors = true;
		}
		return new BoundArgList(node, specialType, hasErrors);
	}

	private BoundExpression BindQualifiedName(QualifiedNameSyntax node, DiagnosticBag diagnostics)
	{
		return BindMemberAccessWithBoundLeft(node, BindLeftOfPotentialColorColorMemberAccess(node.Left, diagnostics), node.Right, node.DotToken, invoked: false, indexed: false, diagnostics);
	}

	private BoundExpression BindParenthesizedExpression(ExpressionSyntax innerExpression, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindExpression(innerExpression, diagnostics);
		CheckNotNamespaceOrType(boundExpression, diagnostics);
		return boundExpression;
	}

	private BoundExpression BindTypeOf(TypeOfExpressionSyntax node, DiagnosticBag diagnostics)
	{
		ExpressionSyntax type = node.Type;
		TypeSymbol type2 = new TypeofBinder(type, this).BindType(type, diagnostics, out var alias);
		bool hasErrors = false;
		if (type2.IsDynamic())
		{
			diagnostics.Add(ErrorCode.ERR_BadDynamicTypeof, node.Location);
			hasErrors = true;
		}
		BoundTypeExpression sourceType = new BoundTypeExpression(type, alias, type2, type2.IsErrorType());
		return new BoundTypeOfOperator(node, sourceType, null, GetWellKnownType(WellKnownType.System_Type, diagnostics, node), hasErrors);
	}

	private BoundExpression BindSizeOf(SizeOfExpressionSyntax node, DiagnosticBag diagnostics)
	{
		ExpressionSyntax type = node.Type;
		TypeSymbol typeSymbol = BindType(type, diagnostics, out var alias);
		bool flag = typeSymbol.IsErrorType();
		if (!flag && typeSymbol.IsManagedType)
		{
			diagnostics.Add(ErrorCode.ERR_ManagedAddr, node.Location, typeSymbol);
			flag = true;
		}
		BoundTypeExpression sourceType = new BoundTypeExpression(type, alias, typeSymbol, flag);
		ConstantValue constantSizeOf = GetConstantSizeOf(typeSymbol);
		bool hasErrors = (object)constantSizeOf == null && ReportUnsafeIfNotAllowed(node, diagnostics, typeSymbol);
		return new BoundSizeOfOperator(node, sourceType, constantSizeOf, GetSpecialType(SpecialType.System_Int32, diagnostics, node), hasErrors);
	}

	internal static ConstantValue GetConstantSizeOf(TypeSymbol type)
	{
		return ConstantValue.CreateSizeOf((type.GetEnumUnderlyingType() ?? type).SpecialType);
	}

	private BoundExpression BindDefaultExpression(DefaultExpressionSyntax node, DiagnosticBag diagnostics)
	{
		TypeSymbol type = BindType(node.Type, diagnostics);
		return new BoundDefaultExpression(node, type);
	}

	private BoundExpression BindIdentifier(SimpleNameSyntax node, bool invoked, bool indexed, DiagnosticBag diagnostics)
	{
		if (node.IsMissing)
		{
			return BadExpression(node);
		}
		int arity = node.Arity;
		bool flag = arity > 0;
		SeparatedSyntaxList<TypeSyntax> separatedSyntaxList = ((node.Kind() == SyntaxKind.GenericName) ? ((GenericNameSyntax)node).TypeArgumentList.Arguments : default(SeparatedSyntaxList<TypeSyntax>));
		ImmutableArray<TypeSymbol> typeArguments = (flag ? BindTypeArguments(separatedSyntaxList, diagnostics) : default(ImmutableArray<TypeSymbol>));
		LookupResult instance = LookupResult.GetInstance();
		LookupOptions lookupOptions = LookupOptions.AllMethodsOnArityZero;
		if (invoked)
		{
			lookupOptions |= LookupOptions.MustBeInvocableIfMember;
		}
		if (!IsInMethodBody && !IsInsideNameof)
		{
			lookupOptions |= LookupOptions.MustNotBeMethodTypeParameter;
		}
		string valueText = node.Identifier.ValueText;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsWithFallback(instance, valueText, arity, ref useSiteDiagnostics, null, lookupOptions);
		diagnostics.Add(node, useSiteDiagnostics);
		BoundExpression boundExpression2;
		if (instance.Kind != LookupResultKind.Empty)
		{
			bool flag2 = false;
			ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
			Symbol symbol = GetSymbolOrMethodOrPropertyGroup(instance, node, valueText, node.Arity, instance2, diagnostics, out var wasError);
			flag2 |= wasError;
			if ((object)symbol == null)
			{
				BoundExpression boundExpression = SynthesizeMethodGroupReceiver(node, instance2);
				boundExpression2 = ConstructBoundMemberGroupAndReportOmittedTypeArguments(node, separatedSyntaxList, typeArguments, boundExpression, valueText, instance2, instance, (boundExpression != null) ? BoundMethodGroupFlags.HasImplicitReceiver : BoundMethodGroupFlags.None, flag2, diagnostics);
			}
			else
			{
				bool flag3 = symbol.Kind == SymbolKind.NamedType || symbol.Kind == SymbolKind.ErrorType;
				if (flag & flag3)
				{
					symbol = ConstructNamedTypeUnlessTypeArgumentOmitted(node, (NamedTypeSymbol)symbol, separatedSyntaxList, typeArguments, diagnostics);
				}
				boundExpression2 = BindNonMethod(node, symbol, diagnostics, instance.Kind, indexed, flag2);
				if (!flag3 && (flag || node.Kind() == SyntaxKind.GenericName))
				{
					boundExpression2 = new BoundBadExpression(node, LookupResultKind.WrongArity, ImmutableArray.Create(symbol), ImmutableArray.Create(boundExpression2), boundExpression2.Type, flag2);
				}
			}
			instance2.Free();
		}
		else
		{
			if (node.IsKind(SyntaxKind.IdentifierName) && FallBackOnDiscard((IdentifierNameSyntax)node, diagnostics))
			{
				instance.Free();
				return new BoundDiscardExpression(node, null);
			}
			boundExpression2 = BadExpression(node);
			if (instance.Error != null)
			{
				Error(diagnostics, instance.Error, node);
			}
			else if (IsJoinRangeVariableInLeftKey(node))
			{
				Error(diagnostics, ErrorCode.ERR_QueryOuterKey, node, valueText);
			}
			else if (IsInJoinRightKey(node))
			{
				Error(diagnostics, ErrorCode.ERR_QueryInnerKey, node, valueText);
			}
			else
			{
				Error(diagnostics, ErrorCode.ERR_NameNotInContext, node, valueText);
			}
		}
		instance.Free();
		return boundExpression2;
	}

	private static bool FallBackOnDiscard(IdentifierNameSyntax node, DiagnosticBag diagnostics)
	{
		if (node.Identifier.ContextualKind() != SyntaxKind.UnderscoreToken)
		{
			return false;
		}
		int num;
		if (node.GetContainingDeconstruction() == null)
		{
			num = (IsOutVarDiscardIdentifier(node) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0036;
			}
		}
		else
		{
			num = 1;
		}
		CheckFeatureAvailability(node, MessageID.IDS_FeatureTuples, diagnostics);
		goto IL_0036;
		IL_0036:
		return (byte)num != 0;
	}

	private static bool IsOutVarDiscardIdentifier(SimpleNameSyntax node)
	{
		CSharpSyntaxNode parent = node.Parent;
		if (parent != null && parent.Kind() == SyntaxKind.Argument)
		{
			return ((ArgumentSyntax)parent).RefOrOutKeyword.Kind() == SyntaxKind.OutKeyword;
		}
		return false;
	}

	private BoundExpression SynthesizeMethodGroupReceiver(CSharpSyntaxNode syntax, ArrayBuilder<Symbol> members)
	{
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType == null)
		{
			return null;
		}
		NamedTypeSymbol containingType2 = members[0].ContainingType;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (containingType.IsEqualToOrDerivedFrom(containingType2, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics))
		{
			return ThisReference(syntax, containingType, hasErrors: false, wasCompilerGenerated: true);
		}
		return TryBindInteractiveReceiver(syntax, ContainingMember(), containingType, containingType2);
	}

	private bool IsBadLocalOrParameterCapture(Symbol symbol, TypeSymbol type, RefKind refKind)
	{
		if ((refKind != RefKind.None || type.IsByRefLikeType) && ContainingMemberOrLambda is MethodSymbol methodSymbol && (object)symbol.ContainingSymbol != methodSymbol)
		{
			if (methodSymbol.MethodKind == MethodKind.AnonymousFunction || methodSymbol.MethodKind == MethodKind.LocalFunction)
			{
				return !IsInsideNameof;
			}
			return false;
		}
		return false;
	}

	private BoundExpression BindNonMethod(SimpleNameSyntax node, Symbol symbol, DiagnosticBag diagnostics, LookupResultKind resultKind, bool indexed, bool isError)
	{
		if (symbol.Kind != SymbolKind.Event)
		{
			ReportDiagnosticsIfObsolete(diagnostics, symbol, node, hasBaseReceiver: false);
		}
		LocalSymbol localSymbol;
		TypeSymbol type;
		ConstantValue constantValueOpt;
		switch (symbol.Kind)
		{
		case SymbolKind.Local:
		{
			localSymbol = (LocalSymbol)symbol;
			Location location = localSymbol.Locations[0];
			if (node.SyntaxTree == location.SourceTree && node.SpanStart < location.SourceSpan.Start)
			{
				LookupResult instance = LookupResult.GetInstance();
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				LookupMembersInType(instance, ContainingType, localSymbol.Name, 0, null, LookupOptions.Default, this, diagnose: false, ref useSiteDiagnostics);
				diagnostics.Add(node, useSiteDiagnostics);
				FieldSymbol fieldSymbol = instance.SingleSymbolOrDefault as FieldSymbol;
				instance.Free();
				if ((object)fieldSymbol != null)
				{
					Error(diagnostics, ErrorCode.ERR_VariableUsedBeforeDeclarationAndHidesField, node, node, fieldSymbol);
				}
				else
				{
					Error(diagnostics, ErrorCode.ERR_VariableUsedBeforeDeclaration, node, node);
				}
				type = new ExtendedErrorTypeSymbol(Compilation, "var", 0, null, unreported: false, variableUsedBeforeDeclaration: true);
			}
			else
			{
				SourceLocalSymbol obj = localSymbol as SourceLocalSymbol;
				if ((object)obj != null && obj.IsVar)
				{
					SyntaxNode forbiddenZone = localSymbol.ForbiddenZone;
					if (forbiddenZone != null && forbiddenZone.Contains(node))
					{
						diagnostics.Add(localSymbol.ForbiddenDiagnostic, node.Location, node);
						type = new ExtendedErrorTypeSymbol(Compilation, "var", 0, null, unreported: false, variableUsedBeforeDeclaration: true);
						goto IL_01da;
					}
				}
				type = localSymbol.Type;
				if (IsBadLocalOrParameterCapture(localSymbol, type, localSymbol.RefKind))
				{
					isError = true;
					Error(diagnostics, ErrorCode.ERR_AnonDelegateCantUseLocal, node, localSymbol);
				}
			}
			goto IL_01da;
		}
		case SymbolKind.Parameter:
		{
			ParameterSymbol parameterSymbol = (ParameterSymbol)symbol;
			if (IsBadLocalOrParameterCapture(parameterSymbol, parameterSymbol.Type, parameterSymbol.RefKind))
			{
				isError = true;
				Error(diagnostics, ErrorCode.ERR_AnonDelegateCantUse, node, parameterSymbol.Name);
			}
			return new BoundParameter(node, parameterSymbol, isError);
		}
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		case SymbolKind.TypeParameter:
			return new BoundTypeExpression(node, null, (TypeSymbol)symbol, isError);
		case SymbolKind.Property:
		{
			BoundExpression receiver3 = SynthesizeReceiver(node, symbol, diagnostics);
			return BindPropertyAccess(node, receiver3, (PropertySymbol)symbol, diagnostics, resultKind, isError);
		}
		case SymbolKind.Event:
		{
			BoundExpression receiver2 = SynthesizeReceiver(node, symbol, diagnostics);
			return BindEventAccess(node, receiver2, (EventSymbol)symbol, diagnostics, resultKind, isError);
		}
		case SymbolKind.Field:
		{
			BoundExpression receiver = SynthesizeReceiver(node, symbol, diagnostics);
			return BindFieldAccess(node, receiver, (FieldSymbol)symbol, diagnostics, resultKind, indexed, isError);
		}
		case SymbolKind.Namespace:
			return new BoundNamespaceExpression(node, (NamespaceSymbol)symbol, isError);
		case SymbolKind.Alias:
		{
			AliasSymbol aliasSymbol = (AliasSymbol)symbol;
			symbol = aliasSymbol.Target;
			switch (symbol.Kind)
			{
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
				return new BoundTypeExpression(node, aliasSymbol, inferredType: false, (NamedTypeSymbol)symbol, isError);
			case SymbolKind.Namespace:
				return new BoundNamespaceExpression(node, (NamespaceSymbol)symbol, aliasSymbol, isError);
			default:
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
		}
		case SymbolKind.RangeVariable:
			return BindRangeVariable(node, (RangeVariableSymbol)symbol, diagnostics);
		default:
			{
				throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
			}
			IL_01da:
			constantValueOpt = ((localSymbol.IsConst && !IsInsideNameof && !type.IsErrorType()) ? localSymbol.GetConstantValue(node, LocalInProgress, diagnostics) : null);
			return new BoundLocal(node, localSymbol, constantValueOpt, type, isError);
		}
	}

	protected virtual BoundExpression BindRangeVariable(SimpleNameSyntax node, RangeVariableSymbol qv, DiagnosticBag diagnostics)
	{
		return Next.BindRangeVariable(node, qv, diagnostics);
	}

	private BoundExpression SynthesizeReceiver(CSharpSyntaxNode node, Symbol member, DiagnosticBag diagnostics)
	{
		if (member.IsStatic)
		{
			return null;
		}
		NamedTypeSymbol containingType = ContainingType;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (containingType.IsEqualToOrDerivedFrom(member.ContainingType, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics))
		{
			bool flag = false;
			if (EnclosingNameofArgument != node)
			{
				if (InFieldInitializer && !containingType.IsScriptClass)
				{
					Error(diagnostics, ErrorCode.ERR_FieldInitRefNonstatic, node, member);
					flag = true;
				}
				else if (InConstructorInitializer || InAttributeArgument)
				{
					Error(diagnostics, ErrorCode.ERR_ObjectRequired, node, member);
					flag = true;
				}
				else
				{
					Symbol symbol = ContainingMember();
					if (symbol.IsStatic || (symbol.Kind == SymbolKind.NamedType && !containingType.IsScriptClass))
					{
						Error(diagnostics, ErrorCode.ERR_ObjectRequired, node, member);
						flag = true;
					}
				}
				flag = flag || IsRefOrOutThisParameterCaptured(node, diagnostics);
			}
			return ThisReference(node, containingType, flag, wasCompilerGenerated: true);
		}
		return TryBindInteractiveReceiver(node, ContainingMember(), containingType, member.ContainingType);
	}

	internal Symbol ContainingMember()
	{
		Symbol symbol = ContainingMemberOrLambda;
		while (symbol.Kind != SymbolKind.NamedType && (object)symbol.ContainingSymbol != null && symbol.ContainingSymbol.Kind != SymbolKind.NamedType)
		{
			symbol = symbol.ContainingSymbol;
		}
		return symbol;
	}

	private BoundExpression TryBindInteractiveReceiver(CSharpSyntaxNode syntax, Symbol currentMember, NamedTypeSymbol currentType, NamedTypeSymbol memberDeclaringType)
	{
		if (currentType.TypeKind == TypeKind.Submission && !currentMember.IsStatic)
		{
			if (memberDeclaringType.TypeKind == TypeKind.Submission)
			{
				return new BoundPreviousSubmissionReference(syntax, memberDeclaringType)
				{
					WasCompilerGenerated = true
				};
			}
			TypeSymbol hostObjectTypeSymbol = Compilation.GetHostObjectTypeSymbol();
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if ((object)hostObjectTypeSymbol != null && hostObjectTypeSymbol.IsEqualToOrDerivedFrom(memberDeclaringType, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics))
			{
				return new BoundHostObjectMemberReference(syntax, hostObjectTypeSymbol)
				{
					WasCompilerGenerated = true
				};
			}
		}
		return null;
	}

	public BoundExpression BindNamespaceOrTypeOrExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		if (node.Kind() == SyntaxKind.PredefinedType)
		{
			return BindNamespaceOrType(node, diagnostics);
		}
		if (SyntaxFacts.IsName(node.Kind()))
		{
			if (SyntaxFacts.IsNamespaceAliasQualifier(node))
			{
				return BindNamespaceAlias((IdentifierNameSyntax)node, diagnostics);
			}
			if (SyntaxFacts.IsInNamespaceOrTypeContext(node))
			{
				return BindNamespaceOrType(node, diagnostics);
			}
		}
		else if (SyntaxFacts.IsTypeSyntax(node.Kind()))
		{
			return BindNamespaceOrType(node, diagnostics);
		}
		return BindExpression(node, diagnostics, SyntaxFacts.IsInvoked(node), SyntaxFacts.IsIndexed(node));
	}

	public BoundExpression BindLabel(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		if (!(node is IdentifierNameSyntax identifierNameSyntax))
		{
			return BadExpression(node, LookupResultKind.NotLabel);
		}
		LookupResult instance = LookupResult.GetInstance();
		string valueText = identifierNameSyntax.Identifier.ValueText;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsWithFallback(instance, valueText, 0, ref useSiteDiagnostics, null, LookupOptions.LabelsOnly);
		diagnostics.Add(node, useSiteDiagnostics);
		if (!instance.IsMultiViable)
		{
			Error(diagnostics, ErrorCode.ERR_LabelNotFound, node, valueText);
			instance.Free();
			return BadExpression(node, instance.Kind);
		}
		LabelSymbol label = (LabelSymbol)instance.Symbols.First();
		instance.Free();
		return new BoundLabel(node, label, null);
	}

	public BoundExpression BindNamespaceOrType(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		Symbol symbol = BindNamespaceOrTypeOrAliasSymbol(node, diagnostics, null, suppressUseSiteDiagnostics: false);
		return CreateBoundNamespaceOrTypeExpression(node, symbol);
	}

	public BoundExpression BindNamespaceAlias(IdentifierNameSyntax node, DiagnosticBag diagnostics)
	{
		Symbol symbol = BindNamespaceAliasSymbol(node, diagnostics);
		return CreateBoundNamespaceOrTypeExpression(node, symbol);
	}

	private static BoundExpression CreateBoundNamespaceOrTypeExpression(ExpressionSyntax node, Symbol symbol)
	{
		AliasSymbol aliasSymbol = symbol as AliasSymbol;
		if ((object)aliasSymbol != null)
		{
			symbol = aliasSymbol.Target;
		}
		if (symbol is TypeSymbol type)
		{
			return new BoundTypeExpression(node, aliasSymbol, inferredType: false, type);
		}
		if (symbol is NamespaceSymbol namespaceSymbol)
		{
			return new BoundNamespaceExpression(node, namespaceSymbol, aliasSymbol);
		}
		throw ExceptionUtilities.UnexpectedValue(symbol);
	}

	private BoundThisReference BindThis(ThisExpressionSyntax node, DiagnosticBag diagnostics)
	{
		bool hasErrors = true;
		if (!HasThis(isExplicit: true, out var inStaticContext))
		{
			Error(diagnostics, inStaticContext ? ErrorCode.ERR_ThisInStaticMeth : ErrorCode.ERR_ThisInBadContext, node);
		}
		else
		{
			hasErrors = IsRefOrOutThisParameterCaptured(node, diagnostics);
		}
		return ThisReference(node, ContainingType, hasErrors);
	}

	private BoundThisReference ThisReference(CSharpSyntaxNode node, NamedTypeSymbol thisTypeOpt, bool hasErrors = false, bool wasCompilerGenerated = false)
	{
		return new BoundThisReference(node, thisTypeOpt ?? CreateErrorType(), hasErrors)
		{
			WasCompilerGenerated = wasCompilerGenerated
		};
	}

	private bool IsRefOrOutThisParameterCaptured(SyntaxNode node, DiagnosticBag diagnostics)
	{
		ParameterSymbol parameterSymbol = ContainingMemberOrLambda.EnclosingThisSymbol();
		if ((object)parameterSymbol != null && parameterSymbol.ContainingSymbol != ContainingMemberOrLambda && parameterSymbol.RefKind != RefKind.None)
		{
			Error(diagnostics, ErrorCode.ERR_ThisStructNotInAnonMeth, node);
			return true;
		}
		return false;
	}

	private BoundBaseReference BindBase(BaseExpressionSyntax node, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = (((object)ContainingType == null) ? null : ContainingType.BaseTypeNoUseSiteDiagnostics);
		bool hasErrors = true;
		if (!HasThis(isExplicit: true, out var inStaticContext))
		{
			Error(diagnostics, inStaticContext ? ErrorCode.ERR_BaseInStaticMeth : ErrorCode.ERR_BaseInBadContext, node);
		}
		else if ((object)namedTypeSymbol == null)
		{
			Error(diagnostics, ErrorCode.ERR_NoBaseClass, node);
		}
		else if (node.Parent.Kind() != SyntaxKind.SimpleMemberAccessExpression && node.Parent.Kind() != SyntaxKind.ElementAccessExpression)
		{
			Error(diagnostics, ErrorCode.ERR_BaseIllegal, node);
		}
		else if (!IsRefOrOutThisParameterCaptured(node, diagnostics))
		{
			hasErrors = false;
		}
		return new BoundBaseReference(node, namedTypeSymbol, hasErrors);
	}

	private BoundExpression BindCast(CastExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		TypeSymbol typeSymbol = BindType(node.Type, diagnostics);
		if (typeSymbol.IsNullableType() && !boundExpression.HasAnyErrors && boundExpression.Type != null && !boundExpression.Type.IsNullableType() && typeSymbol.GetNullableUnderlyingType() != boundExpression.Type)
		{
			return BindExplicitNullableCastFromNonNullable(node, boundExpression, typeSymbol, diagnostics);
		}
		return BindCastCore(node, boundExpression, typeSymbol, boundExpression.WasCompilerGenerated, diagnostics);
	}

	private BoundExpression BindCastCore(ExpressionSyntax node, BoundExpression operand, TypeSymbol targetType, bool wasCompilerGenerated, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(operand, targetType, ref useSiteDiagnostics, forCast: true);
		diagnostics.Add(node, useSiteDiagnostics);
		if (operand.HasAnyErrors || targetType.IsErrorType() || !conversion.IsValid || targetType.IsStatic)
		{
			GenerateExplicitConversionErrors(diagnostics, node, conversion, operand, targetType);
			return new BoundConversion(node, operand, conversion, CheckOverflowAtRuntime, explicitCastInCode: true, null, targetType, hasErrors: true);
		}
		return CreateConversion(node, operand, conversion, isCast: true, wasCompilerGenerated, targetType, diagnostics);
	}

	private void GenerateExplicitConversionErrors(DiagnosticBag diagnostics, SyntaxNode syntax, Conversion conversion, BoundExpression operand, TypeSymbol targetType)
	{
		if (operand.Kind == BoundKind.UnboundLambda)
		{
			GenerateAnonymousFunctionConversionError(diagnostics, operand.Syntax, (UnboundLambda)operand, targetType);
		}
		else
		{
			if (operand.HasAnyErrors || targetType.IsErrorType())
			{
				return;
			}
			if (targetType.IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_ConvertToStaticClass, syntax.Location, targetType);
				return;
			}
			if (!targetType.IsReferenceType && !targetType.IsNullableType() && operand.IsLiteralNull())
			{
				diagnostics.Add(ErrorCode.ERR_ValueCantBeNull, syntax.Location, targetType);
				return;
			}
			if (conversion.ResultKind == LookupResultKind.OverloadResolutionFailure)
			{
				ImmutableArray<MethodSymbol> originalUserDefinedConversions = conversion.OriginalUserDefinedConversions;
				if (originalUserDefinedConversions.Length > 1)
				{
					diagnostics.Add(ErrorCode.ERR_AmbigUDConv, syntax.Location, originalUserDefinedConversions[0], originalUserDefinedConversions[1], operand.Display, targetType);
				}
				else
				{
					SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(Compilation, operand.Type, targetType);
					diagnostics.Add(ErrorCode.ERR_NoExplicitConv, syntax.Location, symbolDistinguisher.First, symbolDistinguisher.Second);
				}
				return;
			}
			switch (operand.Kind)
			{
			case BoundKind.MethodGroup:
			{
				if (targetType.TypeKind != TypeKind.Delegate || !MethodGroupConversionDoesNotExistOrHasErrors((BoundMethodGroup)operand, (NamedTypeSymbol)targetType, syntax.Location, diagnostics, out var _))
				{
					diagnostics.Add(ErrorCode.ERR_NoExplicitConv, syntax.Location, MessageID.IDS_SK_METHOD.Localize(), targetType);
				}
				return;
			}
			case BoundKind.TupleLiteral:
			{
				BoundTupleLiteral boundTupleLiteral = (BoundTupleLiteral)operand;
				ImmutableArray<TypeSymbol> elementTypes = default;
				if (targetType.TryGetElementTypesIfTupleOrCompatible(out elementTypes) && elementTypes.Length == boundTupleLiteral.Arguments.Length)
				{
					GenerateExplicitConversionErrorsForTupleLiteralArguments(diagnostics, boundTupleLiteral.Arguments, elementTypes);
					return;
				}
				if ((object)boundTupleLiteral.Type == null)
				{
					Error(diagnostics, ErrorCode.ERR_ConversionNotTupleCompatible, syntax, boundTupleLiteral.Arguments.Length, targetType);
					return;
				}
				break;
			}
			case BoundKind.StackAllocArrayCreation:
			{
				BoundStackAllocArrayCreation boundStackAllocArrayCreation = (BoundStackAllocArrayCreation)operand;
				Error(diagnostics, ErrorCode.ERR_StackAllocConversionNotPossible, syntax, boundStackAllocArrayCreation.ElementType, targetType);
				return;
			}
			}
			SymbolDistinguisher symbolDistinguisher2 = new SymbolDistinguisher(Compilation, operand.Type, targetType);
			diagnostics.Add(ErrorCode.ERR_NoExplicitConv, syntax.Location, symbolDistinguisher2.First, symbolDistinguisher2.Second);
		}
	}

	private void GenerateExplicitConversionErrorsForTupleLiteralArguments(DiagnosticBag diagnostics, ImmutableArray<BoundExpression> tupleArguments, ImmutableArray<TypeSymbol> targetElementTypes)
	{
		_ = tupleArguments.Length;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		for (int i = 0; i < targetElementTypes.Length; i++)
		{
			BoundExpression boundExpression = tupleArguments[i];
			TypeSymbol typeSymbol = targetElementTypes[i];
			Conversion conversion = Conversions.ClassifyConversionFromExpression(boundExpression, typeSymbol, ref useSiteDiagnostics);
			if (!conversion.IsValid)
			{
				GenerateExplicitConversionErrors(diagnostics, boundExpression.Syntax, conversion, boundExpression, typeSymbol);
			}
		}
	}

	private BoundExpression BindExplicitNullableCastFromNonNullable(ExpressionSyntax node, BoundExpression operand, TypeSymbol targetType, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		TypeSymbol nullableUnderlyingType = targetType.GetNullableUnderlyingType();
		if (!Conversions.ClassifyBuiltInConversion(operand.Type, nullableUnderlyingType, ref useSiteDiagnostics).Exists)
		{
			return BindCastCore(node, operand, targetType, operand.WasCompilerGenerated, diagnostics);
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			BoundExpression boundExpression = BindCastCore(node, operand, targetType.GetNullableUnderlyingType(), wasCompilerGenerated: false, instance);
			if (boundExpression.HasErrors || instance.HasAnyErrors())
			{
				Error(diagnostics, ErrorCode.ERR_NoExplicitConv, node, operand.Type, targetType);
				return new BoundConversion(node, operand, Conversion.NoConversion, CheckOverflowAtRuntime, explicitCastInCode: true, null, targetType, hasErrors: true);
			}
			if (boundExpression.ConstantValue != null)
			{
				boundExpression.WasCompilerGenerated = true;
				return BindCastCore(node, boundExpression, targetType, operand.WasCompilerGenerated, diagnostics);
			}
			return BindCastCore(node, operand, targetType, operand.WasCompilerGenerated, diagnostics);
		}
		finally
		{
			instance.Free();
		}
	}

	private static NameSyntax GetNameSyntax(SyntaxNode syntax)
	{
		string nameString;
		return GetNameSyntax(syntax, out nameString);
	}

	private static NameSyntax GetNameSyntax(SyntaxNode syntax, out string nameString)
	{
		nameString = string.Empty;
		while (true)
		{
			switch (syntax.Kind())
			{
			case SyntaxKind.PredefinedType:
				nameString = ((PredefinedTypeSyntax)syntax).Keyword.ValueText;
				return null;
			case SyntaxKind.SimpleLambdaExpression:
				nameString = MessageID.IDS_Lambda.Localize().ToString();
				return null;
			case SyntaxKind.ParenthesizedExpression:
				syntax = ((ParenthesizedExpressionSyntax)syntax).Expression;
				break;
			case SyntaxKind.CastExpression:
				syntax = ((CastExpressionSyntax)syntax).Expression;
				break;
			case SyntaxKind.SimpleMemberAccessExpression:
			case SyntaxKind.PointerMemberAccessExpression:
				return ((MemberAccessExpressionSyntax)syntax).Name;
			case SyntaxKind.MemberBindingExpression:
				return ((MemberBindingExpressionSyntax)syntax).Name;
			default:
				return syntax as NameSyntax;
			}
		}
	}

	private static string GetName(ExpressionSyntax syntax)
	{
		NameSyntax nameSyntax = GetNameSyntax(syntax, out var nameString);
		if (nameSyntax != null)
		{
			return nameSyntax.GetUnqualifiedName().Identifier.ValueText;
		}
		return nameString;
	}

	private void BindArgumentsAndNames(ArgumentListSyntax argumentListOpt, DiagnosticBag diagnostics, AnalyzedArguments result, bool allowArglist = false, bool isDelegateCreation = false)
	{
		if (argumentListOpt != null)
		{
			BindArgumentsAndNames(argumentListOpt.Arguments, diagnostics, result, allowArglist, isDelegateCreation);
		}
	}

	private void BindArgumentsAndNames(BracketedArgumentListSyntax argumentListOpt, DiagnosticBag diagnostics, AnalyzedArguments result)
	{
		if (argumentListOpt != null)
		{
			BindArgumentsAndNames(argumentListOpt.Arguments, diagnostics, result, allowArglist: false);
		}
	}

	private void BindArgumentsAndNames(SeparatedSyntaxList<ArgumentSyntax> arguments, DiagnosticBag diagnostics, AnalyzedArguments result, bool allowArglist, bool isDelegateCreation = false)
	{
		bool hadError = false;
		bool hadLangVersionError = false;
		foreach (ArgumentSyntax item in arguments)
		{
			BindArgumentAndName(result, diagnostics, ref hadError, ref hadLangVersionError, item, allowArglist, isDelegateCreation);
		}
	}

	private bool RefMustBeObeyed(bool isDelegateCreation, ArgumentSyntax argumentSyntax)
	{
		if (Compilation.FeatureStrictEnabled || !isDelegateCreation)
		{
			return true;
		}
		switch (argumentSyntax.Expression.Kind())
		{
		case SyntaxKind.ParenthesizedExpression:
		case SyntaxKind.InvocationExpression:
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
		case SyntaxKind.ObjectCreationExpression:
		case SyntaxKind.DeclarationExpression:
			return true;
		default:
			return false;
		}
	}

	private void BindArgumentAndName(AnalyzedArguments result, DiagnosticBag diagnostics, ref bool hadError, ref bool hadLangVersionError, ArgumentSyntax argumentSyntax, bool allowArglist, bool isDelegateCreation = false)
	{
		RefKind refKind = argumentSyntax.RefOrOutKeyword.Kind().GetRefKind();
		RefKind refKind2 = ((refKind == RefKind.None || RefMustBeObeyed(isDelegateCreation, argumentSyntax)) ? refKind : RefKind.None);
		BoundExpression boundArgumentExpression = BindArgumentValue(diagnostics, argumentSyntax, allowArglist, refKind2);
		BindArgumentAndName(result, diagnostics, ref hadLangVersionError, argumentSyntax, boundArgumentExpression, argumentSyntax.NameColon, refKind2);
		if ((!hadError & isDelegateCreation) && refKind != RefKind.None && result.Arguments.Count == 1)
		{
			BoundExpression boundExpression = result.Argument(0);
			BoundKind kind = boundExpression.Kind;
			if (kind == BoundKind.PropertyAccess || kind == BoundKind.IndexerAccess)
			{
				BindValueKind valueKind = ((refKind == RefKind.In) ? BindValueKind.ReadonlyRef : BindValueKind.RefOrOut);
				hadError = !CheckValueKind(argumentSyntax, boundExpression, valueKind, checkingReceiver: false, diagnostics);
				return;
			}
		}
		if (argumentSyntax.RefOrOutKeyword.Kind() != SyntaxKind.None)
		{
			argumentSyntax.Expression.CheckDeconstructionCompatibleArgument(diagnostics);
		}
	}

	private BoundExpression BindArgumentValue(DiagnosticBag diagnostics, ArgumentSyntax argumentSyntax, bool allowArglist, RefKind refKind)
	{
		if (argumentSyntax.Expression.Kind() == SyntaxKind.DeclarationExpression)
		{
			DeclarationExpressionSyntax declarationExpressionSyntax = (DeclarationExpressionSyntax)argumentSyntax.Expression;
			if (declarationExpressionSyntax.IsOutDeclaration())
			{
				return BindOutDeclarationArgument(declarationExpressionSyntax, diagnostics);
			}
		}
		return BindArgumentExpression(diagnostics, argumentSyntax.Expression, refKind, allowArglist);
	}

	private BoundExpression BindOutDeclarationArgument(DeclarationExpressionSyntax declarationExpression, DiagnosticBag diagnostics)
	{
		TypeSyntax type = declarationExpression.Type;
		VariableDesignationSyntax designation = declarationExpression.Designation;
		if (type.GetRefKind() != RefKind.None)
		{
			diagnostics.Add(ErrorCode.ERR_OutVariableCannotBeByRef, declarationExpression.Type.Location);
		}
		switch (designation.Kind())
		{
		case SyntaxKind.DiscardDesignation:
		{
			bool isConst = false;
			TypeSymbol type2 = BindVariableType(designation, diagnostics, type, ref isConst, out var _, out var _);
			return new BoundDiscardExpression(declarationExpression, type2);
		}
		case SyntaxKind.SingleVariableDesignation:
			return BindOutVariableDeclarationArgument(declarationExpression, diagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(designation.Kind());
		}
	}

	private BoundExpression BindOutVariableDeclarationArgument(DeclarationExpressionSyntax declarationExpression, DiagnosticBag diagnostics)
	{
		SingleVariableDesignationSyntax singleVariableDesignationSyntax = (SingleVariableDesignationSyntax)declarationExpression.Designation;
		TypeSyntax type = declarationExpression.Type;
		SourceLocalSymbol sourceLocalSymbol = LookupLocal(singleVariableDesignationSyntax.Identifier);
		bool isVar;
		if ((object)sourceLocalSymbol != null)
		{
			if ((InConstructorInitializer || InFieldInitializer) && ContainingMemberOrLambda.ContainingSymbol.Kind == SymbolKind.NamedType)
			{
				CheckFeatureAvailability(declarationExpression, MessageID.IDS_FeatureExpressionVariablesInQueriesAndInitializers, diagnostics);
			}
			bool isConst = false;
			TypeSymbol type2 = BindVariableType(declarationExpression, diagnostics, type, ref isConst, out isVar, out var _);
			sourceLocalSymbol.ScopeBinder.ValidateDeclarationNameConflictsInScope(sourceLocalSymbol, diagnostics);
			if (isVar)
			{
				return new OutVariablePendingInference(declarationExpression, sourceLocalSymbol, null);
			}
			CheckRestrictedTypeInAsync(ContainingMemberOrLambda, type2, diagnostics, type);
			return new BoundLocal(declarationExpression, sourceLocalSymbol, isDeclaration: true, null, type2);
		}
		GlobalExpressionVariable globalExpressionVariable = LookupDeclaredField(singleVariableDesignationSyntax);
		if ((object)globalExpressionVariable == null)
		{
			throw ExceptionUtilities.Unreachable;
		}
		BoundExpression boundExpression = SynthesizeReceiver(singleVariableDesignationSyntax, globalExpressionVariable, diagnostics);
		if (type.IsVar)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BindTypeOrAliasOrVarKeyword(type, instance, out isVar);
			instance.Free();
			if (isVar)
			{
				return new OutVariablePendingInference(declarationExpression, globalExpressionVariable, boundExpression);
			}
		}
		TypeSymbol fieldType = globalExpressionVariable.GetFieldType(FieldsBeingBound);
		return new BoundFieldAccess(declarationExpression, boundExpression, globalExpressionVariable, null, LookupResultKind.Viable, isDeclaration: true, fieldType);
	}

	internal static bool CheckRestrictedTypeInAsync(Symbol containingSymbol, TypeSymbol type, DiagnosticBag diagnostics, SyntaxNode syntax)
	{
		if (containingSymbol.Kind == SymbolKind.Method && ((MethodSymbol)containingSymbol).IsAsync && type.IsRestrictedType())
		{
			Error(diagnostics, ErrorCode.ERR_BadSpecialByRefLocal, syntax, type);
			return true;
		}
		return false;
	}

	internal GlobalExpressionVariable LookupDeclaredField(SingleVariableDesignationSyntax variableDesignator)
	{
		return LookupDeclaredField(variableDesignator, variableDesignator.Identifier.ValueText);
	}

	internal GlobalExpressionVariable LookupDeclaredField(SyntaxNode node, string identifier)
	{
		foreach (Symbol item in ContainingType?.GetMembers(identifier) ?? ImmutableArray<Symbol>.Empty)
		{
			GlobalExpressionVariable globalExpressionVariable;
			if (item.Kind == SymbolKind.Field && (globalExpressionVariable = item as GlobalExpressionVariable)?.SyntaxTree == node.SyntaxTree && globalExpressionVariable.SyntaxNode == node)
			{
				return globalExpressionVariable;
			}
		}
		return null;
	}

	private void BindArgumentAndName(AnalyzedArguments result, DiagnosticBag diagnostics, ref bool hadLangVersionError, CSharpSyntaxNode argumentSyntax, BoundExpression boundArgumentExpression, NameColonSyntax nameColonSyntax, RefKind refKind)
	{
		bool flag = result.RefKinds.Any();
		if (refKind != RefKind.None && !flag)
		{
			flag = true;
			int count = result.Arguments.Count;
			for (int i = 0; i < count; i++)
			{
				result.RefKinds.Add(RefKind.None);
			}
		}
		if (flag)
		{
			result.RefKinds.Add(refKind);
		}
		bool flag2 = result.Names.Any();
		if (nameColonSyntax != null)
		{
			if (!flag2)
			{
				int count2 = result.Arguments.Count;
				for (int j = 0; j < count2; j++)
				{
					result.Names.Add(null);
				}
			}
			result.Names.Add(nameColonSyntax.Name);
		}
		else if (flag2)
		{
			if (!hadLangVersionError && !Compilation.LanguageVersion.AllowNonTrailingNamedArguments())
			{
				Error(diagnostics, ErrorCode.ERR_NamedArgumentSpecificationBeforeFixedArgument, argumentSyntax, new CSharpRequiredLanguageVersion(MessageID.IDS_FeatureNonTrailingNamedArguments.RequiredVersion()));
				hadLangVersionError = true;
			}
			result.Names.Add(null);
		}
		result.Arguments.Add(boundArgumentExpression);
	}

	private BoundExpression BindArgumentExpression(DiagnosticBag diagnostics, ExpressionSyntax argumentExpression, RefKind refKind, bool allowArglist)
	{
		BindValueKind valueKind = refKind switch
		{
			RefKind.In => BindValueKind.ReadonlyRef, 
			RefKind.None => BindValueKind.RValue, 
			_ => BindValueKind.RefOrOut, 
		};
		if (allowArglist)
		{
			return BindValueAllowArgList(argumentExpression, diagnostics, valueKind);
		}
		return BindValue(argumentExpression, diagnostics, valueKind);
	}

	private void CoerceArguments<TMember>(MemberResolutionResult<TMember> methodResult, ArrayBuilder<BoundExpression> arguments, DiagnosticBag diagnostics) where TMember : Symbol
	{
		MemberAnalysisResult result = methodResult.Result;
		ImmutableArray<ParameterSymbol> parameters = methodResult.LeastOverriddenMember.GetParameters();
		for (int i = 0; i < arguments.Count; i++)
		{
			Conversion conversion = result.ConversionForArg(i);
			BoundExpression boundExpression = arguments[i];
			if (!conversion.IsIdentity || boundExpression.Kind == BoundKind.TupleLiteral)
			{
				TypeSymbol correspondingParameterType = GetCorrespondingParameterType(ref result, parameters, i);
				if (!methodResult.Member.IsIndexer() && !boundExpression.HasAnyErrors && correspondingParameterType.IsUnsafe())
				{
					ReportUnsafeIfNotAllowed(boundExpression.Syntax, diagnostics);
				}
				arguments[i] = CreateConversion(boundExpression.Syntax, boundExpression, conversion, isCast: false, correspondingParameterType, diagnostics);
			}
			else if (boundExpression.Kind == BoundKind.OutVariablePendingInference)
			{
				TypeSymbol correspondingParameterType2 = GetCorrespondingParameterType(ref result, parameters, i);
				arguments[i] = ((OutVariablePendingInference)boundExpression).SetInferredType(correspondingParameterType2, diagnostics);
			}
			else if (boundExpression.Kind == BoundKind.OutDeconstructVarPendingInference)
			{
				TypeSymbol correspondingParameterType3 = GetCorrespondingParameterType(ref result, parameters, i);
				arguments[i] = ((OutDeconstructVarPendingInference)boundExpression).SetInferredType(correspondingParameterType3, this, success: true);
			}
			else if (boundExpression.Kind == BoundKind.DiscardExpression && !boundExpression.HasExpressionType())
			{
				TypeSymbol correspondingParameterType4 = GetCorrespondingParameterType(ref result, parameters, i);
				arguments[i] = ((BoundDiscardExpression)boundExpression).SetInferredType(correspondingParameterType4);
			}
		}
	}

	private static TypeSymbol GetCorrespondingParameterType(ref MemberAnalysisResult result, ImmutableArray<ParameterSymbol> parameters, int arg)
	{
		int num = result.ParameterFromArgument(arg);
		if (num != parameters.Length - 1 || result.Kind != MemberResolutionKind.ApplicableInExpandedForm)
		{
			return parameters[num].Type;
		}
		return ((ArrayTypeSymbol)parameters[num].Type).ElementType;
	}

	private BoundExpression BindArrayCreationExpression(ArrayCreationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		ArrayTypeSymbol type = (ArrayTypeSymbol)BindType(node.Type, diagnostics);
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (ExpressionSyntax size in node.Type.RankSpecifiers[0].Sizes)
		{
			if (size.Kind() == SyntaxKind.OmittedArraySizeExpression)
			{
				continue;
			}
			BoundExpression boundExpression = BindValue(size, diagnostics, BindValueKind.RValue);
			if (!boundExpression.HasAnyErrors)
			{
				boundExpression = ConvertToArrayIndex(boundExpression, node, diagnostics);
				if (IsNegativeConstantForArraySize(boundExpression))
				{
					Error(diagnostics, ErrorCode.ERR_NegativeArraySize, size);
				}
			}
			instance.Add(boundExpression);
		}
		ImmutableArray<BoundExpression> immutableArray = instance.ToImmutableAndFree();
		if (node.Initializer != null)
		{
			return BindArrayCreationWithInitializer(diagnostics, node, node.Initializer, type, immutableArray);
		}
		return new BoundArrayCreation(node, immutableArray, null, type);
	}

	private BoundExpression BindImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		InitializerExpressionSyntax initializer = node.Initializer;
		int rank = node.Commas.Count + 1;
		ImmutableArray<BoundExpression> immutableArray = BindArrayInitializerExpressions(initializer, diagnostics, 1, rank);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		TypeSymbol typeSymbol = BestTypeInferrer.InferBestType(immutableArray, Conversions, out var _, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		if ((object)typeSymbol == null || typeSymbol.SpecialType == SpecialType.System_Void)
		{
			Error(diagnostics, ErrorCode.ERR_ImplicitlyTypedArrayNoBestType, node);
			typeSymbol = CreateErrorType();
		}
		if (typeSymbol.IsRestrictedType())
		{
			Error(diagnostics, ErrorCode.ERR_ArrayElementCantBeRefAny, node, typeSymbol);
		}
		ArrayTypeSymbol type = ArrayTypeSymbol.CreateCSharpArray(Compilation.Assembly, typeSymbol, ImmutableArray<CustomModifier>.Empty, rank);
		return BindArrayCreationWithInitializer(diagnostics, node, initializer, type, ImmutableArray<BoundExpression>.Empty, immutableArray);
	}

	private BoundExpression BindImplicitStackAllocArrayCreationExpression(ImplicitStackAllocArrayCreationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		InitializerExpressionSyntax initializer = node.Initializer;
		ImmutableArray<BoundExpression> immutableArray = BindArrayInitializerExpressions(initializer, diagnostics, 1, 1);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		TypeSymbol typeSymbol = BestTypeInferrer.InferBestType(immutableArray, Conversions, out var _, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		if ((object)typeSymbol == null || typeSymbol.SpecialType == SpecialType.System_Void)
		{
			Error(diagnostics, ErrorCode.ERR_ImplicitlyTypedArrayNoBestType, node);
			typeSymbol = CreateErrorType();
		}
		if (!typeSymbol.IsErrorType() && typeSymbol.IsManagedType)
		{
			Error(diagnostics, ErrorCode.ERR_ManagedAddr, node, typeSymbol);
		}
		bool hasErrors;
		return BindStackAllocWithInitializer(node, initializer, GetStackAllocType(node, typeSymbol, diagnostics, out hasErrors), typeSymbol, null, diagnostics, hasErrors, immutableArray);
	}

	private ImmutableArray<BoundExpression> BindArrayInitializerExpressions(InitializerExpressionSyntax initializer, DiagnosticBag diagnostics, int dimension, int rank)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		BindArrayInitializerExpressions(initializer, instance, diagnostics, dimension, rank);
		return instance.ToImmutableAndFree();
	}

	private void BindArrayInitializerExpressions(InitializerExpressionSyntax initializer, ArrayBuilder<BoundExpression> exprBuilder, DiagnosticBag diagnostics, int dimension, int rank)
	{
		if (dimension == rank)
		{
			foreach (ExpressionSyntax expression in initializer.Expressions)
			{
				BoundExpression item = BindValue(expression, diagnostics, BindValueKind.RValue);
				exprBuilder.Add(item);
			}
			return;
		}
		foreach (ExpressionSyntax expression2 in initializer.Expressions)
		{
			if (expression2.Kind() == SyntaxKind.ArrayInitializerExpression)
			{
				BindArrayInitializerExpressions((InitializerExpressionSyntax)expression2, exprBuilder, diagnostics, dimension + 1, rank);
				continue;
			}
			BoundExpression boundExpression = BindValue(expression2, diagnostics, BindValueKind.RValue);
			if ((object)boundExpression.Type == null || !boundExpression.Type.IsErrorType())
			{
				if (!boundExpression.HasAnyErrors)
				{
					Error(diagnostics, ErrorCode.ERR_ArrayInitializerExpected, expression2);
				}
				boundExpression = BadExpression(expression2, LookupResultKind.Empty, ImmutableArray.Create(boundExpression.ExpressionSymbol), ImmutableArray.Create(boundExpression));
			}
			exprBuilder.Add(boundExpression);
		}
	}

	private BoundArrayInitialization ConvertAndBindArrayInitialization(DiagnosticBag diagnostics, InitializerExpressionSyntax node, ArrayTypeSymbol type, int?[] knownSizes, int dimension, ImmutableArray<BoundExpression> boundInitExpr, ref int boundInitExprIndex)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		if (dimension == type.Rank)
		{
			TypeSymbol elementType = type.ElementType;
			foreach (ExpressionSyntax expression2 in node.Expressions)
			{
				_ = expression2;
				BoundExpression expression = boundInitExpr[boundInitExprIndex];
				boundInitExprIndex++;
				BoundExpression item = GenerateConversionForAssignment(elementType, expression, diagnostics);
				instance.Add(item);
			}
		}
		else
		{
			foreach (ExpressionSyntax expression3 in node.Expressions)
			{
				BoundExpression item2;
				if (expression3.Kind() == SyntaxKind.ArrayInitializerExpression)
				{
					item2 = ConvertAndBindArrayInitialization(diagnostics, (InitializerExpressionSyntax)expression3, type, knownSizes, dimension + 1, boundInitExpr, ref boundInitExprIndex);
				}
				else
				{
					item2 = boundInitExpr[boundInitExprIndex];
					boundInitExprIndex++;
				}
				instance.Add(item2);
			}
		}
		bool hasErrors = false;
		int? num = knownSizes[dimension - 1];
		if (!num.HasValue)
		{
			knownSizes[dimension - 1] = instance.Count;
		}
		else if (num != instance.Count && num >= 0)
		{
			Error(diagnostics, ErrorCode.ERR_ArrayInitializerIncorrectLength, node, num.Value);
			hasErrors = true;
		}
		return new BoundArrayInitialization(node, instance.ToImmutableAndFree(), hasErrors);
	}

	private BoundArrayInitialization BindArrayInitializerList(DiagnosticBag diagnostics, InitializerExpressionSyntax node, ArrayTypeSymbol type, int?[] knownSizes, int dimension, ImmutableArray<BoundExpression> boundInitExprOpt = default(ImmutableArray<BoundExpression>))
	{
		if (boundInitExprOpt.IsDefault)
		{
			boundInitExprOpt = BindArrayInitializerExpressions(node, diagnostics, dimension, type.Rank);
		}
		int boundInitExprIndex = 0;
		return ConvertAndBindArrayInitialization(diagnostics, node, type, knownSizes, dimension, boundInitExprOpt, ref boundInitExprIndex);
	}

	private BoundArrayInitialization BindUnexpectedArrayInitializer(InitializerExpressionSyntax node, DiagnosticBag diagnostics, ErrorCode errorCode, CSharpSyntaxNode errorNode = null)
	{
		BoundArrayInitialization boundArrayInitialization = BindArrayInitializerList(diagnostics, node, Compilation.CreateArrayTypeSymbol(GetSpecialType(SpecialType.System_Object, diagnostics, node)), new int?[1], 1);
		if (!boundArrayInitialization.HasAnyErrors)
		{
			boundArrayInitialization = new BoundArrayInitialization(node, boundArrayInitialization.Initializers, hasErrors: true);
		}
		Error(diagnostics, errorCode, errorNode ?? node);
		return boundArrayInitialization;
	}

	private BoundArrayCreation BindArrayCreationWithInitializer(DiagnosticBag diagnostics, ExpressionSyntax creationSyntax, InitializerExpressionSyntax initSyntax, ArrayTypeSymbol type, ImmutableArray<BoundExpression> sizes, ImmutableArray<BoundExpression> boundInitExprOpt = default(ImmutableArray<BoundExpression>))
	{
		bool flag = false;
		int rank = type.Rank;
		int length = sizes.Length;
		int?[] array = new int?[Math.Max(rank, length)];
		for (int i = 0; i < length; i++)
		{
			BoundExpression boundExpression = sizes[i];
			array[i] = GetIntegerConstantForArraySize(boundExpression);
			if (!boundExpression.HasAnyErrors && !array[i].HasValue)
			{
				Error(diagnostics, ErrorCode.ERR_ConstantExpected, boundExpression.Syntax);
				flag = true;
			}
		}
		BoundArrayInitialization boundArrayInitialization = BindArrayInitializerList(diagnostics, initSyntax, type, array, 1, boundInitExprOpt);
		flag = flag || boundArrayInitialization.HasAnyErrors;
		bool flag2 = creationSyntax != null;
		CSharpSyntaxNode cSharpSyntaxNode = creationSyntax ?? initSyntax;
		if (length == 0)
		{
			BoundExpression[] array2 = new BoundExpression[rank];
			for (int j = 0; j < rank; j++)
			{
				array2[j] = new BoundLiteral(cSharpSyntaxNode, ConstantValue.Create(array[j] ?? 0), GetSpecialType(SpecialType.System_Int32, diagnostics, cSharpSyntaxNode))
				{
					WasCompilerGenerated = true
				};
			}
			sizes = array2.AsImmutableOrNull();
		}
		else if (!flag && rank != length)
		{
			Error(diagnostics, ErrorCode.ERR_BadIndexCount, cSharpSyntaxNode, type.Rank);
			flag = true;
		}
		return new BoundArrayCreation(cSharpSyntaxNode, sizes, boundArrayInitialization, type, flag)
		{
			WasCompilerGenerated = (!flag2 && (initSyntax.Parent == null || initSyntax.Parent.Kind() != SyntaxKind.EqualsValueClause || ((EqualsValueClauseSyntax)initSyntax.Parent).Value != initSyntax))
		};
	}

	private BoundExpression BindStackAllocArrayCreationExpression(StackAllocArrayCreationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		TypeSyntax type = node.Type;
		if (type.Kind() != SyntaxKind.ArrayType)
		{
			Error(diagnostics, ErrorCode.ERR_BadStackAllocExpr, type);
			return new BoundBadExpression(node, LookupResultKind.NotCreatable, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, new PointerTypeSymbol(BindType(type, diagnostics)));
		}
		ArrayTypeSyntax arrayTypeSyntax = (ArrayTypeSyntax)type;
		TypeSyntax elementType = arrayTypeSyntax.ElementType;
		TypeSymbol typeSymbol = BindType(elementType, diagnostics);
		TypeSymbol stackAllocType = GetStackAllocType(node, typeSymbol, diagnostics, out var hasErrors);
		if (!typeSymbol.IsErrorType() && typeSymbol.IsManagedType)
		{
			Error(diagnostics, ErrorCode.ERR_ManagedAddr, elementType, typeSymbol);
			hasErrors = true;
		}
		SyntaxList<ArrayRankSpecifierSyntax> rankSpecifiers = arrayTypeSyntax.RankSpecifiers;
		if (rankSpecifiers.Count != 1 || rankSpecifiers[0].Sizes.Count != 1)
		{
			Error(diagnostics, ErrorCode.ERR_BadStackAllocExpr, type);
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			DiagnosticBag instance2 = DiagnosticBag.GetInstance();
			foreach (ArrayRankSpecifierSyntax item in rankSpecifiers)
			{
				foreach (ExpressionSyntax size in item.Sizes)
				{
					if (size.Kind() != SyntaxKind.OmittedArraySizeExpression)
					{
						instance.Add(BindExpression(size, instance2));
					}
				}
			}
			instance2.Free();
			return new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, instance.ToImmutableAndFree(), new PointerTypeSymbol(typeSymbol));
		}
		ExpressionSyntax expressionSyntax = rankSpecifiers[0].Sizes[0];
		BoundExpression boundExpression = null;
		if (expressionSyntax.Kind() != SyntaxKind.OmittedArraySizeExpression)
		{
			boundExpression = BindValue(expressionSyntax, diagnostics, BindValueKind.RValue);
			if (!boundExpression.HasAnyErrors)
			{
				boundExpression = GenerateConversionForAssignment(GetSpecialType(SpecialType.System_Int32, diagnostics, node), boundExpression, diagnostics);
				if (!boundExpression.HasAnyErrors && IsNegativeConstantForArraySize(boundExpression))
				{
					Error(diagnostics, ErrorCode.ERR_NegativeStackAllocSize, expressionSyntax);
					hasErrors = true;
				}
			}
		}
		else if (node.Initializer == null)
		{
			boundExpression = BadExpression(expressionSyntax);
			hasErrors = true;
		}
		if (node.Initializer != null)
		{
			return BindStackAllocWithInitializer(node, node.Initializer, stackAllocType, typeSymbol, boundExpression, diagnostics, hasErrors);
		}
		return new BoundStackAllocArrayCreation(node, typeSymbol, boundExpression, null, stackAllocType, hasErrors);
	}

	private bool ReportBadStackAllocPosition(SyntaxNode node, DiagnosticBag diagnostics)
	{
		bool flag = (IsInMethodBody || IsLocalFunctionsScopeBinder) && node.IsLegalSpanStackAllocPosition();
		if (!flag)
		{
			diagnostics.Add(ErrorCode.ERR_InvalidExprTerm, node.GetFirstToken().GetLocation(), SyntaxFacts.GetText(SyntaxKind.StackAllocKeyword));
		}
		if (Flags.IncludesAny(BinderFlags.InCatchBlock | BinderFlags.InFinallyBlock | BinderFlags.InCatchFilter))
		{
			Error(diagnostics, ErrorCode.ERR_StackallocInCatchFinally, node);
		}
		return flag;
	}

	private TypeSymbol GetStackAllocType(SyntaxNode node, TypeSymbol elementType, DiagnosticBag diagnostics, out bool hasErrors)
	{
		bool flag = ReportBadStackAllocPosition(node, diagnostics);
		hasErrors = !flag;
		if (flag && !node.IsVariableDeclarationInitialization())
		{
			CheckFeatureAvailability(node, MessageID.IDS_FeatureRefStructs, diagnostics);
			NamedTypeSymbol wellKnownType = GetWellKnownType(WellKnownType.System_Span_T, diagnostics, node);
			if (!wellKnownType.IsErrorType())
			{
				return ConstructNamedType(wellKnownType, (node.Kind() == SyntaxKind.StackAllocArrayCreationExpression) ? ((StackAllocArrayCreationExpressionSyntax)node).Type : node, default, ImmutableArray.Create(elementType), null, diagnostics);
			}
		}
		return null;
	}

	private BoundExpression BindStackAllocWithInitializer(SyntaxNode node, InitializerExpressionSyntax initSyntax, TypeSymbol type, TypeSymbol elementType, BoundExpression sizeOpt, DiagnosticBag diagnostics, bool hasErrors, ImmutableArray<BoundExpression> boundInitExprOpt = default(ImmutableArray<BoundExpression>))
	{
		if (boundInitExprOpt.IsDefault)
		{
			boundInitExprOpt = BindArrayInitializerExpressions(initSyntax, diagnostics, 1, 1);
		}
		boundInitExprOpt = boundInitExprOpt.SelectAsArray((BoundExpression expr, (TypeSymbol elementType, DiagnosticBag diagnostics) t) => GenerateConversionForAssignment(t.elementType, expr, t.diagnostics), (elementType, diagnostics));
		if (sizeOpt != null)
		{
			if (!sizeOpt.HasAnyErrors)
			{
				int? integerConstantForArraySize = GetIntegerConstantForArraySize(sizeOpt);
				if (!integerConstantForArraySize.HasValue)
				{
					Error(diagnostics, ErrorCode.ERR_ConstantExpected, sizeOpt.Syntax);
					hasErrors = true;
				}
				else if (boundInitExprOpt.Length != integerConstantForArraySize)
				{
					Error(diagnostics, ErrorCode.ERR_ArrayInitializerIncorrectLength, node, integerConstantForArraySize.Value);
					hasErrors = true;
				}
			}
		}
		else
		{
			sizeOpt = new BoundLiteral(node, ConstantValue.Create(boundInitExprOpt.Length), GetSpecialType(SpecialType.System_Int32, diagnostics, node))
			{
				WasCompilerGenerated = true
			};
		}
		return new BoundStackAllocArrayCreation(node, elementType, sizeOpt, new BoundArrayInitialization(initSyntax, boundInitExprOpt), type, hasErrors);
	}

	private static int? GetIntegerConstantForArraySize(BoundExpression expression)
	{
		if (expression.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = expression.ConstantValue;
		if (constantValue == null || constantValue.IsBad || expression.Type.SpecialType != SpecialType.System_Int32)
		{
			return null;
		}
		return constantValue.Int32Value;
	}

	private static bool IsNegativeConstantForArraySize(BoundExpression expression)
	{
		if (expression.HasAnyErrors)
		{
			return false;
		}
		ConstantValue constantValue = expression.ConstantValue;
		if (constantValue == null || constantValue.IsBad)
		{
			return false;
		}
		return expression.Type.SpecialType switch
		{
			SpecialType.System_Int32 => constantValue.Int32Value < 0, 
			SpecialType.System_Int64 => constantValue.Int64Value < 0, 
			_ => false, 
		};
	}

	internal BoundExpression BindConstructorInitializer(ArgumentListSyntax initializerArgumentListOpt, MethodSymbol constructor, DiagnosticBag diagnostics)
	{
		Binder binder = null;
		if (initializerArgumentListOpt != null)
		{
			binder = GetBinder(initializerArgumentListOpt);
		}
		BoundExpression boundExpression = (binder ?? this).BindConstructorInitializerCore(initializerArgumentListOpt, constructor, diagnostics);
		if (binder != null)
		{
			boundExpression = binder.WrapWithVariablesIfAny(initializerArgumentListOpt, boundExpression);
		}
		return boundExpression;
	}

	private BoundExpression BindConstructorInitializerCore(ArgumentListSyntax initializerArgumentListOpt, MethodSymbol constructor, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol containingType = constructor.ContainingType;
		if ((containingType.TypeKind == TypeKind.Enum || containingType.TypeKind == TypeKind.Struct) && initializerArgumentListOpt == null)
		{
			return null;
		}
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		try
		{
			TypeSymbol returnType = constructor.ReturnType;
			if (initializerArgumentListOpt != null)
			{
				BindArgumentsAndNames(initializerArgumentListOpt, diagnostics, instance, allowArglist: true);
			}
			NamedTypeSymbol namedTypeSymbol = containingType;
			bool flag = initializerArgumentListOpt == null || initializerArgumentListOpt.Parent.Kind() == SyntaxKind.BaseConstructorInitializer;
			if (flag)
			{
				namedTypeSymbol = namedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
				if ((object)namedTypeSymbol == null || containingType.SpecialType == SpecialType.System_Object)
				{
					if (initializerArgumentListOpt == null)
					{
						return null;
					}
					diagnostics.Add(ErrorCode.ERR_ObjectCallingBaseConstructor, constructor.Locations[0], containingType);
					return new BoundBadExpression(initializerArgumentListOpt.Parent, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, BuildArgumentsForErrorRecovery(instance), returnType);
				}
				if (initializerArgumentListOpt != null && containingType.TypeKind == TypeKind.Struct)
				{
					diagnostics.Add(ErrorCode.ERR_StructWithBaseConstructorCall, constructor.Locations[0], containingType);
					return new BoundBadExpression(initializerArgumentListOpt.Parent, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, BuildArgumentsForErrorRecovery(instance), returnType);
				}
			}
			if (initializerArgumentListOpt != null && instance.HasDynamicArgument)
			{
				diagnostics.Add(ErrorCode.ERR_NoDynamicPhantomOnBaseCtor, ((ConstructorInitializerSyntax)initializerArgumentListOpt.Parent).ThisOrBaseKeyword.GetLocation());
				return new BoundBadExpression(initializerArgumentListOpt.Parent, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, BuildArgumentsForErrorRecovery(instance), returnType);
			}
			CSharpSyntaxNode cSharpSyntaxNode;
			Location location;
			if (initializerArgumentListOpt != null)
			{
				cSharpSyntaxNode = initializerArgumentListOpt.Parent;
				location = ((ConstructorInitializerSyntax)cSharpSyntaxNode).ThisOrBaseKeyword.GetLocation();
			}
			else
			{
				cSharpSyntaxNode = constructor.GetNonNullSyntaxNode();
				location = constructor.Locations[0];
			}
			BoundExpression boundExpression = ThisReference(cSharpSyntaxNode, namedTypeSymbol, hasErrors: false, wasCompilerGenerated: true);
			if (TryPerformConstructorOverloadResolution(namedTypeSymbol, instance, ".ctor", location, suppressResultDiagnostics: false, diagnostics, out var memberResolutionResult, out var candidateConstructors, allowProtectedConstructorsOfBaseType: true))
			{
				bool flag2 = false;
				MethodSymbol member = memberResolutionResult.Member;
				if (member == constructor)
				{
					diagnostics.Add(ErrorCode.ERR_RecursiveConstructorCall, ((ConstructorInitializerSyntax)initializerArgumentListOpt.Parent).ThisOrBaseKeyword.GetLocation(), constructor);
					flag2 = true;
				}
				else if (member.HasUnsafeParameter())
				{
					flag2 = ReportUnsafeIfNotAllowed(location, diagnostics);
				}
				ReportDiagnosticsIfObsolete(diagnostics, member, cSharpSyntaxNode, flag);
				ImmutableArray<BoundExpression> immutableArray = instance.Arguments.ToImmutable();
				ImmutableArray<RefKind> argumentRefKindsOpt = instance.RefKinds.ToImmutableOrNull();
				ImmutableArray<int> argsToParamsOpt = memberResolutionResult.Result.ArgsToParamsOpt;
				if (!flag2)
				{
					flag2 = !CheckInvocationArgMixing(cSharpSyntaxNode, member, boundExpression, member.Parameters, immutableArray, argsToParamsOpt, LocalScopeDepth, diagnostics);
				}
				return new BoundCall(cSharpSyntaxNode, boundExpression, member, immutableArray, instance.GetNames(), argumentRefKindsOpt, isDelegateCall: false, memberResolutionResult.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm, invokedAsExtensionMethod: false, argsToParamsOpt, LookupResultKind.Viable, this, returnType, flag2)
				{
					WasCompilerGenerated = (initializerArgumentListOpt == null)
				};
			}
			BoundCall boundCall = CreateBadCall(cSharpSyntaxNode, ".ctor", boundExpression, candidateConstructors, LookupResultKind.OverloadResolutionFailure, ImmutableArray<TypeSymbol>.Empty, instance, invokedAsExtensionMethod: false, isDelegate: false);
			boundCall.WasCompilerGenerated = initializerArgumentListOpt == null;
			return boundCall;
		}
		finally
		{
			instance.Free();
		}
	}

	protected BoundExpression BindObjectCreationExpression(ObjectCreationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = BindType(node.Type, diagnostics);
		BoundObjectInitializerExpressionBase boundInitializerOpt = ((node.Initializer == null) ? null : BindInitializerExpression(node.Initializer, typeSymbol, node.Type, diagnostics));
		switch (typeSymbol.TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Enum:
		case TypeKind.Error:
		case TypeKind.Struct:
			return BindClassCreationExpression(node, (NamedTypeSymbol)typeSymbol, GetName(node.Type), boundInitializerOpt, diagnostics);
		case TypeKind.Delegate:
			return BindDelegateCreationExpression(node, (NamedTypeSymbol)typeSymbol, diagnostics);
		case TypeKind.Interface:
			return BindInterfaceCreationExpression(node, (NamedTypeSymbol)typeSymbol, boundInitializerOpt, diagnostics);
		case TypeKind.TypeParameter:
			return BindTypeParameterCreationExpression(node, (TypeParameterSymbol)typeSymbol, boundInitializerOpt, diagnostics);
		case TypeKind.Submission:
			throw ExceptionUtilities.UnexpectedValue(typeSymbol.TypeKind);
		case TypeKind.Pointer:
			typeSymbol = new ExtendedErrorTypeSymbol(typeSymbol, LookupResultKind.NotCreatable, diagnostics.Add(ErrorCode.ERR_UnsafeTypeInObjectCreation, node.Location, typeSymbol));
			goto case TypeKind.Class;
		case TypeKind.Array:
		case TypeKind.Dynamic:
			typeSymbol = new ExtendedErrorTypeSymbol(typeSymbol, LookupResultKind.NotCreatable, diagnostics.Add(ErrorCode.ERR_InvalidObjectCreation, node.Type.Location, typeSymbol));
			goto case TypeKind.Class;
		default:
			throw ExceptionUtilities.UnexpectedValue(typeSymbol.TypeKind);
		}
	}

	private BoundExpression BindDelegateCreationExpression(ObjectCreationExpressionSyntax node, NamedTypeSymbol type, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		try
		{
			BindArgumentsAndNames(node.ArgumentList, diagnostics, instance, allowArglist: false, isDelegateCreation: true);
			bool flag = false;
			if (!instance.HasErrors)
			{
				if (node.ArgumentList == null || instance.Arguments.Count == 0)
				{
					diagnostics.Add(ErrorCode.ERR_BadCtorArgCount, node.Location, type, 0);
					flag = true;
				}
				else if (instance.Names.Count != 0 || instance.RefKinds.Count != 0 || instance.Arguments.Count != 1)
				{
					SyntaxNode syntax = instance.Arguments[0].Syntax;
					int spanStart = syntax.SpanStart;
					int end = instance.Arguments[instance.Arguments.Count - 1].Syntax.Span.End;
					SourceLocation location = new SourceLocation(span: new TextSpan(spanStart, end - spanStart), syntaxTree: syntax.SyntaxTree);
					diagnostics.Add(ErrorCode.ERR_MethodNameExpected, location);
					flag = true;
				}
			}
			if (node.Initializer != null)
			{
				Error(diagnostics, ErrorCode.ERR_ObjectOrCollectionInitializerWithDelegateCreation, node);
				flag = true;
			}
			BoundExpression boundExpression = ((instance.Arguments.Count >= 1) ? instance.Arguments[0] : null);
			if (!flag)
			{
				if (boundExpression is UnboundLambda)
				{
					UnboundLambda unboundLambda = (UnboundLambda)boundExpression;
					HashSet<DiagnosticInfo> useSiteDiagnostics = null;
					Conversion conversion = Conversions.ClassifyConversionFromExpression(unboundLambda, type, ref useSiteDiagnostics);
					diagnostics.Add(node, useSiteDiagnostics);
					BoundLambda boundLambda = unboundLambda.Bind(type);
					if (!conversion.IsImplicit || !conversion.IsValid)
					{
						GenerateImplicitConversionError(diagnostics, unboundLambda.Syntax, conversion, unboundLambda, type);
					}
					else
					{
						diagnostics.AddRange(boundLambda.Diagnostics);
					}
					return new BoundDelegateCreationExpression(node, boundLambda, null, isExtensionMethod: false, type, !conversion.IsImplicit);
				}
				if (!instance.HasErrors)
				{
					if (boundExpression.Kind == BoundKind.MethodGroup)
					{
						BoundMethodGroup boundMethodGroup = (BoundMethodGroup)boundExpression;
						flag = MethodGroupConversionDoesNotExistOrHasErrors(boundMethodGroup, type, node.Location, diagnostics, out var conversion2);
						boundMethodGroup = FixMethodGroupWithTypeOrValue(boundMethodGroup, conversion2, diagnostics);
						return new BoundDelegateCreationExpression(node, boundMethodGroup, conversion2.Method, conversion2.IsExtensionMethod, type, flag);
					}
					if ((object)boundExpression.Type == null)
					{
						diagnostics.Add(ErrorCode.ERR_MethodNameExpected, boundExpression.Syntax.Location);
					}
					else
					{
						if (boundExpression.HasDynamicType())
						{
							return new BoundDelegateCreationExpression(node, boundExpression, null, isExtensionMethod: false, type);
						}
						if (boundExpression.Type.TypeKind == TypeKind.Delegate)
						{
							NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)boundExpression.Type;
							MethodGroup instance2 = MethodGroup.GetInstance();
							try
							{
								if (ReportDelegateInvokeUseSiteDiagnostic(diagnostics, boundExpression.Type, null, node))
								{
									return new BoundBadExpression(node, LookupResultKind.NotInvocable, StaticCast<Symbol>.From(type.InstanceConstructors), ImmutableArray.Create(boundExpression), type);
								}
								instance2.PopulateWithSingleMethod(boundExpression, namedTypeSymbol.DelegateInvokeMethod);
								HashSet<DiagnosticInfo> useSiteDiagnostics2 = null;
								Conversion conversion3 = Conversions.MethodGroupConversion(boundExpression.Syntax, instance2, type, ref useSiteDiagnostics2);
								diagnostics.Add(node, useSiteDiagnostics2);
								if (!conversion3.Exists)
								{
									BoundMethodGroup expr = new BoundMethodGroup(boundExpression.Syntax, default, "Invoke", ImmutableArray.Create(namedTypeSymbol.DelegateInvokeMethod), namedTypeSymbol.DelegateInvokeMethod, null, BoundMethodGroupFlags.None, boundExpression, LookupResultKind.Viable);
									if (!Conversions.ReportDelegateMethodGroupDiagnostics(this, expr, type, diagnostics))
									{
										diagnostics.Add(ErrorCode.ERR_MethDelegateMismatch, node.Location, namedTypeSymbol.DelegateInvokeMethod, type);
									}
								}
								else if (!MethodGroupConversionHasErrors(boundExpression.Syntax, conversion3, boundExpression, conversion3.IsExtensionMethod, type, diagnostics))
								{
									return new BoundDelegateCreationExpression(node, boundExpression, null, isExtensionMethod: false, type);
								}
							}
							finally
							{
								instance2.Free();
							}
						}
						else
						{
							diagnostics.Add(ErrorCode.ERR_MethodNameExpected, boundExpression.Syntax.Location);
						}
					}
				}
			}
			ImmutableArray<BoundExpression> childBoundNodes = BuildArgumentsForErrorRecovery(instance);
			return new BoundBadExpression(node, LookupResultKind.OverloadResolutionFailure, StaticCast<Symbol>.From(type.InstanceConstructors), childBoundNodes, type);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression BindClassCreationExpression(ObjectCreationExpressionSyntax node, NamedTypeSymbol type, string typeName, BoundObjectInitializerExpressionBase boundInitializerOpt, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		try
		{
			BindArgumentsAndNames(node.ArgumentList, diagnostics, instance, allowArglist: true);
			if (type.IsStatic)
			{
				diagnostics.Add(ErrorCode.ERR_InstantiatingStaticClass, node.Location, type);
				return MakeBadExpressionForObjectCreation(node, type, boundInitializerOpt, instance);
			}
			if (node.Type.Kind() == SyntaxKind.TupleType)
			{
				diagnostics.Add(ErrorCode.ERR_NewWithTupleTypeSyntax, node.Type.GetLocation());
				return MakeBadExpressionForObjectCreation(node, type, boundInitializerOpt, instance);
			}
			return BindClassCreationExpression(node, typeName, node.Type, type, instance, diagnostics, boundInitializerOpt);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression MakeBadExpressionForObjectCreation(ObjectCreationExpressionSyntax node, TypeSymbol type, BoundExpression boundInitializerOpt, AnalyzedArguments analyzedArguments)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		instance.AddRange(BuildArgumentsForErrorRecovery(analyzedArguments));
		if (boundInitializerOpt != null)
		{
			instance.Add(boundInitializerOpt);
		}
		return new BoundBadExpression(node, LookupResultKind.NotCreatable, ImmutableArray.Create((Symbol)type), instance.ToImmutableAndFree(), type);
	}

	private BoundObjectInitializerExpressionBase BindInitializerExpression(InitializerExpressionSyntax syntax, TypeSymbol type, SyntaxNode typeSyntax, DiagnosticBag diagnostics)
	{
		switch (syntax.Kind())
		{
		case SyntaxKind.ObjectInitializerExpression:
		{
			BoundImplicitReceiver implicitReceiver2 = new BoundImplicitReceiver(typeSyntax, type)
			{
				WasCompilerGenerated = true
			};
			return BindObjectInitializerExpression(syntax, type, diagnostics, implicitReceiver2);
		}
		case SyntaxKind.CollectionInitializerExpression:
		{
			BoundImplicitReceiver implicitReceiver = new BoundImplicitReceiver(typeSyntax, type)
			{
				WasCompilerGenerated = true
			};
			return BindCollectionInitializerExpression(syntax, type, diagnostics, implicitReceiver);
		}
		default:
			throw ExceptionUtilities.Unreachable;
		}
	}

	private BoundExpression BindInitializerExpressionOrValue(ExpressionSyntax syntax, TypeSymbol type, SyntaxNode typeSyntax, DiagnosticBag diagnostics)
	{
		SyntaxKind syntaxKind = syntax.Kind();
		if (syntaxKind - 8644 <= SyntaxKind.List)
		{
			return BindInitializerExpression((InitializerExpressionSyntax)syntax, type, typeSyntax, diagnostics);
		}
		return BindValue(syntax, diagnostics, BindValueKind.RValue);
	}

	private BoundObjectInitializerExpression BindObjectInitializerExpression(InitializerExpressionSyntax initializerSyntax, TypeSymbol initializerType, DiagnosticBag diagnostics, BoundImplicitReceiver implicitReceiver)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		HashSet<string> memberNameMap = new HashSet<string>();
		Binder objectInitializerMemberBinder = WithAdditionalFlags(BinderFlags.ObjectInitializerMember);
		foreach (ExpressionSyntax expression in initializerSyntax.Expressions)
		{
			BoundExpression boundExpression = BindObjectInitializerMemberAssignment(expression, initializerType, objectInitializerMemberBinder, diagnostics, implicitReceiver);
			instance.Add(boundExpression);
			ReportDuplicateObjectMemberInitializers(boundExpression, memberNameMap, diagnostics);
		}
		return new BoundObjectInitializerExpression(initializerSyntax, instance.ToImmutableAndFree(), initializerType);
	}

	private BoundExpression BindObjectInitializerMemberAssignment(ExpressionSyntax memberInitializer, TypeSymbol initializerType, Binder objectInitializerMemberBinder, DiagnosticBag diagnostics, BoundImplicitReceiver implicitReceiver)
	{
		if (memberInitializer.Kind() == SyntaxKind.SimpleAssignmentExpression)
		{
			AssignmentExpressionSyntax assignmentExpressionSyntax = (AssignmentExpressionSyntax)memberInitializer;
			ExpressionSyntax left = assignmentExpressionSyntax.Left;
			BoundExpression boundExpression;
			if (initializerType.IsDynamic() && left.Kind() == SyntaxKind.IdentifierName)
			{
				string text = ((IdentifierNameSyntax)left).Identifier.Text;
				boundExpression = new BoundDynamicObjectInitializerMember(left, text, implicitReceiver.Type, initializerType, hasErrors: false);
			}
			else
			{
				boundExpression = objectInitializerMemberBinder.BindObjectInitializerMember(assignmentExpressionSyntax, implicitReceiver, diagnostics);
			}
			if (boundExpression != null)
			{
				BoundExpression op = BindInitializerExpressionOrValue(assignmentExpressionSyntax.Right, boundExpression.Type, boundExpression.Syntax, diagnostics);
				return BindAssignment(assignmentExpressionSyntax, boundExpression, op, isRef: false, diagnostics);
			}
		}
		BoundExpression expr = BindValue(memberInitializer, diagnostics, BindValueKind.RValue);
		Error(diagnostics, ErrorCode.ERR_InvalidInitializerElementInitializer, memberInitializer);
		return ToBadExpression(expr, LookupResultKind.NotAValue);
	}

	private BoundExpression BindObjectInitializerMember(AssignmentExpressionSyntax namedAssignment, BoundImplicitReceiver implicitReceiver, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression;
		LookupResultKind resultKind;
		bool flag;
		if (namedAssignment.Left.Kind() == SyntaxKind.IdentifierName)
		{
			IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)namedAssignment.Left;
			boundExpression = BindInstanceMemberAccess(identifierNameSyntax, identifierNameSyntax, implicitReceiver, identifierNameSyntax.Identifier.ValueText, 0, default, default, invoked: false, indexed: false, diagnostics);
			resultKind = boundExpression.ResultKind;
			flag = boundExpression.HasAnyErrors || implicitReceiver.HasAnyErrors;
			if (boundExpression.Kind == BoundKind.PropertyGroup)
			{
				boundExpression = BindIndexedPropertyAccess((BoundPropertyGroup)boundExpression, mustHaveAllOptionalParameters: true, diagnostics);
				if (boundExpression.HasAnyErrors)
				{
					flag = true;
				}
			}
		}
		else
		{
			if (namedAssignment.Left.Kind() != SyntaxKind.ImplicitElementAccess)
			{
				return null;
			}
			ImplicitElementAccessSyntax implicitElementAccessSyntax = (ImplicitElementAccessSyntax)namedAssignment.Left;
			boundExpression = BindElementAccess(implicitElementAccessSyntax, implicitReceiver, implicitElementAccessSyntax.ArgumentList, diagnostics);
			resultKind = boundExpression.ResultKind;
			flag = boundExpression.HasAnyErrors || implicitReceiver.HasAnyErrors;
		}
		BoundKind kind = boundExpression.Kind;
		SyntaxKind syntaxKind = namedAssignment.Right.Kind();
		bool flag2 = syntaxKind == SyntaxKind.ObjectInitializerExpression || syntaxKind == SyntaxKind.CollectionInitializerExpression;
		BindValueKind valueKind = (flag2 ? BindValueKind.RValue : BindValueKind.Assignable);
		ImmutableArray<BoundExpression> arguments = ImmutableArray<BoundExpression>.Empty;
		ImmutableArray<string> argumentNamesOpt = default;
		ImmutableArray<int> argsToParamsOpt = default;
		ImmutableArray<RefKind> argumentRefKindsOpt = default;
		bool expanded = false;
		switch (kind)
		{
		case BoundKind.FieldAccess:
		{
			FieldSymbol fieldSymbol = ((BoundFieldAccess)boundExpression).FieldSymbol;
			if (flag2 && fieldSymbol.IsReadOnly && fieldSymbol.Type.IsValueType)
			{
				if (!flag)
				{
					Error(diagnostics, ErrorCode.ERR_ReadonlyValueTypeInObjectInitializer, namedAssignment.Left, fieldSymbol, fieldSymbol.Type);
					flag = true;
				}
				resultKind = LookupResultKind.NotAValue;
			}
			break;
		}
		case BoundKind.PropertyAccess:
			flag |= flag2 && !CheckNestedObjectInitializerPropertySymbol(((BoundPropertyAccess)boundExpression).PropertySymbol, namedAssignment.Left, diagnostics, flag, ref resultKind);
			break;
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)boundExpression;
			flag |= flag2 && !CheckNestedObjectInitializerPropertySymbol(boundIndexerAccess.Indexer, namedAssignment.Left, diagnostics, flag, ref resultKind);
			arguments = boundIndexerAccess.Arguments;
			argumentNamesOpt = boundIndexerAccess.ArgumentNamesOpt;
			argsToParamsOpt = boundIndexerAccess.ArgsToParamsOpt;
			argumentRefKindsOpt = boundIndexerAccess.ArgumentRefKindsOpt;
			expanded = boundIndexerAccess.Expanded;
			break;
		}
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)boundExpression;
			arguments = boundDynamicIndexerAccess.Arguments;
			argumentNamesOpt = boundDynamicIndexerAccess.ArgumentNamesOpt;
			argumentRefKindsOpt = boundDynamicIndexerAccess.ArgumentRefKindsOpt;
			break;
		}
		case BoundKind.PointerElementAccess:
		case BoundKind.ArrayAccess:
			return boundExpression;
		default:
			return BadObjectInitializerMemberAccess(boundExpression, implicitReceiver, namedAssignment.Left, diagnostics, valueKind, flag);
		case BoundKind.EventAccess:
			break;
		}
		if (!flag && !CheckValueKind(boundExpression.Syntax, boundExpression, valueKind, checkingReceiver: false, diagnostics))
		{
			flag = true;
			resultKind = (flag2 ? LookupResultKind.NotAValue : LookupResultKind.NotAVariable);
		}
		return new BoundObjectInitializerMember(namedAssignment.Left, boundExpression.ExpressionSymbol, arguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, resultKind, implicitReceiver.Type, this, boundExpression.Type, flag);
	}

	private static bool CheckNestedObjectInitializerPropertySymbol(PropertySymbol propertySymbol, ExpressionSyntax memberNameSyntax, DiagnosticBag diagnostics, bool suppressErrors, ref LookupResultKind resultKind)
	{
		bool flag = false;
		if (propertySymbol.Type.IsValueType)
		{
			if (!suppressErrors)
			{
				Error(diagnostics, ErrorCode.ERR_ValueTypePropertyInObjectInitializer, memberNameSyntax, propertySymbol, propertySymbol.Type);
				flag = true;
			}
			resultKind = LookupResultKind.NotAValue;
		}
		return !flag;
	}

	private BoundExpression BadObjectInitializerMemberAccess(BoundExpression boundMember, BoundImplicitReceiver implicitReceiver, ExpressionSyntax memberNameSyntax, DiagnosticBag diagnostics, BindValueKind valueKind, bool suppressErrors)
	{
		if (!suppressErrors)
		{
			string text = ((!(memberNameSyntax is IdentifierNameSyntax { Identifier: var identifier })) ? memberNameSyntax.ToString() : identifier.ValueText);
			switch (boundMember.ResultKind)
			{
			case LookupResultKind.Empty:
				Error(diagnostics, ErrorCode.ERR_NoSuchMember, memberNameSyntax, implicitReceiver.Type, text);
				break;
			case LookupResultKind.Inaccessible:
				boundMember = CheckValue(boundMember, valueKind, diagnostics);
				break;
			default:
				Error(diagnostics, ErrorCode.ERR_MemberCannotBeInitialized, memberNameSyntax, text);
				break;
			}
		}
		return ToBadExpression(boundMember, (valueKind == BindValueKind.RValue) ? LookupResultKind.NotAValue : LookupResultKind.NotAVariable);
	}

	private static void ReportDuplicateObjectMemberInitializers(BoundExpression boundMemberInitializer, HashSet<string> memberNameMap, DiagnosticBag diagnostics)
	{
		if (!boundMemberInitializer.HasAnyErrors && ((AssignmentExpressionSyntax)boundMemberInitializer.Syntax).Left is IdentifierNameSyntax { Identifier: { ValueText: var valueText } } identifierNameSyntax && !memberNameMap.Add(valueText))
		{
			Error(diagnostics, ErrorCode.ERR_MemberAlreadyInitialized, identifierNameSyntax, valueText);
		}
	}

	private BoundCollectionInitializerExpression BindCollectionInitializerExpression(InitializerExpressionSyntax initializerSyntax, TypeSymbol initializerType, DiagnosticBag diagnostics, BoundImplicitReceiver implicitReceiver)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		bool flag = CollectionInitializerTypeImplementsIEnumerable(initializerType, initializerSyntax, diagnostics);
		if (!flag && !initializerSyntax.HasErrors && !initializerType.IsErrorType())
		{
			Error(diagnostics, ErrorCode.ERR_CollectionInitRequiresIEnumerable, initializerSyntax, initializerType);
		}
		Binder collectionInitializerAddMethodBinder = WithAdditionalFlags(BinderFlags.CollectionInitializerAddMethod);
		foreach (ExpressionSyntax expression in initializerSyntax.Expressions)
		{
			BoundExpression item = BindCollectionInitializerElement(expression, initializerType, flag, collectionInitializerAddMethodBinder, diagnostics, implicitReceiver);
			instance.Add(item);
		}
		return new BoundCollectionInitializerExpression(initializerSyntax, instance.ToImmutableAndFree(), initializerType);
	}

	private bool CollectionInitializerTypeImplementsIEnumerable(TypeSymbol initializerType, CSharpSyntaxNode node, DiagnosticBag diagnostics)
	{
		if (initializerType.IsDynamic())
		{
			return true;
		}
		if (!initializerType.IsErrorType())
		{
			TypeSymbol specialType = GetSpecialType(SpecialType.System_Collections_IEnumerable, diagnostics, node);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			bool isValid = Conversions.ClassifyImplicitConversionFromType(initializerType, specialType, ref useSiteDiagnostics).IsValid;
			diagnostics.Add(node, useSiteDiagnostics);
			return isValid;
		}
		return false;
	}

	private BoundExpression BindCollectionInitializerElement(ExpressionSyntax elementInitializer, TypeSymbol initializerType, bool hasEnumerableInitializerType, Binder collectionInitializerAddMethodBinder, DiagnosticBag diagnostics, BoundImplicitReceiver implicitReceiver)
	{
		if (elementInitializer.Kind() == SyntaxKind.ComplexElementInitializerExpression)
		{
			return BindComplexElementInitializerExpression((InitializerExpressionSyntax)elementInitializer, diagnostics, hasEnumerableInitializerType, collectionInitializerAddMethodBinder, implicitReceiver);
		}
		if (SyntaxFacts.IsAssignmentExpression(elementInitializer.Kind()))
		{
			Error(diagnostics, ErrorCode.ERR_InvalidInitializerElementInitializer, elementInitializer);
		}
		BoundExpression item = BindInitializerExpressionOrValue(elementInitializer, initializerType, implicitReceiver.Syntax, diagnostics);
		BoundExpression boundExpression = BindCollectionInitializerElementAddMethod(elementInitializer, ImmutableArray.Create(item), hasEnumerableInitializerType, collectionInitializerAddMethodBinder, diagnostics, implicitReceiver);
		boundExpression.WasCompilerGenerated = true;
		return boundExpression;
	}

	private BoundExpression BindComplexElementInitializerExpression(InitializerExpressionSyntax elementInitializer, DiagnosticBag diagnostics, bool hasEnumerableInitializerType, Binder collectionInitializerAddMethodBinder = null, BoundImplicitReceiver implicitReceiver = null)
	{
		SeparatedSyntaxList<ExpressionSyntax> expressions = elementInitializer.Expressions;
		if (expressions.Any())
		{
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			foreach (ExpressionSyntax item in expressions)
			{
				instance.Add(BindValue(item, diagnostics, BindValueKind.RValue));
			}
			return BindCollectionInitializerElementAddMethod(elementInitializer, instance.ToImmutableAndFree(), hasEnumerableInitializerType, collectionInitializerAddMethodBinder, diagnostics, implicitReceiver);
		}
		Error(diagnostics, ErrorCode.ERR_EmptyElementInitializer, elementInitializer);
		return BadExpression(elementInitializer, LookupResultKind.NotInvocable);
	}

	private BoundExpression BindUnexpectedComplexElementInitializer(InitializerExpressionSyntax node, DiagnosticBag diagnostics)
	{
		return BindComplexElementInitializerExpression(node, diagnostics, hasEnumerableInitializerType: false);
	}

	private BoundExpression BindCollectionInitializerElementAddMethod(ExpressionSyntax elementInitializer, ImmutableArray<BoundExpression> boundElementInitializerExpressions, bool hasEnumerableInitializerType, Binder collectionInitializerAddMethodBinder, DiagnosticBag diagnostics, BoundImplicitReceiver implicitReceiver)
	{
		if (!hasEnumerableInitializerType)
		{
			return BadExpression(elementInitializer, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, boundElementInitializerExpressions);
		}
		if (implicitReceiver.Type.IsDynamic())
		{
			bool hasErrors = ReportBadDynamicArguments(elementInitializer, boundElementInitializerExpressions, default, diagnostics, null);
			return new BoundDynamicCollectionElementInitializer(elementInitializer, boundElementInitializerExpressions, implicitReceiver, ImmutableArray<MethodSymbol>.Empty, GetSpecialType(SpecialType.System_Void, diagnostics, elementInitializer), hasErrors);
		}
		BoundExpression boundExpression = collectionInitializerAddMethodBinder.MakeInvocationExpression(elementInitializer, implicitReceiver, "Add", boundElementInitializerExpressions, diagnostics);
		if (boundExpression.Kind == BoundKind.DynamicInvocation)
		{
			BoundDynamicInvocation boundDynamicInvocation = (BoundDynamicInvocation)boundExpression;
			return new BoundDynamicCollectionElementInitializer(elementInitializer, boundDynamicInvocation.Arguments, implicitReceiver, boundDynamicInvocation.ApplicableMethods, boundDynamicInvocation.Type, boundDynamicInvocation.HasAnyErrors);
		}
		if (boundExpression.Kind == BoundKind.Call)
		{
			BoundCall boundCall = (BoundCall)boundExpression;
			if (boundCall.HasErrors && !boundCall.OriginalMethodsOpt.IsDefault)
			{
				return boundCall;
			}
			return new BoundCollectionElementInitializer(elementInitializer, boundCall.Method, boundCall.Arguments, boundCall.ReceiverOpt, boundCall.Expanded, boundCall.ArgsToParamsOpt, boundCall.InvokedAsExtensionMethod, boundCall.ResultKind, boundCall.BinderOpt, boundCall.Type, boundCall.HasAnyErrors)
			{
				WasCompilerGenerated = true
			};
		}
		return boundExpression;
	}

	internal ImmutableArray<MethodSymbol> FilterInaccessibleConstructors(ImmutableArray<MethodSymbol> constructors, bool allowProtectedConstructorsOfBaseType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<MethodSymbol> arrayBuilder = null;
		for (int i = 0; i < constructors.Length; i++)
		{
			MethodSymbol methodSymbol = constructors[i];
			if (!IsConstructorAccessible(methodSymbol, ref useSiteDiagnostics, allowProtectedConstructorsOfBaseType))
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<MethodSymbol>.GetInstance();
					arrayBuilder.AddRange(constructors, i);
				}
			}
			else
			{
				arrayBuilder?.Add(methodSymbol);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? constructors;
	}

	private bool IsConstructorAccessible(MethodSymbol constructor, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool allowProtectedConstructorsOfBaseType = false)
	{
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null)
		{
			if (!allowProtectedConstructorsOfBaseType)
			{
				return IsSymbolAccessibleConditional(constructor, containingType, ref useSiteDiagnostics, constructor.ContainingType);
			}
			return IsAccessible(constructor, ref useSiteDiagnostics);
		}
		return IsSymbolAccessibleConditional(constructor, Compilation.Assembly, ref useSiteDiagnostics);
	}

	protected BoundExpression BindClassCreationExpression(CSharpSyntaxNode node, string typeName, CSharpSyntaxNode typeNode, NamedTypeSymbol type, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics, BoundObjectInitializerExpressionBase boundInitializerOpt = null)
	{
		BoundExpression boundExpression = null;
		bool flag = type.IsErrorType();
		if (type.IsAbstract)
		{
			diagnostics.Add(ErrorCode.ERR_NoNewAbstract, node.Location, type);
			flag = true;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (analyzedArguments.HasDynamicArgument)
		{
			OverloadResolutionResult<MethodSymbol> instance = OverloadResolutionResult<MethodSymbol>.GetInstance();
			OverloadResolution.ObjectCreationOverloadResolution(GetAccessibleConstructorsForOverloadResolution(type, ref useSiteDiagnostics), analyzedArguments, instance, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			useSiteDiagnostics = null;
			if (instance.HasAnyApplicableMember)
			{
				ImmutableArray<BoundExpression> arguments = BuildArgumentsForDynamicInvocation(analyzedArguments, diagnostics);
				ImmutableArray<RefKind> immutableArray = analyzedArguments.RefKinds.ToImmutableOrNull();
				flag &= ReportBadDynamicArguments(node, arguments, immutableArray, diagnostics, null);
				boundExpression = new BoundDynamicObjectCreationExpression(node, typeName, arguments, analyzedArguments.GetNames(), immutableArray, boundInitializerOpt, instance.GetAllApplicableMembers(), type, flag);
			}
			instance.Free();
			if (boundExpression != null)
			{
				return boundExpression;
			}
		}
		if (TryPerformConstructorOverloadResolution(type, analyzedArguments, typeName, typeNode.Location, flag, diagnostics, out var memberResolutionResult, out var candidateConstructors, allowProtectedConstructorsOfBaseType: false))
		{
			MethodSymbol member = memberResolutionResult.Member;
			bool flag2 = false;
			if (member.HasUnsafeParameter())
			{
				flag2 = ReportUnsafeIfNotAllowed(node, diagnostics) | flag2;
			}
			ReportDiagnosticsIfObsolete(diagnostics, member, node, hasBaseReceiver: false);
			ConstantValue constantValueOpt = ((boundInitializerOpt == null && member.IsDefaultValueTypeConstructor()) ? FoldParameterlessValueTypeConstructor(type) : null);
			ImmutableArray<BoundExpression> immutableArray2 = analyzedArguments.Arguments.ToImmutable();
			ImmutableArray<RefKind> argumentRefKindsOpt = analyzedArguments.RefKinds.ToImmutableOrNull();
			ImmutableArray<int> argsToParamsOpt = memberResolutionResult.Result.ArgsToParamsOpt;
			if (!flag2)
			{
				flag2 = !CheckInvocationArgMixing(node, member, null, member.Parameters, immutableArray2, argsToParamsOpt, LocalScopeDepth, diagnostics);
			}
			boundExpression = new BoundObjectCreationExpression(node, member, candidateConstructors, immutableArray2, analyzedArguments.GetNames(), argumentRefKindsOpt, memberResolutionResult.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm, argsToParamsOpt, constantValueOpt, boundInitializerOpt, this, type, flag2);
			if (type.IsAbstract)
			{
				boundExpression = BadExpression(node, LookupResultKind.NotCreatable, boundExpression);
			}
			return boundExpression;
		}
		LookupResultKind resultKind;
		if (type.IsAbstract)
		{
			resultKind = LookupResultKind.NotCreatable;
		}
		else
		{
			resultKind = ((!memberResolutionResult.IsValid || IsConstructorAccessible(memberResolutionResult.Member, ref useSiteDiagnostics)) ? LookupResultKind.OverloadResolutionFailure : LookupResultKind.Inaccessible);
		}
		diagnostics.Add(node, useSiteDiagnostics);
		ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
		instance2.AddRange(candidateConstructors);
		ArrayBuilder<BoundExpression> instance3 = ArrayBuilder<BoundExpression>.GetInstance();
		instance3.AddRange(BuildArgumentsForErrorRecovery(analyzedArguments, candidateConstructors));
		if (boundInitializerOpt != null)
		{
			instance3.Add(boundInitializerOpt);
		}
		return new BoundBadExpression(node, resultKind, instance2.ToImmutableAndFree(), instance3.ToImmutableAndFree(), type);
	}

	private BoundExpression BindInterfaceCreationExpression(ObjectCreationExpressionSyntax node, NamedTypeSymbol type, BoundObjectInitializerExpressionBase boundInitializerOpt, DiagnosticBag diagnostics)
	{
		if (!InAttributeArgument && type.IsComImport)
		{
			NamedTypeSymbol comImportCoClass = type.ComImportCoClass;
			if ((object)comImportCoClass != null)
			{
				return BindComImportCoClassCreationExpression(node, type, comImportCoClass, boundInitializerOpt, diagnostics);
			}
		}
		diagnostics.Add(ErrorCode.ERR_NoNewAbstract, node.Location, type);
		return BindBadInterfaceCreationExpression(node, type, diagnostics);
	}

	private BoundExpression BindBadInterfaceCreationExpression(ObjectCreationExpressionSyntax node, NamedTypeSymbol type, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		BindArgumentsAndNames(node.ArgumentList, diagnostics, instance);
		ImmutableArray<BoundExpression> childBoundNodes = BuildArgumentsForErrorRecovery(instance);
		BoundBadExpression result = new BoundBadExpression(node, LookupResultKind.NotCreatable, ImmutableArray.Create((Symbol)type), childBoundNodes, type);
		instance.Free();
		return result;
	}

	private BoundExpression BindComImportCoClassCreationExpression(ObjectCreationExpressionSyntax node, NamedTypeSymbol interfaceType, NamedTypeSymbol coClassType, BoundObjectInitializerExpressionBase boundInitializerOpt, DiagnosticBag diagnostics)
	{
		if (coClassType.IsErrorType())
		{
			Error(diagnostics, ErrorCode.ERR_MissingCoClass, node, coClassType, interfaceType);
		}
		else
		{
			if (!coClassType.IsUnboundGenericType)
			{
				if (interfaceType.ContainingAssembly.IsLinked)
				{
					return BindNoPiaObjectCreationExpression(node, interfaceType, coClassType, boundInitializerOpt, diagnostics);
				}
				BoundExpression boundExpression = BindClassCreationExpression(node, coClassType, coClassType.Name, boundInitializerOpt, diagnostics);
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				Conversion conversion = Conversions.ClassifyConversionFromExpression(boundExpression, interfaceType, ref useSiteDiagnostics, forCast: true);
				diagnostics.Add(node, useSiteDiagnostics);
				if (!conversion.IsValid)
				{
					SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(Compilation, coClassType, interfaceType);
					Error(diagnostics, ErrorCode.ERR_NoExplicitConv, node, symbolDistinguisher.First, symbolDistinguisher.Second);
				}
				CreateConversion(boundExpression, conversion, interfaceType, diagnostics);
				switch (boundExpression.Kind)
				{
				case BoundKind.ObjectCreationExpression:
				{
					BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)boundExpression;
					return boundObjectCreationExpression.Update(boundObjectCreationExpression.Constructor, boundObjectCreationExpression.ConstructorsGroup, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.ArgumentNamesOpt, boundObjectCreationExpression.ArgumentRefKindsOpt, boundObjectCreationExpression.Expanded, boundObjectCreationExpression.ArgsToParamsOpt, boundObjectCreationExpression.ConstantValueOpt, boundObjectCreationExpression.InitializerExpressionOpt, boundObjectCreationExpression.BinderOpt, interfaceType);
				}
				case BoundKind.BadExpression:
				{
					BoundBadExpression boundBadExpression = (BoundBadExpression)boundExpression;
					return boundBadExpression.Update(boundBadExpression.ResultKind, boundBadExpression.Symbols, boundBadExpression.ChildBoundNodes, interfaceType);
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind);
				}
			}
			Error(diagnostics, ErrorCode.ERR_BadCoClassSig, node, coClassType, interfaceType);
		}
		return BindBadInterfaceCreationExpression(node, interfaceType, diagnostics);
	}

	private BoundExpression BindNoPiaObjectCreationExpression(ObjectCreationExpressionSyntax node, NamedTypeSymbol interfaceType, NamedTypeSymbol coClassType, BoundObjectInitializerExpressionBase boundInitializerOpt, DiagnosticBag diagnostics)
	{
		if (!coClassType.GetGuidString(out var guidString))
		{
			Guid empty = Guid.Empty;
			guidString = empty.ToString("D");
		}
		BoundNoPiaObjectCreationExpression boundNoPiaObjectCreationExpression = new BoundNoPiaObjectCreationExpression(node, guidString, boundInitializerOpt, interfaceType);
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		try
		{
			BindArgumentsAndNames(node.ArgumentList, diagnostics, instance);
			if (instance.Arguments.Count > 0)
			{
				diagnostics.Add(ErrorCode.ERR_BadCtorArgCount, node.ArgumentList.Location, interfaceType, instance.Arguments.Count);
				ImmutableArray<BoundExpression> childBoundNodes = BuildArgumentsForErrorRecovery(instance).Add(boundNoPiaObjectCreationExpression);
				return new BoundBadExpression(node, LookupResultKind.OverloadResolutionFailure, ImmutableArray<Symbol>.Empty, childBoundNodes, boundNoPiaObjectCreationExpression.Type);
			}
			return boundNoPiaObjectCreationExpression;
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression BindTypeParameterCreationExpression(ObjectCreationExpressionSyntax node, TypeParameterSymbol typeParameter, BoundObjectInitializerExpressionBase boundInitializerOpt, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		BindArgumentsAndNames(node.ArgumentList, diagnostics, instance);
		bool flag = instance.Arguments.Count > 0;
		try
		{
			if (!typeParameter.HasConstructorConstraint && !typeParameter.IsValueType)
			{
				diagnostics.Add(ErrorCode.ERR_NoNewTyvar, node.Location, typeParameter);
			}
			else
			{
				if (!flag)
				{
					return new BoundNewT(node, boundInitializerOpt, typeParameter);
				}
				diagnostics.Add(ErrorCode.ERR_NewTyvarWithArgs, node.Location, typeParameter);
			}
			return MakeBadExpressionForObjectCreation(node, typeParameter, boundInitializerOpt, instance);
		}
		finally
		{
			instance.Free();
		}
	}

	private bool TryPerformConstructorOverloadResolution(NamedTypeSymbol typeContainingConstructors, AnalyzedArguments analyzedArguments, string errorName, Location errorLocation, bool suppressResultDiagnostics, DiagnosticBag diagnostics, out MemberResolutionResult<MethodSymbol> memberResolutionResult, out ImmutableArray<MethodSymbol> candidateConstructors, bool allowProtectedConstructorsOfBaseType)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		candidateConstructors = GetAccessibleConstructorsForOverloadResolution(typeContainingConstructors, allowProtectedConstructorsOfBaseType, out var allInstanceConstructors, ref useSiteDiagnostics);
		OverloadResolutionResult<MethodSymbol> overloadResolutionResult = OverloadResolutionResult<MethodSymbol>.GetInstance();
		bool flag = false;
		bool flag2 = false;
		if (candidateConstructors.Any())
		{
			OverloadResolution.ObjectCreationOverloadResolution(candidateConstructors, analyzedArguments, overloadResolutionResult, ref useSiteDiagnostics);
			if (overloadResolutionResult.Succeeded)
			{
				flag = true;
				flag2 = true;
			}
		}
		if (!flag && allInstanceConstructors.Length > candidateConstructors.Length)
		{
			OverloadResolutionResult<MethodSymbol> instance = OverloadResolutionResult<MethodSymbol>.GetInstance();
			OverloadResolution.ObjectCreationOverloadResolution(allInstanceConstructors, analyzedArguments, instance, ref useSiteDiagnostics);
			if (instance.Succeeded)
			{
				flag2 = true;
				candidateConstructors = allInstanceConstructors;
				overloadResolutionResult.Free();
				overloadResolutionResult = instance;
			}
			else
			{
				instance.Free();
			}
		}
		diagnostics.Add(errorLocation, useSiteDiagnostics);
		if (flag2)
		{
			CoerceArguments(overloadResolutionResult.ValidResult, analyzedArguments.Arguments, diagnostics);
		}
		memberResolutionResult = (flag2 ? overloadResolutionResult.ValidResult : default(MemberResolutionResult<MethodSymbol>));
		if (!flag && !suppressResultDiagnostics)
		{
			if (flag2)
			{
				diagnostics.Add(ErrorCode.ERR_BadAccess, errorLocation, overloadResolutionResult.ValidResult.Member);
			}
			else
			{
				overloadResolutionResult.ReportDiagnostics(this, errorLocation, null, diagnostics, errorName, null, null, analyzedArguments, candidateConstructors, typeContainingConstructors, null);
			}
		}
		overloadResolutionResult.Free();
		return flag;
	}

	private ImmutableArray<MethodSymbol> GetAccessibleConstructorsForOverloadResolution(NamedTypeSymbol type, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ImmutableArray<MethodSymbol> allInstanceConstructors;
		return GetAccessibleConstructorsForOverloadResolution(type, allowProtectedConstructorsOfBaseType: false, out allInstanceConstructors, ref useSiteDiagnostics);
	}

	private ImmutableArray<MethodSymbol> GetAccessibleConstructorsForOverloadResolution(NamedTypeSymbol type, bool allowProtectedConstructorsOfBaseType, out ImmutableArray<MethodSymbol> allInstanceConstructors, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (type.IsErrorType())
		{
			type = (type.GetNonErrorGuess() as NamedTypeSymbol) ?? type;
		}
		allInstanceConstructors = type.InstanceConstructors;
		return FilterInaccessibleConstructors(allInstanceConstructors, allowProtectedConstructorsOfBaseType, ref useSiteDiagnostics);
	}

	private static ConstantValue FoldParameterlessValueTypeConstructor(NamedTypeSymbol type)
	{
		SpecialType specialType = type.SpecialType;
		if (type.TypeKind == TypeKind.Enum)
		{
			specialType = type.EnumUnderlyingType.SpecialType;
		}
		if ((uint)(specialType - 7) <= 12u)
		{
			return ConstantValue.Default(specialType);
		}
		return null;
	}

	private BoundLiteral BindLiteralConstant(LiteralExpressionSyntax node, DiagnosticBag diagnostics)
	{
		object value = node.Token.Value;
		TypeSymbol type = null;
		ConstantValue constantValueOpt;
		if (value == null)
		{
			constantValueOpt = ConstantValue.Null;
		}
		else
		{
			SpecialType specialType = SpecialTypeExtensions.FromRuntimeTypeOfLiteralValue(value);
			constantValueOpt = ConstantValue.Create(value, specialType);
			type = GetSpecialType(specialType, diagnostics, node);
		}
		return new BoundLiteral(node, constantValueOpt, type);
	}

	private BoundExpression BindCheckedExpression(CheckedExpressionSyntax node, DiagnosticBag diagnostics)
	{
		return WithCheckedOrUncheckedRegion(node.Kind() == SyntaxKind.CheckedExpression).BindParenthesizedExpression(node.Expression, diagnostics);
	}

	private BoundExpression BindMemberAccess(MemberAccessExpressionSyntax node, bool invoked, bool indexed, DiagnosticBag diagnostics)
	{
		ExpressionSyntax expression = node.Expression;
		BoundExpression boundLeft;
		if (node.Kind() == SyntaxKind.SimpleMemberAccessExpression)
		{
			boundLeft = BindLeftOfPotentialColorColorMemberAccess(expression, diagnostics);
		}
		else
		{
			boundLeft = BindExpression(expression, diagnostics);
			BindPointerIndirectionExpressionInternal(node, boundLeft, diagnostics, out var pointedAtType, out var hasErrors);
			boundLeft = (((object)pointedAtType != null) ? new BoundPointerIndirectionOperator(expression, boundLeft, pointedAtType, hasErrors)
			{
				WasCompilerGenerated = true
			} : ToBadExpression(boundLeft));
		}
		return BindMemberAccessWithBoundLeft(node, boundLeft, node.Name, node.OperatorToken, invoked, indexed, diagnostics);
	}

	private BoundExpression BindLeftOfPotentialColorColorMemberAccess(ExpressionSyntax left, DiagnosticBag diagnostics)
	{
		if (left.Kind() == SyntaxKind.IdentifierName)
		{
			IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)left;
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression boundExpression = BindIdentifier(identifierNameSyntax, invoked: false, indexed: false, instance);
			Symbol symbol = ((boundExpression.Kind != BoundKind.Conversion) ? boundExpression.ExpressionSymbol : ((BoundConversion)boundExpression).Operand.ExpressionSymbol);
			if ((object)symbol != null)
			{
				switch (symbol.Kind)
				{
				case SymbolKind.Field:
				case SymbolKind.Local:
				case SymbolKind.Parameter:
				case SymbolKind.Property:
				case SymbolKind.RangeVariable:
				{
					TypeSymbol type = boundExpression.Type;
					string valueText = identifierNameSyntax.Identifier.ValueText;
					if (type.Name == valueText || IsUsingAliasInScope(valueText))
					{
						DiagnosticBag diagnosticBag = new DiagnosticBag();
						BoundExpression boundExpression2 = BindNamespaceOrType(identifierNameSyntax, diagnosticBag);
						if (boundExpression2.Type == type)
						{
							DiagnosticBag diagnosticBag2 = new DiagnosticBag();
							diagnosticBag2.AddRangeAndFree(instance);
							return new BoundTypeOrValueExpression(left, new BoundTypeOrValueData(symbol, boundExpression, diagnosticBag2, boundExpression2, diagnosticBag), type);
						}
					}
					break;
				}
				}
			}
			diagnostics.AddRangeAndFree(instance);
			return boundExpression;
		}
		return BindExpression(left, diagnostics);
	}

	private bool IsUsingAliasInScope(string name)
	{
		bool isSemanticModelBinder = IsSemanticModelBinder;
		for (ImportChain importChain = ImportChain; importChain != null; importChain = importChain.ParentOpt)
		{
			if (importChain.Imports.IsUsingAlias(name, isSemanticModelBinder))
			{
				return true;
			}
		}
		return false;
	}

	private BoundExpression BindDynamicMemberAccess(ExpressionSyntax node, BoundExpression boundLeft, SimpleNameSyntax right, bool invoked, bool indexed, DiagnosticBag diagnostics)
	{
		SeparatedSyntaxList<TypeSyntax> typeArguments = ((right.Kind() == SyntaxKind.GenericName) ? ((GenericNameSyntax)right).TypeArgumentList.Arguments : default(SeparatedSyntaxList<TypeSyntax>));
		bool flag = typeArguments.Count > 0;
		ImmutableArray<TypeSymbol> typeArgumentsOpt = (flag ? BindTypeArguments(typeArguments, diagnostics) : default(ImmutableArray<TypeSymbol>));
		bool hasErrors = false;
		if (!invoked & flag)
		{
			Error(diagnostics, ErrorCode.ERR_TypeArgsNotAllowed, right, right.Identifier.Text, SymbolKind.Property.Localize());
			hasErrors = true;
		}
		if (flag)
		{
			for (int i = 0; i < typeArgumentsOpt.Length; i++)
			{
				TypeSymbol typeSymbol = typeArgumentsOpt[i];
				if (typeSymbol.IsPointerType() || typeSymbol.IsRestrictedType())
				{
					Error(diagnostics, ErrorCode.ERR_BadTypeArgument, typeArguments[i], typeSymbol);
					hasErrors = true;
				}
			}
		}
		return new BoundDynamicMemberAccess(node, boundLeft, typeArgumentsOpt, right.Identifier.ValueText, invoked, indexed, Compilation.DynamicType, hasErrors);
	}

	private BoundExpression BindMemberAccessWithBoundLeft(ExpressionSyntax node, BoundExpression boundLeft, SimpleNameSyntax right, SyntaxToken operatorToken, bool invoked, bool indexed, DiagnosticBag diagnostics)
	{
		boundLeft = MakeMemberAccessValue(boundLeft, diagnostics);
		TypeSymbol type = boundLeft.Type;
		if ((object)type != null && type.IsDynamic())
		{
			boundLeft = CheckValue(boundLeft, BindValueKind.RValue, diagnostics);
			return BindDynamicMemberAccess(node, boundLeft, right, invoked, indexed, diagnostics);
		}
		if ((object)type != null && type.SpecialType == SpecialType.System_Void)
		{
			diagnostics.Add(ErrorCode.ERR_BadUnaryOp, operatorToken.GetLocation(), SyntaxFacts.GetText(operatorToken.Kind()), type);
			return BadExpression(node, boundLeft);
		}
		if (boundLeft.IsLiteralDefault())
		{
			DiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.ERR_BadUnaryOp, SyntaxFacts.GetText(operatorToken.Kind()), "default");
			diagnostics.Add(new CSDiagnostic(info, operatorToken.GetLocation()));
			return BadExpression(node, boundLeft);
		}
		if (boundLeft.Kind == BoundKind.UnboundLambda)
		{
			MessageID messageID = ((UnboundLambda)boundLeft).MessageID;
			diagnostics.Add(ErrorCode.ERR_BadUnaryOp, node.Location, SyntaxFacts.GetText(operatorToken.Kind()), messageID.Localize());
			return BadExpression(node, boundLeft);
		}
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			LookupOptions lookupOptions = LookupOptions.AllMethodsOnArityZero;
			if (invoked)
			{
				lookupOptions |= LookupOptions.MustBeInvocableIfMember;
			}
			SeparatedSyntaxList<TypeSyntax> separatedSyntaxList = ((right.Kind() == SyntaxKind.GenericName) ? ((GenericNameSyntax)right).TypeArgumentList.Arguments : default(SeparatedSyntaxList<TypeSyntax>));
			bool flag = separatedSyntaxList.Count > 0;
			ImmutableArray<TypeSymbol> typeArguments = (flag ? BindTypeArguments(separatedSyntaxList, diagnostics) : default(ImmutableArray<TypeSymbol>));
			string valueText = right.Identifier.ValueText;
			int arity = right.Arity;
			switch (boundLeft.Kind)
			{
			case BoundKind.NamespaceExpression:
			{
				NamespaceSymbol namespaceSymbol = ((BoundNamespaceExpression)boundLeft).NamespaceSymbol;
				HashSet<DiagnosticInfo> useSiteDiagnostics2 = null;
				LookupMembersWithFallback(instance, namespaceSymbol, valueText, arity, ref useSiteDiagnostics2, null, lookupOptions);
				diagnostics.Add(right, useSiteDiagnostics2);
				ArrayBuilder<Symbol> symbols = instance.Symbols;
				if (instance.IsMultiViable)
				{
					Symbol symbol = ResultSymbol(instance, valueText, arity, node, diagnostics, suppressUseSiteDiagnostics: false, out var wasError, namespaceSymbol, lookupOptions);
					if (wasError)
					{
						return new BoundBadExpression(node, LookupResultKind.Ambiguous, instance.Symbols.AsImmutable(), ImmutableArray.Create(boundLeft), CreateErrorType(valueText), hasErrors: true);
					}
					if (symbol.Kind == SymbolKind.Namespace)
					{
						return new BoundNamespaceExpression(node, (NamespaceSymbol)symbol);
					}
					NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
					if (flag)
					{
						namedTypeSymbol = ConstructNamedTypeUnlessTypeArgumentOmitted(right, namedTypeSymbol, separatedSyntaxList, typeArguments, diagnostics);
					}
					ReportDiagnosticsIfObsolete(diagnostics, namedTypeSymbol, node, hasBaseReceiver: false);
					return new BoundTypeExpression(node, null, namedTypeSymbol);
				}
				if (instance.Kind == LookupResultKind.WrongArity)
				{
					Error(diagnostics, instance.Error, right);
					return new BoundTypeExpression(node, null, new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(symbols[0]), symbols.ToImmutable(), instance.Kind, instance.Error, arity));
				}
				if (instance.Kind == LookupResultKind.Empty)
				{
					NotFound(node, valueText, arity, valueText, diagnostics, null, namespaceSymbol, lookupOptions);
					return new BoundBadExpression(node, instance.Kind, symbols.AsImmutable(), ImmutableArray.Create(boundLeft), CreateErrorType(valueText), hasErrors: true);
				}
				break;
			}
			case BoundKind.TypeExpression:
			{
				if (type.TypeKind == TypeKind.TypeParameter)
				{
					Error(diagnostics, ErrorCode.ERR_BadSKunknown, boundLeft.Syntax, type, MessageID.IDS_SK_TYVAR.Localize());
					return BadExpression(node, LookupResultKind.NotAValue, boundLeft);
				}
				if (EnclosingNameofArgument == node)
				{
					return BindInstanceMemberAccess(node, right, boundLeft, valueText, arity, separatedSyntaxList, typeArguments, invoked, indexed, diagnostics);
				}
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				LookupMembersWithFallback(instance, type, valueText, arity, ref useSiteDiagnostics, null, lookupOptions);
				diagnostics.Add(right, useSiteDiagnostics);
				if (instance.IsMultiViable)
				{
					return BindMemberOfType(node, right, valueText, arity, indexed, boundLeft, separatedSyntaxList, typeArguments, instance, BoundMethodGroupFlags.None, diagnostics);
				}
				break;
			}
			case BoundKind.TypeOrValueExpression:
				return BindInstanceMemberAccess(node, right, boundLeft, valueText, arity, separatedSyntaxList, typeArguments, invoked, indexed, diagnostics);
			default:
				if ((boundLeft.Kind == BoundKind.Literal && ((BoundLiteral)boundLeft).ConstantValueOpt == ConstantValue.Null) || boundLeft.Kind == BoundKind.StackAllocArrayCreation)
				{
					if (!boundLeft.HasAnyErrors)
					{
						Error(diagnostics, ErrorCode.ERR_BadUnaryOp, node, operatorToken.Text, boundLeft.Display);
					}
					return BadExpression(node, boundLeft);
				}
				if ((object)type != null)
				{
					boundLeft = CheckValue(boundLeft, BindValueKind.RValue, diagnostics);
					return BindInstanceMemberAccess(node, right, boundLeft, valueText, arity, separatedSyntaxList, typeArguments, invoked, indexed, diagnostics);
				}
				break;
			}
			BindMemberAccessReportError(node, right, valueText, boundLeft, instance.Error, diagnostics);
			return BindMemberAccessBadResult(node, valueText, boundLeft, instance.Error, instance.Symbols.ToImmutable(), instance.Kind);
		}
		finally
		{
			instance.Free();
		}
	}

	private static void WarnOnAccessOfOffDefault(SyntaxNode node, BoundExpression boundLeft, DiagnosticBag diagnostics)
	{
		if (boundLeft != null && boundLeft.Kind == BoundKind.DefaultExpression && boundLeft.ConstantValue == ConstantValue.Null)
		{
			Error(diagnostics, ErrorCode.WRN_DotOnDefault, node, boundLeft.Type);
		}
	}

	private BoundExpression MakeMemberAccessValue(BoundExpression expr, DiagnosticBag diagnostics)
	{
		switch (expr.Kind)
		{
		case BoundKind.MethodGroup:
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)expr;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			MethodGroupResolution methodGroupResolution = ResolveMethodGroup(boundMethodGroup, null, isMethodGroupConversion: false, ref useSiteDiagnostics);
			diagnostics.Add(expr.Syntax, useSiteDiagnostics);
			if (!expr.HasAnyErrors)
			{
				diagnostics.AddRange(methodGroupResolution.Diagnostics);
				if (methodGroupResolution.MethodGroup != null && !methodGroupResolution.HasAnyErrors)
				{
					MethodSymbol methodSymbol = methodGroupResolution.MethodGroup.Methods[0];
					Error(diagnostics, ErrorCode.ERR_BadSKunknown, boundMethodGroup.NameSyntax, methodSymbol, MessageID.IDS_SK_METHOD.Localize());
				}
			}
			expr = BindMemberAccessBadResult(boundMethodGroup);
			methodGroupResolution.Free();
			return expr;
		}
		case BoundKind.PropertyGroup:
			return BindIndexedPropertyAccess((BoundPropertyGroup)expr, mustHaveAllOptionalParameters: false, diagnostics);
		default:
			return expr;
		}
	}

	private BoundExpression BindInstanceMemberAccess(SyntaxNode node, SyntaxNode right, BoundExpression boundLeft, string rightName, int rightArity, SeparatedSyntaxList<TypeSyntax> typeArgumentsSyntax, ImmutableArray<TypeSymbol> typeArguments, bool invoked, bool indexed, DiagnosticBag diagnostics)
	{
		TypeSymbol type = boundLeft.Type;
		LookupOptions lookupOptions = LookupOptions.AllMethodsOnArityZero;
		if (invoked)
		{
			lookupOptions |= LookupOptions.MustBeInvocableIfMember;
		}
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			bool flag = boundLeft.Kind == BoundKind.BaseReference;
			if (flag)
			{
				lookupOptions |= LookupOptions.UseBaseReferenceAccessibility;
			}
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			LookupMembersWithFallback(instance, type, rightName, rightArity, ref useSiteDiagnostics, null, lookupOptions);
			diagnostics.Add(right, useSiteDiagnostics);
			bool flag2 = !flag;
			BoundMethodGroupFlags boundMethodGroupFlags = BoundMethodGroupFlags.None;
			if (flag2)
			{
				boundMethodGroupFlags |= BoundMethodGroupFlags.SearchExtensionMethods;
			}
			if (instance.IsMultiViable)
			{
				return BindMemberOfType(node, right, rightName, rightArity, indexed, boundLeft, typeArgumentsSyntax, typeArguments, instance, boundMethodGroupFlags, diagnostics);
			}
			if (flag2)
			{
				BoundMethodGroup boundMethodGroup = new BoundMethodGroup(node, typeArguments, boundLeft, rightName, instance.Symbols.All((Symbol s) => s.Kind == SymbolKind.Method) ? instance.Symbols.SelectAsArray(s_toMethodSymbolFunc) : ImmutableArray<MethodSymbol>.Empty, instance, boundMethodGroupFlags);
				if (!boundMethodGroup.HasErrors && boundMethodGroup.ResultKind == LookupResultKind.Empty && typeArgumentsSyntax.Any(SyntaxKind.OmittedTypeArgument))
				{
					Error(diagnostics, ErrorCode.ERR_BadArity, node, rightName, MessageID.IDS_MethodGroup.Localize(), typeArgumentsSyntax.Count);
				}
				return boundMethodGroup;
			}
			BindMemberAccessReportError(node, right, rightName, boundLeft, instance.Error, diagnostics);
			return BindMemberAccessBadResult(node, rightName, boundLeft, instance.Error, instance.Symbols.ToImmutable(), instance.Kind);
		}
		finally
		{
			instance.Free();
		}
	}

	private void BindMemberAccessReportError(BoundMethodGroup node, DiagnosticBag diagnostics)
	{
		SyntaxNode nameSyntax = node.NameSyntax;
		SyntaxNode node2 = node.MemberAccessExpressionSyntax ?? nameSyntax;
		BindMemberAccessReportError(node2, nameSyntax, node.Name, node.ReceiverOpt, node.LookupError, diagnostics);
	}

	private void BindMemberAccessReportError(SyntaxNode node, SyntaxNode name, string plainName, BoundExpression boundLeft, DiagnosticInfo lookupError, DiagnosticBag diagnostics)
	{
		if (!boundLeft.HasAnyErrors || boundLeft.Kind == BoundKind.TypeOrValueExpression)
		{
			if (lookupError != null)
			{
				diagnostics.Add(new CSDiagnostic(lookupError, name.Location));
			}
			else if (node.IsQuery())
			{
				ReportQueryLookupFailed(node, boundLeft, plainName, ImmutableArray<Symbol>.Empty, diagnostics);
			}
			else if ((object)boundLeft.Type == null)
			{
				Error(diagnostics, ErrorCode.ERR_NoSuchMember, name, boundLeft.Display, plainName);
			}
			else if (boundLeft.Kind == BoundKind.TypeExpression || boundLeft.Kind == BoundKind.BaseReference || (node.Kind() == SyntaxKind.AwaitExpression && plainName == "GetResult"))
			{
				Error(diagnostics, ErrorCode.ERR_NoSuchMember, name, boundLeft.Type, plainName);
			}
			else if (WouldUsingSystemFindExtension(boundLeft.Type, plainName))
			{
				Error(diagnostics, ErrorCode.ERR_NoSuchMemberOrExtensionNeedUsing, name, boundLeft.Type, plainName, "System");
			}
			else
			{
				Error(diagnostics, ErrorCode.ERR_NoSuchMemberOrExtension, name, boundLeft.Type, plainName);
			}
		}
	}

	private bool WouldUsingSystemFindExtension(TypeSymbol receiver, string methodName)
	{
		if (methodName == "GetAwaiter")
		{
			return ImplementsWinRTAsyncInterface(receiver);
		}
		return false;
	}

	private bool ImplementsWinRTAsyncInterface(TypeSymbol type)
	{
		if (!IsWinRTAsyncInterface(type))
		{
			return type.AllInterfacesNoUseSiteDiagnostics.Any((NamedTypeSymbol i) => IsWinRTAsyncInterface(i));
		}
		return true;
	}

	private bool IsWinRTAsyncInterface(TypeSymbol type)
	{
		if (!type.IsInterfaceType())
		{
			return false;
		}
		NamedTypeSymbol constructedFrom = ((NamedTypeSymbol)type).ConstructedFrom;
		if (!(constructedFrom == Compilation.GetWellKnownType(WellKnownType.Windows_Foundation_IAsyncAction)) && !(constructedFrom == Compilation.GetWellKnownType(WellKnownType.Windows_Foundation_IAsyncActionWithProgress_T)) && !(constructedFrom == Compilation.GetWellKnownType(WellKnownType.Windows_Foundation_IAsyncOperation_T)))
		{
			return constructedFrom == Compilation.GetWellKnownType(WellKnownType.Windows_Foundation_IAsyncOperationWithProgress_T2);
		}
		return true;
	}

	private BoundExpression BindMemberAccessBadResult(BoundMethodGroup node)
	{
		SyntaxNode nameSyntax = node.NameSyntax;
		SyntaxNode node2 = node.MemberAccessExpressionSyntax ?? nameSyntax;
		return BindMemberAccessBadResult(node2, node.Name, node.ReceiverOpt, node.LookupError, StaticCast<Symbol>.From(node.Methods), node.ResultKind);
	}

	private BoundExpression BindMemberAccessBadResult(SyntaxNode node, string nameString, BoundExpression boundLeft, DiagnosticInfo lookupError, ImmutableArray<Symbol> symbols, LookupResultKind lookupKind)
	{
		if (symbols.Length > 0 && symbols[0].Kind == SymbolKind.Method)
		{
			ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (Symbol item2 in symbols)
			{
				if (item2 is MethodSymbol item)
				{
					instance.Add(item);
				}
			}
			ImmutableArray<MethodSymbol> methods = instance.ToImmutableAndFree();
			return new BoundMethodGroup(node, default, nameString, methods, (methods.Length == 1) ? methods[0] : null, lookupError, BoundMethodGroupFlags.None, boundLeft, lookupKind, hasErrors: true);
		}
		Symbol symbol = ((symbols.Length == 1) ? symbols[0] : null);
		return new BoundBadExpression(node, lookupKind, ((object)symbol == null) ? ImmutableArray<Symbol>.Empty : ImmutableArray.Create(symbol), (boundLeft == null) ? ImmutableArray<BoundExpression>.Empty : ImmutableArray.Create(boundLeft), GetNonMethodMemberType(symbol));
	}

	private TypeSymbol GetNonMethodMemberType(Symbol symbolOpt)
	{
		TypeSymbol typeSymbol = null;
		if ((object)symbolOpt != null)
		{
			switch (symbolOpt.Kind)
			{
			case SymbolKind.Field:
				typeSymbol = ((FieldSymbol)symbolOpt).GetFieldType(FieldsBeingBound);
				break;
			case SymbolKind.Property:
				typeSymbol = ((PropertySymbol)symbolOpt).Type;
				break;
			case SymbolKind.Event:
				typeSymbol = ((EventSymbol)symbolOpt).Type;
				break;
			}
		}
		return typeSymbol ?? CreateErrorType();
	}

	private static void CombineExtensionMethodArguments(BoundExpression receiver, AnalyzedArguments originalArguments, AnalyzedArguments extensionMethodArguments)
	{
		extensionMethodArguments.IsExtensionMethodInvocation = true;
		extensionMethodArguments.Arguments.Add(receiver);
		extensionMethodArguments.Arguments.AddRange(originalArguments.Arguments);
		if (originalArguments.Names.Count > 0)
		{
			extensionMethodArguments.Names.Add(null);
			extensionMethodArguments.Names.AddRange(originalArguments.Names);
		}
		if (originalArguments.RefKinds.Count > 0)
		{
			extensionMethodArguments.RefKinds.Add(RefKind.None);
			extensionMethodArguments.RefKinds.AddRange(originalArguments.RefKinds);
		}
	}

	private BoundExpression BindMemberOfType(SyntaxNode node, SyntaxNode right, string plainName, int arity, bool indexed, BoundExpression left, SeparatedSyntaxList<TypeSyntax> typeArgumentsSyntax, ImmutableArray<TypeSymbol> typeArguments, LookupResult lookupResult, BoundMethodGroupFlags methodGroupFlags, DiagnosticBag diagnostics)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		Symbol symbolOrMethodOrPropertyGroup = GetSymbolOrMethodOrPropertyGroup(lookupResult, right, plainName, arity, instance, diagnostics, out var wasError);
		BoundExpression result;
		if ((object)symbolOrMethodOrPropertyGroup == null)
		{
			result = ConstructBoundMemberGroupAndReportOmittedTypeArguments(node, typeArgumentsSyntax, typeArguments, left, plainName, instance, lookupResult, methodGroupFlags, wasError, diagnostics);
		}
		else
		{
			left = ReplaceTypeOrValueReceiver(left, symbolOrMethodOrPropertyGroup.IsStatic || symbolOrMethodOrPropertyGroup.Kind == SymbolKind.NamedType, diagnostics);
			if (symbolOrMethodOrPropertyGroup.Kind != SymbolKind.Event)
			{
				ReportDiagnosticsIfObsolete(diagnostics, symbolOrMethodOrPropertyGroup, node, left.Kind == BoundKind.BaseReference);
			}
			switch (symbolOrMethodOrPropertyGroup.Kind)
			{
			case SymbolKind.ErrorType:
			case SymbolKind.NamedType:
			{
				if (IsInstanceReceiver(left) == true && !wasError)
				{
					Error(diagnostics, ErrorCode.ERR_BadTypeReference, right, plainName, symbolOrMethodOrPropertyGroup);
				}
				NamedTypeSymbol type = (NamedTypeSymbol)symbolOrMethodOrPropertyGroup;
				if (!typeArguments.IsDefault)
				{
					type = ConstructNamedTypeUnlessTypeArgumentOmitted(right, type, typeArgumentsSyntax, typeArguments, diagnostics);
				}
				result = new BoundTypeExpression(node, null, inferredType: false, left as BoundTypeExpression, type);
				break;
			}
			case SymbolKind.Property:
				result = BindPropertyAccess(node, left, (PropertySymbol)symbolOrMethodOrPropertyGroup, diagnostics, lookupResult.Kind, wasError);
				break;
			case SymbolKind.Event:
				result = BindEventAccess(node, left, (EventSymbol)symbolOrMethodOrPropertyGroup, diagnostics, lookupResult.Kind, wasError);
				break;
			case SymbolKind.Field:
				result = BindFieldAccess(node, left, (FieldSymbol)symbolOrMethodOrPropertyGroup, diagnostics, lookupResult.Kind, indexed, wasError);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbolOrMethodOrPropertyGroup.Kind);
			}
		}
		instance.Free();
		return result;
	}

	private MethodGroupResolution BindExtensionMethod(SyntaxNode expression, string methodName, AnalyzedArguments analyzedArguments, BoundExpression left, ImmutableArray<TypeSymbol> typeArguments, bool isMethodGroupConversion, RefKind returnRefKind, TypeSymbol returnType)
	{
		MethodGroupResolution result = default;
		AnalyzedArguments analyzedArguments2 = null;
		foreach (ExtensionMethodScope item in new ExtensionMethodScopes(this))
		{
			MethodGroup instance = MethodGroup.GetInstance();
			DiagnosticBag instance2 = DiagnosticBag.GetInstance();
			PopulateExtensionMethodsFromSingleBinder(item, instance, expression, left, methodName, typeArguments, instance2);
			if (analyzedArguments == null)
			{
				if (expression == EnclosingNameofArgument)
				{
					for (int num = instance.Methods.Count - 1; num >= 0; num--)
					{
						if ((object)instance.Methods[num].ReduceExtensionMethod(left.Type) == null)
						{
							instance.Methods.RemoveAt(num);
						}
					}
				}
				if (instance.Methods.Count != 0)
				{
					return new MethodGroupResolution(instance, instance2.ToReadOnlyAndFree());
				}
			}
			if (instance.Methods.Count == 0)
			{
				instance.Free();
				instance2.Free();
				continue;
			}
			if (analyzedArguments2 == null)
			{
				analyzedArguments2 = AnalyzedArguments.GetInstance();
				CombineExtensionMethodArguments(left, analyzedArguments, analyzedArguments2);
			}
			OverloadResolutionResult<MethodSymbol> instance3 = OverloadResolutionResult<MethodSymbol>.GetInstance();
			bool allowRefOmittedArguments = instance.Receiver.IsExpressionOfComImportType();
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			OverloadResolution.MethodInvocationOverloadResolution(instance.Methods, instance.TypeArguments, instance.Receiver, analyzedArguments2, instance3, ref useSiteDiagnostics, isMethodGroupConversion, allowRefOmittedArguments, inferWithDynamic: false, allowUnexpandedForm: true, returnRefKind, returnType);
			instance2.Add(expression, useSiteDiagnostics);
			ImmutableArray<Diagnostic> diagnostics = instance2.ToReadOnlyAndFree();
			MethodGroupResolution methodGroupResolution = new MethodGroupResolution(instance, null, instance3, analyzedArguments2, instance.ResultKind, diagnostics);
			if (methodGroupResolution.HasAnyApplicableMethod)
			{
				if (!result.IsEmpty)
				{
					result.MethodGroup.Free();
					result.OverloadResolutionResult.Free();
				}
				return methodGroupResolution;
			}
			if (result.IsEmpty)
			{
				result = methodGroupResolution;
				continue;
			}
			instance3.Free();
			instance.Free();
		}
		return result;
	}

	private void PopulateExtensionMethodsFromSingleBinder(ExtensionMethodScope scope, MethodGroup methodGroup, SyntaxNode node, BoundExpression left, string rightName, ImmutableArray<TypeSymbol> typeArguments, DiagnosticBag diagnostics)
	{
		int arity;
		LookupOptions options;
		if (typeArguments.IsDefault)
		{
			arity = 0;
			options = LookupOptions.AllMethodsOnArityZero;
		}
		else
		{
			arity = typeArguments.Length;
			options = LookupOptions.Default;
		}
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupExtensionMethodsInSingleBinder(scope, instance, rightName, arity, options, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		if (instance.IsMultiViable)
		{
			ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
			GetSymbolOrMethodOrPropertyGroup(instance, node, rightName, arity, instance2, diagnostics, out var _);
			methodGroup.PopulateWithExtensionMethods(left, instance2, typeArguments, instance.Kind);
			instance2.Free();
		}
		instance.Free();
	}

	protected BoundExpression BindFieldAccess(SyntaxNode node, BoundExpression receiver, FieldSymbol fieldSymbol, DiagnosticBag diagnostics, LookupResultKind resultKind, bool indexed, bool hasErrors)
	{
		bool flag = false;
		NamedTypeSymbol containingType = fieldSymbol.ContainingType;
		bool flag2 = fieldSymbol.IsStatic && containingType.IsEnumType();
		if (flag2 && !containingType.IsValidEnumType())
		{
			Error(diagnostics, ErrorCode.ERR_BindToBogus, node, fieldSymbol);
			flag = true;
		}
		if (!flag)
		{
			flag = CheckInstanceOrStatic(node, receiver, fieldSymbol, ref resultKind, diagnostics);
		}
		if (!flag && fieldSymbol.IsFixed && !IsInsideNameof)
		{
			TypeSymbol type = receiver.Type;
			flag = (object)type == null || !type.IsValueType;
			if (!flag)
			{
				bool flag3 = SyntaxFacts.IsFixedStatementExpression(node);
				if (IsMoveableVariable(receiver, out var _) != flag3)
				{
					if (indexed)
					{
						CheckFeatureAvailability(node, MessageID.IDS_FeatureIndexingMovableFixedBuffers, diagnostics);
					}
					else
					{
						Error(diagnostics, flag3 ? ErrorCode.ERR_FixedNotNeeded : ErrorCode.ERR_FixedBufferNotFixed, node);
						hasErrors = (flag = true);
					}
				}
			}
			if (!flag)
			{
				flag = !CheckValueKind(node, receiver, BindValueKind.FixedReceiver, checkingReceiver: false, diagnostics);
			}
		}
		ConstantValue constantValue = null;
		if (fieldSymbol.IsConst && !IsInsideNameof)
		{
			constantValue = fieldSymbol.GetConstantValue(ConstantFieldsInProgress, IsEarlyAttributeBinder);
			if (constantValue == ConstantValue.Unset)
			{
				constantValue = ConstantValue.Bad;
			}
		}
		if (!fieldSymbol.IsStatic)
		{
			WarnOnAccessOfOffDefault(node, receiver, diagnostics);
		}
		TypeSymbol fieldType = fieldSymbol.GetFieldType(FieldsBeingBound);
		BoundExpression boundExpression = new BoundFieldAccess(node, receiver, fieldSymbol, constantValue, resultKind, fieldType, hasErrors | flag);
		if (InEnumMemberInitializer())
		{
			NamedTypeSymbol namedTypeSymbol = null;
			if (flag2)
			{
				namedTypeSymbol = containingType;
			}
			else if (constantValue != null && fieldType.IsEnumType())
			{
				namedTypeSymbol = (NamedTypeSymbol)fieldType;
			}
			if ((object)namedTypeSymbol != null)
			{
				NamedTypeSymbol enumUnderlyingType = namedTypeSymbol.EnumUnderlyingType;
				boundExpression = new BoundConversion(node, boundExpression, Conversion.ImplicitNumeric, @checked: true, explicitCastInCode: false, boundExpression.ConstantValue, enumUnderlyingType);
			}
		}
		return boundExpression;
	}

	private bool InEnumMemberInitializer()
	{
		NamedTypeSymbol containingType = ContainingType;
		if (InFieldInitializer && (object)containingType != null)
		{
			return containingType.IsEnumType();
		}
		return false;
	}

	private BoundExpression BindPropertyAccess(SyntaxNode node, BoundExpression receiver, PropertySymbol propertySymbol, DiagnosticBag diagnostics, LookupResultKind lookupResult, bool hasErrors)
	{
		bool flag = CheckInstanceOrStatic(node, receiver, propertySymbol, ref lookupResult, diagnostics);
		if (!propertySymbol.IsStatic)
		{
			WarnOnAccessOfOffDefault(node, receiver, diagnostics);
		}
		return new BoundPropertyAccess(node, receiver, propertySymbol, lookupResult, propertySymbol.Type, hasErrors | flag);
	}

	private BoundExpression BindEventAccess(SyntaxNode node, BoundExpression receiver, EventSymbol eventSymbol, DiagnosticBag diagnostics, LookupResultKind lookupResult, bool hasErrors)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		bool isUsableAsField = eventSymbol.HasAssociatedField && IsAccessible(eventSymbol.AssociatedField, ref useSiteDiagnostics, receiver?.Type);
		diagnostics.Add(node, useSiteDiagnostics);
		bool flag = CheckInstanceOrStatic(node, receiver, eventSymbol, ref lookupResult, diagnostics);
		if (!eventSymbol.IsStatic)
		{
			WarnOnAccessOfOffDefault(node, receiver, diagnostics);
		}
		return new BoundEventAccess(node, receiver, eventSymbol, isUsableAsField, lookupResult, eventSymbol.Type, hasErrors | flag);
	}

	private static bool? IsInstanceReceiver(BoundExpression receiver)
	{
		if (receiver == null)
		{
			return false;
		}
		return receiver.Kind switch
		{
			BoundKind.PreviousSubmissionReference => null, 
			BoundKind.TypeExpression => false, 
			BoundKind.QueryClause => IsInstanceReceiver(((BoundQueryClause)receiver).Value), 
			_ => true, 
		};
	}

	private bool CheckInstanceOrStatic(SyntaxNode node, BoundExpression receiver, Symbol symbol, ref LookupResultKind resultKind, DiagnosticBag diagnostics)
	{
		bool? flag = IsInstanceReceiver(receiver);
		if (symbol.IsStatic)
		{
			if (flag == true)
			{
				ErrorCode code = (Flags.Includes(BinderFlags.ObjectInitializerMember) ? ErrorCode.ERR_StaticMemberInObjectInitializer : ErrorCode.ERR_ObjectProhibited);
				Error(diagnostics, code, node, symbol);
				resultKind = LookupResultKind.StaticInstanceMismatch;
				return true;
			}
		}
		else if (flag == false && !IsInsideNameof)
		{
			Error(diagnostics, ErrorCode.ERR_ObjectRequired, node, symbol);
			resultKind = LookupResultKind.StaticInstanceMismatch;
			return true;
		}
		return false;
	}

	private Symbol GetSymbolOrMethodOrPropertyGroup(LookupResult result, SyntaxNode node, string plainName, int arity, ArrayBuilder<Symbol> methodOrPropertyGroup, DiagnosticBag diagnostics, out bool wasError)
	{
		node = GetNameSyntax(node) ?? node;
		wasError = false;
		Symbol symbol = null;
		foreach (Symbol symbol2 in result.Symbols)
		{
			SymbolKind kind = symbol2.Kind;
			if (methodOrPropertyGroup.Count > 0)
			{
				SymbolKind kind2 = methodOrPropertyGroup[0].Kind;
				if (kind2 != kind)
				{
					if (kind2 == SymbolKind.Method || (kind2 == SymbolKind.Property && kind != SymbolKind.Method))
					{
						symbol = symbol2;
						continue;
					}
					symbol = methodOrPropertyGroup[0];
					methodOrPropertyGroup.Clear();
				}
			}
			if (kind == SymbolKind.Method || kind == SymbolKind.Property)
			{
				methodOrPropertyGroup.Add(symbol2);
			}
			else
			{
				symbol = symbol2;
			}
		}
		if (methodOrPropertyGroup.Count > 0 && IsMethodOrPropertyGroup(methodOrPropertyGroup) && (methodOrPropertyGroup[0].Kind == SymbolKind.Method || (object)symbol == null))
		{
			if (result.Error != null)
			{
				Error(diagnostics, result.Error, node);
				wasError = result.Error.Severity == DiagnosticSeverity.Error;
			}
			return null;
		}
		methodOrPropertyGroup.Clear();
		return ResultSymbol(result, plainName, arity, node, diagnostics, suppressUseSiteDiagnostics: false, out wasError);
	}

	private static bool IsMethodOrPropertyGroup(ArrayBuilder<Symbol> members)
	{
		Symbol symbol = members[0];
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return true;
		case SymbolKind.Property:
			foreach (PropertySymbol member in members)
			{
				if (member.IsIndexedProperty)
				{
					return true;
				}
			}
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	private BoundExpression BindElementAccess(ElementAccessExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression receiver = BindExpression(node.Expression, diagnostics, invoked: false, indexed: true);
		return BindElementAccess(node, receiver, node.ArgumentList, diagnostics);
	}

	private BoundExpression BindElementAccess(ExpressionSyntax node, BoundExpression receiver, BracketedArgumentListSyntax argumentList, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		try
		{
			BindArgumentsAndNames(argumentList, diagnostics, instance);
			if (receiver.Kind == BoundKind.PropertyGroup)
			{
				BoundPropertyGroup boundPropertyGroup = (BoundPropertyGroup)receiver;
				return BindIndexedPropertyAccess(node, boundPropertyGroup.ReceiverOpt, boundPropertyGroup.Properties, instance, diagnostics);
			}
			receiver = CheckValue(receiver, BindValueKind.RValue, diagnostics);
			return BindElementOrIndexerAccess(node, receiver, instance, diagnostics);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression BindElementOrIndexerAccess(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		if ((object)expr.Type == null)
		{
			return BadIndexerExpression(node, expr, analyzedArguments, null, diagnostics);
		}
		WarnOnAccessOfOffDefault(node, expr, diagnostics);
		if (analyzedArguments.HasErrors || expr.HasAnyErrors)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression result = BindElementAccessCore(node, expr, analyzedArguments, instance);
			instance.Free();
			return result;
		}
		return BindElementAccessCore(node, expr, analyzedArguments, diagnostics);
	}

	private BoundExpression BadIndexerExpression(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments analyzedArguments, DiagnosticInfo errorOpt, DiagnosticBag diagnostics)
	{
		if (!expr.HasAnyErrors)
		{
			diagnostics.Add(errorOpt ?? new CSDiagnosticInfo(ErrorCode.ERR_BadIndexLHS, expr.Display), node.Location);
		}
		ImmutableArray<BoundExpression> childBoundNodes = BuildArgumentsForErrorRecovery(analyzedArguments).Add(expr);
		return new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, childBoundNodes, CreateErrorType(), hasErrors: true);
	}

	private BoundExpression BindElementAccessCore(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		switch (expr.Type.TypeKind)
		{
		case TypeKind.Array:
			return BindArrayAccess(node, expr, arguments, diagnostics);
		case TypeKind.Dynamic:
			return BindDynamicIndexer(node, expr, arguments, ImmutableArray<PropertySymbol>.Empty, diagnostics);
		case TypeKind.Pointer:
			return BindPointerElementAccess(node, expr, arguments, diagnostics);
		case TypeKind.Class:
		case TypeKind.Interface:
		case TypeKind.Struct:
		case TypeKind.TypeParameter:
			return BindIndexerAccess(node, expr, arguments, diagnostics);
		default:
			return BadIndexerExpression(node, expr, arguments, null, diagnostics);
		}
	}

	private BoundExpression BindArrayAccess(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		if (arguments.Names.Count > 0)
		{
			Error(diagnostics, ErrorCode.ERR_NamedArgumentForArray, node);
		}
		bool hasErrors = ReportRefOrOutArgument(arguments, diagnostics);
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)expr.Type;
		int rank = arrayTypeSymbol.Rank;
		if (arguments.Arguments.Count != rank)
		{
			Error(diagnostics, ErrorCode.ERR_BadIndexCount, node, rank);
			return new BoundArrayAccess(node, expr, BuildArgumentsForErrorRecovery(arguments), arrayTypeSymbol.ElementType, hasErrors: true);
		}
		BoundExpression[] array = new BoundExpression[arguments.Arguments.Count];
		for (int i = 0; i < arguments.Arguments.Count; i++)
		{
			BoundExpression index = arguments.Arguments[i];
			BoundExpression boundExpression = (array[i] = ConvertToArrayIndex(index, node, diagnostics));
			if (rank == 1 && !boundExpression.HasAnyErrors)
			{
				ConstantValue constantValue = boundExpression.ConstantValue;
				if (constantValue != null && constantValue.IsNegativeNumeric)
				{
					Error(diagnostics, ErrorCode.WRN_NegativeArrayIndex, boundExpression.Syntax);
				}
			}
		}
		return new BoundArrayAccess(node, expr, array.AsImmutableOrNull(), arrayTypeSymbol.ElementType, hasErrors);
	}

	private BoundExpression ConvertToArrayIndex(BoundExpression index, SyntaxNode node, DiagnosticBag diagnostics)
	{
		if (index.Kind == BoundKind.OutVariablePendingInference)
		{
			return ((OutVariablePendingInference)index).FailInference(this, diagnostics);
		}
		if (index.Kind == BoundKind.DiscardExpression && !index.HasExpressionType())
		{
			return ((BoundDiscardExpression)index).FailInference(this, diagnostics);
		}
		BoundExpression boundExpression = TryImplicitConversionToArrayIndex(index, SpecialType.System_Int32, node, diagnostics) ?? TryImplicitConversionToArrayIndex(index, SpecialType.System_UInt32, node, diagnostics) ?? TryImplicitConversionToArrayIndex(index, SpecialType.System_Int64, node, diagnostics) ?? TryImplicitConversionToArrayIndex(index, SpecialType.System_UInt64, node, diagnostics);
		if (boundExpression == null)
		{
			NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Int32, diagnostics, node);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion conversion = Conversions.ClassifyConversionFromExpression(index, specialType, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			GenerateImplicitConversionError(diagnostics, node, conversion, index, specialType);
			return CreateConversion(index.Syntax, index, conversion, isCast: false, specialType, new DiagnosticBag());
		}
		return boundExpression;
	}

	private BoundExpression TryImplicitConversionToArrayIndex(BoundExpression expr, SpecialType specialType, SyntaxNode node, DiagnosticBag diagnostics)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		TypeSymbol specialType2 = GetSpecialType(specialType, instance, node);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyImplicitConversionFromExpression(expr, specialType2, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		if (!conversion.Exists)
		{
			instance.Free();
			return null;
		}
		if (conversion.IsDynamic)
		{
			conversion = conversion.SetArrayIndexConversionForDynamic();
		}
		BoundExpression result = CreateConversion(expr.Syntax, expr, conversion, isCast: false, specialType2, instance);
		diagnostics.AddRange(instance);
		instance.Free();
		return result;
	}

	private BoundExpression BindPointerElementAccess(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		bool flag = false;
		if (analyzedArguments.Names.Count > 0)
		{
			Error(diagnostics, ErrorCode.ERR_NamedArgumentForArray, node);
			flag = true;
		}
		flag = flag || ReportRefOrOutArgument(analyzedArguments, diagnostics);
		TypeSymbol pointedAtType = ((PointerTypeSymbol)expr.Type).PointedAtType;
		ArrayBuilder<BoundExpression> arguments = analyzedArguments.Arguments;
		if (arguments.Count != 1)
		{
			if (!flag)
			{
				Error(diagnostics, ErrorCode.ERR_PtrIndexSingle, node);
			}
			return new BoundPointerElementAccess(node, expr, BadExpression(node, BuildArgumentsForErrorRecovery(analyzedArguments)).MakeCompilerGenerated(), CheckOverflowAtRuntime, pointedAtType, hasErrors: true);
		}
		if (pointedAtType.SpecialType == SpecialType.System_Void)
		{
			Error(diagnostics, ErrorCode.ERR_VoidError, expr.Syntax);
			flag = true;
		}
		BoundExpression boundExpression = arguments[0];
		boundExpression = ConvertToArrayIndex(boundExpression, boundExpression.Syntax, diagnostics);
		return new BoundPointerElementAccess(node, expr, boundExpression, CheckOverflowAtRuntime, pointedAtType, flag);
	}

	private static bool ReportRefOrOutArgument(AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		int count = analyzedArguments.Arguments.Count;
		for (int i = 0; i < count; i++)
		{
			RefKind refKind = analyzedArguments.RefKind(i);
			if (refKind != RefKind.None)
			{
				Error(diagnostics, ErrorCode.ERR_BadArgExtraRef, analyzedArguments.Argument(i).Syntax, i + 1, refKind.ToArgumentDisplayString());
				return true;
			}
		}
		return false;
	}

	private BoundExpression BindIndexerAccess(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		LookupResult instance = LookupResult.GetInstance();
		LookupOptions options = ((expr.Kind == BoundKind.BaseReference) ? LookupOptions.UseBaseReferenceAccessibility : LookupOptions.Default);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupMembersWithFallback(instance, expr.Type, "this[]", 0, ref useSiteDiagnostics, null, options);
		diagnostics.Add(node, useSiteDiagnostics);
		BoundExpression result;
		if (!instance.IsMultiViable)
		{
			result = BadIndexerExpression(node, expr, analyzedArguments, instance.Error, diagnostics);
		}
		else
		{
			ArrayBuilder<PropertySymbol> instance2 = ArrayBuilder<PropertySymbol>.GetInstance();
			foreach (Symbol symbol in instance.Symbols)
			{
				instance2.Add((PropertySymbol)symbol);
			}
			result = BindIndexerOrIndexedPropertyAccess(node, expr, instance2, analyzedArguments, diagnostics);
			instance2.Free();
		}
		instance.Free();
		return result;
	}

	private BoundExpression BindIndexedPropertyAccess(BoundPropertyGroup propertyGroup, bool mustHaveAllOptionalParameters, DiagnosticBag diagnostics)
	{
		SyntaxNode syntax = propertyGroup.Syntax;
		BoundExpression receiverOpt = propertyGroup.ReceiverOpt;
		ImmutableArray<PropertySymbol> properties = propertyGroup.Properties;
		if (properties.All(s_isIndexedPropertyWithNonOptionalArguments))
		{
			Error(diagnostics, mustHaveAllOptionalParameters ? ErrorCode.ERR_IndexedPropertyMustHaveAllOptionalParams : ErrorCode.ERR_IndexedPropertyRequiresParams, syntax, properties[0].ToDisplayString(s_propertyGroupFormat));
			return BoundIndexerAccess.ErrorAccess(syntax, receiverOpt, CreateErrorPropertySymbol(properties), ImmutableArray<BoundExpression>.Empty, default, default, properties);
		}
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		BoundExpression result = BindIndexedPropertyAccess(syntax, receiverOpt, properties, instance, diagnostics);
		instance.Free();
		return result;
	}

	private BoundExpression BindIndexedPropertyAccess(SyntaxNode syntax, BoundExpression receiverOpt, ImmutableArray<PropertySymbol> propertyGroup, AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance();
		instance.AddRange(propertyGroup);
		BoundExpression result = BindIndexerOrIndexedPropertyAccess(syntax, receiverOpt, instance, arguments, diagnostics);
		instance.Free();
		return result;
	}

	private BoundExpression BindDynamicIndexer(SyntaxNode syntax, BoundExpression receiverOpt, AnalyzedArguments arguments, ImmutableArray<PropertySymbol> applicableProperties, DiagnosticBag diagnostics)
	{
		bool flag = false;
		if (receiverOpt != null)
		{
			switch (receiverOpt.Kind)
			{
			case BoundKind.BaseReference:
				Error(diagnostics, ErrorCode.ERR_NoDynamicPhantomOnBaseIndexer, syntax);
				flag = true;
				break;
			case BoundKind.TypeOrValueExpression:
			{
				BoundTypeOrValueExpression boundTypeOrValueExpression = (BoundTypeOrValueExpression)receiverOpt;
				bool useType = IsInstance(boundTypeOrValueExpression.Data.ValueSymbol) && !HasThis(isExplicit: false, out var _);
				receiverOpt = ReplaceTypeOrValueReceiver(boundTypeOrValueExpression, useType, diagnostics);
				break;
			}
			}
		}
		ImmutableArray<BoundExpression> arguments2 = BuildArgumentsForDynamicInvocation(arguments, diagnostics);
		ImmutableArray<RefKind> immutableArray = arguments.RefKinds.ToImmutableOrNull();
		flag &= ReportBadDynamicArguments(syntax, arguments2, immutableArray, diagnostics, null);
		return new BoundDynamicIndexerAccess(syntax, receiverOpt, arguments2, arguments.GetNames(), immutableArray, applicableProperties, AssemblySymbol.DynamicType, flag);
	}

	private BoundExpression BindIndexerOrIndexedPropertyAccess(SyntaxNode syntax, BoundExpression receiverOpt, ArrayBuilder<PropertySymbol> propertyGroup, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		OverloadResolutionResult<PropertySymbol> instance = OverloadResolutionResult<PropertySymbol>.GetInstance();
		bool allowRefOmittedArguments = receiverOpt.IsExpressionOfComImportType();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		OverloadResolution.PropertyOverloadResolution(propertyGroup, receiverOpt, analyzedArguments, instance, allowRefOmittedArguments, ref useSiteDiagnostics);
		diagnostics.Add(syntax, useSiteDiagnostics);
		if (analyzedArguments.HasDynamicArgument && instance.HasAnyApplicableMember)
		{
			ImmutableArray<PropertySymbol> candidatesPassingFinalValidation = GetCandidatesPassingFinalValidation(syntax, instance, receiverOpt, default, diagnostics);
			instance.Free();
			return BindDynamicIndexer(syntax, receiverOpt, analyzedArguments, candidatesPassingFinalValidation, diagnostics);
		}
		ImmutableArray<string> names = analyzedArguments.GetNames();
		ImmutableArray<RefKind> immutableArray = analyzedArguments.RefKinds.ToImmutableOrNull();
		BoundExpression result;
		if (!instance.Succeeded)
		{
			ImmutableArray<PropertySymbol> immutableArray2 = propertyGroup.ToImmutable();
			if (!analyzedArguments.HasErrors)
			{
				PropertySymbol propertySymbol = immutableArray2[0];
				string name = (propertySymbol.IsIndexer ? SyntaxFacts.GetText(SyntaxKind.ThisKeyword) : propertySymbol.Name);
				instance.ReportDiagnostics(this, syntax.Location, syntax, diagnostics, name, null, null, analyzedArguments, immutableArray2, null, null);
			}
			ImmutableArray<BoundExpression> arguments = BuildArgumentsForErrorRecovery(analyzedArguments, immutableArray2);
			PropertySymbol indexer = ((immutableArray2.Length == 1) ? immutableArray2[0] : CreateErrorPropertySymbol(immutableArray2));
			result = BoundIndexerAccess.ErrorAccess(syntax, receiverOpt, indexer, arguments, names, immutableArray, immutableArray2);
		}
		else
		{
			MemberResolutionResult<PropertySymbol> validResult = instance.ValidResult;
			PropertySymbol member = validResult.Member;
			CoerceArguments(validResult, analyzedArguments.Arguments, diagnostics);
			bool expanded = validResult.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm;
			ImmutableArray<int> argsToParamsOpt = validResult.Result.ArgsToParamsOpt;
			ReportDiagnosticsIfObsolete(diagnostics, member, syntax, receiverOpt != null && receiverOpt.Kind == BoundKind.BaseReference);
			bool flag = MemberGroupFinalValidationAccessibilityChecks(receiverOpt, member, syntax, diagnostics, invokedAsExtensionMethod: false);
			BoundExpression boundExpression = ReplaceTypeOrValueReceiver(receiverOpt, member.IsStatic, diagnostics);
			if (!flag && boundExpression != null && boundExpression.Kind == BoundKind.ThisReference && boundExpression.WasCompilerGenerated)
			{
				flag = IsRefOrOutThisParameterCaptured(syntax, diagnostics);
			}
			ImmutableArray<BoundExpression> immutableArray3 = analyzedArguments.Arguments.ToImmutable();
			if (!flag)
			{
				flag = !CheckInvocationArgMixing(syntax, member, boundExpression, member.Parameters, immutableArray3, argsToParamsOpt, LocalScopeDepth, diagnostics);
			}
			result = new BoundIndexerAccess(syntax, boundExpression, member, immutableArray3, names, immutableArray, expanded, argsToParamsOpt, this, useSetterForDefaultArgumentGeneration: false, member.Type, flag);
		}
		instance.Free();
		return result;
	}

	private ErrorPropertySymbol CreateErrorPropertySymbol(ImmutableArray<PropertySymbol> propertyGroup)
	{
		TypeSymbol type = GetCommonTypeOrReturnType(propertyGroup) ?? CreateErrorType();
		PropertySymbol propertySymbol = propertyGroup[0];
		return new ErrorPropertySymbol(propertySymbol.ContainingType, type, propertySymbol.Name, propertySymbol.IsIndexer, propertySymbol.IsIndexedProperty);
	}

	internal MethodGroupResolution ResolveMethodGroup(BoundMethodGroup node, AnalyzedArguments analyzedArguments, bool isMethodGroupConversion, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool inferWithDynamic = false, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null)
	{
		return ResolveMethodGroup(node, node.Syntax, node.Name, analyzedArguments, isMethodGroupConversion, ref useSiteDiagnostics, inferWithDynamic, allowUnexpandedForm: true, returnRefKind, returnType);
	}

	internal MethodGroupResolution ResolveMethodGroup(BoundMethodGroup node, SyntaxNode expression, string methodName, AnalyzedArguments analyzedArguments, bool isMethodGroupConversion, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool inferWithDynamic = false, bool allowUnexpandedForm = true, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null)
	{
		MethodGroupResolution result = ResolveMethodGroupInternal(node, expression, methodName, analyzedArguments, isMethodGroupConversion, ref useSiteDiagnostics, inferWithDynamic, allowUnexpandedForm, returnRefKind, returnType);
		if (result.IsEmpty && !result.HasAnyErrors)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			instance.AddRange(result.Diagnostics);
			BindMemberAccessReportError(node, instance);
			return new MethodGroupResolution(result.MethodGroup, result.OtherSymbol, result.OverloadResolutionResult, result.AnalyzedArguments, result.ResultKind, instance.ToReadOnlyAndFree());
		}
		return result;
	}

	private MethodGroupResolution ResolveMethodGroupInternal(BoundMethodGroup methodGroup, SyntaxNode expression, string methodName, AnalyzedArguments analyzedArguments, bool isMethodGroupConversion, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool inferWithDynamic = false, bool allowUnexpandedForm = true, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null)
	{
		MethodGroupResolution result = ResolveDefaultMethodGroup(methodGroup, analyzedArguments, isMethodGroupConversion, ref useSiteDiagnostics, inferWithDynamic, allowUnexpandedForm, returnRefKind, returnType);
		if (!methodGroup.SearchExtensionMethods || result.HasAnyApplicableMethod || methodGroup.MethodGroupReceiverIsDynamic())
		{
			return result;
		}
		MethodGroupResolution result2 = BindExtensionMethod(expression, methodName, analyzedArguments, methodGroup.ReceiverOpt, methodGroup.TypeArgumentsOpt, isMethodGroupConversion, returnRefKind, returnType);
		bool flag = false;
		if (result2.HasAnyApplicableMethod)
		{
			flag = true;
		}
		else if (result2.IsEmpty)
		{
			flag = false;
		}
		else if (result.IsEmpty)
		{
			flag = true;
		}
		else
		{
			LookupResultKind resultKind = result.ResultKind;
			LookupResultKind resultKind2 = result2.ResultKind;
			if (resultKind != resultKind2 && resultKind == resultKind2.WorseResultKind(resultKind))
			{
				flag = true;
			}
		}
		if (flag)
		{
			result.Free();
			return result2;
		}
		result2.Free();
		return result;
	}

	private MethodGroupResolution ResolveDefaultMethodGroup(BoundMethodGroup node, AnalyzedArguments analyzedArguments, bool isMethodGroupConversion, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool inferWithDynamic = false, bool allowUnexpandedForm = true, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null)
	{
		ImmutableArray<MethodSymbol> methods = node.Methods;
		if (methods.Length == 0 && node.LookupSymbolOpt is MethodSymbol item)
		{
			methods = ImmutableArray.Create(item);
		}
		ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;
		if (node.LookupError != null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			Error(instance, node.LookupError, node.NameSyntax);
			diagnostics = instance.ToReadOnlyAndFree();
		}
		if (methods.Length == 0)
		{
			return new MethodGroupResolution(node.LookupSymbolOpt, node.ResultKind, diagnostics);
		}
		MethodGroup instance2 = MethodGroup.GetInstance();
		instance2.PopulateWithNonExtensionMethods(node.ReceiverOpt, methods, node.TypeArgumentsOpt, node.ResultKind, node.LookupError);
		if (node.LookupError != null)
		{
			return new MethodGroupResolution(instance2, diagnostics);
		}
		if (analyzedArguments == null)
		{
			return new MethodGroupResolution(instance2, diagnostics);
		}
		OverloadResolutionResult<MethodSymbol> instance3 = OverloadResolutionResult<MethodSymbol>.GetInstance();
		bool allowRefOmittedArguments = instance2.Receiver.IsExpressionOfComImportType();
		OverloadResolution.MethodInvocationOverloadResolution(instance2.Methods, instance2.TypeArguments, instance2.Receiver, analyzedArguments, instance3, ref useSiteDiagnostics, isMethodGroupConversion, allowRefOmittedArguments, inferWithDynamic, allowUnexpandedForm, returnRefKind, returnType);
		return new MethodGroupResolution(instance2, null, instance3, analyzedArguments, instance2.ResultKind, diagnostics);
	}

	internal static bool ReportDelegateInvokeUseSiteDiagnostic(DiagnosticBag diagnostics, TypeSymbol possibleDelegateType, Location location = null, SyntaxNode node = null)
	{
		if (!possibleDelegateType.IsDelegateType())
		{
			return false;
		}
		MethodSymbol methodSymbol = possibleDelegateType.DelegateInvokeMethod();
		if ((object)methodSymbol == null)
		{
			diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_InvalidDelegateType, possibleDelegateType), location ?? node.Location);
			return true;
		}
		DiagnosticInfo useSiteDiagnostic = methodSymbol.GetUseSiteDiagnostic();
		if (useSiteDiagnostic == null)
		{
			return false;
		}
		if (location == null)
		{
			location = node.Location;
		}
		if (useSiteDiagnostic.Code == 7024)
		{
			diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_InvalidDelegateType, possibleDelegateType), location));
			return true;
		}
		return Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, location);
	}

	private BoundConditionalAccess BindConditionalAccessExpression(ConditionalAccessExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindConditionalAccessReceiver(node, diagnostics);
		BoundExpression boundExpression2 = new BinderWithConditionalReceiver(this, boundExpression).BindValue(node.WhenNotNull, diagnostics, BindValueKind.RValue);
		if (boundExpression.HasAnyErrors || boundExpression2.HasAnyErrors)
		{
			return new BoundConditionalAccess(node, boundExpression, boundExpression2, CreateErrorType(), hasErrors: true);
		}
		_ = boundExpression.Type;
		if (boundExpression2.Kind == BoundKind.MethodGroup)
		{
			return GenerateBadConditionalAccessNodeError(node, boundExpression, boundExpression2, diagnostics);
		}
		TypeSymbol typeSymbol = boundExpression2.Type;
		if ((object)typeSymbol == null)
		{
			return GenerateBadConditionalAccessNodeError(node, boundExpression, boundExpression2, diagnostics);
		}
		if ((!typeSymbol.IsReferenceType && !typeSymbol.IsValueType) || typeSymbol.IsPointerType() || typeSymbol.IsRestrictedType())
		{
			bool flag = true;
			CSharpSyntaxNode parent = node.Parent;
			if (parent != null)
			{
				switch (parent.Kind())
				{
				case SyntaxKind.ExpressionStatement:
					flag = ((ExpressionStatementSyntax)parent).Expression != node;
					break;
				case SyntaxKind.SimpleLambdaExpression:
					flag = ((SimpleLambdaExpressionSyntax)parent).Body != node || ContainingMethodOrLambdaRequiresValue();
					break;
				case SyntaxKind.ParenthesizedLambdaExpression:
					flag = ((ParenthesizedLambdaExpressionSyntax)parent).Body != node || ContainingMethodOrLambdaRequiresValue();
					break;
				case SyntaxKind.ArrowExpressionClause:
					flag = ((ArrowExpressionClauseSyntax)parent).Expression != node || ContainingMethodOrLambdaRequiresValue();
					break;
				case SyntaxKind.ForStatement:
				{
					ForStatementSyntax forStatementSyntax = (ForStatementSyntax)parent;
					flag = !forStatementSyntax.Incrementors.Contains(node) && !forStatementSyntax.Initializers.Contains(node);
					break;
				}
				}
			}
			if (flag)
			{
				return GenerateBadConditionalAccessNodeError(node, boundExpression, boundExpression2, diagnostics);
			}
			typeSymbol = GetSpecialType(SpecialType.System_Void, diagnostics, node);
		}
		if (typeSymbol.IsValueType && !typeSymbol.IsNullableType() && typeSymbol.SpecialType != SpecialType.System_Void)
		{
			typeSymbol = GetSpecialType(SpecialType.System_Nullable_T, diagnostics, node).Construct(typeSymbol);
		}
		return new BoundConditionalAccess(node, boundExpression, boundExpression2, typeSymbol);
	}

	private bool ContainingMethodOrLambdaRequiresValue()
	{
		if (ContainingMemberOrLambda is MethodSymbol methodSymbol)
		{
			if (!methodSymbol.ReturnsVoid)
			{
				return !methodSymbol.IsTaskReturningAsync(Compilation);
			}
			return false;
		}
		return true;
	}

	private BoundConditionalAccess GenerateBadConditionalAccessNodeError(ConditionalAccessExpressionSyntax node, BoundExpression receiver, BoundExpression access, DiagnosticBag diagnostics)
	{
		SyntaxToken operatorToken = node.OperatorToken;
		DiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.ERR_BadUnaryOp, SyntaxFacts.GetText(operatorToken.Kind()), access.Display);
		diagnostics.Add(new CSDiagnostic(info, operatorToken.GetLocation()));
		access = BadExpression(access.Syntax, access);
		return new BoundConditionalAccess(node, receiver, access, CreateErrorType(), hasErrors: true);
	}

	private BoundExpression BindMemberBindingExpression(MemberBindingExpressionSyntax node, bool invoked, bool indexed, DiagnosticBag diagnostics)
	{
		BoundExpression receiverForConditionalBinding = GetReceiverForConditionalBinding(node, diagnostics);
		return BindMemberAccessWithBoundLeft(node, receiverForConditionalBinding, node.Name, node.OperatorToken, invoked, indexed, diagnostics);
	}

	private BoundExpression BindElementBindingExpression(ElementBindingExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression receiverForConditionalBinding = GetReceiverForConditionalBinding(node, diagnostics);
		return BindElementAccess(node, receiverForConditionalBinding, node.ArgumentList, diagnostics);
	}

	private static CSharpSyntaxNode GetConditionalReceiverSyntax(ConditionalAccessExpressionSyntax node)
	{
		ExpressionSyntax expression = node.Expression;
		while (expression.IsKind(SyntaxKind.ParenthesizedExpression))
		{
			expression = ((ParenthesizedExpressionSyntax)expression).Expression;
		}
		return expression;
	}

	private BoundExpression GetReceiverForConditionalBinding(ExpressionSyntax binding, DiagnosticBag diagnostics)
	{
		ConditionalAccessExpressionSyntax node = SyntaxFactory.FindConditionalAccessNodeForBinding(binding);
		BoundExpression boundExpression = ConditionalReceiverExpression;
		if (boundExpression?.Syntax != GetConditionalReceiverSyntax(node))
		{
			boundExpression = BindConditionalAccessReceiver(node, diagnostics);
		}
		TypeSymbol typeSymbol = boundExpression.Type;
		if ((object)typeSymbol != null && typeSymbol.IsNullableType())
		{
			typeSymbol = typeSymbol.GetNullableUnderlyingType();
		}
		return new BoundConditionalReceiver(boundExpression.Syntax, 0, typeSymbol ?? CreateErrorType(), boundExpression.HasErrors)
		{
			WasCompilerGenerated = true
		};
	}

	private BoundExpression BindConditionalAccessReceiver(ConditionalAccessExpressionSyntax node, DiagnosticBag diagnostics)
	{
		ExpressionSyntax expression = node.Expression;
		BoundExpression expr = BindValue(expression, diagnostics, BindValueKind.RValue);
		expr = MakeMemberAccessValue(expr, diagnostics);
		if (expr.HasAnyErrors)
		{
			return expr;
		}
		SyntaxToken operatorToken = node.OperatorToken;
		if (expr.Kind == BoundKind.UnboundLambda)
		{
			MessageID messageID = ((UnboundLambda)expr).MessageID;
			DiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.ERR_BadUnaryOp, SyntaxFacts.GetText(operatorToken.Kind()), messageID.Localize());
			diagnostics.Add(new CSDiagnostic(info, node.Location));
			return BadExpression(expression, expr);
		}
		TypeSymbol type = expr.Type;
		if ((object)type == null)
		{
			Error(diagnostics, ErrorCode.ERR_BadUnaryOp, operatorToken.GetLocation(), operatorToken.Text, expr.Display);
			return BadExpression(expression, expr);
		}
		if (type.SpecialType == SpecialType.System_Void)
		{
			Error(diagnostics, ErrorCode.ERR_BadUnaryOp, operatorToken.GetLocation(), operatorToken.Text, type);
			return BadExpression(expression, expr);
		}
		if (type.IsValueType && !type.IsNullableType())
		{
			Error(diagnostics, ErrorCode.ERR_BadUnaryOp, operatorToken.GetLocation(), operatorToken.Text, type);
			return BadExpression(expression, expr);
		}
		return expr;
	}

	internal Binder WithFlags(BinderFlags flags)
	{
		if (Flags != flags)
		{
			return new Binder(this, flags);
		}
		return this;
	}

	internal Binder WithAdditionalFlags(BinderFlags flags)
	{
		if (!Flags.Includes(flags))
		{
			return new Binder(this, Flags | flags);
		}
		return this;
	}

	internal Binder WithContainingMemberOrLambda(Symbol containing)
	{
		return new BinderWithContainingMemberOrLambda(this, containing);
	}

	internal Binder WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags flags, Symbol containing)
	{
		return new BinderWithContainingMemberOrLambda(this, Flags | flags, containing);
	}

	internal Binder WithUnsafeRegionIfNecessary(SyntaxTokenList modifiers)
	{
		if (!Flags.Includes(BinderFlags.UnsafeRegion) && modifiers.Any(SyntaxKind.UnsafeKeyword))
		{
			return new Binder(this, Flags | BinderFlags.UnsafeRegion);
		}
		return this;
	}

	internal Binder WithCheckedOrUncheckedRegion(bool @checked)
	{
		BinderFlags binderFlags = (@checked ? BinderFlags.CheckedRegion : BinderFlags.UncheckedRegion);
		BinderFlags binderFlags2 = (@checked ? BinderFlags.UncheckedRegion : BinderFlags.CheckedRegion);
		if (!Flags.Includes(binderFlags))
		{
			return new Binder(this, (Flags & ~binderFlags2) | binderFlags);
		}
		return this;
	}

	internal static void BindFieldInitializers(CSharpCompilation compilation, SynthesizedInteractiveInitializerMethod scriptInitializerOpt, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> fieldInitializers, DiagnosticBag diagnostics, ref ProcessedFieldInitializers processedInitializers)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		processedInitializers.BoundInitializers = BindFieldInitializers(compilation, scriptInitializerOpt, fieldInitializers, instance, out var firstImportChain);
		processedInitializers.HasErrors = instance.HasAnyErrors();
		processedInitializers.FirstImportChain = firstImportChain;
		diagnostics.AddRange(instance);
		instance.Free();
	}

	private static ImmutableArray<BoundInitializer> BindFieldInitializers(CSharpCompilation compilation, SynthesizedInteractiveInitializerMethod scriptInitializerOpt, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers, DiagnosticBag diagnostics, out ImportChain firstImportChain)
	{
		if (initializers.IsEmpty)
		{
			firstImportChain = null;
			return ImmutableArray<BoundInitializer>.Empty;
		}
		ArrayBuilder<BoundInitializer> instance = ArrayBuilder<BoundInitializer>.GetInstance();
		if ((object)scriptInitializerOpt == null)
		{
			BindRegularCSharpFieldInitializers(compilation, initializers, instance, diagnostics, out firstImportChain);
		}
		else
		{
			BindScriptFieldInitializers(compilation, scriptInitializerOpt, initializers, instance, diagnostics, out firstImportChain);
		}
		return instance.ToImmutableAndFree();
	}

	internal static void BindRegularCSharpFieldInitializers(CSharpCompilation compilation, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers, ArrayBuilder<BoundInitializer> boundInitializers, DiagnosticBag diagnostics, out ImportChain firstDebugImports)
	{
		firstDebugImports = null;
		foreach (ImmutableArray<FieldOrPropertyInitializer> item2 in initializers)
		{
			BinderFactory binderFactory = null;
			foreach (FieldOrPropertyInitializer item3 in item2)
			{
				FieldSymbol fieldOpt = item3.FieldOpt;
				if (!fieldOpt.IsMetadataConstant)
				{
					SyntaxReference syntax = item3.Syntax;
					EqualsValueClauseSyntax equalsValueClauseSyntax = (EqualsValueClauseSyntax)syntax.GetSyntax();
					if (binderFactory == null)
					{
						binderFactory = compilation.GetBinderFactory(syntax.SyntaxTree);
					}
					Binder binder = binderFactory.GetBinder(equalsValueClauseSyntax);
					if (firstDebugImports == null)
					{
						firstDebugImports = binder.ImportChain;
					}
					binder = new LocalScopeBinder(binder).WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.FieldInitializer, fieldOpt);
					BoundFieldEqualsValue item = BindFieldInitializer(binder, fieldOpt, equalsValueClauseSyntax, diagnostics);
					boundInitializers.Add(item);
				}
			}
		}
	}

	private static void BindScriptFieldInitializers(CSharpCompilation compilation, SynthesizedInteractiveInitializerMethod scriptInitializer, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers, ArrayBuilder<BoundInitializer> boundInitializers, DiagnosticBag diagnostics, out ImportChain firstDebugImports)
	{
		firstDebugImports = null;
		for (int i = 0; i < initializers.Length; i++)
		{
			ImmutableArray<FieldOrPropertyInitializer> immutableArray = initializers[i];
			BinderFactory binderFactory = null;
			ScriptLocalScopeBinder.Labels labels = null;
			for (int j = 0; j < immutableArray.Length; j++)
			{
				FieldOrPropertyInitializer fieldOrPropertyInitializer = immutableArray[j];
				FieldSymbol fieldOpt = fieldOrPropertyInitializer.FieldOpt;
				if ((object)fieldOpt == null || !fieldOpt.IsConst)
				{
					SyntaxReference syntax = fieldOrPropertyInitializer.Syntax;
					SyntaxTree syntaxTree = syntax.SyntaxTree;
					CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)syntax.GetSyntax();
					CompilationUnitSyntax compilationUnitRoot = syntaxTree.GetCompilationUnitRoot();
					if (binderFactory == null)
					{
						binderFactory = compilation.GetBinderFactory(syntaxTree);
						labels = new ScriptLocalScopeBinder.Labels(scriptInitializer, compilationUnitRoot);
					}
					Binder binder = binderFactory.GetBinder(cSharpSyntaxNode);
					if (firstDebugImports == null)
					{
						firstDebugImports = binder.ImportChain;
					}
					Binder binder2 = new ExecutableCodeBinder(compilationUnitRoot, scriptInitializer, new ScriptLocalScopeBinder(labels, binder));
					BoundInitializer item = (((object)fieldOpt == null) ? BindGlobalStatement(binder2, scriptInitializer, (StatementSyntax)cSharpSyntaxNode, diagnostics, i == initializers.Length - 1 && j == immutableArray.Length - 1) : BindFieldInitializer(binder2.WithAdditionalFlagsAndContainingMemberOrLambda(BinderFlags.FieldInitializer, fieldOpt), fieldOpt, (EqualsValueClauseSyntax)cSharpSyntaxNode, diagnostics));
					boundInitializers.Add(item);
				}
			}
		}
	}

	private static BoundInitializer BindGlobalStatement(Binder binder, SynthesizedInteractiveInitializerMethod scriptInitializer, StatementSyntax statementNode, DiagnosticBag diagnostics, bool isLast)
	{
		BoundStatement boundStatement = binder.BindStatement(statementNode, diagnostics);
		if (isLast && !boundStatement.HasAnyErrors)
		{
			if (binder.Compilation.IsSubmission)
			{
				BoundExpression trailingScriptExpression = InitializerRewriter.GetTrailingScriptExpression(boundStatement);
				if (trailingScriptExpression != null && ((object)trailingScriptExpression.Type == null || trailingScriptExpression.Type.SpecialType != SpecialType.System_Void))
				{
					TypeSymbol resultType = scriptInitializer.ResultType;
					trailingScriptExpression = binder.GenerateConversionForAssignment(resultType, trailingScriptExpression, diagnostics);
					boundStatement = new BoundExpressionStatement(boundStatement.Syntax, trailingScriptExpression, trailingScriptExpression.HasErrors);
				}
			}
			if (boundStatement.Kind == BoundKind.LabeledStatement)
			{
				BoundStatement body = ((BoundLabeledStatement)boundStatement).Body;
				while (body.Kind == BoundKind.LabeledStatement)
				{
					body = ((BoundLabeledStatement)body).Body;
				}
				if (InitializerRewriter.GetTrailingScriptExpression(body) != null)
				{
					Error(diagnostics, ErrorCode.ERR_SemicolonExpected, ((ExpressionStatementSyntax)body.Syntax).SemicolonToken);
				}
			}
		}
		return new BoundGlobalStatementInitializer(statementNode, boundStatement);
	}

	private static BoundFieldEqualsValue BindFieldInitializer(Binder binder, FieldSymbol fieldSymbol, EqualsValueClauseSyntax equalsValueClauseNode, DiagnosticBag diagnostics)
	{
		ConsList<FieldSymbol> fieldsBeingBound = binder.FieldsBeingBound;
		bool flag = fieldSymbol is SourceMemberFieldSymbolFromDeclarator sourceMemberFieldSymbolFromDeclarator && sourceMemberFieldSymbolFromDeclarator.FieldTypeInferred(fieldsBeingBound);
		DiagnosticBag diagnosticBag = ((!flag) ? diagnostics : DiagnosticBag.GetInstance());
		binder = new ExecutableCodeBinder(equalsValueClauseNode, fieldSymbol, new LocalScopeBinder(binder));
		BoundFieldEqualsValue result = binder.BindFieldInitializer(fieldSymbol, equalsValueClauseNode, diagnosticBag);
		if (flag)
		{
			diagnosticBag.Free();
		}
		return result;
	}

	private BoundExpression BindInterpolatedString(InterpolatedStringExpressionSyntax node, DiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_String, diagnostics, node);
		NamedTypeSymbol specialType2 = GetSpecialType(SpecialType.System_Object, diagnostics, node);
		NamedTypeSymbol specialType3 = GetSpecialType(SpecialType.System_Int32, diagnostics, node);
		foreach (InterpolatedStringContentSyntax content in node.Contents)
		{
			switch (content.Kind())
			{
			case SyntaxKind.Interpolation:
			{
				InterpolationSyntax interpolationSyntax = (InterpolationSyntax)content;
				BoundExpression boundExpression = BindValue(interpolationSyntax.Expression, diagnostics, BindValueKind.RValue);
				GenerateConversionForAssignment(specialType2, boundExpression, diagnostics);
				BoundExpression boundExpression2 = null;
				BoundLiteral format = null;
				if (interpolationSyntax.AlignmentClause != null)
				{
					boundExpression2 = GenerateConversionForAssignment(specialType3, BindValue(interpolationSyntax.AlignmentClause.Value, diagnostics, BindValueKind.RValue), diagnostics);
					ConstantValue constantValue = boundExpression2.ConstantValue;
					if (constantValue != null && !constantValue.IsBad)
					{
						int int32Value = constantValue.Int32Value;
						int32Value = ((int32Value > 0) ? (-int32Value) : int32Value);
						if (int32Value < -32767)
						{
							diagnostics.Add(ErrorCode.WRN_AlignmentMagnitude, boundExpression2.Syntax.Location, constantValue.Int32Value, 32767);
						}
					}
					else if (!boundExpression2.HasErrors)
					{
						diagnostics.Add(ErrorCode.ERR_ConstantExpected, interpolationSyntax.AlignmentClause.Value.Location);
					}
				}
				if (interpolationSyntax.FormatClause != null)
				{
					string valueText2 = interpolationSyntax.FormatClause.FormatStringToken.ValueText;
					bool hasErrors = false;
					char ch;
					if (valueText2.Length == 0)
					{
						diagnostics.Add(ErrorCode.ERR_EmptyFormatSpecifier, interpolationSyntax.FormatClause.Location);
						hasErrors = true;
					}
					else if (SyntaxFacts.IsWhitespace(ch = valueText2[valueText2.Length - 1]) || SyntaxFacts.IsNewLine(ch))
					{
						diagnostics.Add(ErrorCode.ERR_TrailingWhitespaceInFormatSpecifier, interpolationSyntax.FormatClause.Location);
						hasErrors = true;
					}
					format = new BoundLiteral(interpolationSyntax.FormatClause, ConstantValue.Create(valueText2), specialType, hasErrors);
				}
				instance.Add(new BoundStringInsert(interpolationSyntax, boundExpression, boundExpression2, format, null));
				break;
			}
			case SyntaxKind.InterpolatedStringText:
			{
				string valueText = ((InterpolatedStringTextSyntax)content).TextToken.ValueText;
				instance.Add(new BoundLiteral(content, ConstantValue.Create(valueText, SpecialType.System_String), specialType));
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(content.Kind());
			}
		}
		return new BoundInterpolatedString(node, instance.ToImmutableAndFree(), specialType);
	}

	private BoundExpression BindMethodGroup(ExpressionSyntax node, bool invoked, bool indexed, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.IdentifierName:
		case SyntaxKind.GenericName:
			return BindIdentifier((SimpleNameSyntax)node, invoked, indexed, diagnostics);
		case SyntaxKind.SimpleMemberAccessExpression:
		case SyntaxKind.PointerMemberAccessExpression:
			return BindMemberAccess((MemberAccessExpressionSyntax)node, invoked, indexed, diagnostics);
		case SyntaxKind.ParenthesizedExpression:
			return BindMethodGroup(((ParenthesizedExpressionSyntax)node).Expression, invoked: false, indexed: false, diagnostics);
		default:
			return BindExpression(node, diagnostics, invoked, indexed);
		}
	}

	private static ImmutableArray<MethodSymbol> GetOriginalMethods(OverloadResolutionResult<MethodSymbol> overloadResolutionResult)
	{
		if (overloadResolutionResult == null)
		{
			return ImmutableArray<MethodSymbol>.Empty;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		foreach (MemberResolutionResult<MethodSymbol> result in overloadResolutionResult.Results)
		{
			instance.Add(result.Member);
		}
		return instance.ToImmutableAndFree();
	}

	internal BoundExpression MakeInvocationExpression(SyntaxNode node, BoundExpression receiver, string methodName, ImmutableArray<BoundExpression> args, DiagnosticBag diagnostics, SeparatedSyntaxList<TypeSyntax> typeArgsSyntax = default(SeparatedSyntaxList<TypeSyntax>), ImmutableArray<TypeSymbol> typeArgs = default(ImmutableArray<TypeSymbol>), CSharpSyntaxNode queryClause = null, bool allowFieldsAndProperties = false, bool allowUnexpandedForm = true)
	{
		BoundExpression boundExpression = BindInstanceMemberAccess(node, node, receiver, methodName, typeArgs.NullToEmpty().Length, typeArgsSyntax, typeArgs, invoked: true, indexed: false, diagnostics);
		if (!allowFieldsAndProperties && (boundExpression.Kind == BoundKind.FieldAccess || boundExpression.Kind == BoundKind.PropertyAccess))
		{
			MessageID id;
			Symbol item;
			if (boundExpression.Kind == BoundKind.FieldAccess)
			{
				id = MessageID.IDS_SK_FIELD;
				item = ((BoundFieldAccess)boundExpression).FieldSymbol;
			}
			else
			{
				id = MessageID.IDS_SK_PROPERTY;
				item = ((BoundPropertyAccess)boundExpression).PropertySymbol;
			}
			diagnostics.Add(ErrorCode.ERR_BadSKknown, node.Location, methodName, id.Localize(), MessageID.IDS_SK_METHOD.Localize());
			return BadExpression(node, LookupResultKind.Empty, ImmutableArray.Create(item), args.Add(receiver));
		}
		boundExpression = CheckValue(boundExpression, BindValueKind.RValueOrMethodGroup, diagnostics);
		boundExpression.WasCompilerGenerated = true;
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		instance.Arguments.AddRange(args);
		BoundExpression boundExpression2 = BindInvocationExpression(node, node, methodName, boundExpression, instance, diagnostics, queryClause, allowUnexpandedForm);
		if (queryClause != null && boundExpression2.Kind == BoundKind.DynamicInvocation)
		{
			boundExpression2 = CreateBadCall(node, boundExpression, LookupResultKind.Viable, instance);
		}
		boundExpression2.WasCompilerGenerated = true;
		instance.Free();
		return boundExpression2;
	}

	private BoundExpression BindInvocationExpression(InvocationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		if (TryBindNameofOperator(node, diagnostics, out var result))
		{
			return result;
		}
		bool flag = node.Expression.Kind() == SyntaxKind.ArgListExpression;
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		if (flag)
		{
			BindArgumentsAndNames(node.ArgumentList, diagnostics, instance);
			result = BindArgListOperator(node, diagnostics, instance);
		}
		else
		{
			BoundExpression expr = BindMethodGroup(node.Expression, invoked: true, indexed: false, diagnostics);
			expr = CheckValue(expr, BindValueKind.RValueOrMethodGroup, diagnostics);
			string methodName = ((expr.Kind == BoundKind.MethodGroup) ? GetName(node.Expression) : null);
			BindArgumentsAndNames(node.ArgumentList, diagnostics, instance, allowArglist: true);
			result = BindInvocationExpression(node, node.Expression, methodName, expr, instance, diagnostics);
		}
		instance.Free();
		return result;
	}

	private BoundExpression BindArgListOperator(InvocationExpressionSyntax node, DiagnosticBag diagnostics, AnalyzedArguments analyzedArguments)
	{
		TypeSymbol specialType = GetSpecialType(SpecialType.System_Object, diagnostics, node);
		for (int i = 0; i < analyzedArguments.Arguments.Count; i++)
		{
			BoundExpression boundExpression = analyzedArguments.Arguments[i];
			if (boundExpression.Kind == BoundKind.OutVariablePendingInference)
			{
				analyzedArguments.Arguments[i] = ((OutVariablePendingInference)boundExpression).FailInference(this, diagnostics);
			}
			else if ((object)boundExpression.Type == null && !boundExpression.HasAnyErrors)
			{
				analyzedArguments.Arguments[i] = GenerateConversionForAssignment(specialType, boundExpression, diagnostics);
			}
			else if (boundExpression.Type.SpecialType == SpecialType.System_Void)
			{
				Error(diagnostics, ErrorCode.ERR_CantUseVoidInArglist, boundExpression.Syntax);
			}
			RefKind refKind = analyzedArguments.RefKind(i);
			if (refKind > RefKind.Ref)
			{
				Error(diagnostics, ErrorCode.ERR_CantUseInOrOutInArglist, boundExpression.Syntax);
			}
		}
		ImmutableArray<BoundExpression> arguments = analyzedArguments.Arguments.ToImmutable();
		ImmutableArray<RefKind> argumentRefKindsOpt = analyzedArguments.RefKinds.ToImmutableOrNull();
		return new BoundArgListOperator(node, arguments, argumentRefKindsOpt, null, analyzedArguments.HasErrors);
	}

	private BoundExpression BindInvocationExpression(SyntaxNode node, SyntaxNode expression, string methodName, BoundExpression boundExpression, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics, CSharpSyntaxNode queryClause = null, bool allowUnexpandedForm = true)
	{
		BoundExpression boundExpression2;
		NamedTypeSymbol delegateType;
		if ((object)boundExpression.Type != null && boundExpression.Type.IsDynamic())
		{
			boundExpression2 = BindDynamicInvocation(node, boundExpression, analyzedArguments, ImmutableArray<MethodSymbol>.Empty, diagnostics, queryClause);
		}
		else if (boundExpression.Kind == BoundKind.MethodGroup)
		{
			boundExpression2 = BindMethodGroupInvocation(node, expression, methodName, (BoundMethodGroup)boundExpression, analyzedArguments, diagnostics, queryClause, allowUnexpandedForm);
		}
		else if ((object)(delegateType = GetDelegateType(boundExpression)) != null)
		{
			if (ReportDelegateInvokeUseSiteDiagnostic(diagnostics, delegateType, null, node))
			{
				return CreateBadCall(node, boundExpression, LookupResultKind.Viable, analyzedArguments);
			}
			boundExpression2 = BindDelegateInvocation(node, expression, methodName, boundExpression, analyzedArguments, diagnostics, queryClause, delegateType);
		}
		else
		{
			if (!boundExpression.HasAnyErrors)
			{
				diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_MethodNameExpected), expression.Location);
			}
			boundExpression2 = CreateBadCall(node, boundExpression, LookupResultKind.NotInvocable, analyzedArguments);
		}
		CheckRestrictedTypeReceiver(boundExpression2, Compilation, diagnostics);
		return boundExpression2;
	}

	private BoundExpression BindDynamicInvocation(SyntaxNode node, BoundExpression expression, AnalyzedArguments arguments, ImmutableArray<MethodSymbol> applicableMethods, DiagnosticBag diagnostics, CSharpSyntaxNode queryClause)
	{
		CheckNamedArgumentsForDynamicInvocation(arguments, diagnostics);
		bool flag = false;
		if (expression.Kind == BoundKind.MethodGroup)
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)expression;
			BoundExpression receiverOpt = boundMethodGroup.ReceiverOpt;
			if (receiverOpt != null)
			{
				switch (receiverOpt.Kind)
				{
				case BoundKind.BaseReference:
					Error(diagnostics, ErrorCode.ERR_NoDynamicPhantomOnBase, node, boundMethodGroup.Name);
					flag = true;
					break;
				case BoundKind.ThisReference:
					if ((InConstructorInitializer || InFieldInitializer) && receiverOpt.WasCompilerGenerated)
					{
						expression = boundMethodGroup.Update(boundMethodGroup.TypeArgumentsOpt, boundMethodGroup.Name, boundMethodGroup.Methods, boundMethodGroup.LookupSymbolOpt, boundMethodGroup.LookupError, boundMethodGroup.Flags & ~BoundMethodGroupFlags.HasImplicitReceiver, new BoundTypeExpression(node, null, ContainingType).MakeCompilerGenerated(), boundMethodGroup.ResultKind);
					}
					break;
				case BoundKind.TypeOrValueExpression:
				{
					BoundTypeOrValueExpression boundTypeOrValueExpression = (BoundTypeOrValueExpression)receiverOpt;
					bool useType = IsInstance(boundTypeOrValueExpression.Data.ValueSymbol) && !HasThis(isExplicit: false, out var _);
					BoundExpression receiverOpt2 = ReplaceTypeOrValueReceiver(boundTypeOrValueExpression, useType, diagnostics);
					expression = boundMethodGroup.Update(boundMethodGroup.TypeArgumentsOpt, boundMethodGroup.Name, boundMethodGroup.Methods, boundMethodGroup.LookupSymbolOpt, boundMethodGroup.LookupError, boundMethodGroup.Flags, receiverOpt2, boundMethodGroup.ResultKind);
					break;
				}
				}
			}
		}
		ImmutableArray<BoundExpression> arguments2 = BuildArgumentsForDynamicInvocation(arguments, diagnostics);
		ImmutableArray<RefKind> immutableArray = arguments.RefKinds.ToImmutableOrNull();
		flag &= ReportBadDynamicArguments(node, arguments2, immutableArray, diagnostics, queryClause);
		return new BoundDynamicInvocation(node, expression, arguments2, arguments.GetNames(), immutableArray, applicableMethods, Compilation.DynamicType, flag);
	}

	private void CheckNamedArgumentsForDynamicInvocation(AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		if (arguments.Names.Count == 0 || !Compilation.LanguageVersion.AllowNonTrailingNamedArguments())
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < arguments.Names.Count; i++)
		{
			if (arguments.Names[i] != null)
			{
				flag = true;
			}
			else if (flag)
			{
				Error(diagnostics, ErrorCode.ERR_NamedArgumentSpecificationBeforeFixedArgumentInDynamicInvocation, arguments.Arguments[i].Syntax);
				break;
			}
		}
	}

	private ImmutableArray<BoundExpression> BuildArgumentsForDynamicInvocation(AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		for (int i = 0; i < arguments.Arguments.Count; i++)
		{
			if (arguments.Arguments[i].Kind != BoundKind.OutVariablePendingInference && (arguments.Arguments[i].Kind != BoundKind.DiscardExpression || arguments.Arguments[i].HasExpressionType()))
			{
				continue;
			}
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(arguments.Arguments.Count);
			instance.AddRange(arguments.Arguments);
			do
			{
				BoundExpression boundExpression = instance[i];
				if (boundExpression.Kind == BoundKind.OutVariablePendingInference)
				{
					instance[i] = ((OutVariablePendingInference)boundExpression).FailInference(this, diagnostics);
				}
				else if (boundExpression.Kind == BoundKind.DiscardExpression && !boundExpression.HasExpressionType())
				{
					instance[i] = ((BoundDiscardExpression)boundExpression).FailInference(this, diagnostics);
				}
				i++;
			}
			while (i < instance.Count);
			return instance.ToImmutableAndFree();
		}
		return arguments.Arguments.ToImmutable();
	}

	private static bool ReportBadDynamicArguments(SyntaxNode node, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> refKinds, DiagnosticBag diagnostics, CSharpSyntaxNode queryClause)
	{
		bool result = false;
		bool flag = false;
		if (!refKinds.IsDefault)
		{
			for (int i = 0; i < refKinds.Length; i++)
			{
				if (refKinds[i] == RefKind.In)
				{
					Error(diagnostics, ErrorCode.ERR_InDynamicMethodArg, arguments[i].Syntax);
					result = true;
				}
			}
		}
		foreach (BoundExpression item in arguments)
		{
			if (!IsLegalDynamicOperand(item))
			{
				if (queryClause != null && !flag)
				{
					flag = true;
					Error(diagnostics, ErrorCode.ERR_BadDynamicQuery, node);
					result = true;
				}
				else if (item.Kind == BoundKind.Lambda || item.Kind == BoundKind.UnboundLambda)
				{
					Error(diagnostics, ErrorCode.ERR_BadDynamicMethodArgLambda, item.Syntax);
					result = true;
				}
				else if (item.Kind == BoundKind.MethodGroup)
				{
					Error(diagnostics, ErrorCode.ERR_BadDynamicMethodArgMemgrp, item.Syntax);
					result = true;
				}
				else if (item.Kind == BoundKind.ArgListOperator)
				{
					Error(diagnostics, ErrorCode.ERR_BadDynamicMethodArg, item.Syntax, "__arglist");
				}
				else if (item.IsLiteralDefault())
				{
					Error(diagnostics, ErrorCode.ERR_BadDynamicMethodArgDefaultLiteral, item.Syntax);
					result = true;
				}
				else
				{
					Error(diagnostics, ErrorCode.ERR_BadDynamicMethodArg, item.Syntax, item.Type);
					result = true;
				}
			}
		}
		return result;
	}

	private BoundExpression BindDelegateInvocation(SyntaxNode node, SyntaxNode expression, string methodName, BoundExpression boundExpression, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics, CSharpSyntaxNode queryClause, NamedTypeSymbol delegateType)
	{
		MethodGroup instance = MethodGroup.GetInstance();
		instance.PopulateWithSingleMethod(boundExpression, delegateType.DelegateInvokeMethod);
		OverloadResolutionResult<MethodSymbol> instance2 = OverloadResolutionResult<MethodSymbol>.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		OverloadResolution.MethodInvocationOverloadResolution(instance.Methods, instance.TypeArguments, instance.Receiver, analyzedArguments, instance2, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		BoundExpression result = ((!analyzedArguments.HasDynamicArgument || !instance2.HasAnyApplicableMember) ? BindInvocationExpressionContinued(node, expression, methodName, instance2, analyzedArguments, instance, delegateType, diagnostics, queryClause) : BindDynamicInvocation(node, boundExpression, analyzedArguments, instance2.GetAllApplicableMembers(), diagnostics, queryClause));
		instance2.Free();
		instance.Free();
		return result;
	}

	private static bool HasApplicableConditionalMethod(OverloadResolutionResult<MethodSymbol> results)
	{
		ImmutableArray<MemberResolutionResult<MethodSymbol>> results2 = results.Results;
		for (int i = 0; i < results2.Length; i++)
		{
			if (results2[i].IsApplicable && results2[i].Member.IsConditional)
			{
				return true;
			}
		}
		return false;
	}

	private BoundExpression BindMethodGroupInvocation(SyntaxNode syntax, SyntaxNode expression, string methodName, BoundMethodGroup methodGroup, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics, CSharpSyntaxNode queryClause, bool allowUnexpandedForm = true)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		MethodGroupResolution resolution = ResolveMethodGroup(methodGroup, expression, methodName, analyzedArguments, isMethodGroupConversion: false, ref useSiteDiagnostics, inferWithDynamic: false, allowUnexpandedForm);
		diagnostics.Add(expression, useSiteDiagnostics);
		if (!methodGroup.HasAnyErrors)
		{
			diagnostics.AddRange(resolution.Diagnostics);
		}
		BoundExpression result;
		if (resolution.HasAnyErrors)
		{
			ImmutableArray<MethodSymbol> methods;
			LookupResultKind resultKind;
			ImmutableArray<TypeSymbol> typeArguments;
			if (resolution.OverloadResolutionResult != null)
			{
				methods = GetOriginalMethods(resolution.OverloadResolutionResult);
				resultKind = resolution.MethodGroup.ResultKind;
				typeArguments = resolution.MethodGroup.TypeArguments.ToImmutable();
			}
			else
			{
				methods = methodGroup.Methods;
				resultKind = methodGroup.ResultKind;
				typeArguments = methodGroup.TypeArgumentsOpt;
			}
			result = CreateBadCall(syntax, methodName, methodGroup.ReceiverOpt, methods, resultKind, typeArguments, analyzedArguments, resolution.IsExtensionMethodGroup, isDelegate: false);
		}
		else if (!resolution.IsEmpty)
		{
			if (resolution.ResultKind != LookupResultKind.Viable)
			{
				if (resolution.MethodGroup != null)
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					BindInvocationExpressionContinued(syntax, expression, methodName, resolution.OverloadResolutionResult, resolution.AnalyzedArguments, resolution.MethodGroup, null, instance, queryClause);
					instance.Free();
				}
				result = CreateBadCall(syntax, methodGroup, methodGroup.ResultKind, analyzedArguments);
			}
			else if (resolution.AnalyzedArguments.HasDynamicArgument && resolution.OverloadResolutionResult.HasAnyApplicableMember)
			{
				if (resolution.IsLocalFunctionInvocation)
				{
					result = BindLocalFunctionInvocationWithDynamicArgument(syntax, expression, methodName, methodGroup, diagnostics, queryClause, resolution);
				}
				else if (resolution.IsExtensionMethodGroup)
				{
					Error(diagnostics, ErrorCode.ERR_BadArgTypeDynamicExtension, syntax, methodGroup.InstanceOpt.Type, methodGroup.Name);
					result = CreateBadCall(syntax, methodGroup, methodGroup.ResultKind, analyzedArguments);
				}
				else
				{
					if (HasApplicableConditionalMethod(resolution.OverloadResolutionResult))
					{
						Error(diagnostics, ErrorCode.WRN_DynamicDispatchToConditionalMethod, syntax, methodGroup.Name);
					}
					ImmutableArray<MethodSymbol> candidatesPassingFinalValidation = GetCandidatesPassingFinalValidation(syntax, resolution.OverloadResolutionResult, methodGroup.ReceiverOpt, methodGroup.TypeArgumentsOpt, diagnostics);
					result = ((candidatesPassingFinalValidation.Length <= 0) ? CreateBadCall(syntax, methodGroup, methodGroup.ResultKind, analyzedArguments) : BindDynamicInvocation(syntax, methodGroup, resolution.AnalyzedArguments, candidatesPassingFinalValidation, diagnostics, queryClause));
				}
			}
			else
			{
				result = BindInvocationExpressionContinued(syntax, expression, methodName, resolution.OverloadResolutionResult, resolution.AnalyzedArguments, resolution.MethodGroup, null, diagnostics, queryClause);
			}
		}
		else
		{
			result = CreateBadCall(syntax, methodGroup, methodGroup.ResultKind, analyzedArguments);
		}
		resolution.Free();
		return result;
	}

	private BoundExpression BindLocalFunctionInvocationWithDynamicArgument(SyntaxNode syntax, SyntaxNode expression, string methodName, BoundMethodGroup boundMethodGroup, DiagnosticBag diagnostics, CSharpSyntaxNode queryClause, MethodGroupResolution resolution)
	{
		MemberResolutionResult<MethodSymbol> validResult = resolution.OverloadResolutionResult.ValidResult;
		ImmutableArray<BoundExpression> arguments = resolution.AnalyzedArguments.Arguments.ToImmutable();
		ImmutableArray<RefKind> refKinds = resolution.AnalyzedArguments.RefKinds.ToImmutableOrNull();
		ReportBadDynamicArguments(syntax, arguments, refKinds, diagnostics, queryClause);
		MethodSymbol member = validResult.Member;
		MemberAnalysisResult result = validResult.Result;
		if (OverloadResolution.IsValidParams(member) && result.Kind == MemberResolutionKind.ApplicableInNormalForm)
		{
			ImmutableArray<ParameterSymbol> parameters = member.Parameters;
			int num = parameters.Length - 1;
			for (int i = 0; i < arguments.Length; i++)
			{
				if (arguments[i].HasDynamicType() && result.ParameterFromArgument(i) == num)
				{
					Error(diagnostics, ErrorCode.ERR_DynamicLocalFunctionParamsParameter, syntax, parameters.Last().Name, member.Name);
					return BindDynamicInvocation(syntax, boundMethodGroup, resolution.AnalyzedArguments, resolution.OverloadResolutionResult.GetAllApplicableMembers(), diagnostics, queryClause);
				}
			}
		}
		if (boundMethodGroup.TypeArgumentsOpt.IsDefaultOrEmpty && member.IsGenericMethod)
		{
			Error(diagnostics, ErrorCode.ERR_DynamicLocalFunctionTypeParameter, syntax, member.Name);
			return BindDynamicInvocation(syntax, boundMethodGroup, resolution.AnalyzedArguments, resolution.OverloadResolutionResult.GetAllApplicableMembers(), diagnostics, queryClause);
		}
		return BindInvocationExpressionContinued(syntax, expression, methodName, resolution.OverloadResolutionResult, resolution.AnalyzedArguments, resolution.MethodGroup, null, diagnostics, queryClause);
	}

	private ImmutableArray<TMethodOrPropertySymbol> GetCandidatesPassingFinalValidation<TMethodOrPropertySymbol>(SyntaxNode syntax, OverloadResolutionResult<TMethodOrPropertySymbol> overloadResolutionResult, BoundExpression receiverOpt, ImmutableArray<TypeSymbol> typeArgumentsOpt, DiagnosticBag diagnostics) where TMethodOrPropertySymbol : Symbol
	{
		ArrayBuilder<TMethodOrPropertySymbol> instance = ArrayBuilder<TMethodOrPropertySymbol>.GetInstance();
		DiagnosticBag diagnosticBag = null;
		DiagnosticBag instance2 = DiagnosticBag.GetInstance();
		int i = 0;
		for (int count = overloadResolutionResult.ResultsBuilder.Count; i < count; i++)
		{
			MemberResolutionResult<TMethodOrPropertySymbol> memberResolutionResult = overloadResolutionResult.ResultsBuilder[i];
			if (memberResolutionResult.Result.IsApplicable)
			{
				if (!MemberGroupFinalValidationAccessibilityChecks(receiverOpt, memberResolutionResult.Member, syntax, instance2, invokedAsExtensionMethod: false) && (typeArgumentsOpt.IsDefault || ((MethodSymbol)(object)memberResolutionResult.Member).CheckConstraints(Conversions, syntax, Compilation, instance2)))
				{
					instance.Add(memberResolutionResult.Member);
				}
				else if (diagnosticBag == null)
				{
					diagnosticBag = instance2;
					instance2 = DiagnosticBag.GetInstance();
				}
				else
				{
					instance2.Clear();
				}
			}
		}
		if (diagnosticBag != null)
		{
			if (instance.Count == 0)
			{
				diagnostics.AddRange(diagnosticBag);
			}
			diagnosticBag.Free();
		}
		instance2.Free();
		return instance.ToImmutableAndFree();
	}

	private void CheckRestrictedTypeReceiver(BoundExpression expression, Compilation compilation, DiagnosticBag diagnostics)
	{
		switch (expression.Kind)
		{
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expression;
			if (!boundCall.HasAnyErrors && boundCall.ReceiverOpt != null && (object)boundCall.ReceiverOpt.Type != null)
			{
				if (boundCall.ReceiverOpt.Type.IsRestrictedType() && boundCall.Method.ContainingType != boundCall.ReceiverOpt.Type)
				{
					SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(compilation, boundCall.ReceiverOpt.Type, boundCall.Method.ContainingType);
					Error(diagnostics, ErrorCode.ERR_NoImplicitConv, boundCall.ReceiverOpt.Syntax, symbolDistinguisher.First, symbolDistinguisher.Second);
				}
				else if (boundCall.ReceiverOpt.Kind == BoundKind.BaseReference && ContainingType.IsRestrictedType())
				{
					SymbolDistinguisher symbolDistinguisher2 = new SymbolDistinguisher(compilation, ContainingType, boundCall.Method.ContainingType);
					Error(diagnostics, ErrorCode.ERR_NoImplicitConv, boundCall.ReceiverOpt.Syntax, symbolDistinguisher2.First, symbolDistinguisher2.Second);
				}
			}
			break;
		}
		case BoundKind.DynamicInvocation:
		{
			BoundDynamicInvocation boundDynamicInvocation = (BoundDynamicInvocation)expression;
			if (!boundDynamicInvocation.HasAnyErrors && (object)boundDynamicInvocation.Expression.Type != null && boundDynamicInvocation.Expression.Type.IsRestrictedType())
			{
				Error(diagnostics, ErrorCode.ERR_BadDynamicMethodArg, boundDynamicInvocation.Expression.Syntax, boundDynamicInvocation.Expression.Type);
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(expression.Kind);
		}
	}

	private BoundCall BindInvocationExpressionContinued(SyntaxNode node, SyntaxNode expression, string methodName, OverloadResolutionResult<MethodSymbol> result, AnalyzedArguments analyzedArguments, MethodGroup methodGroup, NamedTypeSymbol delegateTypeOpt, DiagnosticBag diagnostics, CSharpSyntaxNode queryClause = null)
	{
		bool isExtensionMethodGroup = methodGroup.IsExtensionMethodGroup;
		if (!result.Succeeded)
		{
			if (analyzedArguments.HasErrors)
			{
				foreach (BoundExpression argument in analyzedArguments.Arguments)
				{
					if (argument is UnboundLambda unboundLambda)
					{
						BoundLambda boundLambda = unboundLambda.BindForErrorRecovery();
						diagnostics.AddRange(boundLambda.Diagnostics);
					}
				}
			}
			else
			{
				string name = (((object)delegateTypeOpt == null) ? methodName : null);
				result.ReportDiagnostics(this, GetLocationForOverloadResolutionDiagnostic(node, expression), node, diagnostics, name, methodGroup.Receiver, expression, analyzedArguments, methodGroup.Methods.ToImmutable(), null, delegateTypeOpt, queryClause);
			}
			return CreateBadCall(node, methodGroup.Name, (isExtensionMethodGroup && analyzedArguments.Arguments.Count > 0 && methodGroup.Receiver == analyzedArguments.Arguments[0]) ? null : methodGroup.Receiver, GetOriginalMethods(result), methodGroup.ResultKind, methodGroup.TypeArguments.ToImmutable(), analyzedArguments, isExtensionMethodGroup, (object)delegateTypeOpt != null);
		}
		MemberResolutionResult<MethodSymbol> validResult = result.ValidResult;
		TypeSymbol returnType = validResult.Member.ReturnType;
		CoerceArguments(validResult, analyzedArguments.Arguments, diagnostics);
		MethodSymbol member = validResult.Member;
		bool expanded = validResult.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm;
		ImmutableArray<int> argsToParamsOpt = validResult.Result.ArgsToParamsOpt;
		BoundExpression boundExpression = ReplaceTypeOrValueReceiver(methodGroup.Receiver, member.IsStatic && !isExtensionMethodGroup, diagnostics);
		bool flag = MemberGroupFinalValidation(boundExpression, member, expression, diagnostics, isExtensionMethodGroup);
		if (isExtensionMethodGroup)
		{
			BoundExpression boundExpression2 = analyzedArguments.Argument(0);
			ParameterSymbol parameterSymbol = member.Parameters.First();
			if (boundExpression != boundExpression2)
			{
				boundExpression2 = CreateConversion(boundExpression, validResult.Result.ConversionForArg(0), parameterSymbol.Type, diagnostics);
			}
			if (parameterSymbol.RefKind == RefKind.Ref)
			{
				boundExpression2 = CheckValue(boundExpression2, BindValueKind.RefOrOut, diagnostics);
				if (analyzedArguments.RefKinds.Count == 0)
				{
					analyzedArguments.RefKinds.Count = analyzedArguments.Arguments.Count;
				}
				analyzedArguments.RefKinds[0] = RefKind.Ref;
				CheckFeatureAvailability(boundExpression2.Syntax, MessageID.IDS_FeatureRefExtensionMethods, diagnostics);
			}
			else if (parameterSymbol.RefKind == RefKind.In)
			{
				CheckFeatureAvailability(boundExpression2.Syntax, MessageID.IDS_FeatureRefExtensionMethods, diagnostics);
			}
			analyzedArguments.Arguments[0] = boundExpression2;
		}
		if (isExtensionMethodGroup || (member.IsStatic && boundExpression != null && boundExpression.WasCompilerGenerated))
		{
			boundExpression = null;
		}
		ImmutableArray<string> names = analyzedArguments.GetNames();
		ImmutableArray<RefKind> argumentRefKindsOpt = analyzedArguments.RefKinds.ToImmutableOrNull();
		ImmutableArray<BoundExpression> immutableArray = analyzedArguments.Arguments.ToImmutable();
		if (!flag && !member.IsStatic && boundExpression != null && boundExpression.Kind == BoundKind.ThisReference && boundExpression.WasCompilerGenerated)
		{
			flag = IsRefOrOutThisParameterCaptured(node, diagnostics);
		}
		if (member.HasUnsafeParameter())
		{
			flag = ReportUnsafeIfNotAllowed(node, diagnostics) | flag;
		}
		bool flag2 = boundExpression != null && boundExpression.Kind == BoundKind.BaseReference;
		ReportDiagnosticsIfObsolete(diagnostics, member, node, flag2);
		if (member.IsRuntimeFinalizer())
		{
			ErrorCode code = (flag2 ? ErrorCode.ERR_CallingBaseFinalizeDeprecated : ErrorCode.ERR_CallingFinalizeDeprecated);
			Error(diagnostics, code, node);
			flag = true;
		}
		if (!flag)
		{
			flag = !CheckInvocationArgMixing(node, member, boundExpression, member.Parameters, immutableArray, argsToParamsOpt, LocalScopeDepth, diagnostics);
		}
		if ((object)delegateTypeOpt != null)
		{
			return new BoundCall(node, boundExpression, member, immutableArray, names, argumentRefKindsOpt, isDelegateCall: true, expanded, isExtensionMethodGroup, argsToParamsOpt, LookupResultKind.Viable, this, returnType, flag);
		}
		if (boundExpression != null && boundExpression.Kind == BoundKind.BaseReference && member.IsAbstract)
		{
			Error(diagnostics, ErrorCode.ERR_AbstractBaseCall, node, member);
			flag = true;
		}
		if (!member.IsStatic)
		{
			WarnOnAccessOfOffDefault((node.Kind() == SyntaxKind.InvocationExpression) ? ((InvocationExpressionSyntax)node).Expression : node, boundExpression, diagnostics);
		}
		return new BoundCall(node, boundExpression, member, immutableArray, names, argumentRefKindsOpt, isDelegateCall: false, expanded, isExtensionMethodGroup, argsToParamsOpt, LookupResultKind.Viable, this, returnType, flag);
	}

	private static Location GetLocationForOverloadResolutionDiagnostic(SyntaxNode node, SyntaxNode expression)
	{
		if (node != expression)
		{
			switch (expression.Kind())
			{
			case SyntaxKind.QualifiedName:
				return ((QualifiedNameSyntax)expression).Right.GetLocation();
			case SyntaxKind.SimpleMemberAccessExpression:
			case SyntaxKind.PointerMemberAccessExpression:
				return ((MemberAccessExpressionSyntax)expression).Name.GetLocation();
			}
		}
		return expression.GetLocation();
	}

	private BoundExpression ReplaceTypeOrValueReceiver(BoundExpression receiver, bool useType, DiagnosticBag diagnostics)
	{
		if (receiver == null)
		{
			return null;
		}
		switch (receiver.Kind)
		{
		case BoundKind.TypeOrValueExpression:
		{
			BoundTypeOrValueExpression boundTypeOrValueExpression = (BoundTypeOrValueExpression)receiver;
			if (useType)
			{
				diagnostics.AddRange(boundTypeOrValueExpression.Data.TypeDiagnostics);
				return boundTypeOrValueExpression.Data.TypeExpression;
			}
			diagnostics.AddRange(boundTypeOrValueExpression.Data.ValueDiagnostics);
			return CheckValue(boundTypeOrValueExpression.Data.ValueExpression, BindValueKind.RValue, diagnostics);
		}
		case BoundKind.QueryClause:
		{
			BoundQueryClause boundQueryClause = (BoundQueryClause)receiver;
			BoundExpression value = boundQueryClause.Value;
			BoundExpression boundExpression = ReplaceTypeOrValueReceiver(value, useType, diagnostics);
			if (value != boundExpression)
			{
				return boundQueryClause.Update(boundExpression, boundQueryClause.DefinedSymbol, boundQueryClause.Operation, boundQueryClause.Cast, boundQueryClause.Binder, boundQueryClause.UnoptimizedForm, boundQueryClause.Type);
			}
			return boundQueryClause;
		}
		default:
			return receiver;
		}
	}

	private static NamedTypeSymbol GetDelegateType(BoundExpression expr)
	{
		if (expr != null && expr.Kind != BoundKind.TypeExpression && expr.Type is NamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsDelegateType())
		{
			return namedTypeSymbol;
		}
		return null;
	}

	private BoundCall CreateBadCall(SyntaxNode node, string name, BoundExpression receiver, ImmutableArray<MethodSymbol> methods, LookupResultKind resultKind, ImmutableArray<TypeSymbol> typeArguments, AnalyzedArguments analyzedArguments, bool invokedAsExtensionMethod, bool isDelegate)
	{
		if (!typeArguments.IsDefaultOrEmpty)
		{
			ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (MethodSymbol item in methods)
			{
				instance.Add((item.ConstructedFrom == item && item.Arity == typeArguments.Length) ? item.Construct(typeArguments) : item);
			}
			methods = instance.ToImmutableAndFree();
		}
		MethodSymbol method;
		if (methods.Length == 1 && !IsUnboundGeneric(methods[0]))
		{
			method = methods[0];
		}
		else
		{
			TypeSymbol returnType = GetCommonTypeOrReturnType(methods) ?? new ExtendedErrorTypeSymbol(Compilation, string.Empty, 0, null);
			method = new ErrorMethodSymbol((receiver != null && (object)receiver.Type != null) ? receiver.Type : ContainingType, returnType, name);
		}
		ImmutableArray<BoundExpression> arguments = BuildArgumentsForErrorRecovery(analyzedArguments, methods);
		ImmutableArray<string> names = analyzedArguments.GetNames();
		ImmutableArray<RefKind> refKinds = analyzedArguments.RefKinds.ToImmutableOrNull();
		return BoundCall.ErrorCall(node, receiver, method, arguments, names, refKinds, isDelegate, invokedAsExtensionMethod, methods, resultKind, this);
	}

	private static bool IsUnboundGeneric(MethodSymbol method)
	{
		if (method.IsGenericMethod)
		{
			return method.ConstructedFrom() == method;
		}
		return false;
	}

	private ImmutableArray<BoundExpression> BuildArgumentsForErrorRecovery(AnalyzedArguments analyzedArguments, ImmutableArray<MethodSymbol> methods)
	{
		ArrayBuilder<ImmutableArray<ParameterSymbol>> instance = ArrayBuilder<ImmutableArray<ParameterSymbol>>.GetInstance();
		foreach (MethodSymbol item in methods)
		{
			if (!IsUnboundGeneric(item) && item.ParameterCount > 0)
			{
				instance.Add(item.Parameters);
				if (instance.Count == 10)
				{
					break;
				}
			}
		}
		ImmutableArray<BoundExpression> result = BuildArgumentsForErrorRecovery(analyzedArguments, instance);
		instance.Free();
		return result;
	}

	private ImmutableArray<BoundExpression> BuildArgumentsForErrorRecovery(AnalyzedArguments analyzedArguments, ImmutableArray<PropertySymbol> properties)
	{
		ArrayBuilder<ImmutableArray<ParameterSymbol>> instance = ArrayBuilder<ImmutableArray<ParameterSymbol>>.GetInstance();
		foreach (PropertySymbol item in properties)
		{
			if (item.ParameterCount > 0)
			{
				instance.Add(item.Parameters);
				if (instance.Count == 10)
				{
					break;
				}
			}
		}
		ImmutableArray<BoundExpression> result = BuildArgumentsForErrorRecovery(analyzedArguments, instance);
		instance.Free();
		return result;
	}

	private ImmutableArray<BoundExpression> BuildArgumentsForErrorRecovery(AnalyzedArguments analyzedArguments, IEnumerable<ImmutableArray<ParameterSymbol>> parameterListList)
	{
		if (!analyzedArguments.Arguments.Any((BoundExpression e) => (object)e.Type == null))
		{
			return analyzedArguments.Arguments.ToImmutable();
		}
		int count = analyzedArguments.Arguments.Count;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(count);
		instance.AddRange(analyzedArguments.Arguments);
		for (int num = 0; num < count; num++)
		{
			BoundExpression boundExpression = instance[num];
			if ((object)boundExpression.Type != null)
			{
				continue;
			}
			switch (boundExpression.Kind)
			{
			case BoundKind.UnboundLambda:
			{
				UnboundLambda unboundLambda = (UnboundLambda)boundExpression;
				foreach (ImmutableArray<ParameterSymbol> parameterList in parameterListList)
				{
					TypeSymbol correspondingParameterType2 = GetCorrespondingParameterType(analyzedArguments, num, parameterList);
					if ((object)correspondingParameterType2 != null && correspondingParameterType2.Kind == SymbolKind.NamedType && (object)correspondingParameterType2.GetDelegateType() != null)
					{
						unboundLambda.Bind((NamedTypeSymbol)correspondingParameterType2);
					}
				}
				instance[num] = unboundLambda.BindForErrorRecovery();
				break;
			}
			case BoundKind.DiscardExpression:
			case BoundKind.OutVariablePendingInference:
			{
				if (boundExpression.HasExpressionType())
				{
					break;
				}
				TypeSymbol typeSymbol = null;
				foreach (ImmutableArray<ParameterSymbol> parameterList2 in parameterListList)
				{
					TypeSymbol correspondingParameterType = GetCorrespondingParameterType(analyzedArguments, num, parameterList2);
					if ((object)correspondingParameterType != null)
					{
						if ((object)typeSymbol == null)
						{
							typeSymbol = correspondingParameterType;
						}
						else if (!typeSymbol.Equals(correspondingParameterType, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds))
						{
							typeSymbol = null;
							break;
						}
					}
				}
				if (boundExpression.Kind == BoundKind.OutVariablePendingInference)
				{
					if ((object)typeSymbol == null)
					{
						instance[num] = ((OutVariablePendingInference)boundExpression).FailInference(this, null);
					}
					else
					{
						instance[num] = ((OutVariablePendingInference)boundExpression).SetInferredType(typeSymbol, null);
					}
				}
				else if (boundExpression.Kind == BoundKind.DiscardExpression)
				{
					if ((object)typeSymbol == null)
					{
						instance[num] = ((BoundDiscardExpression)boundExpression).FailInference(this, null);
					}
					else
					{
						instance[num] = ((BoundDiscardExpression)boundExpression).SetInferredType(typeSymbol);
					}
				}
				break;
			}
			case BoundKind.OutDeconstructVarPendingInference:
				instance[num] = ((OutDeconstructVarPendingInference)boundExpression).FailInference(this);
				break;
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static TypeSymbol GetCorrespondingParameterType(AnalyzedArguments analyzedArguments, int i, ImmutableArray<ParameterSymbol> parameterList)
	{
		string text = analyzedArguments.Name(i);
		if (text != null)
		{
			foreach (ParameterSymbol item in parameterList)
			{
				if (item.Name == text)
				{
					return item.Type;
				}
			}
			return null;
		}
		if (i >= parameterList.Length)
		{
			return null;
		}
		return parameterList[i].Type;
	}

	private ImmutableArray<BoundExpression> BuildArgumentsForErrorRecovery(AnalyzedArguments analyzedArguments)
	{
		return BuildArgumentsForErrorRecovery(analyzedArguments, Enumerable.Empty<ImmutableArray<ParameterSymbol>>());
	}

	private BoundCall CreateBadCall(SyntaxNode node, BoundExpression expr, LookupResultKind resultKind, AnalyzedArguments analyzedArguments)
	{
		TypeSymbol returnType = new ExtendedErrorTypeSymbol(Compilation, string.Empty, 0, null);
		MethodSymbol method = new ErrorMethodSymbol(expr.Type ?? ContainingType, returnType, string.Empty);
		ImmutableArray<BoundExpression> arguments = BuildArgumentsForErrorRecovery(analyzedArguments);
		ImmutableArray<string> names = analyzedArguments.GetNames();
		ImmutableArray<RefKind> refKinds = analyzedArguments.RefKinds.ToImmutableOrNull();
		ImmutableArray<MethodSymbol> originalMethods = ((expr.Kind == BoundKind.MethodGroup) ? ((BoundMethodGroup)expr).Methods : ImmutableArray<MethodSymbol>.Empty);
		return BoundCall.ErrorCall(node, expr, method, arguments, names, refKinds, isDelegateCall: false, invokedAsExtensionMethod: false, originalMethods, resultKind, this);
	}

	private static TypeSymbol GetCommonTypeOrReturnType<TMember>(ImmutableArray<TMember> members) where TMember : Symbol
	{
		TypeSymbol typeSymbol = null;
		int i = 0;
		for (int length = members.Length; i < length; i++)
		{
			TypeSymbol typeOrReturnType = members[i].GetTypeOrReturnType();
			if ((object)typeSymbol == null)
			{
				typeSymbol = typeOrReturnType;
			}
			else if (typeSymbol != typeOrReturnType)
			{
				return null;
			}
		}
		return typeSymbol;
	}

	private bool TryBindNameofOperator(InvocationExpressionSyntax node, DiagnosticBag diagnostics, out BoundExpression result)
	{
		result = null;
		if (node.Expression.Kind() != SyntaxKind.IdentifierName || ((IdentifierNameSyntax)node.Expression).Identifier.ContextualKind() != SyntaxKind.NameOfKeyword || node.ArgumentList.Arguments.Count != 1)
		{
			return false;
		}
		ArgumentSyntax argumentSyntax = node.ArgumentList.Arguments[0];
		if (argumentSyntax.NameColon != null || argumentSyntax.RefOrOutKeyword != default(SyntaxToken) || InvocableNameofInScope())
		{
			return false;
		}
		result = BindNameofOperatorInternal(node, diagnostics);
		return true;
	}

	private BoundExpression BindNameofOperatorInternal(InvocationExpressionSyntax node, DiagnosticBag diagnostics)
	{
		CheckFeatureAvailability(node, MessageID.IDS_FeatureNameof, diagnostics);
		ExpressionSyntax expression = node.ArgumentList.Arguments[0].Expression;
		string name = "";
		NameofBinder nameofBinder = new NameofBinder(expression, this);
		BoundExpression boundExpression = nameofBinder.BindExpression(expression, diagnostics);
		if (!boundExpression.HasAnyErrors && CheckSyntaxForNameofArgument(expression, out name, diagnostics) && boundExpression.Kind == BoundKind.MethodGroup)
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)boundExpression;
			if (!boundMethodGroup.TypeArgumentsOpt.IsDefaultOrEmpty)
			{
				diagnostics.Add(ErrorCode.ERR_NameofMethodGroupWithTypeParameters, expression.Location);
			}
			else
			{
				nameofBinder.EnsureNameofExpressionSymbols(boundMethodGroup, diagnostics);
			}
		}
		return new BoundNameOfOperator(node, boundExpression, ConstantValue.Create(name), Compilation.GetSpecialType(SpecialType.System_String));
	}

	private void EnsureNameofExpressionSymbols(BoundMethodGroup methodGroup, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		MethodGroupResolution methodGroupResolution = ResolveMethodGroup(methodGroup, null, isMethodGroupConversion: false, ref useSiteDiagnostics);
		diagnostics.Add(methodGroup.Syntax, useSiteDiagnostics);
		diagnostics.AddRange(methodGroupResolution.Diagnostics);
		if (methodGroupResolution.IsExtensionMethodGroup)
		{
			diagnostics.Add(ErrorCode.ERR_NameofExtensionMethod, methodGroup.Syntax.Location);
		}
	}

	private bool CheckSyntaxForNameofArgument(ExpressionSyntax argument, out string name, DiagnosticBag diagnostics, bool top = true)
	{
		switch (argument.Kind())
		{
		case SyntaxKind.IdentifierName:
		{
			IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)argument;
			name = identifierNameSyntax.Identifier.ValueText;
			return true;
		}
		case SyntaxKind.GenericName:
		{
			GenericNameSyntax genericNameSyntax = (GenericNameSyntax)argument;
			name = genericNameSyntax.Identifier.ValueText;
			return true;
		}
		case SyntaxKind.SimpleMemberAccessExpression:
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)argument;
			bool result = true;
			SyntaxKind syntaxKind = memberAccessExpressionSyntax.Expression.Kind();
			if (syntaxKind - 8746 > SyntaxKind.List)
			{
				result = CheckSyntaxForNameofArgument(memberAccessExpressionSyntax.Expression, out name, diagnostics, top: false);
			}
			name = memberAccessExpressionSyntax.Name.Identifier.ValueText;
			return result;
		}
		case SyntaxKind.AliasQualifiedName:
		{
			AliasQualifiedNameSyntax aliasQualifiedNameSyntax = (AliasQualifiedNameSyntax)argument;
			bool result2 = true;
			if (top)
			{
				diagnostics.Add(ErrorCode.ERR_AliasQualifiedNameNotAnExpression, argument.Location);
				result2 = false;
			}
			name = aliasQualifiedNameSyntax.Name.Identifier.ValueText;
			return result2;
		}
		case SyntaxKind.PredefinedType:
		case SyntaxKind.ThisExpression:
		case SyntaxKind.BaseExpression:
			name = "";
			if (!top)
			{
				return true;
			}
			break;
		}
		ErrorCode code = (top ? ErrorCode.ERR_ExpressionHasNoName : ErrorCode.ERR_SubexpressionNotInNameof);
		diagnostics.Add(code, argument.Location);
		name = "";
		return false;
	}

	private bool InvocableNameofInScope()
	{
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsWithFallback(instance, SyntaxFacts.GetText(SyntaxKind.NameOfKeyword), 0, ref useSiteDiagnostics, null, LookupOptions.MustBeInvocableIfMember | LookupOptions.AllMethodsOnArityZero);
		bool isMultiViable = instance.IsMultiViable;
		instance.Free();
		return isMultiViable;
	}

	private Tuple<ImmutableArray<RefKind>, ImmutableArray<TypeSymbol>, ImmutableArray<string>, bool> AnalyzeAnonymousFunction(CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		ImmutableArray<string> item = default;
		ImmutableArray<RefKind> item2 = default;
		ImmutableArray<TypeSymbol> item3 = default;
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		SeparatedSyntaxList<ParameterSyntax>? separatedSyntaxList = null;
		bool flag;
		bool item4;
		switch (syntax.Kind())
		{
		default:
		{
			flag = true;
			SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax = (SimpleLambdaExpressionSyntax)syntax;
			instance.Add(simpleLambdaExpressionSyntax.Parameter.Identifier.ValueText);
			item4 = simpleLambdaExpressionSyntax.AsyncKeyword.Kind() == SyntaxKind.AsyncKeyword;
			break;
		}
		case SyntaxKind.ParenthesizedLambdaExpression:
		{
			flag = true;
			ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax = (ParenthesizedLambdaExpressionSyntax)syntax;
			separatedSyntaxList = parenthesizedLambdaExpressionSyntax.ParameterList.Parameters;
			CheckParenthesizedLambdaParameters(separatedSyntaxList.Value, diagnostics);
			item4 = parenthesizedLambdaExpressionSyntax.AsyncKeyword.Kind() == SyntaxKind.AsyncKeyword;
			break;
		}
		case SyntaxKind.AnonymousMethodExpression:
		{
			AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax = (AnonymousMethodExpressionSyntax)syntax;
			flag = anonymousMethodExpressionSyntax.ParameterList != null;
			if (flag)
			{
				separatedSyntaxList = anonymousMethodExpressionSyntax.ParameterList.Parameters;
			}
			item4 = anonymousMethodExpressionSyntax.AsyncKeyword.Kind() == SyntaxKind.AsyncKeyword;
			break;
		}
		}
		if (separatedSyntaxList.HasValue)
		{
			bool flag2 = true;
			bool flag3 = true;
			ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
			ArrayBuilder<RefKind> instance3 = ArrayBuilder<RefKind>.GetInstance();
			foreach (ParameterSyntax item7 in separatedSyntaxList.Value)
			{
				foreach (AttributeListSyntax attributeList in item7.AttributeLists)
				{
					Error(diagnostics, ErrorCode.ERR_AttributesNotAllowed, attributeList);
				}
				if (item7.Default != null)
				{
					Error(diagnostics, ErrorCode.ERR_DefaultValueNotAllowed, item7.Default.EqualsToken);
				}
				if (item7.IsArgList)
				{
					Error(diagnostics, ErrorCode.ERR_IllegalVarArgs, item7);
					continue;
				}
				TypeSyntax type = item7.Type;
				TypeSymbol item5 = null;
				RefKind item6 = RefKind.None;
				if (type == null)
				{
					flag2 = false;
				}
				else
				{
					item5 = BindType(type, diagnostics);
					foreach (SyntaxToken modifier in item7.Modifiers)
					{
						switch (modifier.Kind())
						{
						case SyntaxKind.RefKeyword:
							item6 = RefKind.Ref;
							flag3 = false;
							break;
						case SyntaxKind.OutKeyword:
							item6 = RefKind.Out;
							flag3 = false;
							break;
						case SyntaxKind.InKeyword:
							item6 = RefKind.In;
							flag3 = false;
							break;
						case SyntaxKind.ParamsKeyword:
							Error(diagnostics, ErrorCode.ERR_IllegalParams, item7);
							break;
						case SyntaxKind.ThisKeyword:
							Error(diagnostics, ErrorCode.ERR_ThisInBadContext, modifier);
							break;
						}
					}
				}
				instance.Add(item7.Identifier.ValueText);
				instance2.Add(item5);
				instance3.Add(item6);
			}
			if (flag2)
			{
				item3 = instance2.ToImmutable();
			}
			if (!flag3)
			{
				item2 = instance3.ToImmutable();
			}
			instance2.Free();
			instance3.Free();
		}
		if (flag)
		{
			item = instance.ToImmutable();
		}
		instance.Free();
		return Tuple.Create(item2, item3, item, item4);
	}

	private void CheckParenthesizedLambdaParameters(SeparatedSyntaxList<ParameterSyntax> parameterSyntaxList, DiagnosticBag diagnostics)
	{
		if (parameterSyntaxList.Count <= 0)
		{
			return;
		}
		bool flag = parameterSyntaxList[0].Type != null;
		int i = 1;
		for (int count = parameterSyntaxList.Count; i < count; i++)
		{
			ParameterSyntax parameterSyntax = parameterSyntaxList[i];
			if (!parameterSyntax.Identifier.IsMissing)
			{
				bool flag2 = parameterSyntax.Type != null;
				if (flag != flag2)
				{
					diagnostics.Add(ErrorCode.ERR_InconsistentLambdaParameterUsage, parameterSyntax.Type?.GetLocation() ?? parameterSyntax.Identifier.GetLocation());
				}
			}
		}
	}

	private UnboundLambda BindAnonymousFunction(CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		Tuple<ImmutableArray<RefKind>, ImmutableArray<TypeSymbol>, ImmutableArray<string>, bool> tuple = AnalyzeAnonymousFunction(syntax, diagnostics);
		ImmutableArray<RefKind> item = tuple.Item1;
		ImmutableArray<TypeSymbol> item2 = tuple.Item2;
		ImmutableArray<string> item3 = tuple.Item3;
		bool item4 = tuple.Item4;
		if (!item2.IsDefault)
		{
			foreach (TypeSymbol item5 in item2)
			{
				if ((object)item5 != null && item5.IsStatic)
				{
					Error(diagnostics, ErrorCode.ERR_ParameterIsStaticClass, syntax, item5);
				}
			}
		}
		UnboundLambda unboundLambda = new UnboundLambda(syntax, this, item, item2, item3, item4);
		if (!item3.IsDefault)
		{
			LocalScopeBinder localScopeBinder = new LocalScopeBinder(this);
			PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
			for (int i = 0; i < unboundLambda.ParameterCount; i++)
			{
				string text = unboundLambda.ParameterName(i);
				if (!string.IsNullOrEmpty(text))
				{
					if (!instance.Add(text))
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateParamName, unboundLambda.ParameterLocation(i), text);
					}
					else
					{
						localScopeBinder.ValidateLambdaParameterNameConflictsInScope(unboundLambda.ParameterLocation(i), text, diagnostics);
					}
				}
			}
			instance.Free();
		}
		return unboundLambda;
	}

	internal void LookupSymbolsSimpleName(LookupResult result, NamespaceOrTypeSymbol qualifierOpt, string plainName, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (options.IsAttributeTypeLookup())
		{
			LookupAttributeType(result, qualifierOpt, plainName, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
		}
		else
		{
			LookupSymbolsOrMembersInternal(result, qualifierOpt, plainName, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
		}
	}

	internal void LookupExtensionMethods(LookupResult result, string name, int arity, LookupOptions options, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		foreach (ExtensionMethodScope item in new ExtensionMethodScopes(this))
		{
			LookupExtensionMethodsInSingleBinder(item, result, name, arity, options, ref useSiteDiagnostics);
		}
	}

	private Binder LookupSymbolsWithFallback(LookupResult result, string name, int arity, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null, LookupOptions options = LookupOptions.Default)
	{
		Binder result2 = LookupSymbolsInternal(result, name, arity, basesBeingResolved, options, diagnose: false, ref useSiteDiagnostics);
		if (result.Kind != LookupResultKind.Viable && result.Kind != LookupResultKind.Empty)
		{
			result.Clear();
			LookupSymbolsInternal(result, name, arity, basesBeingResolved, options, diagnose: true, ref useSiteDiagnostics);
		}
		return result2;
	}

	private Binder LookupSymbolsInternal(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Binder binder = null;
		Binder binder2 = this;
		while (binder2 != null && !result.IsMultiViable)
		{
			if (binder != null)
			{
				LookupResult instance = LookupResult.GetInstance();
				binder2.LookupSymbolsInSingleBinder(instance, name, arity, basesBeingResolved, options, this, diagnose, ref useSiteDiagnostics);
				result.MergeEqual(instance);
				instance.Free();
			}
			else
			{
				binder2.LookupSymbolsInSingleBinder(result, name, arity, basesBeingResolved, options, this, diagnose, ref useSiteDiagnostics);
				if (!result.IsClear)
				{
					binder = binder2;
				}
			}
			binder2 = binder2.Next;
		}
		return binder;
	}

	internal virtual void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
	}

	private void LookupSymbolsOrMembersInternal(LookupResult result, NamespaceOrTypeSymbol qualifierOpt, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)qualifierOpt == null)
		{
			LookupSymbolsInternal(result, name, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
		}
		else
		{
			LookupMembersInternal(result, qualifierOpt, name, arity, basesBeingResolved, options, this, diagnose, ref useSiteDiagnostics);
		}
	}

	private void LookupMembersWithFallback(LookupResult result, NamespaceOrTypeSymbol nsOrType, string name, int arity, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null, LookupOptions options = LookupOptions.Default)
	{
		LookupMembersInternal(result, nsOrType, name, arity, basesBeingResolved, options, this, diagnose: false, ref useSiteDiagnostics);
		if (!result.IsMultiViable && !result.IsClear)
		{
			result.Clear();
			LookupMembersInternal(result, nsOrType, name, arity, basesBeingResolved, options, this, diagnose: true, ref useSiteDiagnostics);
		}
	}

	protected void LookupMembersInternal(LookupResult result, NamespaceOrTypeSymbol nsOrType, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (nsOrType.IsNamespace)
		{
			LookupMembersInNamespace(result, (NamespaceSymbol)nsOrType, name, arity, options, originalBinder, diagnose, ref useSiteDiagnostics);
		}
		else
		{
			LookupMembersInType(result, (TypeSymbol)nsOrType, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
		}
	}

	protected void LookupMembersInType(LookupResult result, TypeSymbol type, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		switch (type.TypeKind)
		{
		case TypeKind.TypeParameter:
			LookupMembersInTypeParameter(result, (TypeParameterSymbol)type, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			break;
		case TypeKind.Interface:
			LookupMembersInInterface(result, (NamedTypeSymbol)type, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			break;
		case TypeKind.Array:
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Dynamic:
		case TypeKind.Enum:
		case TypeKind.Struct:
			LookupMembersInClass(result, type, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			break;
		case TypeKind.Submission:
			LookupMembersInSubmissions(result, type, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			break;
		case TypeKind.Error:
			LookupMembersInErrorType(result, (ErrorTypeSymbol)type, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			break;
		case TypeKind.Pointer:
			result.Clear();
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
		}
	}

	private void LookupMembersInErrorType(LookupResult result, ErrorTypeSymbol errorType, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!errorType.CandidateSymbols.IsDefault && errorType.CandidateSymbols.Length == 1 && errorType.ResultKind == LookupResultKind.Inaccessible && errorType.CandidateSymbols.First() is TypeSymbol type)
		{
			LookupMembersInType(result, type, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
		}
		else
		{
			result.Clear();
		}
	}

	private void LookupMembersInSubmissions(LookupResult result, TypeSymbol submissionClass, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		LookupResult instance = LookupResult.GetInstance();
		LookupResult instance2 = LookupResult.GetInstance();
		SymbolKind? symbolKind = null;
		for (CSharpCompilation cSharpCompilation = Compilation; cSharpCompilation != null; cSharpCompilation = cSharpCompilation.PreviousSubmission)
		{
			instance.Clear();
			bool flag = cSharpCompilation == Compilation;
			bool flag2 = !flag || !Flags.Includes(BinderFlags.InScriptUsing);
			Imports imports;
			if (flag2)
			{
				if (Flags.Includes(BinderFlags.InLoadedSyntaxTree))
				{
					imports = ((!flag) ? Imports.Empty : GetImports(basesBeingResolved));
				}
				else
				{
					imports = cSharpCompilation.GetSubmissionImports();
				}
			}
			else
			{
				imports = Imports.Empty;
			}
			if ((options & LookupOptions.NamespaceAliasesOnly) == 0 && (object)cSharpCompilation.ScriptClass != null)
			{
				LookupMembersWithoutInheritance(instance, cSharpCompilation.ScriptClass, name, arity, options, originalBinder, submissionClass, diagnose, ref useSiteDiagnostics, basesBeingResolved);
				if ((instance.IsMultiViable & flag2) && imports.IsUsingAlias(name, originalBinder.IsSemanticModelBinder))
				{
					Symbol symbol = instance.Symbols.First();
					if (symbol.Kind != SymbolKind.NamedType || arity == 0)
					{
						CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_ConflictingAliasAndDefinition, name, symbol.GetKindText());
						ExtendedErrorTypeSymbol symbol2 = new ExtendedErrorTypeSymbol((NamespaceOrTypeSymbol)null, name, arity, (DiagnosticInfo)errorInfo, true, false);
						result.SetFrom(LookupResult.Good(symbol2));
						break;
					}
				}
			}
			if (!instance.IsMultiViable & flag2)
			{
				if (!flag)
				{
					imports = Imports.ExpandPreviousSubmissionImports(imports, Compilation);
				}
				imports.LookupSymbolInAliases(originalBinder, instance, name, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
			}
			if (!symbolKind.HasValue)
			{
				if (!instance.IsMultiViable)
				{
					instance2.MergePrioritized(instance);
				}
				else
				{
					result.MergeEqual(instance);
					Symbol symbol3 = instance.Symbols.First();
					if (!IsMethodOrIndexer(symbol3))
					{
						break;
					}
					options &= ~(LookupOptions.NamespacesOrTypesOnly | LookupOptions.MustBeInvocableIfMember);
					symbolKind = symbol3.Kind;
				}
			}
			else
			{
				if (instance.Symbols.Count > 0 && instance.Symbols.First().Kind != symbolKind.Value)
				{
					break;
				}
				if (instance.IsMultiViable)
				{
					result.MergeEqual(instance);
				}
			}
		}
		if (result.Symbols.Count == 0)
		{
			result.SetFrom(instance2);
		}
		instance.Free();
		instance2.Free();
	}

	private static void LookupMembersInNamespace(LookupResult result, NamespaceSymbol ns, string name, int arity, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		foreach (Symbol candidateMember in GetCandidateMembers(ns, name, options, originalBinder))
		{
			SingleLookupResult result2 = originalBinder.CheckViability(candidateMember, arity, options, null, diagnose, ref useSiteDiagnostics);
			result.MergeEqual(result2);
		}
	}

	private void LookupExtensionMethodsInSingleBinder(ExtensionMethodScope scope, LookupResult result, string name, int arity, LookupOptions options, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		scope.Binder.GetCandidateExtensionMethods(scope.SearchUsingsNotNamespace, instance, name, arity, options, this);
		foreach (MethodSymbol item in instance)
		{
			SingleLookupResult result2 = CheckViability(item, arity, options, null, diagnose: true, ref useSiteDiagnostics);
			result.MergeEqual(result2);
		}
		instance.Free();
	}

	private void LookupAttributeType(LookupResult result, NamespaceOrTypeSymbol qualifierOpt, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		LookupSymbolsOrMembersInternal(result, qualifierOpt, name, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
		bool flag = IsSingleViableAttributeType(result, out var symbol);
		LookupResult lookupResult = null;
		Symbol symbol2 = null;
		bool flag2 = false;
		if (!options.IsVerbatimNameAttributeTypeLookup())
		{
			lookupResult = LookupResult.GetInstance();
			LookupSymbolsOrMembersInternal(lookupResult, qualifierOpt, name + "Attribute", arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
			flag2 = IsSingleViableAttributeType(lookupResult, out symbol2);
		}
		if (flag & flag2)
		{
			result.MergeEqual(lookupResult);
		}
		else if (!flag)
		{
			if (flag2)
			{
				result.SetFrom(lookupResult);
			}
			else
			{
				if (!result.IsClear && (object)symbol != null)
				{
					result.SetFrom(GenerateNonViableAttributeTypeResult(symbol, result.Error, diagnose));
				}
				if (lookupResult != null)
				{
					if (!lookupResult.IsClear && (object)symbol2 != null)
					{
						lookupResult.SetFrom(GenerateNonViableAttributeTypeResult(symbol2, lookupResult.Error, diagnose));
					}
					result.MergePrioritized(lookupResult);
				}
			}
		}
		lookupResult?.Free();
	}

	private bool IsAmbiguousResult(LookupResult result, out Symbol resultSymbol)
	{
		resultSymbol = null;
		ArrayBuilder<Symbol> symbols = result.Symbols;
		switch (symbols.Count)
		{
		case 0:
			return false;
		case 1:
			resultSymbol = symbols[0];
			return false;
		default:
			resultSymbol = ResolveMultipleSymbolsInAttributeTypeLookup(symbols);
			return (object)resultSymbol == null;
		}
	}

	private Symbol ResolveMultipleSymbolsInAttributeTypeLookup(ArrayBuilder<Symbol> symbols)
	{
		ImmutableArray<Symbol> immutableArray = symbols.ToImmutable();
		for (int i = 0; i < symbols.Count; i++)
		{
			symbols[i] = UnwrapAliasNoDiagnostics(symbols[i]);
		}
		BestSymbolInfo bestSymbolInfo = GetBestSymbolInfo(symbols, out var secondBest);
		if (bestSymbolInfo.IsFromCompilation && !secondBest.IsFromCompilation)
		{
			Symbol symbol = symbols[bestSymbolInfo.Index];
			Symbol symbol2 = symbols[secondBest.Index];
			if (symbol.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat) == symbol2.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat))
			{
				return immutableArray[bestSymbolInfo.Index];
			}
		}
		return null;
	}

	private bool IsSingleViableAttributeType(LookupResult result, out Symbol symbol)
	{
		if (IsAmbiguousResult(result, out symbol))
		{
			return false;
		}
		if (result == null || result.Kind != LookupResultKind.Viable || (object)symbol == null)
		{
			return false;
		}
		DiagnosticInfo diagInfo = null;
		return CheckAttributeTypeViability(UnwrapAliasNoDiagnostics(symbol), diagnose: false, ref diagInfo);
	}

	private SingleLookupResult GenerateNonViableAttributeTypeResult(Symbol symbol, DiagnosticInfo diagInfo, bool diagnose)
	{
		symbol = UnwrapAliasNoDiagnostics(symbol);
		CheckAttributeTypeViability(symbol, diagnose, ref diagInfo);
		return LookupResult.NotAnAttributeType(symbol, diagInfo);
	}

	private bool CheckAttributeTypeViability(Symbol symbol, bool diagnose, ref DiagnosticInfo diagInfo)
	{
		if (symbol.Kind == SymbolKind.NamedType)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
			if (namedTypeSymbol.IsGenericType)
			{
				diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_AttributeCantBeGeneric, symbol) : null);
				return false;
			}
			if (namedTypeSymbol.IsAbstract)
			{
				diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_AbstractAttributeClass, symbol) : null);
				return false;
			}
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if (Compilation.IsEqualOrDerivedFromWellKnownClass(namedTypeSymbol, WellKnownType.System_Attribute, ref useSiteDiagnostics))
			{
				return true;
			}
			if (diagnose && !useSiteDiagnostics.IsNullOrEmpty())
			{
				foreach (DiagnosticInfo item in useSiteDiagnostics)
				{
					if (item.Severity == DiagnosticSeverity.Error)
					{
						diagInfo = item;
						return false;
					}
				}
			}
		}
		diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_NotAnAttributeClass, symbol) : null);
		return false;
	}

	internal virtual void GetCandidateExtensionMethods(bool searchUsingsNotNamespace, ArrayBuilder<MethodSymbol> methods, string name, int arity, LookupOptions options, Binder originalBinder)
	{
	}

	protected static void LookupMembersWithoutInheritance(LookupResult result, TypeSymbol type, string name, int arity, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		foreach (Symbol candidateMember in GetCandidateMembers(type, name, options, originalBinder))
		{
			SingleLookupResult result2 = originalBinder.CheckViability(candidateMember, arity, options, accessThroughType, diagnose, ref useSiteDiagnostics, basesBeingResolved);
			result.MergeEqual(result2);
		}
	}

	private void LookupMembersInClass(LookupResult result, TypeSymbol type, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol typeSymbol = type;
		LookupResult instance = LookupResult.GetInstance();
		PooledHashSet<NamedTypeSymbol> visited = null;
		while ((object)typeSymbol != null)
		{
			instance.Clear();
			LookupMembersWithoutInheritance(instance, typeSymbol, name, arity, options, originalBinder, type, diagnose, ref useSiteDiagnostics, basesBeingResolved);
			MergeHidingLookupResults(result, instance, ref useSiteDiagnostics);
			if (typeSymbol is NamedTypeSymbol { ShouldAddWinRTMembers: not false } namedTypeSymbol)
			{
				AddWinRTMembers(result, namedTypeSymbol, name, arity, options, originalBinder, diagnose, ref useSiteDiagnostics);
			}
			bool flag = instance.IsMultiViable && !IsMethodOrIndexer(instance.Symbols[0]);
			if (result.IsMultiViable && (flag || !IsMethodOrIndexer(result.Symbols[0])))
			{
				break;
			}
			if (basesBeingResolved != null && basesBeingResolved.ContainsReference(type.OriginalDefinition))
			{
				Symbol nearestOtherSymbol = GetNearestOtherSymbol(basesBeingResolved, type);
				CSDiagnosticInfo errorInfo = new CSDiagnosticInfo(ErrorCode.ERR_CircularBase, type, nearestOtherSymbol);
				ExtendedErrorTypeSymbol symbol = new ExtendedErrorTypeSymbol(Compilation, name, arity, errorInfo, unreported: true);
				result.SetFrom(LookupResult.Good(symbol));
			}
			if (originalBinder.InCrefButNotParameterOrReturnType)
			{
				break;
			}
			typeSymbol = typeSymbol.GetNextBaseTypeNoUseSiteDiagnostics(basesBeingResolved, Compilation, ref visited);
			typeSymbol?.OriginalDefinition.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		visited?.Free();
		instance.Free();
	}

	private void AddWinRTMembers(LookupResult result, NamedTypeSymbol type, string name, int arity, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		MemberSignatureComparer cSharpOverrideComparer = MemberSignatureComparer.CSharpOverrideComparer;
		HashSet<Symbol> hashSet = new HashSet<Symbol>(cSharpOverrideComparer);
		HashSet<Symbol> hashSet2 = new HashSet<Symbol>(cSharpOverrideComparer);
		if (result.IsMultiViable)
		{
			foreach (Symbol symbol in result.Symbols)
			{
				if (symbol.Kind == SymbolKind.Method || symbol.Kind == SymbolKind.Property)
				{
					hashSet.Add(symbol);
				}
			}
		}
		LookupResult instance = LookupResult.GetInstance();
		GetWellKnownWinRTMemberInterfaces(out var idictSymbol, out var iroDictSymbol, out var iListSymbol, out var iCollectionSymbol, out var inccSymbol, out var inpcSymbol);
		foreach (NamedTypeSymbol item in type.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
		{
			if (!ShouldAddWinRTMembersForInterface(item, idictSymbol, iroDictSymbol, iListSymbol, iCollectionSymbol, inccSymbol, inpcSymbol))
			{
				continue;
			}
			LookupMembersWithoutInheritance(instance, item, name, arity, options, originalBinder, item, diagnose, ref useSiteDiagnostics);
			if (instance.IsMultiViable)
			{
				foreach (Symbol symbol2 in instance.Symbols)
				{
					if (!hashSet.Add(symbol2))
					{
						hashSet2.Add(symbol2);
					}
				}
			}
			instance.Clear();
		}
		instance.Free();
		if (result.IsMultiViable)
		{
			foreach (Symbol symbol3 in result.Symbols)
			{
				if (symbol3.Kind == SymbolKind.Method || symbol3.Kind == SymbolKind.Property)
				{
					hashSet.Remove(symbol3);
					hashSet2.Remove(symbol3);
				}
			}
		}
		foreach (Symbol item2 in hashSet)
		{
			if (!hashSet2.Contains(item2))
			{
				result.MergeEqual(new SingleLookupResult(LookupResultKind.Viable, item2, null));
			}
		}
	}

	private void GetWellKnownWinRTMemberInterfaces(out NamedTypeSymbol idictSymbol, out NamedTypeSymbol iroDictSymbol, out NamedTypeSymbol iListSymbol, out NamedTypeSymbol iCollectionSymbol, out NamedTypeSymbol inccSymbol, out NamedTypeSymbol inpcSymbol)
	{
		idictSymbol = Compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IDictionary_KV);
		iroDictSymbol = Compilation.GetWellKnownType(WellKnownType.System_Collections_Generic_IReadOnlyDictionary_KV);
		iListSymbol = Compilation.GetWellKnownType(WellKnownType.System_Collections_IList);
		iCollectionSymbol = Compilation.GetWellKnownType(WellKnownType.System_Collections_ICollection);
		inccSymbol = Compilation.GetWellKnownType(WellKnownType.System_Collections_Specialized_INotifyCollectionChanged);
		inpcSymbol = Compilation.GetWellKnownType(WellKnownType.System_ComponentModel_INotifyPropertyChanged);
	}

	private static bool ShouldAddWinRTMembersForInterface(NamedTypeSymbol iface, NamedTypeSymbol idictSymbol, NamedTypeSymbol iroDictSymbol, NamedTypeSymbol iListSymbol, NamedTypeSymbol iCollectionSymbol, NamedTypeSymbol inccSymbol, NamedTypeSymbol inpcSymbol)
	{
		NamedTypeSymbol originalDefinition = iface.OriginalDefinition;
		SpecialType specialType = originalDefinition.SpecialType;
		if (specialType != SpecialType.System_Collections_Generic_IEnumerable_T && specialType != SpecialType.System_Collections_Generic_IList_T && specialType != SpecialType.System_Collections_Generic_ICollection_T && !(originalDefinition == idictSymbol) && specialType != SpecialType.System_Collections_Generic_IReadOnlyList_T && specialType != SpecialType.System_Collections_Generic_IReadOnlyCollection_T && !(originalDefinition == iroDictSymbol) && specialType != SpecialType.System_Collections_IEnumerable && !(originalDefinition == iListSymbol) && !(originalDefinition == iCollectionSymbol) && !(originalDefinition == inccSymbol))
		{
			return originalDefinition == inpcSymbol;
		}
		return true;
	}

	private static Symbol GetNearestOtherSymbol(ConsList<Symbol> list, TypeSymbol type)
	{
		Symbol symbol = type;
		while (list != null && list != ConsList<Symbol>.Empty)
		{
			if (list.Head == type.OriginalDefinition)
			{
				if (symbol == type && list.Tail != null && list.Tail != ConsList<Symbol>.Empty)
				{
					symbol = list.Tail.Head;
				}
				break;
			}
			symbol = list.Head;
			list = list.Tail;
		}
		return symbol;
	}

	private static void LookupMembersInInterfaceOnly(LookupResult current, NamedTypeSymbol type, string name, int arity, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		LookupMembersWithoutInheritance(current, type, name, arity, options, originalBinder, accessThroughType, diagnose, ref useSiteDiagnostics);
		if ((options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly)) == 0 && !originalBinder.InCrefButNotParameterOrReturnType)
		{
			LookupMembersInInterfacesWithoutInheritance(current, type.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics), name, arity, options, originalBinder, accessThroughType, diagnose, ref useSiteDiagnostics);
		}
	}

	private static void LookupMembersInInterfacesWithoutInheritance(LookupResult current, ImmutableArray<NamedTypeSymbol> interfaces, string name, int arity, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (interfaces.Length > 0)
		{
			LookupResult instance = LookupResult.GetInstance();
			foreach (NamedTypeSymbol item in interfaces)
			{
				LookupMembersWithoutInheritance(instance, item, name, arity, options, originalBinder, accessThroughType, diagnose, ref useSiteDiagnostics);
				MergeHidingLookupResults(current, instance, ref useSiteDiagnostics);
				instance.Clear();
			}
			instance.Free();
		}
	}

	private void LookupMembersInInterface(LookupResult current, NamedTypeSymbol type, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		LookupMembersInInterfaceOnly(current, type, name, arity, options, originalBinder, type, diagnose, ref useSiteDiagnostics);
		if (!originalBinder.InCrefButNotParameterOrReturnType)
		{
			LookupResult instance = LookupResult.GetInstance();
			LookupMembersInClass(instance, Compilation.GetSpecialType(SpecialType.System_Object), name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			MergeHidingLookupResults(current, instance, ref useSiteDiagnostics);
			instance.Free();
		}
	}

	private void LookupMembersInTypeParameter(LookupResult current, TypeParameterSymbol typeParameter, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((options & (LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly)) == 0)
		{
			LookupMembersInClass(current, typeParameter.EffectiveBaseClass(ref useSiteDiagnostics), name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
			LookupMembersInInterfacesWithoutInheritance(current, typeParameter.AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics), name, arity, options, originalBinder, typeParameter, diagnose, ref useSiteDiagnostics);
		}
	}

	private static bool IsDerivedType(NamedTypeSymbol baseType, NamedTypeSymbol derivedType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol namedTypeSymbol = derivedType.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		while ((object)namedTypeSymbol != null)
		{
			if (namedTypeSymbol == baseType)
			{
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		if (baseType.IsInterface)
		{
			return derivedType.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics).Contains(baseType);
		}
		return false;
	}

	private static void MergeHidingLookupResults(LookupResult resultHiding, LookupResult resultHidden, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (resultHiding.IsMultiViable && resultHidden.IsMultiViable)
		{
			ArrayBuilder<Symbol> symbols = resultHiding.Symbols;
			int count = symbols.Count;
			ArrayBuilder<Symbol> symbols2 = resultHidden.Symbols;
			int count2 = symbols2.Count;
			for (int i = 0; i < count2; i++)
			{
				Symbol symbol = symbols2[i];
				NamedTypeSymbol containingType = symbol.ContainingType;
				int num = 0;
				while (true)
				{
					if (num < count)
					{
						Symbol symbol2 = symbols[num];
						if ((!symbol2.ContainingType.IsInterface || IsDerivedType(containingType, symbol2.ContainingType, ref useSiteDiagnostics) || containingType.SpecialType == SpecialType.System_Object) && (!IsMethodOrIndexer(symbol2) || !IsMethodOrIndexer(symbol)))
						{
							break;
						}
						num++;
						continue;
					}
					symbols.Add(symbol);
					break;
				}
			}
		}
		else
		{
			resultHiding.MergePrioritized(resultHidden);
		}
	}

	private static bool IsMethodOrIndexer(Symbol symbol)
	{
		if (symbol.Kind != SymbolKind.Method)
		{
			return symbol.IsIndexer();
		}
		return true;
	}

	internal static ImmutableArray<Symbol> GetCandidateMembers(NamespaceOrTypeSymbol nsOrType, string name, LookupOptions options, Binder originalBinder)
	{
		if ((options & LookupOptions.NamespacesOrTypesOnly) != 0 && nsOrType is TypeSymbol)
		{
			return nsOrType.GetTypeMembers(name).Cast<NamedTypeSymbol, Symbol>();
		}
		if (nsOrType.Kind == SymbolKind.NamedType && originalBinder.IsEarlyAttributeBinder)
		{
			return ((NamedTypeSymbol)nsOrType).GetEarlyAttributeDecodingMembers(name);
		}
		if ((options & LookupOptions.LabelsOnly) != 0)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return nsOrType.GetMembers(name);
	}

	internal static ImmutableArray<Symbol> GetCandidateMembers(NamespaceOrTypeSymbol nsOrType, LookupOptions options, Binder originalBinder)
	{
		if ((options & LookupOptions.NamespacesOrTypesOnly) != 0 && nsOrType is TypeSymbol)
		{
			return StaticCast<Symbol>.From(nsOrType.GetTypeMembersUnordered());
		}
		if (nsOrType.Kind == SymbolKind.NamedType && originalBinder.IsEarlyAttributeBinder)
		{
			return ((NamedTypeSymbol)nsOrType).GetEarlyAttributeDecodingMembers();
		}
		if ((options & LookupOptions.LabelsOnly) != 0)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return nsOrType.GetMembersUnordered();
	}

	internal SingleLookupResult CheckViability(Symbol symbol, int arity, LookupOptions options, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		Symbol unwrappedSymbol = ((symbol.Kind == SymbolKind.Alias) ? ((AliasSymbol)symbol).GetAliasTarget(basesBeingResolved) : symbol);
		if (!Compilation.SourceModule.Equals(unwrappedSymbol.ContainingModule) && unwrappedSymbol.IsHiddenByCodeAnalysisEmbeddedAttribute())
		{
			return LookupResult.Empty();
		}
		if (WrongArity(symbol, arity, diagnose, options, out var diagInfo))
		{
			return LookupResult.WrongArity(symbol, diagInfo);
		}
		if (!InCref && !unwrappedSymbol.CanBeReferencedByNameIgnoringIllegalCharacters)
		{
			diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_CantCallSpecialMethod, unwrappedSymbol) : null);
			return LookupResult.NotReferencable(symbol, diagInfo);
		}
		if ((options & LookupOptions.NamespacesOrTypesOnly) != 0 && !(unwrappedSymbol is NamespaceOrTypeSymbol))
		{
			return LookupResult.NotTypeOrNamespace(unwrappedSymbol, symbol, diagnose);
		}
		if ((options & LookupOptions.MustBeInvocableIfMember) != 0 && IsNonInvocableMember(unwrappedSymbol))
		{
			return LookupResult.NotInvocable(unwrappedSymbol, symbol, diagnose);
		}
		if (InCref && !IsCrefAccessible(unwrappedSymbol))
		{
			ImmutableArray<Symbol> symbols = ImmutableArray.Create(unwrappedSymbol);
			object obj;
			if (!diagnose)
			{
				obj = null;
			}
			else
			{
				object[] args = new Symbol[1] { unwrappedSymbol };
				obj = new CSDiagnosticInfo(ErrorCode.ERR_BadAccess, args, symbols, ImmutableArray<Location>.Empty);
			}
			diagInfo = (DiagnosticInfo)obj;
			return LookupResult.Inaccessible(symbol, diagInfo);
		}
		if (!InCref && !IsAccessible(unwrappedSymbol, RefineAccessThroughType(options, accessThroughType), out var failedThroughTypeCheck, ref useSiteDiagnostics, basesBeingResolved))
		{
			if (!diagnose)
			{
				diagInfo = null;
			}
			else if (failedThroughTypeCheck)
			{
				diagInfo = new CSDiagnosticInfo(ErrorCode.ERR_BadProtectedAccess, unwrappedSymbol, accessThroughType, ContainingType);
			}
			else if (IsBadIvtSpecification())
			{
				diagInfo = new CSDiagnosticInfo(ErrorCode.ERR_FriendRefNotEqualToThis, unwrappedSymbol.ContainingAssembly.Identity.ToString(), AssemblyIdentity.PublicKeyToString(Compilation.Assembly.PublicKey));
			}
			else
			{
				object[] args = new Symbol[1] { unwrappedSymbol };
				diagInfo = new CSDiagnosticInfo(ErrorCode.ERR_BadAccess, args, ImmutableArray.Create(unwrappedSymbol), ImmutableArray<Location>.Empty);
			}
			return LookupResult.Inaccessible(symbol, diagInfo);
		}
		if (!InCref && unwrappedSymbol.MustCallMethodsDirectly())
		{
			diagInfo = (diagnose ? MakeCallMethodsDirectlyDiagnostic(unwrappedSymbol) : null);
			return LookupResult.NotReferencable(symbol, diagInfo);
		}
		if ((options & LookupOptions.MustBeInstance) != 0 && !IsInstance(unwrappedSymbol))
		{
			diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_ObjectRequired, unwrappedSymbol) : null);
			return LookupResult.StaticInstanceMismatch(symbol, diagInfo);
		}
		if ((options & LookupOptions.MustNotBeInstance) != 0 && IsInstance(unwrappedSymbol))
		{
			diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_ObjectProhibited, unwrappedSymbol) : null);
			return LookupResult.StaticInstanceMismatch(symbol, diagInfo);
		}
		if ((options & LookupOptions.MustNotBeNamespace) != 0 && unwrappedSymbol.Kind == SymbolKind.Namespace)
		{
			diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_BadSKunknown, unwrappedSymbol, unwrappedSymbol.GetKindText()) : null);
			return LookupResult.NotTypeOrNamespace(symbol, diagInfo);
		}
		if ((options & LookupOptions.LabelsOnly) != 0 && unwrappedSymbol.Kind != SymbolKind.Label)
		{
			diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_LabelNotFound, unwrappedSymbol.Name) : null);
			return LookupResult.NotLabel(symbol, diagInfo);
		}
		return LookupResult.Good(symbol);
		bool IsBadIvtSpecification()
		{
			if ((unwrappedSymbol.DeclaredAccessibility == Accessibility.Internal || unwrappedSymbol.DeclaredAccessibility == Accessibility.ProtectedAndInternal || unwrappedSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal) && !options.IsAttributeTypeLookup())
			{
				IEnumerable<ImmutableArray<byte>> internalsVisibleToPublicKeys = unwrappedSymbol.ContainingAssembly.GetInternalsVisibleToPublicKeys(Compilation.AssemblyName);
				if (!internalsVisibleToPublicKeys.Any())
				{
					return false;
				}
				foreach (ImmutableArray<byte> item in internalsVisibleToPublicKeys)
				{
					if (item.SequenceEqual(Compilation.Assembly.Identity.PublicKey))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}

	private CSDiagnosticInfo MakeCallMethodsDirectlyDiagnostic(Symbol symbol)
	{
		MethodSymbol methodSymbol;
		MethodSymbol methodSymbol2;
		switch (symbol.Kind)
		{
		case SymbolKind.Property:
		{
			PropertySymbol leastOverriddenProperty = ((PropertySymbol)symbol).GetLeastOverriddenProperty(ContainingType);
			methodSymbol = leastOverriddenProperty.GetMethod;
			methodSymbol2 = leastOverriddenProperty.SetMethod;
			break;
		}
		case SymbolKind.Event:
		{
			EventSymbol leastOverriddenEvent = ((EventSymbol)symbol).GetLeastOverriddenEvent(ContainingType);
			methodSymbol = leastOverriddenEvent.AddMethod;
			methodSymbol2 = leastOverriddenEvent.RemoveMethod;
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
		if ((object)methodSymbol == null || (object)methodSymbol2 == null)
		{
			return new CSDiagnosticInfo(ErrorCode.ERR_BindToBogusProp1, symbol, methodSymbol ?? methodSymbol2);
		}
		return new CSDiagnosticInfo(ErrorCode.ERR_BindToBogusProp2, symbol, methodSymbol, methodSymbol2);
	}

	internal void CheckViability<TSymbol>(LookupResult result, ImmutableArray<TSymbol> symbols, int arity, LookupOptions options, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved) where TSymbol : Symbol
	{
		foreach (TSymbol item in symbols)
		{
			SingleLookupResult result2 = CheckViability(item, arity, options, accessThroughType, diagnose, ref useSiteDiagnostics, basesBeingResolved);
			result.MergeEqual(result2);
		}
	}

	internal bool CanAddLookupSymbolInfo(Symbol symbol, LookupOptions options, LookupSymbolsInfo info, TypeSymbol accessThroughType, AliasSymbol aliasSymbol = null)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		string name = ((aliasSymbol != null) ? aliasSymbol.Name : symbol.Name);
		if (!info.CanBeAdded(name))
		{
			return false;
		}
		if ((options & LookupOptions.NamespacesOrTypesOnly) != 0 && !(symbol is NamespaceOrTypeSymbol))
		{
			return false;
		}
		if ((options & LookupOptions.MustBeInvocableIfMember) != 0 && IsNonInvocableMember(symbol))
		{
			return false;
		}
		if (InCref ? (!IsCrefAccessible(symbol)) : (!IsAccessible(symbol, ref useSiteDiagnostics, RefineAccessThroughType(options, accessThroughType))))
		{
			return false;
		}
		if ((options & LookupOptions.MustBeInstance) != 0 && !IsInstance(symbol))
		{
			return false;
		}
		if ((options & LookupOptions.MustNotBeInstance) != 0 && IsInstance(symbol))
		{
			return false;
		}
		if ((options & LookupOptions.MustNotBeNamespace) != 0 && symbol.Kind == SymbolKind.Namespace)
		{
			return false;
		}
		return true;
	}

	private static TypeSymbol RefineAccessThroughType(LookupOptions options, TypeSymbol accessThroughType)
	{
		if ((options & LookupOptions.UseBaseReferenceAccessibility) == 0)
		{
			return accessThroughType;
		}
		return null;
	}

	private bool IsCrefAccessible(Symbol symbol)
	{
		if (IsEffectivelyPrivate(symbol))
		{
			return symbol.ContainingAssembly == Compilation.Assembly;
		}
		return true;
	}

	private static bool IsEffectivelyPrivate(Symbol symbol)
	{
		Symbol symbol2 = symbol;
		while ((object)symbol2 != null)
		{
			if (symbol2.DeclaredAccessibility == Accessibility.Private)
			{
				return true;
			}
			symbol2 = symbol2.ContainingSymbol;
		}
		return false;
	}

	internal bool IsAccessible(Symbol symbol, ref HashSet<DiagnosticInfo> useSiteDiagnostics, TypeSymbol accessThroughType = null, ConsList<Symbol> basesBeingResolved = null)
	{
		bool failedThroughTypeCheck;
		return IsAccessible(symbol, accessThroughType, out failedThroughTypeCheck, ref useSiteDiagnostics, basesBeingResolved);
	}

	internal bool IsAccessible(Symbol symbol, TypeSymbol accessThroughType, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		if (Flags.Includes(BinderFlags.IgnoreAccessibility))
		{
			failedThroughTypeCheck = false;
			return true;
		}
		return IsAccessibleHelper(symbol, accessThroughType, out failedThroughTypeCheck, ref useSiteDiagnostics, basesBeingResolved);
	}

	internal virtual bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved)
	{
		return Next.IsAccessibleHelper(symbol, accessThroughType, out failedThroughTypeCheck, ref useSiteDiagnostics, basesBeingResolved);
	}

	internal bool IsNonInvocableMember(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Method:
		case SymbolKind.NamedType:
		case SymbolKind.Property:
			return !IsInvocableMember(symbol);
		default:
			return false;
		}
	}

	private bool IsInvocableMember(Symbol symbol)
	{
		TypeSymbol typeSymbol = null;
		switch (symbol.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Method:
			return true;
		case SymbolKind.Field:
			typeSymbol = ((FieldSymbol)symbol).GetFieldType(FieldsBeingBound);
			break;
		case SymbolKind.Property:
			typeSymbol = ((PropertySymbol)symbol).Type;
			break;
		}
		if ((object)typeSymbol != null)
		{
			if (!typeSymbol.IsDelegateType())
			{
				return typeSymbol.IsDynamic();
			}
			return true;
		}
		return false;
	}

	private static bool IsInstance(Symbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if ((uint)(kind - 5) <= 1u || kind == SymbolKind.Method || kind == SymbolKind.Property)
		{
			return !symbol.IsStatic;
		}
		return false;
	}

	private static bool WrongArity(Symbol symbol, int arity, bool diagnose, LookupOptions options, out DiagnosticInfo diagInfo)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.NamedType:
		{
			if (arity == 0 && (options & LookupOptions.AllNamedTypesOnArityZero) != 0)
			{
				break;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)symbol;
			if (namedTypeSymbol.Arity != arity || (options.IsAttributeTypeLookup() && arity != 0))
			{
				if (namedTypeSymbol.Arity == 0)
				{
					diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_HasNoTypeVars, namedTypeSymbol, MessageID.IDS_SK_TYPE.Localize()) : null);
				}
				else
				{
					diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_BadArity, namedTypeSymbol, MessageID.IDS_SK_TYPE.Localize(), namedTypeSymbol.Arity) : null);
				}
				return true;
			}
			break;
		}
		case SymbolKind.Method:
		{
			if (arity == 0 && (options & LookupOptions.AllMethodsOnArityZero) != 0)
			{
				break;
			}
			MethodSymbol methodSymbol = (MethodSymbol)symbol;
			if (methodSymbol.Arity != arity)
			{
				if (methodSymbol.Arity == 0)
				{
					diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_HasNoTypeVars, methodSymbol, MessageID.IDS_SK_METHOD.Localize()) : null);
				}
				else
				{
					diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_BadArity, methodSymbol, MessageID.IDS_SK_METHOD.Localize(), methodSymbol.Arity) : null);
				}
				return true;
			}
			break;
		}
		default:
			if (arity != 0)
			{
				diagInfo = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_TypeArgsNotAllowed, symbol, symbol.Kind.Localize()) : null);
				return true;
			}
			break;
		}
		diagInfo = null;
		return false;
	}

	internal void AddLookupSymbolsInfo(LookupSymbolsInfo result, LookupOptions options = LookupOptions.Default)
	{
		for (Binder binder = this; binder != null; binder = binder.Next)
		{
			binder.AddLookupSymbolsInfoInSingleBinder(result, options, this);
		}
	}

	protected virtual void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo info, LookupOptions options, Binder originalBinder)
	{
	}

	internal void AddMemberLookupSymbolsInfo(LookupSymbolsInfo result, NamespaceOrTypeSymbol nsOrType, LookupOptions options, Binder originalBinder)
	{
		if (nsOrType.IsNamespace)
		{
			AddMemberLookupSymbolsInfoInNamespace(result, (NamespaceSymbol)nsOrType, options, originalBinder);
		}
		else
		{
			AddMemberLookupSymbolsInfoInType(result, (TypeSymbol)nsOrType, options, originalBinder);
		}
	}

	private void AddMemberLookupSymbolsInfoInType(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder)
	{
		switch (type.TypeKind)
		{
		case TypeKind.TypeParameter:
			AddMemberLookupSymbolsInfoInTypeParameter(result, (TypeParameterSymbol)type, options, originalBinder);
			break;
		case TypeKind.Interface:
			AddMemberLookupSymbolsInfoInInterface(result, type, options, originalBinder, type);
			break;
		case TypeKind.Array:
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Dynamic:
		case TypeKind.Enum:
		case TypeKind.Struct:
			AddMemberLookupSymbolsInfoInClass(result, type, options, originalBinder, type);
			break;
		case TypeKind.Submission:
			AddMemberLookupSymbolsInfoInSubmissions(result, type, options, originalBinder);
			break;
		case TypeKind.Error:
		case TypeKind.Module:
		case TypeKind.Pointer:
			break;
		}
	}

	private void AddMemberLookupSymbolsInfoInSubmissions(LookupSymbolsInfo result, TypeSymbol scriptClass, LookupOptions options, Binder originalBinder)
	{
		for (CSharpCompilation cSharpCompilation = Compilation; cSharpCompilation != null; cSharpCompilation = cSharpCompilation.PreviousSubmission)
		{
			if ((object)cSharpCompilation.ScriptClass != null)
			{
				AddMemberLookupSymbolsInfoWithoutInheritance(result, cSharpCompilation.ScriptClass, options, originalBinder, scriptClass);
			}
			bool flag = cSharpCompilation == Compilation;
			if ((options & LookupOptions.LabelsOnly) == 0 && (!flag || !Flags.Includes(BinderFlags.InScriptUsing)))
			{
				Imports imports = cSharpCompilation.GetSubmissionImports();
				if (!flag)
				{
					imports = Imports.ExpandPreviousSubmissionImports(imports, Compilation);
				}
				imports.AddLookupSymbolsInfoInAliases(result, options, originalBinder);
			}
		}
	}

	private static void AddMemberLookupSymbolsInfoInNamespace(LookupSymbolsInfo result, NamespaceSymbol ns, LookupOptions options, Binder originalBinder)
	{
		foreach (Symbol item in (result.FilterName != null) ? GetCandidateMembers(ns, result.FilterName, options, originalBinder) : GetCandidateMembers(ns, options, originalBinder))
		{
			if (originalBinder.CanAddLookupSymbolInfo(item, options, result, null))
			{
				result.AddSymbol(item, item.Name, item.GetArity());
			}
		}
	}

	private static void AddMemberLookupSymbolsInfoWithoutInheritance(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType)
	{
		foreach (Symbol item in (result.FilterName != null) ? GetCandidateMembers(type, result.FilterName, options, originalBinder) : GetCandidateMembers(type, options, originalBinder))
		{
			if (originalBinder.CanAddLookupSymbolInfo(item, options, result, accessThroughType))
			{
				result.AddSymbol(item, item.Name, item.GetArity());
			}
		}
	}

	private void AddWinRTMembersLookupSymbolsInfo(LookupSymbolsInfo result, NamedTypeSymbol type, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType)
	{
		GetWellKnownWinRTMemberInterfaces(out var idictSymbol, out var iroDictSymbol, out var iListSymbol, out var iCollectionSymbol, out var inccSymbol, out var inpcSymbol);
		foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in type.AllInterfacesNoUseSiteDiagnostics)
		{
			if (ShouldAddWinRTMembersForInterface(allInterfacesNoUseSiteDiagnostic, idictSymbol, iroDictSymbol, iListSymbol, iCollectionSymbol, inccSymbol, inpcSymbol))
			{
				AddMemberLookupSymbolsInfoWithoutInheritance(result, allInterfacesNoUseSiteDiagnostic, options, originalBinder, accessThroughType);
			}
		}
	}

	private void AddMemberLookupSymbolsInfoInClass(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType)
	{
		PooledHashSet<NamedTypeSymbol> visited = null;
		while ((object)type != null && type.SpecialType != SpecialType.System_Void)
		{
			AddMemberLookupSymbolsInfoWithoutInheritance(result, type, options, originalBinder, accessThroughType);
			if (type is NamedTypeSymbol { ShouldAddWinRTMembers: not false } namedTypeSymbol)
			{
				AddWinRTMembersLookupSymbolsInfo(result, namedTypeSymbol, options, originalBinder, accessThroughType);
			}
			if (originalBinder.InCrefButNotParameterOrReturnType)
			{
				break;
			}
			type = type.GetNextBaseTypeNoUseSiteDiagnostics(null, Compilation, ref visited);
		}
		visited?.Free();
	}

	private void AddMemberLookupSymbolsInfoInInterface(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType)
	{
		AddMemberLookupSymbolsInfoWithoutInheritance(result, type, options, originalBinder, accessThroughType);
		if (!originalBinder.InCrefButNotParameterOrReturnType)
		{
			foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in type.AllInterfacesNoUseSiteDiagnostics)
			{
				AddMemberLookupSymbolsInfoWithoutInheritance(result, allInterfacesNoUseSiteDiagnostic, options, originalBinder, accessThroughType);
			}
			AddMemberLookupSymbolsInfoInClass(result, Compilation.GetSpecialType(SpecialType.System_Object), options, originalBinder, accessThroughType);
		}
	}

	private void AddMemberLookupSymbolsInfoInTypeParameter(LookupSymbolsInfo result, TypeParameterSymbol type, LookupOptions options, Binder originalBinder)
	{
		if (type.TypeParameterKind != TypeParameterKind.Cref)
		{
			NamedTypeSymbol effectiveBaseClassNoUseSiteDiagnostics = type.EffectiveBaseClassNoUseSiteDiagnostics;
			AddMemberLookupSymbolsInfoInClass(result, effectiveBaseClassNoUseSiteDiagnostics, options, originalBinder, effectiveBaseClassNoUseSiteDiagnostics);
			foreach (NamedTypeSymbol allEffectiveInterfacesNoUseSiteDiagnostic in type.AllEffectiveInterfacesNoUseSiteDiagnostics)
			{
				AddMemberLookupSymbolsInfoWithoutInheritance(result, allEffectiveInterfacesNoUseSiteDiagnostic, options, originalBinder, type);
			}
		}
	}

	private bool ValidateLambdaParameterNameConflictsInScope(Location location, string name, DiagnosticBag diagnostics)
	{
		return ValidateNameConflictsInScope(null, location, name, diagnostics);
	}

	internal bool ValidateDeclarationNameConflictsInScope(Symbol symbol, DiagnosticBag diagnostics)
	{
		Location location = GetLocation(symbol);
		return ValidateNameConflictsInScope(symbol, location, symbol.Name, diagnostics);
	}

	private static Location GetLocation(Symbol symbol)
	{
		ImmutableArray<Location> locations = symbol.Locations;
		if (locations.Length == 0)
		{
			return symbol.ContainingSymbol.Locations[0];
		}
		return locations[0];
	}

	internal void ValidateParameterNameConflicts(ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<ParameterSymbol> parameters, DiagnosticBag diagnostics)
	{
		PooledHashSet<string> pooledHashSet = null;
		if (!typeParameters.IsDefaultOrEmpty)
		{
			pooledHashSet = PooledHashSet<string>.GetInstance();
			foreach (TypeParameterSymbol item in typeParameters)
			{
				string name = item.Name;
				if (!string.IsNullOrEmpty(name) && pooledHashSet.Add(name))
				{
					ValidateDeclarationNameConflictsInScope(item, diagnostics);
				}
			}
		}
		PooledHashSet<string> pooledHashSet2 = null;
		if (!parameters.IsDefaultOrEmpty)
		{
			pooledHashSet2 = PooledHashSet<string>.GetInstance();
			foreach (ParameterSymbol item2 in parameters)
			{
				string name2 = item2.Name;
				if (!string.IsNullOrEmpty(name2))
				{
					if (pooledHashSet != null && pooledHashSet.Contains(name2))
					{
						diagnostics.Add(ErrorCode.ERR_LocalSameNameAsTypeParam, GetLocation(item2), name2);
					}
					if (!pooledHashSet2.Add(name2))
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateParamName, GetLocation(item2), name2);
					}
					else
					{
						ValidateDeclarationNameConflictsInScope(item2, diagnostics);
					}
				}
			}
		}
		pooledHashSet?.Free();
		pooledHashSet2?.Free();
	}

	protected bool ValidateNameConflictsInScope(Symbol symbol, Location location, string name, DiagnosticBag diagnostics)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		bool flag = false;
		Binder binder = this;
		while (binder != null && !((binder is InContainerBinder) | flag))
		{
			if (binder is LocalScopeBinder localScopeBinder)
			{
				flag |= localScopeBinder.EnsureSingleDefinition(symbol, name, location, diagnostics);
			}
			binder = binder.Next;
		}
		return flag;
	}

	private BoundExpression BindCompoundAssignment(AssignmentExpressionSyntax node, DiagnosticBag diagnostics)
	{
		node.Left.CheckDeconstructionCompatibleArgument(diagnostics);
		BoundExpression boundExpression = BindValue(node.Left, diagnostics, GetBinaryAssignmentKind(node.Kind()));
		BoundExpression boundExpression2 = BindValue(node.Right, diagnostics, BindValueKind.RValue);
		BinaryOperatorKind kind = SyntaxKindToBinaryOperatorKind(node.Kind());
		if (boundExpression.Kind == BoundKind.EventAccess)
		{
			BinaryOperatorKind binaryOperatorKind = kind.Operator();
			if (binaryOperatorKind == BinaryOperatorKind.Addition || binaryOperatorKind == BinaryOperatorKind.Subtraction)
			{
				return BindEventAssignment(node, (BoundEventAccess)boundExpression, boundExpression2, binaryOperatorKind, diagnostics);
			}
		}
		if (boundExpression.HasAnyErrors || boundExpression2.HasAnyErrors)
		{
			return new BoundCompoundAssignmentOperator(node, BinaryOperatorSignature.Error, boundExpression, boundExpression2, Conversion.NoConversion, Conversion.NoConversion, LookupResultKind.Empty, CreateErrorType(), hasErrors: true);
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (boundExpression.HasDynamicType() || boundExpression2.HasDynamicType())
		{
			if (IsLegalDynamicOperand(boundExpression2) && IsLegalDynamicOperand(boundExpression))
			{
				Conversion finalConversion = Compilation.Conversions.ClassifyConversionFromExpression(boundExpression2, boundExpression.Type, ref useSiteDiagnostics);
				diagnostics.Add(node, useSiteDiagnostics);
				return new BoundCompoundAssignmentOperator(node, new BinaryOperatorSignature(kind.WithType(BinaryOperatorKind.Dynamic).WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), boundExpression.Type, boundExpression2.Type, Compilation.DynamicType), boundExpression, boundExpression2, Conversion.NoConversion, finalConversion, LookupResultKind.Viable, boundExpression.Type);
			}
			Error(diagnostics, ErrorCode.ERR_BadBinaryOps, node, node.OperatorToken.Text, boundExpression.Display, boundExpression2.Display);
			return new BoundCompoundAssignmentOperator(node, BinaryOperatorSignature.Error, boundExpression, boundExpression2, Conversion.NoConversion, Conversion.NoConversion, LookupResultKind.Empty, CreateErrorType(), hasErrors: true);
		}
		if (boundExpression.Kind == BoundKind.EventAccess && !CheckEventValueKind((BoundEventAccess)boundExpression, BindValueKind.Assignable, diagnostics))
		{
			return new BoundCompoundAssignmentOperator(node, BinaryOperatorSignature.Error, boundExpression, boundExpression2, Conversion.NoConversion, Conversion.NoConversion, LookupResultKind.NotAVariable, CreateErrorType(), hasErrors: true);
		}
		BinaryOperatorAnalysisResult binaryOperatorAnalysisResult = BinaryOperatorOverloadResolution(kind, boundExpression, boundExpression2, node, diagnostics, out var resultKind, out var originalUserDefinedOperators);
		if (!binaryOperatorAnalysisResult.HasValue)
		{
			ReportAssignmentOperatorError(node, diagnostics, boundExpression, boundExpression2, resultKind);
			return new BoundCompoundAssignmentOperator(node, BinaryOperatorSignature.Error, boundExpression, boundExpression2, Conversion.NoConversion, Conversion.NoConversion, resultKind, originalUserDefinedOperators, CreateErrorType(), hasErrors: true);
		}
		bool flag = false;
		BinaryOperatorSignature binaryOperatorSignature = binaryOperatorAnalysisResult.Signature;
		if (CheckOverflowAtRuntime)
		{
			binaryOperatorSignature = new BinaryOperatorSignature(binaryOperatorSignature.Kind.WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), binaryOperatorSignature.LeftType, binaryOperatorSignature.RightType, binaryOperatorSignature.ReturnType, binaryOperatorSignature.Method);
		}
		BoundExpression right = CreateConversion(boundExpression2, binaryOperatorAnalysisResult.RightConversion, binaryOperatorSignature.RightType, diagnostics);
		TypeSymbol type = boundExpression.Type;
		Conversion conversion = Conversions.ClassifyConversionFromExpressionType(binaryOperatorSignature.ReturnType, type, ref useSiteDiagnostics);
		bool flag2 = !binaryOperatorSignature.Kind.IsUserDefined();
		if (!conversion.IsValid || (conversion.IsExplicit && !flag2))
		{
			flag = true;
			GenerateImplicitConversionError(diagnostics, Compilation, node, conversion, binaryOperatorSignature.ReturnType, type);
		}
		else
		{
			ReportDiagnosticsIfObsolete(diagnostics, conversion, node, hasBaseReceiver: false);
		}
		if ((conversion.IsExplicit & flag2) && !kind.IsShift())
		{
			Conversion conversion2 = Conversions.ClassifyConversionFromExpression(boundExpression2, type, ref useSiteDiagnostics);
			if (!conversion2.IsImplicit || !conversion2.IsValid)
			{
				flag = true;
				GenerateImplicitConversionError(diagnostics, node, conversion2, boundExpression2, type);
			}
		}
		diagnostics.Add(node, useSiteDiagnostics);
		if (!flag && type.IsVoidPointer())
		{
			Error(diagnostics, ErrorCode.ERR_VoidError, node);
			flag = true;
		}
		Conversion leftConversion = binaryOperatorAnalysisResult.LeftConversion;
		ReportDiagnosticsIfObsolete(diagnostics, leftConversion, node, hasBaseReceiver: false);
		return new BoundCompoundAssignmentOperator(node, binaryOperatorSignature, boundExpression, right, leftConversion, conversion, resultKind, originalUserDefinedOperators, type, flag);
	}

	private BoundExpression BindEventAssignment(AssignmentExpressionSyntax node, BoundEventAccess left, BoundExpression right, BinaryOperatorKind opKind, DiagnosticBag diagnostics)
	{
		bool hasErrors = false;
		EventSymbol eventSymbol = left.EventSymbol;
		BoundExpression receiverOpt = left.ReceiverOpt;
		TypeSymbol type = left.Type;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(right, type, ref useSiteDiagnostics);
		if (!conversion.IsImplicit || !conversion.IsValid)
		{
			hasErrors = true;
			if (type.IsDelegateType())
			{
				GenerateImplicitConversionError(diagnostics, node, conversion, right, type);
			}
		}
		BoundExpression argument = CreateConversion(right, conversion, type, diagnostics);
		bool flag = opKind == BinaryOperatorKind.Addition;
		MethodSymbol methodSymbol = (flag ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
		TypeSymbol type2;
		if ((object)methodSymbol == null)
		{
			type2 = GetSpecialType(SpecialType.System_Void, diagnostics, node);
			if (!eventSymbol.OriginalDefinition.IsFromCompilation(Compilation))
			{
				Error(diagnostics, ErrorCode.ERR_MissingPredefinedMember, node, type, SourceEventSymbol.GetAccessorName(eventSymbol.Name, flag));
			}
		}
		else if (eventSymbol.IsWindowsRuntimeEvent)
		{
			type2 = GetSpecialType(SpecialType.System_Void, diagnostics, node);
		}
		else
		{
			type2 = methodSymbol.ReturnType;
			if (!IsAccessible(methodSymbol, ref useSiteDiagnostics, GetAccessThroughType(receiverOpt)))
			{
				Error(diagnostics, ErrorCode.ERR_BadAccess, node, methodSymbol);
				hasErrors = true;
			}
		}
		diagnostics.Add(node, useSiteDiagnostics);
		return new BoundEventAssignmentOperator(node, eventSymbol, flag, right.HasDynamicType(), receiverOpt, argument, type2, hasErrors);
	}

	private static bool IsLegalDynamicOperand(BoundExpression operand)
	{
		TypeSymbol type = operand.Type;
		if ((object)type == null)
		{
			return operand.IsLiteralNull();
		}
		if (!type.IsPointerType() && !type.IsRestrictedType())
		{
			return type.SpecialType != SpecialType.System_Void;
		}
		return false;
	}

	private BoundExpression BindDynamicBinaryOperator(BinaryExpressionSyntax node, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		bool flag = false;
		bool flag2 = IsLegalDynamicOperand(left);
		bool flag3 = IsLegalDynamicOperand(right);
		if (!flag2 || !flag3)
		{
			Error(diagnostics, ErrorCode.ERR_BadBinaryOps, node, node.OperatorToken.Text, left.Display, right.Display);
			flag = true;
		}
		MethodSymbol userDefinedOperator = null;
		if (kind.IsLogical() & flag2)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if (!IsValidDynamicCondition(left, kind == BinaryOperatorKind.LogicalAnd, ref useSiteDiagnostics, out userDefinedOperator))
			{
				Error(diagnostics, ErrorCode.ERR_InvalidDynamicCondition, node.Left, left.Type, (kind == BinaryOperatorKind.LogicalAnd) ? "false" : "true");
				flag = true;
			}
			diagnostics.Add(node, useSiteDiagnostics);
		}
		return new BoundBinaryOperator(node, (flag ? kind : kind.WithType(BinaryOperatorKind.Dynamic)).WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), left, right, null, userDefinedOperator, LookupResultKind.Viable, Compilation.DynamicType, flag);
	}

	protected static bool IsSimpleBinaryOperator(SyntaxKind kind)
	{
		if (kind - 8668 <= (SyntaxKind)6 || kind - 8677 <= (SyntaxKind)8)
		{
			return true;
		}
		return false;
	}

	private BoundExpression BindSimpleBinaryOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		ArrayBuilder<BinaryExpressionSyntax> instance = ArrayBuilder<BinaryExpressionSyntax>.GetInstance();
		ExpressionSyntax expressionSyntax = node;
		while (IsSimpleBinaryOperator(expressionSyntax.Kind()))
		{
			BinaryExpressionSyntax binaryExpressionSyntax = (BinaryExpressionSyntax)expressionSyntax;
			instance.Push(binaryExpressionSyntax);
			expressionSyntax = binaryExpressionSyntax.Left;
		}
		int compoundStringLength = 0;
		BoundExpression boundExpression = BindExpression(expressionSyntax, diagnostics);
		if (node.IsKind(SyntaxKind.SubtractExpression) && expressionSyntax.IsKind(SyntaxKind.ParenthesizedExpression))
		{
			if (boundExpression.Kind == BoundKind.TypeExpression && !((ParenthesizedExpressionSyntax)expressionSyntax).Expression.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				Error(diagnostics, ErrorCode.ERR_PossibleBadNegCast, node);
			}
			else if (boundExpression.Kind == BoundKind.BadExpression)
			{
				ParenthesizedExpressionSyntax parenthesizedExpressionSyntax = (ParenthesizedExpressionSyntax)expressionSyntax;
				if (parenthesizedExpressionSyntax.Expression.IsKind(SyntaxKind.IdentifierName) && ((IdentifierNameSyntax)parenthesizedExpressionSyntax.Expression).Identifier.ValueText == "dynamic")
				{
					Error(diagnostics, ErrorCode.ERR_PossibleBadNegCast, node);
				}
			}
		}
		while (instance.Count > 0)
		{
			BinaryExpressionSyntax binaryExpressionSyntax2 = instance.Pop();
			BindValueKind binaryAssignmentKind = GetBinaryAssignmentKind(binaryExpressionSyntax2.Kind());
			BoundExpression left = CheckValue(boundExpression, binaryAssignmentKind, diagnostics);
			BoundExpression right = BindValue(binaryExpressionSyntax2.Right, diagnostics, BindValueKind.RValue);
			boundExpression = BindSimpleBinaryOperator(binaryExpressionSyntax2, diagnostics, left, right, ref compoundStringLength);
		}
		instance.Free();
		return boundExpression;
	}

	private BoundExpression BindSimpleBinaryOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics, BoundExpression left, BoundExpression right, ref int compoundStringLength)
	{
		BinaryOperatorKind binaryOperatorKind = SyntaxKindToBinaryOperatorKind(node.Kind());
		if (left.HasAnyErrors || right.HasAnyErrors)
		{
			return new BoundBinaryOperator(node, binaryOperatorKind, left, right, null, null, LookupResultKind.Empty, GetBinaryOperatorErrorType(binaryOperatorKind, diagnostics, node), hasErrors: true);
		}
		TypeSymbol type = left.Type;
		TypeSymbol type2 = right.Type;
		if (((object)type != null && type.IsDynamic()) || ((object)type2 != null && type2.IsDynamic()))
		{
			return BindDynamicBinaryOperator(node, binaryOperatorKind, left, right, diagnostics);
		}
		bool flag = left.IsLiteralNull();
		bool flag2 = right.IsLiteralNull();
		if ((binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual) & flag & flag2)
		{
			return new BoundLiteral(node, ConstantValue.Create(binaryOperatorKind == BinaryOperatorKind.Equal), GetSpecialType(SpecialType.System_Boolean, diagnostics, node));
		}
		if (IsTupleBinaryOperation(left, right) && (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual))
		{
			CheckFeatureAvailability(node, MessageID.IDS_FeatureTupleEquality, diagnostics);
			return BindTupleBinaryOperator(node, binaryOperatorKind, left, right, diagnostics);
		}
		bool flag3 = BindSimpleBinaryOperatorParts(node, diagnostics, left, right, binaryOperatorKind, out var resultKind, out var originalUserDefinedOperators, out var resultSignature, out var best);
		BinaryOperatorKind binaryOperatorKind2 = resultSignature.Kind;
		bool flag4 = false;
		if (!flag3)
		{
			ReportBinaryOperatorError(node, diagnostics, node.OperatorToken, left, right, resultKind);
			binaryOperatorKind2 &= ~BinaryOperatorKind.TypeMask;
			flag4 = true;
		}
		SyntaxKind syntaxKind = node.Kind();
		if (syntaxKind - 8680 > (SyntaxKind)5 && (type.IsVoidPointer() || type2.IsVoidPointer()))
		{
			Error(diagnostics, ErrorCode.ERR_VoidError, node);
			flag4 = true;
		}
		TypeSymbol returnType = resultSignature.ReturnType;
		BoundExpression left2 = left;
		BoundExpression right2 = right;
		ConstantValue constantValue = null;
		if (flag3 && binaryOperatorKind2.OperandTypes() != BinaryOperatorKind.NullableNull)
		{
			left2 = CreateConversion(left, best.LeftConversion, resultSignature.LeftType, diagnostics);
			right2 = CreateConversion(right, best.RightConversion, resultSignature.RightType, diagnostics);
			constantValue = FoldBinaryOperator(node, binaryOperatorKind2, left2, right2, returnType.SpecialType, diagnostics, ref compoundStringLength);
		}
		flag4 = flag4 || (constantValue != null && constantValue.IsBad);
		return new BoundBinaryOperator(node, binaryOperatorKind2.WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), left2, right2, constantValue, resultSignature.Method, resultKind, originalUserDefinedOperators, returnType, flag4);
	}

	private bool BindSimpleBinaryOperatorParts(BinaryExpressionSyntax node, DiagnosticBag diagnostics, BoundExpression left, BoundExpression right, BinaryOperatorKind kind, out LookupResultKind resultKind, out ImmutableArray<MethodSymbol> originalUserDefinedOperators, out BinaryOperatorSignature resultSignature, out BinaryOperatorAnalysisResult best)
	{
		best = BinaryOperatorOverloadResolution(kind, left, right, node, diagnostics, out resultKind, out originalUserDefinedOperators);
		bool result;
		if (!best.HasValue)
		{
			resultSignature = new BinaryOperatorSignature(kind, null, null, CreateErrorType());
			result = false;
		}
		else
		{
			BinaryOperatorSignature signature = best.Signature;
			bool flag = signature.Kind == BinaryOperatorKind.ObjectEqual || signature.Kind == BinaryOperatorKind.ObjectNotEqual;
			bool flag2 = left.IsLiteralNull();
			bool flag3 = right.IsLiteralNull();
			TypeSymbol type = left.Type;
			TypeSymbol type2 = right.Type;
			if ((object)signature.Method == null && (signature.Kind.Operator() == BinaryOperatorKind.Equal || signature.Kind.Operator() == BinaryOperatorKind.NotEqual) && ((flag2 && (object)type2 != null && type2.IsNullableType()) || (flag3 && (object)type != null && type.IsNullableType())))
			{
				resultSignature = new BinaryOperatorSignature(kind | BinaryOperatorKind.NullableNull, null, null, GetSpecialType(SpecialType.System_Boolean, diagnostics, node));
				result = true;
			}
			else
			{
				resultSignature = signature;
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				bool flag4 = left.IsLiteralDefault();
				bool flag5 = right.IsLiteralDefault();
				result = !flag || BuiltInOperators.IsValidObjectEquality(Conversions, type, flag2 | flag4, type2, flag3 | flag5, ref useSiteDiagnostics);
				diagnostics.Add(node, useSiteDiagnostics);
			}
		}
		return result;
	}

	private static void ReportUnaryOperatorError(CSharpSyntaxNode node, DiagnosticBag diagnostics, string operatorName, BoundExpression operand, LookupResultKind resultKind)
	{
		ErrorCode code = ((resultKind == LookupResultKind.Ambiguous) ? ErrorCode.ERR_AmbigUnaryOp : ErrorCode.ERR_BadUnaryOp);
		Error(diagnostics, code, node, operatorName, operand.Display);
	}

	private void ReportAssignmentOperatorError(AssignmentExpressionSyntax node, DiagnosticBag diagnostics, BoundExpression left, BoundExpression right, LookupResultKind resultKind)
	{
		if (((ushort)node.OperatorToken.RawKind == 8280 || (ushort)node.OperatorToken.RawKind == 8281) && (object)left.Type != null && left.Type.TypeKind == TypeKind.Delegate)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion conversion = Conversions.ClassifyConversionFromExpression(right, left.Type, ref useSiteDiagnostics);
			GenerateImplicitConversionError(diagnostics, right.Syntax, conversion, right, left.Type);
		}
		else
		{
			ReportBinaryOperatorError(node, diagnostics, node.OperatorToken, left, right, resultKind);
		}
	}

	private static void ReportBinaryOperatorError(ExpressionSyntax node, DiagnosticBag diagnostics, SyntaxToken operatorToken, BoundExpression left, BoundExpression right, LookupResultKind resultKind)
	{
		bool flag = left.IsLiteralDefault();
		bool flag2 = right.IsLiteralDefault();
		if (operatorToken.Kind() == SyntaxKind.EqualsEqualsToken || operatorToken.Kind() == SyntaxKind.ExclamationEqualsToken)
		{
			if (flag & flag2)
			{
				Error(diagnostics, ErrorCode.ERR_AmbigBinaryOpsOnDefault, node, operatorToken.Text);
				return;
			}
		}
		else if (flag | flag2)
		{
			Error(diagnostics, ErrorCode.ERR_BadOpOnNullOrDefault, node, operatorToken.Text, "default");
			return;
		}
		ErrorCode code = ((resultKind == LookupResultKind.Ambiguous) ? ErrorCode.ERR_AmbigBinaryOps : ErrorCode.ERR_BadBinaryOps);
		Error(diagnostics, code, node, operatorToken.Text, left.Display, right.Display);
	}

	private BoundExpression BindConditionalLogicalOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BinaryExpressionSyntax binaryExpressionSyntax = node;
		ExpressionSyntax expressionSyntax;
		while (true)
		{
			expressionSyntax = binaryExpressionSyntax.Left;
			if (!(expressionSyntax is BinaryExpressionSyntax binaryExpressionSyntax2) || (binaryExpressionSyntax2.Kind() != SyntaxKind.LogicalOrExpression && binaryExpressionSyntax2.Kind() != SyntaxKind.LogicalAndExpression))
			{
				break;
			}
			binaryExpressionSyntax = binaryExpressionSyntax2;
		}
		BoundExpression boundExpression = BindValue(expressionSyntax, diagnostics, BindValueKind.RValue);
		do
		{
			binaryExpressionSyntax = (BinaryExpressionSyntax)expressionSyntax.Parent;
			BoundExpression right = BindValue(binaryExpressionSyntax.Right, diagnostics, BindValueKind.RValue);
			boundExpression = BindConditionalLogicalOperator(binaryExpressionSyntax, boundExpression, right, diagnostics);
			expressionSyntax = binaryExpressionSyntax;
		}
		while (expressionSyntax != node);
		return boundExpression;
	}

	private BoundExpression BindConditionalLogicalOperator(BinaryExpressionSyntax node, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		BinaryOperatorKind binaryOperatorKind = SyntaxKindToBinaryOperatorKind(node.Kind());
		if (left.HasAnyErrors || right.HasAnyErrors)
		{
			return new BoundBinaryOperator(node, binaryOperatorKind, left, right, null, null, LookupResultKind.Empty, GetBinaryOperatorErrorType(binaryOperatorKind, diagnostics, node), hasErrors: true);
		}
		if ((object)left.Type != null && left.Type.SpecialType == SpecialType.System_Boolean && (object)right.Type != null && right.Type.SpecialType == SpecialType.System_Boolean)
		{
			ConstantValue constantValue = FoldBinaryOperator(node, binaryOperatorKind | BinaryOperatorKind.Bool, left, right, SpecialType.System_Boolean, diagnostics);
			return new BoundBinaryOperator(node, binaryOperatorKind | BinaryOperatorKind.Bool, left, right, constantValue, null, LookupResultKind.Viable, left.Type, constantValue != null && constantValue.IsBad);
		}
		if (left.HasDynamicType() || right.HasDynamicType())
		{
			return BindDynamicBinaryOperator(node, binaryOperatorKind, left, right, diagnostics);
		}
		BinaryOperatorAnalysisResult binaryOperatorAnalysisResult = BinaryOperatorOverloadResolution(binaryOperatorKind, left, right, node, diagnostics, out var resultKind, out var originalUserDefinedOperators);
		if (!binaryOperatorAnalysisResult.HasValue)
		{
			ReportBinaryOperatorError(node, diagnostics, node.OperatorToken, left, right, resultKind);
		}
		else
		{
			BinaryOperatorSignature signature = binaryOperatorAnalysisResult.Signature;
			bool flag = signature.LeftType.SpecialType == SpecialType.System_Boolean && signature.RightType.SpecialType == SpecialType.System_Boolean;
			MethodSymbol trueOperator = null;
			MethodSymbol falseOperator = null;
			if (!flag && !signature.Kind.IsUserDefined())
			{
				ReportBinaryOperatorError(node, diagnostics, node.OperatorToken, left, right, resultKind);
			}
			else if (flag || IsValidUserDefinedConditionalLogicalOperator(node, signature, diagnostics, out trueOperator, out falseOperator))
			{
				BoundExpression left2 = CreateConversion(left, binaryOperatorAnalysisResult.LeftConversion, signature.LeftType, diagnostics);
				BoundExpression right2 = CreateConversion(right, binaryOperatorAnalysisResult.RightConversion, signature.RightType, diagnostics);
				BinaryOperatorKind binaryOperatorKind2 = binaryOperatorKind | signature.Kind.OperandTypes();
				if (signature.Kind.IsLifted())
				{
					binaryOperatorKind2 |= BinaryOperatorKind.Lifted;
				}
				if (binaryOperatorKind2.IsUserDefined())
				{
					return new BoundUserDefinedConditionalLogicalOperator(node, binaryOperatorKind2, left2, right2, signature.Method, trueOperator, falseOperator, resultKind, originalUserDefinedOperators, signature.ReturnType);
				}
				return new BoundBinaryOperator(node, binaryOperatorKind2, left2, right2, null, signature.Method, resultKind, originalUserDefinedOperators, signature.ReturnType);
			}
		}
		return new BoundBinaryOperator(node, binaryOperatorKind, left, right, null, null, resultKind, originalUserDefinedOperators, CreateErrorType(), hasErrors: true);
	}

	private bool IsValidDynamicCondition(BoundExpression left, bool isNegative, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out MethodSymbol userDefinedOperator)
	{
		userDefinedOperator = null;
		TypeSymbol type = left.Type;
		if ((object)type == null)
		{
			return false;
		}
		if (type.IsDynamic())
		{
			return true;
		}
		if (Conversions.ClassifyImplicitConversionFromExpression(left, Compilation.GetSpecialType(SpecialType.System_Boolean), ref useSiteDiagnostics).Exists)
		{
			return true;
		}
		if (type.Kind != SymbolKind.NamedType)
		{
			return false;
		}
		NamedTypeSymbol containingType = type as NamedTypeSymbol;
		return HasApplicableBooleanOperator(containingType, isNegative ? "op_False" : "op_True", type, ref useSiteDiagnostics, out userDefinedOperator);
	}

	private bool IsValidUserDefinedConditionalLogicalOperator(CSharpSyntaxNode syntax, BinaryOperatorSignature signature, DiagnosticBag diagnostics, out MethodSymbol trueOperator, out MethodSymbol falseOperator)
	{
		NamedTypeSymbol containingType = signature.Method.ContainingType;
		bool num = signature.LeftType == signature.RightType && signature.LeftType == signature.ReturnType;
		bool flag = signature.ReturnType == containingType || (signature.ReturnType.IsNullableType() && signature.ReturnType.GetNullableUnderlyingType() == containingType);
		if (!num || !flag)
		{
			Error(diagnostics, ErrorCode.ERR_BadBoolOp, syntax, signature.Method);
			trueOperator = null;
			falseOperator = null;
			return false;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!HasApplicableBooleanOperator(containingType, "op_True", signature.LeftType, ref useSiteDiagnostics, out trueOperator) || !HasApplicableBooleanOperator(containingType, "op_False", signature.LeftType, ref useSiteDiagnostics, out falseOperator))
		{
			Error(diagnostics, ErrorCode.ERR_MustHaveOpTF, syntax, signature.Method, containingType);
			diagnostics.Add(syntax, useSiteDiagnostics);
			trueOperator = null;
			falseOperator = null;
			return false;
		}
		diagnostics.Add(syntax, useSiteDiagnostics);
		return true;
	}

	private bool HasApplicableBooleanOperator(NamedTypeSymbol containingType, string name, TypeSymbol argumentType, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out MethodSymbol @operator)
	{
		NamedTypeSymbol namedTypeSymbol = containingType;
		while (namedTypeSymbol != null)
		{
			ImmutableArray<MethodSymbol> operators = namedTypeSymbol.GetOperators(name);
			for (int i = 0; i < operators.Length; i++)
			{
				MethodSymbol methodSymbol = operators[i];
				if (methodSymbol.ParameterCount == 1 && methodSymbol.DeclaredAccessibility == Accessibility.Public && Conversions.ClassifyConversionFromType(argumentType, methodSymbol.ParameterTypes[0], ref useSiteDiagnostics).IsImplicit)
				{
					@operator = methodSymbol;
					return true;
				}
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		@operator = null;
		return false;
	}

	private TypeSymbol GetBinaryOperatorErrorType(BinaryOperatorKind kind, DiagnosticBag diagnostics, CSharpSyntaxNode node)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			return GetSpecialType(SpecialType.System_Boolean, diagnostics, node);
		default:
			return CreateErrorType();
		}
	}

	private BinaryOperatorAnalysisResult BinaryOperatorOverloadResolution(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, CSharpSyntaxNode node, DiagnosticBag diagnostics, out LookupResultKind resultKind, out ImmutableArray<MethodSymbol> originalUserDefinedOperators)
	{
		if (!IsDefaultLiteralAllowedInBinaryOperator(kind, left, right))
		{
			resultKind = LookupResultKind.OverloadResolutionFailure;
			originalUserDefinedOperators = default;
			return default;
		}
		BinaryOperatorOverloadResolutionResult instance = BinaryOperatorOverloadResolutionResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		OverloadResolution.BinaryOperatorOverloadResolution(kind, left, right, instance, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		BinaryOperatorAnalysisResult best = instance.Best;
		if (instance.Results.Any())
		{
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (BinaryOperatorAnalysisResult result in instance.Results)
			{
				MethodSymbol method = result.Signature.Method;
				if ((object)method != null)
				{
					instance2.Add(method);
				}
			}
			originalUserDefinedOperators = instance2.ToImmutableAndFree();
			if (best.HasValue)
			{
				resultKind = LookupResultKind.Viable;
			}
			else if (instance.AnyValid())
			{
				resultKind = LookupResultKind.Ambiguous;
			}
			else
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		else
		{
			originalUserDefinedOperators = ImmutableArray<MethodSymbol>.Empty;
			resultKind = (best.HasValue ? LookupResultKind.Viable : LookupResultKind.Empty);
		}
		if (best.HasValue && (object)best.Signature.Method != null)
		{
			Symbol method2 = best.Signature.Method;
			ReportDiagnosticsIfObsolete(diagnostics, method2, node, hasBaseReceiver: false);
		}
		instance.Free();
		return best;
	}

	private bool IsDefaultLiteralAllowedInBinaryOperator(BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		if (kind == BinaryOperatorKind.Equal || kind == BinaryOperatorKind.NotEqual)
		{
			if (left.IsLiteralDefault())
			{
				return !right.IsLiteralDefault();
			}
			return true;
		}
		if (!left.IsLiteralDefault())
		{
			return !right.IsLiteralDefault();
		}
		return false;
	}

	private UnaryOperatorAnalysisResult UnaryOperatorOverloadResolution(UnaryOperatorKind kind, BoundExpression operand, CSharpSyntaxNode node, DiagnosticBag diagnostics, out LookupResultKind resultKind, out ImmutableArray<MethodSymbol> originalUserDefinedOperators)
	{
		UnaryOperatorOverloadResolutionResult instance = UnaryOperatorOverloadResolutionResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		OverloadResolution.UnaryOperatorOverloadResolution(kind, operand, instance, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		UnaryOperatorAnalysisResult best = instance.Best;
		if (instance.Results.Any())
		{
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (UnaryOperatorAnalysisResult result in instance.Results)
			{
				MethodSymbol method = result.Signature.Method;
				if ((object)method != null)
				{
					instance2.Add(method);
				}
			}
			originalUserDefinedOperators = instance2.ToImmutableAndFree();
			if (best.HasValue)
			{
				resultKind = LookupResultKind.Viable;
			}
			else if (instance.AnyValid())
			{
				if (kind == UnaryOperatorKind.UnaryMinus && (object)operand.Type != null && operand.Type.SpecialType == SpecialType.System_UInt64)
				{
					resultKind = LookupResultKind.OverloadResolutionFailure;
				}
				else
				{
					resultKind = LookupResultKind.Ambiguous;
				}
			}
			else
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		else
		{
			originalUserDefinedOperators = ImmutableArray<MethodSymbol>.Empty;
			resultKind = (best.HasValue ? LookupResultKind.Viable : LookupResultKind.Empty);
		}
		if (best.HasValue && (object)best.Signature.Method != null)
		{
			Symbol method2 = best.Signature.Method;
			ReportDiagnosticsIfObsolete(diagnostics, method2, node, hasBaseReceiver: false);
		}
		instance.Free();
		return best;
	}

	private static object FoldDecimalBinaryOperators(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		return kind switch
		{
			BinaryOperatorKind.DecimalAddition => (object)(valueLeft.DecimalValue + valueRight.DecimalValue), 
			BinaryOperatorKind.DecimalSubtraction => valueLeft.DecimalValue - valueRight.DecimalValue, 
			BinaryOperatorKind.DecimalMultiplication => valueLeft.DecimalValue * valueRight.DecimalValue, 
			BinaryOperatorKind.DecimalDivision => valueLeft.DecimalValue / valueRight.DecimalValue, 
			BinaryOperatorKind.DecimalRemainder => valueLeft.DecimalValue % valueRight.DecimalValue, 
			_ => null, 
		};
	}

	private static object FoldUncheckedIntegralBinaryOperator(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		switch (kind)
		{
		case BinaryOperatorKind.IntAddition:
			return valueLeft.Int32Value + valueRight.Int32Value;
		case BinaryOperatorKind.LongAddition:
			return valueLeft.Int64Value + valueRight.Int64Value;
		case BinaryOperatorKind.UIntAddition:
			return valueLeft.UInt32Value + valueRight.UInt32Value;
		case BinaryOperatorKind.ULongAddition:
			return valueLeft.UInt64Value + valueRight.UInt64Value;
		case BinaryOperatorKind.IntSubtraction:
			return valueLeft.Int32Value - valueRight.Int32Value;
		case BinaryOperatorKind.LongSubtraction:
			return valueLeft.Int64Value - valueRight.Int64Value;
		case BinaryOperatorKind.UIntSubtraction:
			return valueLeft.UInt32Value - valueRight.UInt32Value;
		case BinaryOperatorKind.ULongSubtraction:
			return valueLeft.UInt64Value - valueRight.UInt64Value;
		case BinaryOperatorKind.IntMultiplication:
			return valueLeft.Int32Value * valueRight.Int32Value;
		case BinaryOperatorKind.LongMultiplication:
			return valueLeft.Int64Value * valueRight.Int64Value;
		case BinaryOperatorKind.UIntMultiplication:
			return valueLeft.UInt32Value * valueRight.UInt32Value;
		case BinaryOperatorKind.ULongMultiplication:
			return valueLeft.UInt64Value * valueRight.UInt64Value;
		case BinaryOperatorKind.IntDivision:
			if (valueLeft.Int32Value == int.MinValue && valueRight.Int32Value == -1)
			{
				return int.MinValue;
			}
			return valueLeft.Int32Value / valueRight.Int32Value;
		case BinaryOperatorKind.LongDivision:
			if (valueLeft.Int64Value == long.MinValue && valueRight.Int64Value == -1)
			{
				return long.MinValue;
			}
			return valueLeft.Int64Value / valueRight.Int64Value;
		default:
			return null;
		}
	}

	private static object FoldCheckedIntegralBinaryOperator(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		checked
		{
			return kind switch
			{
				BinaryOperatorKind.IntAddition => (object)(valueLeft.Int32Value + valueRight.Int32Value), 
				BinaryOperatorKind.LongAddition => valueLeft.Int64Value + valueRight.Int64Value, 
				BinaryOperatorKind.UIntAddition => valueLeft.UInt32Value + valueRight.UInt32Value, 
				BinaryOperatorKind.ULongAddition => valueLeft.UInt64Value + valueRight.UInt64Value, 
				BinaryOperatorKind.IntSubtraction => valueLeft.Int32Value - valueRight.Int32Value, 
				BinaryOperatorKind.LongSubtraction => valueLeft.Int64Value - valueRight.Int64Value, 
				BinaryOperatorKind.UIntSubtraction => valueLeft.UInt32Value - valueRight.UInt32Value, 
				BinaryOperatorKind.ULongSubtraction => valueLeft.UInt64Value - valueRight.UInt64Value, 
				BinaryOperatorKind.IntMultiplication => valueLeft.Int32Value * valueRight.Int32Value, 
				BinaryOperatorKind.LongMultiplication => valueLeft.Int64Value * valueRight.Int64Value, 
				BinaryOperatorKind.UIntMultiplication => valueLeft.UInt32Value * valueRight.UInt32Value, 
				BinaryOperatorKind.ULongMultiplication => valueLeft.UInt64Value * valueRight.UInt64Value, 
				BinaryOperatorKind.IntDivision => unchecked(valueLeft.Int32Value / valueRight.Int32Value), 
				BinaryOperatorKind.LongDivision => unchecked(valueLeft.Int64Value / valueRight.Int64Value), 
				_ => null, 
			};
		}
	}

	internal static TypeSymbol GetEnumType(BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		switch (kind)
		{
		case BinaryOperatorKind.EnumAndUnderlyingAddition:
		case BinaryOperatorKind.EnumSubtraction:
		case BinaryOperatorKind.EnumAndUnderlyingSubtraction:
		case BinaryOperatorKind.EnumEqual:
		case BinaryOperatorKind.EnumNotEqual:
		case BinaryOperatorKind.EnumGreaterThan:
		case BinaryOperatorKind.EnumLessThan:
		case BinaryOperatorKind.EnumGreaterThanOrEqual:
		case BinaryOperatorKind.EnumLessThanOrEqual:
		case BinaryOperatorKind.EnumAnd:
		case BinaryOperatorKind.EnumXor:
		case BinaryOperatorKind.EnumOr:
			return left.Type;
		case BinaryOperatorKind.UnderlyingAndEnumAddition:
		case BinaryOperatorKind.UnderlyingAndEnumSubtraction:
			return right.Type;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal static SpecialType GetEnumPromotedType(SpecialType underlyingType)
	{
		switch (underlyingType)
		{
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
			return SpecialType.System_Int32;
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
			return underlyingType;
		default:
			throw ExceptionUtilities.UnexpectedValue(underlyingType);
		}
	}

	private ConstantValue FoldEnumBinaryOperator(CSharpSyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		TypeSymbol enumType = GetEnumType(kind, left, right);
		TypeSymbol enumUnderlyingType = enumType.GetEnumUnderlyingType();
		BoundExpression source = CreateConversion(left, enumUnderlyingType, diagnostics);
		BoundExpression source2 = CreateConversion(right, enumUnderlyingType, diagnostics);
		SpecialType enumPromotedType = GetEnumPromotedType(enumUnderlyingType.SpecialType);
		TypeSymbol typeSymbol = ((enumPromotedType == enumUnderlyingType.SpecialType) ? enumUnderlyingType : GetSpecialType(enumPromotedType, diagnostics, syntax));
		source = CreateConversion(source, typeSymbol, diagnostics);
		source2 = CreateConversion(source2, typeSymbol, diagnostics);
		BinaryOperatorKind kind2 = kind.Operator().WithType(source.Type.SpecialType);
		SpecialType specialType;
		switch (kind2.Operator())
		{
		case BinaryOperatorKind.Addition:
		case BinaryOperatorKind.Subtraction:
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
			specialType = typeSymbol.SpecialType;
			break;
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			specialType = SpecialType.System_Boolean;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind2.Operator());
		}
		ConstantValue constantValue = FoldBinaryOperator(syntax, kind2, source, source2, specialType, diagnostics);
		if (specialType != SpecialType.System_Boolean && constantValue != null && !constantValue.IsBad)
		{
			TypeSymbol destination = ((kind == BinaryOperatorKind.EnumSubtraction) ? enumUnderlyingType : enumType);
			return FoldConstantNumericConversion(syntax, constantValue, destination, diagnostics);
		}
		return constantValue;
	}

	private ConstantValue FoldBinaryOperator(CSharpSyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, SpecialType resultType, DiagnosticBag diagnostics)
	{
		int compoundStringLength = 0;
		return FoldBinaryOperator(syntax, kind, left, right, resultType, diagnostics, ref compoundStringLength);
	}

	private ConstantValue FoldBinaryOperator(CSharpSyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, SpecialType resultType, DiagnosticBag diagnostics, ref int compoundStringLength)
	{
		if (left.HasAnyErrors || right.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = TryFoldingNullableEquality(kind, left, right);
		if (constantValue != null)
		{
			return constantValue;
		}
		ConstantValue constantValue2 = left.ConstantValue;
		ConstantValue constantValue3 = right.ConstantValue;
		if (constantValue2 == null || constantValue3 == null)
		{
			return null;
		}
		if (constantValue2.IsBad || constantValue3.IsBad)
		{
			return ConstantValue.Bad;
		}
		if (kind.IsEnum() && !kind.IsLifted())
		{
			return FoldEnumBinaryOperator(syntax, kind, left, right, diagnostics);
		}
		if (IsDivisionByZero(kind, constantValue3))
		{
			Error(diagnostics, ErrorCode.ERR_IntDivByZero, syntax);
			return ConstantValue.Bad;
		}
		object obj = FoldNeverOverflowBinaryOperators(kind, constantValue2, constantValue3);
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		ConstantValue constantValue4 = FoldStringConcatenation(kind, constantValue2, constantValue3, ref compoundStringLength);
		if (constantValue4 != null)
		{
			if (constantValue4.IsBad)
			{
				Error(diagnostics, ErrorCode.ERR_ConstantStringTooLong, syntax);
			}
			return constantValue4;
		}
		try
		{
			obj = FoldDecimalBinaryOperators(kind, constantValue2, constantValue3);
		}
		catch (OverflowException)
		{
			Error(diagnostics, ErrorCode.ERR_DecConstError, syntax);
			return ConstantValue.Bad;
		}
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		if (CheckOverflowAtCompileTime)
		{
			try
			{
				obj = FoldCheckedIntegralBinaryOperator(kind, constantValue2, constantValue3);
			}
			catch (OverflowException)
			{
				Error(diagnostics, ErrorCode.ERR_CheckedOverflow, syntax);
				return ConstantValue.Bad;
			}
		}
		else
		{
			obj = FoldUncheckedIntegralBinaryOperator(kind, constantValue2, constantValue3);
		}
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		return null;
	}

	private static ConstantValue TryFoldingNullableEquality(BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		if (kind.IsLifted())
		{
			BinaryOperatorKind binaryOperatorKind = kind.Operator();
			if ((binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual) && left.Kind == BoundKind.Conversion && right.Kind == BoundKind.Conversion)
			{
				BoundConversion boundConversion = (BoundConversion)left;
				BoundConversion boundConversion2 = (BoundConversion)right;
				ConstantValue constantValue = boundConversion.Operand.ConstantValue;
				ConstantValue constantValue2 = boundConversion2.Operand.ConstantValue;
				if (constantValue != null && constantValue2 != null)
				{
					bool isNull = constantValue.IsNull;
					bool isNull2 = constantValue2.IsNull;
					if (isNull | isNull2)
					{
						if (isNull == isNull2 != (binaryOperatorKind == BinaryOperatorKind.Equal))
						{
							return ConstantValue.False;
						}
						return ConstantValue.True;
					}
				}
			}
		}
		return null;
	}

	private static object FoldNeverOverflowBinaryOperators(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		switch (kind)
		{
		case BinaryOperatorKind.ObjectEqual:
			if (valueLeft.IsNull)
			{
				return valueRight.IsNull;
			}
			if (valueRight.IsNull)
			{
				return false;
			}
			break;
		case BinaryOperatorKind.ObjectNotEqual:
			if (valueLeft.IsNull)
			{
				return !valueRight.IsNull;
			}
			if (valueRight.IsNull)
			{
				return true;
			}
			break;
		case BinaryOperatorKind.DoubleAddition:
			return valueLeft.DoubleValue + valueRight.DoubleValue;
		case BinaryOperatorKind.FloatAddition:
			return valueLeft.SingleValue + valueRight.SingleValue;
		case BinaryOperatorKind.DoubleSubtraction:
			return valueLeft.DoubleValue - valueRight.DoubleValue;
		case BinaryOperatorKind.FloatSubtraction:
			return valueLeft.SingleValue - valueRight.SingleValue;
		case BinaryOperatorKind.DoubleMultiplication:
			return valueLeft.DoubleValue * valueRight.DoubleValue;
		case BinaryOperatorKind.FloatMultiplication:
			return valueLeft.SingleValue * valueRight.SingleValue;
		case BinaryOperatorKind.DoubleDivision:
			return valueLeft.DoubleValue / valueRight.DoubleValue;
		case BinaryOperatorKind.FloatDivision:
			return valueLeft.SingleValue / valueRight.SingleValue;
		case BinaryOperatorKind.DoubleRemainder:
			return valueLeft.DoubleValue % valueRight.DoubleValue;
		case BinaryOperatorKind.FloatRemainder:
			return valueLeft.SingleValue % valueRight.SingleValue;
		case BinaryOperatorKind.IntLeftShift:
			return valueLeft.Int32Value << valueRight.Int32Value;
		case BinaryOperatorKind.LongLeftShift:
			return valueLeft.Int64Value << valueRight.Int32Value;
		case BinaryOperatorKind.UIntLeftShift:
			return valueLeft.UInt32Value << valueRight.Int32Value;
		case BinaryOperatorKind.ULongLeftShift:
			return valueLeft.UInt64Value << valueRight.Int32Value;
		case BinaryOperatorKind.IntRightShift:
			return valueLeft.Int32Value >> valueRight.Int32Value;
		case BinaryOperatorKind.LongRightShift:
			return valueLeft.Int64Value >> valueRight.Int32Value;
		case BinaryOperatorKind.UIntRightShift:
			return valueLeft.UInt32Value >> valueRight.Int32Value;
		case BinaryOperatorKind.ULongRightShift:
			return valueLeft.UInt64Value >> valueRight.Int32Value;
		case BinaryOperatorKind.BoolAnd:
			return valueLeft.BooleanValue & valueRight.BooleanValue;
		case BinaryOperatorKind.IntAnd:
			return valueLeft.Int32Value & valueRight.Int32Value;
		case BinaryOperatorKind.LongAnd:
			return valueLeft.Int64Value & valueRight.Int64Value;
		case BinaryOperatorKind.UIntAnd:
			return valueLeft.UInt32Value & valueRight.UInt32Value;
		case BinaryOperatorKind.ULongAnd:
			return valueLeft.UInt64Value & valueRight.UInt64Value;
		case BinaryOperatorKind.BoolOr:
			return valueLeft.BooleanValue | valueRight.BooleanValue;
		case BinaryOperatorKind.IntOr:
			return valueLeft.Int32Value | valueRight.Int32Value;
		case BinaryOperatorKind.LongOr:
			return valueLeft.Int64Value | valueRight.Int64Value;
		case BinaryOperatorKind.UIntOr:
			return valueLeft.UInt32Value | valueRight.UInt32Value;
		case BinaryOperatorKind.ULongOr:
			return valueLeft.UInt64Value | valueRight.UInt64Value;
		case BinaryOperatorKind.BoolXor:
			return valueLeft.BooleanValue ^ valueRight.BooleanValue;
		case BinaryOperatorKind.IntXor:
			return valueLeft.Int32Value ^ valueRight.Int32Value;
		case BinaryOperatorKind.LongXor:
			return valueLeft.Int64Value ^ valueRight.Int64Value;
		case BinaryOperatorKind.UIntXor:
			return valueLeft.UInt32Value ^ valueRight.UInt32Value;
		case BinaryOperatorKind.ULongXor:
			return valueLeft.UInt64Value ^ valueRight.UInt64Value;
		case BinaryOperatorKind.LogicalBoolAnd:
			return valueLeft.BooleanValue && valueRight.BooleanValue;
		case BinaryOperatorKind.LogicalBoolOr:
			return valueLeft.BooleanValue || valueRight.BooleanValue;
		case BinaryOperatorKind.BoolEqual:
			return valueLeft.BooleanValue == valueRight.BooleanValue;
		case BinaryOperatorKind.StringEqual:
			return valueLeft.StringValue == valueRight.StringValue;
		case BinaryOperatorKind.DecimalEqual:
			return valueLeft.DecimalValue == valueRight.DecimalValue;
		case BinaryOperatorKind.FloatEqual:
			return valueLeft.SingleValue == valueRight.SingleValue;
		case BinaryOperatorKind.DoubleEqual:
			return valueLeft.DoubleValue == valueRight.DoubleValue;
		case BinaryOperatorKind.IntEqual:
			return valueLeft.Int32Value == valueRight.Int32Value;
		case BinaryOperatorKind.LongEqual:
			return valueLeft.Int64Value == valueRight.Int64Value;
		case BinaryOperatorKind.UIntEqual:
			return valueLeft.UInt32Value == valueRight.UInt32Value;
		case BinaryOperatorKind.ULongEqual:
			return valueLeft.UInt64Value == valueRight.UInt64Value;
		case BinaryOperatorKind.BoolNotEqual:
			return valueLeft.BooleanValue != valueRight.BooleanValue;
		case BinaryOperatorKind.StringNotEqual:
			return valueLeft.StringValue != valueRight.StringValue;
		case BinaryOperatorKind.DecimalNotEqual:
			return valueLeft.DecimalValue != valueRight.DecimalValue;
		case BinaryOperatorKind.FloatNotEqual:
			return valueLeft.SingleValue != valueRight.SingleValue;
		case BinaryOperatorKind.DoubleNotEqual:
			return valueLeft.DoubleValue != valueRight.DoubleValue;
		case BinaryOperatorKind.IntNotEqual:
			return valueLeft.Int32Value != valueRight.Int32Value;
		case BinaryOperatorKind.LongNotEqual:
			return valueLeft.Int64Value != valueRight.Int64Value;
		case BinaryOperatorKind.UIntNotEqual:
			return valueLeft.UInt32Value != valueRight.UInt32Value;
		case BinaryOperatorKind.ULongNotEqual:
			return valueLeft.UInt64Value != valueRight.UInt64Value;
		case BinaryOperatorKind.DecimalLessThan:
			return valueLeft.DecimalValue < valueRight.DecimalValue;
		case BinaryOperatorKind.FloatLessThan:
			return valueLeft.SingleValue < valueRight.SingleValue;
		case BinaryOperatorKind.DoubleLessThan:
			return valueLeft.DoubleValue < valueRight.DoubleValue;
		case BinaryOperatorKind.IntLessThan:
			return valueLeft.Int32Value < valueRight.Int32Value;
		case BinaryOperatorKind.LongLessThan:
			return valueLeft.Int64Value < valueRight.Int64Value;
		case BinaryOperatorKind.UIntLessThan:
			return valueLeft.UInt32Value < valueRight.UInt32Value;
		case BinaryOperatorKind.ULongLessThan:
			return valueLeft.UInt64Value < valueRight.UInt64Value;
		case BinaryOperatorKind.DecimalGreaterThan:
			return valueLeft.DecimalValue > valueRight.DecimalValue;
		case BinaryOperatorKind.FloatGreaterThan:
			return valueLeft.SingleValue > valueRight.SingleValue;
		case BinaryOperatorKind.DoubleGreaterThan:
			return valueLeft.DoubleValue > valueRight.DoubleValue;
		case BinaryOperatorKind.IntGreaterThan:
			return valueLeft.Int32Value > valueRight.Int32Value;
		case BinaryOperatorKind.LongGreaterThan:
			return valueLeft.Int64Value > valueRight.Int64Value;
		case BinaryOperatorKind.UIntGreaterThan:
			return valueLeft.UInt32Value > valueRight.UInt32Value;
		case BinaryOperatorKind.ULongGreaterThan:
			return valueLeft.UInt64Value > valueRight.UInt64Value;
		case BinaryOperatorKind.DecimalLessThanOrEqual:
			return valueLeft.DecimalValue <= valueRight.DecimalValue;
		case BinaryOperatorKind.FloatLessThanOrEqual:
			return valueLeft.SingleValue <= valueRight.SingleValue;
		case BinaryOperatorKind.DoubleLessThanOrEqual:
			return valueLeft.DoubleValue <= valueRight.DoubleValue;
		case BinaryOperatorKind.IntLessThanOrEqual:
			return valueLeft.Int32Value <= valueRight.Int32Value;
		case BinaryOperatorKind.LongLessThanOrEqual:
			return valueLeft.Int64Value <= valueRight.Int64Value;
		case BinaryOperatorKind.UIntLessThanOrEqual:
			return valueLeft.UInt32Value <= valueRight.UInt32Value;
		case BinaryOperatorKind.ULongLessThanOrEqual:
			return valueLeft.UInt64Value <= valueRight.UInt64Value;
		case BinaryOperatorKind.DecimalGreaterThanOrEqual:
			return valueLeft.DecimalValue >= valueRight.DecimalValue;
		case BinaryOperatorKind.FloatGreaterThanOrEqual:
			return valueLeft.SingleValue >= valueRight.SingleValue;
		case BinaryOperatorKind.DoubleGreaterThanOrEqual:
			return valueLeft.DoubleValue >= valueRight.DoubleValue;
		case BinaryOperatorKind.IntGreaterThanOrEqual:
			return valueLeft.Int32Value >= valueRight.Int32Value;
		case BinaryOperatorKind.LongGreaterThanOrEqual:
			return valueLeft.Int64Value >= valueRight.Int64Value;
		case BinaryOperatorKind.UIntGreaterThanOrEqual:
			return valueLeft.UInt32Value >= valueRight.UInt32Value;
		case BinaryOperatorKind.ULongGreaterThanOrEqual:
			return valueLeft.UInt64Value >= valueRight.UInt64Value;
		case BinaryOperatorKind.UIntDivision:
			return valueLeft.UInt32Value / valueRight.UInt32Value;
		case BinaryOperatorKind.ULongDivision:
			return valueLeft.UInt64Value / valueRight.UInt64Value;
		case BinaryOperatorKind.IntRemainder:
			return (valueRight.Int32Value != -1) ? (valueLeft.Int32Value % valueRight.Int32Value) : 0;
		case BinaryOperatorKind.LongRemainder:
			return (valueRight.Int64Value != -1) ? (valueLeft.Int64Value % valueRight.Int64Value) : 0;
		case BinaryOperatorKind.UIntRemainder:
			return valueLeft.UInt32Value % valueRight.UInt32Value;
		case BinaryOperatorKind.ULongRemainder:
			return valueLeft.UInt64Value % valueRight.UInt64Value;
		}
		return null;
	}

	private static ConstantValue FoldStringConcatenation(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight, ref int compoundStringLength)
	{
		if (kind == BinaryOperatorKind.StringConcatenation)
		{
			string text = valueLeft.StringValue ?? string.Empty;
			string text2 = valueRight.StringValue ?? string.Empty;
			if (compoundStringLength == 0)
			{
				compoundStringLength = text.Length;
			}
			long num = (long)compoundStringLength + (long)text.Length + text2.Length;
			if (num > int.MaxValue)
			{
				return ConstantValue.Bad;
			}
			try
			{
				ConstantValue result = ConstantValue.Create(text + text2);
				compoundStringLength = (int)num;
				return result;
			}
			catch (OutOfMemoryException)
			{
				return ConstantValue.Bad;
			}
		}
		return null;
	}

	private static BinaryOperatorKind SyntaxKindToBinaryOperatorKind(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.MultiplyExpression:
		case SyntaxKind.MultiplyAssignmentExpression:
			return BinaryOperatorKind.Multiplication;
		case SyntaxKind.DivideExpression:
		case SyntaxKind.DivideAssignmentExpression:
			return BinaryOperatorKind.Division;
		case SyntaxKind.ModuloExpression:
		case SyntaxKind.ModuloAssignmentExpression:
			return BinaryOperatorKind.Remainder;
		case SyntaxKind.AddExpression:
		case SyntaxKind.AddAssignmentExpression:
			return BinaryOperatorKind.Addition;
		case SyntaxKind.SubtractExpression:
		case SyntaxKind.SubtractAssignmentExpression:
			return BinaryOperatorKind.Subtraction;
		case SyntaxKind.RightShiftExpression:
		case SyntaxKind.RightShiftAssignmentExpression:
			return BinaryOperatorKind.RightShift;
		case SyntaxKind.LeftShiftExpression:
		case SyntaxKind.LeftShiftAssignmentExpression:
			return BinaryOperatorKind.LeftShift;
		case SyntaxKind.EqualsExpression:
			return BinaryOperatorKind.Equal;
		case SyntaxKind.NotEqualsExpression:
			return BinaryOperatorKind.NotEqual;
		case SyntaxKind.GreaterThanExpression:
			return BinaryOperatorKind.GreaterThan;
		case SyntaxKind.LessThanExpression:
			return BinaryOperatorKind.LessThan;
		case SyntaxKind.GreaterThanOrEqualExpression:
			return BinaryOperatorKind.GreaterThanOrEqual;
		case SyntaxKind.LessThanOrEqualExpression:
			return BinaryOperatorKind.LessThanOrEqual;
		case SyntaxKind.BitwiseAndExpression:
		case SyntaxKind.AndAssignmentExpression:
			return BinaryOperatorKind.And;
		case SyntaxKind.BitwiseOrExpression:
		case SyntaxKind.OrAssignmentExpression:
			return BinaryOperatorKind.Or;
		case SyntaxKind.ExclusiveOrExpression:
		case SyntaxKind.ExclusiveOrAssignmentExpression:
			return BinaryOperatorKind.Xor;
		case SyntaxKind.LogicalAndExpression:
			return BinaryOperatorKind.LogicalAnd;
		case SyntaxKind.LogicalOrExpression:
			return BinaryOperatorKind.LogicalOr;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private BoundExpression BindIncrementOperator(CSharpSyntaxNode node, ExpressionSyntax operandSyntax, SyntaxToken operatorToken, DiagnosticBag diagnostics)
	{
		operandSyntax.CheckDeconstructionCompatibleArgument(diagnostics);
		BoundExpression boundExpression = BindValue(operandSyntax, diagnostics, BindValueKind.IncrementDecrement);
		UnaryOperatorKind unaryOperatorKind = SyntaxKindToUnaryOperatorKind(node.Kind());
		if (boundExpression.HasAnyErrors)
		{
			return new BoundIncrementOperator(node, unaryOperatorKind, boundExpression, null, Conversion.NoConversion, Conversion.NoConversion, LookupResultKind.Empty, CreateErrorType(), hasErrors: true);
		}
		TypeSymbol type = boundExpression.Type;
		if (type.IsDynamic())
		{
			return new BoundIncrementOperator(node, unaryOperatorKind.WithType(UnaryOperatorKind.Dynamic).WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), boundExpression, null, Conversion.NoConversion, Conversion.NoConversion, LookupResultKind.Viable, default, type);
		}
		UnaryOperatorAnalysisResult unaryOperatorAnalysisResult = UnaryOperatorOverloadResolution(unaryOperatorKind, boundExpression, node, diagnostics, out var resultKind, out var originalUserDefinedOperators);
		if (!unaryOperatorAnalysisResult.HasValue)
		{
			ReportUnaryOperatorError(node, diagnostics, operatorToken.Text, boundExpression, resultKind);
			return new BoundIncrementOperator(node, unaryOperatorKind, boundExpression, null, Conversion.NoConversion, Conversion.NoConversion, resultKind, originalUserDefinedOperators, CreateErrorType(), hasErrors: true);
		}
		UnaryOperatorSignature signature = unaryOperatorAnalysisResult.Signature;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromType(signature.ReturnType, type, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		bool flag = false;
		if (!conversion.IsImplicit || !conversion.IsValid)
		{
			GenerateImplicitConversionError(diagnostics, Compilation, node, conversion, signature.ReturnType, type);
			flag = true;
		}
		else
		{
			ReportDiagnosticsIfObsolete(diagnostics, conversion, node, hasBaseReceiver: false);
		}
		if (!flag && type.IsVoidPointer())
		{
			Error(diagnostics, ErrorCode.ERR_VoidError, node);
			flag = true;
		}
		Conversion conversion2 = unaryOperatorAnalysisResult.Conversion;
		ReportDiagnosticsIfObsolete(diagnostics, conversion2, node, hasBaseReceiver: false);
		return new BoundIncrementOperator(node, signature.Kind.WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), boundExpression, signature.Method, conversion2, conversion, resultKind, originalUserDefinedOperators, type, flag);
	}

	private BoundExpression BindPointerIndirectionExpression(PrefixUnaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression operand = BindValue(node.Operand, diagnostics, GetUnaryAssignmentKind(node.Kind()));
		BindPointerIndirectionExpressionInternal(node, operand, diagnostics, out var pointedAtType, out var hasErrors);
		return new BoundPointerIndirectionOperator(node, operand, pointedAtType ?? CreateErrorType(), hasErrors);
	}

	private static void BindPointerIndirectionExpressionInternal(CSharpSyntaxNode node, BoundExpression operand, DiagnosticBag diagnostics, out TypeSymbol pointedAtType, out bool hasErrors)
	{
		PointerTypeSymbol pointerTypeSymbol = operand.Type as PointerTypeSymbol;
		hasErrors = operand.HasAnyErrors;
		if ((object)pointerTypeSymbol == null)
		{
			pointedAtType = null;
			if (!hasErrors)
			{
				Error(diagnostics, ErrorCode.ERR_PtrExpected, node);
				hasErrors = true;
			}
			return;
		}
		pointedAtType = pointerTypeSymbol.PointedAtType;
		if (pointedAtType.SpecialType == SpecialType.System_Void)
		{
			pointedAtType = null;
			if (!hasErrors)
			{
				Error(diagnostics, ErrorCode.ERR_VoidError, node);
				hasErrors = true;
			}
		}
	}

	private BoundExpression BindAddressOfExpression(PrefixUnaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Operand, diagnostics, BindValueKind.AddressOf);
		bool flag = boundExpression.HasAnyErrors;
		bool flag2 = SyntaxFacts.IsFixedStatementExpression(node);
		BoundKind kind = boundExpression.Kind;
		if (kind == BoundKind.MethodGroup || kind - 136 <= BoundKind.PropertyEqualsValue)
		{
			return new BoundAddressOfOperator(node, boundExpression, CreateErrorType(), hasErrors: true);
		}
		TypeSymbol type = boundExpression.Type;
		bool isManagedType = type.IsManagedType;
		bool flag3 = Flags.Includes(BinderFlags.AllowManagedAddressOf);
		if (!flag3)
		{
			if (!flag & isManagedType)
			{
				flag = true;
				Error(diagnostics, ErrorCode.ERR_ManagedAddr, node, type);
			}
			if (!flag && IsMoveableVariable(boundExpression, out var _) != flag2)
			{
				Error(diagnostics, flag2 ? ErrorCode.ERR_FixedNotNeeded : ErrorCode.ERR_FixedNeeded, node);
				flag = true;
			}
		}
		TypeSymbol type2 = new PointerTypeSymbol((isManagedType & flag3) ? GetSpecialType(SpecialType.System_IntPtr, diagnostics, node) : (type ?? CreateErrorType()));
		return new BoundAddressOfOperator(node, boundExpression, type2, flag);
	}

	internal bool IsMoveableVariable(BoundExpression expr, out Symbol accessedLocalOrParameterOpt)
	{
		accessedLocalOrParameterOpt = null;
		while (true)
		{
			FieldSymbol fieldSymbol;
			BoundExpression receiverOpt;
			DiagnosticBag instance;
			bool flag;
			switch (expr.Kind)
			{
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess2 = (BoundFieldAccess)expr;
				fieldSymbol = boundFieldAccess2.FieldSymbol;
				receiverOpt = boundFieldAccess2.ReceiverOpt;
				goto IL_00c9;
			}
			case BoundKind.EventAccess:
			{
				BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
				if (!boundEventAccess.IsUsableAsField || boundEventAccess.EventSymbol.IsWindowsRuntimeEvent)
				{
					return true;
				}
				fieldSymbol = boundEventAccess.EventSymbol.AssociatedField;
				receiverOpt = boundEventAccess.ReceiverOpt;
				goto IL_00c9;
			}
			case BoundKind.RangeVariable:
				expr = ((BoundRangeVariable)expr).Value;
				break;
			case BoundKind.Parameter:
				return ((ParameterSymbol)(accessedLocalOrParameterOpt = ((BoundParameter)expr).ParameterSymbol)).RefKind != RefKind.None;
			case BoundKind.ThisReference:
			case BoundKind.BaseReference:
				accessedLocalOrParameterOpt = ContainingMemberOrLambda.EnclosingThisSymbol();
				return true;
			case BoundKind.Local:
				return ((LocalSymbol)(accessedLocalOrParameterOpt = ((BoundLocal)expr).LocalSymbol)).RefKind != RefKind.None;
			case BoundKind.PointerIndirectionOperator:
			case BoundKind.ConvertedStackAllocExpression:
				return false;
			case BoundKind.PointerElementAccess:
				if (((BoundPointerElementAccess)expr).Expression is BoundFieldAccess boundFieldAccess && boundFieldAccess.FieldSymbol.IsFixed)
				{
					expr = boundFieldAccess.ReceiverOpt;
					break;
				}
				return false;
			default:
				{
					return true;
				}
				IL_00c9:
				if ((object)fieldSymbol == null || fieldSymbol.IsStatic || receiverOpt == null)
				{
					return true;
				}
				instance = DiagnosticBag.GetInstance();
				flag = CheckValueKind(receiverOpt.Syntax, receiverOpt, BindValueKind.AddressOf, checkingReceiver: false, instance);
				instance.Free();
				if (!flag)
				{
					return true;
				}
				if (receiverOpt.Type.IsReferenceType)
				{
					return true;
				}
				expr = receiverOpt;
				break;
			}
		}
	}

	private BoundExpression BindUnaryOperator(PrefixUnaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression operand = BindValue(node.Operand, diagnostics, GetUnaryAssignmentKind(node.Kind()));
		return BindIntegralMinValConstants(node, operand, diagnostics) ?? BindUnaryOperatorCore(node, node.OperatorToken.Text, operand, diagnostics);
	}

	private BoundExpression BindUnaryOperatorCore(CSharpSyntaxNode node, string operatorText, BoundExpression operand, DiagnosticBag diagnostics)
	{
		UnaryOperatorKind unaryOperatorKind = SyntaxKindToUnaryOperatorKind(node.Kind());
		bool flag = operand.IsLiteralNull() || operand.IsLiteralDefault();
		if (flag)
		{
			Error(diagnostics, ErrorCode.ERR_BadOpOnNullOrDefault, node, operatorText, operand.Display);
		}
		if (operand.HasAnyErrors | flag)
		{
			return new BoundUnaryOperator(node, unaryOperatorKind, operand, null, null, LookupResultKind.Empty, CreateErrorType(), hasErrors: true);
		}
		if (operand.HasDynamicType())
		{
			return new BoundUnaryOperator(node, unaryOperatorKind.WithType(UnaryOperatorKind.Dynamic).WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), operand, null, null, LookupResultKind.Viable, operand.Type);
		}
		UnaryOperatorAnalysisResult unaryOperatorAnalysisResult = UnaryOperatorOverloadResolution(unaryOperatorKind, operand, node, diagnostics, out var resultKind, out var originalUserDefinedOperators);
		if (!unaryOperatorAnalysisResult.HasValue)
		{
			ReportUnaryOperatorError(node, diagnostics, operatorText, operand, resultKind);
			return new BoundUnaryOperator(node, unaryOperatorKind, operand, null, null, resultKind, originalUserDefinedOperators, CreateErrorType(), hasErrors: true);
		}
		UnaryOperatorSignature signature = unaryOperatorAnalysisResult.Signature;
		BoundExpression operand2 = CreateConversion(operand.Syntax, operand, unaryOperatorAnalysisResult.Conversion, isCast: false, signature.OperandType, diagnostics);
		TypeSymbol returnType = signature.ReturnType;
		UnaryOperatorKind kind = signature.Kind;
		MethodSymbol method = signature.Method;
		ConstantValue constantValueOpt = FoldUnaryOperator(node, kind, operand2, returnType.SpecialType, diagnostics);
		return new BoundUnaryOperator(node, kind.WithOverflowChecksIfApplicable(CheckOverflowAtRuntime), operand2, constantValueOpt, method, resultKind, returnType);
	}

	private ConstantValue FoldEnumUnaryOperator(CSharpSyntaxNode syntax, UnaryOperatorKind kind, BoundExpression operand, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol enumUnderlyingType = operand.Type.GetEnumUnderlyingType();
		BoundExpression source = CreateConversion(operand, enumUnderlyingType, diagnostics);
		SpecialType enumPromotedType = GetEnumPromotedType(enumUnderlyingType.SpecialType);
		NamedTypeSymbol namedTypeSymbol = ((enumPromotedType == enumUnderlyingType.SpecialType) ? enumUnderlyingType : GetSpecialType(enumPromotedType, diagnostics, syntax));
		CreateConversion(source, namedTypeSymbol, diagnostics);
		UnaryOperatorKind kind2 = kind.Operator().WithType(enumPromotedType);
		ConstantValue constantValue = FoldUnaryOperator(syntax, kind2, operand, namedTypeSymbol.SpecialType, diagnostics);
		if (!constantValue.IsBad)
		{
			return ((kind.Operator() == UnaryOperatorKind.BitwiseComplement) ? WithCheckedOrUncheckedRegion(@checked: false) : this).FoldConstantNumericConversion(syntax, constantValue, enumUnderlyingType, diagnostics);
		}
		return constantValue;
	}

	private ConstantValue FoldUnaryOperator(CSharpSyntaxNode syntax, UnaryOperatorKind kind, BoundExpression operand, SpecialType resultType, DiagnosticBag diagnostics)
	{
		if (operand.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = operand.ConstantValue;
		if (constantValue == null || constantValue.IsBad)
		{
			return constantValue;
		}
		if (kind.IsEnum() && !kind.IsLifted())
		{
			return FoldEnumUnaryOperator(syntax, kind, operand, diagnostics);
		}
		object obj = FoldNeverOverflowUnaryOperator(kind, constantValue);
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		if (CheckOverflowAtCompileTime)
		{
			try
			{
				obj = FoldCheckedIntegralUnaryOperator(kind, constantValue);
			}
			catch (OverflowException)
			{
				Error(diagnostics, ErrorCode.ERR_CheckedOverflow, syntax);
				return ConstantValue.Bad;
			}
		}
		else
		{
			obj = FoldUncheckedIntegralUnaryOperator(kind, constantValue);
		}
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		return null;
	}

	private static object FoldNeverOverflowUnaryOperator(UnaryOperatorKind kind, ConstantValue value)
	{
		switch (kind)
		{
		case UnaryOperatorKind.DecimalUnaryMinus:
			return -value.DecimalValue;
		case UnaryOperatorKind.FloatUnaryMinus:
		case UnaryOperatorKind.DoubleUnaryMinus:
			return 0.0 - value.DoubleValue;
		case UnaryOperatorKind.DecimalUnaryPlus:
			return value.DecimalValue;
		case UnaryOperatorKind.FloatUnaryPlus:
		case UnaryOperatorKind.DoubleUnaryPlus:
			return value.DoubleValue;
		case UnaryOperatorKind.LongUnaryPlus:
			return value.Int64Value;
		case UnaryOperatorKind.ULongUnaryPlus:
			return value.UInt64Value;
		case UnaryOperatorKind.IntUnaryPlus:
			return value.Int32Value;
		case UnaryOperatorKind.UIntUnaryPlus:
			return value.UInt32Value;
		case UnaryOperatorKind.BoolLogicalNegation:
			return !value.BooleanValue;
		case UnaryOperatorKind.IntBitwiseComplement:
			return ~value.Int32Value;
		case UnaryOperatorKind.LongBitwiseComplement:
			return ~value.Int64Value;
		case UnaryOperatorKind.UIntBitwiseComplement:
			return ~value.UInt32Value;
		case UnaryOperatorKind.ULongBitwiseComplement:
			return ~value.UInt64Value;
		default:
			return null;
		}
	}

	private static object FoldUncheckedIntegralUnaryOperator(UnaryOperatorKind kind, ConstantValue value)
	{
		return kind switch
		{
			UnaryOperatorKind.LongUnaryMinus => (object)(-value.Int64Value), 
			UnaryOperatorKind.IntUnaryMinus => -value.Int32Value, 
			_ => null, 
		};
	}

	private static object FoldCheckedIntegralUnaryOperator(UnaryOperatorKind kind, ConstantValue value)
	{
		return checked(kind switch
		{
			UnaryOperatorKind.LongUnaryMinus => (object)(-value.Int64Value), 
			UnaryOperatorKind.IntUnaryMinus => -value.Int32Value, 
			_ => null, 
		});
	}

	private static UnaryOperatorKind SyntaxKindToUnaryOperatorKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PreIncrementExpression => UnaryOperatorKind.PrefixIncrement, 
			SyntaxKind.PostIncrementExpression => UnaryOperatorKind.PostfixIncrement, 
			SyntaxKind.PreDecrementExpression => UnaryOperatorKind.PrefixDecrement, 
			SyntaxKind.PostDecrementExpression => UnaryOperatorKind.PostfixDecrement, 
			SyntaxKind.UnaryPlusExpression => UnaryOperatorKind.UnaryPlus, 
			SyntaxKind.UnaryMinusExpression => UnaryOperatorKind.UnaryMinus, 
			SyntaxKind.LogicalNotExpression => UnaryOperatorKind.LogicalNegation, 
			SyntaxKind.BitwiseNotExpression => UnaryOperatorKind.BitwiseComplement, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private static BindValueKind GetBinaryAssignmentKind(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.SimpleAssignmentExpression:
			return BindValueKind.Assignable;
		case SyntaxKind.AddAssignmentExpression:
		case SyntaxKind.SubtractAssignmentExpression:
		case SyntaxKind.MultiplyAssignmentExpression:
		case SyntaxKind.DivideAssignmentExpression:
		case SyntaxKind.ModuloAssignmentExpression:
		case SyntaxKind.AndAssignmentExpression:
		case SyntaxKind.ExclusiveOrAssignmentExpression:
		case SyntaxKind.OrAssignmentExpression:
		case SyntaxKind.LeftShiftAssignmentExpression:
		case SyntaxKind.RightShiftAssignmentExpression:
			return BindValueKind.CompoundAssignment;
		default:
			return BindValueKind.RValue;
		}
	}

	private static BindValueKind GetUnaryAssignmentKind(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.PreIncrementExpression:
		case SyntaxKind.PreDecrementExpression:
		case SyntaxKind.PostIncrementExpression:
		case SyntaxKind.PostDecrementExpression:
			return BindValueKind.IncrementDecrement;
		default:
			return BindValueKind.RValue;
		}
	}

	private BoundLiteral BindIntegralMinValConstants(PrefixUnaryExpressionSyntax node, BoundExpression operand, DiagnosticBag diagnostics)
	{
		if (node.Kind() != SyntaxKind.UnaryMinusExpression)
		{
			return null;
		}
		if (node.Operand != operand.Syntax || operand.Syntax.Kind() != SyntaxKind.NumericLiteralExpression)
		{
			return null;
		}
		SyntaxToken token = ((LiteralExpressionSyntax)operand.Syntax).Token;
		if (token.Value is uint)
		{
			if ((uint)token.Value != 2147483648u)
			{
				return null;
			}
			if (token.Text.Contains("u") || token.Text.Contains("U") || token.Text.Contains("l") || token.Text.Contains("L"))
			{
				return null;
			}
			return new BoundLiteral(node, ConstantValue.Create(int.MinValue), GetSpecialType(SpecialType.System_Int32, diagnostics, node));
		}
		if (token.Value is ulong)
		{
			if ((ulong)token.Value != 9223372036854775808uL)
			{
				return null;
			}
			if (token.Text.Contains("u") || token.Text.Contains("U"))
			{
				return null;
			}
			return new BoundLiteral(node, ConstantValue.Create(long.MinValue), GetSpecialType(SpecialType.System_Int64, diagnostics, node));
		}
		return null;
	}

	private static bool IsDivisionByZero(BinaryOperatorKind kind, ConstantValue valueRight)
	{
		switch (kind)
		{
		case BinaryOperatorKind.DecimalDivision:
		case BinaryOperatorKind.DecimalRemainder:
			return valueRight.DecimalValue == 0.0m;
		case BinaryOperatorKind.IntDivision:
		case BinaryOperatorKind.IntRemainder:
			return valueRight.Int32Value == 0;
		case BinaryOperatorKind.LongDivision:
		case BinaryOperatorKind.LongRemainder:
			return valueRight.Int64Value == 0;
		case BinaryOperatorKind.UIntDivision:
		case BinaryOperatorKind.UIntRemainder:
			return valueRight.UInt32Value == 0;
		case BinaryOperatorKind.ULongDivision:
		case BinaryOperatorKind.ULongRemainder:
			return valueRight.UInt64Value == 0;
		default:
			return false;
		}
	}

	private bool IsOperandErrors(CSharpSyntaxNode node, ref BoundExpression operand, DiagnosticBag diagnostics)
	{
		BoundKind kind = operand.Kind;
		if (kind == BoundKind.MethodGroup || kind - 136 <= BoundKind.PropertyEqualsValue)
		{
			if (!operand.HasAnyErrors)
			{
				Error(diagnostics, ErrorCode.ERR_LambdaInIsAs, node);
				operand = BadExpression(node, operand).MakeCompilerGenerated();
			}
			return true;
		}
		if ((object)operand.Type == null && !operand.IsLiteralNull())
		{
			if (!operand.HasAnyErrors)
			{
				Error(diagnostics, ErrorCode.ERR_BadUnaryOp, node, SyntaxFacts.GetText(SyntaxKind.IsKeyword), operand.Display);
			}
			return true;
		}
		return operand.HasAnyErrors;
	}

	private bool IsOperatorErrors(CSharpSyntaxNode node, TypeSymbol operandType, BoundTypeExpression typeExpression, DiagnosticBag diagnostics)
	{
		TypeSymbol type = typeExpression.Type;
		TypeKind typeKind = type.TypeKind;
		if (type.IsStatic && Compilation.FeatureStrictEnabled)
		{
			Error(diagnostics, ErrorCode.ERR_StaticInAsOrIs, node, type);
			return true;
		}
		if (((object)operandType != null && operandType.TypeKind == TypeKind.Pointer) || typeKind == TypeKind.Pointer)
		{
			Error(diagnostics, ErrorCode.ERR_PointerInAsOrIs, node);
			return true;
		}
		return typeKind == TypeKind.Error;
	}

	private BoundExpression BindIsOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		TypeSymbol specialType = GetSpecialType(SpecialType.System_Boolean, diagnostics, node);
		BoundExpression operand = BindValue(node.Left, diagnostics, BindValueKind.RValue);
		bool flag = IsOperandErrors(node, ref operand, diagnostics);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		TypeSymbol typeSymbol = BindType(node.Right, instance, out var alias);
		if ((object)typeSymbol != null && typeSymbol.IsErrorType() && instance.HasAnyResolvedErrors() && ((CSharpParseOptions)node.SyntaxTree.Options).IsFeatureEnabled(MessageID.IDS_FeaturePatternMatching))
		{
			DiagnosticBag instance2 = DiagnosticBag.GetInstance();
			if ((object)operand.Type == null)
			{
				if (!flag)
				{
					instance2.Add(ErrorCode.ERR_BadIsPatternExpression, node.Left.Location, operand.Display);
				}
				operand = ToBadExpression(operand);
			}
			BoundConstantPattern boundConstantPattern = BindConstantPattern(node.Right, operand.Type, node.Right, node.Right.HasErrors, instance2, out var wasExpression);
			boundConstantPattern.WasCompilerGenerated = true;
			if (wasExpression)
			{
				instance.Free();
				diagnostics.AddRangeAndFree(instance2);
				return new BoundIsPatternExpression(node, operand, boundConstantPattern, specialType, flag);
			}
			instance2.Free();
		}
		diagnostics.AddRangeAndFree(instance);
		BoundTypeExpression boundTypeExpression = new BoundTypeExpression(node.Right, alias, typeSymbol);
		TypeKind typeKind = typeSymbol.TypeKind;
		if (flag || IsOperatorErrors(node, operand.Type, boundTypeExpression, diagnostics))
		{
			return new BoundIsOperator(node, operand, boundTypeExpression, Conversion.NoConversion, specialType, hasErrors: true);
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (operand.ConstantValue == ConstantValue.Null || operand.Kind == BoundKind.MethodGroup || operand.Type.SpecialType == SpecialType.System_Void)
		{
			Error(diagnostics, ErrorCode.WRN_IsAlwaysFalse, node, typeSymbol);
			Conversion conversion = Conversions.ClassifyConversionFromExpression(operand, typeSymbol, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			return new BoundIsOperator(node, operand, boundTypeExpression, conversion, specialType);
		}
		if (typeKind == TypeKind.Dynamic)
		{
			Error(diagnostics, ErrorCode.WRN_IsDynamicIsConfusing, node, node.OperatorToken.Text, typeSymbol.Name, GetSpecialType(SpecialType.System_Object, diagnostics, node).Name);
		}
		TypeSymbol typeSymbol2 = operand.Type;
		if (typeSymbol2.TypeKind == TypeKind.Dynamic)
		{
			typeSymbol2 = GetSpecialType(SpecialType.System_Object, diagnostics, node);
		}
		Conversion conversion2 = Conversions.ClassifyBuiltInConversion(typeSymbol2, typeSymbol, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		ReportIsOperatorConstantWarnings(node, diagnostics, typeSymbol2, typeSymbol, conversion2.Kind, operand.ConstantValue);
		return new BoundIsOperator(node, operand, boundTypeExpression, conversion2, specialType);
	}

	private static void ReportIsOperatorConstantWarnings(CSharpSyntaxNode syntax, DiagnosticBag diagnostics, TypeSymbol operandType, TypeSymbol targetType, ConversionKind conversionKind, ConstantValue operandConstantValue)
	{
		ConstantValue isOperatorConstantResult = GetIsOperatorConstantResult(operandType, targetType, conversionKind, operandConstantValue);
		if (isOperatorConstantResult != null)
		{
			ErrorCode code = ((isOperatorConstantResult == ConstantValue.True) ? ErrorCode.WRN_IsAlwaysTrue : ErrorCode.WRN_IsAlwaysFalse);
			Error(diagnostics, code, syntax, targetType);
		}
	}

	internal static ConstantValue GetIsOperatorConstantResult(TypeSymbol operandType, TypeSymbol targetType, ConversionKind conversionKind, ConstantValue operandConstantValue, bool operandCouldBeNull = true)
	{
		if (operandConstantValue == ConstantValue.Null)
		{
			return ConstantValue.False;
		}
		operandCouldBeNull = operandCouldBeNull && operandType.CanContainNull() && (operandConstantValue == null || operandConstantValue == ConstantValue.Null);
		switch (conversionKind)
		{
		case ConversionKind.NoConversion:
			if (!operandType.ContainsTypeParameter() && !targetType.ContainsTypeParameter())
			{
				return ConstantValue.False;
			}
			if (operandType.IsValueType && targetType.IsClassType() && targetType.SpecialType != SpecialType.System_Enum)
			{
				return ConstantValue.False;
			}
			if (targetType.IsRestrictedType() || operandType.IsRestrictedType())
			{
				return ConstantValue.False;
			}
			return null;
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ExplicitTuple:
		case ConversionKind.ImplicitConstant:
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitNumeric:
		case ConversionKind.ExplicitUserDefined:
		case ConversionKind.IntPtr:
			return ConstantValue.False;
		case ConversionKind.ExplicitEnumeration:
			if (!operandType.IsEnumType() || !targetType.IsEnumType())
			{
				return ConstantValue.False;
			}
			goto case ConversionKind.NoConversion;
		case ConversionKind.ExplicitNullable:
			if (targetType.IsNullableType())
			{
				return ConstantValue.False;
			}
			if (ConversionsBase.HasIdentityConversion(operandType.GetNullableUnderlyingType(), targetType))
			{
				if (!operandCouldBeNull)
				{
					return ConstantValue.True;
				}
				return null;
			}
			return ConstantValue.False;
		case ConversionKind.ImplicitReference:
			if (!operandCouldBeNull)
			{
				return ConstantValue.True;
			}
			return null;
		case ConversionKind.ExplicitReference:
		case ConversionKind.Unboxing:
			return null;
		case ConversionKind.Identity:
			if (!operandCouldBeNull)
			{
				return ConstantValue.True;
			}
			return null;
		case ConversionKind.Boxing:
			if (!operandCouldBeNull)
			{
				return ConstantValue.True;
			}
			return null;
		case ConversionKind.ImplicitNullable:
			if (!operandType.Equals(targetType.GetNullableUnderlyingType(), TypeCompareKind.AllIgnoreOptions))
			{
				return ConstantValue.False;
			}
			return ConstantValue.True;
		default:
			throw ExceptionUtilities.UnexpectedValue(conversionKind);
		}
	}

	private BoundExpression BindAsOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Left, diagnostics, BindValueKind.RValue);
		TypeSymbol typeSymbol = BindType(node.Right, diagnostics, out var alias);
		BoundTypeExpression targetType = new BoundTypeExpression(node.Right, alias, typeSymbol);
		TypeKind typeKind = typeSymbol.TypeKind;
		TypeSymbol type = typeSymbol;
		switch (boundExpression.Kind)
		{
		case BoundKind.MethodGroup:
		case BoundKind.Lambda:
		case BoundKind.UnboundLambda:
			if (!boundExpression.HasAnyErrors)
			{
				Error(diagnostics, ErrorCode.ERR_LambdaInIsAs, node);
			}
			return new BoundAsOperator(node, boundExpression, targetType, Conversion.NoConversion, type, hasErrors: true);
		case BoundKind.TupleLiteral:
			if ((object)boundExpression.Type == null)
			{
				Error(diagnostics, ErrorCode.ERR_TypelessTupleInAs, node);
				return new BoundAsOperator(node, boundExpression, targetType, Conversion.NoConversion, type, hasErrors: true);
			}
			break;
		}
		if (boundExpression.HasAnyErrors || typeKind == TypeKind.Error)
		{
			return new BoundAsOperator(node, boundExpression, targetType, Conversion.NoConversion, type, hasErrors: true);
		}
		if (!typeSymbol.IsReferenceType && !typeSymbol.IsNullableType())
		{
			switch (typeKind)
			{
			case TypeKind.TypeParameter:
				Error(diagnostics, ErrorCode.ERR_AsWithTypeVar, node, typeSymbol);
				break;
			case TypeKind.Pointer:
				Error(diagnostics, ErrorCode.ERR_PointerInAsOrIs, node);
				break;
			default:
				Error(diagnostics, ErrorCode.ERR_AsMustHaveReferenceType, node, typeSymbol);
				break;
			}
			return new BoundAsOperator(node, boundExpression, targetType, Conversion.NoConversion, type, hasErrors: true);
		}
		if (typeSymbol.IsStatic && Compilation.FeatureStrictEnabled)
		{
			Error(diagnostics, ErrorCode.ERR_StaticInAsOrIs, node, typeSymbol);
			return new BoundAsOperator(node, boundExpression, targetType, Conversion.NoConversion, type, hasErrors: true);
		}
		if (boundExpression.IsLiteralNull())
		{
			return new BoundAsOperator(node, boundExpression, targetType, Conversion.DefaultOrNullLiteral, type);
		}
		if (boundExpression.IsLiteralDefault())
		{
			boundExpression = new BoundDefaultExpression(((BoundDefaultExpression)boundExpression).Syntax, ConstantValue.Null, GetSpecialType(SpecialType.System_Object, diagnostics, node));
		}
		TypeSymbol typeSymbol2 = boundExpression.Type;
		switch (typeSymbol2.TypeKind)
		{
		case TypeKind.Pointer:
			Error(diagnostics, ErrorCode.ERR_PointerInAsOrIs, node);
			return new BoundAsOperator(node, boundExpression, targetType, Conversion.NoConversion, type, hasErrors: true);
		case TypeKind.Dynamic:
			typeSymbol2 = GetSpecialType(SpecialType.System_Object, diagnostics, node);
			_ = typeSymbol2.TypeKind;
			break;
		}
		if (typeKind == TypeKind.Dynamic)
		{
			typeSymbol = GetSpecialType(SpecialType.System_Object, diagnostics, node);
			_ = typeSymbol.TypeKind;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyBuiltInConversion(typeSymbol2, typeSymbol, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		bool hasErrors = ReportAsOperatorConversionDiagnostics(node, diagnostics, Compilation, typeSymbol2, typeSymbol, conversion.Kind, boundExpression.ConstantValue);
		return new BoundAsOperator(node, boundExpression, targetType, conversion, type, hasErrors);
	}

	private static bool ReportAsOperatorConversionDiagnostics(CSharpSyntaxNode node, DiagnosticBag diagnostics, Compilation compilation, TypeSymbol operandType, TypeSymbol targetType, ConversionKind conversionKind, ConstantValue operandConstantValue)
	{
		bool flag = false;
		switch (conversionKind)
		{
		default:
			if ((!operandType.ContainsTypeParameter() && !targetType.ContainsTypeParameter()) || operandType.SpecialType == SpecialType.System_Void)
			{
				SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(compilation, operandType, targetType);
				Error(diagnostics, ErrorCode.ERR_NoExplicitBuiltinConv, node, symbolDistinguisher.First, symbolDistinguisher.Second);
				flag = true;
			}
			break;
		case ConversionKind.Identity:
		case ConversionKind.ImplicitNullable:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
		case ConversionKind.ExplicitNullable:
		case ConversionKind.ExplicitReference:
		case ConversionKind.Unboxing:
			break;
		}
		if (!flag)
		{
			ReportAsOperatorConstantWarnings(node, diagnostics, operandType, targetType, conversionKind, operandConstantValue);
		}
		return flag;
	}

	private static void ReportAsOperatorConstantWarnings(CSharpSyntaxNode node, DiagnosticBag diagnostics, TypeSymbol operandType, TypeSymbol targetType, ConversionKind conversionKind, ConstantValue operandConstantValue)
	{
		if (GetAsOperatorConstantResult(operandType, targetType, conversionKind, operandConstantValue) != null)
		{
			Error(diagnostics, ErrorCode.WRN_AlwaysNull, node, targetType);
		}
	}

	internal static ConstantValue GetAsOperatorConstantResult(TypeSymbol operandType, TypeSymbol targetType, ConversionKind conversionKind, ConstantValue operandConstantValue)
	{
		ConstantValue isOperatorConstantResult = GetIsOperatorConstantResult(operandType, targetType, conversionKind, operandConstantValue);
		if (isOperatorConstantResult != null && !isOperatorConstantResult.BooleanValue)
		{
			return ConstantValue.Null;
		}
		return null;
	}

	private BoundExpression GenerateNullCoalescingBadBinaryOpsError(BinaryExpressionSyntax node, BoundExpression leftOperand, BoundExpression rightOperand, Conversion leftConversion, DiagnosticBag diagnostics)
	{
		Error(diagnostics, ErrorCode.ERR_BadBinaryOps, node, SyntaxFacts.GetText(node.OperatorToken.Kind()), leftOperand.Display, rightOperand.Display);
		return new BoundNullCoalescingOperator(node, leftOperand, rightOperand, leftConversion, CreateErrorType(), hasErrors: true);
	}

	private BoundExpression BindNullCoalescingOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Left, diagnostics, BindValueKind.RValue);
		BoundExpression boundExpression2 = BindValue(node.Right, diagnostics, BindValueKind.RValue);
		if (boundExpression.HasAnyErrors || boundExpression2.HasAnyErrors)
		{
			return new BoundNullCoalescingOperator(node, boundExpression, boundExpression2, Conversion.NoConversion, CreateErrorType(), hasErrors: true);
		}
		if (boundExpression.IsLiteralDefault())
		{
			Error(diagnostics, ErrorCode.ERR_BadOpOnNullOrDefault, node, node.OperatorToken.Text, "default");
			return new BoundNullCoalescingOperator(node, boundExpression, boundExpression2, Conversion.NoConversion, CreateErrorType(), hasErrors: true);
		}
		TypeSymbol type = boundExpression.Type;
		TypeSymbol type2 = boundExpression2.Type;
		bool flag = type?.IsNullableType() ?? false;
		TypeSymbol typeSymbol = (flag ? type.GetNullableUnderlyingType() : type);
		if (boundExpression.Kind == BoundKind.UnboundLambda || boundExpression.Kind == BoundKind.MethodGroup)
		{
			return GenerateNullCoalescingBadBinaryOpsError(node, boundExpression, boundExpression2, Conversion.NoConversion, diagnostics);
		}
		if ((object)type != null && !type.IsReferenceType && !flag)
		{
			return GenerateNullCoalescingBadBinaryOpsError(node, boundExpression, boundExpression2, Conversion.NoConversion, diagnostics);
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if ((object)type2 != null && type2.IsDynamic())
		{
			Conversion leftConversion = Conversions.ClassifyConversionFromExpression(boundExpression, GetSpecialType(SpecialType.System_Object, diagnostics, node), ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			return new BoundNullCoalescingOperator(node, boundExpression, boundExpression2, leftConversion, type2);
		}
		if (flag)
		{
			Conversion conversion = Conversions.ClassifyImplicitConversionFromExpression(boundExpression2, typeSymbol, ref useSiteDiagnostics);
			if (conversion.Exists)
			{
				Conversion leftConversion2 = Conversions.ClassifyConversionFromExpression(boundExpression, typeSymbol, ref useSiteDiagnostics);
				diagnostics.Add(node, useSiteDiagnostics);
				BoundExpression rightOperand = CreateConversion(boundExpression2, conversion, typeSymbol, diagnostics);
				return new BoundNullCoalescingOperator(node, boundExpression, rightOperand, leftConversion2, typeSymbol);
			}
		}
		if ((object)type != null)
		{
			Conversion conversion2 = Conversions.ClassifyImplicitConversionFromExpression(boundExpression2, type, ref useSiteDiagnostics);
			if (conversion2.Exists)
			{
				BoundExpression rightOperand2 = CreateConversion(boundExpression2, conversion2, type, diagnostics);
				Conversion identity = Conversion.Identity;
				diagnostics.Add(node, useSiteDiagnostics);
				return new BoundNullCoalescingOperator(node, boundExpression, rightOperand2, identity, type);
			}
		}
		if ((object)type2 != null)
		{
			Conversion conversion3 = ((!flag) ? Conversions.ClassifyImplicitConversionFromExpression(boundExpression, type2, ref useSiteDiagnostics) : Conversions.ClassifyImplicitConversionFromType(typeSymbol, type2, ref useSiteDiagnostics));
			if (conversion3.Exists)
			{
				if (!conversion3.IsValid)
				{
					if (flag)
					{
						Conversion conversion4 = Conversion.MakeNullableConversion(ConversionKind.ExplicitNullable, conversion3);
						BoundExpression source = CreateConversion(boundExpression, conversion4, typeSymbol, diagnostics);
						boundExpression = CreateConversion(source, conversion3, type2, diagnostics);
					}
					else
					{
						boundExpression = CreateConversion(boundExpression, conversion3, type2, diagnostics);
					}
				}
				else
				{
					ReportDiagnosticsIfObsolete(diagnostics, conversion3, node, hasBaseReceiver: false);
				}
				diagnostics.Add(node, useSiteDiagnostics);
				return new BoundNullCoalescingOperator(node, boundExpression, boundExpression2, conversion3, type2);
			}
		}
		diagnostics.Add(node, useSiteDiagnostics);
		return GenerateNullCoalescingBadBinaryOpsError(node, boundExpression, boundExpression2, Conversion.NoConversion, diagnostics);
	}

	private BoundExpression BindConditionalOperator(ConditionalExpressionSyntax node, DiagnosticBag diagnostics)
	{
		ExpressionSyntax expressionSyntax = node.WhenTrue.CheckAndUnwrapRefExpression(diagnostics, out var refKind);
		ExpressionSyntax expressionSyntax2 = node.WhenFalse.CheckAndUnwrapRefExpression(diagnostics, out var refKind2);
		bool flag = refKind == RefKind.Ref && refKind2 == RefKind.Ref;
		if (!flag)
		{
			if (refKind2 == RefKind.Ref)
			{
				diagnostics.Add(ErrorCode.ERR_RefConditionalNeedsTwoRefs, expressionSyntax2.GetFirstToken().GetLocation());
			}
			if (refKind == RefKind.Ref)
			{
				diagnostics.Add(ErrorCode.ERR_RefConditionalNeedsTwoRefs, expressionSyntax.GetFirstToken().GetLocation());
			}
		}
		else
		{
			CheckFeatureAvailability(node, MessageID.IDS_FeatureRefConditional, diagnostics);
		}
		BoundExpression condition = BindBooleanExpression(node.Condition, diagnostics);
		BindValueKind bindValueKind = BindValueKind.RValue;
		if (flag)
		{
			bindValueKind |= BindValueKind.RefersToLocation;
		}
		BoundExpression boundExpression = BindValue(expressionSyntax, diagnostics, bindValueKind);
		BoundExpression boundExpression2 = BindValue(expressionSyntax2, diagnostics, bindValueKind);
		TypeSymbol type = boundExpression.Type;
		TypeSymbol type2 = boundExpression2.Type;
		bool flag2 = false;
		TypeSymbol type3;
		if (type == type2)
		{
			if ((object)type == null)
			{
				diagnostics.Add(ErrorCode.ERR_InvalidQM, node.Location, boundExpression.Display, boundExpression2.Display);
				type3 = CreateErrorType();
				flag2 = true;
			}
			else
			{
				type3 = type;
			}
		}
		else
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			TypeSymbol typeSymbol = BestTypeInferrer.InferBestTypeForConditionalOperator(boundExpression, boundExpression2, Conversions, out var hadMultipleCandidates, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			if ((object)typeSymbol == null)
			{
				if (hadMultipleCandidates)
				{
					diagnostics.Add(ErrorCode.ERR_AmbigQM, node.Location, boundExpression.Display, boundExpression2.Display);
				}
				else
				{
					object obj = boundExpression.Display;
					object obj2 = boundExpression2.Display;
					Symbol symbol = obj as Symbol;
					Symbol symbol2 = obj2 as Symbol;
					if ((object)symbol != null && (object)symbol2 != null)
					{
						SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(Compilation, symbol, symbol2);
						obj = symbolDistinguisher.First;
						obj2 = symbolDistinguisher.Second;
					}
					diagnostics.Add(ErrorCode.ERR_InvalidQM, node.Location, obj, obj2);
				}
				type3 = CreateErrorType();
				flag2 = true;
			}
			else if (typeSymbol.IsErrorType())
			{
				type3 = typeSymbol;
				flag2 = true;
			}
			else if (flag)
			{
				if (!ConversionsBase.HasIdentityConversion(type, type2))
				{
					diagnostics.Add(ErrorCode.ERR_RefConditionalDifferentTypes, boundExpression2.Syntax.Location, type);
					type3 = CreateErrorType();
					flag2 = true;
				}
				else
				{
					type3 = typeSymbol;
				}
			}
			else
			{
				boundExpression = GenerateConversionForAssignment(typeSymbol, boundExpression, diagnostics);
				boundExpression2 = GenerateConversionForAssignment(typeSymbol, boundExpression2, diagnostics);
				if (boundExpression.HasAnyErrors || boundExpression2.HasAnyErrors)
				{
					type3 = CreateErrorType();
					flag2 = true;
				}
				else
				{
					type3 = typeSymbol;
				}
			}
		}
		if (!flag2 & flag)
		{
			uint localScopeDepth = LocalScopeDepth;
			uint valEscape = GetValEscape(boundExpression, localScopeDepth);
			uint valEscape2 = GetValEscape(boundExpression2, localScopeDepth);
			if (valEscape != valEscape2)
			{
				if (valEscape < valEscape2)
				{
					CheckValEscape(boundExpression2.Syntax, boundExpression2, localScopeDepth, valEscape, checkingReceiver: false, diagnostics);
				}
				else
				{
					CheckValEscape(boundExpression.Syntax, boundExpression, localScopeDepth, valEscape2, checkingReceiver: false, diagnostics);
				}
				diagnostics.Add(ErrorCode.ERR_MismatchedRefEscapeInTernary, node.Location);
				flag2 = true;
			}
		}
		ConstantValue constantValue = null;
		if (!flag2)
		{
			constantValue = FoldConditionalOperator(condition, boundExpression, boundExpression2);
			flag2 = constantValue != null && constantValue.IsBad;
		}
		return new BoundConditionalOperator(node, flag, condition, boundExpression, boundExpression2, constantValue, type3, flag2);
	}

	private static ConstantValue FoldConditionalOperator(BoundExpression condition, BoundExpression trueExpr, BoundExpression falseExpr)
	{
		ConstantValue constantValue = trueExpr.ConstantValue;
		if (constantValue == null || constantValue.IsBad)
		{
			return constantValue;
		}
		ConstantValue constantValue2 = falseExpr.ConstantValue;
		if (constantValue2 == null || constantValue2.IsBad)
		{
			return constantValue2;
		}
		ConstantValue constantValue3 = condition.ConstantValue;
		if (constantValue3 == null || constantValue3.IsBad)
		{
			return constantValue3;
		}
		if (constantValue3 == ConstantValue.True)
		{
			return constantValue;
		}
		if (constantValue3 == ConstantValue.False)
		{
			return constantValue2;
		}
		return ConstantValue.Bad;
	}

	private BoundExpression BindIsPatternExpression(IsPatternExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression operand = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		bool flag = IsOperandErrors(node, ref operand, diagnostics);
		TypeSymbol typeSymbol = operand.Type;
		if ((object)typeSymbol == null || typeSymbol.SpecialType == SpecialType.System_Void)
		{
			typeSymbol = CreateErrorType();
			if (!flag)
			{
				diagnostics.Add(ErrorCode.ERR_BadIsPatternExpression, node.Expression.Location, operand.Display);
				flag = true;
			}
		}
		BoundPattern boundPattern = BindPattern(operand, node.Pattern, typeSymbol, flag, diagnostics);
		if (!flag && boundPattern is BoundDeclarationPattern { IsVar: false } boundDeclarationPattern && operand.ConstantValue == ConstantValue.Null)
		{
			diagnostics.Add(ErrorCode.WRN_IsAlwaysFalse, node.Location, boundDeclarationPattern.DeclaredType.Type);
		}
		return new BoundIsPatternExpression(node, operand, boundPattern, GetSpecialType(SpecialType.System_Boolean, diagnostics, node), flag);
	}

	internal BoundPattern BindPattern(BoundExpression sourceExpression, PatternSyntax node, TypeSymbol operandType, bool hasErrors, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.DeclarationPattern:
			return BindDeclarationPattern(sourceExpression, (DeclarationPatternSyntax)node, operandType, hasErrors, diagnostics);
		case SyntaxKind.ConstantPattern:
		{
			ConstantPatternSyntax constantPatternSyntax = (ConstantPatternSyntax)node;
			bool wasExpression;
			return BindConstantPattern(constantPatternSyntax, operandType, constantPatternSyntax.Expression, hasErrors, diagnostics, out wasExpression);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind());
		}
	}

	internal BoundConstantPattern BindConstantPattern(CSharpSyntaxNode node, TypeSymbol operandType, ExpressionSyntax patternExpression, bool hasErrors, DiagnosticBag diagnostics, out bool wasExpression)
	{
		SyntaxNode syntaxNode = patternExpression.SkipParens();
		if (syntaxNode.Kind() == SyntaxKind.DefaultLiteralExpression)
		{
			diagnostics.Add(ErrorCode.ERR_DefaultInPattern, syntaxNode.Location);
			hasErrors = true;
		}
		return BindConstantAsPattern(node, operandType, patternExpression, hasErrors, diagnostics, out wasExpression);
	}

	internal BoundConstantPattern BindConstantAsPattern(CSharpSyntaxNode node, TypeSymbol operandType, ExpressionSyntax patternExpression, bool hasErrors, DiagnosticBag diagnostics, out bool wasExpression)
	{
		BoundExpression boundExpression = BindValue(patternExpression, diagnostics, BindValueKind.RValue);
		ConstantValue constantValue = null;
		BoundExpression boundExpression2 = ConvertPatternExpression(operandType, patternExpression, boundExpression, ref constantValue, diagnostics);
		TypeSymbol type = boundExpression.Type;
		wasExpression = (object)type == null || !type.IsErrorType();
		if (!boundExpression2.HasErrors && constantValue == null)
		{
			diagnostics.Add(ErrorCode.ERR_ConstantExpected, patternExpression.Location);
			hasErrors = true;
		}
		return new BoundConstantPattern(node, boundExpression2, constantValue, hasErrors);
	}

	internal BoundExpression ConvertPatternExpression(TypeSymbol inputType, CSharpSyntaxNode node, BoundExpression expression, ref ConstantValue constantValue, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = GenerateConversionForAssignment(inputType, expression, diagnostics);
		if (boundExpression.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)boundExpression;
			BoundExpression operand = boundConversion.Operand;
			if (inputType.IsNullableType() && (boundExpression.ConstantValue == null || !boundExpression.ConstantValue.IsNull))
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				boundExpression = CreateConversion(operand, inputType.GetNullableUnderlyingType(), instance);
				instance.Free();
			}
			else if ((boundConversion.ConversionKind == ConversionKind.Boxing || boundConversion.ConversionKind == ConversionKind.ImplicitReference) && operand.ConstantValue != null && boundExpression.ConstantValue == null)
			{
				boundExpression = operand;
			}
			else if (boundConversion.ConversionKind == ConversionKind.NoConversion)
			{
				TypeSymbol type = boundExpression.Type;
				if ((object)type != null && type.IsErrorType())
				{
					boundExpression = operand;
				}
			}
		}
		constantValue = boundExpression.ConstantValue;
		return boundExpression;
	}

	private bool CheckValidPatternType(CSharpSyntaxNode typeSyntax, TypeSymbol operandType, TypeSymbol patternType, bool patternTypeWasInSource, bool isVar, DiagnosticBag diagnostics)
	{
		if (operandType.IsErrorType() || patternType.IsErrorType())
		{
			return false;
		}
		if ((patternType.IsNullableType() && !isVar) & patternTypeWasInSource)
		{
			Error(diagnostics, ErrorCode.ERR_PatternNullableType, typeSyntax, patternType, patternType.GetNullableUnderlyingType());
			return true;
		}
		if (patternType.IsStatic)
		{
			Error(diagnostics, ErrorCode.ERR_VarDeclIsStaticClass, typeSyntax, patternType);
			return true;
		}
		if (!isVar)
		{
			if (patternType.IsDynamic())
			{
				Error(diagnostics, ErrorCode.ERR_PatternDynamicType, typeSyntax);
				return true;
			}
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			bool? flag = ExpressionOfTypeMatchesPatternType(Conversions, operandType, patternType, ref useSiteDiagnostics, out var conversion, null, operandCouldBeNull: true);
			diagnostics.Add(typeSyntax, useSiteDiagnostics);
			if (flag == false)
			{
				Error(diagnostics, ErrorCode.ERR_PatternWrongType, typeSyntax, operandType, patternType);
				return true;
			}
			if (!conversion.Exists && (operandType.ContainsTypeParameter() || patternType.ContainsTypeParameter()))
			{
				LanguageVersion languageVersion = MessageID.IDS_FeatureGenericPatternMatching.RequiredVersion();
				if (languageVersion > Compilation.LanguageVersion)
				{
					Error(diagnostics, ErrorCode.ERR_PatternWrongGenericTypeInVersion, typeSyntax, operandType, patternType, Compilation.LanguageVersion.ToDisplayString(), new CSharpRequiredLanguageVersion(languageVersion));
					return true;
				}
			}
		}
		return false;
	}

	internal static bool? ExpressionOfTypeMatchesPatternType(Conversions conversions, TypeSymbol expressionType, TypeSymbol patternType, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out Conversion conversion, ConstantValue operandConstantValue = null, bool operandCouldBeNull = false)
	{
		if (expressionType.IsDynamic())
		{
			expressionType = conversions.CorLibrary.GetSpecialType(SpecialType.System_Object);
		}
		conversion = conversions.ClassifyBuiltInConversion(expressionType, patternType, ref useSiteDiagnostics);
		ConstantValue isOperatorConstantResult = GetIsOperatorConstantResult(expressionType, patternType, conversion.Kind, operandConstantValue, operandCouldBeNull);
		if (!(isOperatorConstantResult == null))
		{
			int value;
			if (!(isOperatorConstantResult == ConstantValue.True))
			{
				if (!(isOperatorConstantResult == ConstantValue.False))
				{
					throw ExceptionUtilities.UnexpectedValue(isOperatorConstantResult);
				}
				value = 0;
			}
			else
			{
				value = 1;
			}
			return (byte)value != 0;
		}
		return null;
	}

	private BoundPattern BindDeclarationPattern(BoundExpression sourceExpression, DeclarationPatternSyntax node, TypeSymbol operandType, bool hasErrors, DiagnosticBag diagnostics)
	{
		TypeSyntax type = node.Type;
		TypeSymbol typeSymbol = BindTypeOrVarKeyword(type, diagnostics, out var isVar, out var alias);
		if (isVar)
		{
			typeSymbol = operandType;
		}
		if ((object)typeSymbol == null)
		{
			typeSymbol = CreateErrorType("var");
		}
		BoundTypeExpression boundTypeExpression = new BoundTypeExpression(type, alias, isVar, typeSymbol);
		hasErrors = IsOperatorErrors(node, operandType, boundTypeExpression, diagnostics) || (hasErrors | CheckValidPatternType(type, operandType, typeSymbol, patternTypeWasInSource: true, isVar, diagnostics));
		switch (node.Designation.Kind())
		{
		case SyntaxKind.DiscardDesignation:
			return new BoundDeclarationPattern(node, null, boundTypeExpression, isVar, hasErrors);
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Designation.Kind());
		case SyntaxKind.SingleVariableDesignation:
		{
			SingleVariableDesignationSyntax singleVariableDesignationSyntax = (SingleVariableDesignationSyntax)node.Designation;
			SyntaxToken identifier = singleVariableDesignationSyntax.Identifier;
			SourceLocalSymbol sourceLocalSymbol = LookupLocal(identifier);
			if ((object)sourceLocalSymbol != null)
			{
				if ((InConstructorInitializer || InFieldInitializer) && ContainingMemberOrLambda.ContainingSymbol.Kind == SymbolKind.NamedType)
				{
					CheckFeatureAvailability(node, MessageID.IDS_FeatureExpressionVariablesInQueriesAndInitializers, diagnostics);
				}
				sourceLocalSymbol.SetType(typeSymbol);
				sourceLocalSymbol.SetValEscape(GetValEscape(sourceExpression, LocalScopeDepth));
				hasErrors |= sourceLocalSymbol.ScopeBinder.ValidateDeclarationNameConflictsInScope(sourceLocalSymbol, diagnostics);
				if (!hasErrors)
				{
					hasErrors = CheckRestrictedTypeInAsync(ContainingMemberOrLambda, typeSymbol, diagnostics, type);
				}
				return new BoundDeclarationPattern(node, sourceLocalSymbol, boundTypeExpression, isVar, hasErrors);
			}
			GlobalExpressionVariable globalExpressionVariable = LookupDeclaredField(singleVariableDesignationSyntax);
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			globalExpressionVariable.SetType(typeSymbol, instance);
			instance.Free();
			BoundExpression receiver = SynthesizeReceiver(node, globalExpressionVariable, diagnostics);
			BoundFieldAccess variableAccess = new BoundFieldAccess(node, receiver, globalExpressionVariable, null, hasErrors);
			return new BoundDeclarationPattern(node, globalExpressionVariable, variableAccess, boundTypeExpression, isVar, hasErrors);
		}
		}
	}

	internal BoundExpression BindQuery(QueryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		FromClauseSyntax fromClause = node.FromClause;
		BoundExpression boundExpression = BindLeftOfPotentialColorColorMemberAccess(fromClause.Expression, diagnostics);
		if (boundExpression.HasDynamicType())
		{
			diagnostics.Add(ErrorCode.ERR_BadDynamicQuery, fromClause.Expression.Location);
			boundExpression = BadExpression(fromClause.Expression, boundExpression);
		}
		QueryTranslationState queryTranslationState = new QueryTranslationState();
		queryTranslationState.fromExpression = MakeMemberAccessValue(boundExpression, diagnostics);
		RangeVariableSymbol definedSymbol = (queryTranslationState.rangeVariable = queryTranslationState.AddRangeVariable(this, fromClause.Identifier, diagnostics));
		for (int num = node.Body.Clauses.Count - 1; num >= 0; num--)
		{
			queryTranslationState.clauses.Push(node.Body.Clauses[num]);
		}
		queryTranslationState.selectOrGroup = node.Body.SelectOrGroup;
		BoundExpression castInvocation = null;
		if (fromClause.Type != null)
		{
			TypeSymbol typeArg = BindTypeArgument(fromClause.Type, diagnostics);
			castInvocation = (queryTranslationState.fromExpression = MakeQueryInvocation(fromClause, queryTranslationState.fromExpression, "Cast", fromClause.Type, typeArg, diagnostics));
		}
		queryTranslationState.fromExpression = MakeQueryClause(fromClause, queryTranslationState.fromExpression, definedSymbol, null, castInvocation);
		BoundExpression boundExpression2 = BindQueryInternal1(queryTranslationState, diagnostics);
		for (QueryContinuationSyntax continuation = node.Body.Continuation; continuation != null; continuation = continuation.Body.Continuation)
		{
			queryTranslationState.Clear();
			queryTranslationState.fromExpression = boundExpression2;
			definedSymbol = (queryTranslationState.rangeVariable = queryTranslationState.AddRangeVariable(this, continuation.Identifier, diagnostics));
			SyntaxList<QueryClauseSyntax> clauses = continuation.Body.Clauses;
			for (int num2 = clauses.Count - 1; num2 >= 0; num2--)
			{
				queryTranslationState.clauses.Push(clauses[num2]);
			}
			queryTranslationState.selectOrGroup = continuation.Body.SelectOrGroup;
			boundExpression2 = BindQueryInternal1(queryTranslationState, diagnostics);
			boundExpression2 = MakeQueryClause(continuation.Body, boundExpression2, definedSymbol);
			boundExpression2 = MakeQueryClause(continuation, boundExpression2, definedSymbol);
		}
		queryTranslationState.Free();
		return MakeQueryClause(node, boundExpression2);
	}

	private BoundExpression BindQueryInternal1(QueryTranslationState state, DiagnosticBag diagnostics)
	{
		if (!IsDegenerateQuery(state))
		{
			return BindQueryInternal2(state, diagnostics);
		}
		return FinalTranslation(state, diagnostics);
	}

	private static bool IsDegenerateQuery(QueryTranslationState state)
	{
		if (!state.clauses.IsEmpty())
		{
			return false;
		}
		if (!(state.selectOrGroup is SelectClauseSyntax selectClauseSyntax))
		{
			return false;
		}
		if (selectClauseSyntax.Expression is IdentifierNameSyntax identifierNameSyntax)
		{
			return state.rangeVariable.Name == identifierNameSyntax.Identifier.ValueText;
		}
		return false;
	}

	private BoundExpression BindQueryInternal2(QueryTranslationState state, DiagnosticBag diagnostics)
	{
		while (!state.clauses.IsEmpty())
		{
			ReduceQuery(state, diagnostics);
		}
		if (state.selectOrGroup == null)
		{
			return state.fromExpression;
		}
		if (IsDegenerateQuery(state))
		{
			BoundExpression fromExpression = state.fromExpression;
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression boundExpression = FinalTranslation(state, instance);
			instance.Free();
			if (boundExpression.HasAnyErrors && !fromExpression.HasAnyErrors)
			{
				boundExpression = null;
			}
			return MakeQueryClause(state.selectOrGroup, fromExpression, null, null, null, boundExpression);
		}
		return FinalTranslation(state, diagnostics);
	}

	private BoundExpression FinalTranslation(QueryTranslationState state, DiagnosticBag diagnostics)
	{
		switch (state.selectOrGroup.Kind())
		{
		case SyntaxKind.SelectClause:
		{
			SelectClauseSyntax selectClauseSyntax = (SelectClauseSyntax)state.selectOrGroup;
			RangeVariableSymbol rangeVariable2 = state.rangeVariable;
			BoundExpression fromExpression2 = state.fromExpression;
			ExpressionSyntax expression = selectClauseSyntax.Expression;
			UnboundLambda arg = MakeQueryUnboundLambda(state.RangeVariableMap(), rangeVariable2, expression);
			BoundCall boundCall = MakeQueryInvocation(state.selectOrGroup, fromExpression2, "Select", arg, diagnostics);
			return MakeQueryClause(selectClauseSyntax, boundCall, null, boundCall);
		}
		case SyntaxKind.GroupClause:
		{
			GroupClauseSyntax groupClauseSyntax = (GroupClauseSyntax)state.selectOrGroup;
			RangeVariableSymbol rangeVariable = state.rangeVariable;
			BoundExpression fromExpression = state.fromExpression;
			ExpressionSyntax groupExpression = groupClauseSyntax.GroupExpression;
			ExpressionSyntax byExpression = groupClauseSyntax.ByExpression;
			IdentifierNameSyntax identifierNameSyntax = groupExpression as IdentifierNameSyntax;
			UnboundLambda unboundLambda = MakeQueryUnboundLambda(state.RangeVariableMap(), rangeVariable, byExpression);
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression item = MakeQueryUnboundLambda(state.RangeVariableMap(), rangeVariable, groupExpression);
			BoundCall result = MakeQueryInvocation(state.selectOrGroup, fromExpression, "GroupBy", ImmutableArray.Create(unboundLambda, item), instance);
			result = ReverseLastTwoParameterOrder(result);
			BoundExpression boundExpression = null;
			if (identifierNameSyntax != null && identifierNameSyntax.Identifier.ValueText == rangeVariable.Name)
			{
				boundExpression = result;
				result = MakeQueryInvocation(state.selectOrGroup, fromExpression, "GroupBy", unboundLambda, diagnostics);
				if (boundExpression.HasAnyErrors && !result.HasAnyErrors)
				{
					boundExpression = null;
				}
			}
			else
			{
				diagnostics.AddRange(instance);
			}
			instance.Free();
			return MakeQueryClause(groupClauseSyntax, result, null, result, null, boundExpression);
		}
		default:
			return new BoundBadExpression(state.selectOrGroup, LookupResultKind.OverloadResolutionFailure, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(state.fromExpression), state.fromExpression.Type);
		}
	}

	private static BoundCall ReverseLastTwoParameterOrder(BoundCall result)
	{
		int length = result.Arguments.Length;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		instance.AddRange(result.Arguments);
		BoundExpression value = instance[length - 1];
		instance[length - 1] = instance[length - 2];
		instance[length - 2] = value;
		ArrayBuilder<int> instance2 = ArrayBuilder<int>.GetInstance();
		instance2.AddRange(Enumerable.Range(0, length));
		instance2[length - 1] = length - 2;
		instance2[length - 2] = length - 1;
		return result.Update(result.ReceiverOpt, result.Method, instance.ToImmutableAndFree(), default, default, result.IsDelegateCall, result.Expanded, result.InvokedAsExtensionMethod, instance2.ToImmutableAndFree(), result.ResultKind, result.BinderOpt, result.Type);
	}

	private void ReduceQuery(QueryTranslationState state, DiagnosticBag diagnostics)
	{
		QueryClauseSyntax queryClauseSyntax = state.clauses.Pop();
		switch (queryClauseSyntax.Kind())
		{
		case SyntaxKind.WhereClause:
			ReduceWhere((WhereClauseSyntax)queryClauseSyntax, state, diagnostics);
			break;
		case SyntaxKind.JoinClause:
			ReduceJoin((JoinClauseSyntax)queryClauseSyntax, state, diagnostics);
			break;
		case SyntaxKind.OrderByClause:
			ReduceOrderBy((OrderByClauseSyntax)queryClauseSyntax, state, diagnostics);
			break;
		case SyntaxKind.FromClause:
			ReduceFrom((FromClauseSyntax)queryClauseSyntax, state, diagnostics);
			break;
		case SyntaxKind.LetClause:
			ReduceLet((LetClauseSyntax)queryClauseSyntax, state, diagnostics);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(queryClauseSyntax.Kind());
		}
	}

	private void ReduceWhere(WhereClauseSyntax where, QueryTranslationState state, DiagnosticBag diagnostics)
	{
		UnboundLambda arg = MakeQueryUnboundLambda(state.RangeVariableMap(), state.rangeVariable, where.Condition);
		BoundCall boundCall = MakeQueryInvocation(where, state.fromExpression, "Where", arg, diagnostics);
		state.fromExpression = MakeQueryClause(where, boundCall, null, boundCall);
	}

	private void ReduceJoin(JoinClauseSyntax join, QueryTranslationState state, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(join.InExpression, diagnostics, BindValueKind.RValue);
		if (boundExpression.HasDynamicType())
		{
			diagnostics.Add(ErrorCode.ERR_BadDynamicQuery, join.InExpression.Location);
			boundExpression = BadExpression(join.InExpression, boundExpression);
		}
		BoundExpression boundExpression2 = null;
		if (join.Type != null)
		{
			TypeSymbol typeArg = BindTypeArgument(join.Type, diagnostics);
			boundExpression2 = MakeQueryInvocation(join, boundExpression, "Cast", join.Type, typeArg, diagnostics);
			boundExpression = boundExpression2;
		}
		UnboundLambda item = MakeQueryUnboundLambda(state.RangeVariableMap(), state.rangeVariable, join.LeftExpression);
		RangeVariableSymbol rangeVariable = state.rangeVariable;
		RangeVariableSymbol rangeVariableSymbol = state.AddRangeVariable(this, join.Identifier, diagnostics);
		UnboundLambda item2 = MakeQueryUnboundLambda(QueryTranslationState.RangeVariableMap(rangeVariableSymbol), rangeVariableSymbol, join.RightExpression);
		if (state.clauses.IsEmpty() && state.selectOrGroup.Kind() == SyntaxKind.SelectClause)
		{
			SelectClauseSyntax selectClauseSyntax = (SelectClauseSyntax)state.selectOrGroup;
			BoundCall boundCall;
			if (join.Into == null)
			{
				UnboundLambda item3 = MakeQueryUnboundLambda(state.RangeVariableMap(), ImmutableArray.Create(rangeVariable, rangeVariableSymbol), selectClauseSyntax.Expression);
				boundCall = MakeQueryInvocation(join, state.fromExpression, "Join", ImmutableArray.Create(boundExpression, item, item2, item3), diagnostics);
			}
			else
			{
				state.allRangeVariables[rangeVariableSymbol].Free();
				state.allRangeVariables.Remove(rangeVariableSymbol);
				RangeVariableSymbol rangeVariableSymbol2 = state.AddRangeVariable(this, join.Into.Identifier, diagnostics);
				UnboundLambda item4 = MakeQueryUnboundLambda(state.RangeVariableMap(), ImmutableArray.Create(rangeVariable, rangeVariableSymbol2), selectClauseSyntax.Expression);
				boundCall = MakeQueryInvocation(join, state.fromExpression, "GroupJoin", ImmutableArray.Create(boundExpression, item, item2, item4), diagnostics);
				ImmutableArray<BoundExpression> arguments = boundCall.Arguments;
				arguments = arguments.SetItem(arguments.Length - 1, MakeQueryClause(join.Into, arguments[arguments.Length - 1], rangeVariableSymbol2));
				boundCall = boundCall.Update(boundCall.ReceiverOpt, boundCall.Method, arguments);
			}
			state.Clear();
			state.fromExpression = MakeQueryClause(join, boundCall, rangeVariableSymbol, boundCall, boundExpression2);
			state.fromExpression = MakeQueryClause(selectClauseSyntax, state.fromExpression);
		}
		else
		{
			BoundCall boundCall2;
			if (join.Into == null)
			{
				UnboundLambda item5 = MakePairLambda(join, state, rangeVariable, rangeVariableSymbol);
				boundCall2 = MakeQueryInvocation(join, state.fromExpression, "Join", ImmutableArray.Create(boundExpression, item, item2, item5), diagnostics);
			}
			else
			{
				state.allRangeVariables[rangeVariableSymbol].Free();
				state.allRangeVariables.Remove(rangeVariableSymbol);
				RangeVariableSymbol rangeVariableSymbol3 = state.AddRangeVariable(this, join.Into.Identifier, diagnostics);
				UnboundLambda item6 = MakePairLambda(join, state, rangeVariable, rangeVariableSymbol3);
				boundCall2 = MakeQueryInvocation(join, state.fromExpression, "GroupJoin", ImmutableArray.Create(boundExpression, item, item2, item6), diagnostics);
				ImmutableArray<BoundExpression> arguments2 = boundCall2.Arguments;
				arguments2 = arguments2.SetItem(arguments2.Length - 1, MakeQueryClause(join.Into, arguments2[arguments2.Length - 1], rangeVariableSymbol3));
				boundCall2 = boundCall2.Update(boundCall2.ReceiverOpt, boundCall2.Method, arguments2);
			}
			state.fromExpression = MakeQueryClause(join, boundCall2, rangeVariableSymbol, boundCall2, boundExpression2);
		}
	}

	private void ReduceOrderBy(OrderByClauseSyntax orderby, QueryTranslationState state, DiagnosticBag diagnostics)
	{
		bool flag = true;
		foreach (OrderingSyntax ordering in orderby.Orderings)
		{
			string methodName = (flag ? "OrderBy" : "ThenBy") + (ordering.IsKind(SyntaxKind.DescendingOrdering) ? "Descending" : "");
			UnboundLambda arg = MakeQueryUnboundLambda(state.RangeVariableMap(), state.rangeVariable, ordering.Expression);
			BoundCall boundCall = MakeQueryInvocation(ordering, state.fromExpression, methodName, arg, diagnostics);
			state.fromExpression = MakeQueryClause(ordering, boundCall, null, boundCall);
			flag = false;
		}
		state.fromExpression = MakeQueryClause(orderby, state.fromExpression);
	}

	private void ReduceFrom(FromClauseSyntax from, QueryTranslationState state, DiagnosticBag diagnostics)
	{
		RangeVariableSymbol rangeVariable = state.rangeVariable;
		BoundExpression item = ((from.Type != null) ? MakeQueryUnboundLambdaWithCast(state.RangeVariableMap(), rangeVariable, from.Expression, from.Type, BindTypeArgument(from.Type, diagnostics)) : MakeQueryUnboundLambda(state.RangeVariableMap(), rangeVariable, from.Expression));
		RangeVariableSymbol rangeVariableSymbol = state.AddRangeVariable(this, from.Identifier, diagnostics);
		if (state.clauses.IsEmpty() && state.selectOrGroup.IsKind(SyntaxKind.SelectClause))
		{
			SelectClauseSyntax selectClauseSyntax = (SelectClauseSyntax)state.selectOrGroup;
			UnboundLambda item2 = MakeQueryUnboundLambda(state.RangeVariableMap(), ImmutableArray.Create(rangeVariable, rangeVariableSymbol), selectClauseSyntax.Expression);
			BoundCall boundCall = MakeQueryInvocation(from, state.fromExpression, "SelectMany", ImmutableArray.Create(item, item2), diagnostics);
			BoundExpression castInvocation = ((from.Type != null) ? ExtractCastInvocation(boundCall) : null);
			ImmutableArray<BoundExpression> arguments = boundCall.Arguments;
			boundCall = boundCall.Update(boundCall.ReceiverOpt, boundCall.Method, arguments.SetItem(arguments.Length - 2, MakeQueryClause(from, arguments[arguments.Length - 2], rangeVariableSymbol, boundCall, castInvocation)));
			state.Clear();
			state.fromExpression = MakeQueryClause(from, boundCall, rangeVariableSymbol, boundCall);
			state.fromExpression = MakeQueryClause(selectClauseSyntax, state.fromExpression);
		}
		else
		{
			UnboundLambda item3 = MakePairLambda(from, state, rangeVariable, rangeVariableSymbol);
			BoundCall boundCall2 = MakeQueryInvocation(from, state.fromExpression, "SelectMany", ImmutableArray.Create(item, item3), diagnostics);
			BoundExpression castInvocation2 = ((from.Type != null) ? ExtractCastInvocation(boundCall2) : null);
			state.fromExpression = MakeQueryClause(from, boundCall2, rangeVariableSymbol, boundCall2, castInvocation2);
		}
	}

	private static BoundExpression ExtractCastInvocation(BoundCall invocation)
	{
		int index = (invocation.InvokedAsExtensionMethod ? 1 : 0);
		BoundLambda boundLambda = ((invocation.Arguments[index] is BoundConversion boundConversion) ? (boundConversion.Operand as BoundLambda) : null);
		BoundReturnStatement boundReturnStatement = ((boundLambda != null) ? (boundLambda.Body.Statements[0] as BoundReturnStatement) : null);
		if (boundReturnStatement == null)
		{
			return null;
		}
		return boundReturnStatement.ExpressionOpt as BoundCall;
	}

	private UnboundLambda MakePairLambda(CSharpSyntaxNode node, QueryTranslationState state, RangeVariableSymbol x1, RangeVariableSymbol x2)
	{
		LambdaBodyFactory bodyFactory = (LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag d) =>
		{
			BoundParameter field1Value = new BoundParameter(node, lambdaSymbol.Parameters[0])
			{
				WasCompilerGenerated = true
			};
			BoundParameter field2Value = new BoundParameter(node, lambdaSymbol.Parameters[1])
			{
				WasCompilerGenerated = true
			};
			BoundExpression expression = MakePair(node, x1.Name, field1Value, x2.Name, field2Value, state, d);
			return lambdaBodyBinder.CreateBlockFromExpression(node, ImmutableArray<LocalSymbol>.Empty, RefKind.None, expression, null, d);
		};
		UnboundLambda result = MakeQueryUnboundLambda(state.RangeVariableMap(), ImmutableArray.Create(x1, x2), node, bodyFactory);
		state.rangeVariable = state.TransparentRangeVariable(this);
		state.AddTransparentIdentifier(x1.Name);
		ArrayBuilder<string> arrayBuilder = state.allRangeVariables[x2];
		arrayBuilder[arrayBuilder.Count - 1] = x2.Name;
		return result;
	}

	private void ReduceLet(LetClauseSyntax let, QueryTranslationState state, DiagnosticBag diagnostics)
	{
		RangeVariableSymbol x = state.rangeVariable;
		LambdaBodyFactory bodyFactory = (LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag d) =>
		{
			BoundParameter field1Value = new BoundParameter(let, lambdaSymbol.Parameters[0])
			{
				WasCompilerGenerated = true
			};
			lambdaBodyBinder = lambdaBodyBinder.GetBinder(let.Expression);
			BoundExpression boundExpression = lambdaBodyBinder.BindValue(let.Expression, d, BindValueKind.RValue);
			SourceLocation location = new SourceLocation(let.SyntaxTree, new TextSpan(let.Identifier.SpanStart, let.Expression.Span.End - let.Identifier.SpanStart));
			if (!boundExpression.HasAnyErrors && !boundExpression.HasExpressionType())
			{
				Error(d, ErrorCode.ERR_QueryRangeVariableAssignedBadValue, location, boundExpression.Display);
				boundExpression = new BoundBadExpression(boundExpression.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), CreateErrorType());
			}
			else if (!boundExpression.HasAnyErrors && boundExpression.Type.SpecialType == SpecialType.System_Void)
			{
				Error(d, ErrorCode.ERR_QueryRangeVariableAssignedBadValue, location, boundExpression.Type);
				boundExpression = new BoundBadExpression(boundExpression.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), boundExpression.Type);
			}
			BoundExpression result = MakePair(let, x.Name, field1Value, let.Identifier.ValueText, boundExpression, state, d);
			return lambdaBodyBinder.CreateLambdaBlockForQueryClause(let.Expression, result, d);
		};
		UnboundLambda arg = MakeQueryUnboundLambda(state.RangeVariableMap(), ImmutableArray.Create(x), let.Expression, bodyFactory);
		state.rangeVariable = state.TransparentRangeVariable(this);
		state.AddTransparentIdentifier(x.Name);
		RangeVariableSymbol rangeVariableSymbol = state.AddRangeVariable(this, let.Identifier, diagnostics);
		state.allRangeVariables[rangeVariableSymbol].Add(let.Identifier.ValueText);
		BoundCall boundCall = MakeQueryInvocation(let, state.fromExpression, "Select", arg, diagnostics);
		state.fromExpression = MakeQueryClause(let, boundCall, rangeVariableSymbol, boundCall);
	}

	private BoundBlock CreateLambdaBlockForQueryClause(ExpressionSyntax expression, BoundExpression result, DiagnosticBag diagnostics)
	{
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetDeclaredLocalsForScope(expression);
		if (declaredLocalsForScope.Any())
		{
			CheckFeatureAvailability(expression, MessageID.IDS_FeatureExpressionVariablesInQueriesAndInitializers, diagnostics, declaredLocalsForScope[0].Locations[0]);
		}
		return CreateBlockFromExpression(expression, declaredLocalsForScope, RefKind.None, result, expression, diagnostics);
	}

	private BoundQueryClause MakeQueryClause(CSharpSyntaxNode syntax, BoundExpression expression, RangeVariableSymbol definedSymbol = null, BoundExpression queryInvocation = null, BoundExpression castInvocation = null, BoundExpression unoptimizedForm = null)
	{
		if (unoptimizedForm != null && unoptimizedForm.HasAnyErrors && !expression.HasAnyErrors)
		{
			unoptimizedForm = null;
		}
		return new BoundQueryClause(syntax, expression, definedSymbol, queryInvocation, castInvocation, this, unoptimizedForm, TypeOrError(expression));
	}

	private BoundExpression MakePair(CSharpSyntaxNode node, string field1Name, BoundExpression field1Value, string field2Name, BoundExpression field2Value, QueryTranslationState state, DiagnosticBag diagnostics)
	{
		if (field1Name == field2Name)
		{
			field2Name = state.TransparentRangeVariableName();
			field2Value = new BoundBadExpression(field2Value.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(field2Value), field2Value.Type, hasErrors: true);
		}
		AnonymousTypeDescriptor typeDescr = new AnonymousTypeDescriptor(ImmutableArray.Create(new AnonymousTypeField(field1Name, field1Value.Syntax.Location, TypeOrError(field1Value)), new AnonymousTypeField(field2Name, field2Value.Syntax.Location, TypeOrError(field2Value))), node.Location);
		NamedTypeSymbol toCreate = Compilation.AnonymousTypeManager.ConstructAnonymousTypeSymbol(typeDescr);
		return MakeConstruction(node, toCreate, ImmutableArray.Create(field1Value, field2Value), diagnostics);
	}

	private TypeSymbol TypeOrError(BoundExpression e)
	{
		return e.Type ?? CreateErrorType();
	}

	private UnboundLambda MakeQueryUnboundLambda(RangeVariableMap qvm, RangeVariableSymbol parameter, ExpressionSyntax expression)
	{
		return MakeQueryUnboundLambda(qvm, ImmutableArray.Create(parameter), expression);
	}

	private UnboundLambda MakeQueryUnboundLambda(RangeVariableMap qvm, ImmutableArray<RangeVariableSymbol> parameters, ExpressionSyntax expression)
	{
		return MakeQueryUnboundLambda(expression, new QueryUnboundLambdaState(this, qvm, parameters, (LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag diagnostics) =>
		{
			lambdaBodyBinder = lambdaBodyBinder.GetBinder(expression);
			BoundExpression result = lambdaBodyBinder.BindValue(expression, diagnostics, BindValueKind.RValue);
			return lambdaBodyBinder.CreateLambdaBlockForQueryClause(expression, result, diagnostics);
		}));
	}

	private UnboundLambda MakeQueryUnboundLambdaWithCast(RangeVariableMap qvm, RangeVariableSymbol parameter, ExpressionSyntax expression, TypeSyntax castTypeSyntax, TypeSymbol castType)
	{
		return MakeQueryUnboundLambda(expression, new QueryUnboundLambdaState(this, qvm, ImmutableArray.Create(parameter), (LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag diagnostics) =>
		{
			lambdaBodyBinder = lambdaBodyBinder.GetBinder(expression);
			BoundExpression receiver = lambdaBodyBinder.BindValue(expression, diagnostics, BindValueKind.RValue);
			receiver = lambdaBodyBinder.MakeQueryInvocation(expression, receiver, "Cast", castTypeSyntax, castType, diagnostics);
			return lambdaBodyBinder.CreateLambdaBlockForQueryClause(expression, receiver, diagnostics);
		}));
	}

	private UnboundLambda MakeQueryUnboundLambda(RangeVariableMap qvm, ImmutableArray<RangeVariableSymbol> parameters, CSharpSyntaxNode node, LambdaBodyFactory bodyFactory)
	{
		return MakeQueryUnboundLambda(node, new QueryUnboundLambdaState(this, qvm, parameters, bodyFactory));
	}

	private UnboundLambda MakeQueryUnboundLambda(CSharpSyntaxNode node, QueryUnboundLambdaState state)
	{
		UnboundLambda unboundLambda = new UnboundLambda(node, state, hasErrors: false)
		{
			WasCompilerGenerated = true
		};
		state.SetUnboundLambda(unboundLambda);
		return unboundLambda;
	}

	protected BoundCall MakeQueryInvocation(CSharpSyntaxNode node, BoundExpression receiver, string methodName, BoundExpression arg, DiagnosticBag diagnostics)
	{
		return MakeQueryInvocation(node, receiver, methodName, default, default, ImmutableArray.Create(arg), diagnostics);
	}

	protected BoundCall MakeQueryInvocation(CSharpSyntaxNode node, BoundExpression receiver, string methodName, ImmutableArray<BoundExpression> args, DiagnosticBag diagnostics)
	{
		return MakeQueryInvocation(node, receiver, methodName, default, default, args, diagnostics);
	}

	protected BoundCall MakeQueryInvocation(CSharpSyntaxNode node, BoundExpression receiver, string methodName, TypeSyntax typeArgSyntax, TypeSymbol typeArg, DiagnosticBag diagnostics)
	{
		return MakeQueryInvocation(node, receiver, methodName, new SeparatedSyntaxList<TypeSyntax>(new SyntaxNodeOrTokenList(typeArgSyntax, 0)), ImmutableArray.Create(typeArg), ImmutableArray<BoundExpression>.Empty, diagnostics);
	}

	protected BoundCall MakeQueryInvocation(CSharpSyntaxNode node, BoundExpression receiver, string methodName, SeparatedSyntaxList<TypeSyntax> typeArgsSyntax, ImmutableArray<TypeSymbol> typeArgs, ImmutableArray<BoundExpression> args, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = receiver;
		while (boundExpression.Kind == BoundKind.QueryClause)
		{
			boundExpression = ((BoundQueryClause)boundExpression).Value;
		}
		if ((object)boundExpression.Type == null)
		{
			if (!boundExpression.HasAnyErrors && !node.HasErrors)
			{
				if (boundExpression.IsLiteralNull())
				{
					diagnostics.Add(ErrorCode.ERR_NullNotValid, node.Location);
				}
				else if (boundExpression.IsLiteralDefault())
				{
					diagnostics.Add(ErrorCode.ERR_DefaultLiteralNotValid, node.Location);
				}
				else if (boundExpression.Kind == BoundKind.NamespaceExpression)
				{
					diagnostics.Add(ErrorCode.ERR_BadSKunknown, boundExpression.Syntax.Location, boundExpression.Syntax, MessageID.IDS_SK_NAMESPACE.Localize());
				}
				else if (boundExpression.Kind == BoundKind.Lambda || boundExpression.Kind == BoundKind.UnboundLambda)
				{
					diagnostics.Add(ErrorCode.ERR_QueryNoProvider, node.Location, MessageID.IDS_AnonMethod.Localize(), methodName);
				}
				else if (boundExpression.Kind == BoundKind.MethodGroup)
				{
					BoundMethodGroup node2 = (BoundMethodGroup)boundExpression;
					HashSet<DiagnosticInfo> useSiteDiagnostics = null;
					MethodGroupResolution methodGroupResolution = ResolveMethodGroup(node2, null, isMethodGroupConversion: false, ref useSiteDiagnostics);
					diagnostics.Add(node, useSiteDiagnostics);
					diagnostics.AddRange(methodGroupResolution.Diagnostics);
					if (methodGroupResolution.HasAnyErrors)
					{
						receiver = BindMemberAccessBadResult(node2);
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_QueryNoProvider, node.Location, MessageID.IDS_SK_METHOD.Localize(), methodName);
					}
					methodGroupResolution.Free();
				}
			}
			receiver = new BoundBadExpression(receiver.Syntax, LookupResultKind.NotAValue, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(receiver), CreateErrorType());
		}
		else if (receiver.Type.SpecialType == SpecialType.System_Void)
		{
			if (!receiver.HasAnyErrors && !node.HasErrors)
			{
				diagnostics.Add(ErrorCode.ERR_QueryNoProvider, node.Location, "void", methodName);
			}
			receiver = new BoundBadExpression(receiver.Syntax, LookupResultKind.NotAValue, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(receiver), CreateErrorType());
		}
		return (BoundCall)MakeInvocationExpression(node, receiver, methodName, args, diagnostics, typeArgsSyntax, typeArgs, node, allowFieldsAndProperties: true);
	}

	protected BoundExpression MakeConstruction(CSharpSyntaxNode node, NamedTypeSymbol toCreate, ImmutableArray<BoundExpression> args, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		instance.Arguments.AddRange(args);
		BoundExpression boundExpression = BindClassCreationExpression(node, toCreate.Name, node, toCreate, instance, diagnostics);
		boundExpression.WasCompilerGenerated = true;
		instance.Free();
		return boundExpression;
	}

	internal void ReportQueryLookupFailed(SyntaxNode queryClause, BoundExpression instanceArgument, string name, ImmutableArray<Symbol> symbols, DiagnosticBag diagnostics)
	{
		SyntaxNode syntaxNode = queryClause;
		QueryExpressionSyntax queryExpressionSyntax;
		while (true)
		{
			queryExpressionSyntax = syntaxNode as QueryExpressionSyntax;
			if (queryExpressionSyntax != null)
			{
				break;
			}
			syntaxNode = syntaxNode.Parent;
		}
		FromClauseSyntax fromClause = queryExpressionSyntax.FromClause;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (instanceArgument.Type.IsDynamic())
		{
			diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_BadDynamicQuery, Array.Empty<object>(), symbols), new SourceLocation(queryClause));
		}
		else if (ImplementsStandardQueryInterface(instanceArgument.Type, name, ref useSiteDiagnostics))
		{
			diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_QueryNoProviderStandard, new object[2] { instanceArgument.Type, name }, symbols), new SourceLocation((fromClause != null) ? fromClause.Expression : queryClause));
		}
		else if (fromClause != null && fromClause.Type == null && HasCastToQueryProvider(instanceArgument.Type, ref useSiteDiagnostics))
		{
			diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_QueryNoProviderCastable, new object[3]
			{
				instanceArgument.Type,
				name,
				fromClause.Identifier.ValueText
			}, symbols), new SourceLocation(fromClause.Expression));
		}
		else
		{
			diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_QueryNoProvider, new object[2] { instanceArgument.Type, name }, symbols), new SourceLocation((fromClause != null) ? fromClause.Expression : queryClause));
		}
		diagnostics.Add(queryClause, useSiteDiagnostics);
	}

	private bool ImplementsStandardQueryInterface(TypeSymbol instanceType, string name, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (instanceType.TypeKind == TypeKind.Array || (name == "Cast" && HasCastToQueryProvider(instanceType, ref useSiteDiagnostics)))
		{
			return true;
		}
		bool nonUnique = false;
		TypeSymbol originalDefinition = instanceType.OriginalDefinition;
		NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		NamedTypeSymbol wellKnownType = Compilation.GetWellKnownType(WellKnownType.System_Linq_IQueryable_T);
		bool flag = originalDefinition == specialType || HasUniqueInterface(instanceType, specialType, ref nonUnique, ref useSiteDiagnostics);
		bool flag2 = originalDefinition == wellKnownType || HasUniqueInterface(instanceType, wellKnownType, ref nonUnique, ref useSiteDiagnostics);
		if (flag != flag2)
		{
			return !nonUnique;
		}
		return false;
	}

	private static bool HasUniqueInterface(TypeSymbol instanceType, NamedTypeSymbol interfaceType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool nonUnique = false;
		return HasUniqueInterface(instanceType, interfaceType, ref nonUnique, ref useSiteDiagnostics);
	}

	private static bool HasUniqueInterface(TypeSymbol instanceType, NamedTypeSymbol interfaceType, ref bool nonUnique, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol typeSymbol = null;
		foreach (NamedTypeSymbol item in instanceType.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
		{
			if (item.OriginalDefinition == interfaceType)
			{
				if ((object)typeSymbol == null)
				{
					typeSymbol = item;
				}
				else if (typeSymbol != item)
				{
					nonUnique = true;
					return false;
				}
			}
		}
		return (object)typeSymbol != null;
	}

	private bool HasCastToQueryProvider(TypeSymbol instanceType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol originalDefinition = instanceType.OriginalDefinition;
		NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
		NamedTypeSymbol wellKnownType = Compilation.GetWellKnownType(WellKnownType.System_Linq_IQueryable);
		bool flag = originalDefinition == specialType || HasUniqueInterface(instanceType, specialType, ref useSiteDiagnostics);
		bool flag2 = originalDefinition == wellKnownType || HasUniqueInterface(instanceType, wellKnownType, ref useSiteDiagnostics);
		return flag != flag2;
	}

	private static bool IsJoinRangeVariableInLeftKey(SimpleNameSyntax node)
	{
		for (CSharpSyntaxNode parent = node.Parent; parent != null; parent = parent.Parent)
		{
			if (parent.Kind() == SyntaxKind.JoinClause)
			{
				JoinClauseSyntax joinClauseSyntax = (JoinClauseSyntax)parent;
				if (joinClauseSyntax.LeftExpression.Span.Contains(node.Span) && joinClauseSyntax.Identifier.ValueText == node.Identifier.ValueText)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsInJoinRightKey(SimpleNameSyntax node)
	{
		for (CSharpSyntaxNode parent = node.Parent; parent != null; parent = parent.Parent)
		{
			if (parent.Kind() == SyntaxKind.JoinClause && ((JoinClauseSyntax)parent).RightExpression.Span.Contains(node.Span))
			{
				return true;
			}
		}
		return false;
	}

	internal static void ReportQueryInferenceFailed(CSharpSyntaxNode queryClause, string methodName, BoundExpression receiver, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, DiagnosticBag diagnostics)
	{
		bool flag = false;
		string text;
		switch (queryClause.Kind())
		{
		case SyntaxKind.JoinClause:
			text = SyntaxFacts.GetText(SyntaxKind.JoinKeyword);
			flag = true;
			break;
		case SyntaxKind.LetClause:
			text = SyntaxFacts.GetText(SyntaxKind.LetKeyword);
			break;
		case SyntaxKind.SelectClause:
			text = SyntaxFacts.GetText(SyntaxKind.SelectKeyword);
			break;
		case SyntaxKind.WhereClause:
			text = SyntaxFacts.GetText(SyntaxKind.WhereKeyword);
			break;
		case SyntaxKind.OrderByClause:
		case SyntaxKind.AscendingOrdering:
		case SyntaxKind.DescendingOrdering:
			text = SyntaxFacts.GetText(SyntaxKind.OrderByKeyword);
			flag = true;
			break;
		case SyntaxKind.QueryContinuation:
			text = SyntaxFacts.GetText(SyntaxKind.IntoKeyword);
			break;
		case SyntaxKind.GroupClause:
			text = SyntaxFacts.GetText(SyntaxKind.GroupKeyword) + " " + SyntaxFacts.GetText(SyntaxKind.ByKeyword);
			flag = true;
			break;
		case SyntaxKind.FromClause:
			if (ReportQueryInferenceFailedSelectMany((FromClauseSyntax)queryClause, methodName, receiver, arguments, symbols, diagnostics))
			{
				return;
			}
			text = SyntaxFacts.GetText(SyntaxKind.FromKeyword);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(queryClause.Kind());
		}
		diagnostics.Add(new DiagnosticInfoWithSymbols(flag ? ErrorCode.ERR_QueryTypeInferenceFailedMulti : ErrorCode.ERR_QueryTypeInferenceFailed, new object[2] { text, methodName }, symbols), queryClause.GetFirstToken().GetLocation());
	}

	private static bool ReportQueryInferenceFailedSelectMany(FromClauseSyntax fromClause, string methodName, BoundExpression receiver, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = arguments.Argument(arguments.IsExtensionMethodInvocation ? 1 : 0);
		TypeSymbol typeSymbol = null;
		if (boundExpression.Kind == BoundKind.UnboundLambda)
		{
			foreach (TypeSymbol item in ((UnboundLambda)boundExpression).Data.InferredReturnTypes())
			{
				if (!item.IsErrorType())
				{
					typeSymbol = item;
					break;
				}
			}
		}
		if ((object)typeSymbol == null || typeSymbol.IsErrorType())
		{
			return false;
		}
		TypeSymbol typeSymbol2 = receiver?.Type;
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_QueryTypeInferenceFailedSelectMany, new object[3] { typeSymbol, typeSymbol2, methodName }, symbols), fromClause.Expression.Location);
		return true;
	}

	public virtual BoundStatement BindStatement(StatementSyntax node, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.Block:
			return BindBlock((BlockSyntax)node, diagnostics);
		case SyntaxKind.LocalDeclarationStatement:
			return BindLocalDeclarationStatement((LocalDeclarationStatementSyntax)node, diagnostics);
		case SyntaxKind.LocalFunctionStatement:
			return BindLocalFunctionStatement((LocalFunctionStatementSyntax)node, diagnostics);
		case SyntaxKind.ExpressionStatement:
			return BindExpressionStatement((ExpressionStatementSyntax)node, diagnostics);
		case SyntaxKind.IfStatement:
			return BindIfStatement((IfStatementSyntax)node, diagnostics);
		case SyntaxKind.SwitchStatement:
			return BindSwitchStatement((SwitchStatementSyntax)node, diagnostics);
		case SyntaxKind.DoStatement:
			return BindDo((DoStatementSyntax)node, diagnostics);
		case SyntaxKind.WhileStatement:
			return BindWhile((WhileStatementSyntax)node, diagnostics);
		case SyntaxKind.ForStatement:
			return BindFor((ForStatementSyntax)node, diagnostics);
		case SyntaxKind.ForEachStatement:
		case SyntaxKind.ForEachVariableStatement:
			return BindForEach((CommonForEachStatementSyntax)node, diagnostics);
		case SyntaxKind.BreakStatement:
			return BindBreak((BreakStatementSyntax)node, diagnostics);
		case SyntaxKind.ContinueStatement:
			return BindContinue((ContinueStatementSyntax)node, diagnostics);
		case SyntaxKind.ReturnStatement:
			return BindReturn((ReturnStatementSyntax)node, diagnostics);
		case SyntaxKind.FixedStatement:
			return BindFixedStatement((FixedStatementSyntax)node, diagnostics);
		case SyntaxKind.LabeledStatement:
			return BindLabeled((LabeledStatementSyntax)node, diagnostics);
		case SyntaxKind.GotoStatement:
		case SyntaxKind.GotoCaseStatement:
		case SyntaxKind.GotoDefaultStatement:
			return BindGoto((GotoStatementSyntax)node, diagnostics);
		case SyntaxKind.TryStatement:
			return BindTryStatement((TryStatementSyntax)node, diagnostics);
		case SyntaxKind.EmptyStatement:
			return BindEmpty((EmptyStatementSyntax)node);
		case SyntaxKind.ThrowStatement:
			return BindThrow((ThrowStatementSyntax)node, diagnostics);
		case SyntaxKind.UnsafeStatement:
			return BindUnsafeStatement((UnsafeStatementSyntax)node, diagnostics);
		case SyntaxKind.CheckedStatement:
		case SyntaxKind.UncheckedStatement:
			return BindCheckedStatement((CheckedStatementSyntax)node, diagnostics);
		case SyntaxKind.UsingStatement:
			return BindUsingStatement((UsingStatementSyntax)node, diagnostics);
		case SyntaxKind.YieldBreakStatement:
			return BindYieldBreakStatement((YieldStatementSyntax)node, diagnostics);
		case SyntaxKind.YieldReturnStatement:
			return BindYieldReturnStatement((YieldStatementSyntax)node, diagnostics);
		case SyntaxKind.LockStatement:
			return BindLockStatement((LockStatementSyntax)node, diagnostics);
		default:
			return new BoundBadStatement(node, ImmutableArray<BoundNode>.Empty, hasErrors: true);
		}
	}

	private BoundStatement BindCheckedStatement(CheckedStatementSyntax node, DiagnosticBag diagnostics)
	{
		return BindEmbeddedBlock(node.Block, diagnostics);
	}

	private BoundStatement BindUnsafeStatement(UnsafeStatementSyntax node, DiagnosticBag diagnostics)
	{
		GetBinder(node);
		if (!Compilation.Options.AllowUnsafe)
		{
			Error(diagnostics, ErrorCode.ERR_IllegalUnsafe, node.UnsafeKeyword);
		}
		else if (IsIndirectlyInIterator)
		{
			Error(diagnostics, ErrorCode.ERR_IllegalInnerUnsafe, node.UnsafeKeyword);
		}
		return BindEmbeddedBlock(node.Block, diagnostics);
	}

	private BoundStatement BindFixedStatement(FixedStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		binder.ReportUnsafeIfNotAllowed(node, diagnostics);
		return binder.BindFixedStatementParts(node, diagnostics);
	}

	private BoundStatement BindFixedStatementParts(FixedStatementSyntax node, DiagnosticBag diagnostics)
	{
		VariableDeclarationSyntax declaration = node.Declaration;
		BindForOrUsingOrFixedDeclarations(declaration, LocalDeclarationKind.FixedVariable, diagnostics, out var declarations);
		BoundMultipleLocalDeclarations declarations2 = new BoundMultipleLocalDeclarations(declaration, declarations);
		BoundStatement body = BindPossibleEmbeddedStatement(node.Statement, diagnostics);
		return new BoundFixedStatement(node, GetDeclaredLocalsForScope(node), declarations2, body);
	}

	private BoundStatement BindYieldReturnStatement(YieldStatementSyntax node, DiagnosticBag diagnostics)
	{
		TypeSymbol iteratorElementType = GetIteratorElementType(node, diagnostics);
		BoundExpression expr = ((node.Expression == null) ? BadExpression(node).MakeCompilerGenerated() : BindValue(node.Expression, diagnostics, BindValueKind.RValue));
		expr = ValidateEscape(expr, 0u, isByRef: false, diagnostics);
		if (!expr.HasAnyErrors)
		{
			expr = GenerateConversionForAssignment(iteratorElementType, expr, diagnostics);
		}
		if (Flags.Includes(BinderFlags.InFinallyBlock))
		{
			Error(diagnostics, ErrorCode.ERR_BadYieldInFinally, node.YieldKeyword);
		}
		else if (Flags.Includes(BinderFlags.InTryBlockOfTryCatch))
		{
			Error(diagnostics, ErrorCode.ERR_BadYieldInTryOfCatch, node.YieldKeyword);
		}
		else if (Flags.Includes(BinderFlags.InCatchBlock))
		{
			Error(diagnostics, ErrorCode.ERR_BadYieldInCatch, node.YieldKeyword);
		}
		else if (BindingTopLevelScriptCode)
		{
			Error(diagnostics, ErrorCode.ERR_YieldNotAllowedInScript, node.YieldKeyword);
		}
		return new BoundYieldReturnStatement(node, expr);
	}

	private BoundStatement BindYieldBreakStatement(YieldStatementSyntax node, DiagnosticBag diagnostics)
	{
		if (Flags.Includes(BinderFlags.InFinallyBlock))
		{
			Error(diagnostics, ErrorCode.ERR_BadYieldInFinally, node.YieldKeyword);
		}
		else if (BindingTopLevelScriptCode)
		{
			Error(diagnostics, ErrorCode.ERR_YieldNotAllowedInScript, node.YieldKeyword);
		}
		GetIteratorElementType(node, diagnostics);
		return new BoundYieldBreakStatement(node);
	}

	private BoundStatement BindLockStatement(LockStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return binder.BindLockStatementParts(diagnostics, binder);
	}

	internal virtual BoundStatement BindLockStatementParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindLockStatementParts(diagnostics, originalBinder);
	}

	private BoundStatement BindUsingStatement(UsingStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return binder.BindUsingStatementParts(diagnostics, binder);
	}

	internal virtual BoundStatement BindUsingStatementParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindUsingStatementParts(diagnostics, originalBinder);
	}

	internal BoundStatement BindPossibleEmbeddedStatement(StatementSyntax node, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.LocalDeclarationStatement:
			diagnostics.Add(ErrorCode.ERR_BadEmbeddedStmt, node.GetLocation());
			goto case SyntaxKind.ExpressionStatement;
		case SyntaxKind.ExpressionStatement:
		case SyntaxKind.ReturnStatement:
		case SyntaxKind.YieldReturnStatement:
		case SyntaxKind.ThrowStatement:
		case SyntaxKind.LockStatement:
		case SyntaxKind.IfStatement:
		{
			Binder binder = GetBinder(node);
			return binder.WrapWithVariablesIfAny(node, binder.BindStatement(node, diagnostics));
		}
		case SyntaxKind.LabeledStatement:
		case SyntaxKind.LocalFunctionStatement:
		{
			diagnostics.Add(ErrorCode.ERR_BadEmbeddedStmt, node.GetLocation());
			Binder binder = GetBinder(node);
			return binder.WrapWithVariablesAndLocalFunctionsIfAny(node, binder.BindStatement(node, diagnostics));
		}
		case SyntaxKind.SwitchStatement:
		{
			SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)node;
			Binder binder = GetBinder(switchStatementSyntax.Expression);
			return binder.WrapWithVariablesIfAny(switchStatementSyntax.Expression, binder.BindStatement(node, diagnostics));
		}
		case SyntaxKind.EmptyStatement:
		{
			EmptyStatementSyntax emptyStatementSyntax = (EmptyStatementSyntax)node;
			if (!emptyStatementSyntax.SemicolonToken.IsMissing)
			{
				SyntaxKind syntaxKind = node.Parent.Kind();
				if ((syntaxKind != SyntaxKind.WhileStatement && syntaxKind - 8811 > SyntaxKind.List && syntaxKind != SyntaxKind.ForEachVariableStatement) || emptyStatementSyntax.SemicolonToken.GetNextToken().Kind() == SyntaxKind.OpenBraceToken)
				{
					diagnostics.Add(ErrorCode.WRN_PossibleMistakenNullStatement, node.GetLocation());
				}
			}
			break;
		}
		}
		return BindStatement(node, diagnostics);
	}

	private BoundExpression BindThrownExpression(ExpressionSyntax exprSyntax, DiagnosticBag diagnostics, ref bool hasErrors)
	{
		BoundExpression boundExpression = BindValue(exprSyntax, diagnostics, BindValueKind.RValue);
		if (!boundExpression.IsLiteralNull())
		{
			TypeSymbol type = boundExpression.Type;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			if ((object)type == null || (!type.IsErrorType() && !Compilation.IsExceptionType(type.EffectiveType(ref useSiteDiagnostics), ref useSiteDiagnostics)))
			{
				diagnostics.Add(ErrorCode.ERR_BadExceptionType, exprSyntax.Location);
				hasErrors = true;
				diagnostics.Add(exprSyntax, useSiteDiagnostics);
			}
		}
		return boundExpression;
	}

	private BoundStatement BindThrow(ThrowStatementSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression expressionOpt = null;
		bool hasErrors = false;
		ExpressionSyntax expression = node.Expression;
		if (expression != null)
		{
			expressionOpt = BindThrownExpression(expression, diagnostics, ref hasErrors);
		}
		else if (!Flags.Includes(BinderFlags.InCatchBlock))
		{
			diagnostics.Add(ErrorCode.ERR_BadEmptyThrow, node.ThrowKeyword.GetLocation());
			hasErrors = true;
		}
		else if (Flags.Includes(BinderFlags.InNestedFinallyBlock))
		{
			diagnostics.Add(ErrorCode.ERR_BadEmptyThrowInFinally, node.ThrowKeyword.GetLocation());
			hasErrors = true;
		}
		return new BoundThrowStatement(node, expressionOpt, hasErrors);
	}

	private static BoundStatement BindEmpty(EmptyStatementSyntax node)
	{
		return new BoundNoOpStatement(node, NoOpStatementFlavor.Default);
	}

	private BoundLabeledStatement BindLabeled(LabeledStatementSyntax node, DiagnosticBag diagnostics)
	{
		bool hasErrors = false;
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Binder binder = LookupSymbolsWithFallback(instance, node.Identifier.ValueText, 0, ref useSiteDiagnostics, null, LookupOptions.LabelsOnly);
		LabelSymbol labelSymbol = ((instance.Symbols.Count > 0 && instance.IsMultiViable) ? ((LabelSymbol)instance.Symbols.First()) : new SourceLabelSymbol((MethodSymbol)ContainingMemberOrLambda, node.Identifier));
		if (!labelSymbol.IdentifierNodeOrToken.IsToken || labelSymbol.IdentifierNodeOrToken.AsToken() != node.Identifier)
		{
			Error(diagnostics, ErrorCode.ERR_DuplicateLabel, node.Identifier, node.Identifier.ValueText);
			hasErrors = true;
		}
		if (binder != null)
		{
			instance.Clear();
			binder.Next.LookupSymbolsWithFallback(instance, node.Identifier.ValueText, 0, ref useSiteDiagnostics, null, LookupOptions.LabelsOnly);
			if (instance.IsMultiViable)
			{
				Error(diagnostics, ErrorCode.ERR_LabelShadow, node.Identifier, node.Identifier.ValueText);
				hasErrors = true;
			}
		}
		diagnostics.Add(node, useSiteDiagnostics);
		instance.Free();
		BoundStatement body = BindStatement(node.Statement, diagnostics);
		return new BoundLabeledStatement(node, labelSymbol, body, hasErrors);
	}

	private BoundStatement BindGoto(GotoStatementSyntax node, DiagnosticBag diagnostics)
	{
		switch (node.Kind())
		{
		case SyntaxKind.GotoStatement:
		{
			BoundExpression boundExpression = BindLabel(node.Expression, diagnostics);
			if (!(boundExpression is BoundLabel { Label: var label } boundLabel))
			{
				return new BoundBadStatement(node, ImmutableArray.Create((BoundNode)boundExpression), hasErrors: true);
			}
			return new BoundGotoStatement(node, label, null, boundLabel);
		}
		case SyntaxKind.GotoCaseStatement:
		case SyntaxKind.GotoDefaultStatement:
		{
			SwitchBinder switchBinder = GetSwitchBinder(this);
			if (switchBinder == null)
			{
				Error(diagnostics, ErrorCode.ERR_InvalidGotoCase, node);
				ImmutableArray<BoundNode> childBoundNodes;
				if (node.Expression != null)
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					childBoundNodes = ImmutableArray.Create((BoundNode)BindValue(node.Expression, instance, BindValueKind.RValue));
					instance.Free();
				}
				else
				{
					childBoundNodes = ImmutableArray<BoundNode>.Empty;
				}
				return new BoundBadStatement(node, childBoundNodes, hasErrors: true);
			}
			return switchBinder.BindGotoCaseOrDefault(node, this, diagnostics);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind());
		}
	}

	private BoundStatement BindLocalFunctionStatement(LocalFunctionStatementSyntax node, DiagnosticBag diagnostics)
	{
		LocalFunctionSymbol localSymbol = LookupLocalFunction(node.Identifier);
		bool hasErrors = localSymbol.ScopeBinder.ValidateDeclarationNameConflictsInScope(localSymbol, diagnostics);
		BoundBlock blockBody = null;
		BoundBlock expressionBody = null;
		if (node.Body != null)
		{
			blockBody = runAnalysis(BindEmbeddedBlock(node.Body, diagnostics), diagnostics);
			if (node.ExpressionBody != null)
			{
				DiagnosticBag diagnosticBag = new DiagnosticBag();
				expressionBody = runAnalysis(BindExpressionBodyAsBlock(node.ExpressionBody, diagnosticBag), diagnosticBag);
			}
		}
		else if (node.ExpressionBody != null)
		{
			expressionBody = runAnalysis(BindExpressionBodyAsBlock(node.ExpressionBody, diagnostics), diagnostics);
		}
		else
		{
			hasErrors = true;
			diagnostics.Add(ErrorCode.ERR_LocalFunctionMissingBody, localSymbol.Locations[0], localSymbol);
		}
		localSymbol.GetDeclarationDiagnostics(diagnostics);
		Symbol.CheckForBlockAndExpressionBody(node.Body, node.ExpressionBody, node, diagnostics);
		return new BoundLocalFunctionStatement(node, localSymbol, blockBody, expressionBody, hasErrors);
		BoundBlock runAnalysis(BoundBlock block, DiagnosticBag blockDiagnostics)
		{
			if (block != null && ControlFlowPass.Analyze(localSymbol.DeclaringCompilation, localSymbol, block, blockDiagnostics))
			{
				if (ImplicitReturnIsOkay(localSymbol))
				{
					block = FlowAnalysisPass.AppendImplicitReturn(block, localSymbol);
				}
				else
				{
					blockDiagnostics.Add(ErrorCode.ERR_ReturnExpected, localSymbol.Locations[0], localSymbol);
				}
			}
			return block;
		}
	}

	private bool ImplicitReturnIsOkay(MethodSymbol method)
	{
		if (!method.ReturnsVoid && !method.IsIterator)
		{
			return method.IsTaskReturningAsync(Compilation);
		}
		return true;
	}

	public BoundStatement BindExpressionStatement(ExpressionStatementSyntax node, DiagnosticBag diagnostics)
	{
		return BindExpressionStatement(node, node.Expression, node.AllowsAnyExpression, diagnostics);
	}

	private BoundExpressionStatement BindExpressionStatement(CSharpSyntaxNode node, ExpressionSyntax syntax, bool allowsAnyExpression, DiagnosticBag diagnostics)
	{
		BoundExpression expression = BindValue(syntax, diagnostics, BindValueKind.RValue);
		BoundExpressionStatement result;
		if (!allowsAnyExpression && !IsValidStatementExpression(syntax, expression))
		{
			if (!node.HasErrors)
			{
				Error(diagnostics, ErrorCode.ERR_IllegalStatement, syntax);
			}
			result = new BoundExpressionStatement(node, expression, hasErrors: true);
		}
		else
		{
			result = new BoundExpressionStatement(node, expression);
		}
		CheckForUnobservedAwaitable(expression, diagnostics);
		return result;
	}

	private void CheckForUnobservedAwaitable(BoundExpression expression, DiagnosticBag diagnostics)
	{
		if (CouldBeAwaited(expression))
		{
			Error(diagnostics, ErrorCode.WRN_UnobservedAwaitableExpression, expression.Syntax);
		}
	}

	internal BoundStatement BindLocalDeclarationStatement(LocalDeclarationStatementSyntax node, DiagnosticBag diagnostics)
	{
		return BindDeclarationStatementParts(node, diagnostics);
	}

	private BoundStatement BindDeclarationStatementParts(LocalDeclarationStatementSyntax node, DiagnosticBag diagnostics)
	{
		TypeSyntax typeSyntax = node.Declaration.Type.SkipRef(out var _);
		bool isConst = node.IsConst;
		TypeSymbol declTypeOpt = BindVariableType(node.Declaration, diagnostics, typeSyntax, ref isConst, out var isVar, out var alias);
		LocalDeclarationKind kind = LocalDeclarationKind.RegularVariable;
		if (isConst)
		{
			kind = LocalDeclarationKind.Constant;
		}
		SeparatedSyntaxList<VariableDeclaratorSyntax> variables = node.Declaration.Variables;
		int count = variables.Count;
		if (count == 1)
		{
			return BindVariableDeclaration(kind, isVar, variables[0], typeSyntax, declTypeOpt, alias, diagnostics, node);
		}
		BoundLocalDeclaration[] array = new BoundLocalDeclaration[count];
		int num = 0;
		foreach (VariableDeclaratorSyntax item in variables)
		{
			array[num++] = BindVariableDeclaration(kind, isVar, item, typeSyntax, declTypeOpt, alias, diagnostics);
		}
		return new BoundMultipleLocalDeclarations(node, array.AsImmutableOrNull());
	}

	private TypeSymbol BindVariableType(CSharpSyntaxNode declarationNode, DiagnosticBag diagnostics, TypeSyntax typeSyntax, ref bool isConst, out bool isVar, out AliasSymbol alias)
	{
		TypeSymbol typeSymbol = BindTypeOrVarKeyword(typeSyntax.SkipRef(out var _), diagnostics, out isVar, out alias);
		if (isVar)
		{
			if (isConst)
			{
				Error(diagnostics, ErrorCode.ERR_ImplicitlyTypedVariableCannotBeConst, declarationNode);
				isConst = false;
			}
			if (declarationNode.Parent.Kind() == SyntaxKind.LocalDeclarationStatement && ((VariableDeclarationSyntax)declarationNode).Variables.Count > 1 && !declarationNode.HasErrors)
			{
				Error(diagnostics, ErrorCode.ERR_ImplicitlyTypedVariableMultipleDeclarator, declarationNode);
			}
		}
		else
		{
			if (typeSymbol.IsStatic)
			{
				Error(diagnostics, ErrorCode.ERR_VarDeclIsStaticClass, typeSyntax, typeSymbol);
			}
			if (isConst && !typeSymbol.CanBeConst())
			{
				Error(diagnostics, ErrorCode.ERR_BadConstType, typeSyntax, typeSymbol);
				isConst = false;
			}
		}
		return typeSymbol;
	}

	internal BoundExpression BindInferredVariableInitializer(DiagnosticBag diagnostics, RefKind refKind, EqualsValueClauseSyntax initializer, CSharpSyntaxNode errorSyntax)
	{
		IsInitializerRefKindValid(initializer, initializer, refKind, diagnostics, out var valueKind, out var value);
		return BindInferredVariableInitializer(diagnostics, value, valueKind, refKind, errorSyntax);
	}

	protected BoundExpression BindInferredVariableInitializer(DiagnosticBag diagnostics, ExpressionSyntax initializer, BindValueKind valueKind, RefKind refKind, CSharpSyntaxNode errorSyntax)
	{
		if (initializer == null)
		{
			if (!errorSyntax.HasErrors)
			{
				Error(diagnostics, ErrorCode.ERR_ImplicitlyTypedVariableWithNoInitializer, errorSyntax);
			}
			return null;
		}
		if (initializer.Kind() == SyntaxKind.ArrayInitializerExpression)
		{
			BoundArrayInitialization expr = BindUnexpectedArrayInitializer((InitializerExpressionSyntax)initializer, diagnostics, ErrorCode.ERR_ImplicitlyTypedVariableAssignedArrayInitializer, errorSyntax);
			return CheckValue(expr, valueKind, diagnostics);
		}
		BoundExpression boundExpression = BindValue(initializer, diagnostics, valueKind);
		if (boundExpression is BoundStackAllocArrayCreation boundStackAllocArrayCreation)
		{
			PointerTypeSymbol targetType = new PointerTypeSymbol(boundStackAllocArrayCreation.ElementType);
			boundExpression = GenerateConversionForAssignment(targetType, boundStackAllocArrayCreation, diagnostics, isDefaultParameter: false, refKind != RefKind.None);
		}
		if (!boundExpression.HasAnyErrors && !boundExpression.HasExpressionType())
		{
			Error(diagnostics, ErrorCode.ERR_ImplicitlyTypedVariableAssignedBadValue, errorSyntax, boundExpression.Display);
		}
		return boundExpression;
	}

	private static bool IsInitializerRefKindValid(EqualsValueClauseSyntax initializer, CSharpSyntaxNode node, RefKind variableRefKind, DiagnosticBag diagnostics, out BindValueKind valueKind, out ExpressionSyntax value)
	{
		RefKind refKind = RefKind.None;
		value = initializer?.Value.CheckAndUnwrapRefExpression(diagnostics, out refKind);
		if (variableRefKind == RefKind.None)
		{
			valueKind = BindValueKind.RValue;
			if (refKind == RefKind.Ref)
			{
				Error(diagnostics, ErrorCode.ERR_InitializeByValueVariableWithReference, node);
				return false;
			}
		}
		else
		{
			valueKind = ((variableRefKind == RefKind.In) ? BindValueKind.ReadonlyRef : BindValueKind.RefOrOut);
			if (initializer == null)
			{
				Error(diagnostics, ErrorCode.ERR_ByReferenceVariableMustBeInitialized, node);
				return false;
			}
			if (refKind != RefKind.Ref)
			{
				Error(diagnostics, ErrorCode.ERR_InitializeByReferenceVariableWithValue, node);
				return false;
			}
		}
		return true;
	}

	protected BoundLocalDeclaration BindVariableDeclaration(LocalDeclarationKind kind, bool isVar, VariableDeclaratorSyntax declarator, TypeSyntax typeSyntax, TypeSymbol declTypeOpt, AliasSymbol aliasOpt, DiagnosticBag diagnostics, CSharpSyntaxNode associatedSyntaxNode = null)
	{
		return BindVariableDeclaration(LocateDeclaredVariableSymbol(declarator, typeSyntax), kind, isVar, declarator, typeSyntax, declTypeOpt, aliasOpt, diagnostics, associatedSyntaxNode);
	}

	protected BoundLocalDeclaration BindVariableDeclaration(SourceLocalSymbol localSymbol, LocalDeclarationKind kind, bool isVar, VariableDeclaratorSyntax declarator, TypeSyntax typeSyntax, TypeSymbol declTypeOpt, AliasSymbol aliasOpt, DiagnosticBag diagnostics, CSharpSyntaxNode associatedSyntaxNode = null)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		associatedSyntaxNode = associatedSyntaxNode ?? declarator;
		bool flag = localSymbol.ScopeBinder.ValidateDeclarationNameConflictsInScope(localSymbol, diagnostics);
		MethodSymbol methodSymbol = ContainingMemberOrLambda as MethodSymbol;
		if (methodSymbol != null && methodSymbol.IsAsync && localSymbol.RefKind != RefKind.None)
		{
			Error(diagnostics, ErrorCode.ERR_BadAsyncLocalType, declarator);
		}
		EqualsValueClauseSyntax initializer = declarator.Initializer;
		if (!IsInitializerRefKindValid(initializer, declarator, localSymbol.RefKind, diagnostics, out var valueKind, out var value))
		{
			flag = true;
		}
		BoundExpression initializerOpt;
		if (isVar)
		{
			aliasOpt = null;
			initializerOpt = BindInferredVariableInitializer(diagnostics, value, valueKind, localSymbol.RefKind, declarator);
			if ((object)initializerOpt?.Type != null)
			{
				declTypeOpt = initializerOpt.Type;
				if (declTypeOpt.SpecialType == SpecialType.System_Void)
				{
					Error(instance, ErrorCode.ERR_ImplicitlyTypedVariableAssignedBadValue, declarator, declTypeOpt);
					declTypeOpt = CreateErrorType("var");
					flag = true;
				}
				if (!declTypeOpt.IsErrorType() && declTypeOpt.IsStatic)
				{
					Error(instance, ErrorCode.ERR_VarDeclIsStaticClass, typeSyntax, initializerOpt.Type);
					flag = true;
				}
			}
			else
			{
				declTypeOpt = CreateErrorType("var");
				flag = true;
			}
		}
		else if (initializer == null)
		{
			initializerOpt = null;
		}
		else
		{
			initializerOpt = BindPossibleArrayInitializer(value, declTypeOpt, valueKind, diagnostics);
			if (kind != LocalDeclarationKind.FixedVariable)
			{
				initializerOpt = GenerateConversionForAssignment(declTypeOpt, initializerOpt, instance, isDefaultParameter: false, localSymbol.RefKind != RefKind.None);
			}
		}
		if (kind == LocalDeclarationKind.FixedVariable)
		{
			if (isVar && !flag)
			{
				Error(instance, ErrorCode.ERR_ImplicitlyTypedLocalCannotBeFixed, declarator);
				flag = true;
			}
			if (!declTypeOpt.IsPointerType())
			{
				if (!flag)
				{
					Error(instance, ErrorCode.ERR_BadFixedInitType, declarator);
					flag = true;
				}
			}
			else if (!IsValidFixedVariableInitializer(declTypeOpt, localSymbol, ref initializerOpt, instance))
			{
				flag = true;
			}
		}
		if (CheckRestrictedTypeInAsync(ContainingMemberOrLambda, declTypeOpt, instance, typeSyntax))
		{
			flag = true;
		}
		localSymbol.SetType(declTypeOpt);
		if (initializerOpt != null)
		{
			uint localScopeDepth = LocalScopeDepth;
			localSymbol.SetValEscape(GetValEscape(initializerOpt, localScopeDepth));
			if (localSymbol.RefKind != RefKind.None)
			{
				localSymbol.SetRefEscape(GetRefEscape(initializerOpt, localScopeDepth));
			}
		}
		ImmutableArray<BoundExpression> argumentsOpt = BindDeclaratorArguments(declarator, instance);
		switch (kind)
		{
		case LocalDeclarationKind.FixedVariable:
		case LocalDeclarationKind.UsingVariable:
			if (initializerOpt == null)
			{
				Error(instance, ErrorCode.ERR_FixedMustInit, declarator);
				flag = true;
			}
			break;
		case LocalDeclarationKind.Constant:
			if (initializerOpt == null || instance.HasAnyResolvedErrors())
			{
				break;
			}
			foreach (Diagnostic constantValueDiagnostic in localSymbol.GetConstantValueDiagnostics(initializerOpt))
			{
				diagnostics.Add(constantValueDiagnostic);
				if (constantValueDiagnostic.Severity == DiagnosticSeverity.Error)
				{
					flag = true;
				}
			}
			break;
		}
		diagnostics.AddRangeAndFree(instance);
		BoundTypeExpression declaredType = new BoundTypeExpression(typeSyntax, aliasOpt, isVar, declTypeOpt);
		return new BoundLocalDeclaration(associatedSyntaxNode, localSymbol, declaredType, initializerOpt, argumentsOpt, flag);
	}

	internal ImmutableArray<BoundExpression> BindDeclaratorArguments(VariableDeclaratorSyntax declarator, DiagnosticBag diagnostics)
	{
		ImmutableArray<BoundExpression> result = default;
		if (declarator.ArgumentList != null)
		{
			AnalyzedArguments instance = AnalyzedArguments.GetInstance();
			BindArgumentsAndNames(declarator.ArgumentList, diagnostics, instance);
			result = BuildArgumentsForErrorRecovery(instance);
			instance.Free();
		}
		return result;
	}

	private SourceLocalSymbol LocateDeclaredVariableSymbol(VariableDeclaratorSyntax declarator, TypeSyntax typeSyntax)
	{
		return LocateDeclaredVariableSymbol(declarator.Identifier, typeSyntax, declarator.Initializer);
	}

	private SourceLocalSymbol LocateDeclaredVariableSymbol(SyntaxToken identifier, TypeSyntax typeSyntax, EqualsValueClauseSyntax equalsValue)
	{
		SourceLocalSymbol sourceLocalSymbol = LookupLocal(identifier);
		if ((object)sourceLocalSymbol == null)
		{
			sourceLocalSymbol = SourceLocalSymbol.MakeLocal(ContainingMemberOrLambda, this, allowRefKind: false, typeSyntax, identifier, LocalDeclarationKind.RegularVariable, equalsValue);
		}
		return sourceLocalSymbol;
	}

	private bool IsValidFixedVariableInitializer(TypeSymbol declType, SourceLocalSymbol localSymbol, ref BoundExpression initializerOpt, DiagnosticBag diagnostics)
	{
		BoundExpression obj = initializerOpt;
		if (obj == null || obj.HasAnyErrors)
		{
			return false;
		}
		TypeSymbol type = initializerOpt.Type;
		SyntaxNode syntax = initializerOpt.Syntax;
		if ((object)type == null)
		{
			Error(diagnostics, ErrorCode.ERR_ExprCannotBeFixed, syntax);
			return false;
		}
		bool hasErrors = false;
		MethodSymbol methodSymbol = null;
		BoundKind kind = initializerOpt.Kind;
		TypeSymbol typeSymbol;
		if (kind != BoundKind.AddressOfOperator)
		{
			if (kind == BoundKind.FieldAccess)
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)initializerOpt;
				if (boundFieldAccess.FieldSymbol.IsFixed)
				{
					typeSymbol = ((PointerTypeSymbol)boundFieldAccess.Type).PointedAtType;
					goto IL_0169;
				}
			}
			if (type.IsArray())
			{
				typeSymbol = ((ArrayTypeSymbol)type).ElementType;
			}
			else
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				methodSymbol = GetFixedPatternMethodOpt(initializerOpt, instance);
				if (type.SpecialType == SpecialType.System_String && ((object)methodSymbol == null || methodSymbol.ContainingType.SpecialType != SpecialType.System_String))
				{
					methodSymbol = null;
					typeSymbol = GetSpecialType(SpecialType.System_Char, diagnostics, syntax);
					instance.Free();
				}
				else
				{
					CSharpParseOptions cSharpParseOptions = (CSharpParseOptions)initializerOpt.SyntaxTree.Options;
					if ((object)cSharpParseOptions == null || cSharpParseOptions.IsFeatureEnabled(MessageID.IDS_FeatureExtensibleFixedStatement))
					{
						diagnostics.AddRange(instance);
					}
					instance.Free();
					if ((object)methodSymbol == null)
					{
						Error(diagnostics, ErrorCode.ERR_ExprCannotBeFixed, syntax);
						return false;
					}
					typeSymbol = methodSymbol.ReturnType;
					CheckFeatureAvailability(initializerOpt.Syntax, MessageID.IDS_FeatureExtensibleFixedStatement, diagnostics);
				}
			}
		}
		else
		{
			typeSymbol = ((BoundAddressOfOperator)initializerOpt).Operand.Type;
		}
		goto IL_0169;
		IL_0169:
		if (typeSymbol.IsManagedType)
		{
			Error(diagnostics, ErrorCode.ERR_ManagedAddr, syntax, typeSymbol);
			hasErrors = true;
		}
		initializerOpt = GetFixedLocalCollectionInitializer(initializerOpt, typeSymbol, declType, methodSymbol, hasErrors, diagnostics);
		return true;
	}

	private MethodSymbol GetFixedPatternMethodOpt(BoundExpression initializer, DiagnosticBag additionalDiagnostics)
	{
		if (initializer.Type.SpecialType == SpecialType.System_Void)
		{
			return null;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			BoundExpression boundExpression = BindInstanceMemberAccess(initializer.Syntax, initializer.Syntax, initializer, "GetPinnableReference", 0, default, default, invoked: true, indexed: false, instance);
			if (boundExpression.Kind != BoundKind.MethodGroup)
			{
				return null;
			}
			AnalyzedArguments instance2 = AnalyzedArguments.GetInstance();
			BoundExpression boundExpression2 = BindMethodGroupInvocation(initializer.Syntax, initializer.Syntax, "GetPinnableReference", (BoundMethodGroup)boundExpression, instance2, instance, null, allowUnexpandedForm: false);
			instance2.Free();
			if (boundExpression2.Kind != BoundKind.Call)
			{
				return null;
			}
			BoundCall boundCall = (BoundCall)boundExpression2;
			if (boundCall.ResultKind == LookupResultKind.Empty)
			{
				return null;
			}
			additionalDiagnostics.AddRange(instance);
			MethodSymbol method = boundCall.Method;
			if (method is ErrorMethodSymbol || boundExpression2.HasAnyErrors)
			{
				return null;
			}
			if (HasOptionalOrVariableParameters(method) || method.ReturnsVoid || !method.RefKind.IsManagedReference() || (method.ParameterCount != 0 && (!method.IsStatic || method.ParameterCount != 1)))
			{
				additionalDiagnostics.Add(ErrorCode.WRN_PatternBadSignature, initializer.Syntax.Location, initializer.Type, "fixed", method);
				return null;
			}
			return method;
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression GetFixedLocalCollectionInitializer(BoundExpression initializer, TypeSymbol elementType, TypeSymbol declType, MethodSymbol patternMethodOpt, bool hasErrors, DiagnosticBag diagnostics)
	{
		SyntaxNode syntax = initializer.Syntax;
		TypeSymbol typeSymbol = new PointerTypeSymbol(elementType);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromType(typeSymbol, declType, ref useSiteDiagnostics);
		diagnostics.Add(syntax, useSiteDiagnostics);
		if (!conversion.IsValid || !conversion.IsImplicit)
		{
			GenerateImplicitConversionError(diagnostics, Compilation, syntax, conversion, typeSymbol, declType);
			hasErrors = true;
		}
		return new BoundFixedLocalCollectionInitializer(syntax, typeSymbol, conversion, initializer, patternMethodOpt, declType, hasErrors);
	}

	private BoundExpression BindAssignment(AssignmentExpressionSyntax node, DiagnosticBag diagnostics)
	{
		node.Left.CheckDeconstructionCompatibleArgument(diagnostics);
		if (node.Left.Kind() == SyntaxKind.TupleExpression || node.Left.Kind() == SyntaxKind.DeclarationExpression)
		{
			return BindDeconstruction(node, diagnostics);
		}
		bool isRef = false;
		BindValueKind bindValueKind;
		BindValueKind bindValueKind2;
		ExpressionSyntax node2;
		if (node.Right.Kind() == SyntaxKind.RefExpression)
		{
			isRef = true;
			bindValueKind = BindValueKind.RefAssignable;
			bindValueKind2 = BindValueKind.RefersToLocation;
			node2 = ((RefExpressionSyntax)node.Right).Expression;
		}
		else
		{
			bindValueKind = BindValueKind.Assignable;
			bindValueKind2 = BindValueKind.RValue;
			node2 = node.Right;
		}
		BoundExpression boundExpression = BindValue(node.Left, diagnostics, bindValueKind);
		if (bindValueKind == BindValueKind.RefAssignable && !boundExpression.HasErrors)
		{
			RefKind refKind = boundExpression.GetRefKind();
			if (refKind == RefKind.Ref || refKind == RefKind.Out)
			{
				bindValueKind2 |= BindValueKind.Assignable;
			}
		}
		BoundExpression op = BindValue(node2, diagnostics, bindValueKind2);
		if (boundExpression.Kind == BoundKind.DiscardExpression)
		{
			boundExpression = InferTypeForDiscardAssignment((BoundDiscardExpression)boundExpression, op, diagnostics);
		}
		return BindAssignment(node, boundExpression, op, isRef, diagnostics);
	}

	private BoundExpression InferTypeForDiscardAssignment(BoundDiscardExpression op1, BoundExpression op2, DiagnosticBag diagnostics)
	{
		TypeSymbol type = op2.Type;
		if (type == null)
		{
			return op1.FailInference(this, diagnostics);
		}
		if (type.SpecialType == SpecialType.System_Void)
		{
			diagnostics.Add(ErrorCode.ERR_VoidAssignment, op1.Syntax.Location);
		}
		return op1.SetInferredType(type);
	}

	private BoundAssignmentOperator BindAssignment(SyntaxNode node, BoundExpression op1, BoundExpression op2, bool isRef, DiagnosticBag diagnostics)
	{
		bool hasErrors = op1.HasAnyErrors || op2.HasAnyErrors;
		if (!op1.HasAnyErrors)
		{
			BoundExpression boundExpression = GenerateConversionForAssignment(op1.Type, op2, diagnostics, isDefaultParameter: false, isRef);
			if (op1.Kind != BoundKind.DynamicIndexerAccess && op1.Kind != BoundKind.DynamicMemberAccess && op1.Kind != BoundKind.DynamicObjectInitializerMember)
			{
				op2 = boundExpression;
			}
			if (isRef)
			{
				uint refEscape = GetRefEscape(op1, LocalScopeDepth);
				uint refEscape2 = GetRefEscape(op2, LocalScopeDepth);
				if (refEscape < refEscape2)
				{
					Error(diagnostics, ErrorCode.ERR_RefAssignNarrower, node, op1.ExpressionSymbol.Name, op2.Syntax);
					op2 = ToBadExpression(op2);
				}
			}
			if (op1.Type.IsByRefLikeType)
			{
				uint valEscape = GetValEscape(op1, LocalScopeDepth);
				op2 = ValidateEscape(op2, valEscape, isByRef: false, diagnostics);
			}
		}
		TypeSymbol type = ((op1.Kind != BoundKind.EventAccess || !((BoundEventAccess)op1).EventSymbol.IsWindowsRuntimeEvent) ? op1.Type : GetSpecialType(SpecialType.System_Void, diagnostics, node));
		return new BoundAssignmentOperator(node, op1, op2, isRef, type, hasErrors);
	}

	private static PropertySymbol GetPropertySymbol(BoundExpression expr, out BoundExpression receiver, out SyntaxNode propertySyntax)
	{
		PropertySymbol result;
		switch (expr.Kind)
		{
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)expr;
			receiver = boundPropertyAccess.ReceiverOpt;
			result = boundPropertyAccess.PropertySymbol;
			break;
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)expr;
			receiver = boundIndexerAccess.ReceiverOpt;
			result = boundIndexerAccess.Indexer;
			break;
		}
		default:
			receiver = null;
			propertySyntax = null;
			return null;
		}
		SyntaxNode syntax = expr.Syntax;
		switch (syntax.Kind())
		{
		case SyntaxKind.SimpleMemberAccessExpression:
		case SyntaxKind.PointerMemberAccessExpression:
			propertySyntax = ((MemberAccessExpressionSyntax)syntax).Name;
			break;
		case SyntaxKind.IdentifierName:
			propertySyntax = syntax;
			break;
		case SyntaxKind.ElementAccessExpression:
			propertySyntax = ((ElementAccessExpressionSyntax)syntax).ArgumentList;
			break;
		default:
			propertySyntax = syntax;
			break;
		}
		return result;
	}

	private static SyntaxNode GetEventName(BoundEventAccess expr)
	{
		SyntaxNode syntax = expr.Syntax;
		switch (syntax.Kind())
		{
		case SyntaxKind.SimpleMemberAccessExpression:
		case SyntaxKind.PointerMemberAccessExpression:
			return ((MemberAccessExpressionSyntax)syntax).Name;
		case SyntaxKind.QualifiedName:
			return ((QualifiedNameSyntax)syntax).Right;
		case SyntaxKind.IdentifierName:
			return syntax;
		case SyntaxKind.MemberBindingExpression:
			return ((MemberBindingExpressionSyntax)syntax).Name;
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
		}
	}

	private DiagnosticInfo GetBadEventUsageDiagnosticInfo(EventSymbol eventSymbol)
	{
		EventSymbol eventSymbol2 = (EventSymbol)eventSymbol.GetLeastOverriddenMember(ContainingType);
		if (!eventSymbol2.HasAssociatedField)
		{
			return new CSDiagnosticInfo(ErrorCode.ERR_BadEventUsageNoField, eventSymbol2);
		}
		return new CSDiagnosticInfo(ErrorCode.ERR_BadEventUsage, eventSymbol2, eventSymbol2.ContainingType);
	}

	internal static bool AccessingAutoPropertyFromConstructor(BoundPropertyAccess propertyAccess, Symbol fromMember)
	{
		return AccessingAutoPropertyFromConstructor(propertyAccess.ReceiverOpt, propertyAccess.PropertySymbol, fromMember);
	}

	private static bool AccessingAutoPropertyFromConstructor(BoundExpression receiver, PropertySymbol propertySymbol, Symbol fromMember)
	{
		SourcePropertySymbol sourcePropertySymbol = propertySymbol as SourcePropertySymbol;
		bool isStatic = propertySymbol.IsStatic;
		if ((object)sourcePropertySymbol != null && sourcePropertySymbol.IsAutoProperty && sourcePropertySymbol.ContainingType == fromMember.ContainingType && IsConstructorOrField(fromMember, isStatic))
		{
			if (!isStatic)
			{
				return receiver.Kind == BoundKind.ThisReference;
			}
			return true;
		}
		return false;
	}

	private static bool IsConstructorOrField(Symbol member, bool isStatic)
	{
		if ((member as MethodSymbol)?.MethodKind != (MethodKind?)((!isStatic) ? 1 : 14))
		{
			FieldSymbol obj = member as FieldSymbol;
			if ((object)obj == null)
			{
				return false;
			}
			return obj.IsStatic == isStatic;
		}
		return true;
	}

	private TypeSymbol GetAccessThroughType(BoundExpression receiver)
	{
		if (receiver == null)
		{
			return ContainingType;
		}
		if (receiver.Kind == BoundKind.BaseReference)
		{
			return null;
		}
		return receiver.Type;
	}

	private BoundExpression BindPossibleArrayInitializer(ExpressionSyntax node, TypeSymbol destinationType, BindValueKind valueKind, DiagnosticBag diagnostics)
	{
		if (node.Kind() != SyntaxKind.ArrayInitializerExpression)
		{
			return BindValue(node, diagnostics, valueKind);
		}
		BoundExpression expr = ((destinationType.Kind != SymbolKind.ArrayType) ? ((BoundExpression)BindUnexpectedArrayInitializer((InitializerExpressionSyntax)node, diagnostics, ErrorCode.ERR_ArrayInitToNonArrayType)) : ((BoundExpression)BindArrayCreationWithInitializer(diagnostics, null, (InitializerExpressionSyntax)node, (ArrayTypeSymbol)destinationType, ImmutableArray<BoundExpression>.Empty)));
		return CheckValue(expr, valueKind, diagnostics);
	}

	protected virtual SourceLocalSymbol LookupLocal(SyntaxToken nameToken)
	{
		return Next.LookupLocal(nameToken);
	}

	protected virtual LocalFunctionSymbol LookupLocalFunction(SyntaxToken nameToken)
	{
		return Next.LookupLocalFunction(nameToken);
	}

	internal BoundBlock BindEmbeddedBlock(BlockSyntax node, DiagnosticBag diagnostics)
	{
		return BindBlock(node, diagnostics);
	}

	private BoundBlock BindBlock(BlockSyntax node, DiagnosticBag diagnostics)
	{
		return GetBinder(node).BindBlockParts(node, diagnostics);
	}

	private BoundBlock BindBlockParts(BlockSyntax node, DiagnosticBag diagnostics)
	{
		SyntaxList<StatementSyntax> statements = node.Statements;
		int count = statements.Count;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(count);
		for (int i = 0; i < count; i++)
		{
			BoundStatement item = BindStatement(statements[i], diagnostics);
			instance.Add(item);
		}
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetDeclaredLocalsForScope(node);
		if (IsDirectlyInIterator)
		{
			if (ContainingMemberOrLambda is MethodSymbol methodSymbol)
			{
				methodSymbol.IteratorElementType = GetIteratorElementType(null, diagnostics);
			}
			foreach (LocalSymbol item2 in declaredLocalsForScope)
			{
				if (item2.RefKind != RefKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_BadIteratorLocalType, item2.Locations[0]);
				}
			}
		}
		return new BoundBlock(node, declaredLocalsForScope, GetDeclaredLocalFunctionsForScope(node), instance.ToImmutableAndFree());
	}

	internal BoundExpression GenerateConversionForAssignment(TypeSymbol targetType, BoundExpression expression, DiagnosticBag diagnostics, bool isDefaultParameter = false, bool isRefAssignment = false)
	{
		if (expression.HasAnyErrors && expression.Kind != BoundKind.UnboundLambda)
		{
			diagnostics = new DiagnosticBag();
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(expression, targetType, ref useSiteDiagnostics);
		diagnostics.Add(expression.Syntax, useSiteDiagnostics);
		if (isRefAssignment)
		{
			if (conversion.Kind == ConversionKind.Identity)
			{
				return expression;
			}
			Error(diagnostics, ErrorCode.ERR_RefAssignmentMustHaveIdentityConversion, expression.Syntax, targetType);
		}
		else if (!conversion.IsImplicit || !conversion.IsValid)
		{
			if (!isDefaultParameter)
			{
				GenerateImplicitConversionError(diagnostics, expression.Syntax, conversion, expression, targetType);
			}
			diagnostics = new DiagnosticBag();
		}
		return CreateConversion(expression.Syntax, expression, conversion, isCast: false, targetType, diagnostics);
	}

	internal void GenerateAnonymousFunctionConversionError(DiagnosticBag diagnostics, SyntaxNode syntax, UnboundLambda anonymousFunction, TypeSymbol targetType)
	{
		if (targetType.IsErrorType())
		{
			return;
		}
		LambdaConversionResult lambdaConversionResult = ConversionsBase.IsAnonymousFunctionCompatibleWithType(anonymousFunction, targetType);
		if (lambdaConversionResult == LambdaConversionResult.Success)
		{
			return;
		}
		LocalizableErrorArgument localizableErrorArgument = anonymousFunction.MessageID.Localize();
		switch (lambdaConversionResult)
		{
		case LambdaConversionResult.BadTargetType:
			if (!ReportDelegateInvokeUseSiteDiagnostic(diagnostics, targetType, null, syntax))
			{
				Error(diagnostics, ErrorCode.ERR_AnonMethToNonDel, syntax, localizableErrorArgument, targetType);
			}
			return;
		case LambdaConversionResult.ExpressionTreeMustHaveDelegateTypeArgument:
			Error(diagnostics, ErrorCode.ERR_ExpressionTreeMustHaveDelegate, syntax, ((NamedTypeSymbol)targetType).TypeArgumentsNoUseSiteDiagnostics[0]);
			return;
		case LambdaConversionResult.ExpressionTreeFromAnonymousMethod:
			Error(diagnostics, ErrorCode.ERR_AnonymousMethodToExpressionTree, syntax);
			return;
		}
		NamedTypeSymbol delegateType = targetType.GetDelegateType();
		switch (lambdaConversionResult)
		{
		case LambdaConversionResult.MissingSignatureWithOutParameter:
			Error(diagnostics, ErrorCode.ERR_CantConvAnonMethNoParams, syntax, targetType);
			return;
		case LambdaConversionResult.BadParameterCount:
			Error(diagnostics, ErrorCode.ERR_BadDelArgCount, syntax, delegateType, anonymousFunction.ParameterCount);
			return;
		}
		if (anonymousFunction.HasExplicitlyTypedParameterList)
		{
			for (int i = 0; i < anonymousFunction.ParameterCount; i++)
			{
				if (anonymousFunction.ParameterType(i).IsErrorType())
				{
					return;
				}
			}
		}
		ImmutableArray<ParameterSymbol> immutableArray = delegateType.DelegateParameters();
		switch (lambdaConversionResult)
		{
		case LambdaConversionResult.RefInImplicitlyTypedLambda:
		{
			for (int k = 0; k < anonymousFunction.ParameterCount; k++)
			{
				RefKind refKind3 = immutableArray[k].RefKind;
				if (refKind3 != RefKind.None)
				{
					Error(diagnostics, ErrorCode.ERR_BadParamRef, anonymousFunction.ParameterLocation(k), k + 1, refKind3.ToParameterDisplayString());
				}
			}
			break;
		}
		case LambdaConversionResult.StaticTypeInImplicitlyTypedLambda:
		{
			for (int l = 0; l < anonymousFunction.ParameterCount; l++)
			{
				if (immutableArray[l].Type.IsStatic)
				{
					Error(diagnostics, ErrorCode.ERR_ParameterIsStaticClass, anonymousFunction.ParameterLocation(l), immutableArray[l].Type);
				}
			}
			break;
		}
		case LambdaConversionResult.MismatchedParameterType:
		{
			Error(diagnostics, ErrorCode.ERR_CantConvAnonMethParams, syntax, localizableErrorArgument, targetType);
			for (int j = 0; j < anonymousFunction.ParameterCount; j++)
			{
				TypeSymbol typeSymbol = anonymousFunction.ParameterType(j);
				if (typeSymbol.IsErrorType())
				{
					continue;
				}
				Location location = anonymousFunction.ParameterLocation(j);
				RefKind refKind = anonymousFunction.RefKind(j);
				TypeSymbol type = immutableArray[j].Type;
				RefKind refKind2 = immutableArray[j].RefKind;
				if (!typeSymbol.Equals(type, TypeCompareKind.AllIgnoreOptions))
				{
					SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(Compilation, typeSymbol, type);
					Error(diagnostics, ErrorCode.ERR_BadParamType, location, j + 1, refKind.ToParameterPrefix(), symbolDistinguisher.First, refKind2.ToParameterPrefix(), symbolDistinguisher.Second);
				}
				else if (refKind != refKind2)
				{
					if (refKind2 == RefKind.None)
					{
						Error(diagnostics, ErrorCode.ERR_BadParamExtraRef, location, j + 1, refKind.ToParameterDisplayString());
					}
					else
					{
						Error(diagnostics, ErrorCode.ERR_BadParamRef, location, j + 1, refKind2.ToParameterDisplayString());
					}
				}
			}
			break;
		}
		case LambdaConversionResult.BindingFailed:
		{
			BoundLambda boundLambda = anonymousFunction.Bind(delegateType);
			diagnostics.AddRange(boundLambda.Diagnostics);
			break;
		}
		}
	}

	protected static void GenerateImplicitConversionError(DiagnosticBag diagnostics, Compilation compilation, SyntaxNode syntax, Conversion conversion, TypeSymbol sourceType, TypeSymbol targetType, ConstantValue sourceConstantValueOpt = null)
	{
		if (sourceType.IsErrorType() || targetType.IsErrorType())
		{
			return;
		}
		if (conversion.IsExplicit)
		{
			if (sourceType.SpecialType == SpecialType.System_Double && syntax.Kind() == SyntaxKind.NumericLiteralExpression && (targetType.SpecialType == SpecialType.System_Single || targetType.SpecialType == SpecialType.System_Decimal))
			{
				Error(diagnostics, ErrorCode.ERR_LiteralDoubleCast, syntax, (targetType.SpecialType == SpecialType.System_Single) ? "F" : "M", targetType);
			}
			else if (conversion.Kind == ConversionKind.ExplicitNumeric && sourceConstantValueOpt != null && sourceConstantValueOpt != ConstantValue.Bad && ConversionsBase.HasImplicitConstantExpressionConversion(new BoundLiteral(syntax, ConstantValue.Bad, sourceType), targetType))
			{
				Error(diagnostics, ErrorCode.ERR_ConstOutOfRange, syntax, sourceConstantValueOpt.Value, targetType);
			}
			else
			{
				SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(compilation, sourceType, targetType);
				Error(diagnostics, ErrorCode.ERR_NoImplicitConvCast, syntax, symbolDistinguisher.First, symbolDistinguisher.Second);
			}
		}
		else if (conversion.ResultKind == LookupResultKind.OverloadResolutionFailure)
		{
			ImmutableArray<MethodSymbol> originalUserDefinedConversions = conversion.OriginalUserDefinedConversions;
			if (originalUserDefinedConversions.Length > 1)
			{
				Error(diagnostics, ErrorCode.ERR_AmbigUDConv, syntax, originalUserDefinedConversions[0], originalUserDefinedConversions[1], sourceType, targetType);
			}
			else
			{
				SymbolDistinguisher symbolDistinguisher2 = new SymbolDistinguisher(compilation, sourceType, targetType);
				Error(diagnostics, ErrorCode.ERR_NoImplicitConv, syntax, symbolDistinguisher2.First, symbolDistinguisher2.Second);
			}
		}
		else if (sourceType == targetType)
		{
			Error(diagnostics, ErrorCode.ERR_NoImplicitConv, syntax, sourceType, targetType);
		}
		else
		{
			SymbolDistinguisher symbolDistinguisher3 = new SymbolDistinguisher(compilation, sourceType, targetType);
			Error(diagnostics, ErrorCode.ERR_NoImplicitConv, syntax, symbolDistinguisher3.First, symbolDistinguisher3.Second);
		}
	}

	protected void GenerateImplicitConversionError(DiagnosticBag diagnostics, SyntaxNode syntax, Conversion conversion, BoundExpression operand, TypeSymbol targetType)
	{
		if (targetType.TypeKind == TypeKind.Error)
		{
			return;
		}
		switch (operand.Kind)
		{
		case BoundKind.BadExpression:
			return;
		case BoundKind.UnboundLambda:
			GenerateAnonymousFunctionConversionError(diagnostics, syntax, (UnboundLambda)operand, targetType);
			return;
		case BoundKind.TupleLiteral:
		{
			BoundTupleLiteral boundTupleLiteral = (BoundTupleLiteral)operand;
			ImmutableArray<TypeSymbol> elementTypes = default;
			if (targetType.TryGetElementTypesIfTupleOrCompatible(out elementTypes) && elementTypes.Length == boundTupleLiteral.Arguments.Length)
			{
				GenerateImplicitConversionErrorsForTupleLiteralArguments(diagnostics, boundTupleLiteral.Arguments, elementTypes);
				return;
			}
			if ((object)boundTupleLiteral.Type == null)
			{
				Error(diagnostics, ErrorCode.ERR_ConversionNotTupleCompatible, syntax, boundTupleLiteral.Arguments.Length, targetType);
				return;
			}
			break;
		}
		case BoundKind.MethodGroup:
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)operand;
			if (!Conversions.ReportDelegateMethodGroupDiagnostics(this, boundMethodGroup, targetType, diagnostics))
			{
				SyntaxNode syntaxNode = syntax;
				while (syntaxNode.Kind() == SyntaxKind.ParenthesizedExpression)
				{
					syntaxNode = ((ParenthesizedExpressionSyntax)syntaxNode).Expression;
				}
				if (syntaxNode.Kind() == SyntaxKind.SimpleMemberAccessExpression || syntaxNode.Kind() == SyntaxKind.PointerMemberAccessExpression)
				{
					syntaxNode = ((MemberAccessExpressionSyntax)syntaxNode).Name;
				}
				Location location = syntaxNode.Location;
				if (!ReportDelegateInvokeUseSiteDiagnostic(diagnostics, targetType, location))
				{
					Error(diagnostics, targetType.IsDelegateType() ? ErrorCode.ERR_MethDelegateMismatch : ErrorCode.ERR_MethGrpToNonDel, location, boundMethodGroup.Name, targetType);
				}
			}
			return;
		}
		case BoundKind.Literal:
			if (operand.IsLiteralNull())
			{
				if (targetType.TypeKind == TypeKind.TypeParameter)
				{
					Error(diagnostics, ErrorCode.ERR_TypeVarCantBeNull, syntax, targetType);
					return;
				}
				if (targetType.IsValueType)
				{
					Error(diagnostics, ErrorCode.ERR_ValueCantBeNull, syntax, targetType);
					return;
				}
			}
			break;
		case BoundKind.StackAllocArrayCreation:
		{
			BoundStackAllocArrayCreation boundStackAllocArrayCreation = (BoundStackAllocArrayCreation)operand;
			Error(diagnostics, ErrorCode.ERR_StackAllocConversionNotPossible, syntax, boundStackAllocArrayCreation.ElementType, targetType);
			return;
		}
		}
		TypeSymbol type = operand.Type;
		if ((object)type != null)
		{
			GenerateImplicitConversionError(diagnostics, Compilation, syntax, conversion, type, targetType, operand.ConstantValue);
		}
	}

	private void GenerateImplicitConversionErrorsForTupleLiteralArguments(DiagnosticBag diagnostics, ImmutableArray<BoundExpression> tupleArguments, ImmutableArray<TypeSymbol> targetElementTypes)
	{
		_ = tupleArguments.Length;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		for (int i = 0; i < targetElementTypes.Length; i++)
		{
			BoundExpression boundExpression = tupleArguments[i];
			TypeSymbol typeSymbol = targetElementTypes[i];
			Conversion conversion = Conversions.ClassifyImplicitConversionFromExpression(boundExpression, typeSymbol, ref useSiteDiagnostics);
			if (!conversion.IsValid)
			{
				GenerateImplicitConversionError(diagnostics, boundExpression.Syntax, conversion, boundExpression, typeSymbol);
			}
		}
	}

	private BoundStatement BindIfStatement(IfStatementSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression condition = BindBooleanExpression(node.Condition, diagnostics);
		BoundStatement consequence = BindPossibleEmbeddedStatement(node.Statement, diagnostics);
		BoundStatement alternativeOpt = ((node.Else == null) ? null : BindPossibleEmbeddedStatement(node.Else.Statement, diagnostics));
		return new BoundIfStatement(node, condition, consequence, alternativeOpt);
	}

	internal BoundExpression BindBooleanExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node, diagnostics, BindValueKind.RValue);
		NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Boolean, diagnostics, node);
		if (boundExpression.HasAnyErrors)
		{
			return BoundConversion.Synthesized(node, boundExpression, Conversion.NoConversion, @checked: false, explicitCastInCode: false, null, specialType, hasErrors: true);
		}
		if (boundExpression.HasDynamicType())
		{
			return new BoundUnaryOperator(node, UnaryOperatorKind.DynamicTrue, boundExpression, null, null, LookupResultKind.Viable, specialType)
			{
				WasCompilerGenerated = true
			};
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(boundExpression, specialType, ref useSiteDiagnostics);
		diagnostics.Add(boundExpression.Syntax, useSiteDiagnostics);
		if (conversion.IsImplicit)
		{
			if (conversion.Kind == ConversionKind.Identity)
			{
				if (boundExpression.Kind == BoundKind.AssignmentOperator)
				{
					BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)boundExpression;
					if (boundAssignmentOperator.Right.Kind == BoundKind.Literal && boundAssignmentOperator.Right.ConstantValue.Discriminator == ConstantValueTypeDiscriminator.Boolean)
					{
						Error(diagnostics, ErrorCode.WRN_IncorrectBooleanAssg, boundAssignmentOperator.Syntax);
					}
				}
				return boundExpression;
			}
			return CreateConversion(boundExpression.Syntax, boundExpression, conversion, isCast: false, wasCompilerGenerated: true, specialType, diagnostics);
		}
		UnaryOperatorAnalysisResult unaryOperatorAnalysisResult = UnaryOperatorOverloadResolution(UnaryOperatorKind.True, boundExpression, node, diagnostics, out var resultKind, out var originalUserDefinedOperators);
		if (!unaryOperatorAnalysisResult.HasValue)
		{
			GenerateImplicitConversionError(diagnostics, node, conversion, boundExpression, specialType);
			return BoundConversion.Synthesized(node, boundExpression, Conversion.NoConversion, @checked: false, explicitCastInCode: false, null, specialType, hasErrors: true);
		}
		UnaryOperatorSignature signature = unaryOperatorAnalysisResult.Signature;
		BoundExpression operand = CreateConversion(node, boundExpression, unaryOperatorAnalysisResult.Conversion, isCast: false, unaryOperatorAnalysisResult.Signature.OperandType, diagnostics);
		return new BoundUnaryOperator(node, signature.Kind, operand, null, signature.Method, resultKind, originalUserDefinedOperators, signature.ReturnType)
		{
			WasCompilerGenerated = true
		};
	}

	private BoundStatement BindSwitchStatement(SwitchStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return binder.BindSwitchExpressionAndSections(node, binder, diagnostics);
	}

	internal virtual BoundStatement BindSwitchExpressionAndSections(SwitchStatementSyntax node, Binder originalBinder, DiagnosticBag diagnostics)
	{
		return Next.BindSwitchExpressionAndSections(node, originalBinder, diagnostics);
	}

	internal virtual void BindPatternSwitchLabelForInference(CasePatternSwitchLabelSyntax node, DiagnosticBag diagnostics)
	{
		Next.BindPatternSwitchLabelForInference(node, diagnostics);
	}

	private BoundStatement BindWhile(WhileStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return binder.BindWhileParts(diagnostics, binder);
	}

	internal virtual BoundWhileStatement BindWhileParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindWhileParts(diagnostics, originalBinder);
	}

	private BoundStatement BindDo(DoStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return binder.BindDoParts(diagnostics, binder);
	}

	internal virtual BoundDoStatement BindDoParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindDoParts(diagnostics, originalBinder);
	}

	internal BoundForStatement BindFor(ForStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return binder.BindForParts(diagnostics, binder);
	}

	internal virtual BoundForStatement BindForParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindForParts(diagnostics, originalBinder);
	}

	internal BoundStatement BindForOrUsingOrFixedDeclarations(VariableDeclarationSyntax nodeOpt, LocalDeclarationKind localKind, DiagnosticBag diagnostics, out ImmutableArray<BoundLocalDeclaration> declarations)
	{
		if (nodeOpt == null)
		{
			declarations = ImmutableArray<BoundLocalDeclaration>.Empty;
			return null;
		}
		TypeSyntax typeSyntax = nodeOpt.Type;
		if (localKind == LocalDeclarationKind.RegularVariable)
		{
			typeSyntax = typeSyntax.SkipRef(out var _);
		}
		TypeSymbol declTypeOpt = BindTypeOrVarKeyword(typeSyntax, diagnostics, out var isVar, out var alias);
		SeparatedSyntaxList<VariableDeclaratorSyntax> variables = nodeOpt.Variables;
		int count = variables.Count;
		if (isVar && count > 1)
		{
			Error(diagnostics, ErrorCode.ERR_ImplicitlyTypedVariableMultipleDeclarator, nodeOpt);
		}
		BoundLocalDeclaration[] array = new BoundLocalDeclaration[count];
		for (int i = 0; i < count; i++)
		{
			VariableDeclaratorSyntax declarator = variables[i];
			BoundLocalDeclaration boundLocalDeclaration = BindVariableDeclaration(localKind, isVar, declarator, typeSyntax, declTypeOpt, alias, diagnostics);
			array[i] = boundLocalDeclaration;
		}
		declarations = array.AsImmutableOrNull();
		if (count != 1)
		{
			return new BoundMultipleLocalDeclarations(nodeOpt, declarations);
		}
		return declarations[0];
	}

	internal BoundStatement BindStatementExpressionList(SeparatedSyntaxList<ExpressionSyntax> statements, DiagnosticBag diagnostics)
	{
		int count = statements.Count;
		switch (count)
		{
		case 0:
			return null;
		case 1:
		{
			ExpressionSyntax expressionSyntax2 = statements[0];
			return BindExpressionStatement(expressionSyntax2, expressionSyntax2, allowsAnyExpression: false, diagnostics);
		}
		default:
		{
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			for (int i = 0; i < count; i++)
			{
				ExpressionSyntax expressionSyntax = statements[i];
				BoundExpressionStatement item = BindExpressionStatement(expressionSyntax, expressionSyntax, allowsAnyExpression: false, diagnostics);
				instance.Add(item);
			}
			return BoundStatementList.Synthesized(statements.Node, instance.ToImmutableAndFree());
		}
		}
	}

	private BoundStatement BindForEach(CommonForEachStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return GetBinder(node.Expression).WrapWithVariablesIfAny(node.Expression, binder.BindForEachParts(diagnostics, binder));
	}

	internal virtual BoundStatement BindForEachParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindForEachParts(diagnostics, originalBinder);
	}

	internal virtual BoundStatement BindForEachDeconstruction(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindForEachDeconstruction(diagnostics, originalBinder);
	}

	private BoundStatement BindBreak(BreakStatementSyntax node, DiagnosticBag diagnostics)
	{
		GeneratedLabelSymbol breakLabel = BreakLabel;
		if ((object)breakLabel == null)
		{
			Error(diagnostics, ErrorCode.ERR_NoBreakOrCont, node);
			return new BoundBadStatement(node, ImmutableArray<BoundNode>.Empty, hasErrors: true);
		}
		return new BoundBreakStatement(node, breakLabel);
	}

	private BoundStatement BindContinue(ContinueStatementSyntax node, DiagnosticBag diagnostics)
	{
		GeneratedLabelSymbol continueLabel = ContinueLabel;
		if ((object)continueLabel == null)
		{
			Error(diagnostics, ErrorCode.ERR_NoBreakOrCont, node);
			return new BoundBadStatement(node, ImmutableArray<BoundNode>.Empty, hasErrors: true);
		}
		return new BoundContinueStatement(node, continueLabel);
	}

	private static SwitchBinder GetSwitchBinder(Binder binder)
	{
		SwitchBinder switchBinder = binder as SwitchBinder;
		while (binder != null && switchBinder == null)
		{
			binder = binder.Next;
			switchBinder = binder as SwitchBinder;
		}
		return switchBinder;
	}

	protected static bool IsInAsyncMethod(MethodSymbol method)
	{
		return method?.IsAsync ?? false;
	}

	protected bool IsInAsyncMethod()
	{
		return IsInAsyncMethod(ContainingMemberOrLambda as MethodSymbol);
	}

	protected bool IsTaskReturningAsyncMethod()
	{
		Symbol containingMemberOrLambda = ContainingMemberOrLambda;
		if ((object)containingMemberOrLambda != null && containingMemberOrLambda.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)containingMemberOrLambda).IsTaskReturningAsync(Compilation);
		}
		return false;
	}

	protected bool IsGenericTaskReturningAsyncMethod()
	{
		Symbol containingMemberOrLambda = ContainingMemberOrLambda;
		if ((object)containingMemberOrLambda != null && containingMemberOrLambda.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)containingMemberOrLambda).IsGenericTaskReturningAsync(Compilation);
		}
		return false;
	}

	protected virtual TypeSymbol GetCurrentReturnType(out RefKind refKind)
	{
		if (ContainingMemberOrLambda is MethodSymbol methodSymbol)
		{
			refKind = methodSymbol.RefKind;
			return methodSymbol.ReturnType;
		}
		refKind = RefKind.None;
		return null;
	}

	private BoundStatement BindReturn(ReturnStatementSyntax syntax, DiagnosticBag diagnostics)
	{
		RefKind refKind = RefKind.None;
		ExpressionSyntax expressionSyntax = syntax.Expression?.CheckAndUnwrapRefExpression(diagnostics, out refKind);
		BoundExpression boundExpression = null;
		if (expressionSyntax != null)
		{
			BindValueKind requiredReturnValueKind = GetRequiredReturnValueKind(refKind);
			boundExpression = BindValue(expressionSyntax, diagnostics, requiredReturnValueKind);
			boundExpression = ValidateEscape(boundExpression, 0u, refKind != RefKind.None, diagnostics);
		}
		else
		{
			SynthesizedInteractiveInitializerMethod synthesizedInteractiveInitializerMethod = ContainingMemberOrLambda as SynthesizedInteractiveInitializerMethod;
			if (synthesizedInteractiveInitializerMethod != null)
			{
				boundExpression = new BoundDefaultExpression(synthesizedInteractiveInitializerMethod.GetNonNullSyntaxNode(), synthesizedInteractiveInitializerMethod.ResultType);
			}
		}
		TypeSymbol currentReturnType = GetCurrentReturnType(out var refKind2);
		if (IsDirectlyInIterator)
		{
			diagnostics.Add(ErrorCode.ERR_ReturnInIterator, syntax.ReturnKeyword.GetLocation());
		}
		else if (IsInAsyncMethod() && refKind != RefKind.None)
		{
			diagnostics.Add(ErrorCode.ERR_MustNotHaveRefReturn, syntax.ReturnKeyword.GetLocation());
		}
		else if ((object)currentReturnType != null && refKind != RefKind.None != (refKind2 != RefKind.None))
		{
			ErrorCode code = ((refKind != RefKind.None) ? ErrorCode.ERR_MustNotHaveRefReturn : ErrorCode.ERR_MustHaveRefReturn);
			diagnostics.Add(code, syntax.ReturnKeyword.GetLocation());
		}
		if (boundExpression != null && (boundExpression.HasErrors || ((object)boundExpression.Type != null && boundExpression.Type.IsErrorType())))
		{
			return new BoundReturnStatement(syntax, refKind, boundExpression, hasErrors: true);
		}
		if ((object)currentReturnType != null)
		{
			if (currentReturnType.SpecialType == SpecialType.System_Void || IsTaskReturningAsyncMethod())
			{
				if (boundExpression != null)
				{
					Symbol containingMemberOrLambda = ContainingMemberOrLambda;
					if (containingMemberOrLambda is LambdaSymbol)
					{
						ErrorCode code2 = ((currentReturnType.SpecialType == SpecialType.System_Void) ? ErrorCode.ERR_RetNoObjectRequiredLambda : ErrorCode.ERR_TaskRetNoObjectRequiredLambda);
						Error(diagnostics, code2, syntax.ReturnKeyword);
					}
					else
					{
						ErrorCode code3 = ((currentReturnType.SpecialType == SpecialType.System_Void) ? ErrorCode.ERR_RetNoObjectRequired : ErrorCode.ERR_TaskRetNoObjectRequired);
						Error(diagnostics, code3, syntax.ReturnKeyword, containingMemberOrLambda);
					}
				}
			}
			else if (boundExpression == null)
			{
				TypeSymbol typeSymbol = (IsGenericTaskReturningAsyncMethod() ? currentReturnType.GetMemberTypeArgumentsNoUseSiteDiagnostics().Single() : currentReturnType);
				Error(diagnostics, ErrorCode.ERR_RetObjectRequired, syntax.ReturnKeyword, typeSymbol);
			}
			else
			{
				boundExpression = CreateReturnConversion(syntax, diagnostics, boundExpression, refKind2, currentReturnType);
			}
		}
		else if ((object)boundExpression?.Type != null && boundExpression.Type.SpecialType == SpecialType.System_Void)
		{
			Error(diagnostics, ErrorCode.ERR_CantReturnVoid, expressionSyntax);
		}
		return new BoundReturnStatement(syntax, refKind, boundExpression);
	}

	internal BoundExpression CreateReturnConversion(SyntaxNode syntax, DiagnosticBag diagnostics, BoundExpression argument, RefKind returnRefKind, TypeSymbol returnType)
	{
		bool flag = false;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion;
		if (IsInAsyncMethod())
		{
			if (!IsGenericTaskReturningAsyncMethod())
			{
				conversion = Conversion.NoConversion;
				flag = true;
			}
			else
			{
				returnType = returnType.GetMemberTypeArgumentsNoUseSiteDiagnostics().Single();
				conversion = Conversions.ClassifyConversionFromExpression(argument, returnType, ref useSiteDiagnostics);
			}
		}
		else
		{
			conversion = Conversions.ClassifyConversionFromExpression(argument, returnType, ref useSiteDiagnostics);
		}
		diagnostics.Add(syntax, useSiteDiagnostics);
		if (!argument.HasAnyErrors)
		{
			if (returnRefKind != RefKind.None)
			{
				if (conversion.Kind == ConversionKind.Identity)
				{
					return argument;
				}
				Error(diagnostics, ErrorCode.ERR_RefReturnMustHaveIdentityConversion, argument.Syntax, returnType);
			}
			else if ((!conversion.IsImplicit || !conversion.IsValid) && !flag)
			{
				if (IsGenericTaskReturningAsyncMethod() && argument.Type == GetCurrentReturnType(out var _))
				{
					Error(diagnostics, ErrorCode.ERR_BadAsyncReturnExpression, argument.Syntax, returnType);
				}
				else
				{
					GenerateImplicitConversionError(diagnostics, argument.Syntax, conversion, argument, returnType);
					if (ContainingMemberOrLambda is LambdaSymbol)
					{
						ReportCantConvertLambdaReturn(argument.Syntax, diagnostics);
					}
				}
			}
		}
		return CreateConversion(argument.Syntax, argument, conversion, isCast: false, returnType, diagnostics);
	}

	private BoundTryStatement BindTryStatement(TryStatementSyntax node, DiagnosticBag diagnostics)
	{
		BoundBlock tryBlock = BindEmbeddedBlock(node.Block, diagnostics);
		ImmutableArray<BoundCatchBlock> catchBlocks = BindCatchBlocks(node.Catches, diagnostics);
		BoundBlock finallyBlockOpt = ((node.Finally != null) ? BindEmbeddedBlock(node.Finally.Block, diagnostics) : null);
		return new BoundTryStatement(node, tryBlock, catchBlocks, finallyBlockOpt);
	}

	private ImmutableArray<BoundCatchBlock> BindCatchBlocks(SyntaxList<CatchClauseSyntax> catchClauses, DiagnosticBag diagnostics)
	{
		int count = catchClauses.Count;
		if (count == 0)
		{
			return ImmutableArray<BoundCatchBlock>.Empty;
		}
		ArrayBuilder<BoundCatchBlock> instance = ArrayBuilder<BoundCatchBlock>.GetInstance(count);
		bool flag = false;
		foreach (CatchClauseSyntax item2 in catchClauses)
		{
			if (flag)
			{
				diagnostics.Add(ErrorCode.ERR_TooManyCatches, item2.CatchKeyword.GetLocation());
			}
			BoundCatchBlock item = GetBinder(item2).BindCatchBlock(item2, instance, diagnostics);
			instance.Add(item);
			flag |= item2.Declaration == null && item2.Filter == null;
		}
		return instance.ToImmutableAndFree();
	}

	private BoundCatchBlock BindCatchBlock(CatchClauseSyntax node, ArrayBuilder<BoundCatchBlock> previousBlocks, DiagnosticBag diagnostics)
	{
		bool flag = false;
		TypeSymbol typeSymbol = null;
		BoundExpression boundExpression = null;
		CatchDeclarationSyntax declaration = node.Declaration;
		if (declaration != null)
		{
			typeSymbol = BindType(declaration.Type, diagnostics);
			if (typeSymbol.IsErrorType())
			{
				flag = true;
			}
			else
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				TypeSymbol type = typeSymbol.EffectiveType(ref useSiteDiagnostics);
				if (!Compilation.IsExceptionType(type, ref useSiteDiagnostics))
				{
					Error(diagnostics, ErrorCode.ERR_BadExceptionType, declaration.Type);
					flag = true;
					diagnostics.Add(declaration.Type, useSiteDiagnostics);
				}
			}
		}
		CatchFilterClauseSyntax filter = node.Filter;
		if (filter != null)
		{
			boundExpression = GetBinder(filter).BindCatchFilter(filter, diagnostics);
			flag |= boundExpression.HasAnyErrors;
		}
		if (!flag)
		{
			foreach (BoundCatchBlock previousBlock in previousBlocks)
			{
				TypeSymbol exceptionTypeOpt = previousBlock.ExceptionTypeOpt;
				if (previousBlock.ExceptionFilterOpt != null || (object)exceptionTypeOpt == null || exceptionTypeOpt.IsErrorType())
				{
					continue;
				}
				if ((object)typeSymbol != null)
				{
					HashSet<DiagnosticInfo> useSiteDiagnostics2 = null;
					if (Conversions.HasIdentityOrImplicitReferenceConversion(typeSymbol, exceptionTypeOpt, ref useSiteDiagnostics2))
					{
						Error(diagnostics, ErrorCode.ERR_UnreachableCatch, declaration.Type, exceptionTypeOpt);
						diagnostics.Add(declaration.Type, useSiteDiagnostics2);
						flag = true;
						break;
					}
					diagnostics.Add(declaration.Type, useSiteDiagnostics2);
				}
				else if (exceptionTypeOpt == Compilation.GetWellKnownType(WellKnownType.System_Exception) && Compilation.SourceAssembly.RuntimeCompatibilityWrapNonExceptionThrows)
				{
					Error(diagnostics, ErrorCode.WRN_UnreachableGeneralCatch, node.CatchKeyword);
					break;
				}
			}
		}
		ImmutableArray<LocalSymbol> declaredLocalsForScope = GetBinder(node).GetDeclaredLocalsForScope(node);
		BoundExpression exceptionSourceOpt = null;
		LocalSymbol localSymbol = declaredLocalsForScope.FirstOrDefault();
		if ((object)localSymbol != null && localSymbol.DeclarationKind == LocalDeclarationKind.CatchVariable)
		{
			flag |= ValidateDeclarationNameConflictsInScope(localSymbol, diagnostics);
			exceptionSourceOpt = new BoundLocal(declaration, localSymbol, null, localSymbol.Type);
		}
		BoundBlock body = BindEmbeddedBlock(node.Block, diagnostics);
		return new BoundCatchBlock(node, declaredLocalsForScope, exceptionSourceOpt, typeSymbol, boundExpression, body, flag);
	}

	private BoundExpression BindCatchFilter(CatchFilterClauseSyntax filter, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindBooleanExpression(filter.FilterExpression, diagnostics);
		if (boundExpression.ConstantValue != null)
		{
			ErrorCode code;
			if (boundExpression.ConstantValue.BooleanValue)
			{
				code = ErrorCode.WRN_FilterIsConstantTrue;
			}
			else
			{
				code = ((filter.Parent.Parent is TryStatementSyntax { Catches: { Count: 1 }, Finally: null }) ? ErrorCode.WRN_FilterIsConstantFalseRedundantTryCatch : ErrorCode.WRN_FilterIsConstantFalse);
			}
			Error(diagnostics, code, filter.FilterExpression);
		}
		return boundExpression;
	}

	private void ReportCantConvertLambdaReturn(SyntaxNode syntax, DiagnosticBag diagnostics)
	{
		if (ContainingMemberOrLambda is LambdaSymbol lambdaSymbol)
		{
			Location location = getLocationForDiagnostics(syntax);
			if (IsInAsyncMethod())
			{
				Error(diagnostics, ErrorCode.ERR_CantConvAsyncAnonFuncReturns, location, lambdaSymbol.MessageID.Localize(), lambdaSymbol.ReturnType);
			}
			else
			{
				Error(diagnostics, ErrorCode.ERR_CantConvAnonMethReturns, location, lambdaSymbol.MessageID.Localize());
			}
		}
		static Location getLocationForDiagnostics(SyntaxNode node)
		{
			if (node != null)
			{
				if (node is LambdaExpressionSyntax lambdaExpressionSyntax)
				{
					LambdaExpressionSyntax lambdaExpressionSyntax2 = lambdaExpressionSyntax;
					return Location.Create(lambdaExpressionSyntax2.SyntaxTree, TextSpan.FromBounds(lambdaExpressionSyntax2.SpanStart, lambdaExpressionSyntax2.ArrowToken.Span.End));
				}
				if (node is AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax)
				{
					AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax2 = anonymousMethodExpressionSyntax;
					return Location.Create(anonymousMethodExpressionSyntax2.SyntaxTree, TextSpan.FromBounds(anonymousMethodExpressionSyntax2.SpanStart, anonymousMethodExpressionSyntax2.ParameterList?.Span.End ?? anonymousMethodExpressionSyntax2.DelegateKeyword.Span.End));
				}
			}
			return node.Location;
		}
	}

	private static bool IsValidStatementExpression(SyntaxNode syntax, BoundExpression expression)
	{
		if (!SyntaxFacts.IsStatementExpression(syntax))
		{
			return false;
		}
		if (expression.Kind == BoundKind.DelegateCreationExpression || expression.Kind == BoundKind.NameOfOperator)
		{
			return false;
		}
		return true;
	}

	internal BoundBlock CreateBlockFromExpression(CSharpSyntaxNode node, ImmutableArray<LocalSymbol> locals, RefKind refKind, BoundExpression expression, ExpressionSyntax expressionSyntax, DiagnosticBag diagnostics)
	{
		TypeSymbol currentReturnType = GetCurrentReturnType(out var refKind2);
		SyntaxNode syntaxNode = expressionSyntax ?? expression.Syntax;
		BoundStatement item;
		if (IsInAsyncMethod() && refKind != RefKind.None)
		{
			Error(diagnostics, ErrorCode.ERR_MustNotHaveRefReturn, syntaxNode);
			item = new BoundReturnStatement(syntaxNode, refKind, expression)
			{
				WasCompilerGenerated = true
			};
		}
		else if ((object)currentReturnType != null)
		{
			if (refKind != RefKind.None != (refKind2 != RefKind.None) && expression.Kind != BoundKind.ThrowExpression)
			{
				ErrorCode code = ((refKind != RefKind.None) ? ErrorCode.ERR_MustNotHaveRefReturn : ErrorCode.ERR_MustHaveRefReturn);
				Error(diagnostics, code, syntaxNode);
				item = new BoundReturnStatement(syntaxNode, RefKind.None, expression)
				{
					WasCompilerGenerated = true
				};
			}
			else if (currentReturnType.SpecialType == SpecialType.System_Void || IsTaskReturningAsyncMethod())
			{
				bool hasErrors = false;
				if (expressionSyntax == null || !IsValidExpressionBody(expressionSyntax, expression))
				{
					Error(diagnostics, ErrorCode.ERR_IllegalStatement, syntaxNode);
					hasErrors = true;
				}
				BoundExpressionStatement boundExpressionStatement = new BoundExpressionStatement(syntaxNode, expression, hasErrors);
				CheckForUnobservedAwaitable(expression, diagnostics);
				item = boundExpressionStatement;
			}
			else
			{
				expression = CreateReturnConversion(syntaxNode, diagnostics, expression, refKind, currentReturnType);
				item = new BoundReturnStatement(syntaxNode, refKind2, expression)
				{
					WasCompilerGenerated = true
				};
			}
		}
		else
		{
			TypeSymbol type = expression.Type;
			item = (((object)type == null || type.SpecialType != SpecialType.System_Void) ? ((BoundStatement)new BoundReturnStatement(syntaxNode, refKind, expression)
			{
				WasCompilerGenerated = true
			}) : ((BoundStatement)new BoundExpressionStatement(syntaxNode, expression)
			{
				WasCompilerGenerated = true
			}));
		}
		return new BoundBlock(node, locals, ImmutableArray.Create(item))
		{
			WasCompilerGenerated = (node.Kind() != SyntaxKind.ArrowExpressionClause)
		};
	}

	private static bool IsValidExpressionBody(SyntaxNode expressionSyntax, BoundExpression expression)
	{
		if (!IsValidStatementExpression(expressionSyntax, expression))
		{
			return expressionSyntax.Kind() == SyntaxKind.ThrowExpression;
		}
		return true;
	}

	internal virtual BoundBlock BindExpressionBodyAsBlock(ArrowExpressionClauseSyntax expressionBody, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(expressionBody);
		RefKind refKind = RefKind.None;
		ExpressionSyntax expressionSyntax = expressionBody.Expression.CheckAndUnwrapRefExpression(diagnostics, out refKind);
		BindValueKind requiredReturnValueKind = GetRequiredReturnValueKind(refKind);
		BoundExpression expr = binder.BindValue(expressionSyntax, diagnostics, requiredReturnValueKind);
		expr = ValidateEscape(expr, 0u, refKind != RefKind.None, diagnostics);
		return binder.CreateBlockFromExpression(expressionBody, binder.GetDeclaredLocalsForScope(expressionBody), refKind, expr, expressionSyntax, diagnostics);
	}

	public BoundBlock BindLambdaExpressionAsBlock(ExpressionSyntax body, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(body);
		ExpressionSyntax expressionSyntax = body.CheckAndUnwrapRefExpression(diagnostics, out var refKind);
		BindValueKind requiredReturnValueKind = GetRequiredReturnValueKind(refKind);
		BoundExpression expr = binder.BindValue(expressionSyntax, diagnostics, requiredReturnValueKind);
		expr = ValidateEscape(expr, 0u, refKind != RefKind.None, diagnostics);
		return binder.CreateBlockFromExpression(body, binder.GetDeclaredLocalsForScope(body), refKind, expr, expressionSyntax, diagnostics);
	}

	private BindValueKind GetRequiredReturnValueKind(RefKind refKind)
	{
		BindValueKind result = BindValueKind.RValue;
		if (refKind != RefKind.None)
		{
			GetCurrentReturnType(out var refKind2);
			result = ((refKind2 == RefKind.Ref) ? BindValueKind.RefReturn : BindValueKind.ReadonlyRef);
		}
		return result;
	}

	public virtual BoundNode BindMethodBody(CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		if (syntax != null)
		{
			if (syntax is BaseMethodDeclarationSyntax baseMethodDeclarationSyntax)
			{
				BaseMethodDeclarationSyntax baseMethodDeclarationSyntax2 = baseMethodDeclarationSyntax;
				if (baseMethodDeclarationSyntax2.Kind() == SyntaxKind.ConstructorDeclaration)
				{
					return BindConstructorBody((ConstructorDeclarationSyntax)baseMethodDeclarationSyntax2, diagnostics);
				}
				return BindMethodBody(baseMethodDeclarationSyntax2, baseMethodDeclarationSyntax2.Body, baseMethodDeclarationSyntax2.ExpressionBody, diagnostics);
			}
			if (syntax is AccessorDeclarationSyntax accessorDeclarationSyntax)
			{
				AccessorDeclarationSyntax accessorDeclarationSyntax2 = accessorDeclarationSyntax;
				return BindMethodBody(accessorDeclarationSyntax2, accessorDeclarationSyntax2.Body, accessorDeclarationSyntax2.ExpressionBody, diagnostics);
			}
			if (syntax is ArrowExpressionClauseSyntax arrowExpressionClauseSyntax)
			{
				ArrowExpressionClauseSyntax expressionBody = arrowExpressionClauseSyntax;
				return BindExpressionBodyAsBlock(expressionBody, diagnostics);
			}
		}
		throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
	}

	private BoundNode BindConstructorBody(ConstructorDeclarationSyntax constructor, DiagnosticBag diagnostics)
	{
		if (constructor.Initializer == null && constructor.Body == null && constructor.ExpressionBody == null)
		{
			return null;
		}
		Binder binder = GetBinder(constructor);
		return new BoundConstructorMethodBody(constructor, binder.GetDeclaredLocalsForScope(constructor), (constructor.Initializer == null) ? null : binder.BindConstructorInitializer(constructor.Initializer, diagnostics), (constructor.Body == null) ? null : ((BoundBlock)binder.BindStatement(constructor.Body, diagnostics)), (constructor.ExpressionBody == null) ? null : binder.BindExpressionBodyAsBlock(constructor.ExpressionBody, (constructor.Body == null) ? diagnostics : new DiagnosticBag()));
	}

	internal virtual BoundExpressionStatement BindConstructorInitializer(ConstructorInitializerSyntax initializer, DiagnosticBag diagnostics)
	{
		BoundExpression expression = GetBinder(initializer).BindConstructorInitializer(initializer.ArgumentList, (MethodSymbol)ContainingMember(), diagnostics);
		return new BoundExpressionStatement(initializer, expression);
	}

	private BoundNode BindMethodBody(CSharpSyntaxNode declaration, BlockSyntax blockBody, ArrowExpressionClauseSyntax expressionBody, DiagnosticBag diagnostics)
	{
		if (blockBody == null && expressionBody == null)
		{
			return null;
		}
		return new BoundNonConstructorMethodBody(declaration, (blockBody == null) ? null : ((BoundBlock)BindStatement(blockBody, diagnostics)), (expressionBody == null) ? null : BindExpressionBodyAsBlock(expressionBody, (blockBody == null) ? diagnostics : new DiagnosticBag()));
	}

	internal TypeSymbol BindTypeOrVarKeyword(TypeSyntax syntax, DiagnosticBag diagnostics, out bool isVar)
	{
		Symbol symbol = BindTypeOrAliasOrVarKeyword(syntax, diagnostics, out isVar);
		if (!isVar)
		{
			return (TypeSymbol)UnwrapAlias(symbol, diagnostics, syntax);
		}
		return null;
	}

	internal TypeSymbol BindTypeOrUnmanagedKeyword(TypeSyntax syntax, DiagnosticBag diagnostics, out bool isUnmanaged)
	{
		Symbol symbol = BindTypeOrAliasOrUnmanagedKeyword(syntax, diagnostics, out isUnmanaged);
		if (!isUnmanaged)
		{
			return (TypeSymbol)UnwrapAlias(symbol, diagnostics, syntax);
		}
		return null;
	}

	internal TypeSymbol BindTypeOrVarKeyword(TypeSyntax syntax, DiagnosticBag diagnostics, out bool isVar, out AliasSymbol alias)
	{
		Symbol symbol = BindTypeOrAliasOrVarKeyword(syntax, diagnostics, out isVar);
		if (isVar)
		{
			alias = null;
			return null;
		}
		return (TypeSymbol)UnwrapAlias(symbol, out alias, diagnostics, syntax);
	}

	private Symbol BindTypeOrAliasOrVarKeyword(TypeSyntax syntax, DiagnosticBag diagnostics, out bool isVar)
	{
		if (syntax.IsVar)
		{
			Symbol result = BindTypeOrAliasOrKeyword(syntax, diagnostics, out isVar);
			if (isVar)
			{
				CheckFeatureAvailability(syntax, MessageID.IDS_FeatureImplicitLocal, diagnostics);
			}
			return result;
		}
		isVar = false;
		return BindTypeOrAlias(syntax, diagnostics);
	}

	private Symbol BindTypeOrAliasOrUnmanagedKeyword(TypeSyntax syntax, DiagnosticBag diagnostics, out bool isUnmanaged)
	{
		if (syntax.IsUnmanaged)
		{
			Symbol result = BindTypeOrAliasOrKeyword(syntax, diagnostics, out isUnmanaged);
			if (isUnmanaged)
			{
				CheckFeatureAvailability(syntax, MessageID.IDS_FeatureUnmanagedGenericTypeConstraint, diagnostics);
			}
			return result;
		}
		isUnmanaged = false;
		return BindTypeOrAlias(syntax, diagnostics);
	}

	private Symbol BindTypeOrAliasOrKeyword(TypeSyntax syntax, DiagnosticBag diagnostics, out bool isKeyword)
	{
		string valueText = ((IdentifierNameSyntax)syntax).Identifier.ValueText;
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsInternal(instance, valueText, 0, null, LookupOptions.NamespacesOrTypesOnly, diagnose: false, ref useSiteDiagnostics);
		LookupResultKind kind = instance.Kind;
		Symbol symbol;
		if (kind != LookupResultKind.Empty)
		{
			if (kind == LookupResultKind.Viable)
			{
				DiagnosticBag instance2 = DiagnosticBag.GetInstance();
				symbol = ResultSymbol(instance, valueText, 0, syntax, instance2, suppressUseSiteDiagnostics: false, out var wasError);
				if (!wasError || !instance.IsSingleViable)
				{
					diagnostics.AddRange(instance2);
					instance2.Free();
					if (instance.IsSingleViable)
					{
						if (UnwrapAlias(symbol, diagnostics, syntax) is TypeSymbol)
						{
							isKeyword = false;
						}
						else
						{
							isKeyword = true;
							symbol = null;
						}
					}
					else
					{
						isKeyword = false;
					}
					goto IL_00b5;
				}
				instance2.Free();
			}
			isKeyword = true;
			symbol = null;
		}
		else
		{
			isKeyword = true;
			symbol = null;
		}
		goto IL_00b5;
		IL_00b5:
		instance.Free();
		return symbol;
	}

	internal TypeSymbol BindType(ExpressionSyntax syntax, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		Symbol symbol = BindTypeOrAlias(syntax, diagnostics, basesBeingResolved);
		return (TypeSymbol)UnwrapAlias(symbol, diagnostics, syntax, basesBeingResolved);
	}

	internal TypeSymbol BindType(ExpressionSyntax syntax, DiagnosticBag diagnostics, out AliasSymbol alias, ConsList<Symbol> basesBeingResolved = null)
	{
		Symbol symbol = BindTypeOrAlias(syntax, diagnostics, basesBeingResolved);
		return (TypeSymbol)UnwrapAlias(symbol, out alias, diagnostics, syntax, basesBeingResolved);
	}

	internal Symbol BindTypeOrAlias(ExpressionSyntax syntax, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		Symbol symbol = BindNamespaceOrTypeOrAliasSymbol(syntax, diagnostics, basesBeingResolved, basesBeingResolved != null);
		if (UnwrapAliasNoDiagnostics(symbol, basesBeingResolved) is TypeSymbol typeSymbol)
		{
			if ((object)typeSymbol == symbol)
			{
				ReportDiagnosticsIfObsolete(diagnostics, typeSymbol, syntax, hasBaseReceiver: false);
			}
			return symbol;
		}
		CSDiagnosticInfo errorInfo = diagnostics.Add(ErrorCode.ERR_BadSKknown, syntax.Location, syntax, symbol.GetKindText(), MessageID.IDS_SK_TYPE.Localize());
		return new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(symbol), symbol, LookupResultKind.NotATypeOrNamespace, errorInfo);
	}

	private NamespaceOrTypeSymbol GetContainingNamespaceOrType(Symbol symbol)
	{
		return symbol.ContainingNamespaceOrType() ?? Compilation.Assembly.GlobalNamespace;
	}

	internal Symbol BindNamespaceAliasSymbol(IdentifierNameSyntax node, DiagnosticBag diagnostics)
	{
		if (node.Identifier.Kind() == SyntaxKind.GlobalKeyword)
		{
			return Compilation.GlobalNamespaceAlias;
		}
		string valueText = node.Identifier.ValueText;
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsWithFallback(instance, valueText, 0, ref useSiteDiagnostics, null, LookupOptions.NamespaceAliasesOnly);
		diagnostics.Add(node, useSiteDiagnostics);
		Symbol result = ResultSymbol(instance, valueText, 0, node, diagnostics, suppressUseSiteDiagnostics: false, out var _, null, LookupOptions.NamespaceAliasesOnly);
		instance.Free();
		return result;
	}

	internal NamespaceOrTypeSymbol BindNamespaceOrTypeSymbol(ExpressionSyntax syntax, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		return BindNamespaceOrTypeSymbol(syntax, diagnostics, basesBeingResolved, basesBeingResolved != null);
	}

	internal NamespaceOrTypeSymbol BindNamespaceOrTypeSymbol(ExpressionSyntax syntax, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved, bool suppressUseSiteDiagnostics)
	{
		Symbol symbol = BindNamespaceOrTypeOrAliasSymbol(syntax, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics);
		return (NamespaceOrTypeSymbol)UnwrapAlias(symbol, diagnostics, syntax, basesBeingResolved);
	}

	internal Symbol BindNamespaceOrTypeOrAliasSymbol(ExpressionSyntax syntax, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved, bool suppressUseSiteDiagnostics)
	{
		switch (syntax.Kind())
		{
		case SyntaxKind.NullableType:
		{
			NamedTypeSymbol specialType = GetSpecialType(SpecialType.System_Nullable_T, diagnostics, syntax);
			TypeSyntax elementType = ((NullableTypeSyntax)syntax).ElementType;
			TypeSymbol typeSymbol2 = BindType(elementType, diagnostics, basesBeingResolved);
			NamedTypeSymbol namedTypeSymbol = specialType.Construct(typeSymbol2);
			if (ShouldCheckConstraints)
			{
				namedTypeSymbol.CheckConstraints(Compilation, Conversions, syntax.Location, diagnostics);
			}
			return namedTypeSymbol;
		}
		case SyntaxKind.PredefinedType:
			return BindPredefinedTypeSymbol((PredefinedTypeSyntax)syntax, diagnostics);
		case SyntaxKind.IdentifierName:
			return BindNonGenericSimpleNamespaceOrTypeOrAliasSymbol((IdentifierNameSyntax)syntax, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics, null);
		case SyntaxKind.GenericName:
			return BindGenericSimpleNamespaceOrTypeOrAliasSymbol((GenericNameSyntax)syntax, diagnostics, basesBeingResolved, null);
		case SyntaxKind.AliasQualifiedName:
		{
			AliasQualifiedNameSyntax aliasQualifiedNameSyntax = (AliasQualifiedNameSyntax)syntax;
			Symbol symbol = BindNamespaceAliasSymbol(aliasQualifiedNameSyntax.Alias, diagnostics);
			NamespaceOrTypeSymbol namespaceOrTypeSymbol = ((symbol is AliasSymbol aliasSymbol) ? aliasSymbol.Target : ((NamespaceOrTypeSymbol)symbol));
			if (namespaceOrTypeSymbol.Kind == SymbolKind.NamedType)
			{
				return new ExtendedErrorTypeSymbol(namespaceOrTypeSymbol, LookupResultKind.NotATypeOrNamespace, diagnostics.Add(ErrorCode.ERR_ColColWithTypeAlias, aliasQualifiedNameSyntax.Alias.Location, aliasQualifiedNameSyntax.Alias.Identifier.Text));
			}
			return BindSimpleNamespaceOrTypeOrAliasSymbol(aliasQualifiedNameSyntax.Name, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics, namespaceOrTypeSymbol);
		}
		case SyntaxKind.QualifiedName:
		{
			QualifiedNameSyntax qualifiedNameSyntax = (QualifiedNameSyntax)syntax;
			return BindQualifiedName(qualifiedNameSyntax.Left, qualifiedNameSyntax.Right, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics);
		}
		case SyntaxKind.SimpleMemberAccessExpression:
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)syntax;
			return BindQualifiedName(memberAccessExpressionSyntax.Expression, memberAccessExpressionSyntax.Name, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics);
		}
		case SyntaxKind.ArrayType:
		{
			ArrayTypeSyntax arrayTypeSyntax = (ArrayTypeSyntax)syntax;
			TypeSymbol typeSymbol3 = BindType(arrayTypeSyntax.ElementType, diagnostics, basesBeingResolved);
			if (typeSymbol3.IsStatic)
			{
				Error(diagnostics, ErrorCode.ERR_ArrayOfStaticClass, arrayTypeSyntax.ElementType, typeSymbol3);
			}
			if (typeSymbol3.IsRestrictedType())
			{
				Error(diagnostics, ErrorCode.ERR_ArrayElementCantBeRefAny, arrayTypeSyntax.ElementType, typeSymbol3);
			}
			for (int num = arrayTypeSyntax.RankSpecifiers.Count - 1; num >= 0; num--)
			{
				ArrayRankSpecifierSyntax arrayRankSpecifierSyntax = arrayTypeSyntax.RankSpecifiers[num];
				typeSymbol3 = ArrayTypeSymbol.CreateCSharpArray(Compilation.Assembly, typeSymbol3, ImmutableArray<CustomModifier>.Empty, arrayRankSpecifierSyntax.Rank);
			}
			return typeSymbol3;
		}
		case SyntaxKind.PointerType:
		{
			PointerTypeSyntax pointerTypeSyntax = (PointerTypeSyntax)syntax;
			TypeSymbol typeSymbol = BindType(pointerTypeSyntax.ElementType, diagnostics, basesBeingResolved);
			ReportUnsafeIfNotAllowed(pointerTypeSyntax, diagnostics);
			if (Flags.HasFlag(BinderFlags.GenericConstraintsClause) && typeSymbol.TypeKind == TypeKind.TypeParameter)
			{
				Error(diagnostics, ErrorCode.ERR_BadConstraintType, pointerTypeSyntax);
			}
			else if (typeSymbol.IsManagedType)
			{
				Error(diagnostics, ErrorCode.ERR_ManagedAddr, pointerTypeSyntax, typeSymbol);
			}
			return new PointerTypeSymbol(typeSymbol);
		}
		case SyntaxKind.OmittedTypeArgument:
			return BindTypeArgument((TypeSyntax)syntax, diagnostics, basesBeingResolved);
		case SyntaxKind.TupleType:
			return BindTupleType((TupleTypeSyntax)syntax, diagnostics);
		case SyntaxKind.RefType:
		{
			RefTypeSyntax refTypeSyntax = (RefTypeSyntax)syntax;
			SyntaxToken refKeyword = refTypeSyntax.RefKeyword;
			if (!syntax.HasErrors)
			{
				diagnostics.Add(ErrorCode.ERR_UnexpectedToken, refKeyword.GetLocation(), refKeyword.ToString());
			}
			return BindNamespaceOrTypeOrAliasSymbol(refTypeSyntax.Type, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
		}
	}

	private TypeSymbol BindTupleType(TupleTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		int count = syntax.Elements.Count;
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(count);
		ArrayBuilder<Location> instance2 = ArrayBuilder<Location>.GetInstance(count);
		ArrayBuilder<string> elementNames = null;
		PooledHashSet<string> instance3 = PooledHashSet<string>.GetInstance();
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			TupleElementSyntax tupleElementSyntax = syntax.Elements[i];
			TypeSymbol item = BindType(tupleElementSyntax.Type, diagnostics);
			instance.Add(item);
			string name = null;
			SyntaxToken identifier = tupleElementSyntax.Identifier;
			if (identifier.Kind() == SyntaxKind.IdentifierToken)
			{
				name = identifier.ValueText;
				flag = true;
				CheckTupleMemberName(name, i, identifier, diagnostics, instance3);
				instance2.Add(identifier.GetLocation());
			}
			else
			{
				instance2.Add(tupleElementSyntax.Location);
			}
			CollectTupleFieldMemberName(name, i, count, ref elementNames);
		}
		instance3.Free();
		if (flag)
		{
			GetSpecialType(SpecialType.System_String, diagnostics, syntax);
			if (!Compilation.HasTupleNamesAttributes)
			{
				CSDiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.ERR_TupleElementNamesAttributeMissing, AttributeDescription.TupleElementNamesAttribute.FullName);
				Error(diagnostics, info, syntax);
			}
		}
		ImmutableArray<TypeSymbol> elementTypes = instance.ToImmutableAndFree();
		ImmutableArray<Location> elementLocations = instance2.ToImmutableAndFree();
		if (elementTypes.Length < 2)
		{
			throw ExceptionUtilities.UnexpectedValue(elementTypes.Length);
		}
		return TupleTypeSymbol.Create(syntax.Location, elementTypes, elementLocations, elementNames?.ToImmutableAndFree() ?? default(ImmutableArray<string>), Compilation, ShouldCheckConstraints, default, syntax, diagnostics);
	}

	private static void CollectTupleFieldMemberName(string name, int elementIndex, int tupleSize, ref ArrayBuilder<string> elementNames)
	{
		if (elementNames != null)
		{
			elementNames.Add(name);
		}
		else if (name != null)
		{
			elementNames = ArrayBuilder<string>.GetInstance(tupleSize);
			for (int i = 0; i < elementIndex; i++)
			{
				elementNames.Add(null);
			}
			elementNames.Add(name);
		}
	}

	private static bool CheckTupleMemberName(string name, int index, SyntaxNodeOrToken syntax, DiagnosticBag diagnostics, PooledHashSet<string> uniqueFieldNames)
	{
		int num = TupleTypeSymbol.IsElementNameReserved(name);
		if (num == 0)
		{
			Error(diagnostics, ErrorCode.ERR_TupleReservedElementNameAnyPosition, syntax, name);
			return false;
		}
		if (num > 0 && num != index + 1)
		{
			Error(diagnostics, ErrorCode.ERR_TupleReservedElementName, syntax, name, num);
			return false;
		}
		if (!uniqueFieldNames.Add(name))
		{
			Error(diagnostics, ErrorCode.ERR_TupleDuplicateElementName, syntax);
			return false;
		}
		return true;
	}

	private Symbol BindPredefinedTypeSymbol(PredefinedTypeSyntax node, DiagnosticBag diagnostics)
	{
		return GetSpecialType(node.Keyword.Kind().GetSpecialType(), diagnostics, node);
	}

	private Symbol BindSimpleNamespaceOrTypeOrAliasSymbol(SimpleNameSyntax syntax, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved, bool suppressUseSiteDiagnostics, NamespaceOrTypeSymbol qualifierOpt = null)
	{
		return syntax.Kind() switch
		{
			SyntaxKind.IdentifierName => BindNonGenericSimpleNamespaceOrTypeOrAliasSymbol((IdentifierNameSyntax)syntax, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics, qualifierOpt), 
			SyntaxKind.GenericName => BindGenericSimpleNamespaceOrTypeOrAliasSymbol((GenericNameSyntax)syntax, diagnostics, basesBeingResolved, qualifierOpt), 
			_ => new ExtendedErrorTypeSymbol(qualifierOpt ?? Compilation.Assembly.GlobalNamespace, string.Empty, 0, null), 
		};
	}

	private static bool IsViableType(LookupResult result)
	{
		if (!result.IsMultiViable)
		{
			return false;
		}
		foreach (Symbol symbol in result.Symbols)
		{
			switch (symbol.Kind)
			{
			case SymbolKind.Alias:
				if (((AliasSymbol)symbol).Target.Kind == SymbolKind.NamedType)
				{
					return true;
				}
				break;
			case SymbolKind.NamedType:
			case SymbolKind.TypeParameter:
				return true;
			}
		}
		return false;
	}

	protected Symbol BindNonGenericSimpleNamespaceOrTypeOrAliasSymbol(IdentifierNameSyntax node, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved, bool suppressUseSiteDiagnostics, NamespaceOrTypeSymbol qualifierOpt, bool isNameofArgument = false, ArrayBuilder<Symbol> symbols = null)
	{
		string valueText = node.Identifier.ValueText;
		if (string.IsNullOrWhiteSpace(valueText))
		{
			return new ExtendedErrorTypeSymbol(Compilation.Assembly.GlobalNamespace, valueText, 0, new CSDiagnosticInfo(ErrorCode.ERR_SingleTypeNameNotFound));
		}
		ExtendedErrorTypeSymbol extendedErrorTypeSymbol = CreateErrorIfLookupOnTypeParameter(node.Parent, qualifierOpt, valueText, 0, diagnostics);
		if ((object)extendedErrorTypeSymbol != null)
		{
			return extendedErrorTypeSymbol;
		}
		LookupResult instance = LookupResult.GetInstance();
		LookupOptions options = (isNameofArgument ? (LookupOptions.AllMethodsOnArityZero | LookupOptions.AllNamedTypesOnArityZero) : GetSimpleNameLookupOptions(node, node.Identifier.IsVerbatimIdentifier()));
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsSimpleName(instance, qualifierOpt, valueText, 0, basesBeingResolved, options, diagnose: true, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		Symbol symbol;
		if ((object)qualifierOpt == null && (node.Parent == null || (node.Parent.Kind() != SyntaxKind.Attribute && SyntaxFacts.IsInTypeOnlyContext(node))) && node.Identifier.ValueText == "dynamic" && !IsViableType(instance) && ((CSharpParseOptions)node.SyntaxTree.Options).LanguageVersion >= MessageID.IDS_FeatureDynamic.RequiredVersion())
		{
			symbol = Compilation.DynamicType;
			ReportUseSiteDiagnosticForDynamic(diagnostics, node);
		}
		else if (isNameofArgument && instance.IsMultiViable && instance.Symbols.Count > 1)
		{
			symbols.AddRange(instance.Symbols);
			symbol = null;
		}
		else
		{
			symbol = ResultSymbol(instance, valueText, 0, node, diagnostics, suppressUseSiteDiagnostics, out var _, qualifierOpt, options);
			if (symbol.Kind == SymbolKind.Alias)
			{
				NamespaceOrTypeSymbol aliasTarget = ((AliasSymbol)symbol).GetAliasTarget(basesBeingResolved);
				if (aliasTarget.Kind == SymbolKind.NamedType && ((NamedTypeSymbol)aliasTarget).ContainsDynamic())
				{
					ReportUseSiteDiagnosticForDynamic(diagnostics, node);
				}
			}
			if (isNameofArgument)
			{
				symbols.Add(symbol);
				symbol = null;
			}
		}
		instance.Free();
		return symbol;
	}

	private void ReportUseSiteDiagnosticForDynamic(DiagnosticBag diagnostics, IdentifierNameSyntax node)
	{
		if (node.IsTypeInContextWhichNeedsDynamicAttribute())
		{
			if (!Compilation.HasDynamicEmitAttributes())
			{
				Symbol.ReportUseSiteDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_DynamicAttributeMissing, AttributeDescription.DynamicAttribute.FullName), diagnostics, node.Location);
			}
			GetSpecialType(SpecialType.System_Boolean, diagnostics, node);
		}
	}

	private static LookupOptions GetSimpleNameLookupOptions(NameSyntax node, bool isVerbatimIdentifier)
	{
		if (SyntaxFacts.IsAttributeName(node))
		{
			if (!isVerbatimIdentifier)
			{
				return LookupOptions.AttributeTypeOnly;
			}
			return LookupOptions.VerbatimNameAttributeTypeOnly;
		}
		return LookupOptions.NamespacesOrTypesOnly;
	}

	private static Symbol UnwrapAliasNoDiagnostics(Symbol symbol, ConsList<Symbol> basesBeingResolved = null)
	{
		if (symbol.Kind == SymbolKind.Alias)
		{
			return ((AliasSymbol)symbol).GetAliasTarget(basesBeingResolved);
		}
		return symbol;
	}

	private Symbol UnwrapAlias(Symbol symbol, DiagnosticBag diagnostics, SyntaxNode syntax, ConsList<Symbol> basesBeingResolved = null)
	{
		AliasSymbol alias;
		return UnwrapAlias(symbol, out alias, diagnostics, syntax, basesBeingResolved);
	}

	private Symbol UnwrapAlias(Symbol symbol, out AliasSymbol alias, DiagnosticBag diagnostics, SyntaxNode syntax, ConsList<Symbol> basesBeingResolved = null)
	{
		if (symbol.Kind == SymbolKind.Alias)
		{
			alias = (AliasSymbol)symbol;
			NamespaceOrTypeSymbol aliasTarget = alias.GetAliasTarget(basesBeingResolved);
			if (aliasTarget is TypeSymbol type)
			{
				TypeSymbolExtensions.VisitType(arg: (this, diagnostics, syntax), type: type, predicate: (TypeSymbol typePart, (Binder, DiagnosticBag diagnostics, SyntaxNode syntax) argTuple, bool isNested) =>
				{
					argTuple.Item1.ReportDiagnosticsIfObsolete(argTuple.diagnostics, typePart, argTuple.syntax, hasBaseReceiver: false);
					return false;
				});
			}
			return aliasTarget;
		}
		alias = null;
		return symbol;
	}

	private NamedTypeSymbol BindGenericSimpleNamespaceOrTypeOrAliasSymbol(GenericNameSyntax node, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved, NamespaceOrTypeSymbol qualifierOpt)
	{
		string valueText = node.Identifier.ValueText;
		SeparatedSyntaxList<TypeSyntax> arguments = node.TypeArgumentList.Arguments;
		bool isUnboundGenericName = node.IsUnboundGenericName;
		LookupOptions simpleNameLookupOptions = GetSimpleNameLookupOptions(node, isVerbatimIdentifier: false);
		NamedTypeSymbol namedTypeSymbol = LookupGenericTypeName(diagnostics, basesBeingResolved, qualifierOpt, node, valueText, node.Arity, simpleNameLookupOptions);
		NamedTypeSymbol namedTypeSymbol2;
		if (isUnboundGenericName)
		{
			if (!IsUnboundTypeAllowed(node))
			{
				if (!namedTypeSymbol.IsErrorType())
				{
					diagnostics.Add(ErrorCode.ERR_UnexpectedUnboundGenericName, node.Location);
				}
				namedTypeSymbol2 = namedTypeSymbol.Construct(UnboundArgumentErrorTypeSymbol.CreateTypeArguments(namedTypeSymbol.TypeParameters, node.Arity, null), unbound: false);
			}
			else
			{
				namedTypeSymbol2 = namedTypeSymbol.AsUnboundGenericType();
			}
		}
		else
		{
			namedTypeSymbol2 = ConstructNamedType(namedTypeSymbol, node, arguments, BindTypeArguments(arguments, diagnostics, basesBeingResolved), basesBeingResolved, diagnostics);
		}
		if (simpleNameLookupOptions.IsAttributeTypeLookup())
		{
			namedTypeSymbol2 = new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(namedTypeSymbol2), namedTypeSymbol2, LookupResultKind.NotAnAttributeType, null);
		}
		return namedTypeSymbol2;
	}

	private NamedTypeSymbol LookupGenericTypeName(DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved, NamespaceOrTypeSymbol qualifierOpt, GenericNameSyntax node, string plainName, int arity, LookupOptions options)
	{
		ExtendedErrorTypeSymbol extendedErrorTypeSymbol = CreateErrorIfLookupOnTypeParameter(node.Parent, qualifierOpt, plainName, arity, diagnostics);
		if ((object)extendedErrorTypeSymbol != null)
		{
			return extendedErrorTypeSymbol;
		}
		LookupResult instance = LookupResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsSimpleName(instance, qualifierOpt, plainName, arity, basesBeingResolved, options, diagnose: true, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		Symbol symbol = ResultSymbol(instance, plainName, arity, node, diagnostics, basesBeingResolved != null, out var _, qualifierOpt, options);
		NamedTypeSymbol namedTypeSymbol = symbol as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null)
		{
			namedTypeSymbol = new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(symbol), ImmutableArray.Create(symbol), instance.Kind, instance.Error, arity);
		}
		instance.Free();
		return namedTypeSymbol;
	}

	private ExtendedErrorTypeSymbol CreateErrorIfLookupOnTypeParameter(CSharpSyntaxNode node, NamespaceOrTypeSymbol qualifierOpt, string name, int arity, DiagnosticBag diagnostics)
	{
		if ((object)qualifierOpt != null && qualifierOpt.Kind == SymbolKind.TypeParameter)
		{
			CSDiagnosticInfo cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_LookupInTypeVariable, qualifierOpt);
			diagnostics.Add(cSDiagnosticInfo, node.Location);
			return new ExtendedErrorTypeSymbol(Compilation, name, arity, cSDiagnosticInfo);
		}
		return null;
	}

	private ImmutableArray<TypeSymbol> BindTypeArguments(SeparatedSyntaxList<TypeSyntax> typeArguments, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		foreach (TypeSyntax item in typeArguments)
		{
			instance.Add(BindTypeArgument(item, diagnostics, basesBeingResolved));
		}
		return instance.ToImmutableAndFree();
	}

	private TypeSymbol BindTypeArgument(TypeSyntax typeArgument, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		Binder binder = WithAdditionalFlags(BinderFlags.SuppressUnsafeDiagnostics);
		if (typeArgument.Kind() != SyntaxKind.OmittedTypeArgument)
		{
			return binder.BindType(typeArgument, diagnostics, basesBeingResolved);
		}
		return UnboundArgumentErrorTypeSymbol.Instance;
	}

	private NamedTypeSymbol ConstructNamedTypeUnlessTypeArgumentOmitted(SyntaxNode typeSyntax, NamedTypeSymbol type, SeparatedSyntaxList<TypeSyntax> typeArgumentsSyntax, ImmutableArray<TypeSymbol> typeArguments, DiagnosticBag diagnostics)
	{
		if (typeArgumentsSyntax.Any(SyntaxKind.OmittedTypeArgument))
		{
			Error(diagnostics, ErrorCode.ERR_BadArity, typeSyntax, type, MessageID.IDS_SK_TYPE.Localize(), typeArgumentsSyntax.Count);
			return type;
		}
		return ConstructNamedType(type, typeSyntax, typeArgumentsSyntax, typeArguments, null, diagnostics);
	}

	private static BoundMethodOrPropertyGroup ConstructBoundMemberGroupAndReportOmittedTypeArguments(SyntaxNode syntax, SeparatedSyntaxList<TypeSyntax> typeArgumentsSyntax, ImmutableArray<TypeSymbol> typeArguments, BoundExpression receiver, string plainName, ArrayBuilder<Symbol> members, LookupResult lookupResult, BoundMethodGroupFlags methodGroupFlags, bool hasErrors, DiagnosticBag diagnostics)
	{
		if (!hasErrors && lookupResult.IsMultiViable && typeArgumentsSyntax.Any(SyntaxKind.OmittedTypeArgument))
		{
			Error(diagnostics, ErrorCode.ERR_BadArity, syntax, plainName, MessageID.IDS_MethodGroup.Localize(), typeArgumentsSyntax.Count);
			hasErrors = true;
		}
		return members[0].Kind switch
		{
			SymbolKind.Method => (BoundMethodOrPropertyGroup)new BoundMethodGroup(syntax, typeArguments, receiver, plainName, members.SelectAsArray(s_toMethodSymbolFunc), lookupResult, methodGroupFlags, hasErrors), 
			SymbolKind.Property => new BoundPropertyGroup(syntax, members.SelectAsArray(s_toPropertySymbolFunc), receiver, lookupResult.Kind, hasErrors), 
			_ => throw ExceptionUtilities.UnexpectedValue(members[0].Kind), 
		};
	}

	private NamedTypeSymbol ConstructNamedType(NamedTypeSymbol type, SyntaxNode typeSyntax, SeparatedSyntaxList<TypeSyntax> typeArgumentsSyntax, ImmutableArray<TypeSymbol> typeArguments, ConsList<Symbol> basesBeingResolved, DiagnosticBag diagnostics)
	{
		type = type.Construct(typeArguments);
		if (ShouldCheckConstraints)
		{
			type.CheckConstraintsForNonTuple(Conversions, typeSyntax, typeArgumentsSyntax, Compilation, basesBeingResolved, diagnostics);
		}
		type = (NamedTypeSymbol)TupleTypeSymbol.TransformToTupleIfCompatible(type);
		return type;
	}

	private NamespaceOrTypeSymbol BindQualifiedName(ExpressionSyntax leftName, SimpleNameSyntax rightName, DiagnosticBag diagnostics, ConsList<Symbol> basesBeingResolved, bool suppressUseSiteDiagnostics)
	{
		NamespaceOrTypeSymbol namespaceOrTypeSymbol = BindNamespaceOrTypeSymbol(leftName, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics: false);
		ReportDiagnosticsIfObsolete(diagnostics, namespaceOrTypeSymbol, leftName, hasBaseReceiver: false);
		int num;
		if (namespaceOrTypeSymbol.Kind == SymbolKind.NamedType)
		{
			num = (((NamedTypeSymbol)namespaceOrTypeSymbol).IsUnboundGenericType ? 1 : 0);
			if (num != 0)
			{
				namespaceOrTypeSymbol = ((NamedTypeSymbol)namespaceOrTypeSymbol).OriginalDefinition;
			}
		}
		else
		{
			num = 0;
		}
		NamespaceOrTypeSymbol namespaceOrTypeSymbol2 = BindSimpleNamespaceOrTypeOrAliasSymbol(rightName, diagnostics, basesBeingResolved, suppressUseSiteDiagnostics, namespaceOrTypeSymbol) as NamespaceOrTypeSymbol;
		if (num != 0 && namespaceOrTypeSymbol2 is NamedTypeSymbol { IsGenericType: not false } namedTypeSymbol)
		{
			namespaceOrTypeSymbol2 = namedTypeSymbol.AsUnboundGenericType();
		}
		return namespaceOrTypeSymbol2;
	}

	internal NamedTypeSymbol GetSpecialType(SpecialType typeId, DiagnosticBag diagnostics, SyntaxNode node)
	{
		return GetSpecialType(Compilation, typeId, node, diagnostics);
	}

	internal static NamedTypeSymbol GetSpecialType(CSharpCompilation compilation, SpecialType typeId, SyntaxNode node, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol specialType = compilation.GetSpecialType(typeId);
		ReportUseSiteDiagnostics(specialType, diagnostics, node);
		return specialType;
	}

	internal Symbol GetSpecialTypeMember(SpecialMember member, DiagnosticBag diagnostics, SyntaxNode syntax)
	{
		if (!TryGetSpecialTypeMember<Symbol>(Compilation, member, syntax, diagnostics, out var symbol))
		{
			return null;
		}
		return symbol;
	}

	internal static bool TryGetSpecialTypeMember<TSymbol>(CSharpCompilation compilation, SpecialMember specialMember, SyntaxNode syntax, DiagnosticBag diagnostics, out TSymbol symbol) where TSymbol : Symbol
	{
		symbol = (TSymbol)compilation.GetSpecialTypeMember(specialMember);
		if ((object)symbol == null)
		{
			MemberDescriptor descriptor = SpecialMembers.GetDescriptor(specialMember);
			diagnostics.Add(ErrorCode.ERR_MissingPredefinedMember, syntax.Location, descriptor.DeclaringTypeMetadataName, descriptor.Name);
			return false;
		}
		DiagnosticInfo useSiteDiagnosticForSymbolOrContainingType = symbol.GetUseSiteDiagnosticForSymbolOrContainingType();
		if (useSiteDiagnosticForSymbolOrContainingType != null)
		{
			Symbol.ReportUseSiteDiagnostic(useSiteDiagnosticForSymbolOrContainingType, diagnostics, new SourceLocation(syntax));
		}
		return true;
	}

	internal static bool ReportUseSiteDiagnostics(Symbol symbol, DiagnosticBag diagnostics, SyntaxNode node)
	{
		DiagnosticInfo useSiteDiagnostic = symbol.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			return Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, node.Location);
		}
		return false;
	}

	internal static bool ReportUseSiteDiagnostics(Symbol symbol, DiagnosticBag diagnostics, Location location)
	{
		DiagnosticInfo useSiteDiagnostic = symbol.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			return Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, location);
		}
		return false;
	}

	internal NamedTypeSymbol GetWellKnownType(WellKnownType type, DiagnosticBag diagnostics, SyntaxNode node)
	{
		NamedTypeSymbol wellKnownType = Compilation.GetWellKnownType(type);
		ReportUseSiteDiagnostics(wellKnownType, diagnostics, node);
		return wellKnownType;
	}

	internal NamedTypeSymbol GetWellKnownType(WellKnownType type, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol wellKnownType = Compilation.GetWellKnownType(type);
		HashSetExtensions.InitializeAndAdd(ref useSiteDiagnostics, wellKnownType.GetUseSiteDiagnostic());
		return wellKnownType;
	}

	internal static Symbol GetWellKnownTypeMember(CSharpCompilation compilation, WellKnownMember member, DiagnosticBag diagnostics, Location location = null, SyntaxNode syntax = null, bool isOptional = false)
	{
		Symbol wellKnownTypeMember = GetWellKnownTypeMember(compilation, member, out var diagnosticInfo, isOptional);
		if (diagnosticInfo != null)
		{
			Symbol.ReportUseSiteDiagnostic(diagnosticInfo, diagnostics, location ?? syntax.Location);
		}
		return wellKnownTypeMember;
	}

	internal static Symbol GetWellKnownTypeMember(CSharpCompilation compilation, WellKnownMember member, out DiagnosticInfo diagnosticInfo, bool isOptional = false)
	{
		Symbol wellKnownTypeMember = compilation.GetWellKnownTypeMember(member);
		if ((object)wellKnownTypeMember != null)
		{
			diagnosticInfo = wellKnownTypeMember.GetUseSiteDiagnosticForSymbolOrContainingType();
			if (diagnosticInfo != null && isOptional)
			{
				DiagnosticSeverity severity = diagnosticInfo.Severity;
				diagnosticInfo = null;
				if (severity == DiagnosticSeverity.Error)
				{
					return null;
				}
			}
		}
		else if (!isOptional)
		{
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(member);
			diagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, descriptor.DeclaringTypeMetadataName, descriptor.Name);
		}
		else
		{
			diagnosticInfo = null;
		}
		return wellKnownTypeMember;
	}

	internal Symbol ResultSymbol(LookupResult result, string simpleName, int arity, SyntaxNode where, DiagnosticBag diagnostics, bool suppressUseSiteDiagnostics, out bool wasError, NamespaceOrTypeSymbol qualifierOpt = null, LookupOptions options = LookupOptions.Default)
	{
		ArrayBuilder<Symbol> symbols = result.Symbols;
		wasError = false;
		if (result.IsMultiViable)
		{
			if (symbols.Count > 1)
			{
				symbols.Sort(ConsistentSymbolOrder.Instance);
				ImmutableArray<Symbol> immutableArray = symbols.ToImmutable();
				for (int i = 0; i < symbols.Count; i++)
				{
					symbols[i] = UnwrapAlias(symbols[i], diagnostics, where);
				}
				BestSymbolInfo bestSymbolInfo = GetBestSymbolInfo(symbols, out var secondBest);
				if (bestSymbolInfo.IsFromCompilation && !secondBest.IsFromCompilation)
				{
					Symbol symbol = symbols[bestSymbolInfo.Index];
					Symbol symbol2 = symbols[secondBest.Index];
					object obj = ((!bestSymbolInfo.IsFromSourceModule) ? ((object)symbol.ContainingModule) : ((object)symbol.Locations.First().SourceTree.FilePath));
					if (symbol.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat) == symbol2.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat))
					{
						if (symbol.Kind == SymbolKind.Namespace && symbol2.Kind == SymbolKind.NamedType)
						{
							diagnostics.Add(ErrorCode.WRN_SameFullNameThisNsAgg, where.Location, immutableArray, obj, symbol, symbol2.ContainingAssembly, symbol2);
							return immutableArray[bestSymbolInfo.Index];
						}
						if (symbol.Kind == SymbolKind.NamedType && symbol2.Kind == SymbolKind.Namespace)
						{
							diagnostics.Add(ErrorCode.WRN_SameFullNameThisAggNs, where.Location, immutableArray, obj, symbol, GetContainingAssembly(symbol2), symbol2);
							return immutableArray[bestSymbolInfo.Index];
						}
						if (symbol.Kind == SymbolKind.NamedType && symbol2.Kind == SymbolKind.NamedType)
						{
							diagnostics.Add(ErrorCode.WRN_SameFullNameThisAggAgg, where.Location, immutableArray, obj, symbol, symbol2.ContainingAssembly, symbol2);
							return immutableArray[bestSymbolInfo.Index];
						}
					}
				}
				Symbol symbol3 = symbols[bestSymbolInfo.Index];
				Symbol symbol4 = symbols[secondBest.Index];
				bool flag;
				CSDiagnosticInfo cSDiagnosticInfo;
				if (symbol3 != symbol4 && symbol3.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat) == symbol4.ToDisplayString(SymbolDisplayFormat.QualifiedNameArityFormat))
				{
					flag = !bestSymbolInfo.IsFromSourceModule || !secondBest.IsFromSourceModule;
					if (symbol3.Kind == SymbolKind.NamedType && symbol4.Kind == SymbolKind.NamedType)
					{
						if (symbol3.OriginalDefinition == symbol4.OriginalDefinition)
						{
							flag = true;
							cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_AmbigContext, immutableArray, new object[3]
							{
								(where as NameSyntax)?.ErrorDisplayName() ?? simpleName,
								new FormattedSymbol(symbol3, SymbolDisplayFormat.CSharpErrorMessageFormat),
								new FormattedSymbol(symbol4, SymbolDisplayFormat.CSharpErrorMessageFormat)
							});
						}
						else
						{
							cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_SameFullNameAggAgg, immutableArray, new object[3] { symbol3.ContainingAssembly, symbol3, symbol4.ContainingAssembly });
							if (secondBest.IsFromAddedModule)
							{
								flag = false;
							}
							else if (Flags.Includes(BinderFlags.IgnoreCorLibraryDuplicatedTypes) && secondBest.IsFromCorLibrary)
							{
								return symbol3;
							}
						}
					}
					else if (symbol3.Kind == SymbolKind.Namespace && symbol4.Kind == SymbolKind.NamedType)
					{
						cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_SameFullNameNsAgg, immutableArray, new object[4]
						{
							GetContainingAssembly(symbol3),
							symbol3,
							symbol4.ContainingAssembly,
							symbol4
						});
						if (bestSymbolInfo.IsFromSourceModule && secondBest.IsFromAddedModule)
						{
							flag = false;
						}
					}
					else if (symbol3.Kind == SymbolKind.NamedType && symbol4.Kind == SymbolKind.Namespace)
					{
						if (!secondBest.IsFromCompilation || secondBest.IsFromSourceModule)
						{
							cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_SameFullNameNsAgg, immutableArray, new object[4]
							{
								GetContainingAssembly(symbol4),
								symbol4,
								symbol3.ContainingAssembly,
								symbol3
							});
						}
						else
						{
							object obj2 = ((!bestSymbolInfo.IsFromSourceModule) ? ((object)symbol3.ContainingModule) : ((object)symbol3.Locations.First().SourceTree.FilePath));
							ModuleSymbol moduleSymbol = symbol4.ContainingModule;
							if ((object)moduleSymbol == null)
							{
								foreach (NamespaceSymbol constituentNamespace in ((NamespaceSymbol)symbol4).ConstituentNamespaces)
								{
									if (constituentNamespace.ContainingAssembly == Compilation.Assembly)
									{
										ModuleSymbol containingModule = constituentNamespace.ContainingModule;
										if ((object)moduleSymbol == null || moduleSymbol.Ordinal > containingModule.Ordinal)
										{
											moduleSymbol = containingModule;
										}
									}
								}
							}
							cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_SameFullNameThisAggThisNs, immutableArray, new object[4] { obj2, symbol3, moduleSymbol, symbol4 });
						}
					}
					else if (symbol3.Kind == SymbolKind.RangeVariable && symbol4.Kind == SymbolKind.RangeVariable)
					{
						cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_AmbigMember, immutableArray, new object[2] { symbol3, symbol4 });
					}
					else
					{
						cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_AmbigMember, immutableArray, new object[2] { symbol3, symbol4 });
						flag = true;
					}
				}
				else
				{
					flag = true;
					if (!(symbol3 is NamespaceOrTypeSymbol) || !(symbol4 is NamespaceOrTypeSymbol))
					{
						cSDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_AmbigMember, immutableArray, new object[2] { symbol3, symbol4 });
					}
					else
					{
						cSDiagnosticInfo = ((!options.IsAttributeTypeLookup() || symbol3.Kind != SymbolKind.NamedType || symbol4.Kind != SymbolKind.NamedType || !(immutableArray[bestSymbolInfo.Index].Name != immutableArray[secondBest.Index].Name) || !Compilation.IsAttributeType((NamedTypeSymbol)symbol3) || !Compilation.IsAttributeType((NamedTypeSymbol)symbol4)) ? new CSDiagnosticInfo(ErrorCode.ERR_AmbigContext, immutableArray, new object[3]
						{
							(where as NameSyntax)?.ErrorDisplayName() ?? simpleName,
							new FormattedSymbol(symbol3, SymbolDisplayFormat.CSharpErrorMessageFormat),
							new FormattedSymbol(symbol4, SymbolDisplayFormat.CSharpErrorMessageFormat)
						}) : new CSDiagnosticInfo(ErrorCode.ERR_AmbiguousAttribute, immutableArray, new object[3]
						{
							(where as NameSyntax)?.ErrorDisplayName() ?? simpleName,
							symbol3,
							symbol4
						}));
					}
				}
				wasError = true;
				if (flag)
				{
					diagnostics.Add(cSDiagnosticInfo, where.Location);
				}
				return new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(immutableArray[0]), immutableArray, LookupResultKind.Ambiguous, cSDiagnosticInfo, arity);
			}
			Symbol symbol5 = symbols[0];
			if (symbol5 is TypeSymbol { PrimitiveTypeCode: PrimitiveTypeCode.Void } && simpleName == "Void")
			{
				wasError = true;
				CSDiagnosticInfo cSDiagnosticInfo2 = new CSDiagnosticInfo(ErrorCode.ERR_SystemVoid);
				diagnostics.Add(cSDiagnosticInfo2, where.Location);
				symbol5 = new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(symbol5), symbol5, LookupResultKind.NotReferencable, cSDiagnosticInfo2);
			}
			else
			{
				if (symbol5.Kind == SymbolKind.NamedType && ((SourceModuleSymbol)Compilation.SourceModule).AnyReferencedAssembliesAreLinked)
				{
					EmbeddedTypesManager.IsValidEmbeddableType((NamedTypeSymbol)symbol5, where, diagnostics);
				}
				if (!suppressUseSiteDiagnostics)
				{
					wasError = ReportUseSiteDiagnostics(symbol5, diagnostics, where);
				}
				else if (symbol5.Kind == SymbolKind.ErrorType)
				{
					ErrorTypeSymbol errorTypeSymbol = (ErrorTypeSymbol)symbol5;
					if (errorTypeSymbol.Unreported)
					{
						DiagnosticInfo errorInfo = errorTypeSymbol.ErrorInfo;
						if (errorInfo != null && errorInfo.Code == 146)
						{
							wasError = true;
							diagnostics.Add(errorInfo, where.Location);
							symbol5 = new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(errorTypeSymbol), errorTypeSymbol.Name, errorTypeSymbol.Arity, errorInfo);
						}
					}
				}
			}
			return symbol5;
		}
		wasError = true;
		if (result.Kind == LookupResultKind.Empty)
		{
			string aliasOpt = null;
			SyntaxNode syntaxNode = where;
			while (syntaxNode is ExpressionSyntax)
			{
				if (syntaxNode.Kind() == SyntaxKind.AliasQualifiedName)
				{
					aliasOpt = ((AliasQualifiedNameSyntax)syntaxNode).Alias.Identifier.ValueText;
					break;
				}
				syntaxNode = syntaxNode.Parent;
			}
			CSDiagnosticInfo errorInfo2 = NotFound(where, simpleName, arity, (where as NameSyntax)?.ErrorDisplayName() ?? simpleName, diagnostics, aliasOpt, qualifierOpt, options);
			return new ExtendedErrorTypeSymbol(qualifierOpt ?? Compilation.Assembly.GlobalNamespace, simpleName, arity, errorInfo2);
		}
		if (!suppressUseSiteDiagnostics)
		{
			for (int j = 0; j < symbols.Count; j++)
			{
				ReportUseSiteDiagnostics(symbols[j], diagnostics, where);
			}
		}
		if (result.Error != null && ((object)qualifierOpt == null || qualifierOpt.Kind != SymbolKind.ErrorType))
		{
			diagnostics.Add(new CSDiagnostic(result.Error, where.Location));
		}
		if (symbols.Count > 1 || symbols[0] is NamespaceOrTypeSymbol || symbols[0] is AliasSymbol || result.Kind == LookupResultKind.NotATypeOrNamespace || result.Kind == LookupResultKind.NotAnAttributeType)
		{
			return new ExtendedErrorTypeSymbol(GetContainingNamespaceOrType(symbols[0]), symbols.ToImmutable(), result.Kind, result.Error, arity);
		}
		return symbols[0];
	}

	private static AssemblySymbol GetContainingAssembly(Symbol symbol)
	{
		return symbol.ContainingAssembly ?? ((NamespaceSymbol)symbol).ConstituentNamespaces.First().ContainingAssembly;
	}

	private BestSymbolInfo GetBestSymbolInfo(ArrayBuilder<Symbol> symbols, out BestSymbolInfo secondBest)
	{
		BestSymbolInfo first = default;
		BestSymbolInfo first2 = default;
		CSharpCompilation compilation = Compilation;
		for (int i = 0; i < symbols.Count; i++)
		{
			Symbol symbol = symbols[i];
			BestSymbolLocation bestSymbolLocation;
			if (symbol.Kind == SymbolKind.Namespace)
			{
				bestSymbolLocation = BestSymbolLocation.None;
				foreach (NamespaceSymbol constituentNamespace in ((NamespaceSymbol)symbol).ConstituentNamespaces)
				{
					BestSymbolLocation location = GetLocation(compilation, constituentNamespace);
					if (BestSymbolInfo.IsSecondLocationBetter(bestSymbolLocation, location))
					{
						bestSymbolLocation = location;
						if (bestSymbolLocation == BestSymbolLocation.FromSourceModule)
						{
							break;
						}
					}
				}
			}
			else
			{
				bestSymbolLocation = GetLocation(compilation, symbol);
			}
			BestSymbolInfo second = new BestSymbolInfo(bestSymbolLocation, i);
			if (BestSymbolInfo.Sort(ref first2, ref second))
			{
				BestSymbolInfo.Sort(ref first, ref first2);
			}
		}
		secondBest = first2;
		return first;
	}

	private static BestSymbolLocation GetLocation(CSharpCompilation compilation, Symbol symbol)
	{
		AssemblySymbol containingAssembly = symbol.ContainingAssembly;
		if (containingAssembly == compilation.SourceAssembly)
		{
			if (!(symbol.ContainingModule == compilation.SourceModule))
			{
				return BestSymbolLocation.FromAddedModule;
			}
			return BestSymbolLocation.FromSourceModule;
		}
		if (!(containingAssembly == containingAssembly.CorLibrary))
		{
			return BestSymbolLocation.FromReferencedAssembly;
		}
		return BestSymbolLocation.FromCorLibrary;
	}

	private CSDiagnosticInfo NotFound(SyntaxNode where, string simpleName, int arity, string whereText, DiagnosticBag diagnostics, string aliasOpt, NamespaceOrTypeSymbol qualifierOpt, LookupOptions options)
	{
		Location location = where.Location;
		if (options.IsAttributeTypeLookup() && !options.IsVerbatimNameAttributeTypeLookup())
		{
			NotFound(where, simpleName, arity, whereText + "Attribute", diagnostics, aliasOpt, qualifierOpt, options | LookupOptions.VerbatimNameAttributeTypeOnly);
		}
		string text;
		AssemblySymbol forwardedToAssembly;
		if ((object)qualifierOpt != null)
		{
			if (qualifierOpt.IsType)
			{
				if (qualifierOpt is ErrorTypeSymbol { ErrorInfo: not null } errorTypeSymbol)
				{
					return (CSDiagnosticInfo)errorTypeSymbol.ErrorInfo;
				}
				return diagnostics.Add(ErrorCode.ERR_DottedTypeNameNotFoundInAgg, location, whereText, qualifierOpt);
			}
			bool flag = (object)qualifierOpt == Compilation.GlobalNamespace;
			text = MetadataHelpers.ComposeAritySuffixedMetadataName(simpleName, arity);
			if (!flag)
			{
				text = qualifierOpt.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat) + "." + text;
			}
			forwardedToAssembly = GetForwardedToAssembly(text, arity, diagnostics, location);
			if (flag)
			{
				if ((object)forwardedToAssembly != null)
				{
					return diagnostics.Add(ErrorCode.ERR_GlobalSingleTypeNameNotFoundFwd, location, whereText, forwardedToAssembly);
				}
				return diagnostics.Add(ErrorCode.ERR_GlobalSingleTypeNameNotFound, location, whereText, qualifierOpt);
			}
			object obj = qualifierOpt;
			if (aliasOpt != null && qualifierOpt.IsNamespace && ((NamespaceSymbol)qualifierOpt).IsGlobalNamespace)
			{
				obj = aliasOpt;
			}
			if ((object)forwardedToAssembly != null)
			{
				return diagnostics.Add(ErrorCode.ERR_DottedTypeNameNotFoundInNSFwd, location, whereText, obj, forwardedToAssembly);
			}
			return diagnostics.Add(ErrorCode.ERR_DottedTypeNameNotFoundInNS, location, whereText, obj);
		}
		if (options == LookupOptions.NamespaceAliasesOnly)
		{
			return diagnostics.Add(ErrorCode.ERR_AliasNotFound, location, whereText);
		}
		if ((where as IdentifierNameSyntax)?.Identifier.Text == "var" && !options.IsAttributeTypeLookup())
		{
			ErrorCode code = ((where.Parent is QueryClauseSyntax) ? ErrorCode.ERR_TypeVarNotFoundRangeVariable : ErrorCode.ERR_TypeVarNotFound);
			return diagnostics.Add(code, location);
		}
		text = MetadataHelpers.ComposeAritySuffixedMetadataName(simpleName, arity);
		forwardedToAssembly = GetForwardedToAssembly(text, arity, diagnostics, location);
		if ((object)forwardedToAssembly != null)
		{
			return diagnostics.Add(ErrorCode.ERR_SingleTypeNameNotFoundFwd, location, whereText, forwardedToAssembly);
		}
		return diagnostics.Add(ErrorCode.ERR_SingleTypeNameNotFound, location, whereText);
	}

	private AssemblySymbol GetForwardedToAssembly(string fullName, int arity, DiagnosticBag diagnostics, Location location)
	{
		Binder binder = this;
		do
		{
			if (binder is ContextualAttributeBinder contextualAttributeBinder)
			{
				if ((object)contextualAttributeBinder.AttributeTarget == null || contextualAttributeBinder.AttributeTarget.Kind != SymbolKind.Assembly)
				{
					break;
				}
				return null;
			}
			binder = binder.Next;
		}
		while (binder != null);
		MetadataTypeName emittedName = MetadataTypeName.FromFullName(fullName, useCLSCompliantNameArityEncoding: true, arity);
		AssemblySymbol assembly = Compilation.Assembly;
		NamedTypeSymbol namedTypeSymbol = null;
		foreach (AssemblySymbol referencedAssemblySymbol in assembly.Modules[0].GetReferencedAssemblySymbols())
		{
			namedTypeSymbol = referencedAssemblySymbol.TryLookupForwardedMetadataType(ref emittedName);
			if ((object)namedTypeSymbol != null)
			{
				break;
			}
		}
		if ((object)namedTypeSymbol != null)
		{
			if (namedTypeSymbol.Kind == SymbolKind.ErrorType)
			{
				DiagnosticInfo errorInfo = ((ErrorTypeSymbol)namedTypeSymbol).ErrorInfo;
				if (errorInfo.Code == 731)
				{
					diagnostics.Add(ErrorCode.ERR_CycleInTypeForwarder, location, fullName, namedTypeSymbol.ContainingAssembly.Name);
				}
				else if (errorInfo.Code == 8206)
				{
					diagnostics.Add(errorInfo, location);
					return null;
				}
			}
			return namedTypeSymbol.ContainingAssembly;
		}
		return null;
	}

	internal static bool CheckFeatureAvailability(SyntaxNode syntax, MessageID feature, DiagnosticBag diagnostics, Location locationOpt = null)
	{
		return CheckFeatureAvailability(syntax.SyntaxTree, feature, diagnostics, locationOpt ?? syntax.GetLocation());
	}

	internal static bool CheckFeatureAvailability(SyntaxTree tree, MessageID feature, DiagnosticBag diagnostics, Location location)
	{
		CSDiagnosticInfo featureAvailabilityDiagnosticInfo = GetFeatureAvailabilityDiagnosticInfo(tree, feature);
		if (featureAvailabilityDiagnosticInfo == null)
		{
			return true;
		}
		diagnostics.Add(new CSDiagnostic(featureAvailabilityDiagnosticInfo, location));
		return false;
	}

	private static CSDiagnosticInfo GetFeatureAvailabilityDiagnosticInfo(SyntaxTree tree, MessageID feature)
	{
		CSharpParseOptions cSharpParseOptions = (CSharpParseOptions)tree.Options;
		if (cSharpParseOptions.IsFeatureEnabled(feature))
		{
			return null;
		}
		LanguageVersion languageVersion = cSharpParseOptions.LanguageVersion;
		LanguageVersion languageVersion2 = feature.RequiredVersion();
		if (languageVersion2 > languageVersion)
		{
			return new CSDiagnosticInfo(languageVersion.GetErrorCode(), feature.Localize(), new CSharpRequiredLanguageVersion(languageVersion2));
		}
		return null;
	}

	private BoundTupleBinaryOperator BindTupleBinaryOperator(BinaryExpressionSyntax node, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		TupleBinaryOperatorInfo.Multiple multiple = BindTupleBinaryOperatorNestedInfo(node, kind, left, right, diagnostics);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		BoundExpression convertedLeft = ApplyConvertedTypes(left, multiple, isRight: false, instance);
		BoundExpression convertedRight = ApplyConvertedTypes(right, multiple, isRight: true, instance);
		instance.Free();
		TypeSymbol specialType = GetSpecialType(SpecialType.System_Boolean, diagnostics, node);
		return new BoundTupleBinaryOperator(node, left, right, convertedLeft, convertedRight, kind, multiple, specialType);
	}

	private BoundExpression ApplyConvertedTypes(BoundExpression expr, TupleBinaryOperatorInfo @operator, bool isRight, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = (isRight ? @operator.RightConvertedTypeOpt : @operator.LeftConvertedTypeOpt);
		if ((object)typeSymbol == null)
		{
			if (@operator.InfoKind == TupleBinaryOperatorInfoKind.Multiple && expr.Kind == BoundKind.TupleLiteral)
			{
				TupleBinaryOperatorInfo.Multiple multiple = (TupleBinaryOperatorInfo.Multiple)@operator;
				if (multiple.Operators.Length == 0)
				{
					return expr;
				}
				BoundTupleLiteral boundTupleLiteral = (BoundTupleLiteral)expr;
				ImmutableArray<BoundExpression> arguments = boundTupleLiteral.Arguments;
				int length = arguments.Length;
				ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
				for (int i = 0; i < length; i++)
				{
					instance.Add(ApplyConvertedTypes(arguments[i], multiple.Operators[i], isRight, diagnostics));
				}
				return boundTupleLiteral.Update(default, default, instance.ToImmutableAndFree(), boundTupleLiteral.Type);
			}
			return expr;
		}
		return GenerateConversionForAssignment(typeSymbol, expr, diagnostics);
	}

	private TupleBinaryOperatorInfo BindTupleBinaryOperatorInfo(BinaryExpressionSyntax node, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		TypeSymbol type = left.Type;
		TypeSymbol type2 = right.Type;
		if (((object)type != null && type.IsDynamic()) || ((object)type2 != null && type2.IsDynamic()))
		{
			return BindTupleDynamicBinaryOperatorSingleInfo(node, kind, left, right, diagnostics);
		}
		if (IsTupleBinaryOperation(left, right))
		{
			return BindTupleBinaryOperatorNestedInfo(node, kind, left, right, diagnostics);
		}
		if (left.IsLiteralNull() && right.IsLiteralNull())
		{
			return new TupleBinaryOperatorInfo.NullNull(kind);
		}
		if (!BindSimpleBinaryOperatorParts(node, diagnostics, left, right, kind, out var resultKind, out var _, out var resultSignature, out var best))
		{
			ReportBinaryOperatorError(node, diagnostics, node.OperatorToken, left, right, resultKind);
		}
		ReportDiagnosticsIfObsolete(diagnostics, best.LeftConversion, left.Syntax, hasBaseReceiver: false);
		ReportDiagnosticsIfObsolete(diagnostics, best.RightConversion, right.Syntax, hasBaseReceiver: false);
		PrepareBoolConversionAndTruthOperator(resultSignature.ReturnType, node, kind, diagnostics, out var conversionForBool, out var boolOperator);
		return new TupleBinaryOperatorInfo.Single(resultSignature.LeftType, resultSignature.RightType, resultSignature.Kind, best.LeftConversion, best.RightConversion, resultSignature.Method, conversionForBool, boolOperator);
	}

	private void PrepareBoolConversionAndTruthOperator(TypeSymbol type, BinaryExpressionSyntax node, BinaryOperatorKind binaryOperator, DiagnosticBag diagnostics, out Conversion conversionForBool, out UnaryOperatorSignature boolOperator)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		TypeSymbol specialType = GetSpecialType(SpecialType.System_Boolean, diagnostics, node);
		Conversion conversion = Conversions.ClassifyImplicitConversionFromType(type, specialType, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		if (conversion.IsImplicit)
		{
			ReportDiagnosticsIfObsolete(diagnostics, conversion, node, hasBaseReceiver: false);
			conversionForBool = conversion;
			boolOperator = default;
			return;
		}
		UnaryOperatorKind kind = binaryOperator switch
		{
			BinaryOperatorKind.Equal => UnaryOperatorKind.False, 
			BinaryOperatorKind.NotEqual => UnaryOperatorKind.True, 
			_ => throw ExceptionUtilities.UnexpectedValue(binaryOperator), 
		};
		BoundExpression operand = new BoundTupleOperandPlaceholder(node, type);
		UnaryOperatorAnalysisResult unaryOperatorAnalysisResult = UnaryOperatorOverloadResolution(kind, operand, node, diagnostics, out var _, out var _);
		if (unaryOperatorAnalysisResult.HasValue)
		{
			conversionForBool = unaryOperatorAnalysisResult.Conversion;
			boolOperator = unaryOperatorAnalysisResult.Signature;
		}
		else
		{
			GenerateImplicitConversionError(diagnostics, node, conversion, operand, specialType);
			conversionForBool = Conversion.NoConversion;
			boolOperator = default;
		}
	}

	private TupleBinaryOperatorInfo BindTupleDynamicBinaryOperatorSingleInfo(BinaryExpressionSyntax node, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		bool flag = false;
		if (!IsLegalDynamicOperand(left) || !IsLegalDynamicOperand(right))
		{
			Error(diagnostics, ErrorCode.ERR_BadBinaryOps, node, node.OperatorToken.Text, left.Display, right.Display);
			flag = true;
		}
		BinaryOperatorKind kind2 = (flag ? kind : kind.WithType(BinaryOperatorKind.Dynamic));
		TypeSymbol dynamicType = Compilation.DynamicType;
		return new TupleBinaryOperatorInfo.Single(dynamicType, dynamicType, kind2, Conversion.NoConversion, Conversion.NoConversion, null, Conversion.Identity, default);
	}

	private TupleBinaryOperatorInfo.Multiple BindTupleBinaryOperatorNestedInfo(BinaryExpressionSyntax node, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		left = GiveTupleTypeToDefaultLiteralIfNeeded(left, right.Type);
		right = GiveTupleTypeToDefaultLiteralIfNeeded(right, left.Type);
		if (((object)left.Type == null && left.IsLiteralDefault()) || ((object)right.Type == null && right.IsLiteralDefault()))
		{
			Error(diagnostics, ErrorCode.ERR_AmbigBinaryOps, node, node.OperatorToken.Text, left.Display, right.Display);
			return TupleBinaryOperatorInfo.Multiple.ErrorInstance;
		}
		int tupleCardinality = GetTupleCardinality(left);
		int tupleCardinality2 = GetTupleCardinality(right);
		if (tupleCardinality != tupleCardinality2)
		{
			Error(diagnostics, ErrorCode.ERR_TupleSizesMismatchForBinOps, node, tupleCardinality, tupleCardinality2);
			return TupleBinaryOperatorInfo.Multiple.ErrorInstance;
		}
		var (elements, immutableArray) = GetTupleArgumentsOrPlaceholders(left);
		var (elements2, immutableArray2) = GetTupleArgumentsOrPlaceholders(right);
		ReportNamesMismatchesIfAny(left, right, immutableArray, immutableArray2, diagnostics);
		int length = elements.Length;
		ArrayBuilder<TupleBinaryOperatorInfo> instance = ArrayBuilder<TupleBinaryOperatorInfo>.GetInstance(length);
		for (int i = 0; i < length; i++)
		{
			instance.Add(BindTupleBinaryOperatorInfo(node, kind, elements[i], elements2[i], diagnostics));
		}
		CSharpCompilation compilation = Compilation;
		ImmutableArray<TupleBinaryOperatorInfo> immutableArray3 = instance.ToImmutableAndFree();
		bool num = left.Type?.IsNullableType() ?? false;
		bool flag = right.Type?.IsNullableType() ?? false;
		bool isNullable = num | flag;
		TypeSymbol leftConvertedTypeOpt = MakeConvertedType(immutableArray3.SelectAsArray((TupleBinaryOperatorInfo o) => o.LeftConvertedTypeOpt), node.Left, elements, immutableArray, isNullable, compilation, diagnostics);
		TypeSymbol rightConvertedTypeOpt = MakeConvertedType(immutableArray3.SelectAsArray((TupleBinaryOperatorInfo o) => o.RightConvertedTypeOpt), node.Right, elements2, immutableArray2, isNullable, compilation, diagnostics);
		return new TupleBinaryOperatorInfo.Multiple(immutableArray3, leftConvertedTypeOpt, rightConvertedTypeOpt);
	}

	private static void ReportNamesMismatchesIfAny(BoundExpression left, BoundExpression right, ImmutableArray<string> leftNames, ImmutableArray<string> rightNames, DiagnosticBag diagnostics)
	{
		bool flag = left.Kind == BoundKind.TupleLiteral;
		bool flag2 = right.Kind == BoundKind.TupleLiteral;
		if (!flag && !flag2)
		{
			return;
		}
		bool isDefault = leftNames.IsDefault;
		bool isDefault2 = rightNames.IsDefault;
		if (isDefault & isDefault2)
		{
			return;
		}
		ImmutableArray<bool> immutableArray = (flag ? ((BoundTupleLiteral)left).InferredNamesOpt : default(ImmutableArray<bool>));
		bool isDefault3 = immutableArray.IsDefault;
		ImmutableArray<bool> immutableArray2 = (flag2 ? ((BoundTupleLiteral)right).InferredNamesOpt : default(ImmutableArray<bool>));
		bool isDefault4 = immutableArray2.IsDefault;
		int num = (isDefault ? rightNames.Length : leftNames.Length);
		for (int i = 0; i < num; i++)
		{
			string text = (isDefault ? null : leftNames[i]);
			string text2 = (isDefault2 ? null : rightNames[i]);
			if (string.CompareOrdinal(text2, text) != 0)
			{
				bool flag3 = !isDefault3 && immutableArray[i];
				bool flag4 = !isDefault4 && immutableArray2[i];
				bool flag5 = flag && text != null && !flag3;
				bool flag6 = flag2 && text2 != null && !flag4;
				if (flag5 || flag6)
				{
					bool flag7 = ((flag5 & flag6) ? flag2 : flag6);
					Location location = ((BoundTupleLiteral)(flag7 ? right : left)).Arguments[i].Syntax.Parent.Location;
					string text3 = (flag7 ? text2 : text);
					diagnostics.Add(ErrorCode.WRN_TupleBinopLiteralNameMismatch, location, text3);
				}
			}
		}
	}

	internal static BoundExpression GiveTupleTypeToDefaultLiteralIfNeeded(BoundExpression expr, TypeSymbol targetType)
	{
		if (!expr.IsLiteralDefault() || (object)targetType == null)
		{
			return expr;
		}
		return new BoundDefaultExpression(expr.Syntax, targetType);
	}

	private static bool IsTupleBinaryOperation(BoundExpression left, BoundExpression right)
	{
		bool flag = left.IsLiteralDefault();
		bool flag2 = right.IsLiteralDefault();
		if (flag & flag2)
		{
			return false;
		}
		if ((GetTupleCardinality(left) > 1) | flag)
		{
			return (GetTupleCardinality(right) > 1) | flag2;
		}
		return false;
	}

	private static int GetTupleCardinality(BoundExpression expr)
	{
		if (expr.Kind == BoundKind.TupleLiteral)
		{
			return ((BoundTupleLiteral)expr).Arguments.Length;
		}
		TypeSymbol type = expr.Type;
		if ((object)type == null)
		{
			return -1;
		}
		type = type.StrippedType();
		if (type.IsTupleType)
		{
			return type.TupleElementTypes.Length;
		}
		return -1;
	}

	private static (ImmutableArray<BoundExpression> Elements, ImmutableArray<string> Names) GetTupleArgumentsOrPlaceholders(BoundExpression expr)
	{
		if (expr.Kind == BoundKind.TupleLiteral)
		{
			BoundTupleLiteral boundTupleLiteral = (BoundTupleLiteral)expr;
			return (Elements: boundTupleLiteral.Arguments, Names: boundTupleLiteral.ArgumentNamesOpt);
		}
		TypeSymbol typeSymbol = expr.Type.StrippedType();
		return (Elements: typeSymbol.TupleElementTypes.SelectAsArray((Func<TypeSymbol, SyntaxNode, BoundExpression>)((TypeSymbol t, SyntaxNode s) => new BoundTupleOperandPlaceholder(s, t)), expr.Syntax), Names: typeSymbol.TupleElementNames);
	}

	private TypeSymbol MakeConvertedType(ImmutableArray<TypeSymbol> convertedTypes, CSharpSyntaxNode syntax, ImmutableArray<BoundExpression> elements, ImmutableArray<string> names, bool isNullable, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		foreach (TypeSymbol item in convertedTypes)
		{
			if ((object)item == null)
			{
				return null;
			}
		}
		ImmutableArray<Location> elementLocations = elements.SelectAsArray((BoundExpression e) => e.Syntax.Location);
		NamedTypeSymbol namedTypeSymbol = TupleTypeSymbol.Create(null, convertedTypes, elementLocations, names, compilation, shouldCheckConstraints: true, default, syntax, diagnostics);
		if (!isNullable)
		{
			return namedTypeSymbol;
		}
		return GetSpecialType(SpecialType.System_Nullable_T, diagnostics, syntax).Construct(namedTypeSymbol);
	}

	internal bool ReportUnsafeIfNotAllowed(SyntaxNode node, DiagnosticBag diagnostics, TypeSymbol sizeOfTypeOpt = null)
	{
		return ReportUnsafeIfNotAllowed(node.Location, diagnostics, sizeOfTypeOpt);
	}

	internal bool ReportUnsafeIfNotAllowed(Location location, DiagnosticBag diagnostics, TypeSymbol sizeOfTypeOpt = null)
	{
		CSDiagnosticInfo unsafeDiagnosticInfo = GetUnsafeDiagnosticInfo(sizeOfTypeOpt);
		if (unsafeDiagnosticInfo == null)
		{
			return false;
		}
		diagnostics.Add(new CSDiagnostic(unsafeDiagnosticInfo, location));
		return true;
	}

	private CSDiagnosticInfo GetUnsafeDiagnosticInfo(TypeSymbol sizeOfTypeOpt)
	{
		if (Flags.Includes(BinderFlags.SuppressUnsafeDiagnostics))
		{
			return null;
		}
		if (IsIndirectlyInIterator)
		{
			return new CSDiagnosticInfo(ErrorCode.ERR_IllegalInnerUnsafe);
		}
		if (!InUnsafeRegion)
		{
			if ((object)sizeOfTypeOpt != null)
			{
				return new CSDiagnosticInfo(ErrorCode.ERR_SizeofUnsafe, sizeOfTypeOpt);
			}
			return new CSDiagnosticInfo(ErrorCode.ERR_UnsafeNeeded);
		}
		return null;
	}

	internal ImmutableArray<Symbol> BindXmlNameAttribute(XmlNameAttributeSyntax syntax, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		IdentifierNameSyntax identifier = syntax.Identifier;
		if (identifier.IsMissing)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		string valueText = identifier.Identifier.ValueText;
		LookupResult instance = LookupResult.GetInstance();
		LookupSymbolsWithFallback(instance, valueText, 0, ref useSiteDiagnostics);
		if (instance.Kind == LookupResultKind.Empty)
		{
			instance.Free();
			return ImmutableArray<Symbol>.Empty;
		}
		ImmutableArray<Symbol> result = instance.Symbols.ToImmutable();
		instance.Free();
		return result;
	}
}
