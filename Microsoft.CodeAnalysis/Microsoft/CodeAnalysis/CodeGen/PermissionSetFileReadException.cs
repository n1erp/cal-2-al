using System;

namespace Microsoft.CodeAnalysis.CodeGen;

internal class PermissionSetFileReadException : Exception
{
	private readonly string _file;

	public string FileName => _file;

	public string PropertyName => "File";

	public PermissionSetFileReadException(string message, string file)
		: base(message)
	{
		_file = file;
	}
}
