using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class CSharpDefinitionMap : DefinitionMap<CSharpSymbolMatcher>
{
	private readonly MetadataDecoder _metadataDecoder;

	internal override CommonMessageProvider MessageProvider => Microsoft.CodeAnalysis.CSharp.MessageProvider.Instance;

	public CSharpDefinitionMap(PEModule module, IEnumerable<SemanticEdit> edits, MetadataDecoder metadataDecoder, CSharpSymbolMatcher mapToMetadata, CSharpSymbolMatcher mapToPrevious)
		: base(module, edits, mapToMetadata, mapToPrevious)
	{
		_metadataDecoder = metadataDecoder;
	}

	protected override LambdaSyntaxFacts GetLambdaSyntaxFacts()
	{
		return CSharpLambdaSyntaxFacts.Instance;
	}

	internal bool TryGetAnonymousTypeName(NamedTypeSymbol template, out string name, out int index)
	{
		return mapToPrevious.TryGetAnonymousTypeName(template, out name, out index);
	}

	internal override bool TryGetTypeHandle(ITypeDefinition def, out TypeDefinitionHandle handle)
	{
		if (mapToMetadata.MapDefinition(def) is PENamedTypeSymbol pENamedTypeSymbol)
		{
			handle = pENamedTypeSymbol.Handle;
			return true;
		}
		handle = default;
		return false;
	}

	internal override bool TryGetEventHandle(IEventDefinition def, out EventDefinitionHandle handle)
	{
		if (mapToMetadata.MapDefinition(def) is PEEventSymbol pEEventSymbol)
		{
			handle = pEEventSymbol.Handle;
			return true;
		}
		handle = default;
		return false;
	}

	internal override bool TryGetFieldHandle(IFieldDefinition def, out FieldDefinitionHandle handle)
	{
		if (mapToMetadata.MapDefinition(def) is PEFieldSymbol pEFieldSymbol)
		{
			handle = pEFieldSymbol.Handle;
			return true;
		}
		handle = default;
		return false;
	}

	internal override bool TryGetMethodHandle(IMethodDefinition def, out MethodDefinitionHandle handle)
	{
		if (mapToMetadata.MapDefinition(def) is PEMethodSymbol pEMethodSymbol)
		{
			handle = pEMethodSymbol.Handle;
			return true;
		}
		handle = default;
		return false;
	}

	internal override bool TryGetPropertyHandle(IPropertyDefinition def, out PropertyDefinitionHandle handle)
	{
		if (mapToMetadata.MapDefinition(def) is PEPropertySymbol pEPropertySymbol)
		{
			handle = pEPropertySymbol.Handle;
			return true;
		}
		handle = default;
		return false;
	}

	protected override void GetStateMachineFieldMapFromMetadata(ITypeSymbol stateMachineType, ImmutableArray<LocalSlotDebugInfo> localSlotDebugInfo, out IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap, out IReadOnlyDictionary<ITypeReference, int> awaiterMap, out int awaiterSlotCount)
	{
		Dictionary<EncHoistedLocalInfo, int> dictionary = new Dictionary<EncHoistedLocalInfo, int>();
		Dictionary<ITypeReference, int> dictionary2 = new Dictionary<ITypeReference, int>();
		int num = -1;
		foreach (ISymbol member in stateMachineType.GetMembers())
		{
			if (member.Kind != SymbolKind.Field)
			{
				continue;
			}
			string name = member.Name;
			int slotIndex;
			switch (GeneratedNames.GetKind(name))
			{
			case GeneratedNameKind.AwaiterField:
				if (GeneratedNames.TryParseSlotIndex(name, out slotIndex))
				{
					IFieldSymbol fieldSymbol2 = (IFieldSymbol)member;
					dictionary2[(ITypeReference)fieldSymbol2.Type] = slotIndex;
					if (slotIndex > num)
					{
						num = slotIndex;
					}
				}
				break;
			case GeneratedNameKind.HoistedLocalField:
			case GeneratedNameKind.HoistedSynthesizedLocalField:
				if (GeneratedNames.TryParseSlotIndex(name, out slotIndex))
				{
					IFieldSymbol fieldSymbol = (IFieldSymbol)member;
					if (slotIndex < localSlotDebugInfo.Length)
					{
						EncHoistedLocalInfo key = new EncHoistedLocalInfo(localSlotDebugInfo[slotIndex], (ITypeReference)fieldSymbol.Type);
						dictionary[key] = slotIndex;
					}
				}
				break;
			}
		}
		hoistedLocalMap = dictionary;
		awaiterMap = dictionary2;
		awaiterSlotCount = num + 1;
	}

	protected override ImmutableArray<EncLocalInfo> GetLocalSlotMapFromMetadata(StandaloneSignatureHandle handle, EditAndContinueMethodDebugInformation debugInfo)
	{
		ImmutableArray<LocalInfo<TypeSymbol>> localsOrThrow = _metadataDecoder.GetLocalsOrThrow(handle);
		return CreateLocalSlotMap(debugInfo, localsOrThrow);
	}

	protected override ITypeSymbol TryGetStateMachineType(EntityHandle methodHandle)
	{
		if (_metadataDecoder.Module.HasStringValuedAttribute(methodHandle, AttributeDescription.AsyncStateMachineAttribute, out var value) || _metadataDecoder.Module.HasStringValuedAttribute(methodHandle, AttributeDescription.IteratorStateMachineAttribute, out value))
		{
			return _metadataDecoder.GetTypeSymbolForSerializedType(value);
		}
		return null;
	}

	private static ImmutableArray<EncLocalInfo> CreateLocalSlotMap(EditAndContinueMethodDebugInformation methodEncInfo, ImmutableArray<LocalInfo<TypeSymbol>> slotMetadata)
	{
		EncLocalInfo[] array = new EncLocalInfo[slotMetadata.Length];
		ImmutableArray<LocalSlotDebugInfo> localSlots = methodEncInfo.LocalSlots;
		if (!localSlots.IsDefault)
		{
			int num = Math.Min(localSlots.Length, slotMetadata.Length);
			Dictionary<EncLocalInfo, int> dictionary = new Dictionary<EncLocalInfo, int>();
			for (int i = 0; i < num; i++)
			{
				LocalSlotDebugInfo slotInfo = localSlots[i];
				if (slotInfo.SynthesizedKind.IsLongLived())
				{
					LocalInfo<TypeSymbol> localInfo = slotMetadata[i];
					if (localInfo.CustomModifiers.IsDefaultOrEmpty)
					{
						EncLocalInfo key = new EncLocalInfo(slotInfo, (ITypeReference)localInfo.Type, localInfo.Constraints, localInfo.SignatureOpt);
						dictionary.Add(key, i);
					}
				}
			}
			foreach (KeyValuePair<EncLocalInfo, int> item in dictionary)
			{
				array[item.Value] = item.Key;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].IsDefault)
			{
				array[j] = new EncLocalInfo(slotMetadata[j].SignatureOpt);
			}
		}
		return ImmutableArray.Create(array);
	}
}
