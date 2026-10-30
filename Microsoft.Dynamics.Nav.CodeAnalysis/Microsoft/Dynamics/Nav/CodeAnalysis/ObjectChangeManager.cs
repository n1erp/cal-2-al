using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class ObjectChangeManager : IObjectChangeManager
{
	private readonly ConcurrentDictionary<ObjectChangeElementWrapper, ObjectChangeState> objectChangeMap = new ConcurrentDictionary<ObjectChangeElementWrapper, ObjectChangeState>();

	private ImmutableArray<ObjectChangeContext> changes = ImmutableArray<ObjectChangeContext>.Empty;

	public IReadOnlyCollection<ObjectChangeContext> Changes
	{
		get
		{
			if (objectChangeMap.Count == 0)
			{
				return ImmutableArray<ObjectChangeContext>.Empty;
			}
			if (changes == ImmutableArray<ObjectChangeContext>.Empty)
			{
				changes = objectChangeMap.Select((KeyValuePair<ObjectChangeElementWrapper, ObjectChangeState> kv) => new ObjectChangeContext(kv.Key, kv.Value)).ToImmutableArray();
			}
			return changes;
		}
	}

	public void Update(IObjectTypeSymbol symbol, string declaringObjectPath, ObjectChangeState state)
	{
		if (symbol.Kind.IsRadObjectTypeSymbol())
		{
			ObjectChangeElementWrapper objectChangeElementWrapper = CreateObjectChangeElementWrapper(symbol, declaringObjectPath);
			if (objectChangeElementWrapper.ObjectChangeElement != null)
			{
				Update(objectChangeElementWrapper, state);
			}
		}
	}

	public void Clear()
	{
		changes = ImmutableArray<ObjectChangeContext>.Empty;
		objectChangeMap.Clear();
	}

	public bool ContainsSymbol(IObjectTypeSymbol symbol)
	{
		int value = (symbol.Kind.IsApplicationObject() ? ((IApplicationObjectTypeSymbol)symbol).Id : 0);
		ObjectChangeElementWrapper key = new ObjectChangeElementWrapper(new ObjectChangeElement
		{
			Id = value,
			Name = symbol.Name,
			Kind = symbol.Kind
		});
		return objectChangeMap.ContainsKey(key);
	}

	public void Serialize(string path, IFileSystem fileSystem)
	{
		ObjectChangeModelDefinition value = CreateModelDefinition();
		if (!fileSystem.DirectoryExistsForFile(path))
		{
			fileSystem.CreateDirectoryForFile(path);
		}
		using Stream stream = fileSystem.CreateFile(path);
		using StreamWriter textWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
		JsonSerializer.Create(SymbolReferenceJsonWriter.SerializerSettings).Serialize(textWriter, value);
	}

	internal void Load(ObjectChangeModelDefinition objectChangeModelDefinition)
	{
		if (objectChangeModelDefinition != null)
		{
			Clear();
			Update(objectChangeModelDefinition.Added, ObjectChangeState.Added);
			Update(objectChangeModelDefinition.Modified, ObjectChangeState.Modified);
			Update(objectChangeModelDefinition.Removed, ObjectChangeState.Removed);
		}
	}

	public void Load(string path, IFileSystem fileSystem)
	{
		ObjectChangeModelDefinition objectChangeModelDefinition = LoadModel(path, fileSystem);
		Load(objectChangeModelDefinition);
	}

	private static ObjectChangeModelDefinition LoadModel(string path, IFileSystem fileSystem)
	{
		try
		{
			using Stream stream = fileSystem.OpenRead(path);
			using StreamReader reader = new StreamReader(stream);
			return JsonSerializer.Create(SymbolReferenceJsonWriter.SerializerSettings).Deserialize(reader, typeof(ObjectChangeModelDefinition)) as ObjectChangeModelDefinition;
		}
		catch (Exception ex)
		{
			DebugAssertHelper.Fail(string.Format(CultureInfo.InvariantCulture, "Failed loading the '{0}' rad file. Reason:{1}", path, ex.Message));
		}
		return null;
	}

	private static ObjectChangeElementWrapper CreateObjectChangeElementWrapper(IObjectTypeSymbol symbol, string declaringObjectPath)
	{
		ObjectChangeElement objectChangeElement = new ObjectChangeElement
		{
			Name = symbol.Name,
			Kind = symbol.Kind,
			PackagePath = declaringObjectPath
		};
		if (symbol.Kind.IsApplicationObject())
		{
			objectChangeElement.Id = ((IApplicationObjectTypeSymbol)symbol).Id;
		}
		return new ObjectChangeElementWrapper(objectChangeElement);
	}

	private void Update(IEnumerable<ObjectChangeElement> objectChangeElements, ObjectChangeState state)
	{
		foreach (ObjectChangeElement objectChangeElement in objectChangeElements)
		{
			ObjectChangeElementWrapper key = new ObjectChangeElementWrapper(objectChangeElement);
			Update(key, state);
		}
	}

	private void Update(ObjectChangeElementWrapper key, ObjectChangeState state)
	{
		changes = ImmutableArray<ObjectChangeContext>.Empty;
		if (objectChangeMap.TryGetValue(key, out var value))
		{
			switch (value)
			{
			case ObjectChangeState.Added:
				HandleAddedState(key, state);
				break;
			case ObjectChangeState.Modified:
				HandleModifiedState(key, state);
				break;
			case ObjectChangeState.Removed:
				HandleRemovedState(key, state);
				break;
			}
			return;
		}
		if (state == ObjectChangeState.Removed || state == ObjectChangeState.Added)
		{
			foreach (ObjectChangeElementWrapper key2 in objectChangeMap.Keys)
			{
				if (key2.ObjectChangeElement.Kind == key.ObjectChangeElement.Kind && SemanticFacts.IsSameName(key2.ObjectChangeElement.Name, key.ObjectChangeElement.Name) && objectChangeMap[key2] == state)
				{
					switch (state)
					{
					case ObjectChangeState.Removed:
						return;
					case ObjectChangeState.Added:
					{
						objectChangeMap.TryRemove(key2, out var _);
						break;
					}
					}
				}
			}
		}
		objectChangeMap.TryAdd(key, state);
	}

	private void HandleRemovedState(ObjectChangeElementWrapper key, ObjectChangeState state)
	{
		if (state == ObjectChangeState.Added)
		{
			UpdateOrAddNewKey(key, ObjectChangeState.Modified);
		}
		else
		{
			UpdateOrAddNewKey(key, state);
		}
	}

	private void HandleModifiedState(ObjectChangeElementWrapper key, ObjectChangeState state)
	{
		if (state != ObjectChangeState.Added)
		{
			UpdateOrAddNewKey(key, state);
		}
	}

	private void HandleAddedState(ObjectChangeElementWrapper key, ObjectChangeState state)
	{
		if (state == ObjectChangeState.Removed)
		{
			objectChangeMap.TryRemove(key, out var _);
		}
		else
		{
			UpdateOrAddNewKey(key, ObjectChangeState.Added);
		}
	}

	private void UpdateOrAddNewKey(ObjectChangeElementWrapper newKey, ObjectChangeState state)
	{
		foreach (ObjectChangeElementWrapper key in objectChangeMap.Keys)
		{
			if (key == newKey && !SemanticFacts.NameEqualityComparer.Equals(key.ObjectChangeElement.Name, newKey.ObjectChangeElement.Name))
			{
				objectChangeMap.TryRemove(key, out var _);
				objectChangeMap.TryAdd(newKey, state);
				return;
			}
		}
		objectChangeMap[newKey] = state;
	}

	private ObjectChangeModelDefinition CreateModelDefinition(byte[] serverThumbPrint = null)
	{
		PooledList<ObjectChangeElement> instance = PooledList<ObjectChangeElement>.GetInstance();
		PooledList<ObjectChangeElement> instance2 = PooledList<ObjectChangeElement>.GetInstance();
		PooledList<ObjectChangeElement> instance3 = PooledList<ObjectChangeElement>.GetInstance();
		try
		{
			foreach (KeyValuePair<ObjectChangeElementWrapper, ObjectChangeState> item in objectChangeMap)
			{
				ObjectChangeState value = item.Value;
				ObjectChangeElement objectChangeElement = item.Key?.ObjectChangeElement;
				if (objectChangeElement != null)
				{
					switch (value)
					{
					case ObjectChangeState.Added:
						instance.Add(objectChangeElement);
						break;
					case ObjectChangeState.Modified:
						instance3.Add(objectChangeElement);
						break;
					case ObjectChangeState.Removed:
						instance2.Add(objectChangeElement);
						break;
					}
				}
			}
			return new ObjectChangeModelDefinition
			{
				Added = instance.ToArray(),
				Removed = instance2.ToArray(),
				Modified = instance3.ToArray()
			};
		}
		finally
		{
			instance.Free();
			instance2.Free();
			instance3.Free();
		}
	}
}
