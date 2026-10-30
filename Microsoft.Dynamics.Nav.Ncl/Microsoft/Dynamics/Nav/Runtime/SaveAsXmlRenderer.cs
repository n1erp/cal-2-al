using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class SaveAsXmlRenderer : SaveAsRenderer
{
	private XmlWriter xmlWriter;

	private bool disposed;

	public SaveAsXmlRenderer(NavDataSet dataSet, string fileName)
		: base(fileName, dataSet)
	{
	}

	public SaveAsXmlRenderer(NavDataSet dataSet, NavOutStream outStream)
		: base(outStream, dataSet)
	{
	}

	public override void Start()
	{
		DataTableWriter.Clear();
		DataTableWriter.ResetRow();
		DataSet.Clear();
		WriteSchema();
	}

	private void WriteSchema()
	{
		Exception ex = null;
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
		try
		{
			xmlWriter = ((OutStream != null) ? XmlWriter.Create(OutStream.InternalStreamCheckedForWrite, xmlWriterSettings) : XmlWriter.Create(FileName, xmlWriterSettings));
			string text = "version=\"1.0\" standalone=\"yes\"";
			xmlWriter.WriteProcessingInstruction("xml", text);
			xmlWriter.WriteStartElement(DataSet.DataSetName);
			DataSet.Tables[0].GetDataTable().WriteXmlSchema(xmlWriter, writeHierarchy: true);
		}
		catch (DirectoryNotFoundException ex2)
		{
			ex = ex2;
		}
		catch (PathTooLongException ex3)
		{
			ex = ex3;
		}
		catch (SecurityException ex4)
		{
			ex = ex4;
		}
		catch (InvalidOperationException ex5)
		{
			ex = ex5;
		}
		catch (ArgumentException ex6)
		{
			ex = ex6;
		}
		catch (UnauthorizedAccessException ex7)
		{
			ex = ex7;
		}
		if (ex != null)
		{
			throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, ex.Message), ex);
		}
	}

	public override void EndOfRow()
	{
		DataTableWriter.DecompressRow();
		NavDataRow navDataRow = DataSet.Tables[0].NewRow(DataTableWriter.CurrentRow);
		Exception ex = null;
		try
		{
			navDataRow.WriteXml(xmlWriter);
		}
		catch (InvalidOperationException ex2)
		{
			ex = ex2;
		}
		catch (ArgumentException ex3)
		{
			ex = ex3;
		}
		if (ex != null)
		{
			throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, ex.Message), ex);
		}
		DataTableWriter.ResetRow();
	}

	protected override void Render()
	{
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			if (xmlWriter != null)
			{
				xmlWriter.Close();
				xmlWriter = null;
			}
			base.Dispose(disposing);
		}
	}
}
