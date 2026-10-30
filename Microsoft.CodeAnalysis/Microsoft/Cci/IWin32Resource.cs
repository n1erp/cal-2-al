using System.Collections.Generic;

namespace Microsoft.Cci;

internal interface IWin32Resource
{
	string TypeName { get; }

	int TypeId { get; }

	string Name { get; }

	int Id { get; }

	uint LanguageId { get; }

	uint CodePage { get; }

	IEnumerable<byte> Data { get; }
}
