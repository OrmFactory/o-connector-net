using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.Net.Sockets;
using ZstdSharp;

namespace OBridgeConnector;

public class OBridgeConnection : DbConnection
{
	private ConnectionState state = ConnectionState.Closed;
	public override ConnectionState State => state;

	[AllowNull]
	public override string ConnectionString { get; set; } = null;
	public override string Database { get; } = "";
	public override string DataSource { get; } = "";
	public override string ServerVersion { get; } = "";

	private OBridgeSession? session;

	public OBridgeConnection() {}

	public OBridgeConnection(string connectionString)
	{
		ConnectionString = connectionString;
	}

	private void SetState(ConnectionState newState)
	{
		if (state != newState)
		{
			var old = state;
			state = newState;
			OnStateChange(new StateChangeEventArgs(old, newState));
		}
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
		if (state == ConnectionState.Closed) return;
		if (session != null)
		{
			ConnectionPool.Return(session);
			session = null;
		}
		SetState(ConnectionState.Closed);
	}

	public override async Task CloseAsync()
	{
		if (state == ConnectionState.Closed) return;
		if (session != null)
		{
			ConnectionPool.Return(session);
			session = null;
		}
		SetState(ConnectionState.Closed);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Close();
		}
	}

	public override async ValueTask DisposeAsync()
	{
		if (session != null)
		{
			await session.DisposeAsync();
			session = null;
			SetState(ConnectionState.Closed);
		}
	}

	public override void Open()
	{
		OpenAsync(CancellationToken.None).GetAwaiter().GetResult();
	}

	public override async Task OpenAsync(CancellationToken token)
	{
		if (State != ConnectionState.Closed) return;

		try
		{
			state = ConnectionState.Connecting;
			session = await ConnectionPool.CreateSession(ConnectionString);
			await session.OpenConnectionAsync(token);
			SetState(ConnectionState.Open);
		}
		catch (Exception)
		{
			await CloseAsync();
			throw;
		}
	}

	protected override DbCommand CreateDbCommand()
	{
		return new OBridgeCommand(this);
	}

	public async Task<OBridgeDataReader> RequestReader(Request request, OBridgeCommand command, CancellationToken token)
	{
		if (session == null) session = await ConnectionPool.CreateSession(ConnectionString);

		await session.SendRequest(request, token);
		SetState(ConnectionState.Open);

		var dbReader = new OBridgeDataReader(session.Reader, command);
		await dbReader.ReadHeader(token);
		return dbReader;
	}

	public async Task SendCancelCommandAsync(CancellationToken token)
	{
		var request = new Request(CommandEnum.CancelFetch);
		await request.SendAsync(session.Stream, token);
	}
}

public class OBridgeFactory : DbProviderFactory
{

}

public enum ResponseTypeEnum
{
	ConnectionSuccess = 0,
	TableHeader = 0x01,
	RowData = 0x02,
	EndOfRowStream = 0x03,
	Error = 0x10,
	OracleQueryError = 0x11,
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