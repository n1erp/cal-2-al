namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavStreamReader : INavStreamMode, INavStreamTextEncoding
{
	int ReadByte();

	int PeekByte();

	byte[] ReadBytes(int length);

	byte[] PeekBytes(int length);

	string ReadChar();
}
