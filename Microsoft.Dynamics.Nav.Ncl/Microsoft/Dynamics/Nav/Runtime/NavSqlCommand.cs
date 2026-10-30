using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlCommand : TreeObject, IDisposable
{
	private SqlCommand sqlCommand;

	private SqlDataReader reader;

	private int readerToken;

	private NavSqlCommandState state;

	private NavSqlCommand previousNavSqlCommand;

	private NavSqlCommand nextNavSqlCommand;

	private NavSqlCommandCacheKey commandKey;

	private int totalRecordsFetched;

	private int recordsFetchedThisExecution;

	private int totalExecutions;

	public SqlDataReader Reader => reader;

	public int ReaderToken => readerToken;

	public NavSqlCommandState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

	public NavSqlConnection Connection => Tree.Parent as NavSqlConnection;

	public NavSqlCommand PreviousSqlCommand => previousNavSqlCommand;

	public NavSqlCommand NextSqlCommand => nextNavSqlCommand;

	public SqlCommand Command => sqlCommand;

	public virtual bool Stale => false;

	public NavSqlCommandCacheKey CommandKey
	{
		get
		{
			return commandKey;
		}
		set
		{
			commandKey = value;
		}
	}

	public int TotalRecordsFetched => totalRecordsFetched;

	public int RecordsFetchedThisExecution => recordsFetchedThisExecution;

	public int TotalExecutions => totalExecutions;

	public bool CanCacheCommand
	{
		get
		{
			NavDatabase navDatabase = Connection?.Database;
			if (navDatabase != null)
			{
				if (navDatabase.DatabaseType == NavDatabaseType.Tenant && navDatabase.TenantDatabaseProperties.IsReady)
				{
					return navDatabase.IsCurrentVersion;
				}
				return true;
			}
			return false;
		}
	}

	private void ForceSqlParametersToOrdinal()
	{
		if (State == NavSqlCommandState.Executed || !ServerUserSettings.Instance.SqlParametersByOrdinal.Value)
		{
			return;
		}
		foreach (SqlParameter parameter in sqlCommand.Parameters)
		{
			parameter.ParameterName = null;
		}
	}

	protected NavSqlCommand(NavSqlConnection parentConnection)
		: base(parentConnection)
	{
		CreateSqlCommand(parentConnection);
	}

	public static NavSqlCommand Create(NavSqlConnection parentConnection)
	{
		return new NavSqlCommand(parentConnection);
	}

	private void DisposeReader()
	{
		SqlDataReader readerToDispose = reader;
		readerToken++;
		reader = null;
		if (readerToDispose != null && !Connection.ExecuteAdoFunction(() => readerToDispose.IsClosed, "Reader IsClosed"))
		{
			Connection.ExecuteAdoAction(() =>
			{
				readerToDispose.Dispose();
			}, "Reader Dispose");
		}
	}

	internal virtual void CloseReader(bool forceClose, bool recreateCommand = true)
	{
		if (forceClose && state == NavSqlCommandState.ExecutedWithPendingResults)
		{
			try
			{
				if (state == NavSqlCommandState.ExecutedWithPendingResults)
				{
					Connection.ExecuteAdoAction(() =>
					{
						Command.Cancel();
					}, "Reader Cancel");
				}
				return;
			}
			finally
			{
				if (!Tree.IsDisposed)
				{
					DisposeReader();
					if (recreateCommand)
					{
						string commandText = Command.CommandText;
						SqlParameter[] array = null;
						bool flag = sqlCommand.Parameters.Count > 0;
						if (flag)
						{
							array = new SqlParameter[sqlCommand.Parameters.Count];
							sqlCommand.Parameters.CopyTo(array, 0);
							sqlCommand.Parameters.Clear();
						}
						SqlCommand commandToDispose = sqlCommand;
						sqlCommand = null;
						Connection.ExecuteAdoAction(() =>
						{
							commandToDispose.Dispose();
						}, "Finally Reader Dispose");
						CreateSqlCommand(Connection);
						SetCommandText(commandText);
						if (flag)
						{
							sqlCommand.Parameters.AddRange(array);
						}
					}
					else
					{
						state = NavSqlCommandState.Created;
					}
				}
			}
		}
		DisposeReader();
		State = NavSqlCommandState.Prepared;
	}

	public void SetCommandTimeout(int? commandTimeout = null)
	{
		if (Command != null)
		{
			Command.CommandTimeout = commandTimeout ?? Math.Max(0, ServerUserSettings.Instance.SqlCommandTimeout.TotalSeconds);
		}
	}

	public void CreateSqlCommand(NavSqlConnection parentConnection)
	{
		if (parentConnection == null)
		{
			ArgumentNullException ex = new ArgumentNullException("parentConnection");
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000YA", Category.Sql, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("parentConnection is null contrary to expectations"));
			throw ex;
		}
		if (parentConnection.Connection == null)
		{
			InvalidOperationException ex2 = new InvalidOperationException("parentConnection.Connection");
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000YC", Category.Sql, ex2, Verbosity.Error, DiagnosticParameter.SystemMetadata("parentConnection.Connection is null contrary to expectations"));
			throw ex2;
		}
		sqlCommand = parentConnection.Connection.CreateCommand();
		sqlCommand.CommandTimeout = Math.Max(0, ServerUserSettings.Instance.SqlCommandTimeout.TotalSeconds);
		state = NavSqlCommandState.Created;
	}

	public void LinkToList(ref NavSqlCommand firstInList, ref NavSqlCommand lastInList)
	{
		nextNavSqlCommand = firstInList;
		if (nextNavSqlCommand != null)
		{
			nextNavSqlCommand.previousNavSqlCommand = this;
		}
		else
		{
			lastInList = this;
		}
		previousNavSqlCommand = null;
		firstInList = this;
	}

	public void UnlinkFromList(ref NavSqlCommand firstInList, ref NavSqlCommand lastInList)
	{
		if (lastInList == this)
		{
			lastInList = previousNavSqlCommand;
		}
		if (previousNavSqlCommand == null)
		{
			firstInList = nextNavSqlCommand;
		}
		else
		{
			previousNavSqlCommand.nextNavSqlCommand = nextNavSqlCommand;
		}
		if (nextNavSqlCommand != null)
		{
			nextNavSqlCommand.previousNavSqlCommand = previousNavSqlCommand;
			nextNavSqlCommand = null;
		}
		previousNavSqlCommand = null;
	}

	public void SetCommandText(string commandText)
	{
		Command.CommandText = commandText;
		state = NavSqlCommandState.CreatedWithCommandText;
	}

	public void PrepareStatement()
	{
		if (state == NavSqlCommandState.CreatedWithCommandText)
		{
			Command.Transaction = Connection.CurrentTransaction;
			Connection.ExecuteActionWithTrace((EventTask)7, Connection.TenantTraceId, NavCurrentThread.SessionId, NavCurrentThread.UserName, () =>
			{
				Command.Prepare();
			}, this);
			State = NavSqlCommandState.Prepared;
		}
	}

	public object ExecuteScalar()
	{
		BeforeSqlStatementExceuted(this);
		Command.Transaction = Connection.CurrentTransaction;
		object result = Connection.ExecuteFunctionWithTrace((EventTask)1, () => Command.ExecuteScalar(), this);
		ForceSqlParametersToOrdinal();
		State = NavSqlCommandState.Executed;
		return result;
	}

	public object ExecuteScalar(string statement)
	{
		SetCommandText(statement);
		return ExecuteScalar();
	}

	public T ExecuteScalar<T>(string statement)
	{
		return (T)ExecuteScalar(statement);
	}

	public T ExecuteScalar<T>()
	{
		return (T)ExecuteScalar();
	}

	public int ExecuteNonQuery(string statement)
	{
		SetCommandText(statement);
		return ExecuteNonQuery();
	}

	public int ExecuteNonQuery()
	{
		BeforeSqlStatementExceuted(this);
		return ExecuteNonQueryImp();
	}

	private int ExecuteNonQueryImp()
	{
		Command.Transaction = Connection.CurrentTransaction;
		int result = Connection.ExecuteFunctionWithTrace((EventTask)2, () => Command.ExecuteNonQuery(), this);
		ForceSqlParametersToOrdinal();
		State = NavSqlCommandState.Executed;
		return result;
	}

	public int ExecuteNonQueryNoTrace(string statement)
	{
		SetCommandText(statement);
		return ExecuteNonQueryImp();
	}

	public bool ReadNextRow()
	{
		if (reader.IsClosed)
		{
			return false;
		}
		bool flag = false;
		if (reader.HasRows)
		{
			flag = Connection.ExecuteFunctionWithTrace((EventTask)5, () => Reader.Read(), this);
		}
		if (flag)
		{
			totalRecordsFetched++;
			recordsFetchedThisExecution++;
		}
		else
		{
			while (Connection.ExecuteFunctionWithTrace((EventTask)4, () => reader.NextResult(), this))
			{
			}
			CloseReader(forceClose: false);
		}
		return flag;
	}

	public SqlDataReader ExecuteReader()
	{
		BeforeSqlStatementExceuted(this);
		Command.Transaction = Connection.CurrentTransaction;
		readerToken++;
		Connection.ExecuteFunctionWithTrace((EventTask)3, () => reader = Command.ExecuteReader(CommandBehavior.SequentialAccess), this);
		State = NavSqlCommandState.ExecutedWithPendingResults;
		recordsFetchedThisExecution = 0;
		totalExecutions++;
		return reader;
	}

	public SqlDataReader ExecuteReader(string statement)
	{
		SetCommandText(statement);
		return ExecuteReader();
	}

	protected NavValue CreateNavValueFromReader(INavFieldMetadata field, int columnIndex, int parentTableId = -1)
	{
		try
		{
			if (reader.IsDBNull(columnIndex))
			{
				if (field.NclType == NavNclType.NavBlob)
				{
					return new NavBLOB(0);
				}
				return field.EmptyValue;
			}
			switch (field.NclType)
			{
			case NavNclType.NavMedia:
				return new NavMedia(reader.GetGuid(columnIndex), parentTableId);
			case NavNclType.NavMediaSet:
				return new NavMediaSet(reader.GetGuid(columnIndex), parentTableId);
			case NavNclType.NavGuid:
				return new NavGuid(reader.GetGuid(columnIndex));
			case NavNclType.NavBigInteger:
				if (field.SqlDataType == SqlDataType.Timestamp)
				{
					byte[] array3 = new byte[8];
					reader.GetBytes(columnIndex, 0L, array3, 0, array3.Length);
					return NavBigInteger.Create(TimestampToInt64(array3), array3);
				}
				return NavBigInteger.Create(reader.GetInt64(columnIndex));
			case NavNclType.NavDuration:
				return new NavDuration(reader.GetInt64(columnIndex));
			case NavNclType.NavBlob:
				return new NavBLOB(reader.GetInt32(columnIndex));
			case NavNclType.NavBoolean:
				return NavBoolean.Create(reader.GetByte(columnIndex) != 0);
			case NavNclType.NavByte:
				return NavByte.Create(reader.GetByte(columnIndex));
			case NavNclType.NavDecimal:
			{
				SqlDecimal sqlDecimal = reader.GetSqlDecimal(columnIndex);
				return NavDecimal.Create(NavSqlDecimalHelper.SqlDecimalToDecimal(sqlDecimal));
			}
			case NavNclType.NavOption:
				return NavOption.Create(field.NavOptionMetadata, reader.GetInt32(columnIndex));
			case NavNclType.NavInteger:
				return NavInteger.Create(reader.GetInt32(columnIndex));
			case NavNclType.NavRecordId:
			{
				byte[] array = new byte[448];
				reader.GetBytes(columnIndex, 0L, array, 0, array.Length);
				return new NavRecordId(array);
			}
			case NavNclType.NavTime:
			{
				DateTime dateTime = reader.GetDateTime(columnIndex);
				if (dateTime.Equals(NavTime.SqlTimeUndefined))
				{
					return NavTime.Undefined;
				}
				return NavTime.Create(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
			}
			case NavNclType.NavDate:
			{
				DateTime initValue2 = new DateTime(reader.GetDateTime(columnIndex).Ticks, DateTimeKind.Local);
				if (initValue2.Equals(NavDate.SqlDateTimeUndefined))
				{
					return NavDate.Undefined;
				}
				return NavDate.Create(initValue2);
			}
			case NavNclType.NavDateTime:
			{
				DateTime initValue = new DateTime(reader.GetDateTime(columnIndex).Ticks, DateTimeKind.Utc);
				if (initValue.Equals(NavDateTime.SqlDateTimeUtcUndefined))
				{
					return NavDateTime.Undefined;
				}
				return NavDateTime.Create(initValue, DateTimeReferenceFrame.Server, unspecifiedAsLocal: false);
			}
			case NavNclType.NavTableFilter:
			{
				byte[] array2 = new byte[504];
				reader.GetBytes(columnIndex, 0L, array2, 0, array2.Length);
				return new NavTableFilter(array2);
			}
			case NavNclType.NavOemCode:
				switch (field.SqlDataType)
				{
				case SqlDataType.VarChar:
				case SqlDataType.Default:
				{
					string value6 = reader.GetString(columnIndex);
					if (string.IsNullOrEmpty(value6))
					{
						return field.EmptyValue;
					}
					return new NavOemCode(field.NavDefinedLengthMetadata, value6);
				}
				case SqlDataType.BigInt:
				{
					long int3 = reader.GetInt64(columnIndex);
					if (int3 < 0 || int3 == long.MaxValue)
					{
						throw new NavCSideException(22928072);
					}
					if (int3 > 0)
					{
						return new NavOemCode(field.NavDefinedLengthMetadata, int3);
					}
					return new NavOemCode(field.NavDefinedLengthMetadata, string.Empty);
				}
				case SqlDataType.Integer:
				{
					int int4 = reader.GetInt32(columnIndex);
					if (int4 < 0 || int4 == int.MaxValue)
					{
						throw new NavCSideException(22928072);
					}
					if (int4 > 0)
					{
						return new NavOemCode(field.NavDefinedLengthMetadata, int4);
					}
					return new NavOemCode(field.NavDefinedLengthMetadata, string.Empty);
				}
				case SqlDataType.Variant:
				{
					object value5 = reader.GetValue(columnIndex);
					if (value5 is int)
					{
						return new NavOemCode(field.NavDefinedLengthMetadata, (int)value5);
					}
					if (value5 is long)
					{
						return new NavOemCode(field.NavDefinedLengthMetadata, (long)value5);
					}
					return new NavOemCode(field.NavDefinedLengthMetadata, value5.ToString());
				}
				default:
					throw new NotSupportedException();
				}
			case NavNclType.NavOemText:
			{
				string value4 = reader.GetString(columnIndex);
				if (string.IsNullOrEmpty(value4))
				{
					return field.EmptyValue;
				}
				return new NavOemText(field.NavDefinedLengthMetadata, value4);
			}
			case NavNclType.NavDateFormula:
			{
				string text = reader.GetString(columnIndex);
				if (string.IsNullOrEmpty(text))
				{
					return field.EmptyValue;
				}
				return new NavDateFormula(text, isTokenString: true);
			}
			case NavNclType.NavText:
			{
				string value3 = reader.GetString(columnIndex);
				if (string.IsNullOrEmpty(value3))
				{
					return field.EmptyValue;
				}
				return new NavText(field.NavDefinedLengthMetadata, value3);
			}
			case NavNclType.NavCode:
				switch (field.SqlDataType)
				{
				case SqlDataType.VarChar:
				case SqlDataType.Default:
				{
					string value2 = reader.GetString(columnIndex);
					if (string.IsNullOrEmpty(value2))
					{
						return field.EmptyValue;
					}
					return new NavCode(field.NavDefinedLengthMetadata, value2);
				}
				case SqlDataType.BigInt:
				{
					long @int = reader.GetInt64(columnIndex);
					if (@int < 0 || @int == long.MaxValue)
					{
						throw new NavCSideException(22928072);
					}
					if (@int > 0)
					{
						return new NavCode(field.NavDefinedLengthMetadata, @int);
					}
					return new NavCode(field.NavDefinedLengthMetadata, string.Empty);
				}
				case SqlDataType.Integer:
				{
					int int2 = reader.GetInt32(columnIndex);
					if (int2 < 0 || int2 == int.MaxValue)
					{
						throw new NavCSideException(22928072);
					}
					if (int2 > 0)
					{
						return new NavCode(field.NavDefinedLengthMetadata, int2);
					}
					return new NavCode(field.NavDefinedLengthMetadata, string.Empty);
				}
				case SqlDataType.Variant:
				{
					object value = reader.GetValue(columnIndex);
					if (value is int)
					{
						return new NavCode(field.NavDefinedLengthMetadata, (int)value);
					}
					if (value is long)
					{
						return new NavCode(field.NavDefinedLengthMetadata, (long)value);
					}
					return new NavCode(field.NavDefinedLengthMetadata, value.ToString());
				}
				default:
					throw new NotSupportedException();
				}
			default:
				throw new NotSupportedException();
			}
		}
		catch (InvalidCastException)
		{
			throw new NavCSideException(22928069, string.Format(CultureInfo.CurrentCulture, Lang.IncompatibleSqlType, field.DescriptiveParentName, field.DescriptiveName, field.NavType, Reader.GetDataTypeName(columnIndex), ProductNames.ProductNameShort));
		}
	}

	internal unsafe static long TimestampToInt64(byte[] buffer)
	{
		fixed (byte* ptr = &buffer[0])
		{
			return (uint)((ptr[4] << 24) | (ptr[5] << 16) | (ptr[6] << 8) | ptr[7]) | ((long)((*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | ptr[3]) << 32);
		}
	}

	private static void BeforeSqlStatementExceuted(NavSqlCommand command)
	{
		NavCurrentThread.Session?.SqlProfilerCommentProvider.BeforeSqlStatementExecuted(command);
	}

	[Conditional("DEBUG")]
	private void SetParameterCountOnPrepare()
	{
	}

	[Conditional("DEBUG")]
	private void VerifyParameterCountOnExecute()
	{
	}

	internal ReadOnlyRecordBuffer ReaderToRecord(NCLMetaTable table, FieldList keyFieldsToRead = null)
	{
		ReadOnlyCollection<NCLMetaField> sqlReadableFields = table.SqlReadableFields;
		NavValue[] array = new NavValue[table.FieldCount];
		if (keyFieldsToRead != null)
		{
			foreach (NCLMetaField item in keyFieldsToRead.GetFieldsSortedBySqlColumnIndex())
			{
				array[item.FieldIndex] = CreateNavValueFromReader(item, item.SqlColumnIndex);
			}
		}
		else
		{
			for (int i = 0; i < sqlReadableFields.Count; i++)
			{
				NCLMetaField nCLMetaField = sqlReadableFields[i];
				array[nCLMetaField.FieldIndex] = CreateNavValueFromReader(nCLMetaField, i);
			}
			NCLMetaField userDefinedTimestampField = table.UserDefinedTimestampField;
			if (userDefinedTimestampField != null && table.SqlHasTimestamp)
			{
				array[userDefinedTimestampField.FieldIndex] = array[0];
			}
		}
		return new ReadOnlyRecordBuffer(table, keyFieldsToRead != null, array);
	}

	internal int GetBlobDataFromReader(bool isCompressed, int resultIndex, NavBLOB navBlob)
	{
		int num = (int)((!Reader.IsDBNull(resultIndex)) ? Reader.GetBytes(resultIndex, 0L, null, 0, 0) : 0);
		if (num > 0)
		{
			using Stream stream = new BlobReaderStream(Reader, resultIndex, num);
			using Stream stream2 = navBlob.GetNewStream();
			if (isCompressed)
			{
				byte[] array = new byte[4];
				long num2 = stream.Read(array, 0, 4);
				if (num2 < 4 || !NavBLOB.BlobMagicOk(array))
				{
					CloseReader(forceClose: false);
					return 22926086;
				}
				using DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
				deflateStream.CopyTo(stream2);
			}
			else
			{
				stream.CopyTo(stream2);
			}
			stream2.SetLength(stream2.Position);
		}
		return 0;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!IsDisposed & disposing)
			{
				CloseReader(forceClose: true, recreateCommand: false);
				sqlCommand?.Dispose();
				sqlCommand = null;
			}
		}
		catch (SqlException)
		{
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
