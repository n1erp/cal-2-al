using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal struct SwitchStringJumpTableEmitter
{
	public delegate void EmitStringCompareAndBranch(LocalOrParameter key, ConstantValue stringConstant, object targetLabel);

	public delegate uint GetStringHashCode(string key);

	private readonly ILBuilder _builder;

	private readonly LocalOrParameter _key;

	private readonly KeyValuePair<ConstantValue, object>[] _caseLabels;

	private readonly object _fallThroughLabel;

	private readonly EmitStringCompareAndBranch _emitStringCondBranchDelegate;

	private readonly GetStringHashCode _computeStringHashcodeDelegate;

	private readonly LocalDefinition _keyHash;

	internal SwitchStringJumpTableEmitter(ILBuilder builder, LocalOrParameter key, KeyValuePair<ConstantValue, object>[] caseLabels, object fallThroughLabel, LocalDefinition keyHash, EmitStringCompareAndBranch emitStringCondBranchDelegate, GetStringHashCode computeStringHashcodeDelegate)
	{
		_builder = builder;
		_key = key;
		_caseLabels = caseLabels;
		_fallThroughLabel = fallThroughLabel;
		_keyHash = keyHash;
		_emitStringCondBranchDelegate = emitStringCondBranchDelegate;
		_computeStringHashcodeDelegate = computeStringHashcodeDelegate;
	}

	internal void EmitJumpTable()
	{
		if (_keyHash != null)
		{
			EmitHashTableSwitch();
		}
		else
		{
			EmitNonHashTableSwitch(_caseLabels);
		}
	}

	private void EmitHashTableSwitch()
	{
		Dictionary<uint, List<KeyValuePair<ConstantValue, object>>> dictionary = ComputeStringHashMap(_caseLabels, _computeStringHashcodeDelegate);
		Dictionary<uint, object> dictionary2 = EmitHashBucketJumpTable(dictionary);
		foreach (KeyValuePair<uint, List<KeyValuePair<ConstantValue, object>>> item in dictionary)
		{
			_builder.MarkLabel(dictionary2[item.Key]);
			List<KeyValuePair<ConstantValue, object>> value = item.Value;
			EmitNonHashTableSwitch(value.ToArray());
		}
	}

	private Dictionary<uint, object> EmitHashBucketJumpTable(Dictionary<uint, List<KeyValuePair<ConstantValue, object>>> stringHashMap)
	{
		int count = stringHashMap.Count;
		Dictionary<uint, object> dictionary = new Dictionary<uint, object>(count);
		KeyValuePair<ConstantValue, object>[] array = new KeyValuePair<ConstantValue, object>[count];
		int num = 0;
		foreach (uint key2 in stringHashMap.Keys)
		{
			ConstantValue key = ConstantValue.Create(key2);
			object value = new object();
			array[num] = new KeyValuePair<ConstantValue, object>(key, value);
			dictionary[key2] = value;
			num++;
		}
		new SwitchIntegralJumpTableEmitter(_builder, array, _fallThroughLabel, Microsoft.Cci.PrimitiveTypeCode.UInt32, _keyHash).EmitJumpTable();
		return dictionary;
	}

	private void EmitNonHashTableSwitch(KeyValuePair<ConstantValue, object>[] labels)
	{
		for (int i = 0; i < labels.Length; i++)
		{
			KeyValuePair<ConstantValue, object> keyValuePair = labels[i];
			EmitCondBranchForStringSwitch(keyValuePair.Key, keyValuePair.Value);
		}
		_builder.EmitBranch(ILOpCode.Br, _fallThroughLabel);
	}

	private void EmitCondBranchForStringSwitch(ConstantValue stringConstant, object targetLabel)
	{
		_emitStringCondBranchDelegate(_key, stringConstant, targetLabel);
	}

	private static Dictionary<uint, List<KeyValuePair<ConstantValue, object>>> ComputeStringHashMap(KeyValuePair<ConstantValue, object>[] caseLabels, GetStringHashCode computeStringHashcodeDelegate)
	{
		Dictionary<uint, List<KeyValuePair<ConstantValue, object>>> dictionary = new Dictionary<uint, List<KeyValuePair<ConstantValue, object>>>(caseLabels.Length);
		for (int i = 0; i < caseLabels.Length; i++)
		{
			KeyValuePair<ConstantValue, object> item = caseLabels[i];
			ConstantValue key = item.Key;
			uint key2 = computeStringHashcodeDelegate((string)key.Value);
			if (!dictionary.TryGetValue(key2, out var value))
			{
				value = new List<KeyValuePair<ConstantValue, object>>();
				dictionary.Add(key2, value);
			}
			value.Add(item);
		}
		return dictionary;
	}

	internal static bool ShouldGenerateHashTableSwitch(CommonPEModuleBuilder module, int labelsCount)
	{
		if (module.SupportsPrivateImplClass)
		{
			return ShouldGenerateHashTableSwitch(labelsCount);
		}
		return false;
	}

	private static bool ShouldGenerateHashTableSwitch(int labelsCount)
	{
		return labelsCount >= 7;
	}
}
