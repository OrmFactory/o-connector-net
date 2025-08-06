using OBridgeConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests;

public class DataTypesTests
{
	[Fact]
	public async Task CanReadVariousNumberFormats()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
		SELECT
			CAST(NULL AS NUMBER) AS N1,
			CAST(42 AS NUMBER) AS N2,
			CAST(-123456 AS NUMBER(10)) AS N3,
			CAST(123456.78 AS NUMBER(10,2)) AS N4,
			CAST(99999999999999999999 AS NUMBER(20,0)) AS N5,
			CAST(0 AS NUMBER) AS N6,
			CAST(-1 AS NUMBER(1)) AS N7,
			CAST(0.000001 AS NUMBER(9, 6)) AS N8,
			CAST(99999999999999999999999999999999999999 AS NUMBER(38)) AS N9
		FROM DUAL";

		await using var reader = await command.ExecuteReaderAsync();

		Assert.True(await reader.ReadAsync());

		Assert.True(reader.IsDBNull(0));
		Assert.Equal(42, reader.GetInt32(1));
		Assert.Equal(-123456, reader.GetInt32(2));
		Assert.Equal(123456.78m, reader.GetDecimal(3));
		Assert.Equal(99999999999999999999m, reader.GetDecimal(4));
		Assert.Equal(0, reader.GetInt32(5));
		Assert.Equal(-1, reader.GetInt32(6));
		Assert.Equal(0.000001m, reader.GetDecimal(7));
		Assert.Equal("99999999999999999999999999999999999999", reader.GetString(8));
	}

	[Fact]
	public async Task MinusOne()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
		SELECT CAST(-1 AS NUMBER(1)) AS N7
		FROM DUAL";

		await using var reader = await command.ExecuteReaderAsync();
		Assert.True(await reader.ReadAsync());
		Assert.Equal(-1, reader.GetInt32(0));
	}

	[Fact]
	public async Task Number_WithVariousScale_ShouldPreserveTrailingZeroes()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
		SELECT
			CAST(1 AS NUMBER(5, 0))   AS N0,
			CAST(1.1 AS NUMBER(5, 1)) AS N1,
			CAST(1.12 AS NUMBER(5, 2)) AS N2,
			CAST(1.123 AS NUMBER(5, 3)) AS N3,
			CAST(1.1234 AS NUMBER(5, 4)) AS N4,
			CAST(1.12345 AS NUMBER(10, 5)) AS N5,
			CAST(1.123456 AS NUMBER(10, 6)) AS N6,
			CAST(1.1234567 AS NUMBER(10, 7)) AS N7,
			CAST(1.12345678 AS NUMBER(10, 8)) AS N8,
			CAST(1.123456789 AS NUMBER(10, 9)) AS N9
		FROM DUAL";

		await using var reader = await command.ExecuteReaderAsync();
		Assert.True(await reader.ReadAsync());

		string[] expectedStrings = new[]
		{
			"1",               // N0
			"1.1",             // N1
			"1.12",            // N2
			"1.123",           // N3
			"1.1234",          // N4
			"1.12345",         // N5
			"1.123456",        // N6
			"1.1234567",       // N7
			"1.12345678",      // N8
			"1.123456789"      // N9
		};

		decimal[] expectedDecimals = new[]
		{
			1m,
			1.1m,
			1.12m,
			1.123m,
			1.1234m,
			1.12345m,
			1.123456m,
			1.1234567m,
			1.12345678m,
			1.123456789m
		};

		for (int i = 0; i < expectedStrings.Length; i++)
		{
			Assert.Equal(expectedDecimals[i], reader.GetDecimal(i));
			Assert.Equal(expectedStrings[i], reader.GetString(i));
		}
	}

	//[Fact]
	//public async Task Number_WithPrecisionAndScale_ShouldMatchDecimalAndString()
	//{
	//	await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
	//	await connection.OpenAsync();

	//	await using var command = connection.CreateCommand();
	//	command.CommandText = @"
	//	SELECT
	//		CAST(0 AS NUMBER(10, 0)) AS N0,
	//		CAST(0.0 AS NUMBER(10, 1)) AS N1,
	//		CAST(0.00 AS NUMBER(10, 2)) AS N2,
	//		CAST(0.000 AS NUMBER(10, 3)) AS N3,
	//		CAST(1.1 AS NUMBER(10, 1)) AS N4,
	//		CAST(1.10 AS NUMBER(10, 2)) AS N5,
	//		CAST(1.100 AS NUMBER(10, 3)) AS N6,
	//		CAST(1.000000000 AS NUMBER(10, 9)) AS N7,
	//		CAST(1234567890 AS NUMBER(10, 0)) AS N8,
	//		CAST(123.456000000 AS NUMBER(12, 9)) AS N9
	//	FROM DUAL";

	//	await using var reader = await command.ExecuteReaderAsync();
	//	Assert.True(await reader.ReadAsync());

	//	(decimal expectedDecimal, string expectedString)[] expected = new[]
	//	{
	//		(0m, "0"),                  // N0
	//		(0.0m, "0.0"),              // N1
	//		(0.00m, "0.00"),            // N2
	//		(0.000m, "0.000"),          // N3
	//		(1.1m, "1.1"),              // N4
	//		(1.10m, "1.10"),            // N5
	//		(1.100m, "1.100"),          // N6
	//		(1.000000000m, "1.000000000"), // N7
	//		(1234567890m, "1234567890"),   // N8
	//		(123.456000000m, "123.456000000") // N9
	//	};

	//	for (int i = 0; i < expected.Length; i++)
	//	{
	//		var actualDecimal = reader.GetDecimal(i);
	//		var actualString = reader.GetString(i);

	//		Assert.Equal(expected[i].expectedDecimal, actualDecimal);
	//		Assert.Equal(expected[i].expectedString, actualString);
	//	}
	//}

	[Fact]
	public async Task OracleDate_ShouldPreserveTimeComponent()
	{
		await using var connection = new OBridgeConnection(ConnectionStrings.PlainConnectionString);
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
		SELECT
			DATE '0001-01-01' AS D1,
			TO_DATE('1900-12-31 23:59:59', 'YYYY-MM-DD HH24:MI:SS') AS D2,
			TO_DATE('1999-12-31 13:02:13', 'YYYY-MM-DD HH24:MI:SS') AS D3,
			TO_DATE('2025-08-06 12:34:56', 'YYYY-MM-DD HH24:MI:SS') AS D4,
			TO_DATE('9999-12-31 00:00:01', 'YYYY-MM-DD HH24:MI:SS') AS D5,
			SYSDATE AS D6
		FROM DUAL";

		await using var reader = await command.ExecuteReaderAsync();
		Assert.True(await reader.ReadAsync());

		Assert.Equal(new DateTime(1, 1, 1, 0, 0, 0), reader.GetDateTime(0));
		Assert.Equal("0001-01-01 00:00:00", reader.GetString(0));

		Assert.Equal(new DateTime(1900, 12, 31, 23, 59, 59), reader.GetDateTime(1));
		Assert.Equal("1900-12-31 23:59:59", reader.GetString(1));

		Assert.Equal(new DateTime(1999, 12, 31, 13, 2, 13), reader.GetDateTime(2));
		Assert.Equal("1999-12-31 13:02:13", reader.GetString(2));

		Assert.Equal(new DateTime(2025, 8, 6, 12, 34, 56), reader.GetDateTime(3));
		Assert.Equal("2025-08-06 12:34:56", reader.GetString(3));

		Assert.Equal(new DateTime(9999, 12, 31, 0, 0, 1), reader.GetDateTime(4));
		Assert.Equal("9999-12-31 00:00:01", reader.GetString(4));

		var d6 = reader.GetDateTime(5);
		var s6 = reader.GetString(5);
		var now = DateTime.Now;
		Assert.True((now - d6).Duration() < TimeSpan.FromMinutes(5));
		Assert.Equal(d6.ToString("yyyy-MM-dd HH:mm:ss"), s6);
	}
}