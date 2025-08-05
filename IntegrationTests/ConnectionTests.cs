using OBridgeConnector;

namespace IntegrationTests;

public class ConnectionTests
{
	private const string PlainConnectionString = "BridgeHost=localhost;ServerName=srv1;Username=test;Password=test;SslMode=None";
	private const string SslConnectionString = "BridgeHost=localhost;ServerName=srv1;Username=test;Password=test";

	[Fact]
	public async Task CanOpenPlainConnection()
	{
		await using var connection = new OBridgeConnection(PlainConnectionString);
		await connection.OpenAsync();
		Assert.Equal(System.Data.ConnectionState.Open, connection.State);
	}

	[Fact]
	public async Task CanOpenSslConnection()
	{
		await using var connection = new OBridgeConnection(SslConnectionString);
		await connection.OpenAsync();
		Assert.Equal(System.Data.ConnectionState.Open, connection.State);
	}
}