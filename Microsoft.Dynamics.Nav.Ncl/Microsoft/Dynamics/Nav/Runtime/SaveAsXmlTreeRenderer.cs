using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class SaveAsXmlTreeRenderer : IResultSetProcessor, IDisposable
{
	private readonly string fileName;

	private readonly NavOutStream outStream;

	private readonly NavReport report;

	private XmlWriter xmlWriter;

	private bool disposed;

	public string FileName => fileName;

	protected SaveAsXmlTreeRenderer(string fileName, NavReport report, NavOutStream outStream)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		this.fileName = NavFile.ExpandFileName(DataError.ThrowError, fileName);
		this.outStream = outStream;
		this.report = report;
	}

	public virtual void SetValue(int columnIndex, object value)
	{
	}

	public void SetValue(MetaDataItemColumn metaColumn, object value)
	{
		xmlWriter.WriteStartElement("Column");
		xmlWriter.WriteAttributeString("name", metaColumn.Name);
		WriteValue(value);
		xmlWriter.WriteEndElement();
	}

	public void SetValue(MetaDataItemColumn metaColumn, object value, string decimalFormatter)
	{
		xmlWriter.WriteStartElement("Column");
		xmlWriter.WriteAttributeString("name", metaColumn.Name);
		xmlWriter.WriteAttributeString("decimalformatter", decimalFormatter);
		WriteValue(value);
		xmlWriter.WriteEndElement();
	}

	private void WriteValue(object value)
	{
		if (value is byte[] array)
		{
			xmlWriter.WriteBase64(array, 0, array.Length);
		}
		else
		{
			xmlWriter.WriteString(value.ToString());
		}
	}

	public void StartLevel(string elementName, string name)
	{
		xmlWriter.WriteStartElement(elementName);
		if (name != null)
		{
			xmlWriter.WriteAttributeString("name", name);
		}
	}

	public void EndLevel()
	{
		xmlWriter.WriteEndElement();
	}

	public virtual void Start()
	{
		Exception ex = null;
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.CheckCharacters = false;
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
		try
		{
			xmlWriter = XmlWriter.Create(FileName, xmlWriterSettings);
			string text = "version=\"1.0\" standalone=\"yes\"";
			xmlWriter.WriteProcessingInstruction("xml", text);
			xmlWriter.WriteStartElement("ReportDataSet");
			xmlWriter.WriteAttributeString("name", report.ObjectName);
			xmlWriter.WriteAttributeString("id", report.ReportId.ToString(CultureInfo.InvariantCulture));
			if (!string.IsNullOrWhiteSpace(report.Metadata.WordMergeDataItem))
			{
				xmlWriter.WriteAttributeString("wordMergeDataItem", report.Metadata.WordMergeDataItem);
			}
			KeyValuePair<string, string>[] parameters = report.GetParameters();
			if (parameters.Length != 0)
			{
				xmlWriter.WriteStartElement("Labels");
				KeyValuePair<string, string>[] array = parameters;
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<string, string> keyValuePair = array[i];
					xmlWriter.WriteStartElement("Label");
					xmlWriter.WriteAttributeString("name", keyValuePair.Key);
					xmlWriter.WriteString(keyValuePair.Value);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteStartElement("DataItems");
		}
		catch (IOException ex2)
		{
			ex = ex2;
		}
		catch (SecurityException ex3)
		{
			ex = ex3;
		}
		catch (InvalidOperationException ex4)
		{
			ex = ex4;
		}
		catch (ArgumentException ex5)
		{
			ex = ex5;
		}
		catch (UnauthorizedAccessException ex6)
		{
			ex = ex6;
		}
		if (ex != null)
		{
			throw new NavNCLReportSaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.ReportOutputCouldNotBeSaved, ex.Message), ex);
		}
	}

	public void InsertRepeatedData(int[] columnIndexes)
	{
	}

	public void Finish()
	{
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		Render();
	}

	public virtual void EndOfRow()
	{
	}

	public void Abort()
	{
	}

	protected void Close()
	{
		if (xmlWriter == null)
		{
			return;
		}
		xmlWriter.Close();
		xmlWriter = null;
		if (outStream == null)
		{
			return;
		}
		try
		{
			using FileStream source = File.Open(fileName, FileMode.Open);
			NCLStreamHelper.CopyStream(source, outStream.InternalStream);
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: true);
			throw;
		}
	}

	protected abstract void Render();

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			Close();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void CleanupOnError()
	{
		Dispose(disposing: true);
		try
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}
}
