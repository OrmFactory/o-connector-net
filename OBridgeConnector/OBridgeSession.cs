using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace OBridgeConnector;

public class OBridgeSession : IAsyncDisposable, IDisposable
{
	public Stream? Stream => sslStream ?? stream;

	private TcpClient client = new();
	private Stream? stream;
	private SslStream? sslStream;
	private DecompressionStream? decompressionStream;
	
	private AsyncBinaryReader? reader;
	public AsyncBinaryReader Reader => reader;

	private bool isConnected = false;
	public bool IsConnected => isConnected;

	private readonly string connectionString;
	public string ConnectionString => connectionString;

	public bool IsExpired
	{
		get
		{
			if (client?.Client == null || !client.Client.Connected) return true;

			try
			{
				if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
					return true;

				return false;
			}
			catch
			{
				return true;
			}
		}
	}

	public OBridgeSession(string connectionString)
	{
		this.connectionString = connectionString;
	}

	public async Task SendRequest(Request request, CancellationToken token)
	{
		if (!IsConnected)
		{
			var builder = new OBridgeConnectionStringBuilder { ConnectionString = ConnectionString };
			await CreateTransport(builder, token);
			var connectionRequest = GetConnectionRequest(builder);
			connectionRequest.Append(request);
			request = connectionRequest;
		}

		await request.SendAsync(Stream, token);

		if (!IsConnected)
		{
			await ReadConnectionResponse(token);
			isConnected = true;
		}
	}

	public async Task OpenConnectionAsync(CancellationToken token)
	{
		if (isConnected) return;

		var builder = new OBridgeConnectionStringBuilder { ConnectionString = ConnectionString };
		await CreateTransport(builder, token);
		var request = GetConnectionRequest(builder);
		await request.SendAsync(Stream, token);
		await ReadConnectionResponse(token);
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

	private async Task CreateTransport(OBridgeConnectionStringBuilder builder, CancellationToken token)
	{
		var host = builder.BridgeHost ?? throw new ArgumentException("BridgeHost is required");
		var sslMode = builder.SslMode ?? SslMode.Require;

		var port = sslMode == SslMode.None ? 0x0f0f : 0x0fac;
		port = builder.BridgePort ?? port;

		await client.ConnectAsync(host, port, token);

		stream = client.GetStream();

		if (sslMode == SslMode.None)
		{
			reader = new AsyncBinaryReader(stream);
			return;
		}

		sslStream = new SslStream(stream, leaveInnerStreamOpen: false,
			userCertificateValidationCallback: (sender, cert, chain, errors) =>
			{
				if (sslMode == SslMode.Strict)
				{
					return errors == SslPolicyErrors.None;
				}
				return true;
			});

		await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
		{
			TargetHost = host,
			EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
			RemoteCertificateValidationCallback = null
		}, token);

		reader = new AsyncBinaryReader(sslStream);
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
				decompressionStream = new DecompressionStream(Stream);
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

	public void Dispose()
	{
		try { decompressionStream?.Dispose(); } catch { }
		try { stream?.Dispose(); } catch { }

		reader = null;
		client.Dispose();
		stream = null;
		sslStream = null;
		decompressionStream = null;
	}

	public async ValueTask DisposeAsync()
	{
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
		client.Dispose();
		stream = null;
		sslStream = null;
		decompressionStream = null;
	}
}

public class ConnectionPool
{
	private static readonly ConcurrentDictionary<string, ConcurrentBag<OBridgeSession>> pools = new();

	public static async Task<OBridgeSession> CreateSession(string connectionString)
	{
		if (pools.TryGetValue(connectionString, out var bag))
		{
			while (bag.TryTake(out var session))
			{
				if (!session.IsExpired) return session;
				await session.DisposeAsync();
			}
		}

		var newSession = new OBridgeSession(connectionString);
		return newSession;
	}

	public static void Return(OBridgeSession session)
	{
		if (session.IsExpired || !session.IsConnected)
		{
			session.Dispose();
			return;
		}

		var bag = pools.GetOrAdd(session.ConnectionString, _ => new ConcurrentBag<OBridgeSession>());
		bag.Add(session);
	}

	public static async Task ClearAsync()
	{
		foreach (var kvp in pools)
		{
			while (kvp.Value.TryTake(out var session))
			{
				await session.DisposeAsync();
			}
		}
		pools.Clear();
	}
}