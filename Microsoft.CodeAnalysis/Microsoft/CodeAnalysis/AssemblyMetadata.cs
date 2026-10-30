using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public sealed class AssemblyMetadata : Metadata
{
	private sealed class Data
	{
		public static readonly Data Disposed = new Data();

		public readonly ImmutableArray<ModuleMetadata> Modules;

		public readonly PEAssembly Assembly;

		public bool IsDisposed => Assembly == null;

		private Data()
		{
		}

		public Data(ImmutableArray<ModuleMetadata> modules, PEAssembly assembly)
		{
			Modules = modules;
			Assembly = assembly;
		}
	}

	private readonly Func<string, ModuleMetadata> _moduleFactoryOpt;

	private readonly ImmutableArray<ModuleMetadata> _initialModules;

	private Data _lazyData;

	private ImmutableArray<ModuleMetadata> _lazyPublishedModules;

	internal readonly WeakList<IAssemblySymbol> CachedSymbols = new WeakList<IAssemblySymbol>();

	public override MetadataImageKind Kind => MetadataImageKind.Assembly;

	private AssemblyMetadata(AssemblyMetadata other)
		: base(isImageOwner: false, other.Id)
	{
		CachedSymbols = other.CachedSymbols;
		_lazyData = other._lazyData;
		_moduleFactoryOpt = other._moduleFactoryOpt;
		_initialModules = other._initialModules;
	}

	internal AssemblyMetadata(ImmutableArray<ModuleMetadata> modules)
		: base(isImageOwner: true, MetadataId.CreateNewId())
	{
		_initialModules = modules;
	}

	internal AssemblyMetadata(ModuleMetadata manifestModule, Func<string, ModuleMetadata> moduleFactory)
		: base(isImageOwner: true, MetadataId.CreateNewId())
	{
		_initialModules = ImmutableArray.Create(manifestModule);
		_moduleFactoryOpt = moduleFactory;
	}

	public static AssemblyMetadata CreateFromImage(ImmutableArray<byte> peImage)
	{
		return Create(ModuleMetadata.CreateFromImage(peImage));
	}

	public static AssemblyMetadata CreateFromImage(IEnumerable<byte> peImage)
	{
		return Create(ModuleMetadata.CreateFromImage(peImage));
	}

	public static AssemblyMetadata CreateFromStream(Stream peStream, bool leaveOpen = false)
	{
		return Create(ModuleMetadata.CreateFromStream(peStream, leaveOpen));
	}

	public static AssemblyMetadata CreateFromStream(Stream peStream, PEStreamOptions options)
	{
		return Create(ModuleMetadata.CreateFromStream(peStream, options));
	}

	public static AssemblyMetadata CreateFromFile(string path)
	{
		return CreateFromFile(ModuleMetadata.CreateFromFile(path), path);
	}

	internal static AssemblyMetadata CreateFromFile(ModuleMetadata manifestModule, string path)
	{
		return new AssemblyMetadata(manifestModule, (string moduleName) => ModuleMetadata.CreateFromFile(Path.Combine(Path.GetDirectoryName(path), moduleName)));
	}

	public static AssemblyMetadata Create(ModuleMetadata module)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		return new AssemblyMetadata(ImmutableArray.Create(module));
	}

	public static AssemblyMetadata Create(ImmutableArray<ModuleMetadata> modules)
	{
		if (modules.IsDefaultOrEmpty)
		{
			throw new ArgumentException(CodeAnalysisResources.AssemblyMustHaveAtLeastOneModule, "modules");
		}
		for (int i = 0; i < modules.Length; i++)
		{
			if (modules[i] == null)
			{
				throw new ArgumentNullException("modules[" + i + "]");
			}
			if (!modules[i].IsImageOwner)
			{
				throw new ArgumentException(CodeAnalysisResources.ModuleCopyCannotBeUsedToCreateAssemblyMetadata, "modules[" + i + "]");
			}
		}
		return new AssemblyMetadata(modules);
	}

	public static AssemblyMetadata Create(IEnumerable<ModuleMetadata> modules)
	{
		return Create(modules.AsImmutableOrNull());
	}

	public static AssemblyMetadata Create(params ModuleMetadata[] modules)
	{
		return Create(ImmutableArray.CreateRange(modules));
	}

	internal new AssemblyMetadata Copy()
	{
		return new AssemblyMetadata(this);
	}

	protected override Metadata CommonCopy()
	{
		return Copy();
	}

	public ImmutableArray<ModuleMetadata> GetModules()
	{
		if (_lazyPublishedModules.IsDefault)
		{
			ImmutableArray<ModuleMetadata> immutableArray = GetOrCreateData().Modules;
			if (!IsImageOwner)
			{
				immutableArray = immutableArray.SelectAsArray((ModuleMetadata module) => module.Copy());
			}
			ImmutableInterlocked.InterlockedInitialize(ref _lazyPublishedModules, immutableArray);
		}
		if (_lazyData == Data.Disposed)
		{
			throw new ObjectDisposedException("AssemblyMetadata");
		}
		return _lazyPublishedModules;
	}

	internal PEAssembly GetAssembly()
	{
		return GetOrCreateData().Assembly;
	}

	private Data GetOrCreateData()
	{
		if (_lazyData == null)
		{
			ImmutableArray<ModuleMetadata> immutableArray = _initialModules;
			ImmutableArray<ModuleMetadata>.Builder builder = null;
			bool flag = false;
			try
			{
				if (_moduleFactoryOpt != null)
				{
					ImmutableArray<string> moduleNames = _initialModules[0].GetModuleNames();
					if (moduleNames.Length > 0)
					{
						builder = ImmutableArray.CreateBuilder<ModuleMetadata>(1 + moduleNames.Length);
						builder.Add(_initialModules[0]);
						foreach (string item in moduleNames)
						{
							builder.Add(_moduleFactoryOpt(item));
						}
						immutableArray = builder.ToImmutable();
					}
				}
				PEAssembly assembly = new PEAssembly(this, immutableArray.SelectAsArray((ModuleMetadata m) => m.Module));
				Data value = new Data(immutableArray, assembly);
				flag = Interlocked.CompareExchange(ref _lazyData, value, null) == null;
			}
			finally
			{
				if (builder != null && !flag)
				{
					for (int num = _initialModules.Length; num < builder.Count; num++)
					{
						builder[num].Dispose();
					}
				}
			}
		}
		if (_lazyData.IsDisposed)
		{
			throw new ObjectDisposedException("AssemblyMetadata");
		}
		return _lazyData;
	}

	public override void Dispose()
	{
		Data data = Interlocked.Exchange(ref _lazyData, Data.Disposed);
		if (data == Data.Disposed || !IsImageOwner)
		{
			return;
		}
		foreach (ModuleMetadata initialModule in _initialModules)
		{
			initialModule.Dispose();
		}
		if (data != null)
		{
			for (int i = _initialModules.Length; i < data.Modules.Length; i++)
			{
				data.Modules[i].Dispose();
			}
		}
	}

	internal bool IsValidAssembly()
	{
		ImmutableArray<ModuleMetadata> modules = GetModules();
		if (!modules[0].Module.IsManifestModule)
		{
			return false;
		}
		for (int i = 1; i < modules.Length; i++)
		{
			PEModule module = modules[i].Module;
			if (!module.IsLinkedModule && module.MetadataReader.MetadataKind != MetadataKind.WindowsMetadata)
			{
				return false;
			}
		}
		return true;
	}

	public PortableExecutableReference GetReference(DocumentationProvider documentation = null, ImmutableArray<string> aliases = default(ImmutableArray<string>), bool embedInteropTypes = false, string filePath = null, string display = null)
	{
		return new MetadataImageReference(this, new MetadataReferenceProperties(MetadataImageKind.Assembly, aliases, embedInteropTypes), documentation, filePath, display);
	}
}
