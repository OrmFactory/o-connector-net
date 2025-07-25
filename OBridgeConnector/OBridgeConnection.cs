﻿using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace OBridgeConnector;

public class OBridgeConnection : DbConnection
{
	[AllowNull]
	public override string ConnectionString { get; set; } = null;
	public override string Database { get; } = "";
	public override ConnectionState State { get; } = ConnectionState.Closed;
	public override string DataSource { get; } = "";
	public override string ServerVersion { get; } = "";

	public OBridgeConnection() {}

	public OBridgeConnection(string connectionString)
	{
		ConnectionString = connectionString;
	}

	protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
	{
		throw new NotImplementedException();
	}

	public override void ChangeDatabase(string databaseName)
	{
		throw new NotImplementedException();
	}

	public override void Close()
	{
		CloseAsync().GetAwaiter().GetResult();
	}

	public override void Open()
	{
		OpenAsync().GetAwaiter().GetResult();
	}

	public override async Task CloseAsync()
	{
		
	}

	public override async Task OpenAsync(CancellationToken token)
	{
		
	}

	protected override DbCommand CreateDbCommand()
	{
		return new OBridgeCommand(this);
	}

	private Request GetConnectionRequest()
	{
		var request = new Request();
		request.WriteBytes("OCON"u8.ToArray());

		var builder = new OBridgeConnectionStringBuilder { ConnectionString = ConnectionString };
		byte useCompressionByte = 0;
		if (builder.Compression != false) useCompressionByte = 1;
		request.WriteByte(useCompressionByte);
	}

	public async Task<OBridgeDataReader> RequestReader(Request request, CancellationToken token)
	{
		
	}
}

public class OBridgeDataReader : DbDataReader
{
	public override bool GetBoolean(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override byte GetByte(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
	{
		throw new NotImplementedException();
	}

	public override char GetChar(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
	{
		throw new NotImplementedException();
	}

	public override string GetDataTypeName(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override DateTime GetDateTime(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override decimal GetDecimal(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override double GetDouble(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override Type GetFieldType(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override float GetFloat(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override Guid GetGuid(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override short GetInt16(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override int GetInt32(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override long GetInt64(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override string GetName(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override int GetOrdinal(string name)
	{
		throw new NotImplementedException();
	}

	public override string GetString(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override object GetValue(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override int GetValues(object[] values)
	{
		throw new NotImplementedException();
	}

	public override bool IsDBNull(int ordinal)
	{
		throw new NotImplementedException();
	}

	public override int FieldCount { get; }

	public override object this[int ordinal] => throw new NotImplementedException();

	public override object this[string name] => throw new NotImplementedException();

	public override int RecordsAffected { get; }
	public override bool HasRows { get; }
	public override bool IsClosed { get; }

	public override bool NextResult()
	{
		throw new NotImplementedException();
	}

	public override bool Read()
	{
		throw new NotImplementedException();
	}

	public override int Depth { get; }

	public override IEnumerator GetEnumerator()
	{
		throw new NotImplementedException();
	}
}

public class OBridgeParameter : DbParameter
{
	public override void ResetDbType()
	{
		throw new NotImplementedException();
	}

	public override DbType DbType { get; set; }
	public override ParameterDirection Direction { get; set; }
	public override bool IsNullable { get; set; }
	[AllowNull] public override string ParameterName { get; set; }
	[AllowNull] public override string SourceColumn { get; set; }
	public override object? Value { get; set; }
	public override bool SourceColumnNullMapping { get; set; }
	public override int Size { get; set; }
}

public class OBridgeFactory : DbProviderFactory
{

}

public enum CommandEnum
{
	ConnectNamed = 0x02,
	ConnectProxy = 0x03,
	BeginTransaction = 0x10,
	CommitTransaction = 0x11,
	RollbackTransaction = 0x12,
	Query = 0x20,
	QueryPrepared = 0x21,
	Prepare = 0x22,
	ClosePrepared = 0x23,
	CancelFetch = 0x30
}