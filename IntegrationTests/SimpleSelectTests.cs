using OBridgeConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests;

public class SimpleSelectTests
{
	[Fact]
	public async Task CanExecuteSimpleSelect()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = "SELECT 1 AS Value FROM DUAL";

		await using var reader = await command.ExecuteReaderAsync();

		Assert.True(await reader.ReadAsync());
		Assert.Equal(1, reader.GetInt32(0));
		Assert.False(await reader.ReadAsync());
	}

	[Fact]
	public async Task CanReadMultipleColumnTypes()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = "SELECT 1 AS Id, 'hello' AS Message, CURRENT_TIMESTAMP AS Now FROM DUAL";

		await using var reader = await command.ExecuteReaderAsync();

		Assert.True(await reader.ReadAsync());
		Assert.Equal(1, reader.GetInt32(0));
		Assert.Equal("hello", reader.GetString(1));
		Assert.True(reader.GetDateTime(2) <= DateTime.UtcNow.AddSeconds(10));
	}
}