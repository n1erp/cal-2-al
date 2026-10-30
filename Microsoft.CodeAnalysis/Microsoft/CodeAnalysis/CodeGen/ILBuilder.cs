using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class ILBuilder
{
	internal enum BlockType
	{
		Normal,
		Try,
		Catch,
		Filter,
		Finally,
		Fault,
		Switch
	}

	internal enum Reachability : byte
	{
		NotReachable,
		Reachable,
		BlockedByFinally
	}

	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal class BasicBlock
	{
		private class PooledBasicBlock : BasicBlock
		{
			internal override void Free()
			{
				base.Free();
				_branchLabel = null;
				BranchCode = ILOpCode.Nop;
				_revBranchCode = 0;
				NextBlock = null;
				builder = null;
				Reachability = Reachability.NotReachable;
				Start = 0;
				Pool.Free(this);
			}
		}

		public static readonly ObjectPool<BasicBlock> Pool = CreatePool(32);

		internal ILBuilder builder;

		private PooledBlobBuilder _lazyRegularInstructions;

		public BasicBlock NextBlock;

		private object _branchLabel;

		public int Start;

		private byte _revBranchCode;

		private ILOpCode _branchCode;

		internal Reachability Reachability;

		public PooledBlobBuilder Writer
		{
			get
			{
				if (_lazyRegularInstructions == null)
				{
					_lazyRegularInstructions = PooledBlobBuilder.GetInstance();
				}
				return _lazyRegularInstructions;
			}
		}

		public int FirstILMarker { get; private set; }

		public int LastILMarker { get; private set; }

		public virtual ExceptionHandlerScope EnclosingHandler => null;

		public object BranchLabel => _branchLabel;

		public ILOpCode BranchCode
		{
			get
			{
				return _branchCode;
			}
			set
			{
				_branchCode = value;
			}
		}

		public ILOpCode RevBranchCode
		{
			get
			{
				return (ILOpCode)_revBranchCode;
			}
			set
			{
				_revBranchCode = (byte)value;
			}
		}

		public BasicBlock BranchBlock
		{
			get
			{
				BasicBlock result = null;
				if (BranchLabel != null)
				{
					result = builder._labelInfos[BranchLabel].bb;
				}
				return result;
			}
		}

		private bool IsBranchToLabel
		{
			get
			{
				if (BranchLabel != null)
				{
					return BranchCode != ILOpCode.Nop;
				}
				return false;
			}
		}

		public virtual BlockType Type => BlockType.Normal;

		public BlobBuilder RegularInstructions => _lazyRegularInstructions;

		public bool HasNoRegularInstructions => _lazyRegularInstructions == null;

		public int RegularInstructionsLength => _lazyRegularInstructions?.Count ?? 0;

		private BasicBlock NextNontrivial
		{
			get
			{
				BasicBlock nextBlock = NextBlock;
				while (nextBlock != null && nextBlock.BranchCode == ILOpCode.Nop && nextBlock.HasNoRegularInstructions)
				{
					nextBlock = nextBlock.NextBlock;
				}
				return nextBlock;
			}
		}

		public virtual int TotalSize
		{
			get
			{
				int num;
				switch (BranchCode)
				{
				case ILOpCode.Nop:
					num = 0;
					break;
				case ILOpCode.Ret:
				case ILOpCode.Throw:
				case ILOpCode.Endfinally:
					num = 1;
					break;
				case ILOpCode.Endfilter:
				case ILOpCode.Rethrow:
					num = 2;
					break;
				default:
					num = 1 + BranchCode.GetBranchOperandSize();
					break;
				}
				return RegularInstructionsLength + num;
			}
		}

		private static ObjectPool<BasicBlock> CreatePool(int size)
		{
			return new ObjectPool<BasicBlock>(() => new PooledBasicBlock(), size);
		}

		protected BasicBlock()
		{
		}

		internal BasicBlock(ILBuilder builder)
		{
			Initialize(builder);
		}

		internal void Initialize(ILBuilder builder)
		{
			this.builder = builder;
			FirstILMarker = -1;
			LastILMarker = -1;
		}

		public void AddILMarker(int marker)
		{
			if (FirstILMarker < 0)
			{
				FirstILMarker = marker;
			}
			LastILMarker = marker;
		}

		public void RemoveTailILMarker(int marker)
		{
			if (FirstILMarker == LastILMarker)
			{
				FirstILMarker = -1;
				LastILMarker = -1;
			}
			else
			{
				LastILMarker--;
			}
		}

		internal virtual void Free()
		{
			if (_lazyRegularInstructions != null)
			{
				_lazyRegularInstructions.Free();
				_lazyRegularInstructions = null;
			}
		}

		public void SetBranchCode(ILOpCode newBranchCode)
		{
			BranchCode = newBranchCode;
		}

		public void SetBranch(object newLabel, ILOpCode branchCode, ILOpCode revBranchCode)
		{
			SetBranch(newLabel, branchCode);
			RevBranchCode = revBranchCode;
		}

		public void SetBranch(object newLabel, ILOpCode branchCode)
		{
			BranchCode = branchCode;
			if (_branchLabel == newLabel)
			{
				return;
			}
			_branchLabel = newLabel;
			if (BranchCode.IsConditionalBranch())
			{
				LabelInfo labelInfo = builder._labelInfos[newLabel];
				if (!labelInfo.targetOfConditionalBranches)
				{
					builder._labelInfos[newLabel] = labelInfo.SetTargetOfConditionalBranches();
				}
			}
		}

		internal void AdjustForDelta(int delta)
		{
			if (delta != 0)
			{
				Start += delta;
			}
		}

		internal void RewriteBranchesAcrossExceptionHandlers()
		{
			_ = EnclosingHandler;
			BasicBlock branchBlock = BranchBlock;
			if (branchBlock != null && branchBlock.EnclosingHandler != EnclosingHandler)
			{
				SetBranchCode(BranchCode.GetLeaveOpcode());
			}
		}

		internal void ShortenBranches(ref int delta)
		{
			if (!IsBranchToLabel)
			{
				return;
			}
			ILOpCode branchCode = BranchCode;
			if (branchCode.GetBranchOperandSize() != 1)
			{
				int start = BranchBlock.Start;
				int num = ((start <= Start) ? (start - (Start + TotalSize + -3)) : (start - NextBlock.Start));
				if ((sbyte)num == num)
				{
					SetBranchCode(branchCode.GetShortBranch());
					delta += -3;
				}
			}
		}

		internal bool OptimizeBranches(ref int delta)
		{
			if (IsBranchToLabel)
			{
				BasicBlock nextNontrivial = NextNontrivial;
				if (nextNontrivial != null)
				{
					if (TryOptimizeSameAsNext(nextNontrivial, ref delta))
					{
						return true;
					}
					if (TryOptimizeBranchToNextOrRet(nextNontrivial, ref delta))
					{
						return true;
					}
					if (TryOptimizeBranchOverUncondBranch(nextNontrivial, ref delta))
					{
						return true;
					}
					if (TryOptimizeBranchToEquivalent(nextNontrivial, ref delta))
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool TryOptimizeSameAsNext(BasicBlock next, ref int delta)
		{
			if (next.HasNoRegularInstructions && next.BranchCode == BranchCode && next.BranchBlock.Start == BranchBlock.Start && next.EnclosingHandler == EnclosingHandler)
			{
				int num = BranchCode.Size() + BranchCode.GetBranchOperandSize();
				delta -= num;
				SetBranch(null, ILOpCode.Nop);
				if (HasNoRegularInstructions)
				{
					SmallDictionary<object, LabelInfo> labelInfos = builder._labelInfos;
					foreach (object key in labelInfos.Keys)
					{
						LabelInfo labelInfo = labelInfos[key];
						if (labelInfo.bb == this)
						{
							labelInfos[key] = labelInfo.WithNewTarget(next);
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool TryOptimizeBranchOverUncondBranch(BasicBlock next, ref int delta)
		{
			if (next.HasNoRegularInstructions && next.NextBlock != null && next.NextBlock.Start == BranchBlock.Start && (next.BranchCode == ILOpCode.Br || next.BranchCode == ILOpCode.Br_s) && next.BranchBlock != next)
			{
				ILOpCode iLOpCode = GetReversedBranchOp();
				if (iLOpCode != ILOpCode.Nop)
				{
					BasicBlock nextBlock = NextBlock;
					BasicBlock branchBlock = BranchBlock;
					while (nextBlock != branchBlock)
					{
						nextBlock.Reachability = Reachability.NotReachable;
						nextBlock = nextBlock.NextBlock;
					}
					next.Reachability = Reachability.NotReachable;
					delta -= next.TotalSize;
					if (next.BranchCode == ILOpCode.Br_s)
					{
						iLOpCode = iLOpCode.GetShortBranch();
					}
					NextBlock = BranchBlock;
					ILOpCode branchCode = BranchCode;
					SetBranch(next.BranchLabel, iLOpCode, branchCode);
					return true;
				}
			}
			return false;
		}

		private bool TryOptimizeBranchToNextOrRet(BasicBlock next, ref int delta)
		{
			ILOpCode branchCode = BranchCode;
			if (branchCode == ILOpCode.Br || branchCode == ILOpCode.Br_s)
			{
				if (BranchBlock.Start - next.Start == 0)
				{
					SetBranch(null, ILOpCode.Nop);
					delta -= branchCode.Size() + branchCode.GetBranchOperandSize();
					return true;
				}
				if (BranchBlock.HasNoRegularInstructions && BranchBlock.BranchCode == ILOpCode.Ret)
				{
					SetBranch(null, ILOpCode.Ret);
					delta -= branchCode.Size() + branchCode.GetBranchOperandSize() - 1;
					return true;
				}
			}
			return false;
		}

		private bool TryOptimizeBranchToEquivalent(BasicBlock next, ref int delta)
		{
			ILOpCode branchCode = BranchCode;
			if (branchCode.IsConditionalBranch() && (BranchBlock.Start - next.Start == 0 || AreIdentical(BranchBlock, next)))
			{
				SetBranch(null, ILOpCode.Nop);
				Writer.WriteByte(38);
				delta -= branchCode.Size() + branchCode.GetBranchOperandSize() - 1;
				if (branchCode.IsRelationalBranch())
				{
					Writer.WriteByte(38);
					delta++;
				}
				return true;
			}
			return false;
		}

		private static bool AreIdentical(BasicBlock one, BasicBlock another)
		{
			if (one._branchCode == another._branchCode && !one._branchCode.CanFallThrough() && one._branchLabel == another._branchLabel)
			{
				BlobBuilder regularInstructions = one.RegularInstructions;
				BlobBuilder regularInstructions2 = another.RegularInstructions;
				if (regularInstructions != regularInstructions2)
				{
					return regularInstructions?.ContentEquals(regularInstructions2) ?? false;
				}
				return true;
			}
			return false;
		}

		private ILOpCode GetReversedBranchOp()
		{
			ILOpCode iLOpCode = RevBranchCode;
			if (iLOpCode != ILOpCode.Nop)
			{
				return iLOpCode;
			}
			switch (BranchCode)
			{
			case ILOpCode.Brfalse_s:
			case ILOpCode.Brfalse:
				iLOpCode = ILOpCode.Brtrue;
				break;
			case ILOpCode.Brtrue_s:
			case ILOpCode.Brtrue:
				iLOpCode = ILOpCode.Brfalse;
				break;
			case ILOpCode.Beq_s:
			case ILOpCode.Beq:
				iLOpCode = ILOpCode.Bne_un;
				break;
			case ILOpCode.Bne_un_s:
			case ILOpCode.Bne_un:
				iLOpCode = ILOpCode.Beq;
				break;
			}
			return iLOpCode;
		}

		private string GetDebuggerDisplay()
		{
			return "";
		}
	}

	internal class BasicBlockWithHandlerScope : BasicBlock
	{
		public readonly ExceptionHandlerScope enclosingHandler;

		public override ExceptionHandlerScope EnclosingHandler => enclosingHandler;

		public BasicBlockWithHandlerScope(ILBuilder builder, ExceptionHandlerScope enclosingHandler)
			: base(builder)
		{
			this.enclosingHandler = enclosingHandler;
		}
	}

	internal sealed class ExceptionHandlerLeaderBlock : BasicBlockWithHandlerScope
	{
		private readonly BlockType _type;

		public ExceptionHandlerLeaderBlock NextExceptionHandler;

		public override BlockType Type => _type;

		public ExceptionHandlerLeaderBlock(ILBuilder builder, ExceptionHandlerScope enclosingHandler, BlockType type)
			: base(builder, enclosingHandler)
		{
			_type = type;
		}

		public override string ToString()
		{
			return $"[{_type}] {base.ToString()}";
		}
	}

	internal sealed class SwitchBlock : BasicBlockWithHandlerScope
	{
		public object[] BranchLabels;

		public override BlockType Type => BlockType.Switch;

		public uint BranchesCount => (uint)BranchLabels.Length;

		public override int TotalSize
		{
			get
			{
				uint num = 5 + 4 * BranchesCount;
				return (int)(RegularInstructionsLength + num);
			}
		}

		public SwitchBlock(ILBuilder builder, ExceptionHandlerScope enclosingHandler)
			: base(builder, enclosingHandler)
		{
			SetBranchCode(ILOpCode.Switch);
		}

		public void GetBranchBlocks(ArrayBuilder<BasicBlock> branchBlocksBuilder)
		{
			object[] branchLabels = BranchLabels;
			foreach (object key in branchLabels)
			{
				branchBlocksBuilder.Add(builder._labelInfos[key].bb);
			}
		}
	}

	private struct EmitState
	{
		private int _maxStack;

		private int _curStack;

		private int _instructionsEmitted;

		internal int InstructionsEmitted => _instructionsEmitted;

		internal int MaxStack
		{
			get
			{
				return _maxStack;
			}
			private set
			{
				_maxStack = value;
			}
		}

		internal int CurStack
		{
			get
			{
				return _curStack;
			}
			private set
			{
				_curStack = value;
			}
		}

		internal void InstructionAdded()
		{
			_instructionsEmitted++;
		}

		internal void AdjustStack(int count)
		{
			CurStack += count;
			MaxStack = Math.Max(MaxStack, CurStack);
		}
	}

	private struct ILMarker
	{
		public int BlockOffset;

		public int AbsoluteOffset;
	}

	private struct LabelInfo
	{
		internal readonly int stack;

		internal readonly BasicBlock bb;

		internal readonly bool targetOfConditionalBranches;

		internal LabelInfo(int stack, bool targetOfConditionalBranches)
			: this(null, stack, targetOfConditionalBranches)
		{
		}

		internal LabelInfo(BasicBlock bb, int stack, bool targetOfConditionalBranches)
		{
			this.stack = stack;
			this.bb = bb;
			this.targetOfConditionalBranches = targetOfConditionalBranches;
		}

		internal LabelInfo WithNewTarget(BasicBlock bb)
		{
			return new LabelInfo(bb, stack, targetOfConditionalBranches);
		}

		internal LabelInfo SetTargetOfConditionalBranches()
		{
			return new LabelInfo(bb, stack, targetOfConditionalBranches: true);
		}
	}

	private sealed class LocalScopeManager
	{
		private readonly LocalScopeInfo _rootScope;

		private readonly Stack<ScopeInfo> _scopes;

		private ExceptionHandlerScope _enclosingExceptionHandler;

		private ScopeInfo CurrentScope => _scopes.Peek();

		internal ExceptionHandlerScope EnclosingExceptionHandler => _enclosingExceptionHandler;

		internal LocalScopeManager()
		{
			_rootScope = new LocalScopeInfo();
			_scopes = new Stack<ScopeInfo>(1);
			_scopes.Push(_rootScope);
		}

		internal ScopeInfo OpenScope(ScopeType scopeType, ITypeReference exceptionType)
		{
			ScopeInfo scopeInfo = CurrentScope.OpenScope(scopeType, exceptionType, _enclosingExceptionHandler);
			_scopes.Push(scopeInfo);
			if (scopeInfo.IsExceptionHandler)
			{
				_enclosingExceptionHandler = (ExceptionHandlerScope)scopeInfo;
			}
			return scopeInfo;
		}

		internal void FinishFilterCondition(ILBuilder builder)
		{
			CurrentScope.FinishFilterCondition(builder);
		}

		internal void ClosingScope(ILBuilder builder)
		{
			CurrentScope.ClosingScope(builder);
		}

		internal void CloseScope(ILBuilder builder)
		{
			ScopeInfo scopeInfo = _scopes.Pop();
			scopeInfo.CloseScope(builder);
			if (scopeInfo.IsExceptionHandler)
			{
				_enclosingExceptionHandler = GetEnclosingExceptionHandler();
			}
		}

		private ExceptionHandlerScope GetEnclosingExceptionHandler()
		{
			foreach (ScopeInfo scope in _scopes)
			{
				ScopeType type = scope.Type;
				if ((uint)(type - 2) <= 4u)
				{
					return (ExceptionHandlerScope)scope;
				}
			}
			return null;
		}

		internal BasicBlock CreateBlock(ILBuilder builder)
		{
			return ((LocalScopeInfo)CurrentScope).CreateBlock(builder);
		}

		internal SwitchBlock CreateSwitchBlock(ILBuilder builder)
		{
			return ((LocalScopeInfo)CurrentScope).CreateSwitchBlock(builder);
		}

		internal void AddLocal(LocalDefinition variable)
		{
			((LocalScopeInfo)CurrentScope).AddLocal(variable);
		}

		internal void AddLocalConstant(LocalConstantDefinition constant)
		{
			((LocalScopeInfo)CurrentScope).AddLocalConstant(constant);
		}

		internal ImmutableArray<Microsoft.Cci.LocalScope> GetAllScopesWithLocals()
		{
			ArrayBuilder<Microsoft.Cci.LocalScope> instance = ArrayBuilder<Microsoft.Cci.LocalScope>.GetInstance();
			ScopeBounds localScopes = _rootScope.GetLocalScopes(instance);
			int num = localScopes.End - localScopes.Begin;
			if (instance.Count > 0 && instance[instance.Count - 1].Length != num)
			{
				instance.Add(new Microsoft.Cci.LocalScope(0, num, ImmutableArray<ILocalDefinition>.Empty, ImmutableArray<ILocalDefinition>.Empty));
			}
			instance.Sort(ScopeComparer.Instance);
			return instance.ToImmutableAndFree();
		}

		internal ImmutableArray<ExceptionHandlerRegion> GetExceptionHandlerRegions()
		{
			ArrayBuilder<ExceptionHandlerRegion> instance = ArrayBuilder<ExceptionHandlerRegion>.GetInstance();
			_rootScope.GetExceptionHandlerRegions(instance);
			return instance.ToImmutableAndFree();
		}

		internal ImmutableArray<StateMachineHoistedLocalScope> GetHoistedLocalScopes()
		{
			ArrayBuilder<StateMachineHoistedLocalScope> instance = ArrayBuilder<StateMachineHoistedLocalScope>.GetInstance();
			_rootScope.GetHoistedLocalScopes(instance);
			return instance.ToImmutableAndFree();
		}

		internal void AddUserHoistedLocal(int slotIndex)
		{
			((LocalScopeInfo)CurrentScope).AddUserHoistedLocal(slotIndex);
		}

		internal void FreeBasicBlocks()
		{
			_rootScope.FreeBasicBlocks();
		}

		internal bool PossiblyDefinedOutsideOfTry(LocalDefinition local)
		{
			foreach (ScopeInfo scope in _scopes)
			{
				if (scope.ContainsLocal(local))
				{
					return false;
				}
				if (scope.Type == ScopeType.Try)
				{
					return true;
				}
			}
			return true;
		}
	}

	internal abstract class ScopeInfo
	{
		public abstract ScopeType Type { get; }

		public bool IsExceptionHandler
		{
			get
			{
				ScopeType type = Type;
				if ((uint)(type - 2) <= 4u)
				{
					return true;
				}
				return false;
			}
		}

		public virtual ScopeInfo OpenScope(ScopeType scopeType, ITypeReference exceptionType, ExceptionHandlerScope currentHandler)
		{
			if (scopeType == ScopeType.TryCatchFinally)
			{
				return new ExceptionHandlerContainerScope(currentHandler);
			}
			return new LocalScopeInfo();
		}

		public virtual void ClosingScope(ILBuilder builder)
		{
		}

		public virtual void CloseScope(ILBuilder builder)
		{
		}

		public virtual void FinishFilterCondition(ILBuilder builder)
		{
			throw ExceptionUtilities.Unreachable;
		}

		internal abstract void GetExceptionHandlerRegions(ArrayBuilder<ExceptionHandlerRegion> regions);

		internal abstract ScopeBounds GetLocalScopes(ArrayBuilder<Microsoft.Cci.LocalScope> result);

		protected static ScopeBounds GetLocalScopes<TScopeInfo>(ArrayBuilder<Microsoft.Cci.LocalScope> result, ImmutableArray<TScopeInfo>.Builder scopes) where TScopeInfo : ScopeInfo
		{
			int num = int.MaxValue;
			int num2 = 0;
			foreach (TScopeInfo scope in scopes)
			{
				ScopeBounds localScopes = scope.GetLocalScopes(result);
				num = Math.Min(num, localScopes.Begin);
				num2 = Math.Max(num2, localScopes.End);
			}
			return new ScopeBounds(num, num2);
		}

		internal abstract ScopeBounds GetHoistedLocalScopes(ArrayBuilder<StateMachineHoistedLocalScope> result);

		protected static ScopeBounds GetHoistedLocalScopes<TScopeInfo>(ArrayBuilder<StateMachineHoistedLocalScope> result, ImmutableArray<TScopeInfo>.Builder scopes) where TScopeInfo : ScopeInfo
		{
			int num = int.MaxValue;
			int num2 = 0;
			foreach (TScopeInfo scope in scopes)
			{
				ScopeBounds hoistedLocalScopes = scope.GetHoistedLocalScopes(result);
				num = Math.Min(num, hoistedLocalScopes.Begin);
				num2 = Math.Max(num2, hoistedLocalScopes.End);
			}
			return new ScopeBounds(num, num2);
		}

		public abstract void FreeBasicBlocks();

		internal virtual bool ContainsLocal(LocalDefinition local)
		{
			return false;
		}
	}

	internal class LocalScopeInfo : ScopeInfo
	{
		private ImmutableArray<LocalDefinition>.Builder _localVariables;

		private ImmutableArray<LocalConstantDefinition>.Builder _localConstants;

		private ImmutableArray<int>.Builder _stateMachineUserHoistedLocalSlotIndices;

		private ImmutableArray<ScopeInfo>.Builder _nestedScopes;

		protected ImmutableArray<BasicBlock>.Builder Blocks;

		public override ScopeType Type => ScopeType.Variable;

		public override ScopeInfo OpenScope(ScopeType scopeType, ITypeReference exceptionType, ExceptionHandlerScope currentExceptionHandler)
		{
			ScopeInfo scopeInfo = base.OpenScope(scopeType, exceptionType, currentExceptionHandler);
			if (_nestedScopes == null)
			{
				_nestedScopes = ImmutableArray.CreateBuilder<ScopeInfo>(1);
			}
			_nestedScopes.Add(scopeInfo);
			return scopeInfo;
		}

		internal void AddLocal(LocalDefinition variable)
		{
			if (_localVariables == null)
			{
				_localVariables = ImmutableArray.CreateBuilder<LocalDefinition>(1);
			}
			_localVariables.Add(variable);
		}

		internal void AddLocalConstant(LocalConstantDefinition constant)
		{
			if (_localConstants == null)
			{
				_localConstants = ImmutableArray.CreateBuilder<LocalConstantDefinition>(1);
			}
			_localConstants.Add(constant);
		}

		internal void AddUserHoistedLocal(int slotIndex)
		{
			if (_stateMachineUserHoistedLocalSlotIndices == null)
			{
				_stateMachineUserHoistedLocalSlotIndices = ImmutableArray.CreateBuilder<int>(1);
			}
			_stateMachineUserHoistedLocalSlotIndices.Add(slotIndex);
		}

		internal override bool ContainsLocal(LocalDefinition local)
		{
			return _localVariables?.Contains(local) ?? false;
		}

		public virtual BasicBlock CreateBlock(ILBuilder builder)
		{
			ExceptionHandlerScope enclosingExceptionHandler = builder.EnclosingExceptionHandler;
			BasicBlock basicBlock = ((enclosingExceptionHandler == null) ? AllocatePooledBlock(builder) : new BasicBlockWithHandlerScope(builder, enclosingExceptionHandler));
			AddBlock(basicBlock);
			return basicBlock;
		}

		private static BasicBlock AllocatePooledBlock(ILBuilder builder)
		{
			BasicBlock basicBlock = BasicBlock.Pool.Allocate();
			basicBlock.Initialize(builder);
			return basicBlock;
		}

		public SwitchBlock CreateSwitchBlock(ILBuilder builder)
		{
			SwitchBlock switchBlock = new SwitchBlock(builder, builder.EnclosingExceptionHandler);
			AddBlock(switchBlock);
			return switchBlock;
		}

		protected void AddBlock(BasicBlock block)
		{
			if (Blocks == null)
			{
				Blocks = ImmutableArray.CreateBuilder<BasicBlock>(4);
			}
			Blocks.Add(block);
		}

		internal override void GetExceptionHandlerRegions(ArrayBuilder<ExceptionHandlerRegion> regions)
		{
			if (_nestedScopes != null)
			{
				int i = 0;
				for (int count = _nestedScopes.Count; i < count; i++)
				{
					_nestedScopes[i].GetExceptionHandlerRegions(regions);
				}
			}
		}

		internal override ScopeBounds GetLocalScopes(ArrayBuilder<Microsoft.Cci.LocalScope> result)
		{
			int num = int.MaxValue;
			int num2 = 0;
			if (Blocks != null)
			{
				for (int i = 0; i < Blocks.Count; i++)
				{
					BasicBlock basicBlock = Blocks[i];
					if (basicBlock.Reachability != Reachability.NotReachable)
					{
						num = Math.Min(num, basicBlock.Start);
						num2 = Math.Max(num2, basicBlock.Start + basicBlock.TotalSize);
					}
				}
			}
			if (_nestedScopes != null)
			{
				ScopeBounds localScopes = ScopeInfo.GetLocalScopes(result, _nestedScopes);
				num = Math.Min(num, localScopes.Begin);
				num2 = Math.Max(num2, localScopes.End);
			}
			if ((_localVariables != null || _localConstants != null) && num2 > num)
			{
				Microsoft.Cci.LocalScope item = new Microsoft.Cci.LocalScope(num, num2, ((IEnumerable<ILocalDefinition>)_localConstants).AsImmutableOrEmpty(), ((IEnumerable<ILocalDefinition>)_localVariables).AsImmutableOrEmpty());
				result.Add(item);
			}
			return new ScopeBounds(num, num2);
		}

		internal override ScopeBounds GetHoistedLocalScopes(ArrayBuilder<StateMachineHoistedLocalScope> result)
		{
			int num = int.MaxValue;
			int num2 = 0;
			if (Blocks != null)
			{
				for (int i = 0; i < Blocks.Count; i++)
				{
					BasicBlock basicBlock = Blocks[i];
					if (basicBlock.Reachability != Reachability.NotReachable)
					{
						num = Math.Min(num, basicBlock.Start);
						num2 = Math.Max(num2, basicBlock.Start + basicBlock.TotalSize);
					}
				}
			}
			if (_nestedScopes != null)
			{
				ScopeBounds hoistedLocalScopes = ScopeInfo.GetHoistedLocalScopes(result, _nestedScopes);
				num = Math.Min(num, hoistedLocalScopes.Begin);
				num2 = Math.Max(num2, hoistedLocalScopes.End);
			}
			if (_stateMachineUserHoistedLocalSlotIndices != null && num2 > num)
			{
				StateMachineHoistedLocalScope value = new StateMachineHoistedLocalScope(num, num2);
				foreach (int stateMachineUserHoistedLocalSlotIndex in _stateMachineUserHoistedLocalSlotIndices)
				{
					while (result.Count <= stateMachineUserHoistedLocalSlotIndex)
					{
						result.Add(default);
					}
					result[stateMachineUserHoistedLocalSlotIndex] = value;
				}
			}
			return new ScopeBounds(num, num2);
		}

		public override void FreeBasicBlocks()
		{
			if (Blocks != null)
			{
				int i = 0;
				for (int count = Blocks.Count; i < count; i++)
				{
					Blocks[i].Free();
				}
			}
			if (_nestedScopes != null)
			{
				int j = 0;
				for (int count2 = _nestedScopes.Count; j < count2; j++)
				{
					_nestedScopes[j].FreeBasicBlocks();
				}
			}
		}
	}

	internal sealed class ExceptionHandlerScope : LocalScopeInfo
	{
		private readonly ExceptionHandlerContainerScope _containingScope;

		private readonly ScopeType _type;

		private readonly ITypeReference _exceptionType;

		private BasicBlock _lastFilterConditionBlock;

		private object _blockedByFinallyDestination;

		public ExceptionHandlerContainerScope ContainingExceptionScope => _containingScope;

		public override ScopeType Type => _type;

		public ITypeReference ExceptionType => _exceptionType;

		public object BlockedByFinallyDestination => _blockedByFinallyDestination;

		public int FilterHandlerStart => _lastFilterConditionBlock.Start + _lastFilterConditionBlock.TotalSize;

		public ExceptionHandlerLeaderBlock LeaderBlock => (ExceptionHandlerLeaderBlock)(Blocks?[0]);

		public ExceptionHandlerScope(ExceptionHandlerContainerScope containingScope, ScopeType type, ITypeReference exceptionType)
		{
			_containingScope = containingScope;
			_type = type;
			_exceptionType = exceptionType;
		}

		public void SetBlockedByFinallyDestination(object label)
		{
			_blockedByFinallyDestination = label;
		}

		public void UnblockFinally()
		{
			_blockedByFinallyDestination = null;
		}

		public override void FinishFilterCondition(ILBuilder builder)
		{
			_lastFilterConditionBlock = builder.FinishFilterCondition();
		}

		public override void ClosingScope(ILBuilder builder)
		{
			ScopeType type = _type;
			if ((uint)(type - 5) <= 1u)
			{
				builder.EmitEndFinally();
				return;
			}
			object endLabel = _containingScope.EndLabel;
			builder.EmitBranch(ILOpCode.Br, endLabel);
		}

		public override void CloseScope(ILBuilder builder)
		{
		}

		public override BasicBlock CreateBlock(ILBuilder builder)
		{
			BasicBlockWithHandlerScope basicBlockWithHandlerScope = ((Blocks == null) ? new ExceptionHandlerLeaderBlock(builder, this, GetLeaderBlockType()) : new BasicBlockWithHandlerScope(builder, this));
			AddBlock(basicBlockWithHandlerScope);
			return basicBlockWithHandlerScope;
		}

		private BlockType GetLeaderBlockType()
		{
			return _type switch
			{
				ScopeType.Try => BlockType.Try, 
				ScopeType.Catch => BlockType.Catch, 
				ScopeType.Filter => BlockType.Filter, 
				ScopeType.Finally => BlockType.Finally, 
				_ => BlockType.Fault, 
			};
		}

		public override void FreeBasicBlocks()
		{
			base.FreeBasicBlocks();
		}
	}

	internal sealed class ExceptionHandlerContainerScope : ScopeInfo
	{
		private readonly ImmutableArray<ExceptionHandlerScope>.Builder _handlers;

		private readonly object _endLabel;

		private readonly ExceptionHandlerScope _containingHandler;

		public ExceptionHandlerScope ContainingHandler => _containingHandler;

		public object EndLabel => _endLabel;

		public override ScopeType Type => ScopeType.TryCatchFinally;

		public ExceptionHandlerContainerScope(ExceptionHandlerScope containingHandler)
		{
			_handlers = ImmutableArray.CreateBuilder<ExceptionHandlerScope>(2);
			_containingHandler = containingHandler;
			_endLabel = new object();
		}

		public override ScopeInfo OpenScope(ScopeType scopeType, ITypeReference exceptionType, ExceptionHandlerScope currentExceptionHandler)
		{
			ExceptionHandlerScope exceptionHandlerScope = new ExceptionHandlerScope(this, scopeType, exceptionType);
			_handlers.Add(exceptionHandlerScope);
			return exceptionHandlerScope;
		}

		public override void CloseScope(ILBuilder builder)
		{
			ExceptionHandlerLeaderBlock exceptionHandlerLeaderBlock = _handlers[0].LeaderBlock;
			for (int i = 1; i < _handlers.Count; i++)
			{
				exceptionHandlerLeaderBlock = (exceptionHandlerLeaderBlock.NextExceptionHandler = _handlers[i].LeaderBlock);
			}
			builder.MarkLabel(_endLabel);
			builder.DefineHiddenSequencePoint();
			if (_handlers[1].Type == ScopeType.Finally)
			{
				builder.EmitBranch(ILOpCode.Nop, _endLabel);
				_handlers[1].SetBlockedByFinallyDestination(_endLabel);
			}
		}

		internal override void GetExceptionHandlerRegions(ArrayBuilder<ExceptionHandlerRegion> regions)
		{
			ExceptionHandlerScope exceptionHandlerScope = null;
			ScopeBounds scopeBounds = default;
			foreach (ExceptionHandlerScope handler in _handlers)
			{
				handler.GetExceptionHandlerRegions(regions);
				ScopeBounds bounds = GetBounds(handler);
				if (exceptionHandlerScope == null)
				{
					exceptionHandlerScope = handler;
					scopeBounds = bounds;
					if (exceptionHandlerScope.LeaderBlock.Reachability != Reachability.Reachable)
					{
						break;
					}
				}
				else
				{
					regions.Add(handler.Type switch
					{
						ScopeType.Finally => (ExceptionHandlerRegion)new ExceptionHandlerRegionFinally(scopeBounds.Begin, scopeBounds.End, bounds.Begin, bounds.End), 
						ScopeType.Fault => new ExceptionHandlerRegionFault(scopeBounds.Begin, scopeBounds.End, bounds.Begin, bounds.End), 
						ScopeType.Catch => new ExceptionHandlerRegionCatch(scopeBounds.Begin, scopeBounds.End, bounds.Begin, bounds.End, handler.ExceptionType), 
						ScopeType.Filter => new ExceptionHandlerRegionFilter(scopeBounds.Begin, scopeBounds.End, handler.FilterHandlerStart, bounds.End, bounds.Begin), 
						_ => throw ExceptionUtilities.UnexpectedValue(handler.Type), 
					});
				}
			}
		}

		internal override ScopeBounds GetLocalScopes(ArrayBuilder<Microsoft.Cci.LocalScope> scopesWithVariables)
		{
			return ScopeInfo.GetLocalScopes(scopesWithVariables, _handlers);
		}

		internal override ScopeBounds GetHoistedLocalScopes(ArrayBuilder<StateMachineHoistedLocalScope> result)
		{
			return ScopeInfo.GetHoistedLocalScopes(result, _handlers);
		}

		private static ScopeBounds GetBounds(ExceptionHandlerScope scope)
		{
			ArrayBuilder<Microsoft.Cci.LocalScope> instance = ArrayBuilder<Microsoft.Cci.LocalScope>.GetInstance();
			ScopeBounds localScopes = scope.GetLocalScopes(instance);
			instance.Free();
			return localScopes;
		}

		public override void FreeBasicBlocks()
		{
			foreach (ExceptionHandlerScope handler in _handlers)
			{
				handler.FreeBasicBlocks();
			}
		}

		internal bool FinallyOnly()
		{
			ExceptionHandlerContainerScope exceptionHandlerContainerScope = this;
			do
			{
				ImmutableArray<ExceptionHandlerScope>.Builder handlers = exceptionHandlerContainerScope._handlers;
				if (handlers.Count != 2 || handlers[1].Type != ScopeType.Finally)
				{
					return false;
				}
				exceptionHandlerContainerScope = exceptionHandlerContainerScope._containingHandler?.ContainingExceptionScope;
			}
			while (exceptionHandlerContainerScope != null);
			return true;
		}
	}

	internal struct ScopeBounds
	{
		internal readonly int Begin;

		internal readonly int End;

		internal ScopeBounds(int begin, int end)
		{
			Begin = begin;
			End = end;
		}
	}

	private sealed class ScopeComparer : IComparer<Microsoft.Cci.LocalScope>
	{
		public static readonly ScopeComparer Instance = new ScopeComparer();

		private ScopeComparer()
		{
		}

		public int Compare(Microsoft.Cci.LocalScope x, Microsoft.Cci.LocalScope y)
		{
			int startOffset = x.StartOffset;
			int num = startOffset.CompareTo(y.StartOffset);
			if (num != 0)
			{
				return num;
			}
			startOffset = y.EndOffset;
			return startOffset.CompareTo(x.EndOffset);
		}
	}

	private readonly OptimizationLevel _optimizations;

	internal readonly LocalSlotManager LocalSlotManager;

	private readonly LocalScopeManager _scopeManager;

	internal readonly ITokenDeferral module;

	internal readonly BasicBlock leaderBlock;

	private EmitState _emitState;

	private BasicBlock _lastCompleteBlock;

	private BasicBlock _currentBlock;

	private SyntaxTree _lastSeqPointTree;

	private readonly SmallDictionary<object, LabelInfo> _labelInfos;

	private int _instructionCountAtLastLabel = -1;

	internal ImmutableArray<byte> RealizedIL;

	internal ImmutableArray<ExceptionHandlerRegion> RealizedExceptionHandlers;

	internal SequencePointList RealizedSequencePoints;

	public ArrayBuilder<RawSequencePoint> SeqPointsOpt;

	private ArrayBuilder<ILMarker> _allocatedILMarkers;

	private bool _pendingBlockCreate;

	private int _initialHiddenSequencePointMarker = -1;

	private ExceptionHandlerScope EnclosingExceptionHandler => _scopeManager.EnclosingExceptionHandler;

	internal bool InExceptionHandler => EnclosingExceptionHandler != null;

	internal ushort MaxStack => (ushort)_emitState.MaxStack;

	internal int InstructionsEmitted => _emitState.InstructionsEmitted;

	internal bool HasDynamicLocal { get; private set; }

	internal bool IsStackEmpty => _emitState.CurStack == 0;

	internal ILBuilder(ITokenDeferral module, LocalSlotManager localSlotManager, OptimizationLevel optimizations)
	{
		this.module = module;
		LocalSlotManager = localSlotManager;
		_emitState = default;
		_scopeManager = new LocalScopeManager();
		leaderBlock = (_currentBlock = _scopeManager.CreateBlock(this));
		_labelInfos = new SmallDictionary<object, LabelInfo>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
		_optimizations = optimizations;
	}

	private BasicBlock GetCurrentBlock()
	{
		if (_currentBlock == null)
		{
			CreateBlock();
		}
		return _currentBlock;
	}

	private void CreateBlock()
	{
		BasicBlock block = _scopeManager.CreateBlock(this);
		UpdatesForCreatedBlock(block);
	}

	private SwitchBlock CreateSwitchBlock()
	{
		EndBlock();
		SwitchBlock switchBlock = _scopeManager.CreateSwitchBlock(this);
		UpdatesForCreatedBlock(switchBlock);
		return switchBlock;
	}

	private void UpdatesForCreatedBlock(BasicBlock block)
	{
		_currentBlock = block;
		_lastCompleteBlock.NextBlock = block;
		_pendingBlockCreate = false;
		ReconcileTrailingMarkers();
	}

	private void CreateBlockIfPending()
	{
		if (_pendingBlockCreate)
		{
			CreateBlock();
		}
	}

	private void EndBlock()
	{
		CreateBlockIfPending();
		if (_currentBlock != null)
		{
			_lastCompleteBlock = _currentBlock;
			_currentBlock = null;
		}
	}

	private void ReconcileTrailingMarkers()
	{
		if (_lastCompleteBlock == null || _lastCompleteBlock.BranchCode != ILOpCode.Nop || _lastCompleteBlock.LastILMarker < 0 || _allocatedILMarkers[_lastCompleteBlock.LastILMarker].BlockOffset != _lastCompleteBlock.RegularInstructionsLength)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		while (_lastCompleteBlock.LastILMarker >= 0 && _allocatedILMarkers[_lastCompleteBlock.LastILMarker].BlockOffset == _lastCompleteBlock.RegularInstructionsLength)
		{
			num = _lastCompleteBlock.LastILMarker;
			if (num2 < 0)
			{
				num2 = _lastCompleteBlock.LastILMarker;
			}
			_lastCompleteBlock.RemoveTailILMarker(_lastCompleteBlock.LastILMarker);
		}
		BasicBlock currentBlock = GetCurrentBlock();
		for (int i = num; i <= num2; i++)
		{
			currentBlock.AddILMarker(i);
			_allocatedILMarkers[i] = new ILMarker
			{
				BlockOffset = currentBlock.RegularInstructionsLength,
				AbsoluteOffset = -1
			};
		}
	}

	internal void Realize()
	{
		if (RealizedIL.IsDefault)
		{
			RealizeBlocks();
			_currentBlock = null;
			_lastCompleteBlock = null;
		}
	}

	internal ImmutableArray<Microsoft.Cci.LocalScope> GetAllScopes()
	{
		return _scopeManager.GetAllScopesWithLocals();
	}

	internal ImmutableArray<StateMachineHoistedLocalScope> GetHoistedLocalScopes()
	{
		return _scopeManager.GetHoistedLocalScopes();
	}

	internal void FreeBasicBlocks()
	{
		_scopeManager.FreeBasicBlocks();
		if (SeqPointsOpt != null)
		{
			SeqPointsOpt.Free();
			SeqPointsOpt = null;
		}
		if (_allocatedILMarkers != null)
		{
			_allocatedILMarkers.Free();
			_allocatedILMarkers = null;
		}
	}

	private void MarkReachableBlocks()
	{
		ArrayBuilder<BasicBlock> instance = ArrayBuilder<BasicBlock>.GetInstance();
		MarkReachableFrom(instance, leaderBlock);
		while (instance.Count != 0)
		{
			MarkReachableFrom(instance, instance.Pop());
		}
		instance.Free();
	}

	private static void PushReachableBlockToProcess(ArrayBuilder<BasicBlock> reachableBlocks, BasicBlock block)
	{
		if (block.Reachability == Reachability.NotReachable)
		{
			reachableBlocks.Push(block);
		}
	}

	private static void MarkReachableFrom(ArrayBuilder<BasicBlock> reachableBlocks, BasicBlock block)
	{
		while (block != null && block.Reachability == Reachability.NotReachable)
		{
			block.Reachability = Reachability.Reachable;
			ILOpCode branchCode = block.BranchCode;
			if (branchCode == ILOpCode.Nop && block.Type == BlockType.Normal)
			{
				block = block.NextBlock;
				continue;
			}
			if (branchCode.CanFallThrough())
			{
				PushReachableBlockToProcess(reachableBlocks, block.NextBlock);
			}
			else if (branchCode == ILOpCode.Endfinally)
			{
				block.EnclosingHandler?.UnblockFinally();
			}
			switch (block.Type)
			{
			case BlockType.Switch:
				MarkReachableFromSwitch(reachableBlocks, block);
				break;
			case BlockType.Try:
				MarkReachableFromTry(reachableBlocks, block);
				break;
			default:
				MarkReachableFromBranch(reachableBlocks, block);
				break;
			}
			break;
		}
	}

	private static void MarkReachableFromBranch(ArrayBuilder<BasicBlock> reachableBlocks, BasicBlock block)
	{
		BasicBlock branchBlock = block.BranchBlock;
		if (branchBlock != null)
		{
			object obj = BlockedBranchDestination(block, branchBlock);
			if (obj == null)
			{
				PushReachableBlockToProcess(reachableBlocks, branchBlock);
			}
			else
			{
				RedirectBranchToBlockedDestination(block, obj);
			}
		}
	}

	private static void RedirectBranchToBlockedDestination(BasicBlock block, object blockedDest)
	{
		block.SetBranch(blockedDest, block.BranchCode);
		if (block.BranchBlock.Reachability == Reachability.NotReachable)
		{
			block.BranchBlock.Reachability = Reachability.BlockedByFinally;
		}
	}

	private static object BlockedBranchDestination(BasicBlock src, BasicBlock dest)
	{
		ExceptionHandlerScope enclosingHandler = src.EnclosingHandler;
		if (enclosingHandler == null)
		{
			return null;
		}
		return BlockedBranchDestinationSlow(dest.EnclosingHandler, enclosingHandler);
	}

	private static object BlockedBranchDestinationSlow(ExceptionHandlerScope destHandler, ExceptionHandlerScope srcHandler)
	{
		ScopeInfo scopeInfo = null;
		if (destHandler != null)
		{
			scopeInfo = destHandler.ContainingExceptionScope;
		}
		while (srcHandler != destHandler && srcHandler.ContainingExceptionScope != scopeInfo)
		{
			if (srcHandler.Type == ScopeType.Try)
			{
				ExceptionHandlerLeaderBlock nextExceptionHandler = srcHandler.LeaderBlock.NextExceptionHandler;
				if (nextExceptionHandler.Type == BlockType.Finally)
				{
					object blockedByFinallyDestination = nextExceptionHandler.EnclosingHandler.BlockedByFinallyDestination;
					if (blockedByFinallyDestination != null)
					{
						return blockedByFinallyDestination;
					}
				}
			}
			srcHandler = srcHandler.ContainingExceptionScope.ContainingHandler;
		}
		return null;
	}

	private static void MarkReachableFromTry(ArrayBuilder<BasicBlock> reachableBlocks, BasicBlock block)
	{
		ExceptionHandlerLeaderBlock nextExceptionHandler = ((ExceptionHandlerLeaderBlock)block).NextExceptionHandler;
		if (nextExceptionHandler.Type == BlockType.Finally)
		{
			if (nextExceptionHandler.Reachability != Reachability.Reachable)
			{
				block.Reachability = Reachability.NotReachable;
				PushReachableBlockToProcess(reachableBlocks, block);
				PushReachableBlockToProcess(reachableBlocks, nextExceptionHandler);
				return;
			}
		}
		else
		{
			while (nextExceptionHandler != null)
			{
				PushReachableBlockToProcess(reachableBlocks, nextExceptionHandler);
				nextExceptionHandler = nextExceptionHandler.NextExceptionHandler;
			}
		}
		MarkReachableFromBranch(reachableBlocks, block);
	}

	private static void MarkReachableFromSwitch(ArrayBuilder<BasicBlock> reachableBlocks, BasicBlock block)
	{
		SwitchBlock switchBlock = (SwitchBlock)block;
		ArrayBuilder<BasicBlock> instance = ArrayBuilder<BasicBlock>.GetInstance();
		switchBlock.GetBranchBlocks(instance);
		foreach (BasicBlock item in instance)
		{
			PushReachableBlockToProcess(reachableBlocks, item);
		}
		instance.Free();
	}

	private bool OptimizeLabels()
	{
		return ForwardLabelsNoLeaving() | ForwardLabelsAllowLeaving();
	}

	private bool ForwardLabelsNoLeaving()
	{
		bool result = false;
		SmallDictionary<object, LabelInfo>.KeyCollection keys = _labelInfos.Keys;
		bool flag;
		do
		{
			flag = true;
			foreach (object item in keys)
			{
				LabelInfo labelInfo = _labelInfos[item];
				BasicBlock bb = labelInfo.bb;
				if (!bb.HasNoRegularInstructions)
				{
					continue;
				}
				BasicBlock basicBlock = null;
				switch (bb.BranchCode)
				{
				case ILOpCode.Br:
					basicBlock = bb.BranchBlock;
					break;
				case ILOpCode.Nop:
					basicBlock = bb.NextBlock;
					break;
				}
				if (basicBlock != null && basicBlock != bb)
				{
					ExceptionHandlerScope enclosingHandler = bb.EnclosingHandler;
					ExceptionHandlerScope enclosingHandler2 = basicBlock.EnclosingHandler;
					if (enclosingHandler == enclosingHandler2)
					{
						_labelInfos[item] = labelInfo.WithNewTarget(basicBlock);
						result = true;
						flag = false;
					}
				}
			}
		}
		while (!flag);
		return result;
	}

	private bool ForwardLabelsAllowLeaving()
	{
		bool result = false;
		SmallDictionary<object, LabelInfo>.KeyCollection keys = _labelInfos.Keys;
		bool flag;
		do
		{
			flag = true;
			foreach (object item in keys)
			{
				LabelInfo labelInfo = _labelInfos[item];
				if (labelInfo.targetOfConditionalBranches)
				{
					continue;
				}
				BasicBlock bb = labelInfo.bb;
				if (!bb.HasNoRegularInstructions)
				{
					continue;
				}
				BasicBlock basicBlock = null;
				switch (bb.BranchCode)
				{
				case ILOpCode.Br:
					basicBlock = bb.BranchBlock;
					break;
				case ILOpCode.Nop:
					basicBlock = bb.NextBlock;
					break;
				}
				if (basicBlock != null && basicBlock != bb)
				{
					ExceptionHandlerScope enclosingHandler = bb.EnclosingHandler;
					ExceptionHandlerScope enclosingHandler2 = basicBlock.EnclosingHandler;
					if (CanMoveLabelToAnotherHandler(enclosingHandler, enclosingHandler2))
					{
						_labelInfos[item] = labelInfo.WithNewTarget(basicBlock);
						result = true;
						flag = false;
					}
				}
			}
		}
		while (!flag);
		return result;
	}

	private static bool CanMoveLabelToAnotherHandler(ExceptionHandlerScope currentHandler, ExceptionHandlerScope newHandler)
	{
		if (newHandler == null && currentHandler.ContainingExceptionScope.FinallyOnly())
		{
			return true;
		}
		do
		{
			if (currentHandler == newHandler)
			{
				return true;
			}
			ExceptionHandlerContainerScope containingExceptionScope = currentHandler.ContainingExceptionScope;
			if (!containingExceptionScope.FinallyOnly())
			{
				return false;
			}
			currentHandler = containingExceptionScope.ContainingHandler;
		}
		while (currentHandler != null);
		return false;
	}

	private bool DropUnreachableBlocks()
	{
		bool result = false;
		BasicBlock nextBlock = leaderBlock;
		while (nextBlock.NextBlock != null)
		{
			if (nextBlock.NextBlock.Reachability == Reachability.NotReachable)
			{
				nextBlock.NextBlock = nextBlock.NextBlock.NextBlock;
				result = true;
			}
			else
			{
				nextBlock = nextBlock.NextBlock;
			}
		}
		return result;
	}

	private void MarkAllBlocksUnreachable()
	{
		for (BasicBlock nextBlock = leaderBlock; nextBlock != null; nextBlock = nextBlock.NextBlock)
		{
			nextBlock.Reachability = Reachability.NotReachable;
		}
	}

	private void ComputeOffsets()
	{
		BasicBlock nextBlock = leaderBlock;
		while (nextBlock.NextBlock != null)
		{
			nextBlock.NextBlock.Start = nextBlock.Start + nextBlock.TotalSize;
			nextBlock = nextBlock.NextBlock;
		}
	}

	private void RewriteSpecialBlocks()
	{
		for (BasicBlock nextBlock = leaderBlock; nextBlock != null; nextBlock = nextBlock.NextBlock)
		{
			if (IsSpecialEndHandlerBlock(nextBlock))
			{
				if (nextBlock.Reachability == Reachability.BlockedByFinally)
				{
					nextBlock.SetBranchCode(ILOpCode.Br_s);
				}
				else
				{
					nextBlock.SetBranch(null, ILOpCode.Nop);
				}
			}
		}
	}

	private static bool IsSpecialEndHandlerBlock(BasicBlock block)
	{
		if (block.BranchCode != ILOpCode.Nop || block.BranchLabel == null)
		{
			return false;
		}
		return true;
	}

	private void RewriteBranchesAcrossExceptionHandlers()
	{
		for (BasicBlock nextBlock = leaderBlock; nextBlock != null; nextBlock = nextBlock.NextBlock)
		{
			nextBlock.RewriteBranchesAcrossExceptionHandlers();
		}
	}

	private bool ComputeOffsetsAndAdjustBranches()
	{
		ComputeOffsets();
		bool flag = false;
		int delta;
		do
		{
			delta = 0;
			for (BasicBlock nextBlock = leaderBlock; nextBlock != null; nextBlock = nextBlock.NextBlock)
			{
				nextBlock.AdjustForDelta(delta);
				if (_optimizations == OptimizationLevel.Release)
				{
					flag |= nextBlock.OptimizeBranches(ref delta);
				}
				nextBlock.ShortenBranches(ref delta);
			}
		}
		while (delta < 0);
		return flag;
	}

	private void RealizeBlocks()
	{
		MarkReachableBlocks();
		RewriteSpecialBlocks();
		DropUnreachableBlocks();
		if (_optimizations == OptimizationLevel.Release && OptimizeLabels())
		{
			MarkAllBlocksUnreachable();
			MarkReachableBlocks();
			DropUnreachableBlocks();
		}
		RewriteBranchesAcrossExceptionHandlers();
		while (ComputeOffsetsAndAdjustBranches())
		{
			MarkAllBlocksUnreachable();
			MarkReachableBlocks();
			if (!DropUnreachableBlocks())
			{
				break;
			}
		}
		PooledBlobBuilder instance = PooledBlobBuilder.GetInstance();
		for (BasicBlock nextBlock = leaderBlock; nextBlock != null; nextBlock = nextBlock.NextBlock)
		{
			int firstILMarker = nextBlock.FirstILMarker;
			if (firstILMarker >= 0)
			{
				int lastILMarker = nextBlock.LastILMarker;
				for (int i = firstILMarker; i <= lastILMarker; i++)
				{
					int blockOffset = _allocatedILMarkers[i].BlockOffset;
					int absoluteOffset = instance.Count + blockOffset;
					_allocatedILMarkers[i] = new ILMarker
					{
						BlockOffset = blockOffset,
						AbsoluteOffset = absoluteOffset
					};
				}
			}
			nextBlock.RegularInstructions?.WriteContentTo(instance);
			switch (nextBlock.BranchCode)
			{
			case ILOpCode.Switch:
			{
				WriteOpCode(instance, ILOpCode.Switch);
				SwitchBlock switchBlock = (SwitchBlock)nextBlock;
				instance.WriteUInt32(switchBlock.BranchesCount);
				int num3 = switchBlock.Start + switchBlock.TotalSize;
				ArrayBuilder<BasicBlock> instance2 = ArrayBuilder<BasicBlock>.GetInstance();
				switchBlock.GetBranchBlocks(instance2);
				foreach (BasicBlock item in instance2)
				{
					instance.WriteInt32(item.Start - num3);
				}
				instance2.Free();
				break;
			}
			default:
				WriteOpCode(instance, nextBlock.BranchCode);
				if (nextBlock.BranchLabel != null)
				{
					int start = nextBlock.BranchBlock.Start;
					int num = nextBlock.Start + nextBlock.TotalSize;
					int num2 = start - num;
					if (nextBlock.BranchCode.GetBranchOperandSize() == 1)
					{
						sbyte value = (sbyte)num2;
						instance.WriteSByte(value);
					}
					else
					{
						instance.WriteInt32(num2);
					}
				}
				break;
			case ILOpCode.Nop:
				break;
			}
		}
		RealizedIL = instance.ToImmutableArray();
		instance.Free();
		RealizeSequencePoints();
		RealizedExceptionHandlers = _scopeManager.GetExceptionHandlerRegions();
	}

	private void RealizeSequencePoints()
	{
		if (SeqPointsOpt == null)
		{
			return;
		}
		int num = -1;
		ArrayBuilder<RawSequencePoint> instance = ArrayBuilder<RawSequencePoint>.GetInstance();
		foreach (RawSequencePoint item in SeqPointsOpt)
		{
			int iLOffsetFromMarker = GetILOffsetFromMarker(item.ILMarker);
			if (iLOffsetFromMarker >= 0)
			{
				if (num != iLOffsetFromMarker)
				{
					num = iLOffsetFromMarker;
					instance.Add(item);
				}
				else
				{
					instance[instance.Count - 1] = item;
				}
			}
		}
		if (instance.Count > 0)
		{
			RealizedSequencePoints = SequencePointList.Create(instance, this);
		}
		instance.Free();
	}

	internal void DefineSequencePoint(SyntaxTree syntaxTree, TextSpan span)
	{
		GetCurrentBlock();
		_lastSeqPointTree = syntaxTree;
		if (SeqPointsOpt == null)
		{
			SeqPointsOpt = ArrayBuilder<RawSequencePoint>.GetInstance();
		}
		if (_initialHiddenSequencePointMarker >= 0)
		{
			SeqPointsOpt.Add(new RawSequencePoint(syntaxTree, _initialHiddenSequencePointMarker, RawSequencePoint.HiddenSequencePointSpan));
			_initialHiddenSequencePointMarker = -1;
		}
		SeqPointsOpt.Add(new RawSequencePoint(syntaxTree, AllocateILMarker(), span));
	}

	internal void DefineHiddenSequencePoint()
	{
		SyntaxTree lastSeqPointTree = _lastSeqPointTree;
		if (lastSeqPointTree != null)
		{
			DefineSequencePoint(lastSeqPointTree, RawSequencePoint.HiddenSequencePointSpan);
		}
	}

	internal void DefineInitialHiddenSequencePoint()
	{
		_initialHiddenSequencePointMarker = AllocateILMarker();
	}

	internal void SetInitialDebugDocument(SyntaxTree initialSequencePointTree)
	{
		_lastSeqPointTree = initialSequencePointTree;
	}

	[Conditional("DEBUG")]
	internal void AssertStackEmpty()
	{
	}

	internal bool IsJustPastLabel()
	{
		return _emitState.InstructionsEmitted == _instructionCountAtLastLabel;
	}

	internal void OpenLocalScope(ScopeType scopeType = ScopeType.Variable, ITypeReference exceptionType = null)
	{
		if (scopeType == ScopeType.TryCatchFinally && IsJustPastLabel())
		{
			DefineHiddenSequencePoint();
			EmitOpCode(ILOpCode.Nop);
		}
		if (scopeType == ScopeType.Finally)
		{
			_instructionCountAtLastLabel = _emitState.InstructionsEmitted;
		}
		EndBlock();
		_scopeManager.OpenScope(scopeType, exceptionType);
		switch (scopeType)
		{
		case ScopeType.Try:
			_pendingBlockCreate = true;
			break;
		case ScopeType.Catch:
		case ScopeType.Filter:
		case ScopeType.Finally:
		case ScopeType.Fault:
			_pendingBlockCreate = true;
			DefineHiddenSequencePoint();
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(scopeType);
		case ScopeType.Variable:
		case ScopeType.TryCatchFinally:
		case ScopeType.StateMachineVariable:
			break;
		}
	}

	internal bool PossiblyDefinedOutsideOfTry(LocalDefinition local)
	{
		return _scopeManager.PossiblyDefinedOutsideOfTry(local);
	}

	internal void MarkFilterConditionEnd()
	{
		_scopeManager.FinishFilterCondition(this);
		DefineHiddenSequencePoint();
	}

	internal void CloseLocalScope()
	{
		_scopeManager.ClosingScope(this);
		EndBlock();
		_scopeManager.CloseScope(this);
	}

	internal void DefineUserDefinedStateMachineHoistedLocal(int slotIndex)
	{
		_scopeManager.AddUserHoistedLocal(slotIndex);
	}

	internal void AddLocalToScope(LocalDefinition local)
	{
		HasDynamicLocal |= !local.DynamicTransformFlags.IsEmpty;
		_scopeManager.AddLocal(local);
	}

	internal void AddLocalConstantToScope(LocalConstantDefinition localConstant)
	{
		HasDynamicLocal |= !localConstant.DynamicTransformFlags.IsEmpty;
		_scopeManager.AddLocalConstant(localConstant);
	}

	internal ILBuilder GetSnapshot()
	{
		ILBuilder iLBuilder = (ILBuilder)MemberwiseClone();
		iLBuilder.RealizedIL = RealizedIL;
		return iLBuilder;
	}

	private bool AllBlocks(Func<BasicBlock, bool> predicate)
	{
		for (BasicBlock nextBlock = leaderBlock; nextBlock != null; nextBlock = nextBlock.NextBlock)
		{
			if (!predicate(nextBlock))
			{
				return false;
			}
		}
		return true;
	}

	internal int AllocateILMarker()
	{
		if (_allocatedILMarkers == null)
		{
			_allocatedILMarkers = ArrayBuilder<ILMarker>.GetInstance();
		}
		BasicBlock currentBlock = GetCurrentBlock();
		int count = _allocatedILMarkers.Count;
		currentBlock.AddILMarker(count);
		_allocatedILMarkers.Add(new ILMarker
		{
			BlockOffset = currentBlock.RegularInstructionsLength,
			AbsoluteOffset = -1
		});
		return count;
	}

	public int GetILOffsetFromMarker(int ilMarker)
	{
		return _allocatedILMarkers[ilMarker].AbsoluteOffset;
	}

	private string GetDebuggerDisplay()
	{
		return "";
	}

	public void EmitNumericConversion(Microsoft.Cci.PrimitiveTypeCode fromPredefTypeKind, Microsoft.Cci.PrimitiveTypeCode toPredefTypeKind, bool @checked)
	{
		bool flag = fromPredefTypeKind.IsUnsigned();
		switch (toPredefTypeKind)
		{
		case Microsoft.Cci.PrimitiveTypeCode.Int8:
			if (fromPredefTypeKind != Microsoft.Cci.PrimitiveTypeCode.Int8)
			{
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_i1_un : ILOpCode.Conv_ovf_i1);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_i1);
				}
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			if (fromPredefTypeKind != Microsoft.Cci.PrimitiveTypeCode.UInt8)
			{
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_u1_un : ILOpCode.Conv_ovf_u1);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_u1);
				}
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.Int16:
			if (fromPredefTypeKind != Microsoft.Cci.PrimitiveTypeCode.Int8 && fromPredefTypeKind != Microsoft.Cci.PrimitiveTypeCode.Int16 && fromPredefTypeKind != Microsoft.Cci.PrimitiveTypeCode.UInt8)
			{
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_i2_un : ILOpCode.Conv_ovf_i2);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_i2);
				}
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.Char:
		case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			if (fromPredefTypeKind != Microsoft.Cci.PrimitiveTypeCode.Char && (uint)(fromPredefTypeKind - 12) > 1u)
			{
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_u2_un : ILOpCode.Conv_ovf_u2);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_u2);
				}
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.Int32:
			switch (fromPredefTypeKind)
			{
			case Microsoft.Cci.PrimitiveTypeCode.UInt32:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_i4_un);
				}
				break;
			default:
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_i4_un : ILOpCode.Conv_ovf_i4);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_i4);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Char:
			case Microsoft.Cci.PrimitiveTypeCode.Int8:
			case Microsoft.Cci.PrimitiveTypeCode.Int16:
			case Microsoft.Cci.PrimitiveTypeCode.Int32:
			case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			case Microsoft.Cci.PrimitiveTypeCode.UInt16:
				break;
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.UInt32:
			switch (fromPredefTypeKind)
			{
			case Microsoft.Cci.PrimitiveTypeCode.Int8:
			case Microsoft.Cci.PrimitiveTypeCode.Int16:
			case Microsoft.Cci.PrimitiveTypeCode.Int32:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_u4);
				}
				break;
			default:
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_u4_un : ILOpCode.Conv_ovf_u4);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_u4);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Char:
			case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			case Microsoft.Cci.PrimitiveTypeCode.UInt32:
				break;
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
			switch (fromPredefTypeKind)
			{
			case Microsoft.Cci.PrimitiveTypeCode.Int8:
			case Microsoft.Cci.PrimitiveTypeCode.Int16:
			case Microsoft.Cci.PrimitiveTypeCode.Int32:
				EmitOpCode(ILOpCode.Conv_i);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Char:
			case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			case Microsoft.Cci.PrimitiveTypeCode.UInt16:
				EmitOpCode(ILOpCode.Conv_u);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.UInt32:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_i_un);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_u);
				}
				break;
			default:
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_i_un : ILOpCode.Conv_ovf_i);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_i);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
				break;
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
			switch (fromPredefTypeKind)
			{
			case Microsoft.Cci.PrimitiveTypeCode.Char:
			case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			case Microsoft.Cci.PrimitiveTypeCode.UInt32:
				EmitOpCode(ILOpCode.Conv_u);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Int8:
			case Microsoft.Cci.PrimitiveTypeCode.Int16:
			case Microsoft.Cci.PrimitiveTypeCode.Int32:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_u);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_i);
				}
				break;
			default:
				if (@checked)
				{
					EmitOpCode(flag ? ILOpCode.Conv_ovf_u_un : ILOpCode.Conv_ovf_u);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_u);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
				break;
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.Int64:
			switch (fromPredefTypeKind)
			{
			case Microsoft.Cci.PrimitiveTypeCode.Int8:
			case Microsoft.Cci.PrimitiveTypeCode.Int16:
			case Microsoft.Cci.PrimitiveTypeCode.Int32:
			case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
				EmitOpCode(ILOpCode.Conv_i8);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Char:
			case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			case Microsoft.Cci.PrimitiveTypeCode.UInt32:
				EmitOpCode(ILOpCode.Conv_u8);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Pointer:
			case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_i8_un);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_u8);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.UInt64:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_i8_un);
				}
				break;
			default:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_i8);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_i8);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Int64:
				break;
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.UInt64:
			switch (fromPredefTypeKind)
			{
			case Microsoft.Cci.PrimitiveTypeCode.Char:
			case Microsoft.Cci.PrimitiveTypeCode.Pointer:
			case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			case Microsoft.Cci.PrimitiveTypeCode.UInt32:
			case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
				EmitOpCode(ILOpCode.Conv_u8);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Int8:
			case Microsoft.Cci.PrimitiveTypeCode.Int16:
			case Microsoft.Cci.PrimitiveTypeCode.Int32:
			case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_u8);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_i8);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Int64:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_u8);
				}
				break;
			default:
				if (@checked)
				{
					EmitOpCode(ILOpCode.Conv_ovf_u8);
				}
				else
				{
					EmitOpCode(ILOpCode.Conv_u8);
				}
				break;
			case Microsoft.Cci.PrimitiveTypeCode.UInt64:
				break;
			}
			break;
		case Microsoft.Cci.PrimitiveTypeCode.Float32:
			if ((uint)(fromPredefTypeKind - 14) <= 1u)
			{
				EmitOpCode(ILOpCode.Conv_r_un);
			}
			EmitOpCode(ILOpCode.Conv_r4);
			break;
		case Microsoft.Cci.PrimitiveTypeCode.Float64:
			if ((uint)(fromPredefTypeKind - 14) <= 1u)
			{
				EmitOpCode(ILOpCode.Conv_r_un);
			}
			EmitOpCode(ILOpCode.Conv_r8);
			break;
		case Microsoft.Cci.PrimitiveTypeCode.Pointer:
			if (@checked)
			{
				switch (fromPredefTypeKind)
				{
				case Microsoft.Cci.PrimitiveTypeCode.UInt8:
				case Microsoft.Cci.PrimitiveTypeCode.UInt16:
				case Microsoft.Cci.PrimitiveTypeCode.UInt32:
					EmitOpCode(ILOpCode.Conv_u);
					break;
				case Microsoft.Cci.PrimitiveTypeCode.UInt64:
					EmitOpCode(ILOpCode.Conv_ovf_u_un);
					break;
				case Microsoft.Cci.PrimitiveTypeCode.Int8:
				case Microsoft.Cci.PrimitiveTypeCode.Int16:
				case Microsoft.Cci.PrimitiveTypeCode.Int32:
				case Microsoft.Cci.PrimitiveTypeCode.Int64:
					EmitOpCode(ILOpCode.Conv_ovf_u);
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(fromPredefTypeKind);
				}
			}
			else
			{
				switch (fromPredefTypeKind)
				{
				case Microsoft.Cci.PrimitiveTypeCode.Int64:
				case Microsoft.Cci.PrimitiveTypeCode.UInt8:
				case Microsoft.Cci.PrimitiveTypeCode.UInt16:
				case Microsoft.Cci.PrimitiveTypeCode.UInt32:
				case Microsoft.Cci.PrimitiveTypeCode.UInt64:
					EmitOpCode(ILOpCode.Conv_u);
					break;
				case Microsoft.Cci.PrimitiveTypeCode.Int8:
				case Microsoft.Cci.PrimitiveTypeCode.Int16:
				case Microsoft.Cci.PrimitiveTypeCode.Int32:
					EmitOpCode(ILOpCode.Conv_i);
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(fromPredefTypeKind);
				}
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(toPredefTypeKind);
		}
	}

	internal void AdjustStack(int stackAdjustment)
	{
		_emitState.AdjustStack(stackAdjustment);
	}

	internal void EmitOpCode(ILOpCode code)
	{
		EmitOpCode(code, code.NetStackBehavior());
	}

	internal void EmitOpCode(ILOpCode code, int stackAdjustment)
	{
		WriteOpCode(GetCurrentWriter(), code);
		_emitState.AdjustStack(stackAdjustment);
		_emitState.InstructionAdded();
	}

	internal void EmitToken(string value)
	{
		uint value2 = module?.GetFakeStringTokenForIL(value) ?? 65535;
		GetCurrentWriter().WriteUInt32(value2);
	}

	internal void EmitToken(IReference value, SyntaxNode syntaxNode, DiagnosticBag diagnostics, bool encodeAsRawToken = false)
	{
		uint num = module?.GetFakeSymbolTokenForIL(value, syntaxNode, diagnostics) ?? 65535;
		if (encodeAsRawToken)
		{
			num |= 0x80000000u;
		}
		GetCurrentWriter().WriteUInt32(num);
	}

	internal void EmitGreatestMethodToken()
	{
		GetCurrentWriter().WriteUInt32(1073741824u);
	}

	internal void EmitModuleVersionIdStringToken()
	{
		GetCurrentWriter().WriteUInt32(2147483648u);
	}

	internal void EmitSourceDocumentIndexToken(DebugSourceDocument document)
	{
		GetCurrentWriter().WriteUInt32((module?.GetSourceDocumentIndexForIL(document) ?? 65535) | 0x20000000);
	}

	internal void EmitArrayBlockInitializer(ImmutableArray<byte> data, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		IMethodReference initArrayHelper = module.GetInitArrayHelper();
		IFieldReference fieldForData = module.GetFieldForData(data, syntaxNode, diagnostics);
		EmitOpCode(ILOpCode.Dup);
		EmitOpCode(ILOpCode.Ldtoken);
		EmitToken(fieldForData, syntaxNode, diagnostics);
		EmitOpCode(ILOpCode.Call, -2);
		EmitToken(initArrayHelper, syntaxNode, diagnostics);
	}

	internal void EmitStackAllocBlockInitializer(ImmutableArray<byte> data, SyntaxNode syntaxNode, bool emitInitBlock, DiagnosticBag diagnostics)
	{
		if (emitInitBlock)
		{
			EmitOpCode(ILOpCode.Dup);
			EmitIntConstant(data[0]);
			EmitIntConstant(data.Length);
			EmitOpCode(ILOpCode.Initblk, -3);
		}
		else
		{
			IFieldReference fieldForData = module.GetFieldForData(data, syntaxNode, diagnostics);
			EmitOpCode(ILOpCode.Dup);
			EmitOpCode(ILOpCode.Ldsflda);
			EmitToken(fieldForData, syntaxNode, diagnostics);
			EmitIntConstant(data.Length);
			EmitOpCode(ILOpCode.Cpblk, -3);
		}
	}

	internal void EmitArrayBlockFieldRef(ImmutableArray<byte> data, ITypeSymbol elementType, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		IFieldReference fieldForData = module.GetFieldForData(data, syntaxNode, diagnostics);
		EmitOpCode(ILOpCode.Ldsflda);
		EmitToken(fieldForData, syntaxNode, diagnostics);
	}

	internal void MarkLabel(object label)
	{
		EndBlock();
		BasicBlock currentBlock = GetCurrentBlock();
		if (_labelInfos.TryGetValue(label, out var value))
		{
			_ = _emitState.CurStack;
			_labelInfos[label] = value.WithNewTarget(currentBlock);
		}
		else
		{
			int curStack = _emitState.CurStack;
			_labelInfos[label] = new LabelInfo(currentBlock, curStack, targetOfConditionalBranches: false);
		}
		_instructionCountAtLastLabel = _emitState.InstructionsEmitted;
	}

	internal void EmitBranch(ILOpCode code, object label, ILOpCode revOpCode = ILOpCode.Nop)
	{
		if (code != ILOpCode.Nop)
		{
			code.IsBranch();
		}
		_emitState.AdjustStack(code.NetStackBehavior());
		bool targetOfConditionalBranches = code.IsConditionalBranch();
		if (!_labelInfos.TryGetValue(label, out var _))
		{
			_labelInfos.Add(label, new LabelInfo(_emitState.CurStack, targetOfConditionalBranches));
		}
		GetCurrentBlock().SetBranch(label, code, revOpCode);
		if (code != ILOpCode.Nop)
		{
			_emitState.InstructionAdded();
		}
		EndBlock();
	}

	internal void EmitStringSwitchJumpTable(KeyValuePair<ConstantValue, object>[] caseLabels, object fallThroughLabel, LocalOrParameter key, LocalDefinition keyHash, SwitchStringJumpTableEmitter.EmitStringCompareAndBranch emitStringCondBranchDelegate, SwitchStringJumpTableEmitter.GetStringHashCode computeStringHashcodeDelegate)
	{
		new SwitchStringJumpTableEmitter(this, key, caseLabels, fallThroughLabel, keyHash, emitStringCondBranchDelegate, computeStringHashcodeDelegate).EmitJumpTable();
	}

	internal void EmitIntegerSwitchJumpTable(KeyValuePair<ConstantValue, object>[] caseLabels, object fallThroughLabel, LocalOrParameter key, Microsoft.Cci.PrimitiveTypeCode keyTypeCode)
	{
		new SwitchIntegralJumpTableEmitter(this, caseLabels, fallThroughLabel, keyTypeCode, key).EmitJumpTable();
	}

	internal void EmitSwitch(object[] labels)
	{
		_emitState.AdjustStack(-1);
		int curStack = _emitState.CurStack;
		foreach (object key in labels)
		{
			if (!_labelInfos.TryGetValue(key, out var value))
			{
				_labelInfos.Add(key, new LabelInfo(curStack, targetOfConditionalBranches: true));
			}
			else if (!value.targetOfConditionalBranches)
			{
				_labelInfos[key] = value.SetTargetOfConditionalBranches();
			}
		}
		CreateSwitchBlock().BranchLabels = labels;
		EndBlock();
	}

	internal void EmitRet(bool isVoid)
	{
		if (!isVoid)
		{
			_emitState.AdjustStack(-1);
		}
		GetCurrentBlock().SetBranchCode(ILOpCode.Ret);
		_emitState.InstructionAdded();
		EndBlock();
	}

	internal void EmitThrow(bool isRethrow)
	{
		BasicBlock currentBlock = GetCurrentBlock();
		if (isRethrow)
		{
			currentBlock.SetBranchCode(ILOpCode.Rethrow);
		}
		else
		{
			currentBlock.SetBranchCode(ILOpCode.Throw);
			_emitState.AdjustStack(-1);
		}
		_emitState.InstructionAdded();
		EndBlock();
	}

	private void EmitEndFinally()
	{
		GetCurrentBlock().SetBranchCode(ILOpCode.Endfinally);
		EndBlock();
	}

	private BasicBlock FinishFilterCondition()
	{
		BasicBlock currentBlock = GetCurrentBlock();
		currentBlock.SetBranchCode(ILOpCode.Endfilter);
		EndBlock();
		return currentBlock;
	}

	internal void EmitArrayCreation(IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		ArrayMethod arrayConstructor = module.ArrayMethods.GetArrayConstructor(arrayType);
		EmitOpCode(ILOpCode.Newobj, 1 - arrayType.Rank);
		EmitToken(arrayConstructor, syntaxNode, diagnostics);
	}

	internal void EmitArrayElementLoad(IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		ArrayMethod arrayGet = module.ArrayMethods.GetArrayGet(arrayType);
		EmitOpCode(ILOpCode.Call, -arrayType.Rank);
		EmitToken(arrayGet, syntaxNode, diagnostics);
	}

	internal void EmitArrayElementAddress(IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		ArrayMethod arrayAddress = module.ArrayMethods.GetArrayAddress(arrayType);
		EmitOpCode(ILOpCode.Call, -arrayType.Rank);
		EmitToken(arrayAddress, syntaxNode, diagnostics);
	}

	internal void EmitArrayElementStore(IArrayTypeReference arrayType, SyntaxNode syntaxNode, DiagnosticBag diagnostics)
	{
		ArrayMethod arraySet = module.ArrayMethods.GetArraySet(arrayType);
		EmitOpCode(ILOpCode.Call, -(2 + arrayType.Rank));
		EmitToken(arraySet, syntaxNode, diagnostics);
	}

	internal void EmitLoad(LocalOrParameter localOrParameter)
	{
		if (localOrParameter.Local != null)
		{
			EmitLocalLoad(localOrParameter.Local);
		}
		else
		{
			EmitLoadArgumentOpcode(localOrParameter.ParameterIndex);
		}
	}

	internal void EmitLocalLoad(LocalDefinition local)
	{
		int slotIndex = local.SlotIndex;
		switch (slotIndex)
		{
		case 0:
			EmitOpCode(ILOpCode.Ldloc_0);
			return;
		case 1:
			EmitOpCode(ILOpCode.Ldloc_1);
			return;
		case 2:
			EmitOpCode(ILOpCode.Ldloc_2);
			return;
		case 3:
			EmitOpCode(ILOpCode.Ldloc_3);
			return;
		}
		if (slotIndex < 255)
		{
			EmitOpCode(ILOpCode.Ldloc_s);
			EmitInt8((sbyte)slotIndex);
		}
		else
		{
			EmitOpCode(ILOpCode.Ldloc);
			EmitInt32(slotIndex);
		}
	}

	internal void EmitLocalStore(LocalDefinition local)
	{
		int slotIndex = local.SlotIndex;
		switch (slotIndex)
		{
		case 0:
			EmitOpCode(ILOpCode.Stloc_0);
			return;
		case 1:
			EmitOpCode(ILOpCode.Stloc_1);
			return;
		case 2:
			EmitOpCode(ILOpCode.Stloc_2);
			return;
		case 3:
			EmitOpCode(ILOpCode.Stloc_3);
			return;
		}
		if (slotIndex < 255)
		{
			EmitOpCode(ILOpCode.Stloc_s);
			EmitInt8((sbyte)slotIndex);
		}
		else
		{
			EmitOpCode(ILOpCode.Stloc);
			EmitInt32(slotIndex);
		}
	}

	internal void EmitLocalAddress(LocalDefinition local)
	{
		if (local.IsReference)
		{
			EmitLocalLoad(local);
			return;
		}
		int slotIndex = local.SlotIndex;
		if (slotIndex < 255)
		{
			EmitOpCode(ILOpCode.Ldloca_s);
			EmitInt8((sbyte)slotIndex);
		}
		else
		{
			EmitOpCode(ILOpCode.Ldloca);
			EmitInt32(slotIndex);
		}
	}

	internal void EmitLoadArgumentOpcode(int argNumber)
	{
		switch (argNumber)
		{
		case 0:
			EmitOpCode(ILOpCode.Ldarg_0);
			return;
		case 1:
			EmitOpCode(ILOpCode.Ldarg_1);
			return;
		case 2:
			EmitOpCode(ILOpCode.Ldarg_2);
			return;
		case 3:
			EmitOpCode(ILOpCode.Ldarg_3);
			return;
		}
		if (argNumber < 255)
		{
			EmitOpCode(ILOpCode.Ldarg_s);
			EmitInt8((sbyte)argNumber);
		}
		else
		{
			EmitOpCode(ILOpCode.Ldarg);
			EmitInt32(argNumber);
		}
	}

	internal void EmitLoadArgumentAddrOpcode(int argNumber)
	{
		if (argNumber < 255)
		{
			EmitOpCode(ILOpCode.Ldarga_s);
			EmitInt8((sbyte)argNumber);
		}
		else
		{
			EmitOpCode(ILOpCode.Ldarga);
			EmitInt32(argNumber);
		}
	}

	internal void EmitStoreArgumentOpcode(int argNumber)
	{
		if (argNumber < 255)
		{
			EmitOpCode(ILOpCode.Starg_s);
			EmitInt8((sbyte)argNumber);
		}
		else
		{
			EmitOpCode(ILOpCode.Starg);
			EmitInt32(argNumber);
		}
	}

	internal void EmitConstantValue(ConstantValue value)
	{
		ConstantValueTypeDiscriminator discriminator = value.Discriminator;
		switch (discriminator)
		{
		case ConstantValueTypeDiscriminator.Nothing:
			EmitNullConstant();
			break;
		case ConstantValueTypeDiscriminator.SByte:
			EmitSByteConstant(value.SByteValue);
			break;
		case ConstantValueTypeDiscriminator.Byte:
			EmitByteConstant(value.ByteValue);
			break;
		case ConstantValueTypeDiscriminator.UInt16:
			EmitUShortConstant(value.UInt16Value);
			break;
		case ConstantValueTypeDiscriminator.Char:
			EmitUShortConstant(value.CharValue);
			break;
		case ConstantValueTypeDiscriminator.Int16:
			EmitShortConstant(value.Int16Value);
			break;
		case ConstantValueTypeDiscriminator.Int32:
		case ConstantValueTypeDiscriminator.UInt32:
			EmitIntConstant(value.Int32Value);
			break;
		case ConstantValueTypeDiscriminator.Int64:
		case ConstantValueTypeDiscriminator.UInt64:
			EmitLongConstant(value.Int64Value);
			break;
		case ConstantValueTypeDiscriminator.Single:
			EmitSingleConstant(value.SingleValue);
			break;
		case ConstantValueTypeDiscriminator.Double:
			EmitDoubleConstant(value.DoubleValue);
			break;
		case ConstantValueTypeDiscriminator.String:
			EmitStringConstant(value.StringValue);
			break;
		case ConstantValueTypeDiscriminator.Boolean:
			EmitBoolConstant(value.BooleanValue);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(discriminator);
		}
	}

	internal void EmitIntConstant(int value)
	{
		ILOpCode iLOpCode = ILOpCode.Nop;
		switch (value)
		{
		case -1:
			iLOpCode = ILOpCode.Ldc_i4_m1;
			break;
		case 0:
			iLOpCode = ILOpCode.Ldc_i4_0;
			break;
		case 1:
			iLOpCode = ILOpCode.Ldc_i4_1;
			break;
		case 2:
			iLOpCode = ILOpCode.Ldc_i4_2;
			break;
		case 3:
			iLOpCode = ILOpCode.Ldc_i4_3;
			break;
		case 4:
			iLOpCode = ILOpCode.Ldc_i4_4;
			break;
		case 5:
			iLOpCode = ILOpCode.Ldc_i4_5;
			break;
		case 6:
			iLOpCode = ILOpCode.Ldc_i4_6;
			break;
		case 7:
			iLOpCode = ILOpCode.Ldc_i4_7;
			break;
		case 8:
			iLOpCode = ILOpCode.Ldc_i4_8;
			break;
		}
		if (iLOpCode != ILOpCode.Nop)
		{
			EmitOpCode(iLOpCode);
		}
		else if ((sbyte)value == value)
		{
			EmitOpCode(ILOpCode.Ldc_i4_s);
			EmitInt8((sbyte)value);
		}
		else
		{
			EmitOpCode(ILOpCode.Ldc_i4);
			EmitInt32(value);
		}
	}

	internal void EmitBoolConstant(bool value)
	{
		EmitIntConstant(value ? 1 : 0);
	}

	internal void EmitByteConstant(byte value)
	{
		EmitIntConstant(value);
	}

	internal void EmitSByteConstant(sbyte value)
	{
		EmitIntConstant(value);
	}

	internal void EmitShortConstant(short value)
	{
		EmitIntConstant(value);
	}

	internal void EmitUShortConstant(ushort value)
	{
		EmitIntConstant(value);
	}

	internal void EmitLongConstant(long value)
	{
		if (value >= int.MinValue && value <= int.MaxValue)
		{
			EmitIntConstant((int)value);
			EmitOpCode(ILOpCode.Conv_i8);
		}
		else if (value >= 0 && value <= uint.MaxValue)
		{
			EmitIntConstant((int)value);
			EmitOpCode(ILOpCode.Conv_u8);
		}
		else
		{
			EmitOpCode(ILOpCode.Ldc_i8);
			EmitInt64(value);
		}
	}

	internal void EmitSingleConstant(float value)
	{
		EmitOpCode(ILOpCode.Ldc_r4);
		EmitFloat(value);
	}

	internal void EmitDoubleConstant(double value)
	{
		EmitOpCode(ILOpCode.Ldc_r8);
		EmitDouble(value);
	}

	internal void EmitNullConstant()
	{
		EmitOpCode(ILOpCode.Ldnull);
	}

	internal void EmitStringConstant(string value)
	{
		if (value == null)
		{
			EmitNullConstant();
			return;
		}
		EmitOpCode(ILOpCode.Ldstr);
		EmitToken(value);
	}

	private void EmitInt8(sbyte int8)
	{
		GetCurrentWriter().WriteSByte(int8);
	}

	private void EmitInt32(int int32)
	{
		GetCurrentWriter().WriteInt32(int32);
	}

	private void EmitInt64(long int64)
	{
		GetCurrentWriter().WriteInt64(int64);
	}

	private void EmitFloat(float floatValue)
	{
		int value = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
		GetCurrentWriter().WriteInt32(value);
	}

	private void EmitDouble(double doubleValue)
	{
		long value = BitConverter.DoubleToInt64Bits(doubleValue);
		GetCurrentWriter().WriteInt64(value);
	}

	private static void WriteOpCode(BlobBuilder writer, ILOpCode code)
	{
		if (code.Size() == 1)
		{
			writer.WriteByte((byte)code);
			return;
		}
		writer.WriteByte((byte)((int)code >> 8));
		writer.WriteByte((byte)(code & (ILOpCode)0xFF));
	}

	private BlobBuilder GetCurrentWriter()
	{
		return GetCurrentBlock().Writer;
	}
}
