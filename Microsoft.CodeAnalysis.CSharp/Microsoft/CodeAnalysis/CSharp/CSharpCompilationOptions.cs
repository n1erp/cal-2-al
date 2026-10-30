using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public sealed class CSharpCompilationOptions : CompilationOptions, IEquatable<CSharpCompilationOptions>
{
	public bool AllowUnsafe { get; private set; }

	public ImmutableArray<string> Usings { get; private set; }

	internal BinderFlags TopLevelBinderFlags { get; private set; }

	public override string Language => "C#";

	public CSharpCompilationOptions(OutputKind outputKind, bool reportSuppressedDiagnostics = false, string moduleName = null, string mainTypeName = null, string scriptClassName = null, IEnumerable<string> usings = null, OptimizationLevel optimizationLevel = OptimizationLevel.Debug, bool checkOverflow = false, bool allowUnsafe = false, string cryptoKeyContainer = null, string cryptoKeyFile = null, ImmutableArray<byte> cryptoPublicKey = default(ImmutableArray<byte>), bool? delaySign = null, Platform platform = Platform.AnyCpu, ReportDiagnostic generalDiagnosticOption = ReportDiagnostic.Default, int warningLevel = 4, IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions = null, bool concurrentBuild = true, bool deterministic = false, XmlReferenceResolver xmlReferenceResolver = null, SourceReferenceResolver sourceReferenceResolver = null, MetadataReferenceResolver metadataReferenceResolver = null, AssemblyIdentityComparer assemblyIdentityComparer = null, StrongNameProvider strongNameProvider = null, bool publicSign = false, MetadataImportOptions metadataImportOptions = MetadataImportOptions.Public)
		: this(outputKind, reportSuppressedDiagnostics, moduleName, mainTypeName, scriptClassName, usings, optimizationLevel, checkOverflow, allowUnsafe, cryptoKeyContainer, cryptoKeyFile, cryptoPublicKey, delaySign, platform, generalDiagnosticOption, warningLevel, specificDiagnosticOptions, concurrentBuild, deterministic, default, debugPlusMode: false, xmlReferenceResolver, sourceReferenceResolver, metadataReferenceResolver, assemblyIdentityComparer, strongNameProvider, metadataImportOptions, referencesSupersedeLowerVersions: false, publicSign, BinderFlags.None)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public CSharpCompilationOptions(OutputKind outputKind, bool reportSuppressedDiagnostics, string moduleName, string mainTypeName, string scriptClassName, IEnumerable<string> usings, OptimizationLevel optimizationLevel, bool checkOverflow, bool allowUnsafe, string cryptoKeyContainer, string cryptoKeyFile, ImmutableArray<byte> cryptoPublicKey, bool? delaySign, Platform platform, ReportDiagnostic generalDiagnosticOption, int warningLevel, IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions, bool concurrentBuild, bool deterministic, XmlReferenceResolver xmlReferenceResolver, SourceReferenceResolver sourceReferenceResolver, MetadataReferenceResolver metadataReferenceResolver, AssemblyIdentityComparer assemblyIdentityComparer, StrongNameProvider strongNameProvider, bool publicSign)
		: this(outputKind, reportSuppressedDiagnostics, moduleName, mainTypeName, scriptClassName, usings, optimizationLevel, checkOverflow, allowUnsafe, cryptoKeyContainer, cryptoKeyFile, cryptoPublicKey, delaySign, platform, generalDiagnosticOption, warningLevel, specificDiagnosticOptions, concurrentBuild, deterministic, xmlReferenceResolver, sourceReferenceResolver, metadataReferenceResolver, assemblyIdentityComparer, strongNameProvider, publicSign, MetadataImportOptions.Public)
	{
	}

	internal CSharpCompilationOptions(OutputKind outputKind, bool reportSuppressedDiagnostics, string moduleName, string mainTypeName, string scriptClassName, IEnumerable<string> usings, OptimizationLevel optimizationLevel, bool checkOverflow, bool allowUnsafe, string cryptoKeyContainer, string cryptoKeyFile, ImmutableArray<byte> cryptoPublicKey, bool? delaySign, Platform platform, ReportDiagnostic generalDiagnosticOption, int warningLevel, IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions, bool concurrentBuild, bool deterministic, DateTime currentLocalTime, bool debugPlusMode, XmlReferenceResolver xmlReferenceResolver, SourceReferenceResolver sourceReferenceResolver, MetadataReferenceResolver metadataReferenceResolver, AssemblyIdentityComparer assemblyIdentityComparer, StrongNameProvider strongNameProvider, MetadataImportOptions metadataImportOptions, bool referencesSupersedeLowerVersions, bool publicSign, BinderFlags topLevelBinderFlags)
		: base(outputKind, reportSuppressedDiagnostics, moduleName, mainTypeName, scriptClassName, cryptoKeyContainer, cryptoKeyFile, cryptoPublicKey, delaySign, publicSign, optimizationLevel, checkOverflow, platform, generalDiagnosticOption, warningLevel, specificDiagnosticOptions.ToImmutableDictionaryOrEmpty(), concurrentBuild, deterministic, currentLocalTime, debugPlusMode, xmlReferenceResolver, sourceReferenceResolver, metadataReferenceResolver, assemblyIdentityComparer, strongNameProvider, metadataImportOptions, referencesSupersedeLowerVersions)
	{
		Usings = usings.AsImmutableOrEmpty();
		AllowUnsafe = allowUnsafe;
		TopLevelBinderFlags = topLevelBinderFlags;
	}

	private CSharpCompilationOptions(CSharpCompilationOptions other)
		: this(other.OutputKind, moduleName: other.ModuleName, mainTypeName: other.MainTypeName, scriptClassName: other.ScriptClassName, usings: other.Usings, optimizationLevel: other.OptimizationLevel, checkOverflow: other.CheckOverflow, allowUnsafe: other.AllowUnsafe, cryptoKeyContainer: other.CryptoKeyContainer, cryptoKeyFile: other.CryptoKeyFile, cryptoPublicKey: other.CryptoPublicKey, delaySign: other.DelaySign, platform: other.Platform, generalDiagnosticOption: other.GeneralDiagnosticOption, warningLevel: other.WarningLevel, specificDiagnosticOptions: other.SpecificDiagnosticOptions, concurrentBuild: other.ConcurrentBuild, deterministic: other.Deterministic, currentLocalTime: other.CurrentLocalTime, debugPlusMode: other.DebugPlusMode, xmlReferenceResolver: other.XmlReferenceResolver, sourceReferenceResolver: other.SourceReferenceResolver, metadataReferenceResolver: other.MetadataReferenceResolver, assemblyIdentityComparer: other.AssemblyIdentityComparer, strongNameProvider: other.StrongNameProvider, metadataImportOptions: other.MetadataImportOptions, referencesSupersedeLowerVersions: other.ReferencesSupersedeLowerVersions, reportSuppressedDiagnostics: other.ReportSuppressedDiagnostics, publicSign: other.PublicSign, topLevelBinderFlags: other.TopLevelBinderFlags)
	{
	}

	internal CSharpCompilationOptions WithTopLevelBinderFlags(BinderFlags flags)
	{
		if (flags != TopLevelBinderFlags)
		{
			return new CSharpCompilationOptions(this)
			{
				TopLevelBinderFlags = flags
			};
		}
		return this;
	}

	internal override ImmutableArray<string> GetImports()
	{
		return Usings;
	}

	public new CSharpCompilationOptions WithOutputKind(OutputKind kind)
	{
		if (kind == OutputKind)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			OutputKind = kind
		};
	}

	public new CSharpCompilationOptions WithModuleName(string moduleName)
	{
		if (moduleName == ModuleName)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			ModuleName = moduleName
		};
	}

	public new CSharpCompilationOptions WithScriptClassName(string name)
	{
		if (name == ScriptClassName)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			ScriptClassName = name
		};
	}

	public new CSharpCompilationOptions WithMainTypeName(string name)
	{
		if (name == MainTypeName)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			MainTypeName = name
		};
	}

	public new CSharpCompilationOptions WithCryptoKeyContainer(string name)
	{
		if (name == CryptoKeyContainer)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			CryptoKeyContainer = name
		};
	}

	public new CSharpCompilationOptions WithCryptoKeyFile(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			path = null;
		}
		if (path == CryptoKeyFile)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			CryptoKeyFile = path
		};
	}

	public new CSharpCompilationOptions WithCryptoPublicKey(ImmutableArray<byte> value)
	{
		if (value.IsDefault)
		{
			value = ImmutableArray<byte>.Empty;
		}
		if (value == CryptoPublicKey)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			CryptoPublicKey = value
		};
	}

	public new CSharpCompilationOptions WithDelaySign(bool? value)
	{
		if (value == DelaySign)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			DelaySign = value
		};
	}

	public CSharpCompilationOptions WithUsings(ImmutableArray<string> usings)
	{
		if (Usings == usings)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			Usings = usings
		};
	}

	public CSharpCompilationOptions WithUsings(IEnumerable<string> usings)
	{
		return new CSharpCompilationOptions(this)
		{
			Usings = usings.AsImmutableOrEmpty()
		};
	}

	public CSharpCompilationOptions WithUsings(params string[] usings)
	{
		return WithUsings((IEnumerable<string>)usings);
	}

	public new CSharpCompilationOptions WithOptimizationLevel(OptimizationLevel value)
	{
		if (value == OptimizationLevel)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			OptimizationLevel = value
		};
	}

	public new CSharpCompilationOptions WithOverflowChecks(bool enabled)
	{
		if (enabled == CheckOverflow)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			CheckOverflow = enabled
		};
	}

	public CSharpCompilationOptions WithAllowUnsafe(bool enabled)
	{
		if (enabled == AllowUnsafe)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			AllowUnsafe = enabled
		};
	}

	public new CSharpCompilationOptions WithPlatform(Platform platform)
	{
		if (Platform == platform)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			Platform = platform
		};
	}

	public new CSharpCompilationOptions WithPublicSign(bool publicSign)
	{
		if (PublicSign == publicSign)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			PublicSign = publicSign
		};
	}

	protected override CompilationOptions CommonWithGeneralDiagnosticOption(ReportDiagnostic value)
	{
		return WithGeneralDiagnosticOption(value);
	}

	protected override CompilationOptions CommonWithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions)
	{
		return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
	}

	protected override CompilationOptions CommonWithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions)
	{
		return WithSpecificDiagnosticOptions(specificDiagnosticOptions);
	}

	protected override CompilationOptions CommonWithReportSuppressedDiagnostics(bool reportSuppressedDiagnostics)
	{
		return WithReportSuppressedDiagnostics(reportSuppressedDiagnostics);
	}

	public new CSharpCompilationOptions WithGeneralDiagnosticOption(ReportDiagnostic value)
	{
		if (GeneralDiagnosticOption == value)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			GeneralDiagnosticOption = value
		};
	}

	public new CSharpCompilationOptions WithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> values)
	{
		if (values == null)
		{
			values = ImmutableDictionary<string, ReportDiagnostic>.Empty;
		}
		if (SpecificDiagnosticOptions == values)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			SpecificDiagnosticOptions = values
		};
	}

	public new CSharpCompilationOptions WithSpecificDiagnosticOptions(IEnumerable<KeyValuePair<string, ReportDiagnostic>> values)
	{
		return new CSharpCompilationOptions(this)
		{
			SpecificDiagnosticOptions = values.ToImmutableDictionaryOrEmpty()
		};
	}

	public new CSharpCompilationOptions WithReportSuppressedDiagnostics(bool reportSuppressedDiagnostics)
	{
		if (reportSuppressedDiagnostics == ReportSuppressedDiagnostics)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			ReportSuppressedDiagnostics = reportSuppressedDiagnostics
		};
	}

	public CSharpCompilationOptions WithWarningLevel(int warningLevel)
	{
		if (warningLevel == WarningLevel)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			WarningLevel = warningLevel
		};
	}

	public new CSharpCompilationOptions WithConcurrentBuild(bool concurrentBuild)
	{
		if (concurrentBuild == ConcurrentBuild)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			ConcurrentBuild = concurrentBuild
		};
	}

	public new CSharpCompilationOptions WithDeterministic(bool deterministic)
	{
		if (deterministic == Deterministic)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			Deterministic = deterministic
		};
	}

	internal CSharpCompilationOptions WithCurrentLocalTime(DateTime value)
	{
		if (value == CurrentLocalTime)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			CurrentLocalTime_internal_protected_set = value
		};
	}

	internal CSharpCompilationOptions WithDebugPlusMode(bool debugPlusMode)
	{
		if (debugPlusMode == DebugPlusMode)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			DebugPlusMode_internal_protected_set = debugPlusMode
		};
	}

	public new CSharpCompilationOptions WithMetadataImportOptions(MetadataImportOptions value)
	{
		if (value == MetadataImportOptions)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			MetadataImportOptions = value
		};
	}

	internal CSharpCompilationOptions WithReferencesSupersedeLowerVersions(bool value)
	{
		if (value == ReferencesSupersedeLowerVersions)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			ReferencesSupersedeLowerVersions_internal_protected_set = value
		};
	}

	public new CSharpCompilationOptions WithXmlReferenceResolver(XmlReferenceResolver resolver)
	{
		if (resolver == XmlReferenceResolver)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			XmlReferenceResolver = resolver
		};
	}

	public new CSharpCompilationOptions WithSourceReferenceResolver(SourceReferenceResolver resolver)
	{
		if (resolver == SourceReferenceResolver)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			SourceReferenceResolver = resolver
		};
	}

	public new CSharpCompilationOptions WithMetadataReferenceResolver(MetadataReferenceResolver resolver)
	{
		if (resolver == MetadataReferenceResolver)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			MetadataReferenceResolver = resolver
		};
	}

	public new CSharpCompilationOptions WithAssemblyIdentityComparer(AssemblyIdentityComparer comparer)
	{
		comparer = comparer ?? AssemblyIdentityComparer.Default;
		if (comparer == AssemblyIdentityComparer)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			AssemblyIdentityComparer = comparer
		};
	}

	public new CSharpCompilationOptions WithStrongNameProvider(StrongNameProvider provider)
	{
		if (provider == StrongNameProvider)
		{
			return this;
		}
		return new CSharpCompilationOptions(this)
		{
			StrongNameProvider = provider
		};
	}

	protected override CompilationOptions CommonWithConcurrentBuild(bool concurrent)
	{
		return WithConcurrentBuild(concurrent);
	}

	protected override CompilationOptions CommonWithDeterministic(bool deterministic)
	{
		return WithDeterministic(deterministic);
	}

	protected override CompilationOptions CommonWithOutputKind(OutputKind kind)
	{
		return WithOutputKind(kind);
	}

	protected override CompilationOptions CommonWithPlatform(Platform platform)
	{
		return WithPlatform(platform);
	}

	protected override CompilationOptions CommonWithPublicSign(bool publicSign)
	{
		return WithPublicSign(publicSign);
	}

	protected override CompilationOptions CommonWithOptimizationLevel(OptimizationLevel value)
	{
		return WithOptimizationLevel(value);
	}

	protected override CompilationOptions CommonWithAssemblyIdentityComparer(AssemblyIdentityComparer comparer)
	{
		return WithAssemblyIdentityComparer(comparer);
	}

	protected override CompilationOptions CommonWithXmlReferenceResolver(XmlReferenceResolver resolver)
	{
		return WithXmlReferenceResolver(resolver);
	}

	protected override CompilationOptions CommonWithSourceReferenceResolver(SourceReferenceResolver resolver)
	{
		return WithSourceReferenceResolver(resolver);
	}

	protected override CompilationOptions CommonWithMetadataReferenceResolver(MetadataReferenceResolver resolver)
	{
		return WithMetadataReferenceResolver(resolver);
	}

	protected override CompilationOptions CommonWithStrongNameProvider(StrongNameProvider provider)
	{
		return WithStrongNameProvider(provider);
	}

	protected override CompilationOptions CommonWithMetadataImportOptions(MetadataImportOptions value)
	{
		return WithMetadataImportOptions(value);
	}

	[Obsolete]
	protected override CompilationOptions CommonWithFeatures(ImmutableArray<string> features)
	{
		throw new NotImplementedException();
	}

	internal override void ValidateOptions(ArrayBuilder<Diagnostic> builder)
	{
		ValidateOptions(builder, MessageProvider.Instance);
		if (MainTypeName != null)
		{
			if (OutputKind.IsValid() && !OutputKind.IsApplication())
			{
				builder.Add(Diagnostic.Create(MessageProvider.Instance, 2017));
			}
			if (!MainTypeName.IsValidClrTypeName())
			{
				builder.Add(Diagnostic.Create(MessageProvider.Instance, 7088, "MainTypeName", MainTypeName));
			}
		}
		if (!Platform.IsValid())
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 1672, Platform.ToString()));
		}
		if (ModuleName != null)
		{
			MetadataHelpers.CheckAssemblyOrModuleName(ModuleName, MessageProvider.Instance, 7087, builder);
		}
		if (!OutputKind.IsValid())
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 7088, "OutputKind", OutputKind.ToString()));
		}
		if (!OptimizationLevel.IsValid())
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 7088, "OptimizationLevel", OptimizationLevel.ToString()));
		}
		if (ScriptClassName == null || !ScriptClassName.IsValidClrTypeName())
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 7088, "ScriptClassName", ScriptClassName ?? "null"));
		}
		if (WarningLevel < 0 || WarningLevel > 4)
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 7088, "WarningLevel", WarningLevel));
		}
		if (Usings != null && Usings.Any((string u) => !u.IsValidClrNamespaceName()))
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 7088, "Usings", Usings.Where((string u) => !u.IsValidClrNamespaceName()).First() ?? "null"));
		}
		if (Platform == Platform.AnyCpu32BitPreferred && OutputKind.IsValid() && OutputKind != OutputKind.ConsoleApplication && OutputKind != OutputKind.WindowsApplication && OutputKind != OutputKind.WindowsRuntimeApplication)
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 4023));
		}
		if (!MetadataImportOptions.IsValid())
		{
			builder.Add(Diagnostic.Create(MessageProvider.Instance, 7088, "MetadataImportOptions", MetadataImportOptions.ToString()));
		}
	}

	public bool Equals(CSharpCompilationOptions other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (!EqualsHelper(other))
		{
			return false;
		}
		if (AllowUnsafe == other.AllowUnsafe && TopLevelBinderFlags == other.TopLevelBinderFlags)
		{
			if (!(Usings == null))
			{
				return Usings.SequenceEqual(other.Usings, StringComparer.Ordinal);
			}
			return other.Usings == null;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as CSharpCompilationOptions);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(GetHashCodeHelper(), Hash.Combine(AllowUnsafe, Hash.Combine(Hash.CombineValues(Usings, StringComparer.Ordinal), Hash.Combine(TopLevelBinderFlags.GetHashCode(), 0))));
	}

	internal override Diagnostic FilterDiagnostic(Diagnostic diagnostic)
	{
		return CSharpDiagnosticFilter.Filter(diagnostic, WarningLevel, GeneralDiagnosticOption, SpecificDiagnosticOptions);
	}

	protected override CompilationOptions CommonWithModuleName(string moduleName)
	{
		return WithModuleName(moduleName);
	}

	protected override CompilationOptions CommonWithMainTypeName(string mainTypeName)
	{
		return WithMainTypeName(mainTypeName);
	}

	protected override CompilationOptions CommonWithScriptClassName(string scriptClassName)
	{
		return WithScriptClassName(scriptClassName);
	}

	protected override CompilationOptions CommonWithCryptoKeyContainer(string cryptoKeyContainer)
	{
		return WithCryptoKeyContainer(cryptoKeyContainer);
	}

	protected override CompilationOptions CommonWithCryptoKeyFile(string cryptoKeyFile)
	{
		return WithCryptoKeyFile(cryptoKeyFile);
	}

	protected override CompilationOptions CommonWithCryptoPublicKey(ImmutableArray<byte> cryptoPublicKey)
	{
		return WithCryptoPublicKey(cryptoPublicKey);
	}

	protected override CompilationOptions CommonWithDelaySign(bool? delaySign)
	{
		return WithDelaySign(delaySign);
	}

	protected override CompilationOptions CommonWithCheckOverflow(bool checkOverflow)
	{
		return WithOverflowChecks(checkOverflow);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public CSharpCompilationOptions(OutputKind outputKind, string moduleName, string mainTypeName, string scriptClassName, IEnumerable<string> usings, OptimizationLevel optimizationLevel, bool checkOverflow, bool allowUnsafe, string cryptoKeyContainer, string cryptoKeyFile, ImmutableArray<byte> cryptoPublicKey, bool? delaySign, Platform platform, ReportDiagnostic generalDiagnosticOption, int warningLevel, IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions, bool concurrentBuild, bool deterministic, XmlReferenceResolver xmlReferenceResolver, SourceReferenceResolver sourceReferenceResolver, MetadataReferenceResolver metadataReferenceResolver, AssemblyIdentityComparer assemblyIdentityComparer, StrongNameProvider strongNameProvider)
		: this(outputKind, reportSuppressedDiagnostics: false, moduleName, mainTypeName, scriptClassName, usings, optimizationLevel, checkOverflow, allowUnsafe, cryptoKeyContainer, cryptoKeyFile, cryptoPublicKey, delaySign, platform, generalDiagnosticOption, warningLevel, specificDiagnosticOptions, concurrentBuild, deterministic, xmlReferenceResolver, sourceReferenceResolver, metadataReferenceResolver, assemblyIdentityComparer, strongNameProvider, publicSign: false)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public CSharpCompilationOptions(OutputKind outputKind, string moduleName, string mainTypeName, string scriptClassName, IEnumerable<string> usings, OptimizationLevel optimizationLevel, bool checkOverflow, bool allowUnsafe, string cryptoKeyContainer, string cryptoKeyFile, ImmutableArray<byte> cryptoPublicKey, bool? delaySign, Platform platform, ReportDiagnostic generalDiagnosticOption, int warningLevel, IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions, bool concurrentBuild, XmlReferenceResolver xmlReferenceResolver, SourceReferenceResolver sourceReferenceResolver, MetadataReferenceResolver metadataReferenceResolver, AssemblyIdentityComparer assemblyIdentityComparer, StrongNameProvider strongNameProvider)
		: this(outputKind, moduleName, mainTypeName, scriptClassName, usings, optimizationLevel, checkOverflow, allowUnsafe, cryptoKeyContainer, cryptoKeyFile, cryptoPublicKey, delaySign, platform, generalDiagnosticOption, warningLevel, specificDiagnosticOptions, concurrentBuild, deterministic: false, xmlReferenceResolver, sourceReferenceResolver, metadataReferenceResolver, assemblyIdentityComparer, strongNameProvider)
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public CSharpCompilationOptions(OutputKind outputKind, bool reportSuppressedDiagnostics, string moduleName, string mainTypeName, string scriptClassName, IEnumerable<string> usings, OptimizationLevel optimizationLevel, bool checkOverflow, bool allowUnsafe, string cryptoKeyContainer, string cryptoKeyFile, ImmutableArray<byte> cryptoPublicKey, bool? delaySign, Platform platform, ReportDiagnostic generalDiagnosticOption, int warningLevel, IEnumerable<KeyValuePair<string, ReportDiagnostic>> specificDiagnosticOptions, bool concurrentBuild, bool deterministic, XmlReferenceResolver xmlReferenceResolver, SourceReferenceResolver sourceReferenceResolver, MetadataReferenceResolver metadataReferenceResolver, AssemblyIdentityComparer assemblyIdentityComparer, StrongNameProvider strongNameProvider)
		: this(outputKind, reportSuppressedDiagnostics: false, moduleName, mainTypeName, scriptClassName, usings, optimizationLevel, checkOverflow, allowUnsafe, cryptoKeyContainer, cryptoKeyFile, cryptoPublicKey, delaySign, platform, generalDiagnosticOption, warningLevel, specificDiagnosticOptions, concurrentBuild, deterministic, default, debugPlusMode: false, xmlReferenceResolver, sourceReferenceResolver, metadataReferenceResolver, assemblyIdentityComparer, strongNameProvider, MetadataImportOptions.Public, referencesSupersedeLowerVersions: false, publicSign: false, BinderFlags.None)
	{
	}
}
