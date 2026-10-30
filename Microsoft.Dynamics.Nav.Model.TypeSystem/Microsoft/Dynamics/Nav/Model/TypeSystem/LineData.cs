using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class LineData
{
	private enum Format : byte
	{
		Unknown,
		Utf8,
		Utf8Compressed
	}

	private BlockData data;

	private int lineCount;

	private Format format;

	public int Count => lineCount;

	public void SetLines(IEnumerable<string> lines)
	{
		int num = 256;
		foreach (string line in lines)
		{
			num -= line.Length;
			if (num <= 0)
			{
				break;
			}
		}
		BlockStream blockStream = new BlockStream();
		StreamWriter streamWriter;
		if (num < 0)
		{
			format = Format.Utf8Compressed;
			streamWriter = new StreamWriter(new DeflateStream(blockStream, CompressionMode.Compress), Encoding.UTF8);
		}
		else
		{
			format = Format.Utf8;
			streamWriter = new StreamWriter(blockStream, Encoding.UTF8);
		}
		using (streamWriter)
		{
			lineCount = 0;
			foreach (string line2 in lines)
			{
				streamWriter.WriteLine(line2);
				lineCount++;
			}
			streamWriter.Flush();
			streamWriter.Close();
			data = blockStream.CloseAndGetData();
		}
	}

	public Stream GetStream()
	{
		BlockStream blockStream = new BlockStream(data);
		if (format == Format.Utf8Compressed)
		{
			return new DeflateStream(blockStream, CompressionMode.Decompress);
		}
		return blockStream;
	}

	public IEnumerable<string> GetLines()
	{
		if (lineCount <= 0)
		{
			yield break;
		}
		using StreamReader sr = new StreamReader(GetStream(), Encoding.UTF8);
		while (!sr.EndOfStream)
		{
			yield return sr.ReadLine();
		}
		sr.Close();
	}
}
