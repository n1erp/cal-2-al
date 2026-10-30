namespace Microsoft.Dynamics.Nav.Runtime;

public class NavAppCSharpCompilerResult
{
	public byte[] AssemblyContent { get; }

	public byte[] PdbContent { get; }

	public string Errors { get; }

	public NavAppCSharpCompilerResult(byte[] assemblyContent, byte[] pdbContent, string errors)
	{
		AssemblyContent = assemblyContent;
		PdbContent = pdbContent;
		Errors = errors;
	}
}
