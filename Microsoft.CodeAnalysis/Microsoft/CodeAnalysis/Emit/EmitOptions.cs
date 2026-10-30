using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

public sealed class EmitOptions : IEquatable<EmitOptions>
{
	internal static readonly EmitOptions Default = (PlatformInformation.IsWindows ? new EmitOptions(metadataOnly: false, (DebugInformationFormat)0, null, null, 0, 0uL) : new EmitOptions(metadataOnly: false, (DebugInformationFormat)0, null, null, 0, 0uL).WithDebugInformationFormat(DebugInformationFormat.PortablePdb));

	public bool EmitMetadataOnly { get; private set; }

	public bool TolerateErrors { get; private set; }

	public bool IncludePrivateMembers { get; private set; }

	public ImmutableArray<InstrumentationKind> InstrumentationKinds { get; private set; }

	public SubsystemVersion SubsystemVersion { get; private set; }

	public int FileAlignment { get; private set; }

	public bool HighEntropyVirtualAddressSpace { get; private set; }

	public ulong BaseAddress { get; private set; }

	public DebugInformationFormat DebugInformationFormat { get; private set; }

	public string OutputNameOverride { get; private set; }

	public string PdbFilePath { get; private set; }

	public HashAlgorithmName PdbChecksumAlgorithm { get; private set; }

	public string RuntimeMetadataVersion { get; private set; }

	internal bool EmitTestCoverageData => InstrumentationKinds.Contains(InstrumentationKind.TestCoverage);

	public EmitOptions(bool metadataOnly, DebugInformationFormat debugInformationFormat, string pdbFilePath, string outputNameOverride, int fileAlignment, ulong baseAddress, bool highEntropyVirtualAddressSpace, SubsystemVersion subsystemVersion, string runtimeMetadataVersion, bool tolerateErrors, bool includePrivateMembers)
		: this(metadataOnly, debugInformationFormat, pdbFilePath, outputNameOverride, fileAlignment, baseAddress, highEntropyVirtualAddressSpace, subsystemVersion, runtimeMetadataVersion, tolerateErrors, includePrivateMembers, ImmutableArray<InstrumentationKind>.Empty)
	{
	}

	public EmitOptions(bool metadataOnly, DebugInformationFormat debugInformationFormat, string pdbFilePath, string outputNameOverride, int fileAlignment, ulong baseAddress, bool highEntropyVirtualAddressSpace, SubsystemVersion subsystemVersion, string runtimeMetadataVersion, bool tolerateErrors, bool includePrivateMembers, ImmutableArray<InstrumentationKind> instrumentationKinds)
		: this(metadataOnly, debugInformationFormat, pdbFilePath, outputNameOverride, fileAlignment, baseAddress, highEntropyVirtualAddressSpace, subsystemVersion, runtimeMetadataVersion, tolerateErrors, includePrivateMembers, instrumentationKinds, null)
	{
	}

	public EmitOptions(bool metadataOnly = false, DebugInformationFormat debugInformationFormat = (DebugInformationFormat)0, string pdbFilePath = null, string outputNameOverride = null, int fileAlignment = 0, ulong baseAddress = 0uL, bool highEntropyVirtualAddressSpace = false, SubsystemVersion subsystemVersion = default(SubsystemVersion), string runtimeMetadataVersion = null, bool tolerateErrors = false, bool includePrivateMembers = true, ImmutableArray<InstrumentationKind> instrumentationKinds = default(ImmutableArray<InstrumentationKind>), HashAlgorithmName? pdbChecksumAlgorithm = null)
	{
		EmitMetadataOnly = metadataOnly;
		DebugInformationFormat = ((debugInformationFormat == 0) ? DebugInformationFormat.Pdb : debugInformationFormat);
		PdbFilePath = pdbFilePath;
		OutputNameOverride = outputNameOverride;
		FileAlignment = fileAlignment;
		BaseAddress = baseAddress;
		HighEntropyVirtualAddressSpace = highEntropyVirtualAddressSpace;
		SubsystemVersion = subsystemVersion;
		RuntimeMetadataVersion = runtimeMetadataVersion;
		TolerateErrors = tolerateErrors;
		IncludePrivateMembers = includePrivateMembers;
		InstrumentationKinds = instrumentationKinds.NullToEmpty();
		PdbChecksumAlgorithm = pdbChecksumAlgorithm ?? HashAlgorithmName.SHA256;
	}

	private EmitOptions(EmitOptions other)
		: this(other.EmitMetadataOnly, other.DebugInformationFormat, other.PdbFilePath, other.OutputNameOverride, other.FileAlignment, other.BaseAddress, other.HighEntropyVirtualAddressSpace, other.SubsystemVersion, other.RuntimeMetadataVersion, other.TolerateErrors, other.IncludePrivateMembers, other.InstrumentationKinds, other.PdbChecksumAlgorithm)
	{
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as EmitOptions);
	}

	public bool Equals(EmitOptions other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (EmitMetadataOnly == other.EmitMetadataOnly && BaseAddress == other.BaseAddress && FileAlignment == other.FileAlignment && HighEntropyVirtualAddressSpace == other.HighEntropyVirtualAddressSpace && SubsystemVersion.Equals(other.SubsystemVersion) && DebugInformationFormat == other.DebugInformationFormat && PdbFilePath == other.PdbFilePath && PdbChecksumAlgorithm == other.PdbChecksumAlgorithm && OutputNameOverride == other.OutputNameOverride && RuntimeMetadataVersion == other.RuntimeMetadataVersion && TolerateErrors == other.TolerateErrors && IncludePrivateMembers == other.IncludePrivateMembers)
		{
			return InstrumentationKinds.NullToEmpty().SequenceEqual(other.InstrumentationKinds.NullToEmpty(), (InstrumentationKind a, InstrumentationKind b) => a == b);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(EmitMetadataOnly, Hash.Combine(BaseAddress.GetHashCode(), Hash.Combine(FileAlignment, Hash.Combine(HighEntropyVirtualAddressSpace, Hash.Combine(SubsystemVersion.GetHashCode(), Hash.Combine((int)DebugInformationFormat, Hash.Combine(PdbFilePath, Hash.Combine(PdbChecksumAlgorithm.GetHashCode(), Hash.Combine(OutputNameOverride, Hash.Combine(RuntimeMetadataVersion, Hash.Combine(TolerateErrors, Hash.Combine(IncludePrivateMembers, Hash.Combine(Hash.CombineValues(InstrumentationKinds), 0)))))))))))));
	}

	public static bool operator ==(EmitOptions left, EmitOptions right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(EmitOptions left, EmitOptions right)
	{
		return !object.Equals(left, right);
	}

	internal void ValidateOptions(DiagnosticBag diagnostics, CommonMessageProvider messageProvider, bool isDeterministic)
	{
		if (!DebugInformationFormat.IsValid())
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidDebugInformationFormat, Location.None, (int)DebugInformationFormat));
		}
		foreach (InstrumentationKind instrumentationKind in InstrumentationKinds)
		{
			if (!instrumentationKind.IsValid())
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidInstrumentationKind, Location.None, (int)instrumentationKind));
			}
		}
		if (OutputNameOverride != null)
		{
			MetadataHelpers.CheckAssemblyOrModuleName(OutputNameOverride, messageProvider, messageProvider.ERR_InvalidOutputName, diagnostics);
		}
		if (FileAlignment != 0 && !IsValidFileAlignment(FileAlignment))
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidFileAlignment, Location.None, FileAlignment));
		}
		if (!SubsystemVersion.Equals(SubsystemVersion.None) && !SubsystemVersion.IsValid)
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidSubsystemVersion, Location.None, SubsystemVersion.ToString()));
		}
		if (PdbChecksumAlgorithm.Name != null)
		{
			try
			{
				IncrementalHash.CreateHash(PdbChecksumAlgorithm).Dispose();
			}
			catch
			{
				diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidHashAlgorithmName, Location.None, PdbChecksumAlgorithm.ToString()));
			}
		}
		else if (isDeterministic)
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.ERR_InvalidHashAlgorithmName, Location.None, ""));
		}
		if (PdbFilePath != null && !PathUtilities.IsValidFilePath(PdbFilePath))
		{
			diagnostics.Add(messageProvider.CreateDiagnostic(messageProvider.FTL_InvalidInputFileName, Location.None, PdbFilePath));
		}
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

	public EmitOptions WithEmitMetadataOnly(bool value)
	{
		if (EmitMetadataOnly == value)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			EmitMetadataOnly = value
		};
	}

	public EmitOptions WithPdbFilePath(string path)
	{
		if (PdbFilePath == path)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			PdbFilePath = path
		};
	}

	public EmitOptions WithPdbChecksumAlgorithm(HashAlgorithmName name)
	{
		if (PdbChecksumAlgorithm == name)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			PdbChecksumAlgorithm = name
		};
	}

	public EmitOptions WithOutputNameOverride(string outputName)
	{
		if (OutputNameOverride == outputName)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			OutputNameOverride = outputName
		};
	}

	public EmitOptions WithDebugInformationFormat(DebugInformationFormat format)
	{
		if (DebugInformationFormat == format)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			DebugInformationFormat = format
		};
	}

	public EmitOptions WithFileAlignment(int value)
	{
		if (FileAlignment == value)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			FileAlignment = value
		};
	}

	public EmitOptions WithBaseAddress(ulong value)
	{
		if (BaseAddress == value)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			BaseAddress = value
		};
	}

	public EmitOptions WithHighEntropyVirtualAddressSpace(bool value)
	{
		if (HighEntropyVirtualAddressSpace == value)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			HighEntropyVirtualAddressSpace = value
		};
	}

	public EmitOptions WithSubsystemVersion(SubsystemVersion subsystemVersion)
	{
		if (subsystemVersion.Equals(SubsystemVersion))
		{
			return this;
		}
		return new EmitOptions(this)
		{
			SubsystemVersion = subsystemVersion
		};
	}

	public EmitOptions WithRuntimeMetadataVersion(string version)
	{
		if (RuntimeMetadataVersion == version)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			RuntimeMetadataVersion = version
		};
	}

	public EmitOptions WithTolerateErrors(bool value)
	{
		if (TolerateErrors == value)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			TolerateErrors = value
		};
	}

	public EmitOptions WithIncludePrivateMembers(bool value)
	{
		if (IncludePrivateMembers == value)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			IncludePrivateMembers = value
		};
	}

	public EmitOptions WithInstrumentationKinds(ImmutableArray<InstrumentationKind> instrumentationKinds)
	{
		if (InstrumentationKinds == instrumentationKinds)
		{
			return this;
		}
		return new EmitOptions(this)
		{
			InstrumentationKinds = instrumentationKinds
		};
	}
}
