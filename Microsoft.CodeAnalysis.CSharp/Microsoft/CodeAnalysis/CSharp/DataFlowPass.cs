using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DataFlowPass : AbstractFlowPass<DataFlowPass.LocalState>
{
	internal struct LocalState : AbstractLocalState
	{
		internal BitVector Assigned;

		public bool Reachable
		{
			get
			{
				if (Assigned.Capacity > 0)
				{
					return !IsAssigned(0);
				}
				return true;
			}
		}

		internal LocalState(BitVector assigned)
		{
			Assigned = assigned;
		}

		public LocalState Clone()
		{
			return new LocalState(Assigned.Clone());
		}

		public bool IsAssigned(int slot)
		{
			return Assigned[slot];
		}

		public void Assign(int slot)
		{
			if (slot != -1)
			{
				Assigned[slot] = true;
			}
		}

		public void Unassign(int slot)
		{
			if (slot != -1)
			{
				Assigned[slot] = false;
			}
		}
	}

	private class LocalFuncUsages
	{
		public BitVector ReadVars = BitVector.Empty;

		public LocalState WrittenVars;

		public bool LocalFuncVisited { get; set; }

		public LocalFuncUsages(LocalState unreachableState)
		{
			WrittenVars = unreachableState;
		}
	}

	protected struct VariableIdentifier(Symbol symbol, int containingSlot = 0) : IEquatable<VariableIdentifier>
	{
		public readonly Symbol Symbol = symbol;

		public readonly int ContainingSlot = containingSlot;

		public bool Exists => (object)Symbol != null;

		public override int GetHashCode()
		{
			return Hash.Combine(Symbol, ContainingSlot);
		}

		public bool Equals(VariableIdentifier other)
		{
			if (((object)Symbol == null) ? ((object)other.Symbol == null) : Symbol.Equals(other.Symbol))
			{
				return ContainingSlot == other.ContainingSlot;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			VariableIdentifier? variableIdentifier = obj as VariableIdentifier?;
			if (variableIdentifier.HasValue)
			{
				return Equals(variableIdentifier.Value);
			}
			return false;
		}

		public static bool operator ==(VariableIdentifier left, VariableIdentifier right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(VariableIdentifier left, VariableIdentifier right)
		{
			return !left.Equals(right);
		}
	}

	protected readonly HashSet<Symbol> initiallyAssignedVariables;

	private readonly PooledHashSet<LocalSymbol> _usedVariables = PooledHashSet<LocalSymbol>.GetInstance();

	private readonly PooledHashSet<LocalFunctionSymbol> _usedLocalFunctions = PooledHashSet<LocalFunctionSymbol>.GetInstance();

	private readonly PooledHashSet<Symbol> _writtenVariables = PooledHashSet<Symbol>.GetInstance();

	private readonly PooledDictionary<Symbol, Location> _unsafeAddressTakenVariables = PooledDictionary<Symbol, Location>.GetInstance();

	private readonly PooledHashSet<Symbol> _capturedVariables = PooledHashSet<Symbol>.GetInstance();

	private readonly PooledHashSet<Symbol> _capturedInside = PooledHashSet<Symbol>.GetInstance();

	private readonly PooledHashSet<Symbol> _capturedOutside = PooledHashSet<Symbol>.GetInstance();

	private readonly SourceAssemblySymbol _sourceAssembly;

	private readonly PooledDictionary<VariableIdentifier, int> _variableSlot = PooledDictionary<VariableIdentifier, int>.GetInstance();

	private readonly HashSet<PrefixUnaryExpressionSyntax> _unassignedVariableAddressOfSyntaxes;

	protected VariableIdentifier[] variableBySlot = new VariableIdentifier[1];

	protected int nextVariableSlot = 1;

	private BitVector _alreadyReported;

	private readonly EmptyStructTypeCache _emptyStructTypeCache;

	private readonly bool _requireOutParamsAssigned;

	protected MethodSymbol topLevelMethod;

	protected bool _convertInsufficientExecutionStackExceptionToCancelledByStackGuardException;

	private LocalState? _tryState;

	private readonly SmallDictionary<LocalFunctionSymbol, LocalFuncUsages> _localFuncVarUsages = new SmallDictionary<LocalFunctionSymbol, LocalFuncUsages>();

	protected Symbol currentSymbol { get; private set; }

	protected override void Free()
	{
		_usedVariables.Free();
		_usedLocalFunctions.Free();
		_writtenVariables.Free();
		_capturedVariables.Free();
		_capturedInside.Free();
		_capturedOutside.Free();
		_unsafeAddressTakenVariables.Free();
		_variableSlot.Free();
		base.Free();
	}

	internal DataFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, bool trackUnassignments = false, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes = null, bool requireOutParamsAssigned = true)
		: base(compilation, member, node, trackUnassignments)
	{
		initiallyAssignedVariables = null;
		_sourceAssembly = (((object)member == null) ? null : ((SourceAssemblySymbol)member.ContainingAssembly));
		currentSymbol = member;
		_unassignedVariableAddressOfSyntaxes = unassignedVariableAddressOfSyntaxes;
		bool featureStrictEnabled = compilation.FeatureStrictEnabled;
		_emptyStructTypeCache = new EmptyStructTypeCache(compilation, !featureStrictEnabled);
		_requireOutParamsAssigned = requireOutParamsAssigned;
		topLevelMethod = member as MethodSymbol;
	}

	internal DataFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, EmptyStructTypeCache emptyStructs, bool trackUnassignments = false, HashSet<Symbol> initiallyAssignedVariables = null)
		: base(compilation, member, node, trackUnassignments)
	{
		this.initiallyAssignedVariables = initiallyAssignedVariables;
		_sourceAssembly = (((object)member == null) ? null : ((SourceAssemblySymbol)member.ContainingAssembly));
		currentSymbol = member;
		_unassignedVariableAddressOfSyntaxes = null;
		bool featureStrictEnabled = compilation.FeatureStrictEnabled;
		_emptyStructTypeCache = emptyStructs ?? new EmptyStructTypeCache(compilation, !featureStrictEnabled);
		_requireOutParamsAssigned = true;
		topLevelMethod = member as MethodSymbol;
	}

	internal DataFlowPass(CSharpCompilation compilation, Symbol member, BoundNode node, BoundNode firstInRegion, BoundNode lastInRegion, HashSet<Symbol> initiallyAssignedVariables = null, HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes = null, bool trackUnassignments = false)
		: base(compilation, member, node, firstInRegion, lastInRegion, true, trackUnassignments)
	{
		this.initiallyAssignedVariables = initiallyAssignedVariables;
		_sourceAssembly = null;
		currentSymbol = member;
		_unassignedVariableAddressOfSyntaxes = unassignedVariableAddressOfSyntaxes;
		_emptyStructTypeCache = new NeverEmptyStructTypeCache();
	}

	protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return _convertInsufficientExecutionStackExceptionToCancelledByStackGuardException;
	}

	protected override ImmutableArray<PendingBranch> Scan(ref bool badRegion)
	{
		Diagnostics.Clear();
		ImmutableArray<ParameterSymbol> methodParameters = MethodParameters;
		ParameterSymbol methodThisParameter = MethodThisParameter;
		_alreadyReported = BitVector.Empty;
		State = ReachableState();
		regionPlace = RegionPlace.Before;
		EnterParameters(methodParameters);
		if ((object)methodThisParameter != null)
		{
			EnterParameter(methodThisParameter);
			if (methodThisParameter.Type.SpecialType != SpecialType.None)
			{
				int orCreateSlot = GetOrCreateSlot(methodThisParameter);
				SetSlotState(orCreateSlot, assigned: true);
			}
		}
		ImmutableArray<PendingBranch> result = base.Scan(ref badRegion);
		if (ShouldAnalyzeOutParameters(out var location))
		{
			LeaveParameters(methodParameters, null, location);
			if ((object)methodThisParameter != null)
			{
				LeaveParameter(methodThisParameter, null, location);
			}
			LocalState self = State;
			foreach (PendingBranch item in result)
			{
				State = item.State;
				LeaveParameters(methodParameters, item.Branch.Syntax, null);
				if ((object)methodThisParameter != null)
				{
					LeaveParameter(methodThisParameter, item.Branch.Syntax, null);
				}
				IntersectWith(ref self, ref State);
			}
			State = self;
		}
		return result;
	}

	protected override ImmutableArray<PendingBranch> RemoveReturns()
	{
		ImmutableArray<PendingBranch> immutableArray = base.RemoveReturns();
		if (currentSymbol is MethodSymbol { IsAsync: not false, IsImplicitlyDeclared: false } && !immutableArray.Any((PendingBranch pending) =>
		{
			BoundNode branch = pending.Branch;
			return branch != null && branch.Kind == BoundKind.AwaitExpression;
		}))
		{
			Location location = ((currentSymbol is LambdaSymbol lambdaSymbol) ? lambdaSymbol.DiagnosticLocation : currentSymbol.Locations[0]);
			Diagnostics.Add(ErrorCode.WRN_AsyncLacksAwaits, location);
		}
		return immutableArray;
	}

	protected virtual void ReportUnassignedOutParameter(ParameterSymbol parameter, SyntaxNode node, Location location)
	{
		if ((!_requireOutParamsAssigned && (object)topLevelMethod == currentSymbol) || Diagnostics == null || !State.Reachable)
		{
			return;
		}
		if (location == null)
		{
			location = new SourceLocation(node);
		}
		bool flag = false;
		if (parameter.IsThis)
		{
			int num = VariableSlot(parameter);
			if (!State.IsAssigned(num))
			{
				foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(parameter.Type))
				{
					if (_emptyStructTypeCache.IsEmptyStructType(structInstanceField.Type))
					{
						continue;
					}
					SourceMemberFieldSymbol obj = structInstanceField as SourceMemberFieldSymbol;
					if ((object)obj != null && obj.HasInitializer)
					{
						continue;
					}
					SynthesizedBackingFieldSymbol obj2 = structInstanceField as SynthesizedBackingFieldSymbol;
					if ((object)obj2 != null && obj2.HasInitializer)
					{
						continue;
					}
					int num2 = VariableSlot(structInstanceField, num);
					if (num2 == -1 || !State.IsAssigned(num2))
					{
						Symbol associatedSymbol = structInstanceField.AssociatedSymbol;
						if ((object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Property)
						{
							Diagnostics.Add(ErrorCode.ERR_UnassignedThisAutoProperty, location, associatedSymbol);
						}
						else
						{
							Diagnostics.Add(ErrorCode.ERR_UnassignedThis, location, structInstanceField);
						}
					}
				}
				flag = true;
			}
		}
		if (!flag)
		{
			Diagnostics.Add(ErrorCode.ERR_ParamUnassigned, location, parameter.Name);
		}
	}

	public static void Analyze(CSharpCompilation compilation, Symbol member, BoundNode node, DiagnosticBag diagnostics, bool requireOutParamsAssigned = true)
	{
		DataFlowPass dataFlowPass = new DataFlowPass(compilation, member, node, trackUnassignments: false, null, requireOutParamsAssigned);
		if (diagnostics != null)
		{
			dataFlowPass._convertInsufficientExecutionStackExceptionToCancelledByStackGuardException = true;
		}
		try
		{
			bool badRegion = false;
			dataFlowPass.Analyze(ref badRegion, diagnostics);
		}
		catch (CancelledByStackGuardException ex) when (diagnostics != null)
		{
			ex.AddAnError(diagnostics);
		}
		finally
		{
			dataFlowPass.Free();
		}
	}

	protected void Analyze(ref bool badRegion, DiagnosticBag diagnostics)
	{
		Analyze(ref badRegion);
		if (diagnostics == null)
		{
			return;
		}
		foreach (Symbol capturedVariable in _capturedVariables)
		{
			if (_unsafeAddressTakenVariables.TryGetValue(capturedVariable, out var value))
			{
				diagnostics.Add(ErrorCode.ERR_LocalCantBeFixedAndHoisted, value, capturedVariable.Name);
			}
		}
		diagnostics.AddRange(Diagnostics);
	}

	private void CheckCaptured(Symbol variable, ParameterSymbol rangeVariableUnderlyingParameter = null)
	{
		if (IsCaptured(rangeVariableUnderlyingParameter ?? variable, currentSymbol))
		{
			NoteCaptured(variable);
		}
	}

	private static bool IsCaptured(Symbol variable, Symbol containingSymbol)
	{
		switch (variable.Kind)
		{
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
		case SymbolKind.RangeVariable:
			return false;
		case SymbolKind.Local:
			if (((LocalSymbol)variable).IsConst)
			{
				return false;
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(variable.Kind);
		case SymbolKind.Parameter:
			break;
		}
		Symbol containingSymbol2 = variable.ContainingSymbol;
		while ((object)containingSymbol2 != null)
		{
			if ((object)containingSymbol2 == containingSymbol)
			{
				return false;
			}
			containingSymbol2 = containingSymbol2.ContainingSymbol;
		}
		return true;
	}

	private void NoteCaptured(Symbol variable)
	{
		if (regionPlace == RegionPlace.Inside)
		{
			_capturedInside.Add(variable);
			_capturedVariables.Add(variable);
		}
		else if (variable.Kind != SymbolKind.RangeVariable)
		{
			_capturedOutside.Add(variable);
			_capturedVariables.Add(variable);
		}
	}

	protected IEnumerable<Symbol> GetCapturedInside()
	{
		return _capturedInside.ToArray();
	}

	protected IEnumerable<Symbol> GetCapturedOutside()
	{
		return _capturedOutside.ToArray();
	}

	protected IEnumerable<Symbol> GetCaptured()
	{
		return _capturedVariables.ToArray();
	}

	protected IEnumerable<Symbol> GetUnsafeAddressTaken()
	{
		return _unsafeAddressTakenVariables.Keys.ToArray();
	}

	protected virtual void NoteRead(Symbol variable, ParameterSymbol rangeVariableUnderlyingParameter = null)
	{
		if (variable is LocalSymbol item)
		{
			_usedVariables.Add(item);
		}
		if (variable is LocalFunctionSymbol item2)
		{
			_usedLocalFunctions.Add(item2);
		}
		if ((object)variable != null)
		{
			if ((object)_sourceAssembly != null && variable.Kind == SymbolKind.Field)
			{
				_sourceAssembly.NoteFieldAccess((FieldSymbol)variable.OriginalDefinition, read: true, write: false);
			}
			CheckCaptured(variable, rangeVariableUnderlyingParameter);
		}
	}

	private void NoteRead(BoundNode fieldOrEventAccess)
	{
		BoundNode boundNode = fieldOrEventAccess;
		while (boundNode != null)
		{
			switch (boundNode.Kind)
			{
			default:
				return;
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundNode;
				NoteRead(boundFieldAccess.FieldSymbol);
				if (PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol))
				{
					boundNode = boundFieldAccess.ReceiverOpt;
					break;
				}
				return;
			}
			case BoundKind.EventAccess:
			{
				BoundEventAccess boundEventAccess = (BoundEventAccess)boundNode;
				FieldSymbol associatedField = boundEventAccess.EventSymbol.AssociatedField;
				if ((object)associatedField != null)
				{
					NoteRead(associatedField);
					if (PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundEventAccess.ReceiverOpt, associatedField))
					{
						boundNode = boundEventAccess.ReceiverOpt;
						break;
					}
					return;
				}
				return;
			}
			case BoundKind.ThisReference:
				NoteRead(MethodThisParameter);
				return;
			case BoundKind.Local:
				NoteRead(((BoundLocal)boundNode).LocalSymbol);
				return;
			case BoundKind.Parameter:
				NoteRead(((BoundParameter)boundNode).ParameterSymbol);
				return;
			}
		}
	}

	protected virtual void NoteWrite(Symbol variable, BoundExpression value, bool read)
	{
		if ((object)variable != null)
		{
			_writtenVariables.Add(variable);
			if ((object)_sourceAssembly != null && variable.Kind == SymbolKind.Field)
			{
				FieldSymbol fieldSymbol = (FieldSymbol)variable.OriginalDefinition;
				_sourceAssembly.NoteFieldAccess(fieldSymbol, read && WriteConsideredUse(fieldSymbol.Type, value), write: true);
			}
			LocalSymbol localSymbol = variable as LocalSymbol;
			if ((((object)localSymbol != null) & read) && WriteConsideredUse(localSymbol.Type, value))
			{
				_usedVariables.Add(localSymbol);
			}
			CheckCaptured(variable);
		}
	}

	internal static bool WriteConsideredUse(TypeSymbol type, BoundExpression value)
	{
		if (value == null || value.HasAnyErrors)
		{
			return true;
		}
		if ((object)type != null && type.IsReferenceType && type.SpecialType != SpecialType.System_String)
		{
			return value.ConstantValue != ConstantValue.Null;
		}
		if ((object)type != null && type.IsPointerType())
		{
			return true;
		}
		if (value.ConstantValue != null)
		{
			return false;
		}
		switch (value.Kind)
		{
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)value;
			if (boundConversion.ConversionKind.IsUserDefinedConversion() || boundConversion.ConversionKind == ConversionKind.IntPtr)
			{
				return true;
			}
			return WriteConsideredUse(null, boundConversion.Operand);
		}
		case BoundKind.DefaultExpression:
			return false;
		case BoundKind.ObjectCreationExpression:
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)value;
			if (boundObjectCreationExpression.Constructor.IsImplicitlyDeclared)
			{
				return boundObjectCreationExpression.InitializerExpressionOpt != null;
			}
			return true;
		}
		case BoundKind.TupleLiteral:
		case BoundKind.ConvertedTupleLiteral:
			return false;
		default:
			return true;
		}
	}

	private void NoteWrite(BoundExpression n, BoundExpression value, bool read)
	{
		while (n != null)
		{
			switch (n.Kind)
			{
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)n;
				if ((object)_sourceAssembly != null)
				{
					FieldSymbol originalDefinition2 = boundFieldAccess.FieldSymbol.OriginalDefinition;
					_sourceAssembly.NoteFieldAccess(originalDefinition2, value == null || WriteConsideredUse(boundFieldAccess.FieldSymbol.Type, value), write: true);
				}
				if (PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol))
				{
					n = boundFieldAccess.ReceiverOpt;
					if (n.Kind == BoundKind.Local)
					{
						_usedVariables.Add(((BoundLocal)n).LocalSymbol);
					}
					break;
				}
				return;
			}
			case BoundKind.EventAccess:
			{
				BoundEventAccess boundEventAccess = (BoundEventAccess)n;
				FieldSymbol associatedField = boundEventAccess.EventSymbol.AssociatedField;
				if ((object)associatedField != null)
				{
					if ((object)_sourceAssembly != null)
					{
						FieldSymbol originalDefinition = associatedField.OriginalDefinition;
						_sourceAssembly.NoteFieldAccess(originalDefinition, value == null || WriteConsideredUse(associatedField.Type, value), write: true);
					}
					if (PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundEventAccess.ReceiverOpt, associatedField))
					{
						n = boundEventAccess.ReceiverOpt;
						break;
					}
					return;
				}
				return;
			}
			case BoundKind.ThisReference:
				NoteWrite(MethodThisParameter, value, read);
				return;
			case BoundKind.Local:
				NoteWrite(((BoundLocal)n).LocalSymbol, value, read);
				return;
			case BoundKind.Parameter:
				NoteWrite(((BoundParameter)n).ParameterSymbol, value, read);
				return;
			case BoundKind.RangeVariable:
				NoteWrite(((BoundRangeVariable)n).Value, value, read);
				return;
			default:
				return;
			}
		}
	}

	protected int VariableSlot(Symbol symbol, int containingSlot = 0)
	{
		containingSlot = DescendThroughTupleRestFields(ref symbol, containingSlot, forceContainingSlotsToExist: false);
		if (!_variableSlot.TryGetValue(new VariableIdentifier(symbol, containingSlot), out var value))
		{
			return -1;
		}
		return value;
	}

	protected int GetOrCreateSlot(Symbol symbol, int containingSlot = 0)
	{
		if (symbol is RangeVariableSymbol)
		{
			return -1;
		}
		containingSlot = DescendThroughTupleRestFields(ref symbol, containingSlot, forceContainingSlotsToExist: true);
		VariableIdentifier variableIdentifier = new VariableIdentifier(symbol, containingSlot);
		if (!_variableSlot.TryGetValue(variableIdentifier, out var value))
		{
			TypeSymbol type = VariableType(symbol);
			if (_emptyStructTypeCache.IsEmptyStructType(type))
			{
				return -1;
			}
			value = nextVariableSlot++;
			_variableSlot.Add(variableIdentifier, value);
			if (value >= variableBySlot.Length)
			{
				Array.Resize(ref variableBySlot, value * 2);
			}
			variableBySlot[value] = variableIdentifier;
		}
		Normalize(ref State);
		return value;
	}

	private int DescendThroughTupleRestFields(ref Symbol symbol, int containingSlot, bool forceContainingSlotsToExist)
	{
		if (symbol is TupleFieldSymbol tupleFieldSymbol)
		{
			TypeSymbol typeSymbol = ((TupleTypeSymbol)symbol.ContainingType).UnderlyingNamedType;
			symbol = tupleFieldSymbol.TupleUnderlyingField;
			while (typeSymbol != symbol.ContainingType)
			{
				if (!(typeSymbol.GetMembers("Rest").FirstOrDefault() is FieldSymbol fieldSymbol))
				{
					return -1;
				}
				if (forceContainingSlotsToExist)
				{
					containingSlot = GetOrCreateSlot(fieldSymbol, containingSlot);
				}
				else if (!_variableSlot.TryGetValue(new VariableIdentifier(fieldSymbol, containingSlot), out containingSlot))
				{
					return -1;
				}
				typeSymbol = fieldSymbol.Type.TupleUnderlyingTypeOrSelf();
			}
		}
		return containingSlot;
	}

	private void Normalize(ref LocalState state)
	{
		int capacity = state.Assigned.Capacity;
		state.Assigned.EnsureCapacity(nextVariableSlot);
		for (int i = capacity; i < nextVariableSlot; i++)
		{
			VariableIdentifier variableIdentifier = variableBySlot[i];
			state.Assigned[i] = variableIdentifier.ContainingSlot > 0 && state.Assigned[variableIdentifier.ContainingSlot];
		}
	}

	protected int MakeSlot(BoundExpression node)
	{
		switch (node.Kind)
		{
		case BoundKind.ThisReference:
			if ((object)MethodThisParameter == null)
			{
				return -1;
			}
			return GetOrCreateSlot(MethodThisParameter);
		case BoundKind.BaseReference:
			return GetOrCreateSlot(MethodThisParameter);
		case BoundKind.Local:
			return GetOrCreateSlot(((BoundLocal)node).LocalSymbol);
		case BoundKind.Parameter:
			return GetOrCreateSlot(((BoundParameter)node).ParameterSymbol);
		case BoundKind.RangeVariable:
			return MakeSlot(((BoundRangeVariable)node).Value);
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)node;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			BoundExpression receiverOpt2 = boundFieldAccess.ReceiverOpt;
			if (fieldSymbol.IsStatic || receiverOpt2 == null || receiverOpt2.Kind == BoundKind.TypeExpression)
			{
				return -1;
			}
			if (fieldSymbol.IsFixed)
			{
				return -1;
			}
			if ((object)receiverOpt2.Type == null || receiverOpt2.Type.TypeKind != TypeKind.Struct)
			{
				return -1;
			}
			int num2 = MakeSlot(receiverOpt2);
			if (num2 != -1)
			{
				return GetOrCreateSlot(fieldSymbol, num2);
			}
			return -1;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)node;
			EventSymbol eventSymbol = boundEventAccess.EventSymbol;
			BoundExpression receiverOpt3 = boundEventAccess.ReceiverOpt;
			if (eventSymbol.IsStatic || receiverOpt3 == null || receiverOpt3.Kind == BoundKind.TypeExpression)
			{
				return -1;
			}
			if ((object)receiverOpt3.Type == null || receiverOpt3.Type.TypeKind != TypeKind.Struct)
			{
				return -1;
			}
			if (!eventSymbol.HasAssociatedField)
			{
				return -1;
			}
			int num3 = MakeSlot(receiverOpt3);
			if (num3 != -1)
			{
				return GetOrCreateSlot(eventSymbol.AssociatedField, num3);
			}
			return -1;
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node;
			if (!Binder.AccessingAutoPropertyFromConstructor(boundPropertyAccess, currentSymbol))
			{
				break;
			}
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (propertySymbol as SourcePropertySymbol)?.BackingField;
			if (synthesizedBackingFieldSymbol != null)
			{
				BoundExpression receiverOpt = boundPropertyAccess.ReceiverOpt;
				if (propertySymbol.IsStatic || receiverOpt == null || receiverOpt.Kind == BoundKind.TypeExpression)
				{
					return -1;
				}
				if ((object)receiverOpt.Type == null || receiverOpt.Type.TypeKind != TypeKind.Struct)
				{
					return -1;
				}
				int num = MakeSlot(receiverOpt);
				if (num != -1)
				{
					return GetOrCreateSlot(synthesizedBackingFieldSymbol, num);
				}
				return -1;
			}
			break;
		}
		}
		return -1;
	}

	protected void CheckAssigned(Symbol symbol, SyntaxNode node)
	{
		if ((object)symbol == null)
		{
			return;
		}
		NoteRead(symbol);
		if (State.Reachable)
		{
			int num = VariableSlot(symbol);
			if (num >= State.Assigned.Capacity)
			{
				Normalize(ref State);
			}
			if (num > 0 && !State.IsAssigned(num))
			{
				ReportUnassignedIfNotCapturedInLocalFunction(symbol, node, num);
			}
		}
	}

	private void ReportUnassignedIfNotCapturedInLocalFunction(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration = true)
	{
		if (IsCapturedInLocalFunction(slot))
		{
			RecordReadInLocalFunction(slot);
		}
		else
		{
			ReportUnassigned(symbol, node, slot, skipIfUseBeforeDeclaration);
		}
	}

	protected virtual void ReportUnassigned(Symbol symbol, SyntaxNode node, int slot, bool skipIfUseBeforeDeclaration)
	{
		if (slot <= 0 || symbol is LocalSymbol { IsConst: not false })
		{
			return;
		}
		if (slot >= _alreadyReported.Capacity)
		{
			_alreadyReported.EnsureCapacity(nextVariableSlot);
		}
		if ((!skipIfUseBeforeDeclaration || symbol.Kind != SymbolKind.Local || (symbol.Locations.Length != 0 && node.Span.End >= symbol.Locations[0].SourceSpan.Start)) && !_alreadyReported[slot])
		{
			TypeSymbol typeSymbol = VariableType(symbol);
			if ((object)typeSymbol == null || !typeSymbol.IsErrorType())
			{
				string name = symbol.Name;
				ErrorCode code;
				if (symbol.Kind != SymbolKind.Field)
				{
					if (symbol.Kind != SymbolKind.Parameter || ((ParameterSymbol)symbol).RefKind != RefKind.Out)
					{
						code = ErrorCode.ERR_UseDefViolation;
					}
					else
					{
						code = ((!((ParameterSymbol)symbol).IsThis) ? ErrorCode.ERR_UseDefViolationOut : ErrorCode.ERR_UseDefViolationThis);
					}
				}
				else
				{
					Symbol associatedSymbol = ((FieldSymbol)symbol).AssociatedSymbol;
					if ((object)associatedSymbol != null && associatedSymbol.Kind == SymbolKind.Property)
					{
						code = ErrorCode.ERR_UseDefViolationProperty;
						name = associatedSymbol.Name;
					}
					else
					{
						code = ErrorCode.ERR_UseDefViolationField;
					}
				}
				Diagnostics.Add(code, new SourceLocation(node), name);
			}
		}
		_alreadyReported[slot] = true;
	}

	protected virtual void CheckAssigned(BoundExpression expr, FieldSymbol fieldSymbol, SyntaxNode node)
	{
		if (State.Reachable && !IsAssigned(expr, out var unassignedSlot))
		{
			ReportUnassignedIfNotCapturedInLocalFunction(fieldSymbol, node, unassignedSlot);
		}
		NoteRead(expr);
	}

	private bool IsAssigned(BoundExpression node, out int unassignedSlot)
	{
		unassignedSlot = -1;
		if (_emptyStructTypeCache.IsEmptyStructType(node.Type))
		{
			return true;
		}
		switch (node.Kind)
		{
		case BoundKind.ThisReference:
			if ((object)MethodThisParameter == null)
			{
				unassignedSlot = -1;
				return true;
			}
			unassignedSlot = GetOrCreateSlot(MethodThisParameter);
			break;
		case BoundKind.Local:
			unassignedSlot = GetOrCreateSlot(((BoundLocal)node).LocalSymbol);
			break;
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)node;
			if (!PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundFieldAccess.ReceiverOpt, boundFieldAccess.FieldSymbol) || IsAssigned(boundFieldAccess.ReceiverOpt, out unassignedSlot))
			{
				return true;
			}
			unassignedSlot = GetOrCreateSlot(boundFieldAccess.FieldSymbol, unassignedSlot);
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)node;
			if (!PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundEventAccess.ReceiverOpt, boundEventAccess.EventSymbol.AssociatedField) || IsAssigned(boundEventAccess.ReceiverOpt, out unassignedSlot))
			{
				return true;
			}
			unassignedSlot = GetOrCreateSlot(boundEventAccess.EventSymbol.AssociatedField, unassignedSlot);
			break;
		}
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)node;
			if (Binder.AccessingAutoPropertyFromConstructor(boundPropertyAccess, currentSymbol))
			{
				SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (boundPropertyAccess.PropertySymbol as SourcePropertySymbol)?.BackingField;
				if (synthesizedBackingFieldSymbol != null)
				{
					if (!PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundPropertyAccess.ReceiverOpt, synthesizedBackingFieldSymbol) || IsAssigned(boundPropertyAccess.ReceiverOpt, out unassignedSlot))
					{
						return true;
					}
					unassignedSlot = GetOrCreateSlot(synthesizedBackingFieldSymbol, unassignedSlot);
					break;
				}
			}
			goto default;
		}
		case BoundKind.Parameter:
		{
			BoundParameter boundParameter = (BoundParameter)node;
			unassignedSlot = GetOrCreateSlot(boundParameter.ParameterSymbol);
			break;
		}
		default:
			unassignedSlot = -1;
			return true;
		}
		return State.IsAssigned(unassignedSlot);
	}

	protected Symbol GetNonFieldSymbol(int slot)
	{
		VariableIdentifier variableIdentifier = variableBySlot[slot];
		while (variableIdentifier.ContainingSlot > 0)
		{
			variableIdentifier = variableBySlot[variableIdentifier.ContainingSlot];
		}
		return variableIdentifier.Symbol;
	}

	private Symbol UseNonFieldSymbolUnsafely(BoundExpression expression)
	{
		while (expression != null)
		{
			BoundFieldAccess boundFieldAccess;
			switch (expression.Kind)
			{
			case BoundKind.FieldAccess:
			{
				boundFieldAccess = (BoundFieldAccess)expression;
				FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
				if ((object)_sourceAssembly != null)
				{
					_sourceAssembly.NoteFieldAccess(fieldSymbol, read: true, write: true);
				}
				if (fieldSymbol.ContainingType.IsReferenceType || fieldSymbol.IsStatic)
				{
					return null;
				}
				break;
			}
			case BoundKind.Local:
			{
				LocalSymbol localSymbol = ((BoundLocal)expression).LocalSymbol;
				_usedVariables.Add(localSymbol);
				return localSymbol;
			}
			case BoundKind.RangeVariable:
				return ((BoundRangeVariable)expression).RangeVariableSymbol;
			case BoundKind.Parameter:
				return ((BoundParameter)expression).ParameterSymbol;
			case BoundKind.ThisReference:
				return MethodThisParameter;
			case BoundKind.BaseReference:
				return MethodThisParameter;
			default:
				return null;
			}
			expression = boundFieldAccess.ReceiverOpt;
		}
		return null;
	}

	protected void Assign(BoundNode node, BoundExpression value, bool isRef = false, bool read = true)
	{
		AssignImpl(node, value, isRef, written: true, read);
	}

	protected virtual void AssignImpl(BoundNode node, BoundExpression value, bool isRef, bool written, bool read)
	{
		switch (node.Kind)
		{
		case BoundKind.DeclarationPattern:
		{
			BoundDeclarationPattern boundDeclarationPattern = (BoundDeclarationPattern)node;
			if (boundDeclarationPattern.Variable is LocalSymbol symbol)
			{
				int orCreateSlot2 = GetOrCreateSlot(symbol);
				SetSlotState(orCreateSlot2, written || !State.Reachable);
			}
			if (written)
			{
				NoteWrite(boundDeclarationPattern.VariableAccess, value, read);
			}
			break;
		}
		case BoundKind.LocalDeclaration:
		{
			LocalSymbol localSymbol = ((BoundLocalDeclaration)node).LocalSymbol;
			int orCreateSlot = GetOrCreateSlot(localSymbol);
			SetSlotState(orCreateSlot, written || !State.Reachable);
			if (written)
			{
				NoteWrite(localSymbol, value, read);
			}
			break;
		}
		case BoundKind.Local:
		{
			BoundLocal boundLocal = (BoundLocal)node;
			if (boundLocal.LocalSymbol.RefKind != RefKind.None && !isRef)
			{
				if (written)
				{
					VisitRvalue(boundLocal);
				}
				break;
			}
			int slot = MakeSlot(boundLocal);
			SetSlotState(slot, written);
			if (written)
			{
				NoteWrite(boundLocal, value, read);
			}
			break;
		}
		case BoundKind.Parameter:
		{
			BoundParameter boundParameter = (BoundParameter)node;
			ParameterSymbol parameterSymbol = boundParameter.ParameterSymbol;
			if (isRef && parameterSymbol.RefKind == RefKind.Out)
			{
				LeaveParameter(parameterSymbol, node.Syntax, boundParameter.Syntax.Location);
			}
			int slot3 = MakeSlot(boundParameter);
			SetSlotState(slot3, written);
			if (written)
			{
				NoteWrite(boundParameter, value, read);
			}
			break;
		}
		case BoundKind.ThisReference:
		case BoundKind.FieldAccess:
		case BoundKind.PropertyAccess:
		case BoundKind.EventAccess:
		{
			BoundExpression boundExpression = (BoundExpression)node;
			int slot2 = MakeSlot(boundExpression);
			SetSlotState(slot2, written);
			if (written)
			{
				NoteWrite(boundExpression, value, read);
			}
			break;
		}
		case BoundKind.RangeVariable:
			AssignImpl(((BoundRangeVariable)node).Value, value, isRef, written, read);
			break;
		case BoundKind.BadExpression:
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)node;
			if (!boundBadExpression.ChildBoundNodes.IsDefault && boundBadExpression.ChildBoundNodes.Length == 1)
			{
				AssignImpl(boundBadExpression.ChildBoundNodes[0], value, isRef, written, read);
			}
			break;
		}
		case BoundKind.TupleLiteral:
			((BoundTupleExpression)node).VisitAllElements((BoundExpression x, DataFlowPass self) =>
			{
				self.Assign(x, null, isRef);
			}, this);
			break;
		}
	}

	private bool FieldsAllSet(int containingSlot, LocalState state)
	{
		NamedTypeSymbol type = VariableType(variableBySlot[containingSlot].Symbol) as NamedTypeSymbol;
		foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
		{
			if (!_emptyStructTypeCache.IsEmptyStructType(structInstanceField.Type))
			{
				int num = VariableSlot(structInstanceField, containingSlot);
				if (num == -1 || !state.IsAssigned(num))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static TypeSymbol VariableType(Symbol s)
	{
		return s.Kind switch
		{
			SymbolKind.Local => ((LocalSymbol)s).Type, 
			SymbolKind.Field => ((FieldSymbol)s).Type, 
			SymbolKind.Parameter => ((ParameterSymbol)s).Type, 
			SymbolKind.Method => null, 
			_ => throw ExceptionUtilities.UnexpectedValue(s.Kind), 
		};
	}

	protected void SetSlotState(int slot, bool assigned)
	{
		if (slot > 0)
		{
			if (assigned)
			{
				SetSlotAssigned(slot);
			}
			else
			{
				SetSlotUnassigned(slot);
			}
		}
	}

	private void SetSlotAssigned(int slot, ref LocalState state)
	{
		if (slot < 0)
		{
			return;
		}
		VariableIdentifier variableIdentifier = variableBySlot[slot];
		TypeSymbol type = VariableType(variableIdentifier.Symbol);
		if (slot >= state.Assigned.Capacity)
		{
			Normalize(ref state);
		}
		if (state.IsAssigned(slot))
		{
			return;
		}
		state.Assign(slot);
		if (EmptyStructTypeCache.IsTrackableStructType(type))
		{
			foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
			{
				int num = VariableSlot(structInstanceField, slot);
				if (num > 0)
				{
					SetSlotAssigned(num, ref state);
				}
			}
		}
		while (variableIdentifier.ContainingSlot > 0)
		{
			slot = variableIdentifier.ContainingSlot;
			if (!state.IsAssigned(slot) && FieldsAllSet(slot, state))
			{
				state.Assign(slot);
				variableIdentifier = variableBySlot[slot];
				continue;
			}
			break;
		}
	}

	private void SetSlotAssigned(int slot)
	{
		SetSlotAssigned(slot, ref State);
	}

	private void SetSlotUnassigned(int slot, ref LocalState state)
	{
		if (slot < 0)
		{
			return;
		}
		VariableIdentifier variableIdentifier = variableBySlot[slot];
		TypeSymbol type = VariableType(variableIdentifier.Symbol);
		if (!state.IsAssigned(slot))
		{
			return;
		}
		state.Unassign(slot);
		if (EmptyStructTypeCache.IsTrackableStructType(type))
		{
			foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
			{
				int num = VariableSlot(structInstanceField, slot);
				if (num > 0)
				{
					SetSlotUnassigned(num, ref state);
				}
			}
		}
		while (variableIdentifier.ContainingSlot > 0)
		{
			slot = variableIdentifier.ContainingSlot;
			state.Unassign(slot);
			variableIdentifier = variableBySlot[slot];
		}
	}

	private void SetSlotUnassigned(int slot)
	{
		if (_tryState.HasValue)
		{
			LocalState state = _tryState.Value;
			SetSlotUnassigned(slot, ref state);
			_tryState = state;
		}
		SetSlotUnassigned(slot, ref State);
	}

	protected override LocalState ReachableState()
	{
		return new LocalState(BitVector.Empty);
	}

	protected override LocalState AllBitsSet()
	{
		LocalState result = new LocalState(BitVector.AllSet(nextVariableSlot));
		result.Assigned[0] = false;
		return result;
	}

	private void EnterParameters(ImmutableArray<ParameterSymbol> parameters)
	{
		foreach (ParameterSymbol item in parameters)
		{
			EnterParameter(item);
		}
	}

	protected virtual void EnterParameter(ParameterSymbol parameter)
	{
		if (parameter.RefKind == RefKind.Out && !(currentSymbol is MethodSymbol { IsAsync: not false }))
		{
			int orCreateSlot = GetOrCreateSlot(parameter);
			if (orCreateSlot > 0)
			{
				SetSlotState(orCreateSlot, initiallyAssignedVariables?.Contains(parameter) ?? false);
			}
			return;
		}
		int orCreateSlot2 = GetOrCreateSlot(parameter);
		if (orCreateSlot2 > 0)
		{
			SetSlotState(orCreateSlot2, assigned: true);
		}
		NoteWrite(parameter, null, read: true);
	}

	private void LeaveParameters(ImmutableArray<ParameterSymbol> parameters, SyntaxNode syntax, Location location)
	{
		if (State.Reachable)
		{
			foreach (ParameterSymbol item in parameters)
			{
				LeaveParameter(item, syntax, location);
			}
		}
	}

	private void LeaveParameter(ParameterSymbol parameter, SyntaxNode syntax, Location location)
	{
		if (parameter.RefKind != RefKind.None)
		{
			int num = VariableSlot(parameter);
			if (num > 0 && !State.IsAssigned(num))
			{
				ReportUnassignedOutParameter(parameter, syntax, location);
			}
			NoteRead(parameter);
		}
	}

	protected override LocalState UnreachableState()
	{
		LocalState result = State.Clone();
		result.Assigned.EnsureCapacity(1);
		result.Assign(0);
		return result;
	}

	public override void VisitPattern(BoundExpression expression, BoundPattern pattern)
	{
		base.VisitPattern(expression, pattern);
		LocalState stateWhenFalse = StateWhenFalse;
		SetState(StateWhenTrue);
		AssignPatternVariables(pattern);
		SetConditionalState(State, stateWhenFalse);
	}

	private void AssignPatternVariables(BoundPattern pattern)
	{
		switch (pattern.Kind)
		{
		case BoundKind.DeclarationPattern:
		{
			BoundDeclarationPattern node = (BoundDeclarationPattern)pattern;
			Assign(node, null, isRef: false, read: false);
			break;
		}
		case BoundKind.ConstantPattern:
		{
			BoundConstantPattern boundConstantPattern = (BoundConstantPattern)pattern;
			VisitRvalue(boundConstantPattern.Value);
			break;
		}
		case BoundKind.WildcardPattern:
			break;
		}
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		DeclareVariables(node.Locals);
		VisitStatementsWithLocalFunctions(node);
		ReportUnusedVariables(node.Locals);
		ReportUnusedVariables(node.LocalFunctions);
		return null;
	}

	private void VisitStatementsWithLocalFunctions(BoundBlock block)
	{
		if (!block.LocalFunctions.IsDefaultOrEmpty)
		{
			foreach (BoundStatement statement in block.Statements)
			{
				if (statement.Kind == BoundKind.LocalFunctionStatement)
				{
					VisitAlways(statement);
				}
			}
		}
		foreach (BoundStatement statement2 in block.Statements)
		{
			if (statement2.Kind != BoundKind.LocalFunctionStatement)
			{
				VisitStatement(statement2);
			}
		}
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		DeclareVariables(node.InnerLocals);
		BoundNode result = base.VisitSwitchStatement(node);
		ReportUnusedVariables(node.InnerLocals);
		ReportUnusedVariables(node.InnerLocalFunctions);
		return result;
	}

	public override BoundNode VisitPatternSwitchStatement(BoundPatternSwitchStatement node)
	{
		DeclareVariables(node.InnerLocals);
		BoundNode result = base.VisitPatternSwitchStatement(node);
		ReportUnusedVariables(node.InnerLocals);
		ReportUnusedVariables(node.InnerLocalFunctions);
		return result;
	}

	protected override void VisitPatternSwitchSection(BoundPatternSwitchSection node, BoundExpression switchExpression, bool isLastSection)
	{
		DeclareVariables(node.Locals);
		base.VisitPatternSwitchSection(node, switchExpression, isLastSection);
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		DeclareVariables(node.OuterLocals);
		DeclareVariables(node.InnerLocals);
		BoundNode result = base.VisitForStatement(node);
		ReportUnusedVariables(node.InnerLocals);
		ReportUnusedVariables(node.OuterLocals);
		return result;
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		return base.VisitForEachStatement(node);
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		DeclareVariables(node.Locals);
		BoundNode result = base.VisitDoStatement(node);
		ReportUnusedVariables(node.Locals);
		return result;
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		DeclareVariables(node.Locals);
		BoundNode result = base.VisitWhileStatement(node);
		ReportUnusedVariables(node.Locals);
		return result;
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		return base.VisitIfStatement(node);
	}

	public override BoundNode VisitLockStatement(BoundLockStatement node)
	{
		return base.VisitLockStatement(node);
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		ImmutableArray<LocalSymbol> locals = node.Locals;
		DeclareVariables(locals);
		BoundNode result = base.VisitUsingStatement(node);
		if (!locals.IsDefaultOrEmpty)
		{
			foreach (LocalSymbol item in locals)
			{
				if (item.DeclarationKind == LocalDeclarationKind.UsingVariable)
				{
					NoteRead(item);
				}
			}
		}
		return result;
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		DeclareVariables(node.Locals);
		return base.VisitFixedStatement(node);
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		DeclareVariables(node.Locals);
		BoundNode result = base.VisitSequence(node);
		ReportUnusedVariables(node.Locals);
		return result;
	}

	private void DeclareVariables(ImmutableArray<LocalSymbol> locals)
	{
		foreach (LocalSymbol item in locals)
		{
			DeclareVariable(item);
		}
	}

	private void DeclareVariable(LocalSymbol symbol)
	{
		bool assigned = symbol.IsConst || (initiallyAssignedVariables?.Contains(symbol) ?? false);
		SetSlotState(GetOrCreateSlot(symbol), assigned);
	}

	private void ReportUnusedVariables(ImmutableArray<LocalSymbol> locals)
	{
		foreach (LocalSymbol item in locals)
		{
			ReportIfUnused(item, assigned: true);
		}
	}

	private void ReportIfUnused(LocalSymbol symbol, bool assigned)
	{
		if (!_usedVariables.Contains(symbol) && symbol.DeclarationKind != LocalDeclarationKind.PatternVariable && !string.IsNullOrEmpty(symbol.Name))
		{
			Diagnostics.Add((assigned && _writtenVariables.Contains(symbol)) ? ErrorCode.WRN_UnreferencedVarAssg : ErrorCode.WRN_UnreferencedVar, symbol.Locations[0], symbol.Name);
		}
	}

	private void ReportUnusedVariables(ImmutableArray<LocalFunctionSymbol> locals)
	{
		foreach (LocalFunctionSymbol item in locals)
		{
			ReportIfUnused(item);
		}
	}

	private void ReportIfUnused(LocalFunctionSymbol symbol)
	{
		if (!_usedLocalFunctions.Contains(symbol) && !string.IsNullOrEmpty(symbol.Name))
		{
			Diagnostics.Add(ErrorCode.WRN_UnreferencedLocalFunction, symbol.Locations[0], symbol.Name);
		}
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		LocalSymbol localSymbol = node.LocalSymbol;
		CheckAssigned(localSymbol, node.Syntax);
		if (localSymbol.IsFixed && currentSymbol is MethodSymbol methodSymbol && (methodSymbol.MethodKind == MethodKind.AnonymousFunction || methodSymbol.MethodKind == MethodKind.LocalFunction) && _capturedVariables.Contains(localSymbol))
		{
			Diagnostics.Add(ErrorCode.ERR_FixedLocalInLambda, new SourceLocation(node.Syntax), localSymbol);
		}
		return null;
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		GetOrCreateSlot(node.LocalSymbol);
		HashSet<Symbol> hashSet = initiallyAssignedVariables;
		if (hashSet != null && hashSet.Contains(node.LocalSymbol))
		{
			Assign(node, node.InitializerOpt);
		}
		if (node.InitializerOpt != null)
		{
			base.VisitLocalDeclaration(node);
			Assign(node, node.InitializerOpt);
		}
		return null;
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundNode result = base.VisitCall(node);
		if (node.Method.MethodKind == MethodKind.LocalFunction)
		{
			LocalFunctionSymbol localFunc = (LocalFunctionSymbol)node.Method.OriginalDefinition;
			ReplayReadsAndWrites(localFunc, node.Syntax, writes: true);
		}
		return result;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		if (node.ConversionKind == ConversionKind.MethodGroup)
		{
			MethodSymbol symbolOpt = node.SymbolOpt;
			if ((object)symbolOpt != null && symbolOpt.MethodKind == MethodKind.LocalFunction)
			{
				LocalFunctionSymbol localFunc = (LocalFunctionSymbol)node.SymbolOpt.OriginalDefinition;
				SyntaxNode syntax = node.Syntax;
				ReplayReadsAndWrites(localFunc, syntax, writes: false);
			}
		}
		return base.VisitConversion(node);
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		MethodSymbol methodOpt = node.MethodOpt;
		if ((object)methodOpt != null && methodOpt.MethodKind == MethodKind.LocalFunction)
		{
			SyntaxNode syntax = node.Syntax;
			LocalFunctionSymbol localFunc = (LocalFunctionSymbol)node.MethodOpt.OriginalDefinition;
			ReplayReadsAndWrites(localFunc, syntax, writes: false);
		}
		return base.VisitDelegateCreationExpression(node);
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		foreach (MethodSymbol method in node.Methods)
		{
			if (method.MethodKind == MethodKind.LocalFunction)
			{
				_usedLocalFunctions.Add((LocalFunctionSymbol)method);
			}
		}
		return base.VisitMethodGroup(node);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		Symbol symbol = currentSymbol;
		currentSymbol = node.Symbol;
		SavedPending oldPending = SavePending();
		LocalState self = State;
		State = (State.Reachable ? State.Clone() : AllBitsSet());
		if (!node.WasCompilerGenerated)
		{
			EnterParameters(node.Symbol.Parameters);
		}
		SavedPending oldPending2 = SavePending();
		VisitAlways(node.Body);
		RestorePending(oldPending2);
		ImmutableArray<PendingBranch> immutableArray = RemoveReturns();
		RestorePending(oldPending);
		LeaveParameters(node.Symbol.Parameters, node.Syntax, null);
		IntersectWith(ref self, ref State);
		foreach (PendingBranch item in immutableArray)
		{
			State = item.State;
			if (item.Branch.Kind == BoundKind.ReturnStatement)
			{
				LeaveParameters(node.Symbol.Parameters, item.Branch.Syntax, null);
				IntersectWith(ref self, ref State);
			}
		}
		State = self;
		currentSymbol = symbol;
		return null;
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		CheckAssigned(MethodThisParameter, node.Syntax);
		return null;
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		if (!node.WasCompilerGenerated)
		{
			CheckAssigned(node.ParameterSymbol, node.Syntax);
		}
		return null;
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		base.VisitAssignmentOperator(node);
		Assign(node.Left, node.Right, node.IsRef);
		return null;
	}

	public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		base.VisitDeconstructionAssignmentOperator(node);
		Assign(node.Left, node.Right);
		return null;
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		base.VisitIncrementOperator(node);
		Assign(node.Operand, node.Operand);
		return null;
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		base.VisitCompoundAssignmentOperator(node);
		Assign(node.Left, node.Right);
		return null;
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		BoundExpression boundExpression = node.Expression;
		if (boundExpression.Kind == BoundKind.AddressOfOperator)
		{
			boundExpression = ((BoundAddressOfOperator)boundExpression).Operand;
		}
		VisitAddressOfOperand(boundExpression, shouldReadOperand: false);
		return null;
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		BoundExpression operand = node.Operand;
		bool shouldReadOperand = false;
		Symbol symbol = UseNonFieldSymbolUnsafely(operand);
		if ((object)symbol != null)
		{
			HashSet<PrefixUnaryExpressionSyntax> unassignedVariableAddressOfSyntaxes = _unassignedVariableAddressOfSyntaxes;
			if (unassignedVariableAddressOfSyntaxes != null && !unassignedVariableAddressOfSyntaxes.Contains(node.Syntax as PrefixUnaryExpressionSyntax))
			{
				shouldReadOperand = true;
			}
			if (!_unsafeAddressTakenVariables.ContainsKey(symbol))
			{
				_unsafeAddressTakenVariables.Add(symbol, node.Syntax.Location);
			}
		}
		VisitAddressOfOperand(node.Operand, shouldReadOperand);
		return null;
	}

	protected override void WriteArgument(BoundExpression arg, RefKind refKind, MethodSymbol method)
	{
		if (refKind == RefKind.Ref)
		{
			CheckAssigned(arg, arg.Syntax);
		}
		Assign(arg, null);
		if (refKind != RefKind.None && ((object)method == null || method.IsExtern))
		{
			MarkFieldsUsed(arg.Type);
		}
	}

	protected void CheckAssigned(BoundExpression expr, SyntaxNode node)
	{
		if (!State.Reachable)
		{
			return;
		}
		MakeSlot(expr);
		switch (expr.Kind)
		{
		case BoundKind.Local:
			CheckAssigned(((BoundLocal)expr).LocalSymbol, node);
			break;
		case BoundKind.Parameter:
			CheckAssigned(((BoundParameter)expr).ParameterSymbol, node);
			break;
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (!fieldSymbol.IsFixed && PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundFieldAccess.ReceiverOpt, fieldSymbol))
			{
				CheckAssigned(expr, fieldSymbol, node);
			}
			break;
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			FieldSymbol associatedField = boundEventAccess.EventSymbol.AssociatedField;
			if ((object)associatedField != null && PreciseAbstractFlowPass<LocalState>.MayRequireTracking(boundEventAccess.ReceiverOpt, associatedField))
			{
				CheckAssigned(boundEventAccess, associatedField, node);
			}
			break;
		}
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
			CheckAssigned(MethodThisParameter, node);
			break;
		}
	}

	private void MarkFieldsUsed(TypeSymbol type)
	{
		switch (type.TypeKind)
		{
		case TypeKind.Array:
			MarkFieldsUsed(((ArrayTypeSymbol)type).ElementType);
			break;
		case TypeKind.Class:
		case TypeKind.Struct:
		{
			if (!type.IsFromCompilation(compilation))
			{
				break;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			if (!(type.ContainingAssembly is SourceAssemblySymbol sourceAssemblySymbol) || !sourceAssemblySymbol.TypesReferencedInExternalMethods.Add(type))
			{
				break;
			}
			foreach (Symbol item in namedTypeSymbol.GetMembersUnordered())
			{
				if (item.Kind == SymbolKind.Field)
				{
					FieldSymbol fieldSymbol = (FieldSymbol)item;
					sourceAssemblySymbol.NoteFieldAccess(fieldSymbol, read: true, write: true);
					MarkFieldsUsed(fieldSymbol.Type);
				}
			}
			break;
		}
		}
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		CheckAssigned(MethodThisParameter, node.Syntax);
		return null;
	}

	protected override void VisitTryBlock(BoundStatement tryBlock, BoundTryStatement node, ref LocalState tryState)
	{
		if (trackUnassignments)
		{
			LocalState? tryState2 = _tryState;
			_tryState = AllBitsSet();
			base.VisitTryBlock(tryBlock, node, ref tryState);
			LocalState other = _tryState.Value;
			IntersectWith(ref tryState, ref other);
			if (tryState2.HasValue)
			{
				LocalState self = tryState2.Value;
				IntersectWith(ref self, ref other);
				tryState2 = self;
			}
			_tryState = tryState2;
		}
		else
		{
			base.VisitTryBlock(tryBlock, node, ref tryState);
		}
	}

	protected override void VisitCatchBlock(BoundCatchBlock catchBlock, ref LocalState finallyState)
	{
		if (trackUnassignments)
		{
			LocalState? tryState = _tryState;
			_tryState = AllBitsSet();
			VisitCatchBlockInternal(catchBlock, ref finallyState);
			LocalState other = _tryState.Value;
			IntersectWith(ref finallyState, ref other);
			if (tryState.HasValue)
			{
				LocalState self = tryState.Value;
				IntersectWith(ref self, ref other);
				tryState = self;
			}
			_tryState = tryState;
		}
		else
		{
			VisitCatchBlockInternal(catchBlock, ref finallyState);
		}
	}

	private void VisitCatchBlockInternal(BoundCatchBlock catchBlock, ref LocalState finallyState)
	{
		DeclareVariables(catchBlock.Locals);
		BoundExpression exceptionSourceOpt = catchBlock.ExceptionSourceOpt;
		if (exceptionSourceOpt != null)
		{
			Assign(exceptionSourceOpt, null, isRef: false, read: false);
		}
		base.VisitCatchBlock(catchBlock, ref finallyState);
		foreach (LocalSymbol local in catchBlock.Locals)
		{
			ReportIfUnused(local, local.DeclarationKind != LocalDeclarationKind.CatchVariable);
		}
	}

	protected override void VisitFinallyBlock(BoundStatement finallyBlock, ref LocalState unsetInFinally)
	{
		if (trackUnassignments)
		{
			LocalState? tryState = _tryState;
			_tryState = AllBitsSet();
			base.VisitFinallyBlock(finallyBlock, ref unsetInFinally);
			LocalState other = _tryState.Value;
			IntersectWith(ref unsetInFinally, ref other);
			if (tryState.HasValue)
			{
				LocalState self = tryState.Value;
				IntersectWith(ref self, ref other);
				tryState = self;
			}
			_tryState = tryState;
		}
		else
		{
			base.VisitFinallyBlock(finallyBlock, ref unsetInFinally);
		}
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundNode result = base.VisitFieldAccess(node);
		NoteRead(node.FieldSymbol);
		if (node.FieldSymbol.IsFixed && node.Syntax != null && !SyntaxFacts.IsFixedStatementExpression(node.Syntax))
		{
			Symbol symbol = UseNonFieldSymbolUnsafely(node.ReceiverOpt);
			if ((object)symbol != null)
			{
				CheckCaptured(symbol);
				if (!_unsafeAddressTakenVariables.ContainsKey(symbol))
				{
					_unsafeAddressTakenVariables.Add(symbol, node.Syntax.Location);
					return result;
				}
			}
		}
		else if (PreciseAbstractFlowPass<LocalState>.MayRequireTracking(node.ReceiverOpt, node.FieldSymbol))
		{
			CheckAssigned(node, node.FieldSymbol, node.Syntax);
		}
		return result;
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		BoundNode result = base.VisitPropertyAccess(node);
		if (Binder.AccessingAutoPropertyFromConstructor(node, currentSymbol))
		{
			SynthesizedBackingFieldSymbol synthesizedBackingFieldSymbol = (node.PropertySymbol as SourcePropertySymbol)?.BackingField;
			if (synthesizedBackingFieldSymbol != null && PreciseAbstractFlowPass<LocalState>.MayRequireTracking(node.ReceiverOpt, synthesizedBackingFieldSymbol) && State.Reachable && !IsAssigned(node, out var unassignedSlot))
			{
				ReportUnassignedIfNotCapturedInLocalFunction(synthesizedBackingFieldSymbol, node.Syntax, unassignedSlot);
			}
		}
		return result;
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		BoundNode result = base.VisitEventAccess(node);
		FieldSymbol associatedField = node.EventSymbol.AssociatedField;
		if ((object)associatedField != null)
		{
			NoteRead(associatedField);
			if (PreciseAbstractFlowPass<LocalState>.MayRequireTracking(node.ReceiverOpt, associatedField))
			{
				CheckAssigned(node, associatedField, node.Syntax);
			}
		}
		return result;
	}

	public override void VisitForEachIterationVariables(BoundForEachStatement node)
	{
		foreach (LocalSymbol iterationVariable in node.IterationVariables)
		{
			int orCreateSlot = GetOrCreateSlot(iterationVariable);
			if (orCreateSlot > 0)
			{
				SetSlotAssigned(orCreateSlot);
			}
			NoteWrite(iterationVariable, null, read: true);
		}
	}

	public override BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		BoundNode result = base.VisitObjectInitializerMember(node);
		if ((object)_sourceAssembly != null && node.MemberSymbol != null && node.MemberSymbol.Kind == SymbolKind.Field)
		{
			_sourceAssembly.NoteFieldAccess((FieldSymbol)node.MemberSymbol.OriginalDefinition, read: false, write: true);
		}
		return result;
	}

	public override BoundNode VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		return null;
	}

	protected override string Dump(LocalState state)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[assigned ");
		AppendBitNames(state.Assigned, stringBuilder);
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	protected void AppendBitNames(BitVector a, StringBuilder builder)
	{
		bool flag = false;
		foreach (int item in a.TrueBits())
		{
			if (flag)
			{
				builder.Append(", ");
			}
			flag = true;
			AppendBitName(item, builder);
		}
	}

	protected void AppendBitName(int bit, StringBuilder builder)
	{
		VariableIdentifier variableIdentifier = variableBySlot[bit];
		if (variableIdentifier.ContainingSlot > 0)
		{
			AppendBitName(variableIdentifier.ContainingSlot, builder);
			builder.Append(".");
		}
		StringBuilder stringBuilder = builder;
		string? value;
		if (bit != 0)
		{
			value = (string.IsNullOrEmpty(variableIdentifier.Symbol.Name) ? ("<anon>" + variableIdentifier.Symbol.GetHashCode()) : variableIdentifier.Symbol.Name);
		}
		else
		{
			value = "<unreachable>";
		}
		stringBuilder.Append(value);
	}

	protected override void UnionWith(ref LocalState self, ref LocalState other)
	{
		if (self.Assigned.Capacity != other.Assigned.Capacity)
		{
			Normalize(ref self);
			Normalize(ref other);
		}
		if (!other.Reachable)
		{
			self.Assigned[0] = true;
		}
		for (int i = 1; i < self.Assigned.Capacity; i++)
		{
			if (other.Assigned[i] && !self.Assigned[i])
			{
				SetSlotAssigned(i, ref self);
			}
		}
	}

	protected override bool IntersectWith(ref LocalState self, ref LocalState other)
	{
		if (self.Reachable == other.Reachable)
		{
			if (self.Assigned.Capacity != other.Assigned.Capacity)
			{
				Normalize(ref self);
				Normalize(ref other);
			}
			return self.Assigned.IntersectWith(in other.Assigned);
		}
		if (!self.Reachable)
		{
			self.Assigned = other.Assigned.Clone();
			return true;
		}
		return false;
	}

	private void ReplayReadsAndWrites(LocalFunctionSymbol localFunc, SyntaxNode syntax, bool writes)
	{
		_usedLocalFunctions.Add(localFunc);
		LocalFuncUsages orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages(localFunc);
		ReplayReads(ref orCreateLocalFuncUsages.ReadVars, syntax);
		if (writes)
		{
			UnionWith(ref State, ref orCreateLocalFuncUsages.WrittenVars);
		}
		orCreateLocalFuncUsages.LocalFuncVisited = true;
	}

	private void ReplayReads(ref BitVector reads, SyntaxNode syntax)
	{
		for (int i = 1; i < reads.Capacity; i++)
		{
			if (reads[i])
			{
				Symbol symbol = variableBySlot[i].Symbol;
				CheckIfAssignedDuringLocalFunctionReplay(symbol, syntax, i);
			}
		}
	}

	private void CheckIfAssignedDuringLocalFunctionReplay(Symbol symbol, SyntaxNode node, int slot)
	{
		if ((object)symbol == null)
		{
			return;
		}
		NoteRead(symbol);
		if (State.Reachable)
		{
			if (slot >= State.Assigned.Capacity)
			{
				Normalize(ref State);
			}
			if (slot > 0 && !State.IsAssigned(slot))
			{
				ReportUnassignedIfNotCapturedInLocalFunction(symbol, node, slot, skipIfUseBeforeDeclaration: false);
			}
		}
	}

	private int RootSlot(int slot)
	{
		while (true)
		{
			VariableIdentifier variableIdentifier = variableBySlot[slot];
			if (variableIdentifier.ContainingSlot == 0)
			{
				break;
			}
			slot = variableIdentifier.ContainingSlot;
		}
		return slot;
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement localFunc)
	{
		Symbol symbol = currentSymbol;
		LocalFunctionSymbol localFunctionSymbol = (LocalFunctionSymbol)(currentSymbol = localFunc.Symbol);
		SavedPending oldPending = SavePending();
		LocalState state = State;
		State = ReachableState();
		if (!localFunc.WasCompilerGenerated)
		{
			EnterParameters(localFunctionSymbol.Parameters);
		}
		LocalFuncUsages orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages(localFunctionSymbol);
		BitVector oldReads = orCreateLocalFuncUsages.ReadVars.Clone();
		orCreateLocalFuncUsages.ReadVars.Clear();
		SavedPending oldPending2 = SavePending();
		if (localFunctionSymbol.IsIterator)
		{
			PendingBranches.Add(new PendingBranch(null, State));
		}
		VisitAlways(localFunc.Body);
		RestorePending(oldPending2);
		ImmutableArray<PendingBranch> immutableArray = RemoveReturns();
		RestorePending(oldPending);
		Location location = null;
		if (!localFunctionSymbol.Locations.IsDefaultOrEmpty)
		{
			location = localFunctionSymbol.Locations[0];
		}
		LeaveParameters(localFunctionSymbol.Parameters, localFunc.Syntax, location);
		LocalState self = State;
		foreach (PendingBranch item in immutableArray)
		{
			State = item.State;
			BoundNode branch = item.Branch;
			LeaveParameters(localFunctionSymbol.Parameters, branch?.Syntax, (branch != null && !branch.WasCompilerGenerated) ? null : location);
			IntersectWith(ref self, ref State);
		}
		if (RecordChangedVars(ref orCreateLocalFuncUsages.WrittenVars, ref self, ref oldReads, ref orCreateLocalFuncUsages.ReadVars) && orCreateLocalFuncUsages.LocalFuncVisited)
		{
			stateChangedAfterUse = true;
			orCreateLocalFuncUsages.LocalFuncVisited = false;
		}
		State = state;
		currentSymbol = symbol;
		return null;
	}

	private void RecordReadInLocalFunction(int slot)
	{
		LocalFunctionSymbol nearestLocalFunctionOpt = GetNearestLocalFunctionOpt(currentSymbol);
		LocalFuncUsages orCreateLocalFuncUsages = GetOrCreateLocalFuncUsages(nearestLocalFunctionOpt);
		TypeSymbol type = VariableType(variableBySlot[slot].Symbol);
		if (EmptyStructTypeCache.IsTrackableStructType(type))
		{
			foreach (FieldSymbol structInstanceField in _emptyStructTypeCache.GetStructInstanceFields(type))
			{
				int orCreateSlot = GetOrCreateSlot(structInstanceField, slot);
				if (orCreateSlot > 0 && !State.IsAssigned(orCreateSlot))
				{
					RecordReadInLocalFunction(orCreateSlot);
				}
			}
			return;
		}
		orCreateLocalFuncUsages.ReadVars[slot] = true;
	}

	private bool RecordChangedVars(ref LocalState oldWrites, ref LocalState newWrites, ref BitVector oldReads, ref BitVector newReads)
	{
		return IntersectWith(ref oldWrites, ref newWrites) | RecordCapturedChanges(ref oldReads, ref newReads);
	}

	private bool RecordCapturedChanges(ref BitVector oldState, ref BitVector newState)
	{
		BitVector other = GetCapturedBitmask(ref newState);
		BitVector other2 = newState;
		other2.IntersectWith(in other);
		return oldState.UnionWith(in other2);
	}

	private BitVector GetCapturedBitmask(ref BitVector state)
	{
		BitVector empty = BitVector.Empty;
		for (int i = 1; i < state.Capacity; i++)
		{
			if (IsCapturedInLocalFunction(i))
			{
				empty[i] = true;
			}
		}
		return empty;
	}

	private bool IsCapturedInLocalFunction(int slot)
	{
		if (slot <= 0)
		{
			return false;
		}
		Symbol symbol = variableBySlot[RootSlot(slot)].Symbol;
		LocalFunctionSymbol nearestLocalFunctionOpt = GetNearestLocalFunctionOpt(currentSymbol);
		if ((object)nearestLocalFunctionOpt != null)
		{
			return IsCaptured(symbol, nearestLocalFunctionOpt);
		}
		return false;
	}

	private LocalFuncUsages GetOrCreateLocalFuncUsages(LocalFunctionSymbol localFunc)
	{
		if (!_localFuncVarUsages.TryGetValue(localFunc, out var value))
		{
			return _localFuncVarUsages[localFunc] = new LocalFuncUsages(UnreachableState());
		}
		return value;
	}

	private static LocalFunctionSymbol GetNearestLocalFunctionOpt(Symbol symbol)
	{
		while (symbol != null)
		{
			if (symbol.Kind == SymbolKind.Method && ((MethodSymbol)symbol).MethodKind == MethodKind.LocalFunction)
			{
				return (LocalFunctionSymbol)symbol;
			}
			symbol = symbol.ContainingSymbol;
		}
		return null;
	}
}
