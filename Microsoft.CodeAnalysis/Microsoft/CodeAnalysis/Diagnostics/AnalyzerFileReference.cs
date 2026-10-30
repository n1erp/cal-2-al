using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class AnalyzerFileReference : AnalyzerReference, IEquatable<AnalyzerReference>
{
	private delegate bool AttributePredicate(PEModule module, CustomAttributeHandle attribute);

	private sealed class Extensions<TExtension> where TExtension : class
	{
		private readonly AnalyzerFileReference _reference;

		private readonly AttributePredicate _attributePredicate;

		private ImmutableArray<TExtension> _lazyAllExtensions;

		private ImmutableDictionary<string, ImmutableArray<TExtension>> _lazyExtensionsPerLanguage;

		private ImmutableDictionary<string, ImmutableHashSet<string>> _lazyExtensionTypeNameMap;

		internal Extensions(AnalyzerFileReference reference, AttributePredicate attributePredicate)
		{
			_reference = reference;
			_attributePredicate = attributePredicate;
			_lazyAllExtensions = default;
			_lazyExtensionsPerLanguage = ImmutableDictionary<string, ImmutableArray<TExtension>>.Empty;
		}

		internal ImmutableArray<TExtension> GetExtensionsForAllLanguages()
		{
			if (_lazyAllExtensions.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyAllExtensions, CreateExtensionsForAllLanguages(this));
			}
			return _lazyAllExtensions;
		}

		private static ImmutableArray<TExtension> CreateExtensionsForAllLanguages(Extensions<TExtension> extensions)
		{
			ImmutableDictionary<string, ImmutableArray<TExtension>>.Builder builder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<TExtension>>();
			extensions.AddExtensions(builder);
			ImmutableArray<TExtension>.Builder builder2 = ImmutableArray.CreateBuilder<TExtension>();
			foreach (ImmutableArray<TExtension> value in builder.Values)
			{
				builder2.AddRange(value);
			}
			return builder2.ToImmutable();
		}

		internal ImmutableArray<TExtension> GetExtensions(string language)
		{
			if (string.IsNullOrEmpty(language))
			{
				throw new ArgumentException("language");
			}
			return ImmutableInterlocked.GetOrAdd(ref _lazyExtensionsPerLanguage, language, CreateLanguageSpecificExtensions, this);
		}

		private static ImmutableArray<TExtension> CreateLanguageSpecificExtensions(string language, Extensions<TExtension> extensions)
		{
			ImmutableArray<TExtension>.Builder builder = ImmutableArray.CreateBuilder<TExtension>();
			extensions.AddExtensions(builder, language);
			return builder.ToImmutable();
		}

		internal ImmutableDictionary<string, ImmutableHashSet<string>> GetExtensionTypeNameMap()
		{
			if (_lazyExtensionTypeNameMap == null)
			{
				ImmutableDictionary<string, ImmutableHashSet<string>> analyzerTypeNameMap = GetAnalyzerTypeNameMap(_reference._fullPath, _attributePredicate);
				Interlocked.CompareExchange(ref _lazyExtensionTypeNameMap, analyzerTypeNameMap, null);
			}
			return _lazyExtensionTypeNameMap;
		}

		internal void AddExtensions(ImmutableDictionary<string, ImmutableArray<TExtension>>.Builder builder)
		{
			ImmutableDictionary<string, ImmutableHashSet<string>> extensionTypeNameMap;
			Assembly assembly;
			try
			{
				extensionTypeNameMap = GetExtensionTypeNameMap();
				if (extensionTypeNameMap.Count == 0)
				{
					return;
				}
				assembly = _reference.GetAssembly();
			}
			catch (Exception e)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e));
				return;
			}
			int count = builder.Count;
			bool reportedError = false;
			foreach (var (text2, _) in extensionTypeNameMap)
			{
				if (text2 != null)
				{
					ImmutableArray<TExtension> languageSpecificAnalyzers = GetLanguageSpecificAnalyzers(assembly, extensionTypeNameMap, text2, ref reportedError);
					builder.Add(text2, languageSpecificAnalyzers);
				}
			}
			if (builder.Count == count && !reportedError)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers, CodeAnalysisResources.NoAnalyzersFound));
			}
		}

		internal void AddExtensions(ImmutableArray<TExtension>.Builder builder, string language)
		{
			ImmutableDictionary<string, ImmutableHashSet<string>> extensionTypeNameMap;
			Assembly assembly;
			try
			{
				extensionTypeNameMap = GetExtensionTypeNameMap();
				if (!extensionTypeNameMap.ContainsKey(language))
				{
					return;
				}
				assembly = _reference.GetAssembly();
				if ((object)assembly == null)
				{
					return;
				}
			}
			catch (Exception e)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e));
				return;
			}
			int count = builder.Count;
			bool reportedError = false;
			ImmutableArray<TExtension> languageSpecificAnalyzers = GetLanguageSpecificAnalyzers(assembly, extensionTypeNameMap, language, ref reportedError);
			builder.AddRange(languageSpecificAnalyzers);
			if (builder.Count == count && !reportedError)
			{
				_reference.AnalyzerLoadFailed?.Invoke(_reference, new AnalyzerLoadFailureEventArgs(AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers, CodeAnalysisResources.NoAnalyzersFound));
			}
		}

		private ImmutableArray<TExtension> GetLanguageSpecificAnalyzers(Assembly analyzerAssembly, ImmutableDictionary<string, ImmutableHashSet<string>> analyzerTypeNameMap, string language, ref bool reportedError)
		{
			if (!analyzerTypeNameMap.TryGetValue(language, out var value))
			{
				return ImmutableArray<TExtension>.Empty;
			}
			return GetAnalyzersForTypeNames(analyzerAssembly, value, ref reportedError);
		}

		private ImmutableArray<TExtension> GetAnalyzersForTypeNames(Assembly analyzerAssembly, IEnumerable<string> analyzerTypeNames, ref bool reportedError)
		{
			ImmutableArray<TExtension>.Builder builder = ImmutableArray.CreateBuilder<TExtension>();
			foreach (string analyzerTypeName in analyzerTypeNames)
			{
				Type type;
				try
				{
					type = analyzerAssembly.GetType(analyzerTypeName, throwOnError: true, ignoreCase: false);
				}
				catch (Exception e)
				{
					_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e, analyzerTypeName));
					reportedError = true;
					continue;
				}
				TExtension val;
				try
				{
					val = Activator.CreateInstance(type) as TExtension;
				}
				catch (Exception e2)
				{
					_reference.AnalyzerLoadFailed?.Invoke(_reference, CreateAnalyzerFailedArgs(e2, analyzerTypeName));
					reportedError = true;
					continue;
				}
				if (val != null)
				{
					builder.Add(val);
				}
			}
			return builder.ToImmutable();
		}
	}

	private static readonly string s_diagnosticAnalyzerAttributeNamespace = typeof(DiagnosticAnalyzerAttribute).Namespace;

	private readonly string _fullPath;

	private readonly IAnalyzerAssemblyLoader _assemblyLoader;

	private readonly Extensions<DiagnosticAnalyzer> _diagnosticAnalyzers;

	private string _lazyDisplay;

	private object _lazyIdentity;

	private Assembly _lazyAssembly;

	public override string FullPath => _fullPath;

	public override string Display
	{
		get
		{
			if (_lazyDisplay == null)
			{
				InitializeDisplayAndId();
			}
			return _lazyDisplay;
		}
	}

	public override object Id
	{
		get
		{
			if (_lazyIdentity == null)
			{
				InitializeDisplayAndId();
			}
			return _lazyIdentity;
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
		_fullPath = fullPath;
		_diagnosticAnalyzers = new Extensions<DiagnosticAnalyzer>(this, IsDiagnosticAnalyzerAttribute);
		_assemblyLoader = assemblyLoader;
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzersForAllLanguages()
	{
		return _diagnosticAnalyzers.GetExtensionsForAllLanguages();
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzers(string language)
	{
		return _diagnosticAnalyzers.GetExtensions(language);
	}

	private void InitializeDisplayAndId()
	{
		try
		{
			using PEReader peReader = new PEReader(FileUtilities.OpenRead(_fullPath));
			AssemblyIdentity assemblyIdentity = peReader.GetMetadataReader().ReadAssemblyIdentityOrThrow();
			_lazyDisplay = assemblyIdentity.Name;
			_lazyIdentity = assemblyIdentity;
		}
		catch
		{
			_lazyDisplay = Path.GetFileNameWithoutExtension(_fullPath);
			_lazyIdentity = _lazyDisplay;
		}
	}

	internal void AddAnalyzers(ImmutableArray<DiagnosticAnalyzer>.Builder builder, string language)
	{
		_diagnosticAnalyzers.AddExtensions(builder, language);
	}

	private static AnalyzerLoadFailureEventArgs CreateAnalyzerFailedArgs(Exception e, string typeNameOpt = null)
	{
		e = (e as TargetInvocationException) ?? e;
		string message = e.Message.Replace("\r", "").Replace("\n", "");
		return new AnalyzerLoadFailureEventArgs((typeNameOpt == null) ? AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToLoadAnalyzer : AnalyzerLoadFailureEventArgs.FailureErrorCode.UnableToCreateAnalyzer, message, e, typeNameOpt);
	}

	internal ImmutableDictionary<string, ImmutableHashSet<string>> GetAnalyzerTypeNameMap()
	{
		return _diagnosticAnalyzers.GetExtensionTypeNameMap();
	}

	private static ImmutableDictionary<string, ImmutableHashSet<string>> GetAnalyzerTypeNameMap(string fullPath, AttributePredicate attributePredicate)
	{
		using AssemblyMetadata assemblyMetadata = AssemblyMetadata.CreateFromFile(fullPath);
		return (from module in assemblyMetadata.GetModules()
			from typeDefHandle in module.MetadataReader.TypeDefinitions
			let typeDef = module.MetadataReader.GetTypeDefinition(typeDefHandle)
			let typeName = GetFullyQualifiedTypeName(typeDef, module.Module)
			from supportedLanguage in GetSupportedLanguages(typeDef, module.Module, attributePredicate)
			group typeName by supportedLanguage).ToImmutableDictionary((IGrouping<string, string> g) => g.Key, (IGrouping<string, string> g) => g.ToImmutableHashSet());
	}

	private static IEnumerable<string> GetSupportedLanguages(TypeDefinition typeDef, PEModule peModule, AttributePredicate attributePredicate)
	{
		return (from customAttrHandle in typeDef.GetCustomAttributes()
			where attributePredicate(peModule, customAttrHandle)
			let attributeSupportedLanguages = GetSupportedLanguages(peModule, customAttrHandle)
			where attributeSupportedLanguages != null
			select attributeSupportedLanguages).SelectMany((IEnumerable<string> x) => x);
	}

	private static IEnumerable<string> GetSupportedLanguages(PEModule peModule, CustomAttributeHandle customAttrHandle)
	{
		BlobReader sig = peModule.GetMemoryReaderOrThrow(peModule.GetCustomAttributeValueOrThrow(customAttrHandle));
		if (sig.Length > 4 && sig.ReadByte() == 1 && sig.ReadByte() == 0)
		{
			if (!PEModule.CrackStringInAttributeValue(out var value, ref sig))
			{
				return SpecializedCollections.EmptyEnumerable<string>();
			}
			if (PEModule.CrackStringArrayInAttributeValue(out var value2, ref sig))
			{
				if (value2.Length == 0)
				{
					return SpecializedCollections.SingletonEnumerable(value);
				}
				return value2.Insert(0, value);
			}
		}
		return SpecializedCollections.EmptyEnumerable<string>();
	}

	private static bool IsDiagnosticAnalyzerAttribute(PEModule peModule, CustomAttributeHandle customAttrHandle)
	{
		EntityHandle ctor;
		return peModule.IsTargetAttribute(customAttrHandle, s_diagnosticAnalyzerAttributeNamespace, "DiagnosticAnalyzerAttribute", out ctor);
	}

	private static string GetFullyQualifiedTypeName(TypeDefinition typeDef, PEModule peModule)
	{
		TypeDefinitionHandle declaringType = typeDef.GetDeclaringType();
		if (declaringType.IsNil)
		{
			return peModule.GetFullNameOrThrow(typeDef.Namespace, typeDef.Name);
		}
		return GetFullyQualifiedTypeName(peModule.MetadataReader.GetTypeDefinition(declaringType), peModule) + "+" + peModule.MetadataReader.GetString(typeDef.Name);
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
		return Hash.Combine(Display, FullPath.GetHashCode());
	}

	public Assembly GetAssembly()
	{
		if ((object)_lazyAssembly == null)
		{
			_lazyAssembly = _assemblyLoader.LoadFromPath(_fullPath);
		}
		return _lazyAssembly;
	}
}
