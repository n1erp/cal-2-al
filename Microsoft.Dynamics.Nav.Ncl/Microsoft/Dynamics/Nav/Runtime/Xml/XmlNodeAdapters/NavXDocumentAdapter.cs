using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal class NavXDocumentAdapter : NavXNodeAdapter<XDocument>
{
	private bool HasCustomEncoding => Value.Declaration?.Encoding != null;

	public NavXDocumentAdapter(XDocument node)
		: base(node)
	{
	}

	public override bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			if (HasCustomEncoding)
			{
				Encoding encodingToUse = GetEncodingToUse();
				using (StreamWriter streamWriter = new StreamWriter(data.InternalStreamCheckedForWrite, encodingToUse, 1024, leaveOpen: true))
				{
					streamWriter.Write(Value.Declaration.ToString());
					if (!writeOptions.SaveOptions.HasFlag(SaveOptions.DisableFormatting))
					{
						streamWriter.WriteLine();
					}
				}
				data.Position = data.Length;
				using Stream stream = GetDocumentStream(writeOptions, encodingToUse, omitDeclaration: true, skipPreamble: true);
				stream.CopyTo(data.InternalStreamCheckedForWrite);
				return;
			}
			Value.Save(data.InternalStreamCheckedForWrite, writeOptions.SaveOptions);
		});
	}

	public override bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Encoding encodingToUse = GetEncodingToUse();
			string documentString;
			if (HasCustomEncoding)
			{
				string text = Value.Declaration.ToString();
				documentString = GetDocumentString(writeOptions, encodingToUse, omitDeclaration: true, skipPreamble: true);
				documentString = ((!writeOptions.SaveOptions.HasFlag(SaveOptions.DisableFormatting)) ? (text + Environment.NewLine + documentString) : (text + documentString));
			}
			else
			{
				documentString = GetDocumentString(writeOptions, encodingToUse, omitDeclaration: false, skipPreamble: false);
			}
			data.Value = new NavText(data.Value.MaxLength, documentString);
		});
	}

	private Stream GetDocumentStream(NavXmlWriteOptions writeOptions, Encoding encoding, bool omitDeclaration, bool skipPreamble)
	{
		XmlWriterSettings xmlWriterSettings = GetXmlWriterSettings(writeOptions.SaveOptions, encoding, omitDeclaration);
		MemoryStream memoryStream = new MemoryStream();
		using (XmlWriter writer = XmlWriter.Create(memoryStream, xmlWriterSettings))
		{
			Value.Save(writer);
		}
		memoryStream.Position = 0L;
		if (skipPreamble)
		{
			byte[] preamble = encoding.GetPreamble();
			if (preamble.Length != 0)
			{
				memoryStream.Seek(preamble.Length, SeekOrigin.Current);
			}
		}
		return memoryStream;
	}

	private string GetDocumentString(NavXmlWriteOptions writeOptions, Encoding encoding, bool omitDeclaration, bool skipPreamble)
	{
		using Stream stream = GetDocumentStream(writeOptions, encoding, omitDeclaration, skipPreamble);
		using StreamReader streamReader = new StreamReader(stream, encoding);
		return streamReader.ReadToEnd();
	}

	private Encoding GetEncodingToUse()
	{
		string name = Value.Declaration?.Encoding;
		if (!string.IsNullOrEmpty(name))
		{
			try
			{
				return Encoding.GetEncoding(name);
			}
			catch (ArgumentException)
			{
			}
		}
		return Encoding.Unicode;
	}

	private static XmlWriterSettings GetXmlWriterSettings(SaveOptions options, Encoding encoding, bool omitDeclaration)
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = omitDeclaration;
		xmlWriterSettings.Encoding = encoding;
		if (!options.HasFlag(SaveOptions.DisableFormatting))
		{
			xmlWriterSettings.Indent = true;
		}
		if (options.HasFlag(SaveOptions.OmitDuplicateNamespaces))
		{
			xmlWriterSettings.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
		}
		return xmlWriterSettings;
	}
}
