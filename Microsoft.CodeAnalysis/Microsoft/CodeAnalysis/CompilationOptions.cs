using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public abstract class CompilationOptions
{
	private readonly Lazy<ImmutableArray<Diagnostic>> _lazyErrors;

	public OutputKind OutputKind { get; protected set; }

	public string ModuleName { get; protected set; }

	public string ScriptClassName { get; protected set; }

	public string MainTypeName { get; protected set; }

	public ImmutableArray<byte> CryptoPublicKey { get; protected set; }

	public string CryptoKeyFile { get; protected set; }

	public string CryptoKeyContainer { get; protected set; }

	public bool? DelaySign { get; protected set; }

	public bool PublicSign { get; protected set; }

	public bool CheckOverflow { get; protected set; }

	public Platform Platform { get; protected set; }

	public OptimizationLevel OptimizationLevel { get; protected set; }

	public ReportDiagnostic GeneralDiagnosticOption { get; protected set; }

	public int WarningLevel { get; protected set; }

	public bool ConcurrentBuild { get; protected set; }

	public bool Deterministic { get; protected set; }

	internal DateTime CurrentLocalTime { get; private set; }

	internal DateTime CurrentLocalTime_internal_protected_set
	{
		set
		{
			CurrentLocalTime = value;
		}
	}

	internal bool DebugPlusMode { get; private set; }

	internal bool DebugPlusMode_internal_protected_set
	{
		set
		{
			DebugPlusMode = value;
		}
	}

	public MetadataImportOptions MetadataImportOptions { get; protected set; }

	internal bool ReferencesSupersedeLowerVersions { get; private set; }

	internal bool ReferencesSupersedeLowerVersions_internal_protected_set
	{
		set
		{
			ReferencesSupersedeLowerVersions = value;
		}
	}

	public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; protected set; }

	public bool ReportSuppressedDiagnostics { get; protected set; }

	public MetadataReferenceResolver MetadataReferenceResolver { get; protected set; }

	public XmlReferenceResolver XmlReferenceResolver { get; protected set; }

	public SourceReferenceResolver SourceReferenceResolver { get; protected set; }

	public StrongNameProvider StrongNameProvider { get; protected set; }

	public AssemblyIdentityComparer AssemblyIdentityComparer { get; protected set; }

	[Obsolete]
	protected internal ImmutableArray<string> Features
	{
		get
		{
			throw new NotImplementedException();
		}
		protected set
		{
			throw new NotImplementedException();
		}
	}

	public abstract string Language { get; }

	internal bool EnableEditAndContinue => OptimizationLevel == OptimizationLevel.Debug;

	public ImmutableArray<Diagnostic> Errors => _lazyErrors.Value;

	internal abstract Diagnostic FilterDiagnostic(Diagnostic diagnostic);

	internal CompilationOptions(OutputKind outputKind, bool reportSuppressedDiagnostics, string moduleName, string mainTypeName, string scriptClassName, string cryptoKeyContainer, string cryptoKeyFile, ImmutableArray<byte> cryptoPublicKey, bool? delaySign, bool publicSign, OptimizationLevel optimizationLevel, bool checkOverflow, Platform platform, ReportDiagnostic generalDiagnosticOption, int warningLevel, ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions, bool concurrentBuild, bool deterministic, DateTime currentLocalTime, bool debugPlusMode, XmlReferenceResolver xmlReferenceResolver, SourceReferenceResolver sourceReferenceResolver, MetadataReferenceResolver metadataReferenceResolver, AssemblyIdentityComparer assemblyIdentityComparer, StrongNameProvider strongNameProvider, MetadataImportOptions metadataImportOptions, bool referencesSupersedeLowerVersions)
	{
		OutputKind = outputKind;
		ModuleName = moduleName;
		MainTypeName = mainTypeName;
		ScriptClassName = scriptClassName ?? "Script";
		CryptoKeyContainer = cryptoKeyContainer;
		CryptoKeyFile = (string.IsNullOrEmpty(cryptoKeyFile) ? null : cryptoKeyFile);
		CryptoPublicKey = cryptoPublicKey.NullToEmpty();
		DelaySign = delaySign;
		CheckOverflow = checkOverflow;
		Platform = platform;
		GeneralDiagnosticOption = generalDiagnosticOption;
		WarningLevel = warningLevel;
		SpecificDiagnosticOptions = specificDiagnosticOptions;
		ReportSuppressedDiagnostics = reportSuppressedDiagnostics;
		OptimizationLevel = optimizationLevel;
		ConcurrentBuild = concurrentBuild;
		Deterministic = deterministic;
		CurrentLocalTime = currentLocalTime;
		DebugPlusMode = debugPlusMode;
		XmlReferenceResolver = xmlReferenceResolver;
		SourceReferenceResolver = sourceReferenceResolver;
		MetadataReferenceResolver = metadataReferenceResolver;
		StrongNameProvider = strongNameProvider;
		AssemblyIdentityComparer = assemblyIdentityComparer ?? AssemblyIdentityComparer.Default;
		MetadataImportOptions = metadataImportOptions;
		ReferencesSupersedeLowerVersions = referencesSupersedeLowerVersions;
		PublicSign = publicSign;
		_lazyErrors = new Lazy<ImmutableArray<Diagnostic>>(() =>
		{
			ArrayBuilder<Diagnostic> instance = ArrayBuilder<Diagnostic>.GetInstance();
			ValidateOptions(instance);
			return instance.ToImmutableAndFree();
		});
	}

	internal bool CanReuseCompilationReferenceManager(CompilationOptions other)
	{
		if (MetadataImportOptions == other.MetadataImportOptions && ReferencesSupersedeLowerVersions == other.ReferencesSupersedeLowerVersions && OutputKind.IsNetModule() == other.OutputKind.IsNetModule() && object.Equals(XmlReferenceResolver, other.XmlReferenceResolver) && object.Equals(MetadataReferenceResolver, other.MetadataReferenceResolver))
		{
			return object.Equals(AssemblyIdentityComparer, other.AssemblyIdentityComparer);
		}
		return false;
	}

	internal static bool IsValidFileAlignment(int value)
	{
		switch (value)
		{
		case 512:
		case 1024:
		case 2048:
		case 4096:
		case 8192:
			return true;
		default:
			return false;
		}
	}

	internal abstract ImmutableArray<string> GetImports();

	public CompilationOptions WithGeneralDiagnosticOption(ReportDiagnostic value)
	{
		return CommonWithGeneralDiagnosticOption(value);
	}

	public CompilationOptions WithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> value)
	{
		return CommonWithSpecificDiagnosticOptions(value);
	}

	public CompilationOptions WithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> value)
	{
		return CommonWithSpecificDiagnosticOptions(value);
	}

	public CompilationOptions WithReportSuppressedDiagnostics(bool value)
	{
		return CommonWithReportSuppressedDiagnostics(value);
	}

	public CompilationOptions WithConcurrentBuild(bool concurrent)
	{
		return CommonWithConcurrentBuild(concurrent);
	}

	public CompilationOptions WithDeterministic(bool deterministic)
	{
		return CommonWithDeterministic(deterministic);
	}

	public CompilationOptions WithOutputKind(OutputKind kind)
	{
		return CommonWithOutputKind(kind);
	}

	public CompilationOptions WithPlatform(Platform platform)
	{
		return CommonWithPlatform(platform);
	}

	public CompilationOptions WithPublicSign(bool publicSign)
	{
		return CommonWithPublicSign(publicSign);
	}

	public CompilationOptions WithOptimizationLevel(OptimizationLevel value)
	{
		return CommonWithOptimizationLevel(value);
	}

	public CompilationOptions WithXmlReferenceResolver(XmlReferenceResolver resolver)
	{
		return CommonWithXmlReferenceResolver(resolver);
	}

	public CompilationOptions WithSourceReferenceResolver(SourceReferenceResolver resolver)
	{
		return CommonWithSourceReferenceResolver(resolver);
	}

	public CompilationOptions WithMetadataReferenceResolver(MetadataReferenceResolver resolver)
	{
		return CommonWithMetadataReferenceResolver(resolver);
	}

	public CompilationOptions WithAssemblyIdentityComparer(AssemblyIdentityComparer comparer)
	{
		return CommonWithAssemblyIdentityComparer(comparer);
	}

	public CompilationOptions WithStrongNameProvider(StrongNameProvider provider)
	{
		return CommonWithStrongNameProvider(provider);
	}

	public CompilationOptions WithModuleName(string moduleName)
	{
		return CommonWithModuleName(moduleName);
	}

	public CompilationOptions WithMainTypeName(string mainTypeName)
	{
		return CommonWithMainTypeName(mainTypeName);
	}

	public CompilationOptions WithScriptClassName(string scriptClassName)
	{
		return CommonWithScriptClassName(scriptClassName);
	}

	public CompilationOptions WithCryptoKeyContainer(string cryptoKeyContainer)
	{
		return CommonWithCryptoKeyContainer(cryptoKeyContainer);
	}

	public CompilationOptions WithCryptoKeyFile(string cryptoKeyFile)
	{
		return CommonWithCryptoKeyFile(cryptoKeyFile);
	}

	public CompilationOptions WithCryptoPublicKey(ImmutableArray<byte> cryptoPublicKey)
	{
		return CommonWithCryptoPublicKey(cryptoPublicKey);
	}

	public CompilationOptions WithDelaySign(bool? delaySign)
	{
		return CommonWithDelaySign(delaySign);
	}

	public CompilationOptions WithOverflowChecks(bool checkOverflow)
	{
		return CommonWithCheckOverflow(checkOverflow);
	}

	public CompilationOptions WithMetadataImportOptions(MetadataImportOptions value)
	{
		return CommonWithMetadataImportOptions(value);
	}

	protected abstract CompilationOptions CommonWithConcurrentBuild(bool concurrent);

	protected abstract CompilationOptions CommonWithDeterministic(bool deterministic);

	protected abstract CompilationOptions CommonWithOutputKind(OutputKind kind);

	protected abstract CompilationOptions CommonWithPlatform(Platform platform);

	protected abstract CompilationOptions CommonWithPublicSign(bool publicSign);

	protected abstract CompilationOptions CommonWithOptimizationLevel(OptimizationLevel value);

	protected abstract CompilationOptions CommonWithXmlReferenceResolver(XmlReferenceResolver resolver);

	protected abstract CompilationOptions CommonWithSourceReferenceResolver(SourceReferenceResolver resolver);

	protected abstract CompilationOptions CommonWithMetadataReferenceResolver(MetadataReferenceResolver resolver);

	protected abstract CompilationOptions CommonWithAssemblyIdentityComparer(AssemblyIdentityComparer comparer);

	protected abstract CompilationOptions CommonWithStrongNameProvider(StrongNameProvider provider);

	protected abstract CompilationOptions CommonWithGeneralDiagnosticOption(ReportDiagnostic generalDiagnosticOption);

	protected abstract CompilationOptions CommonWithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions);

	protected abstract CompilationOptions CommonWithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions);

	protected abstract CompilationOptions CommonWithReportSuppressedDiagnostics(bool reportSuppressedDiagnostics);

	protected abstract CompilationOptions CommonWithModuleName(string moduleName);

	protected abstract CompilationOptions CommonWithMainTypeName(string mainTypeName);

	protected abstract CompilationOptions CommonWithScriptClassName(string scriptClassName);

	protected abstract CompilationOptions CommonWithCryptoKeyContainer(string cryptoKeyContainer);

	protected abstract CompilationOptions CommonWithCryptoKeyFile(string cryptoKeyFile);

	protected abstract CompilationOptions CommonWithCryptoPublicKey(ImmutableArray<byte> cryptoPublicKey);

	protected abstract CompilationOptions CommonWithDelaySign(bool? delaySign);

	protected abstract CompilationOptions CommonWithCheckOverflow(bool checkOverflow);

	protected abstract CompilationOptions CommonWithMetadataImportOptions(MetadataImportOptions value);

	[Obsolete]
	protected abstract CompilationOptions CommonWithFeatures(ImmutableArray<string> features);

	internal abstract void ValidateOptions(ArrayBuilder<Diagnostic> builder);

	internal void ValidateOptions(ArrayBuilder<Diagnostic> builder, CommonMessageProvider messageProvider)
	{
		if (!CryptoPublicKey.IsEmpty)
		{
			if (CryptoKeyFile != null)
			{
				builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MutuallyExclusiveOptions, Location.None, "CryptoPublicKey", "CryptoKeyFile"));
			}
			if (CryptoKeyContainer != null)
			{
				builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MutuallyExclusiveOptions, Location.None, "CryptoPublicKey", "CryptoKeyContainer"));
			}
		}
		if (PublicSign)
		{
			if (CryptoKeyFile != null && !PathUtilities.IsAbsolute(CryptoKeyFile))
			{
				builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_OptionMustBeAbsolutePath, Location.None, "CryptoKeyFile"));
			}
			if (CryptoKeyContainer != null)
			{
				builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MutuallyExclusiveOptions, Location.None, "PublicSign", "CryptoKeyContainer"));
			}
			if (DelaySign == true)
			{
				builder.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_MutuallyExclusiveOptions, Location.None, "PublicSign", "DelaySign"));
			}
		}
	}

	public abstract override bool Equals(object obj);

	protected bool EqualsHelper(CompilationOptions other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (CheckOverflow == other.CheckOverflow && ConcurrentBuild == other.ConcurrentBuild && Deterministic == other.Deterministic && CurrentLocalTime == other.CurrentLocalTime && DebugPlusMode == other.DebugPlusMode && string.Equals(CryptoKeyContainer, other.CryptoKeyContainer, StringComparison.Ordinal) && string.Equals(CryptoKeyFile, other.CryptoKeyFile, StringComparison.Ordinal) && CryptoPublicKey.SequenceEqual(other.CryptoPublicKey) && DelaySign == other.DelaySign && GeneralDiagnosticOption == other.GeneralDiagnosticOption && string.Equals(MainTypeName, other.MainTypeName, StringComparison.Ordinal) && MetadataImportOptions == other.MetadataImportOptions && ReferencesSupersedeLowerVersions == other.ReferencesSupersedeLowerVersions && string.Equals(ModuleName, other.ModuleName, StringComparison.Ordinal) && OptimizationLevel == other.OptimizationLevel && OutputKind == other.OutputKind && Platform == other.Platform && ReportSuppressedDiagnostics == other.ReportSuppressedDiagnostics && string.Equals(ScriptClassName, other.ScriptClassName, StringComparison.Ordinal) && SpecificDiagnosticOptions.SequenceEqual(other.SpecificDiagnosticOptions, (KeyValuePair<string, ReportDiagnostic> left, KeyValuePair<string, ReportDiagnostic> right) => left.Key == right.Key && left.Value == right.Value) && WarningLevel == other.WarningLevel && object.Equals(MetadataReferenceResolver, other.MetadataReferenceResolver) && object.Equals(XmlReferenceResolver, other.XmlReferenceResolver) && object.Equals(SourceReferenceResolver, other.SourceReferenceResolver) && object.Equals(StrongNameProvider, other.StrongNameProvider) && object.Equals(AssemblyIdentityComparer, other.AssemblyIdentityComparer))
		{
			return PublicSign == other.PublicSign;
		}
		return false;
	}

	public abstract override int GetHashCode();

	protected int GetHashCodeHelper()
	{
		return Hash.Combine(CheckOverflow, Hash.Combine(ConcurrentBuild, Hash.Combine(Deterministic, Hash.Combine(CurrentLocalTime.GetHashCode(), Hash.Combine(DebugPlusMode, Hash.Combine((CryptoKeyContainer != null) ? StringComparer.Ordinal.GetHashCode(CryptoKeyContainer) : 0, Hash.Combine((CryptoKeyFile != null) ? StringComparer.Ordinal.GetHashCode(CryptoKeyFile) : 0, Hash.Combine(Hash.CombineValues(CryptoPublicKey, 16), Hash.Combine((int)GeneralDiagnosticOption, Hash.Combine((MainTypeName != null) ? StringComparer.Ordinal.GetHashCode(MainTypeName) : 0, Hash.Combine((int)MetadataImportOptions, Hash.Combine(ReferencesSupersedeLowerVersions, Hash.Combine((ModuleName != null) ? StringComparer.Ordinal.GetHashCode(ModuleName) : 0, Hash.Combine((int)OptimizationLevel, Hash.Combine((int)OutputKind, Hash.Combine((int)Platform, Hash.Combine(ReportSuppressedDiagnostics, Hash.Combine((ScriptClassName != null) ? StringComparer.Ordinal.GetHashCode(ScriptClassName) : 0, Hash.Combine(Hash.CombineValues(SpecificDiagnosticOptions), Hash.Combine(WarningLevel, Hash.Combine(MetadataReferenceResolver, Hash.Combine(XmlReferenceResolver, Hash.Combine(SourceReferenceResolver, Hash.Combine(StrongNameProvider, Hash.Combine(AssemblyIdentityComparer, Hash.Combine(PublicSign, 0))))))))))))))))))))))))));
	}

	public static bool operator ==(CompilationOptions left, CompilationOptions right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(CompilationOptions left, CompilationOptions right)
	{
		return !object.Equals(left, right);
	}
}
