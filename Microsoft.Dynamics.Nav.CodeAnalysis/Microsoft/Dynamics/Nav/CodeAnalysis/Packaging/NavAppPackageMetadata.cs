using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal abstract class NavAppPackageMetadata
{
	private static readonly int GuidBytesLength = Guid.Empty.ToByteArray().Length;

	protected const uint MetadataSignature = 1482047822u;

	public abstract uint MetadataSize { get; }

	public abstract uint MetadataVersion { get; }

	public Guid PackageId { get; set; }

	public long ContentLength { get; set; }

	[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
	public static bool TryParse(Stream stream, out NavAppPackageMetadata metadata)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		bool flag = false;
		uint num = 0u;
		uint num2 = 0u;
		metadata = null;
		if (16 <= stream.Length)
		{
			long position = stream.Position;
			try
			{
				stream.Seek(0L, SeekOrigin.Begin);
				using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
				{
					uint num3 = binaryReader.ReadUInt32();
					if (1482047822 == num3)
					{
						uint num4 = binaryReader.ReadUInt32();
						uint num5 = binaryReader.ReadUInt32();
						if (num4 <= stream.Length)
						{
							stream.Seek(num4 - 4, SeekOrigin.Begin);
							num3 = binaryReader.ReadUInt32();
							if (1482047822 == num3)
							{
								num = num4;
								num2 = num5;
								flag = true;
							}
						}
					}
				}
				switch (num2)
				{
				case 1u:
					metadata = NavAppPackageMetadataV1.Create();
					break;
				case 2u:
					metadata = NavAppPackageMetadataV2.Create();
					break;
				}
				if (metadata != null && metadata.MetadataSize != num)
				{
					metadata = null;
				}
			}
			finally
			{
				stream.Seek(position, SeekOrigin.Begin);
			}
		}
		if (flag)
		{
			return metadata != null;
		}
		return false;
	}

	public void Read(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		bool flag = false;
		if (MetadataSize <= stream.Length)
		{
			long position = stream.Position;
			try
			{
				stream.Seek(0L, SeekOrigin.Begin);
				using BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
				uint num = binaryReader.ReadUInt32();
				if (1482047822 == num)
				{
					uint num2 = binaryReader.ReadUInt32();
					if (MetadataSize == num2)
					{
						uint num3 = binaryReader.ReadUInt32();
						if (MetadataVersion == num3)
						{
							byte[] b = binaryReader.ReadBytes(GuidBytesLength);
							long num4 = binaryReader.ReadInt64();
							if (MetadataSize + num4 <= stream.Length)
							{
								num = binaryReader.ReadUInt32();
								if (1482047822 == num)
								{
									PackageId = new Guid(b);
									ContentLength = num4;
									flag = true;
								}
							}
						}
					}
				}
			}
			finally
			{
				stream.Seek(position, SeekOrigin.Begin);
			}
		}
		if (!flag)
		{
			throw new ArgumentException(CodeAnalysisResources.DataNotApp, "stream");
		}
	}

	public void Write(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		try
		{
			stream.Seek(0L, SeekOrigin.Begin);
			using BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
			WriteMetadataHead(binaryWriter);
			WriteMetadataContent(binaryWriter);
			WriteMetadataTail(binaryWriter);
		}
		finally
		{
			stream.Seek(position, SeekOrigin.Begin);
		}
	}

	protected virtual void WriteMetadataContent(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(PackageId.ToByteArray());
		binaryWriter.Write(ContentLength);
	}

	private void WriteMetadataHead(BinaryWriter bw)
	{
		bw.Write(1482047822u);
		bw.Write(MetadataSize);
		bw.Write(MetadataVersion);
	}

	private static void WriteMetadataTail(BinaryWriter bw)
	{
		bw.Write(1482047822u);
	}
}
