using System;
using System.Globalization;
using System.IO;
using System.Security;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class QuerySaveAsCsvRenderer : SaveAsRenderer
{
	internal const string DEFAULTDELIMITER = ",";

	private const int MAXPOSITION = 64000;

	private StreamWriter streamWriter;

	private bool disposed;

	private int[] positions;

	private bool fixedFormat;

	private string delimiter;

	public QuerySaveAsCsvRenderer(NavQuery query, string fileName, string delimiter, bool fixedFormat, int[] positions)
		: base(fileName, NavDataSetBuilder.CreateNavDataSet(query.NCLMetaQuery.MetaQuery))
	{
		SetRendererDefault(query, delimiter, fixedFormat, positions);
	}

	public QuerySaveAsCsvRenderer(NavQuery query, NavOutStream outStream, string delimiter, bool fixedFormat, int[] positions)
		: base(outStream, NavDataSetBuilder.CreateNavDataSet(query.NCLMetaQuery.MetaQuery))
	{
		SetRendererDefault(query, delimiter, fixedFormat, positions);
	}

	private void SetRendererDefault(NavQuery query, string newRenderer, bool newFixedFormat, int[] newPositions)
	{
		fixedFormat = newFixedFormat;
		if (newFixedFormat)
		{
			delimiter = string.Empty;
			positions = newPositions;
			ValidatePositions(query.NCLMetaQuery.QueryDefinition.DefinedColumns.Count);
		}
		else
		{
			delimiter = (string.IsNullOrEmpty(newRenderer) ? "," : newRenderer);
		}
	}

	public override void Start()
	{
		DataTableWriter.Clear();
		DataTableWriter.ResetRow();
		DataSet.Clear();
		CreateWriter();
		OutputRow(header: true);
	}

	public override void EndOfRow()
	{
		OutputRow(header: false);
	}

	protected override void Render()
	{
		streamWriter.Close();
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			if (streamWriter != null)
			{
				streamWriter.Close();
				streamWriter = null;
			}
			base.Dispose(disposing);
		}
	}

	private void OutputRow(bool header)
	{
		Exception ex = null;
		try
		{
			if (header)
			{
				NavDataRow navDataRow = DataSet.Tables[0].NewRow();
				navDataRow.WriteCsv(streamWriter, delimiter, writeColumnNames: true, fixedFormat, positions);
			}
			else
			{
				DataTableWriter.DecompressRow();
				NavDataRow navDataRow = DataSet.Tables[0].NewRow(DataTableWriter.CurrentRow);
				navDataRow.WriteCsv(streamWriter, delimiter, writeColumnNames: false, fixedFormat, positions);
			}
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

	private void CreateWriter()
	{
		Exception ex = null;
		try
		{
			streamWriter = ((OutStream != null) ? new StreamWriter(OutStream.InternalStreamCheckedForWrite) : new StreamWriter(FileName));
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
		catch (IOException ex8)
		{
			ex = ex8;
		}
		catch (NotSupportedException ex9)
		{
			ex = ex9;
		}
		if (ex != null)
		{
			throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, ex.Message), ex);
		}
	}

	private void ValidatePositions(int count)
	{
		if (positions == null || positions.Length < count)
		{
			throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, string.Format(CultureInfo.CurrentCulture, Lang.QueryInvalidPositionsArgument)));
		}
		int num = 1;
		for (int i = 0; i < count; i++)
		{
			if (positions[i] < num || positions[i] > 64000)
			{
				throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, string.Format(CultureInfo.CurrentCulture, Lang.QueryInvalidPositionsArgument)));
			}
			num = positions[i];
		}
	}
}
