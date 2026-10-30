using System.Collections;
using System.Collections.Generic;
using System.Reflection.Internal;
using System.Reflection.Metadata.Ecma335;

namespace System.Reflection.Metadata;

public readonly struct ImportDefinitionCollection : IEnumerable<ImportDefinition>, IEnumerable
{
	public struct Enumerator : IEnumerator<ImportDefinition>, IEnumerator, IDisposable
	{
		private BlobReader _reader;

		private ImportDefinition _current;

		public ImportDefinition Current => _current;

		object IEnumerator.Current => _current;

		internal Enumerator(MemoryBlock block)
		{
			_reader = new BlobReader(block);
			_current = default;
		}

		public bool MoveNext()
		{
			if (_reader.RemainingBytes == 0)
			{
				return false;
			}
			ImportDefinitionKind importDefinitionKind = (ImportDefinitionKind)_reader.ReadByte();
			switch (importDefinitionKind)
			{
			case ImportDefinitionKind.ImportType:
				_current = new ImportDefinition(importDefinitionKind, default, default, _reader.ReadTypeHandle());
				break;
			case ImportDefinitionKind.ImportNamespace:
				_current = new ImportDefinition(importDefinitionKind, default, default, MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()));
				break;
			case ImportDefinitionKind.ImportAssemblyNamespace:
				_current = new ImportDefinition(importDefinitionKind, default, MetadataTokens.AssemblyReferenceHandle(_reader.ReadCompressedInteger()), MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()));
				break;
			case ImportDefinitionKind.ImportAssemblyReferenceAlias:
				_current = new ImportDefinition(importDefinitionKind, MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()));
				break;
			case ImportDefinitionKind.AliasAssemblyReference:
				_current = new ImportDefinition(importDefinitionKind, MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()), MetadataTokens.AssemblyReferenceHandle(_reader.ReadCompressedInteger()));
				break;
			case ImportDefinitionKind.AliasType:
				_current = new ImportDefinition(importDefinitionKind, MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()), default, _reader.ReadTypeHandle());
				break;
			case ImportDefinitionKind.ImportXmlNamespace:
			case ImportDefinitionKind.AliasNamespace:
				_current = new ImportDefinition(importDefinitionKind, MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()), default, MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()));
				break;
			case ImportDefinitionKind.AliasAssemblyNamespace:
				_current = new ImportDefinition(importDefinitionKind, MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()), MetadataTokens.AssemblyReferenceHandle(_reader.ReadCompressedInteger()), MetadataTokens.BlobHandle(_reader.ReadCompressedInteger()));
				break;
			default:
				throw new BadImageFormatException(string.Format(SR.InvalidImportDefinitionKind, new object[1] { importDefinitionKind }));
			}
			return true;
		}

		public void Reset()
		{
			_reader.Reset();
			_current = default;
		}

		void IDisposable.Dispose()
		{
		}
	}

	private readonly MemoryBlock _block;

	internal ImportDefinitionCollection(MemoryBlock block)
	{
		_block = block;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_block);
	}

	IEnumerator<ImportDefinition> IEnumerable<ImportDefinition>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
