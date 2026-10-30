using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class BlobWriterStream : Stream
{
	private class InnerBlobWriterStream : Stream
	{
		private readonly NavSqlConnection connection;

		private readonly string schemaName;

		private readonly string tableName;

		private readonly string blobColumn;

		private readonly string keyColumn;

		private readonly SqlDbType keyColumnType;

		private readonly object keyValue;

		private readonly bool isTenantTable;

		private readonly NavSqlCommandCacheKey firstChunkCommandCacheKey;

		private readonly NavSqlCommandCacheKey firstChunkCommandCacheKeyTenant;

		private readonly NavSqlCommandCacheKey appendChunkCommandCacheKey;

		private readonly NavSqlCommandCacheKey appendChunkCommandCacheKeyTenant;

		private readonly int? companyId;

		private long position;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => 0L;

		public override long Position
		{
			get
			{
				return position;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public InnerBlobWriterStream(NavSqlConnection connection, string schemaName, string tableName, string blobColumn, string keyColumn, SqlDbType keyColumnType, object keyValue, bool isTenantTable, int? companyId)
		{
			this.connection = connection;
			this.schemaName = schemaName;
			this.tableName = tableName;
			this.blobColumn = blobColumn;
			this.keyColumn = keyColumn;
			this.keyColumnType = keyColumnType;
			this.keyValue = keyValue;
			this.isTenantTable = isTenantTable;
			this.companyId = companyId;
			firstChunkCommandCacheKey = new NavSqlCombinedStringBasedCommandCacheKey(this.connection.Database, "BlobWriterStreamFirstChunk", this.schemaName, this.tableName, this.blobColumn, this.keyColumn);
			firstChunkCommandCacheKeyTenant = new NavSqlCombinedStringBasedCommandCacheKey(this.connection.Database, "BlobWriterStreamFirstChunkTenant", this.schemaName, this.tableName, this.blobColumn, this.keyColumn);
			appendChunkCommandCacheKey = new NavSqlCombinedStringBasedCommandCacheKey(this.connection.Database, "BlobWriterStreamAppendChunk", this.schemaName, this.tableName, this.blobColumn, this.keyColumn);
			appendChunkCommandCacheKeyTenant = new NavSqlCombinedStringBasedCommandCacheKey(this.connection.Database, "BlobWriterStreamAppendChunkTenant", this.schemaName, this.tableName, this.blobColumn, this.keyColumn);
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int index, int count)
		{
			byte[] bytesToWrite = buffer;
			if (index != 0)
			{
				bytesToWrite = new byte[count];
				for (int i = 0; i < count; i++)
				{
					bytesToWrite[i] = buffer[i + index];
				}
			}
			if (position == 0L)
			{
				if (isTenantTable)
				{
					connection.ExecuteCommandAndCacheOnSuccess(firstChunkCommandCacheKeyTenant, () =>
					{
						NavSqlCommand navSqlCommand = NavSqlCommand.Create(connection);
						navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "UPDATE {0}.{1}.{2} SET {3} = @firstChunk WHERE {5} {4} = @key", NavSqlStatementHelper.QuoteIdentifier(connection.Database.DatabaseName), NavSqlStatementHelper.QuoteIdentifier(schemaName), NavSqlStatementHelper.QuoteIdentifier(tableName), NavSqlStatementHelper.QuoteIdentifier(blobColumn), NavSqlStatementHelper.QuoteIdentifier(keyColumn), companyId.HasValue ? "\"$companyId\" = @companyid AND" : ""));
						navSqlCommand.Command.Parameters.Add("@firstChunk", SqlDbType.VarBinary, -1);
						navSqlCommand.Command.Parameters.Add("@key", keyColumnType);
						if (companyId.HasValue)
						{
							navSqlCommand.Command.Parameters.Add("@companyid", SqlDbType.Int);
						}
						navSqlCommand.PrepareStatement();
						return navSqlCommand;
					}, (NavSqlCommand selectTableVersion) =>
					{
						selectTableVersion.Command.Parameters[0].Value = bytesToWrite;
						selectTableVersion.Command.Parameters[1].Value = keyValue;
						if (companyId.HasValue)
						{
							selectTableVersion.Command.Parameters[2].Value = companyId.Value;
						}
						selectTableVersion.ExecuteNonQuery();
						return 0;
					});
				}
				else
				{
					connection.ExecuteCommandAndCacheOnSuccess(firstChunkCommandCacheKey, () =>
					{
						NavSqlCommand navSqlCommand = NavSqlCommand.Create(connection);
						navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "UPDATE {0}.{1}.{2} SET {3} = @firstChunk WHERE {4} = @key", NavSqlStatementHelper.QuoteIdentifier(connection.Database.DatabaseName), NavSqlStatementHelper.QuoteIdentifier(schemaName), NavSqlStatementHelper.QuoteIdentifier(tableName), NavSqlStatementHelper.QuoteIdentifier(blobColumn), NavSqlStatementHelper.QuoteIdentifier(keyColumn)));
						navSqlCommand.Command.Parameters.Add("@firstChunk", SqlDbType.VarBinary, -1);
						navSqlCommand.Command.Parameters.Add("@key", keyColumnType);
						navSqlCommand.PrepareStatement();
						return navSqlCommand;
					}, (NavSqlCommand selectTableVersion) =>
					{
						selectTableVersion.Command.Parameters[0].Value = bytesToWrite;
						selectTableVersion.Command.Parameters[1].Value = keyValue;
						selectTableVersion.ExecuteNonQuery();
						return 0;
					});
				}
				position = count;
				return;
			}
			if (isTenantTable)
			{
				connection.ExecuteCommandAndCacheOnSuccess(appendChunkCommandCacheKeyTenant, () =>
				{
					NavSqlCommand navSqlCommand = NavSqlCommand.Create(connection);
					navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "UPDATE {0}.{1}.{2} SET {3}.WRITE(@chunk, NULL, @length) WHERE {5} {4} = @key", NavSqlStatementHelper.QuoteIdentifier(connection.Database.DatabaseName), NavSqlStatementHelper.QuoteIdentifier(schemaName), NavSqlStatementHelper.QuoteIdentifier(tableName), NavSqlStatementHelper.QuoteIdentifier(blobColumn), NavSqlStatementHelper.QuoteIdentifier(keyColumn), companyId.HasValue ? "\"$companyId\" = @companyid AND " : ""));
					navSqlCommand.Command.Parameters.Add("@chunk", SqlDbType.VarBinary, -1);
					navSqlCommand.Command.Parameters.Add("@length", SqlDbType.BigInt);
					navSqlCommand.Command.Parameters.Add("@key", keyColumnType);
					if (companyId.HasValue)
					{
						navSqlCommand.Command.Parameters.Add("@companyid", SqlDbType.Int);
					}
					navSqlCommand.PrepareStatement();
					return navSqlCommand;
				}, (NavSqlCommand selectTableVersion) =>
				{
					selectTableVersion.Command.Parameters[0].Value = bytesToWrite;
					selectTableVersion.Command.Parameters[1].Value = count;
					selectTableVersion.Command.Parameters[2].Value = keyValue;
					if (companyId.HasValue)
					{
						selectTableVersion.Command.Parameters[3].Value = companyId.Value;
					}
					selectTableVersion.ExecuteNonQuery();
					return 0;
				});
			}
			else
			{
				connection.ExecuteCommandAndCacheOnSuccess(appendChunkCommandCacheKey, () =>
				{
					NavSqlCommand navSqlCommand = NavSqlCommand.Create(connection);
					navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "UPDATE {0}.{1}.{2} SET {3}.WRITE(@chunk, NULL, @length) WHERE {4} = @key", NavSqlStatementHelper.QuoteIdentifier(connection.Database.DatabaseName), NavSqlStatementHelper.QuoteIdentifier(schemaName), NavSqlStatementHelper.QuoteIdentifier(tableName), NavSqlStatementHelper.QuoteIdentifier(blobColumn), NavSqlStatementHelper.QuoteIdentifier(keyColumn)));
					navSqlCommand.Command.Parameters.Add("@chunk", SqlDbType.VarBinary, -1);
					navSqlCommand.Command.Parameters.Add("@length", SqlDbType.BigInt);
					navSqlCommand.Command.Parameters.Add("@key", keyColumnType);
					navSqlCommand.PrepareStatement();
					return navSqlCommand;
				}, (NavSqlCommand selectTableVersion) =>
				{
					selectTableVersion.Command.Parameters[0].Value = bytesToWrite;
					selectTableVersion.Command.Parameters[1].Value = count;
					selectTableVersion.Command.Parameters[2].Value = keyValue;
					selectTableVersion.ExecuteNonQuery();
					return 0;
				});
			}
			position += count;
		}
	}

	private const int SQLBUFFERSIZE = 8040;

	private readonly Stream innerStream;

	public override bool CanRead => innerStream.CanRead;

	public override bool CanSeek => innerStream.CanSeek;

	public override bool CanWrite => innerStream.CanWrite;

	public override long Length => innerStream.Length;

	public override long Position
	{
		get
		{
			return innerStream.Position;
		}
		set
		{
			innerStream.Position = value;
		}
	}

	public BlobWriterStream(NavSqlConnection connection, string schemaName, string tableName, string blobColumn, string keyColumn, SqlDbType keyColumnType, object keyValue, bool isTenantTable, int? companyId)
	{
		innerStream = new BufferedStream(new InnerBlobWriterStream(connection, schemaName, tableName, blobColumn, keyColumn, keyColumnType, keyValue, isTenantTable, companyId), 8040);
	}

	public override void Flush()
	{
		innerStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return innerStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		innerStream.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return innerStream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		innerStream.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			innerStream.Dispose();
		}
	}
}
