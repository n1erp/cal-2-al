using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Configuration;

public abstract class SupportsInFileConfigurationSection : ConfigurationSection
{
	private const string FileConfigurationName = "file";

	[ConfigurationProperty("file", DefaultValue = "")]
	public string File
	{
		get
		{
			string text = (string)base["file"];
			if (text == null)
			{
				return string.Empty;
			}
			return text;
		}
		set
		{
			base["file"] = value;
		}
	}

	public string FileFullPath { get; private set; }

	protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
	{
		string name = reader.Name;
		base.DeserializeElement(reader, serializeCollectionKey);
		if (string.IsNullOrEmpty(File))
		{
			return;
		}
		string source = ElementInformation.Source;
		if (string.IsNullOrEmpty(source))
		{
			FileFullPath = File;
		}
		else
		{
			string directoryName = Path.GetDirectoryName(source);
			FileFullPath = Path.Combine(directoryName, File);
		}
		if (System.IO.File.Exists(FileFullPath))
		{
			using (Stream xmlStream = new FileStream(FileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				ConfigXmlReader internalReader = new ConfigXmlReader(xmlStream, FileFullPath, 0);
				Parse(internalReader, name);
			}
		}
	}

	internal void Parse(ConfigXmlReader internalReader, string elementName)
	{
		internalReader.MoveToNextElement();
		if (internalReader.Name != elementName)
		{
			throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The root element must match the name of the section referencing the file, '{0}'.", elementName), internalReader);
		}
		if (internalReader.MoveToNextAttribute())
		{
			throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "Unrecognized attribute '{0}'.", internalReader.Name), internalReader);
		}
		internalReader.MoveToElement();
		if (internalReader.Read())
		{
			base.DeserializeElement(internalReader, serializeCollectionKey: false);
		}
	}
}
