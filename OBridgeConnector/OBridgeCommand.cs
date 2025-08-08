using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System.Threading;

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
		CancelAsync(CancellationToken.None).GetAwaiter().GetResult();
	}

	public async Task CancelAsync(CancellationToken token)
	{
		var request = new Request(CommandEnum.CancelFetch);
		await request.SendAsync(connection.Stream, token);
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
	protected override DbTransaction? DbTransaction { get; set; }
	public override bool DesignTimeVisible { get; set; }

	private readonly OBridgeParameterCollection parameterCollection = new();
	protected override DbParameterCollection DbParameterCollection => parameterCollection;

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
		AddParameters(request);
		var reader = await connection.RequestReader(request, this, token);
		return reader;
	}

	private void AddParameters(Request request)
	{
		var parameters = parameterCollection.Cast<OBridgeParameter>().ToList();
		request.Write7BitEncodedInt(parameters.Count);
		foreach (var parameter in parameters)
		{
			parameter.Serialize(request);
		}
	}

	public override async Task<int> ExecuteNonQueryAsync(CancellationToken token)
	{
		await using var reader = await ExecuteReaderAsync(token).ConfigureAwait(false);

		while (await reader.ReadAsync(token).ConfigureAwait(false))
		{
		}

		return reader.RecordsAffected;
	}

	public override async Task<object?> ExecuteScalarAsync(CancellationToken token)
	{
		await using var reader = await ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SingleRow, token);
		if (await reader.ReadAsync(token))
			return reader.GetValue(0);
		return null;
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