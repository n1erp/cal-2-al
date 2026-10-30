using System.Collections.Generic;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CodeGen;

internal class Win32Resource : IWin32Resource
{
	private readonly byte[] _data;

	private readonly uint _codePage;

	private readonly uint _languageId;

	private readonly int _id;

	private readonly string _name;

	private readonly int _typeId;

	private readonly string _typeName;

	public string TypeName => _typeName;

	public int TypeId => _typeId;

	public string Name => _name;

	public int Id => _id;

	public uint LanguageId => _languageId;

	public uint CodePage => _codePage;

	public IEnumerable<byte> Data => _data;

	internal Win32Resource(byte[] data, uint codePage, uint languageId, int id, string name, int typeId, string typeName)
	{
		_data = data;
		_codePage = codePage;
		_languageId = languageId;
		_id = id;
		_name = name;
		_typeId = typeId;
		_typeName = typeName;
	}
}
