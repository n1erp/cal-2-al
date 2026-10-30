using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public sealed class AnalyzerFileReference : AnalyzerReference, IEquatable<AnalyzerReference>
{
	private readonly string fullPath;

	private readonly IAnalyzerAssemblyLoader assemblyLoader;

	private string lazyDisplay;

	private object lazyIdentity;

	private ImmutableArray<DiagnosticAnalyzer> lazyAnalyzers;

	private ImmutableArray<string> lazyAnalyzerTypeNames;

	private Assembly lazyAssembly;

	internal Assembly Assembly => lazyAssembly ?? (lazyAssembly = assemblyLoader.LoadFromPath(fullPath));

	public override string FullPath => fullPath;

	public override string Display
	{
		get
		{
			if (lazyDisplay == null)
			{
				InitializeDisplayAndId();
			}
			return lazyDisplay;
		}
	}

	public override object Id
	{
		get
		{
			if (lazyIdentity == null)
			{
				InitializeDisplayAndId();
			}
			return lazyIdentity;
		}
	}

	public event EventHandler<AnalyzerLoadFailureEventArgs> AnalyzerLoadFailed;

	public AnalyzerFileReference(string fullPath, IAnalyzerAssemblyLoader assemblyLoader)
	{
		if (fullPath == null)
		{
			throw new ArgumentNullException("fullPath");
		}
		if (assemblyLoader == null)
		{
			throw new ArgumentNullException("assemblyLoader");
		}
		this.fullPath = fullPath;
		lazyAnalyzers = default;
		lazyAnalyzerTypeNames = default;
		this.assemblyLoader = assemblyLoader;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as AnalyzerFileReference);
	}

	public bool Equals(AnalyzerReference other)
	{
		if (other != null)
		{
			if (other.Display == Display)
			{
				return other.FullPath == FullPath;
			}
			return false;
		}
		return base.Equals(other);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Display.GetHashCode(), FullPath.GetHashCode());
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzers()
	{
		if (lazyAnalyzers.IsDefault)
		{
			ImmutableInterlocked.InterlockedInitialize(ref lazyAnalyzers, CreateAnalyzers(this));
		}
		return lazyAnalyzers;
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "by design")]
	internal void AddAnalyzers(ImmutableArray<DiagnosticAnalyzer>.Builder builder)
	{
		ImmutableArray<string> analyzerTypeNames;
		Assembly assembly;
		try
		{
			analyzerTypeNames = GetAnalyzerTypeNames();
			if (analyzerTypeNames.Length == 0)
			{
				return;
			}
			assembly = Assembly;
			if (assembly == null)
			{
				return;
			}
		}
		catch (Exception ex)
		{
			if (AnalyzerLoadFailed != null)
			{
				AnalyzerLoadFailed(this, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToLoadAnalyzer, ex.Message));
			}
			return;
		}
		int count = builder.Count;
		bool reportedError = false;
		IEnumerable<DiagnosticAnalyzer> analyzersForTypeNames = GetAnalyzersForTypeNames(assembly, analyzerTypeNames, ref reportedError);
		builder.AddRange(analyzersForTypeNames);
		if (builder.Count == count && !reportedError && AnalyzerLoadFailed != null)
		{
			AnalyzerLoadFailed(this, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers, CodeAnalysisResources.NoAnalyzersFound));
		}
	}

	internal ImmutableArray<string> GetAnalyzerTypeNames()
	{
		if (lazyAnalyzerTypeNames == null)
		{
			ImmutableArray<string> analyzerTypeNames = GetAnalyzerTypeNames(fullPath);
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyAnalyzerTypeNames, analyzerTypeNames, default);
		}
		return lazyAnalyzerTypeNames;
	}

	private static ImmutableArray<string> GetAnalyzerTypeNames(string fullPath)
	{
		using AssemblyMetadata assemblyMetadata = AssemblyMetadata.CreateFromFile(fullPath);
		return (from module in assemblyMetadata.GetModules()
			from typeDefHandle in module.GetMetadataReader().TypeDefinitions
			let metadataReader = module.GetMetadataReader()
			let typeDef = metadataReader.GetTypeDefinition(typeDefHandle)
			let typeName = metadataReader.GetString(typeDef.Namespace) + "." + module.GetMetadataReader().GetString(typeDef.Name)
			where IsAnalyzer(metadataReader, typeDefHandle)
			select typeName).ToImmutableArray();
	}

	private static bool IsAnalyzer(MetadataReader metadataReader, TypeDefinitionHandle typeDefHandle)
	{
		return metadataReader.GetTypeDefinition(typeDefHandle).GetCustomAttributes().Any((CustomAttributeHandle a) => IsDiagnosticAttribute(metadataReader, a));
	}

	private static bool IsDiagnosticAttribute(MetadataReader metadataReader, CustomAttributeHandle attributeHandle)
	{
		if (!GetTypeAndConstructor(metadataReader, attributeHandle, out var ctorType, out var _))
		{
			return false;
		}
		if (!GetAttributeNamespaceAndName(metadataReader, ctorType, out var namespaceHandle, out var nameHandle))
		{
			return false;
		}
		try
		{
			return StringEquals(metadataReader, nameHandle, typeof(DiagnosticAnalyzerAttribute).Name, ignoreCase: false) && StringEquals(metadataReader, namespaceHandle, typeof(DiagnosticAnalyzerAttribute).Namespace, ignoreCase: false);
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}

	private static bool StringEquals(MetadataReader metadataReader, StringHandle nameHandle, string name, bool ignoreCase)
	{
		if (ignoreCase)
		{
			return string.Equals(metadataReader.GetString(nameHandle), name, StringComparison.OrdinalIgnoreCase);
		}
		return metadataReader.StringComparer.Equals(nameHandle, name);
	}

	private static bool GetTypeAndConstructor(MetadataReader metadataReader, CustomAttributeHandle customAttribute, out EntityHandle ctorType, out EntityHandle attributeCtor)
	{
		try
		{
			ctorType = default;
			attributeCtor = metadataReader.GetCustomAttribute(customAttribute).Constructor;
			if (attributeCtor.Kind == HandleKind.MemberReference)
			{
				MemberReference memberReference = metadataReader.GetMemberReference((MemberReferenceHandle)attributeCtor);
				StringHandle name = memberReference.Name;
				if (!metadataReader.StringComparer.Equals(name, ".ctor"))
				{
					return false;
				}
				ctorType = memberReference.Parent;
			}
			else
			{
				if (attributeCtor.Kind != HandleKind.MethodDefinition)
				{
					return false;
				}
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition((MethodDefinitionHandle)attributeCtor);
				if (!metadataReader.StringComparer.Equals(methodDefinition.Name, ".ctor"))
				{
					return false;
				}
				ctorType = methodDefinition.GetDeclaringType();
				DebugAssertHelper.Assert(!ctorType.IsNil);
			}
			return true;
		}
		catch (BadImageFormatException)
		{
			ctorType = default;
			attributeCtor = default;
			return false;
		}
	}

	private static bool GetAttributeNamespaceAndName(MetadataReader metadataReader, EntityHandle typeDefOrRef, out StringHandle namespaceHandle, out StringHandle nameHandle)
	{
		nameHandle = default;
		namespaceHandle = default;
		try
		{
			if (typeDefOrRef.Kind == HandleKind.TypeReference)
			{
				TypeReference typeReference = metadataReader.GetTypeReference((TypeReferenceHandle)typeDefOrRef);
				HandleKind kind = typeReference.ResolutionScope.Kind;
				if (kind == HandleKind.TypeReference || kind == HandleKind.TypeDefinition)
				{
					return false;
				}
				nameHandle = typeReference.Name;
				namespaceHandle = typeReference.Namespace;
			}
			else
			{
				if (typeDefOrRef.Kind != HandleKind.TypeDefinition)
				{
					return false;
				}
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition((TypeDefinitionHandle)typeDefOrRef);
				nameHandle = typeDefinition.Name;
				namespaceHandle = typeDefinition.Namespace;
			}
			return true;
		}
		catch (BadImageFormatException)
		{
			return false;
		}
	}

	private static ImmutableArray<DiagnosticAnalyzer> CreateAnalyzers(AnalyzerFileReference reference)
	{
		ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
		reference.AddAnalyzers(builder);
		return builder.ToImmutable();
	}

	private void InitializeDisplayAndId()
	{
		lazyDisplay = Path.GetFileNameWithoutExtension(fullPath);
		lazyIdentity = lazyDisplay;
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "by design")]
	private IEnumerable<DiagnosticAnalyzer> GetAnalyzersForTypeNames(Assembly analyzerAssembly, IEnumerable<string> analyzerTypeNames, ref bool reportedError)
	{
		ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
		foreach (string analyzerTypeName in analyzerTypeNames)
		{
			Type type;
			try
			{
				type = analyzerAssembly.GetType(analyzerTypeName, throwOnError: true, ignoreCase: false);
			}
			catch (Exception ex)
			{
				if (AnalyzerLoadFailed != null)
				{
					AnalyzerLoadFailed(this, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToCreateAnalyzer, ex.Message, ex, analyzerTypeName));
				}
				reportedError = true;
				continue;
			}
			DebugAssertHelper.Assert(type != null);
			DiagnosticAnalyzer diagnosticAnalyzer;
			try
			{
				diagnosticAnalyzer = Activator.CreateInstance(type) as DiagnosticAnalyzer;
			}
			catch (Exception ex2)
			{
				if (AnalyzerLoadFailed != null)
				{
					AnalyzerLoadFailed(this, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToCreateAnalyzer, ex2.Message, ex2, analyzerTypeName));
				}
				reportedError = true;
				continue;
			}
			if (diagnosticAnalyzer != null)
			{
				builder.Add(diagnosticAnalyzer);
			}
		}
		return builder.ToImmutable();
	}
}
