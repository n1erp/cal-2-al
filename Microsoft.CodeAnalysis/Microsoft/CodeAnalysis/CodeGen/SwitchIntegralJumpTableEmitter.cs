using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal struct SwitchIntegralJumpTableEmitter
{
	private struct SwitchBucket
	{
		private readonly ImmutableArray<KeyValuePair<ConstantValue, object>> _allLabels;

		private readonly int _startLabelIndex;

		private readonly int _endLabelIndex;

		private readonly bool _isKnownDegenerate;

		internal bool IsDegenerate => _isKnownDegenerate;

		internal uint LabelsCount => (uint)(_endLabelIndex - _startLabelIndex + 1);

		internal KeyValuePair<ConstantValue, object> this[int i] => _allLabels[i + _startLabelIndex];

		internal ulong BucketSize => GetBucketSize(StartConstant, EndConstant);

		internal int DegenerateBucketSplit
		{
			get
			{
				if (IsDegenerate)
				{
					return 0;
				}
				ImmutableArray<KeyValuePair<ConstantValue, object>> allLabels = _allLabels;
				int num = 0;
				ConstantValue lastConst = StartConstant;
				object value = allLabels[_startLabelIndex].Value;
				for (int i = _startLabelIndex + 1; i <= _endLabelIndex; i++)
				{
					KeyValuePair<ConstantValue, object> keyValuePair = allLabels[i];
					if (value != keyValuePair.Value || !IsContiguous(lastConst, keyValuePair.Key))
					{
						if (num != 0)
						{
							return -1;
						}
						num = i;
						value = keyValuePair.Value;
					}
					lastConst = keyValuePair.Key;
				}
				return num;
			}
		}

		internal int StartLabelIndex => _startLabelIndex;

		internal int EndLabelIndex => _endLabelIndex;

		internal ConstantValue StartConstant => _allLabels[_startLabelIndex].Key;

		internal ConstantValue EndConstant => _allLabels[_endLabelIndex].Key;

		internal SwitchBucket(ImmutableArray<KeyValuePair<ConstantValue, object>> allLabels, int index)
		{
			_startLabelIndex = index;
			_endLabelIndex = index;
			_allLabels = allLabels;
			_isKnownDegenerate = true;
		}

		private SwitchBucket(ImmutableArray<KeyValuePair<ConstantValue, object>> allLabels, int startIndex, int endIndex)
		{
			_startLabelIndex = startIndex;
			_endLabelIndex = endIndex;
			_allLabels = allLabels;
			_isKnownDegenerate = false;
		}

		internal SwitchBucket(ImmutableArray<KeyValuePair<ConstantValue, object>> allLabels, int startIndex, int endIndex, bool isDegenerate)
		{
			_startLabelIndex = startIndex;
			_endLabelIndex = endIndex;
			_allLabels = allLabels;
			_isKnownDegenerate = isDegenerate;
		}

		private bool IsContiguous(ConstantValue lastConst, ConstantValue nextConst)
		{
			if (!lastConst.IsNumeric || !nextConst.IsNumeric)
			{
				return false;
			}
			return GetBucketSize(lastConst, nextConst) == 2;
		}

		private static ulong GetBucketSize(ConstantValue startConstant, ConstantValue endConstant)
		{
			if (startConstant.IsNegativeNumeric || endConstant.IsNegativeNumeric)
			{
				return (ulong)(endConstant.Int64Value - startConstant.Int64Value + 1);
			}
			return endConstant.UInt64Value - startConstant.UInt64Value + 1;
		}

		private static bool BucketOverflowUInt64Limit(ConstantValue startConstant, ConstantValue endConstant)
		{
			if (startConstant.Discriminator == ConstantValueTypeDiscriminator.Int64)
			{
				if (startConstant.Int64Value == long.MinValue)
				{
					return endConstant.Int64Value == long.MaxValue;
				}
				return false;
			}
			if (startConstant.Discriminator == ConstantValueTypeDiscriminator.UInt64)
			{
				if (startConstant.UInt64Value == 0L)
				{
					return endConstant.UInt64Value == ulong.MaxValue;
				}
				return false;
			}
			return false;
		}

		private static bool BucketOverflow(ConstantValue startConstant, ConstantValue endConstant)
		{
			if (!BucketOverflowUInt64Limit(startConstant, endConstant))
			{
				return GetBucketSize(startConstant, endConstant) > int.MaxValue;
			}
			return true;
		}

		private static bool IsValidSwitchBucketConstant(ConstantValue constant)
		{
			if (constant != null && SwitchConstantValueHelper.IsValidSwitchCaseLabelConstant(constant) && !constant.IsNull)
			{
				return !constant.IsString;
			}
			return false;
		}

		private static bool IsValidSwitchBucketConstantPair(ConstantValue startConstant, ConstantValue endConstant)
		{
			if (IsValidSwitchBucketConstant(startConstant) && IsValidSwitchBucketConstant(endConstant))
			{
				return startConstant.IsUnsigned == endConstant.IsUnsigned;
			}
			return false;
		}

		private static bool IsSparse(uint labelsCount, ulong bucketSize)
		{
			return bucketSize >= labelsCount * 2;
		}

		internal static bool MergeIsAdvantageous(SwitchBucket bucket1, SwitchBucket bucket2)
		{
			ConstantValue startConstant = bucket1.StartConstant;
			ConstantValue endConstant = bucket2.EndConstant;
			if (BucketOverflow(startConstant, endConstant))
			{
				return false;
			}
			uint labelsCount = bucket1.LabelsCount + bucket2.LabelsCount;
			ulong bucketSize = GetBucketSize(startConstant, endConstant);
			return !IsSparse(labelsCount, bucketSize);
		}

		internal bool TryMergeWith(SwitchBucket prevBucket)
		{
			if (MergeIsAdvantageous(prevBucket, this))
			{
				this = new SwitchBucket(_allLabels, prevBucket._startLabelIndex, _endLabelIndex);
				return true;
			}
			return false;
		}
	}

	private readonly ILBuilder _builder;

	private readonly LocalOrParameter _key;

	private readonly Microsoft.Cci.PrimitiveTypeCode _keyTypeCode;

	private readonly object _fallThroughLabel;

	private readonly ImmutableArray<KeyValuePair<ConstantValue, object>> _sortedCaseLabels;

	private const int LinearSearchThreshold = 3;

	internal SwitchIntegralJumpTableEmitter(ILBuilder builder, KeyValuePair<ConstantValue, object>[] caseLabels, object fallThroughLabel, Microsoft.Cci.PrimitiveTypeCode keyTypeCode, LocalOrParameter key)
	{
		_builder = builder;
		_key = key;
		_keyTypeCode = keyTypeCode;
		_fallThroughLabel = fallThroughLabel;
		Array.Sort(caseLabels, CompareIntegralSwitchLabels);
		_sortedCaseLabels = ImmutableArray.Create(caseLabels);
	}

	internal void EmitJumpTable()
	{
		ImmutableArray<KeyValuePair<ConstantValue, object>> sortedCaseLabels = _sortedCaseLabels;
		int num = sortedCaseLabels.Length - 1;
		int num2 = ((!(sortedCaseLabels[0].Key != ConstantValue.Null)) ? 1 : 0);
		if (num2 <= num)
		{
			ImmutableArray<SwitchBucket> switchBuckets = GenerateSwitchBuckets(num2, num);
			EmitSwitchBuckets(switchBuckets, 0, switchBuckets.Length - 1);
		}
		else
		{
			_builder.EmitBranch(ILOpCode.Br, _fallThroughLabel);
		}
	}

	private static int CompareIntegralSwitchLabels(KeyValuePair<ConstantValue, object> first, KeyValuePair<ConstantValue, object> second)
	{
		ConstantValue key = first.Key;
		ConstantValue key2 = second.Key;
		return SwitchConstantValueHelper.CompareSwitchCaseLabelConstants(key, key2);
	}

	private ImmutableArray<SwitchBucket> GenerateSwitchBuckets(int startLabelIndex, int endLabelIndex)
	{
		ArrayBuilder<SwitchBucket> instance = ArrayBuilder<SwitchBucket>.GetInstance();
		for (int i = startLabelIndex; i <= endLabelIndex; i++)
		{
			SwitchBucket e = CreateNextBucket(i, endLabelIndex);
			while (!instance.IsEmpty())
			{
				SwitchBucket prevBucket = instance.Peek();
				if (!e.TryMergeWith(prevBucket))
				{
					break;
				}
				instance.Pop();
			}
			instance.Push(e);
		}
		ArrayBuilder<SwitchBucket> instance2 = ArrayBuilder<SwitchBucket>.GetInstance();
		foreach (SwitchBucket item in instance)
		{
			int degenerateBucketSplit = item.DegenerateBucketSplit;
			switch (degenerateBucketSplit)
			{
			case -1:
				instance2.Add(item);
				break;
			case 0:
				instance2.Add(new SwitchBucket(_sortedCaseLabels, item.StartLabelIndex, item.EndLabelIndex, isDegenerate: true));
				break;
			default:
				instance2.Add(new SwitchBucket(_sortedCaseLabels, item.StartLabelIndex, degenerateBucketSplit - 1, isDegenerate: true));
				instance2.Add(new SwitchBucket(_sortedCaseLabels, degenerateBucketSplit, item.EndLabelIndex, isDegenerate: true));
				break;
			}
		}
		instance.Free();
		return instance2.ToImmutableAndFree();
	}

	private SwitchBucket CreateNextBucket(int startLabelIndex, int endLabelIndex)
	{
		return new SwitchBucket(_sortedCaseLabels, startLabelIndex);
	}

	private void EmitSwitchBucketsLinearLeaf(ImmutableArray<SwitchBucket> switchBuckets, int low, int high)
	{
		for (int i = low; i < high; i++)
		{
			object obj = new object();
			EmitSwitchBucket(switchBuckets[i], obj);
			_builder.MarkLabel(obj);
		}
		EmitSwitchBucket(switchBuckets[high], _fallThroughLabel);
	}

	private void EmitSwitchBuckets(ImmutableArray<SwitchBucket> switchBuckets, int low, int high)
	{
		if (high - low < 3)
		{
			EmitSwitchBucketsLinearLeaf(switchBuckets, low, high);
			return;
		}
		int num = (low + high + 1) / 2;
		object obj = new object();
		ConstantValue endConstant = switchBuckets[num - 1].EndConstant;
		EmitCondBranchForSwitch(_keyTypeCode.IsUnsigned() ? ILOpCode.Bgt_un : ILOpCode.Bgt, endConstant, obj);
		EmitSwitchBuckets(switchBuckets, low, num - 1);
		_builder.MarkLabel(obj);
		EmitSwitchBuckets(switchBuckets, num, high);
	}

	private void EmitSwitchBucket(SwitchBucket switchBucket, object bucketFallThroughLabel)
	{
		if (switchBucket.LabelsCount == 1)
		{
			KeyValuePair<ConstantValue, object> keyValuePair = switchBucket[0];
			ConstantValue key = keyValuePair.Key;
			object value = keyValuePair.Value;
			EmitEqBranchForSwitch(key, value);
		}
		else if (switchBucket.IsDegenerate)
		{
			EmitRangeCheckedBranch(switchBucket.StartConstant, switchBucket.EndConstant, switchBucket[0].Value);
		}
		else
		{
			EmitNormalizedSwitchKey(switchBucket.StartConstant, switchBucket.EndConstant, bucketFallThroughLabel);
			object[] labels = CreateBucketLabels(switchBucket);
			_builder.EmitSwitch(labels);
		}
		_builder.EmitBranch(ILOpCode.Br, bucketFallThroughLabel);
	}

	private object[] CreateBucketLabels(SwitchBucket switchBucket)
	{
		ConstantValue startConstant = switchBucket.StartConstant;
		bool isNegativeNumeric = startConstant.IsNegativeNumeric;
		int num = 0;
		ulong num2 = 0uL;
		ulong bucketSize = switchBucket.BucketSize;
		object[] array = new object[bucketSize];
		for (ulong num3 = 0uL; num3 < bucketSize; num3++)
		{
			if (num3 == num2)
			{
				array[num3] = switchBucket[num].Value;
				num++;
				if (num >= switchBucket.LabelsCount)
				{
					break;
				}
				ConstantValue key = switchBucket[num].Key;
				num2 = ((!isNegativeNumeric) ? (key.UInt64Value - startConstant.UInt64Value) : ((ulong)(key.Int64Value - startConstant.Int64Value)));
			}
			else
			{
				array[num3] = _fallThroughLabel;
			}
		}
		return array;
	}

	private void EmitCondBranchForSwitch(ILOpCode branchCode, ConstantValue constant, object targetLabel)
	{
		_builder.EmitLoad(_key);
		_builder.EmitConstantValue(constant);
		_builder.EmitBranch(branchCode, targetLabel, GetReverseBranchCode(branchCode));
	}

	private void EmitEqBranchForSwitch(ConstantValue constant, object targetLabel)
	{
		_builder.EmitLoad(_key);
		if (constant.IsDefaultValue)
		{
			_builder.EmitBranch(ILOpCode.Brfalse, targetLabel);
			return;
		}
		_builder.EmitConstantValue(constant);
		_builder.EmitBranch(ILOpCode.Beq, targetLabel);
	}

	private void EmitRangeCheckedBranch(ConstantValue startConstant, ConstantValue endConstant, object targetLabel)
	{
		_builder.EmitLoad(_key);
		if (!startConstant.IsDefaultValue)
		{
			_builder.EmitConstantValue(startConstant);
			_builder.EmitOpCode(ILOpCode.Sub);
		}
		if (_keyTypeCode.Is64BitIntegral())
		{
			_builder.EmitLongConstant(endConstant.Int64Value - startConstant.Int64Value);
		}
		else
		{
			_builder.EmitIntConstant(Int32Value(endConstant) - Int32Value(startConstant));
		}
		_builder.EmitBranch(ILOpCode.Ble_un, targetLabel, ILOpCode.Bgt_un);
		static int Int32Value(ConstantValue value)
		{
			return value.Discriminator switch
			{
				ConstantValueTypeDiscriminator.Byte => value.ByteValue, 
				ConstantValueTypeDiscriminator.UInt16 => value.UInt16Value, 
				_ => value.Int32Value, 
			};
		}
	}

	private static ILOpCode GetReverseBranchCode(ILOpCode branchCode)
	{
		return branchCode switch
		{
			ILOpCode.Beq => ILOpCode.Bne_un, 
			ILOpCode.Blt => ILOpCode.Bge, 
			ILOpCode.Blt_un => ILOpCode.Bge_un, 
			ILOpCode.Bgt => ILOpCode.Ble, 
			ILOpCode.Bgt_un => ILOpCode.Ble_un, 
			_ => throw ExceptionUtilities.UnexpectedValue(branchCode), 
		};
	}

	private void EmitNormalizedSwitchKey(ConstantValue startConstant, ConstantValue endConstant, object bucketFallThroughLabel)
	{
		_builder.EmitLoad(_key);
		if (!startConstant.IsDefaultValue)
		{
			_builder.EmitConstantValue(startConstant);
			_builder.EmitOpCode(ILOpCode.Sub);
		}
		EmitRangeCheckIfNeeded(startConstant, endConstant, bucketFallThroughLabel);
		_builder.EmitNumericConversion(_keyTypeCode, Microsoft.Cci.PrimitiveTypeCode.UInt32, @checked: false);
	}

	private void EmitRangeCheckIfNeeded(ConstantValue startConstant, ConstantValue endConstant, object bucketFallThroughLabel)
	{
		if (_keyTypeCode.Is64BitIntegral())
		{
			object label = new object();
			_builder.EmitOpCode(ILOpCode.Dup);
			_builder.EmitLongConstant(endConstant.Int64Value - startConstant.Int64Value);
			_builder.EmitBranch(ILOpCode.Ble_un, label, ILOpCode.Bgt_un);
			_builder.EmitOpCode(ILOpCode.Pop);
			_builder.EmitBranch(ILOpCode.Br, bucketFallThroughLabel);
			_builder.AdjustStack(1);
			_builder.MarkLabel(label);
		}
	}
}
