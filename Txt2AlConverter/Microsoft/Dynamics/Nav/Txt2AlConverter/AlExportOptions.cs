using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.DotNet;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public class AlExportOptions
{
	private int nextPageExtensionObjectId;

	private int nextTableExtensionObjectId;

	private readonly ConcurrentDictionary<string, int> objectExtensions = new ConcurrentDictionary<string, int>();

	public static AlExportOptions Default { get; } = new AlExportOptions();

	public int Indentation { get; }

	public bool ResolveTypeNames { get; }

	public bool AddLegacyTranslationInformation { get; }

	public bool DataClassificationDefaulting { get; }

	public TranslationFileFormat TranslationFileFormat { get; }

	public bool TableDataOnly { get; }

	public Version TargetRuntimeVersion { get; }

	public IDotNetAddInsInjector DotNetAddInsInjector { get; }

	public AlExportOptions(int nextExtensionObjectId = 70000000, int indentationInSpaces = 4, bool resolveTypeNames = false, IDotNetAddInsInjector dotNetAddInsInjector = null, bool addLegacyTranslationInformation = false, TranslationFileFormat translationFileFormat = TranslationFileFormat.Xliff, Version targetRuntimeVersion = null, bool dataClassificationDefauling = false, bool tableDataOnly = false)
	{
		nextPageExtensionObjectId = nextExtensionObjectId;
		nextTableExtensionObjectId = nextExtensionObjectId;
		Indentation = indentationInSpaces;
		ResolveTypeNames = resolveTypeNames;
		DotNetAddInsInjector = dotNetAddInsInjector ?? NullDotNetAddInsInjector.Instance;
		AddLegacyTranslationInformation = addLegacyTranslationInformation;
		TranslationFileFormat = translationFileFormat;
		TargetRuntimeVersion = targetRuntimeVersion ?? RuntimeVersion.Latest;
		DataClassificationDefaulting = dataClassificationDefauling;
		TableDataOnly = tableDataOnly;
	}

	public int ReserveExtensionObjectId(ElementType elementType, IElement element)
	{
		switch (elementType)
		{
		case ElementType.Page:
		{
			int num2 = Interlocked.Increment(ref nextPageExtensionObjectId);
			objectExtensions.TryAdd(element.Name, num2);
			return num2;
		}
		case ElementType.Table:
		{
			int num = Interlocked.Increment(ref nextTableExtensionObjectId);
			objectExtensions.TryAdd(element.Name, num);
			return num;
		}
		default:
			throw new Exception($"Unexpected element type '{elementType}' for which to reserve an id.");
		}
	}

	public int GetReservedExtensionObjectId(IElement element)
	{
		ElementType elementType = element.ElementTypeInfo.ElementType;
		if (elementType == ElementType.Modification)
		{
			objectExtensions.TryGetValue(element.Name, out var value);
			return value;
		}
		throw new Exception($"Unexpected element type '{elementType}' for which to get a reserved id.");
	}
}
