using OBridgeConnector;

namespace IntegrationTests;

public class ConnectionTests
{

	[Fact]
	public async Task CanOpenPlainConnection()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
		await connection.OpenAsync();
		Assert.Equal(System.Data.ConnectionState.Open, connection.State);
	}

	[Fact]
	public async Task CanOpenSslConnection()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.SslConnectionString);
		await connection.OpenAsync();
		Assert.Equal(System.Data.ConnectionState.Open, connection.State);
	}
}