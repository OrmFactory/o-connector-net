using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace OBridgeConnector;

public class OBridgeCommand : DbCommand
{
	private readonly OBridgeConnection connection;

	public OBridgeCommand(OBridgeConnection connection)
	{
		this.connection = connection;
	}

	public override void Cancel()
	{
		throw new NotImplementedException();
	}

	public override int ExecuteNonQuery()
	{
		return ExecuteNonQueryAsync().GetAwaiter().GetResult();
	}

	public override object? ExecuteScalar()
	{
		return ExecuteScalarAsync().GetAwaiter().GetResult();
	}

	public override void Prepare()
	{
		PrepareAsync(CancellationToken.None).GetAwaiter().GetResult();
	}

	public override async Task PrepareAsync(CancellationToken token)
	{
		
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
		return new OBridgeParameter();
	}

	protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
	{
		return ExecuteDbDataReaderAsync(behavior, CancellationToken.None).GetAwaiter().GetResult();
	}

	protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken token)
	{
		var request = new Request(CommandEnum.Query);
		request.WriteByte((byte)behavior);
		request.WriteString(CommandText);
		var reader = await connection.RequestReader(request, token);
		return reader;
	}

	public override Task<int> ExecuteNonQueryAsync(CancellationToken token)
	{
		return base.ExecuteNonQueryAsync(token);
	}

	public override Task<object?> ExecuteScalarAsync(CancellationToken token)
	{
		return base.ExecuteScalarAsync(token);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public override ValueTask DisposeAsync()
	{
		return base.DisposeAsync();
	}
}