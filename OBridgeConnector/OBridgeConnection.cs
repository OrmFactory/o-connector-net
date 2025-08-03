using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
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
	public Stream? Stream => stream;

	private TcpClient client = new TcpClient();
	private Stream? stream;
	private DecompressionStream? decompressionStream;
	private AsyncBinaryReader? reader;

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
		CloseAsync().GetAwaiter().GetResult();
	}

	public override void Open()
	{
		OpenAsync().GetAwaiter().GetResult();
	}

	public override async Task CloseAsync()
	{
		if (state == ConnectionState.Closed) return;
		state = ConnectionState.Closed;

		try
		{
			if (decompressionStream != null)
				await decompressionStream.DisposeAsync();
		}
		catch { }

		try
		{
			if (stream != null) await stream.DisposeAsync();
		} 
		catch { }

		reader = null;
		client.Close();
		stream = null;
		decompressionStream = null;
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
		await CloseAsync();
	}

	public override async Task OpenAsync(CancellationToken token)
	{
		if (State != ConnectionState.Closed) return;

		try
		{
			state = ConnectionState.Connecting;
			var builder = new OBridgeConnectionStringBuilder { ConnectionString = ConnectionString };
			await CreateTransport(builder, token);
			var request = GetConnectionRequest(builder);
			await request.SendAsync(stream, token);
			await ReadConnectionResponse(token);
			SetState(ConnectionState.Open);
		}
		catch (Exception e)
		{
			SetState(ConnectionState.Closed);
			stream?.Close();
			client.Close();
			stream = null;
			throw;
		}
	}

	protected override DbCommand CreateDbCommand()
	{
		return new OBridgeCommand(this);
	}

	private async Task CreateTransport(OBridgeConnectionStringBuilder builder, CancellationToken token)
	{
		var host = builder.BridgeHost;
		if (host == null) throw new ArgumentException("BridgeHost is required");
		var sslMode = builder.SslMode;
		if (sslMode == null) sslMode = SslMode.Require;

		var port = sslMode == SslMode.None ? 0x0f0f : 0x0fac;
		port = builder.BridgePort ?? port;
		await client.ConnectAsync(host, port, token);
		stream = client.GetStream();
		reader = new AsyncBinaryReader(stream);
	}

	private async Task ReadConnectionResponse(CancellationToken token)
	{
		var code = await reader.ReadByte(token);
		if (code == (byte)ResponseTypeEnum.Error) await ReadError(token);
		if (code == (byte)ResponseTypeEnum.ConnectionSuccess)
		{
			var compressionFlag = await reader.ReadByte(token);
			var protocolVersion = await reader.ReadByte(token);

			if (compressionFlag == 1)
			{
				decompressionStream = new DecompressionStream(stream);
				reader = new AsyncBinaryReader(decompressionStream);
			}
			return;
		}

		throw new Exception("Unknown response code " + code);
	}

	private async Task ReadError(CancellationToken token)
	{
		var errorCode = await reader.ReadByte(token);
		var errorMessage = await reader.ReadString(token);
		throw new Exception(errorMessage);
	}

	private Request GetConnectionRequest(OBridgeConnectionStringBuilder builder)
	{
		var request = new Request();
		request.WriteBytes("OCON"u8.ToArray());

		//protocol version
		request.WriteByte(1);

		byte useCompressionByte = 0;
		if (builder.Compression != false) useCompressionByte = 1;
		request.WriteByte(useCompressionByte);

		request.WriteByte(0);
		request.WriteByte(0);

		if (builder is { ServerName: not null, Username: not null, Password: not null })
		{
			request.WriteByte((byte)CommandEnum.ConnectNamed);
			request.WriteString(builder.ServerName);
			request.WriteString(builder.Username);
			request.WriteString(builder.Password);
			return request;
		}

		var conString = builder.ToOracleConnectionString();
		request.WriteByte((byte)CommandEnum.ConnectProxy);
		request.WriteString(conString);
		return request;
	}

	public async Task<OBridgeDataReader> RequestReader(Request request, CancellationToken token)
	{
		var builder = new OBridgeConnectionStringBuilder { ConnectionString = ConnectionString };

		if (State == ConnectionState.Closed)
		{
			await CreateTransport(builder, token);
			var connectionRequest = GetConnectionRequest(builder);
			await connectionRequest.SendAsync(stream, token);
			SetState(ConnectionState.Connecting);
		}

		await request.SendAsync(stream, token);

		if (State == ConnectionState.Connecting)
		{
			await ReadConnectionResponse(token);
			SetState(ConnectionState.Open);
		}

		var dbReader = new OBridgeDataReader(reader);
		await dbReader.ReadHeader(token);
		return dbReader;
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