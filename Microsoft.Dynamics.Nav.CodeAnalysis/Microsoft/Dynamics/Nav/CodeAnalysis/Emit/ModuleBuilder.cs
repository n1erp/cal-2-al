using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class ModuleBuilder
{
	private readonly ConcurrentDictionary<IObjectTypeSymbol, ConcurrentBag<EmittedMethod>> methodBodyMap = new ConcurrentDictionary<IObjectTypeSymbol, ConcurrentBag<EmittedMethod>>(ReferenceEqualityComparer.Instance);

	private readonly ConcurrentDictionary<IApplicationObjectTypeSymbol, (CompilationUnitSyntax CSharpCompilationUnit, string AlSourceCode)> codeMap = new ConcurrentDictionary<IApplicationObjectTypeSymbol, (CompilationUnitSyntax, string)>(ReferenceEqualityComparer.Instance);

	private readonly ConcurrentDictionary<IApplicationObjectTypeSymbol, MemoryStream> metadataMap = new ConcurrentDictionary<IApplicationObjectTypeSymbol, MemoryStream>(ReferenceEqualityComparer.Instance);

	private readonly ConcurrentDictionary<IObjectTypeSymbol, Tuple<MemoryStream, string>> objectMetadataMap = new ConcurrentDictionary<IObjectTypeSymbol, Tuple<MemoryStream, string>>(ReferenceEqualityComparer.Instance);

	internal EmitOptions Options { get; }

	public CodeGenerator CodeGenerator { get; }

	internal ModuleCompilationState CompilationState { get; }

	public ModuleBuilder(EmitOptions options, CompilationOptions compilationOptions)
	{
		Options = options;
		CodeGenerator = new CodeGenerator(compilationOptions, options);
		CompilationState = new ModuleCompilationState();
	}

	internal void SetMethodBody(IMethodSymbol methodSymbol, EmittedMethod method)
	{
		ObjectTypeSymbol containingSymbolOfType = methodSymbol.GetContainingSymbolOfType<ObjectTypeSymbol>();
		if (!methodBodyMap.TryGetValue(containingSymbolOfType, out var value))
		{
			value = methodBodyMap.GetOrAdd(containingSymbolOfType, new ConcurrentBag<EmittedMethod>());
		}
		value.Add(method);
	}

	internal IEnumerable<EmittedMethod> GetEmittedMethods(IObjectTypeSymbol symbol)
	{
		if (!methodBodyMap.TryGetValue(symbol, out var value))
		{
			return SpecializedCollections.EmptyEnumerable<EmittedMethod>();
		}
		return value.OrderBy((EmittedMethod k) => k.MethodScope.Identifier.ValueText, StringComparer.Ordinal);
	}

	internal void SetApplicationObjectCode(IApplicationObjectTypeSymbol symbol, CompilationUnitSyntax syntax, string alSourceCode)
	{
		codeMap.Add(symbol, (syntax, alSourceCode));
	}

	internal ImmutableDictionary<IApplicationObjectTypeSymbol, (CompilationUnitSyntax CompilationUnit, string AlSourceCode)> GetApplicationObjectCode()
	{
		return codeMap.ToImmutableDictionary();
	}

	internal ImmutableDictionary<IProfileTypeSymbol, Tuple<MemoryStream, string>> GetProfileObjectMetadata()
	{
		return GetObjectMetadata<IProfileTypeSymbol>(SymbolKind.Profile);
	}

	internal ImmutableDictionary<T, Tuple<MemoryStream, string>> GetObjectMetadata<T>(SymbolKind kind) where T : IObjectTypeSymbol
	{
		return objectMetadataMap.Where((KeyValuePair<IObjectTypeSymbol, Tuple<MemoryStream, string>> m) => m.Key.Kind == kind).ToImmutableDictionary((KeyValuePair<IObjectTypeSymbol, Tuple<MemoryStream, string>> kv) => (T)kv.Key, (KeyValuePair<IObjectTypeSymbol, Tuple<MemoryStream, string>> kv) => kv.Value);
	}

	internal void SetApplicationObjectMetadata(IApplicationObjectTypeSymbol symbol, MemoryStream metadata)
	{
		metadataMap.Add(symbol, metadata);
	}

	internal bool TryGetApplicationObjectMetadata(IApplicationObjectTypeSymbol symbol, out MemoryStream metadata)
	{
		return metadataMap.TryGetValue(symbol, out metadata);
	}

	internal void SetObjectMetadata(IObjectTypeSymbol symbol, MemoryStream metadata, string alSourceCode)
	{
		objectMetadataMap.Add(symbol, new Tuple<MemoryStream, string>(metadata, alSourceCode));
	}

	internal void CompilationFinished()
	{
		CompilationState.Freeze();
	}
}
