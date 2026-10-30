using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.DiaSymReader;
using Roslyn.Utilities;

namespace Microsoft.Cci;

internal sealed class PdbWriter : IDisposable
{
	internal const uint Age = 1u;

	private readonly HashAlgorithmName _hashAlgorithmNameOpt;

	private readonly string _fileName;

	private readonly Func<ISymWriterMetadataProvider, SymUnmanagedWriter> _symWriterFactory;

	private readonly Dictionary<DebugSourceDocument, int> _documentIndex;

	private MetadataWriter _metadataWriter;

	private SymUnmanagedWriter _symWriter;

	private SymUnmanagedSequencePointsWriter _sequencePointsWriter;

	private readonly Dictionary<object, string> _qualifiedNameCache;

	private bool IsDeterministic
	{
		get
		{
			HashAlgorithmName hashAlgorithmNameOpt = _hashAlgorithmNameOpt;
			return hashAlgorithmNameOpt.Name != null;
		}
	}

	private CommonPEModuleBuilder Module => Context.Module;

	private EmitContext Context => _metadataWriter.Context;

	public PdbWriter(string fileName, Func<ISymWriterMetadataProvider, SymUnmanagedWriter> symWriterFactory, HashAlgorithmName hashAlgorithmNameOpt)
	{
		_fileName = fileName;
		_symWriterFactory = symWriterFactory;
		_hashAlgorithmNameOpt = hashAlgorithmNameOpt;
		_documentIndex = new Dictionary<DebugSourceDocument, int>();
		_qualifiedNameCache = new Dictionary<object, string>();
	}

	public void WriteTo(Stream stream)
	{
		_symWriter.WriteTo(stream);
	}

	public void Dispose()
	{
		_symWriter?.Dispose();
	}

	public void SerializeDebugInfo(IMethodBody methodBody, StandaloneSignatureHandle localSignatureHandleOpt, CustomDebugInfoWriter customDebugInfoWriter)
	{
		bool flag = methodBody.StateMachineTypeName != null;
		if (!flag && methodBody.SequencePoints.IsEmpty && methodBody.MethodDefinition != (Context.Module.DebugEntryPoint ?? Context.Module.PEEntryPoint))
		{
			return;
		}
		MethodDefinitionHandle methodDefinitionHandle = (MethodDefinitionHandle)_metadataWriter.GetMethodHandle(methodBody.MethodDefinition);
		int token = MetadataTokens.GetToken(methodDefinitionHandle);
		OpenMethod(token, methodBody.MethodDefinition);
		ImmutableArray<LocalScope> localScopes = methodBody.LocalScopes;
		if (localScopes.Length > 0)
		{
			DefineScopeLocals(localScopes[0], localSignatureHandleOpt);
		}
		if (!flag && methodBody.ImportScope != null)
		{
			if (customDebugInfoWriter.ShouldForwardNamespaceScopes(Context, methodBody, methodDefinitionHandle, out var forwardToMethod))
			{
				if (forwardToMethod != null)
				{
					UsingNamespace("@" + MetadataTokens.GetToken(_metadataWriter.GetMethodHandle(forwardToMethod)), methodBody.MethodDefinition);
				}
			}
			else
			{
				DefineNamespaceScopes(methodBody);
			}
		}
		DefineLocalScopes(localScopes, localSignatureHandleOpt);
		EmitSequencePoints(methodBody.SequencePoints);
		if (methodBody.MoveNextBodyInfo is AsyncMoveNextBodyDebugInfo asyncMoveNextBodyDebugInfo)
		{
			_symWriter.SetAsyncInfo(token, MetadataTokens.GetToken(_metadataWriter.GetMethodHandle(asyncMoveNextBodyDebugInfo.KickoffMethod)), asyncMoveNextBodyDebugInfo.CatchHandlerOffset, asyncMoveNextBodyDebugInfo.YieldOffsets.DangerousGetUnderlyingArray(), asyncMoveNextBodyDebugInfo.ResumeOffsets.DangerousGetUnderlyingArray());
		}
		CompilationOptions options = Context.Module.CommonCompilation.Options;
		bool suppressNewCustomDebugInfo = options.OutputKind == OutputKind.WindowsRuntimeMetadata;
		bool emitEncInfo = options.EnableEditAndContinue && _metadataWriter.IsFullMetadata;
		byte[] array = customDebugInfoWriter.SerializeMethodDebugInfo(Context, methodBody, methodDefinitionHandle, emitEncInfo, suppressNewCustomDebugInfo, out var emitExternNamespaces);
		if (array != null)
		{
			_symWriter.DefineCustomMetadata(array);
		}
		if (emitExternNamespaces)
		{
			DefineAssemblyReferenceAliases();
		}
		CloseMethod(methodBody.IL.Length);
	}

	private void DefineNamespaceScopes(IMethodBody methodBody)
	{
		CommonPEModuleBuilder module = Module;
		bool generateVisualBasicStylePdb = module.GenerateVisualBasicStylePdb;
		IMethodDefinition methodDefinition = methodBody.MethodDefinition;
		IImportScope importScope = methodBody.ImportScope;
		PooledHashSet<string> pooledHashSet = null;
		if (!generateVisualBasicStylePdb)
		{
			for (IImportScope importScope2 = importScope; importScope2 != null; importScope2 = importScope2.Parent)
			{
				foreach (UsedNamespaceOrType usedNamespace in importScope2.GetUsedNamespaces())
				{
					if (usedNamespace.TargetNamespaceOpt == null && usedNamespace.TargetTypeOpt == null)
					{
						if (pooledHashSet == null)
						{
							pooledHashSet = PooledHashSet<string>.GetInstance();
						}
						pooledHashSet.Add(usedNamespace.AliasOpt);
					}
				}
			}
		}
		for (IImportScope importScope3 = importScope; importScope3 != null; importScope3 = importScope3.Parent)
		{
			foreach (UsedNamespaceOrType usedNamespace2 in importScope3.GetUsedNamespaces())
			{
				string text = TryEncodeImport(usedNamespace2, pooledHashSet, isProjectLevel: false);
				if (text != null)
				{
					UsingNamespace(text, methodDefinition);
				}
			}
		}
		pooledHashSet?.Free();
		if (!generateVisualBasicStylePdb)
		{
			return;
		}
		string defaultNamespace = module.DefaultNamespace;
		if (!string.IsNullOrEmpty(defaultNamespace))
		{
			UsingNamespace("*" + defaultNamespace, module);
		}
		foreach (string item in module.LinkedAssembliesDebugInfo)
		{
			UsingNamespace("&" + item, module);
		}
		foreach (UsedNamespaceOrType import in module.GetImports())
		{
			string text2 = TryEncodeImport(import, null, isProjectLevel: true);
			if (text2 != null)
			{
				UsingNamespace(text2, methodDefinition);
			}
		}
		UsingNamespace(GetOrCreateSerializedNamespaceName(methodDefinition.ContainingNamespace), methodDefinition);
	}

	private void DefineAssemblyReferenceAliases()
	{
		foreach (AssemblyReferenceAlias assemblyReferenceAlias in Module.GetAssemblyReferenceAliases(Context))
		{
			UsingNamespace("Z" + assemblyReferenceAlias.Name + " " + assemblyReferenceAlias.Assembly.Identity.GetDisplayName(), Module);
		}
	}

	private string TryEncodeImport(UsedNamespaceOrType import, HashSet<string> declaredExternAliasesOpt, bool isProjectLevel)
	{
		if (Module.GenerateVisualBasicStylePdb)
		{
			if (import.TargetTypeOpt != null)
			{
				if (import.TargetTypeOpt.IsTypeSpecification())
				{
					return null;
				}
				string orCreateSerializedTypeName = GetOrCreateSerializedTypeName(import.TargetTypeOpt);
				if (import.AliasOpt != null)
				{
					return (isProjectLevel ? "@PA:" : "@FA:") + import.AliasOpt + "=" + orCreateSerializedTypeName;
				}
				return (isProjectLevel ? "@PT:" : "@FT:") + orCreateSerializedTypeName;
			}
			if (import.TargetNamespaceOpt != null)
			{
				string orCreateSerializedNamespaceName = GetOrCreateSerializedNamespaceName(import.TargetNamespaceOpt);
				if (import.AliasOpt == null)
				{
					return (isProjectLevel ? "@P:" : "@F:") + orCreateSerializedNamespaceName;
				}
				return (isProjectLevel ? "@PA:" : "@FA:") + import.AliasOpt + "=" + orCreateSerializedNamespaceName;
			}
			return (isProjectLevel ? "@PX:" : "@FX:") + import.AliasOpt + "=" + import.TargetXmlNamespaceOpt;
		}
		if (import.TargetTypeOpt != null)
		{
			string orCreateSerializedTypeName2 = GetOrCreateSerializedTypeName(import.TargetTypeOpt);
			if (import.AliasOpt == null)
			{
				return "T" + orCreateSerializedTypeName2;
			}
			return "A" + import.AliasOpt + " T" + orCreateSerializedTypeName2;
		}
		if (import.TargetNamespaceOpt != null)
		{
			string orCreateSerializedNamespaceName2 = GetOrCreateSerializedNamespaceName(import.TargetNamespaceOpt);
			if (import.AliasOpt != null)
			{
				if (import.TargetAssemblyOpt == null)
				{
					return "A" + import.AliasOpt + " U" + orCreateSerializedNamespaceName2;
				}
				return "A" + import.AliasOpt + " E" + orCreateSerializedNamespaceName2 + " " + GetAssemblyReferenceAlias(import.TargetAssemblyOpt, declaredExternAliasesOpt);
			}
			if (import.TargetAssemblyOpt == null)
			{
				return "U" + orCreateSerializedNamespaceName2;
			}
			return "E" + orCreateSerializedNamespaceName2 + " " + GetAssemblyReferenceAlias(import.TargetAssemblyOpt, declaredExternAliasesOpt);
		}
		return "X" + import.AliasOpt;
	}

	internal string GetOrCreateSerializedNamespaceName(INamespace @namespace)
	{
		if (!_qualifiedNameCache.TryGetValue(@namespace, out var value))
		{
			value = TypeNameSerializer.BuildQualifiedNamespaceName(@namespace);
			_qualifiedNameCache.Add(@namespace, value);
		}
		return value;
	}

	internal string GetOrCreateSerializedTypeName(ITypeReference typeReference)
	{
		if (!_qualifiedNameCache.TryGetValue(typeReference, out var value))
		{
			value = ((!Module.GenerateVisualBasicStylePdb) ? typeReference.GetSerializedTypeName(Context) : SerializeVisualBasicImportTypeReference(typeReference));
			_qualifiedNameCache.Add(typeReference, value);
		}
		return value;
	}

	private string SerializeVisualBasicImportTypeReference(ITypeReference typeReference)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		INestedTypeReference asNestedTypeReference = typeReference.AsNestedTypeReference;
		ArrayBuilder<string> arrayBuilder;
		if (asNestedTypeReference != null)
		{
			arrayBuilder = ArrayBuilder<string>.GetInstance();
			while (asNestedTypeReference != null)
			{
				arrayBuilder.Add(asNestedTypeReference.Name);
				typeReference = asNestedTypeReference.GetContainingType(_metadataWriter.Context);
				asNestedTypeReference = typeReference.AsNestedTypeReference;
			}
		}
		else
		{
			arrayBuilder = null;
		}
		INamespaceTypeReference asNamespaceTypeReference = typeReference.AsNamespaceTypeReference;
		string namespaceName = asNamespaceTypeReference.NamespaceName;
		if (namespaceName.Length != 0)
		{
			instance.Builder.Append(namespaceName);
			instance.Builder.Append('.');
		}
		instance.Builder.Append(asNamespaceTypeReference.Name);
		if (arrayBuilder != null)
		{
			for (int num = arrayBuilder.Count - 1; num >= 0; num--)
			{
				instance.Builder.Append('.');
				instance.Builder.Append(arrayBuilder[num]);
			}
			arrayBuilder.Free();
		}
		return instance.ToStringAndFree();
	}

	private string GetAssemblyReferenceAlias(IAssemblyReference assembly, HashSet<string> declaredExternAliases)
	{
		foreach (AssemblyReferenceAlias assemblyReferenceAlias in _metadataWriter.Context.Module.GetAssemblyReferenceAliases(_metadataWriter.Context))
		{
			if (assembly == assemblyReferenceAlias.Assembly && declaredExternAliases.Contains(assemblyReferenceAlias.Name))
			{
				return assemblyReferenceAlias.Name;
			}
		}
		throw ExceptionUtilities.Unreachable;
	}

	private void DefineLocalScopes(ImmutableArray<LocalScope> scopes, StandaloneSignatureHandle localSignatureHandleOpt)
	{
		bool generateVisualBasicStylePdb = Module.GenerateVisualBasicStylePdb;
		ArrayBuilder<LocalScope> instance = ArrayBuilder<LocalScope>.GetInstance();
		for (int i = 1; i < scopes.Length; i++)
		{
			LocalScope localScope = scopes[i];
			while (instance.Count > 0)
			{
				LocalScope localScope2 = instance.Last();
				if (localScope.StartOffset < localScope2.StartOffset + localScope2.Length)
				{
					break;
				}
				instance.RemoveLast();
				_symWriter.CloseScope(generateVisualBasicStylePdb ? (localScope2.EndOffset - 1) : localScope2.EndOffset);
			}
			instance.Add(localScope);
			_symWriter.OpenScope(localScope.StartOffset);
			DefineScopeLocals(localScope, localSignatureHandleOpt);
		}
		for (int num = instance.Count - 1; num >= 0; num--)
		{
			LocalScope localScope3 = instance[num];
			_symWriter.CloseScope(generateVisualBasicStylePdb ? (localScope3.EndOffset - 1) : localScope3.EndOffset);
		}
		instance.Free();
	}

	private void DefineScopeLocals(LocalScope currentScope, StandaloneSignatureHandle localSignatureHandleOpt)
	{
		foreach (ILocalDefinition constant in currentScope.Constants)
		{
			StandaloneSignatureHandle standaloneSignatureHandle = _metadataWriter.SerializeLocalConstantStandAloneSignature(constant);
			if (!_metadataWriter.IsLocalNameTooLong(constant))
			{
				_symWriter.DefineLocalConstant(constant.Name, constant.CompileTimeValue.Value, MetadataTokens.GetToken(standaloneSignatureHandle));
			}
		}
		foreach (ILocalDefinition variable in currentScope.Variables)
		{
			if (!_metadataWriter.IsLocalNameTooLong(variable))
			{
				_symWriter.DefineLocalVariable(variable.SlotIndex, variable.Name, (int)variable.PdbAttributes, (!localSignatureHandleOpt.IsNil) ? MetadataTokens.GetToken(localSignatureHandleOpt) : 0);
			}
		}
	}

	public void SetMetadataEmitter(MetadataWriter metadataWriter)
	{
		SymUnmanagedWriterCreationOptions options = (SymUnmanagedWriterCreationOptions)((IsDeterministic ? 8 : 4) | 2);
		SymWriterMetadataProvider symWriterMetadataProvider = new SymWriterMetadataProvider(metadataWriter);
		SymUnmanagedWriter symUnmanagedWriter;
		try
		{
			symUnmanagedWriter = ((_symWriterFactory != null) ? _symWriterFactory(symWriterMetadataProvider) : SymUnmanagedWriterFactory.CreateWriter(symWriterMetadataProvider, options));
		}
		catch (DllNotFoundException ex)
		{
			throw new SymUnmanagedWriterException(ex.Message);
		}
		catch (SymUnmanagedWriterException ex2) when (ex2.InnerException is NotSupportedException)
		{
			throw new SymUnmanagedWriterException(string.Format(IsDeterministic ? CodeAnalysisResources.SymWriterNotDeterministic : CodeAnalysisResources.SymWriterOlderVersionThanRequired, ex2.ImplementationModuleName));
		}
		_metadataWriter = metadataWriter;
		_symWriter = symUnmanagedWriter;
		_sequencePointsWriter = new SymUnmanagedSequencePointsWriter(symUnmanagedWriter);
	}

	public BlobContentId GetContentId()
	{
		BlobContentId result;
		if (IsDeterministic)
		{
			result = BlobContentId.FromHash(CryptographicHashProvider.ComputeHash(_hashAlgorithmNameOpt, _symWriter.GetUnderlyingData()));
			_symWriter.UpdateSignature(result.Guid, result.Stamp, 1);
		}
		else
		{
			_symWriter.GetSignature(out var guid, out var stamp, out var _);
			result = new BlobContentId(guid, stamp);
		}
		_symWriter.Dispose();
		return result;
	}

	public void SetEntryPoint(int entryMethodToken)
	{
		_symWriter.SetEntryPoint(entryMethodToken);
	}

	private int GetDocumentIndex(DebugSourceDocument document)
	{
		if (_documentIndex.TryGetValue(document, out var value))
		{
			return value;
		}
		DebugSourceInfo sourceInfo = document.GetSourceInfo();
		Guid algorithmId;
		byte[] checksum;
		if (!sourceInfo.Checksum.IsDefault)
		{
			algorithmId = sourceInfo.ChecksumAlgorithmId;
			checksum = sourceInfo.Checksum.DangerousGetUnderlyingArray();
		}
		else
		{
			algorithmId = default;
			checksum = null;
		}
		byte[] source = (sourceInfo.EmbeddedTextBlob.IsDefault ? null : sourceInfo.EmbeddedTextBlob.DangerousGetUnderlyingArray());
		value = _symWriter.DefineDocument(document.Location, document.Language, document.LanguageVendor, document.DocumentType, algorithmId, checksum, source);
		_documentIndex.Add(document, value);
		return value;
	}

	private void OpenMethod(int methodToken, IMethodDefinition method)
	{
		_symWriter.OpenMethod(methodToken);
		_symWriter.OpenScope(0);
	}

	private void CloseMethod(int ilLength)
	{
		_symWriter.CloseScope(ilLength);
		_symWriter.CloseMethod();
	}

	private void UsingNamespace(string fullName, INamedEntity errorEntity)
	{
		if (!_metadataWriter.IsUsingStringTooLong(fullName, errorEntity))
		{
			_symWriter.UsingNamespace(fullName);
		}
	}

	private void EmitSequencePoints(ImmutableArray<SequencePoint> sequencePoints)
	{
		int num = -1;
		DebugSourceDocument debugSourceDocument = null;
		foreach (SequencePoint item in sequencePoints)
		{
			DebugSourceDocument document = item.Document;
			int documentIndex;
			if (debugSourceDocument == document)
			{
				documentIndex = num;
			}
			else
			{
				debugSourceDocument = document;
				documentIndex = (num = GetDocumentIndex(debugSourceDocument));
			}
			_sequencePointsWriter.Add(documentIndex, item.Offset, item.StartLine, item.StartColumn, item.EndLine, item.EndColumn);
		}
		_sequencePointsWriter.Flush();
	}

	[Conditional("DEBUG")]
	public void AssertAllDefinitionsHaveTokens(MultiDictionary<DebugSourceDocument, DefinitionWithLocation> file2definitions)
	{
		foreach (KeyValuePair<DebugSourceDocument, MultiDictionary<DebugSourceDocument, DefinitionWithLocation>.ValueSet> file2definition in file2definitions)
		{
			foreach (DefinitionWithLocation item in file2definition.Value)
			{
				_metadataWriter.GetDefinitionHandle(item.Definition);
			}
		}
	}

	public void WriteDefinitionLocations(MultiDictionary<DebugSourceDocument, DefinitionWithLocation> file2definitions)
	{
		bool flag = false;
		foreach (KeyValuePair<DebugSourceDocument, MultiDictionary<DebugSourceDocument, DefinitionWithLocation>.ValueSet> file2definition in file2definitions)
		{
			foreach (DefinitionWithLocation item in file2definition.Value)
			{
				if (!flag)
				{
					_symWriter.OpenTokensToSourceSpansMap();
					flag = true;
				}
				int token = MetadataTokens.GetToken(_metadataWriter.GetDefinitionHandle(item.Definition));
				_symWriter.MapTokenToSourceSpan(token, GetDocumentIndex(file2definition.Key), item.StartLine + 1, item.StartColumn + 1, item.EndLine + 1, item.EndColumn + 1);
			}
		}
		if (flag)
		{
			_symWriter.CloseTokensToSourceSpansMap();
		}
	}

	public void EmbedSourceLink(Stream stream)
	{
		byte[] sourceLinkData;
		try
		{
			sourceLinkData = stream.ReadAllBytes();
		}
		catch (Exception ex)
		{
			throw new SymUnmanagedWriterException(ex.Message, ex);
		}
		try
		{
			_symWriter.SetSourceLinkData(sourceLinkData);
		}
		catch (SymUnmanagedWriterException ex2) when (ex2.InnerException is NotSupportedException)
		{
			throw new SymUnmanagedWriterException(string.Format(CodeAnalysisResources.SymWriterDoesNotSupportSourceLink, ex2.ImplementationModuleName));
		}
	}

	public void WriteRemainingEmbeddedDocuments(IEnumerable<DebugSourceDocument> embeddedDocuments)
	{
		foreach (DebugSourceDocument embeddedDocument in embeddedDocuments)
		{
			GetDocumentIndex(embeddedDocument);
		}
	}
}
