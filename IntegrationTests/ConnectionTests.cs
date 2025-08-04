using OBridgeConnector;

namespace IntegrationTests;

public class ConnectionTests
{
	private const string ConnectionString = "BridgeHost=localhost;ServerName=srt1;Username=test;Password=test;SslMode=None";

	[Fact]
	public async Task CanOpenConnection()
	{
		await using var connection = new OBridgeConnection(ConnectionString);
		await connection.OpenAsync();
		Assert.Equal(System.Data.ConnectionState.Open, connection.State);
	}
}