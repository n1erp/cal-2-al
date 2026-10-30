using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class DefinitionMap
{
	protected struct MappedMethod(IMethodSymbolInternal previousMethodOpt, Func<SyntaxNode, SyntaxNode> syntaxMap)
	{
		public readonly IMethodSymbolInternal PreviousMethod = previousMethodOpt;

		public readonly Func<SyntaxNode, SyntaxNode> SyntaxMap = syntaxMap;
	}

	protected readonly PEModule module;

	protected readonly IReadOnlyDictionary<IMethodSymbol, MappedMethod> mappedMethods;

	internal abstract CommonMessageProvider MessageProvider { get; }

	protected DefinitionMap(PEModule module, IEnumerable<SemanticEdit> edits)
	{
		this.module = module;
		mappedMethods = GetMappedMethods(edits);
	}

	private static IReadOnlyDictionary<IMethodSymbol, MappedMethod> GetMappedMethods(IEnumerable<SemanticEdit> edits)
	{
		Dictionary<IMethodSymbol, MappedMethod> dictionary = new Dictionary<IMethodSymbol, MappedMethod>();
		foreach (SemanticEdit edit in edits)
		{
			if (edit.Kind == SemanticEditKind.Update && edit.PreserveLocalVariables && edit.NewSymbol is IMethodSymbol key)
			{
				dictionary.Add(key, new MappedMethod((IMethodSymbolInternal)edit.OldSymbol, edit.SyntaxMap));
			}
		}
		return dictionary;
	}

	internal abstract IDefinition MapDefinition(IDefinition definition);

	internal bool DefinitionExists(IDefinition definition)
	{
		return MapDefinition(definition) != null;
	}

	internal abstract bool TryGetTypeHandle(ITypeDefinition def, out TypeDefinitionHandle handle);

	internal abstract bool TryGetEventHandle(IEventDefinition def, out EventDefinitionHandle handle);

	internal abstract bool TryGetFieldHandle(IFieldDefinition def, out FieldDefinitionHandle handle);

	internal abstract bool TryGetMethodHandle(IMethodDefinition def, out MethodDefinitionHandle handle);

	internal abstract bool TryGetPropertyHandle(IPropertyDefinition def, out PropertyDefinitionHandle handle);
}
internal abstract class DefinitionMap<TSymbolMatcher> : DefinitionMap where TSymbolMatcher : SymbolMatcher
{
	protected readonly TSymbolMatcher mapToMetadata;

	protected readonly TSymbolMatcher mapToPrevious;

	protected DefinitionMap(PEModule module, IEnumerable<SemanticEdit> edits, TSymbolMatcher mapToMetadata, TSymbolMatcher mapToPrevious)
		: base(module, edits)
	{
		this.mapToMetadata = mapToMetadata;
		this.mapToPrevious = mapToPrevious ?? mapToMetadata;
	}

	internal sealed override IDefinition MapDefinition(IDefinition definition)
	{
		IDefinition definition2 = mapToPrevious.MapDefinition(definition);
		if (definition2 == null)
		{
			if (mapToMetadata == mapToPrevious)
			{
				return null;
			}
			definition2 = mapToMetadata.MapDefinition(definition);
		}
		return definition2;
	}

	private bool TryGetMethodHandle(EmitBaseline baseline, IMethodDefinition def, out MethodDefinitionHandle handle)
	{
		if (TryGetMethodHandle(def, out handle))
		{
			return true;
		}
		def = (IMethodDefinition)mapToPrevious.MapDefinition(def);
		if (def != null && baseline.MethodsAdded.TryGetValue(def, out var value))
		{
			handle = MetadataTokens.MethodDefinitionHandle(value);
			return true;
		}
		handle = default;
		return false;
	}

	protected static IReadOnlyDictionary<SyntaxNode, int> CreateDeclaratorToSyntaxOrdinalMap(ImmutableArray<SyntaxNode> declarators)
	{
		Dictionary<SyntaxNode, int> dictionary = new Dictionary<SyntaxNode, int>();
		for (int i = 0; i < declarators.Length; i++)
		{
			dictionary.Add(declarators[i], i);
		}
		return dictionary;
	}

	protected abstract void GetStateMachineFieldMapFromMetadata(ITypeSymbol stateMachineType, ImmutableArray<LocalSlotDebugInfo> localSlotDebugInfo, out IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap, out IReadOnlyDictionary<ITypeReference, int> awaiterMap, out int awaiterSlotCount);

	protected abstract ImmutableArray<EncLocalInfo> GetLocalSlotMapFromMetadata(StandaloneSignatureHandle handle, EditAndContinueMethodDebugInformation debugInfo);

	protected abstract ITypeSymbol TryGetStateMachineType(EntityHandle methodHandle);

	internal VariableSlotAllocator TryCreateVariableSlotAllocator(EmitBaseline baseline, Compilation compilation, IMethodSymbolInternal method, IMethodSymbol topLevelMethod, DiagnosticBag diagnostics)
	{
		if (!mappedMethods.TryGetValue(topLevelMethod, out var value))
		{
			return null;
		}
		if (!TryGetMethodHandle(baseline, (IMethodDefinition)method, out var handle))
		{
			return null;
		}
		IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap = null;
		IReadOnlyDictionary<ITypeReference, int> awaiterMap = null;
		IReadOnlyDictionary<int, KeyValuePair<DebugId, int>> lambdaMap = null;
		IReadOnlyDictionary<int, DebugId> closureMap = null;
		int hoistedLocalSlotCount = 0;
		int awaiterSlotCount = 0;
		string stateMachineTypeNameOpt = null;
		int rowNumber = MetadataTokens.GetRowNumber(handle);
		DebugId methodId;
		ImmutableArray<EncLocalInfo> previousLocals;
		TSymbolMatcher symbolMap;
		if (baseline.AddedOrChangedMethods.TryGetValue(rowNumber, out var value2))
		{
			methodId = value2.MethodId;
			MakeLambdaAndClosureMaps(value2.LambdaDebugInfo, value2.ClosureDebugInfo, out lambdaMap, out closureMap);
			if (value2.StateMachineTypeNameOpt != null)
			{
				GetStateMachineFieldMapFromPreviousCompilation(value2.StateMachineHoistedLocalSlotsOpt, value2.StateMachineAwaiterSlotsOpt, out hoistedLocalMap, out awaiterMap);
				hoistedLocalSlotCount = value2.StateMachineHoistedLocalSlotsOpt.Length;
				awaiterSlotCount = value2.StateMachineAwaiterSlotsOpt.Length;
				previousLocals = ImmutableArray<EncLocalInfo>.Empty;
				stateMachineTypeNameOpt = value2.StateMachineTypeNameOpt;
			}
			else
			{
				previousLocals = value2.Locals;
			}
			symbolMap = mapToPrevious;
		}
		else
		{
			EditAndContinueMethodDebugInformation debugInfo;
			StandaloneSignatureHandle standaloneSignatureHandle;
			try
			{
				debugInfo = baseline.DebugInformationProvider(handle);
				standaloneSignatureHandle = baseline.LocalSignatureProvider(handle);
			}
			catch (Exception ex) when (ex is InvalidDataException || ex is IOException)
			{
				diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_InvalidDebugInfo, method.Locations.First(), method, MetadataTokens.GetToken(handle), method.ContainingAssembly));
				return null;
			}
			methodId = new DebugId(debugInfo.MethodOrdinal, 0);
			if (!debugInfo.Lambdas.IsDefaultOrEmpty)
			{
				MakeLambdaAndClosureMaps(debugInfo.Lambdas, debugInfo.Closures, out lambdaMap, out closureMap);
			}
			ITypeSymbol typeSymbol = TryGetStateMachineType(handle);
			if (typeSymbol != null)
			{
				ImmutableArray<LocalSlotDebugInfo> localSlotDebugInfo = debugInfo.LocalSlots.NullToEmpty();
				GetStateMachineFieldMapFromMetadata(typeSymbol, localSlotDebugInfo, out hoistedLocalMap, out awaiterMap, out awaiterSlotCount);
				hoistedLocalSlotCount = localSlotDebugInfo.Length;
				previousLocals = ImmutableArray<EncLocalInfo>.Empty;
				stateMachineTypeNameOpt = typeSymbol.Name;
			}
			else
			{
				if (method.IsAsync)
				{
					if (compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_AsyncStateMachineAttribute__ctor) == null)
					{
						ReportMissingStateMachineAttribute(diagnostics, method, AttributeDescription.AsyncStateMachineAttribute.FullName);
						return null;
					}
				}
				else if (method.IsIterator && compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Runtime_CompilerServices_IteratorStateMachineAttribute__ctor) == null)
				{
					ReportMissingStateMachineAttribute(diagnostics, method, AttributeDescription.IteratorStateMachineAttribute.FullName);
					return null;
				}
				try
				{
					previousLocals = (standaloneSignatureHandle.IsNil ? ImmutableArray<EncLocalInfo>.Empty : GetLocalSlotMapFromMetadata(standaloneSignatureHandle, debugInfo));
				}
				catch (Exception ex2) when (ex2 is UnsupportedSignatureContent || ex2 is BadImageFormatException || ex2 is IOException)
				{
					diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_InvalidDebugInfo, method.Locations.First(), method, MetadataTokens.GetToken(standaloneSignatureHandle), method.ContainingAssembly));
					return null;
				}
			}
			symbolMap = mapToMetadata;
		}
		return new EncVariableSlotAllocator(symbolMap, value.SyntaxMap, value.PreviousMethod, methodId, previousLocals, lambdaMap, closureMap, stateMachineTypeNameOpt, hoistedLocalSlotCount, hoistedLocalMap, awaiterSlotCount, awaiterMap, GetLambdaSyntaxFacts());
	}

	protected abstract LambdaSyntaxFacts GetLambdaSyntaxFacts();

	private void ReportMissingStateMachineAttribute(DiagnosticBag diagnostics, IMethodSymbolInternal method, string stateMachineAttributeFullName)
	{
		diagnostics.Add(MessageProvider.CreateDiagnostic(MessageProvider.ERR_EncUpdateFailedMissingAttribute, method.Locations.First(), MessageProvider.GetErrorDisplayString(method), stateMachineAttributeFullName));
	}

	private static void MakeLambdaAndClosureMaps(ImmutableArray<LambdaDebugInfo> lambdaDebugInfo, ImmutableArray<ClosureDebugInfo> closureDebugInfo, out IReadOnlyDictionary<int, KeyValuePair<DebugId, int>> lambdaMap, out IReadOnlyDictionary<int, DebugId> closureMap)
	{
		Dictionary<int, KeyValuePair<DebugId, int>> dictionary = new Dictionary<int, KeyValuePair<DebugId, int>>(lambdaDebugInfo.Length);
		Dictionary<int, DebugId> dictionary2 = new Dictionary<int, DebugId>(closureDebugInfo.Length);
		for (int i = 0; i < lambdaDebugInfo.Length; i++)
		{
			LambdaDebugInfo lambdaDebugInfo2 = lambdaDebugInfo[i];
			dictionary[lambdaDebugInfo2.SyntaxOffset] = KeyValuePairUtil.Create(lambdaDebugInfo2.LambdaId, lambdaDebugInfo2.ClosureOrdinal);
		}
		for (int j = 0; j < closureDebugInfo.Length; j++)
		{
			ClosureDebugInfo closureDebugInfo2 = closureDebugInfo[j];
			dictionary2[closureDebugInfo2.SyntaxOffset] = closureDebugInfo2.ClosureId;
		}
		lambdaMap = dictionary;
		closureMap = dictionary2;
	}

	private static void GetStateMachineFieldMapFromPreviousCompilation(ImmutableArray<EncHoistedLocalInfo> hoistedLocalSlots, ImmutableArray<ITypeReference> hoistedAwaiters, out IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalMap, out IReadOnlyDictionary<ITypeReference, int> awaiterMap)
	{
		Dictionary<EncHoistedLocalInfo, int> dictionary = new Dictionary<EncHoistedLocalInfo, int>();
		Dictionary<ITypeReference, int> dictionary2 = new Dictionary<ITypeReference, int>();
		for (int i = 0; i < hoistedLocalSlots.Length; i++)
		{
			EncHoistedLocalInfo key = hoistedLocalSlots[i];
			if (!key.IsUnused)
			{
				dictionary.Add(key, i);
			}
		}
		for (int j = 0; j < hoistedAwaiters.Length; j++)
		{
			ITypeReference typeReference = hoistedAwaiters[j];
			if (typeReference != null)
			{
				dictionary2.Add(typeReference, j);
			}
		}
		hoistedLocalMap = dictionary;
		awaiterMap = dictionary2;
	}
}
