using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.DiaSymReader;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class CompilationTestData
{
	internal struct MethodData(ILBuilder ilBuilder, IMethodSymbol method)
	{
		public readonly ILBuilder ILBuilder = ilBuilder;

		public readonly IMethodSymbol Method = method;
	}

	public readonly ConcurrentDictionary<IMethodSymbol, MethodData> Methods = new ConcurrentDictionary<IMethodSymbol, MethodData>();

	public CommonPEModuleBuilder Module;

	public Func<ISymWriterMetadataProvider, SymUnmanagedWriter> SymWriterFactory;

	private ImmutableDictionary<string, MethodData> _lazyMethodsByName;

	private static readonly SymbolDisplayFormat _testDataKeyFormat = new SymbolDisplayFormat(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames, SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining, SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance, SymbolDisplayMemberOptions.IncludeExplicitInterface | SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeContainingType, SymbolDisplayParameterOptions.IncludeExtensionThis | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType, SymbolDisplayDelegateStyle.NameOnly, SymbolDisplayExtensionMethodStyle.Default, SymbolDisplayPropertyStyle.NameOnly, SymbolDisplayLocalOptions.None, SymbolDisplayKindOptions.None, SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName);

	private static readonly SymbolDisplayFormat _testDataOperatorKeyFormat = new SymbolDisplayFormat(_testDataKeyFormat.CompilerInternalOptions, _testDataKeyFormat.GlobalNamespaceStyle, _testDataKeyFormat.TypeQualificationStyle, _testDataKeyFormat.GenericsOptions, _testDataKeyFormat.MemberOptions | SymbolDisplayMemberOptions.IncludeType, _testDataKeyFormat.ParameterOptions, _testDataKeyFormat.DelegateStyle, _testDataKeyFormat.ExtensionMethodStyle, _testDataKeyFormat.PropertyStyle, _testDataKeyFormat.LocalOptions, _testDataKeyFormat.KindOptions, _testDataKeyFormat.MiscellaneousOptions);

	public ILBuilder GetIL(Func<IMethodSymbol, bool> predicate)
	{
		return Methods.Single((KeyValuePair<IMethodSymbol, MethodData> p) => predicate(p.Key)).Value.ILBuilder;
	}

	public ImmutableDictionary<string, MethodData> GetMethodsByName()
	{
		if (_lazyMethodsByName == null)
		{
			Dictionary<string, MethodData> dictionary = new Dictionary<string, MethodData>();
			foreach (KeyValuePair<IMethodSymbol, MethodData> method in Methods)
			{
				string methodName = GetMethodName(method.Key);
				if (dictionary.ContainsKey(methodName))
				{
					dictionary[methodName] = default;
				}
				else
				{
					dictionary.Add(methodName, method.Value);
				}
			}
			ImmutableDictionary<string, MethodData> value = dictionary.Where((KeyValuePair<string, MethodData> p) => p.Value.Method != null).ToImmutableDictionary();
			Interlocked.CompareExchange(ref _lazyMethodsByName, value, null);
		}
		return _lazyMethodsByName;
	}

	private static string GetMethodName(IMethodSymbol methodSymbol)
	{
		SymbolDisplayFormat format = ((methodSymbol.MethodKind == MethodKind.UserDefinedOperator) ? _testDataOperatorKeyFormat : _testDataKeyFormat);
		return methodSymbol.ToDisplayString(format);
	}
}
