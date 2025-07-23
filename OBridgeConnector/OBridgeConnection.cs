using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace OBridgeConnector;

public class OBridgeConnection : DbConnection
{
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
		throw new NotImplementedException();
	}

	public override void Open()
	{
		throw new NotImplementedException();
	}

	[AllowNull] 
	public override string ConnectionString { get; set; }
	public override string Database { get; }
	public override ConnectionState State { get; }
	public override string DataSource { get; }
	public override string ServerVersion { get; }

	protected override DbCommand CreateDbCommand()
	{
		throw new NotImplementedException();
	}
}

public class OBridgeCommand : DbCommand
{
	public override void Cancel()
	{
		throw new NotImplementedException();
	}

	public override int ExecuteNonQuery()
	{
		throw new NotImplementedException();
	}

	public override object? ExecuteScalar()
	{
		throw new NotImplementedException();
	}

	public override void Prepare()
	{
		throw new NotImplementedException();
	}

	[AllowNull] public override string CommandText { get; set; }
	public override int CommandTimeout { get; set; }
	public override CommandType CommandType { get; set; }
	public override UpdateRowSource UpdatedRowSource { get; set; }
	protected override DbConnection? DbConnection { get; set; }
	protected override DbParameterCollection DbParameterCollection { get; }
	protected override DbTransaction? DbTransaction { get; set; }
	public override bool DesignTimeVisible { get; set; }

	protected override DbParameter CreateDbParameter()
	{
		throw new NotImplementedException();
	}

	protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
	{
		throw new NotImplementedException();
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