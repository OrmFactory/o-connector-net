using OBridgeConnector;
using OBridgeConnector.OracleTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBridgeConnector.OBridgeTypes;

namespace IntegrationTests;

public class ParametersTests
{
	[Fact]
	public async Task Parameter_Should_Roundtrip_To_Oracle_And_Back()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);

		await connection.OpenAsync();
		await using var command = connection.CreateCommand();
		command.CommandText = @"
		BEGIN
		    :p_out := :p_in + 100;
		END;";

		var pIn = new OBridgeParameter
		{
			ParameterName = "p_in",
			OBridgeDbType = OBridgeDbType.Int32,
			Direction = ParameterDirection.Input,
			Value = 23
		};
		var pOut = new OBridgeParameter
		{
			ParameterName = "p_out",
			OBridgeDbType = OBridgeDbType.Int32,
			Direction = ParameterDirection.Output
		};

		command.Parameters.Add(pIn);
		command.Parameters.Add(pOut);

		await command.ExecuteNonQueryAsync();
		Assert.Equal(123, pOut.Value);
	}
}