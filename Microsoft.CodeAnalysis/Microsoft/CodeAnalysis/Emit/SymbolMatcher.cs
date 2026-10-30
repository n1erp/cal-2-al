using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Emit;

internal abstract class SymbolMatcher
{
	public abstract ITypeReference MapReference(ITypeReference reference);

	public abstract IDefinition MapDefinition(IDefinition reference);

	public EmitBaseline MapBaselineToCompilation(EmitBaseline baseline, Compilation targetCompilation, CommonPEModuleBuilder targetModuleBuilder, ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> mappedSynthesizedMembers)
	{
		IReadOnlyDictionary<ITypeDefinition, int> typesAdded = MapDefinitions(baseline.TypesAdded);
		IReadOnlyDictionary<IEventDefinition, int> eventsAdded = MapDefinitions(baseline.EventsAdded);
		IReadOnlyDictionary<IFieldDefinition, int> fieldsAdded = MapDefinitions(baseline.FieldsAdded);
		IReadOnlyDictionary<IMethodDefinition, int> methodsAdded = MapDefinitions(baseline.MethodsAdded);
		IReadOnlyDictionary<IPropertyDefinition, int> propertiesAdded = MapDefinitions(baseline.PropertiesAdded);
		return baseline.With(targetCompilation, targetModuleBuilder, baseline.Ordinal, baseline.EncId, typesAdded, eventsAdded, fieldsAdded, methodsAdded, propertiesAdded, baseline.EventMapAdded, baseline.PropertyMapAdded, baseline.MethodImplsAdded, baseline.TableEntriesAdded, baseline.BlobStreamLengthAdded, baseline.StringStreamLengthAdded, baseline.UserStringStreamLengthAdded, baseline.GuidStreamLengthAdded, MapAnonymousTypes(baseline.AnonymousTypeMap), mappedSynthesizedMembers, MapAddedOrChangedMethods(baseline.AddedOrChangedMethods), baseline.DebugInformationProvider, baseline.LocalSignatureProvider);
	}

	private IReadOnlyDictionary<K, V> MapDefinitions<K, V>(IReadOnlyDictionary<K, V> items) where K : IDefinition
	{
		Dictionary<K, V> dictionary = new Dictionary<K, V>();
		foreach (KeyValuePair<K, V> item in items)
		{
			K val = (K)MapDefinition(item.Key);
			if (val != null)
			{
				dictionary.Add(val, item.Value);
			}
		}
		return dictionary;
	}

	private IReadOnlyDictionary<int, AddedOrChangedMethodInfo> MapAddedOrChangedMethods(IReadOnlyDictionary<int, AddedOrChangedMethodInfo> addedOrChangedMethods)
	{
		Dictionary<int, AddedOrChangedMethodInfo> dictionary = new Dictionary<int, AddedOrChangedMethodInfo>();
		foreach (KeyValuePair<int, AddedOrChangedMethodInfo> addedOrChangedMethod in addedOrChangedMethods)
		{
			dictionary.Add(addedOrChangedMethod.Key, addedOrChangedMethod.Value.MapTypes(this));
		}
		return dictionary;
	}

	private IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> MapAnonymousTypes(IReadOnlyDictionary<AnonymousTypeKey, AnonymousTypeValue> anonymousTypeMap)
	{
		Dictionary<AnonymousTypeKey, AnonymousTypeValue> dictionary = new Dictionary<AnonymousTypeKey, AnonymousTypeValue>();
		foreach (KeyValuePair<AnonymousTypeKey, AnonymousTypeValue> item in anonymousTypeMap)
		{
			AnonymousTypeKey key = item.Key;
			AnonymousTypeValue value = item.Value;
			ITypeDefinition type = (ITypeDefinition)MapDefinition(value.Type);
			dictionary.Add(key, new AnonymousTypeValue(value.Name, value.UniqueIndex, type));
		}
		return dictionary;
	}

	internal ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> MapSynthesizedMembers(ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> previousMembers, ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> newMembers)
	{
		if (previousMembers.Count == 0)
		{
			return newMembers;
		}
		ImmutableDictionary<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>>.Builder builder = ImmutableDictionary.CreateBuilder<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>>();
		builder.AddRange(newMembers);
		foreach (KeyValuePair<ITypeDefinition, ImmutableArray<ITypeDefinitionMember>> previousMember in previousMembers)
		{
			ITypeDefinition key = previousMember.Key;
			ImmutableArray<ITypeDefinitionMember> value = previousMember.Value;
			ITypeDefinition typeDefinition = (ITypeDefinition)MapDefinition(key);
			if (typeDefinition == null)
			{
				builder.Add(key, value);
				continue;
			}
			if (!newMembers.TryGetValue(typeDefinition, out var value2))
			{
				builder.Add(typeDefinition, value);
				continue;
			}
			ArrayBuilder<ITypeDefinitionMember> instance = ArrayBuilder<ITypeDefinitionMember>.GetInstance();
			instance.AddRange(value2);
			foreach (ITypeDefinitionMember item in value)
			{
				if ((ITypeDefinitionMember)MapDefinition(item) == null)
				{
					instance.Add(item);
				}
			}
			builder[typeDefinition] = instance.ToImmutableAndFree();
		}
		return builder.ToImmutable();
	}
}
