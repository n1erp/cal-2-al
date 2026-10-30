using System;
using System.IO;
using System.Reflection;
using System.Text;
using CommandLine.Infrastructure;

namespace CommandLine.Text;

public class HeadingInfo
{
	private readonly string _programName;

	private readonly string _version;

	public static HeadingInfo Default
	{
		get
		{
			AssemblyTitleAttribute attribute = ReflectionHelper.GetAttribute<AssemblyTitleAttribute>();
			string programName = ((attribute == null) ? ReflectionHelper.AssemblyFromWhichToPullInformation.GetName().Name : Path.GetFileNameWithoutExtension(attribute.Title));
			AssemblyInformationalVersionAttribute attribute2 = ReflectionHelper.GetAttribute<AssemblyInformationalVersionAttribute>();
			string version = ((attribute2 == null) ? ReflectionHelper.AssemblyFromWhichToPullInformation.GetName().Version.ToString() : attribute2.InformationalVersion);
			return new HeadingInfo(programName, version);
		}
	}

	public HeadingInfo(string programName)
		: this(programName, null)
	{
	}

	public HeadingInfo(string programName, string version)
	{
		Assumes.NotNullOrEmpty(programName, "programName");
		_programName = programName;
		_version = version;
	}

	public static implicit operator string(HeadingInfo info)
	{
		return info.ToString();
	}

	public override string ToString()
	{
		bool flag = string.IsNullOrEmpty(_version);
		StringBuilder stringBuilder = new StringBuilder(_programName.Length + ((!flag) ? (_version.Length + 1) : 0));
		stringBuilder.Append(_programName);
		if (!flag)
		{
			stringBuilder.Append(' ');
			stringBuilder.Append(_version);
		}
		return stringBuilder.ToString();
	}

	public void WriteMessage(string message, TextWriter writer)
	{
		Assumes.NotNullOrEmpty(message, "message");
		Assumes.NotNull(writer, "writer");
		StringBuilder stringBuilder = new StringBuilder(_programName.Length + message.Length + 2);
		stringBuilder.Append(_programName);
		stringBuilder.Append(": ");
		stringBuilder.Append(message);
		writer.WriteLine(stringBuilder.ToString());
	}

	public void WriteMessage(string message)
	{
		WriteMessage(message, Console.Out);
	}

	public void WriteError(string message)
	{
		WriteMessage(message, Console.Error);
	}
}
