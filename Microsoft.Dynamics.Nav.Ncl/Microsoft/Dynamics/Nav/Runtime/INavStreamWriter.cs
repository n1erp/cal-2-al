namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavStreamWriter : INavStreamMode, INavStreamTextEncoding
{
	void WriteByte(byte value);

	void WriteBytes(byte[] bytes, int length);
}
