using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public static class SymbolReferenceJsonReader
{
	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public static ModuleDefinition ReadModule(Stream stream, IList<Diagnostic> diagnostics = null)
	{
		ModuleDefinition result = null;
		try
		{
			using StreamReader reader = new StreamReader(stream);
			result = JsonSerializer.Create(SymbolReferenceJsonWriter.SerializerSettings).Deserialize(reader, typeof(ModuleDefinition)) as ModuleDefinition;
			ApplyGlobalPostProcessingToModule(result, diagnostics);
		}
		catch (Exception ex)
		{
			diagnostics?.Add(Diagnostic.Create(MessageProvider.Instance, 1054, ex.Message));
		}
		return result;
	}

	private static ModuleDefinition ApplyGlobalPostProcessingToModule(ModuleDefinition result, IList<Diagnostic> diagnostics)
	{
		if (result == null)
		{
			return null;
		}
		result.EnumTypes = ExtractLegacyEnums(result, diagnostics);
		result.AddDotNetTypeDefinitions();
		return result;
	}

	private static EnumTypeDefinition[] ExtractLegacyEnums(ModuleDefinition moduleDef, IList<Diagnostic> diagnostics)
	{
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		PooledDictionary<int, EnumTypeDefinition> instance2 = PooledDictionary<int, EnumTypeDefinition>.GetInstance();
		ArrayBuilder<EnumTypeDefinition> instance3 = ArrayBuilder<EnumTypeDefinition>.GetInstance();
		try
		{
			if (moduleDef.Tables != null)
			{
				TableDefinition[] tables = moduleDef.Tables;
				foreach (TableDefinition tableDefinition in tables)
				{
					if (tableDefinition.DefinedEnums != null)
					{
						instance3.AddRange(tableDefinition.DefinedEnums);
						tableDefinition.DefinedEnums = null;
					}
				}
			}
			if (moduleDef.EnumTypes != null)
			{
				instance3.AddRange(moduleDef.EnumTypes);
			}
			ArrayBuilder<EnumTypeDefinition> instance4 = ArrayBuilder<EnumTypeDefinition>.GetInstance();
			foreach (EnumTypeDefinition item in instance3)
			{
				bool flag = instance2.ContainsKey(item.Id.Value);
				bool flag2 = instance.Contains(item.Name);
				if (!flag && !flag2)
				{
					instance4.Add(item);
					instance2.Add(item.Id.Value, item);
					instance.Add(item.Name);
				}
				else if (flag & flag2)
				{
					EnumTypeDefinition enumTypeDefinition = instance2[item.Id.Value];
					bool flag3 = enumTypeDefinition.Values.Length == item.Values.Length;
					if (flag3)
					{
						for (int j = 0; j < item.Values.Length; j++)
						{
							if (!SemanticFacts.IsSameName(item.Values[j].Name, enumTypeDefinition.Values[j].Name) || item.Values[j].Ordinal != enumTypeDefinition.Values[j].Ordinal)
							{
								flag3 = false;
								break;
							}
						}
					}
					if (!flag3)
					{
						diagnostics?.Add(NavDiagnostic.Create(ErrorCode.ERR_DuplicateLegacyEnumValuesDiffer, item.Id, item.Name, moduleDef.Name));
					}
				}
				else if (flag)
				{
					diagnostics?.Add(NavDiagnostic.Create(ErrorCode.ERR_DuplicateLegacyEnumId, item.Id, moduleDef.Name));
				}
				else if (flag2)
				{
					diagnostics?.Add(NavDiagnostic.Create(ErrorCode.ERR_DuplicateLegacyEnumName, item.Name, moduleDef.Name));
				}
			}
			return instance4.ToArrayAndFree();
		}
		finally
		{
			instance2.Free();
			instance.Free();
			instance3.Free();
		}
	}
}
