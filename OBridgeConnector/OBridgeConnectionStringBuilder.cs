using System.Data.Common;

namespace OBridgeConnector;

public class OBridgeConnectionStringBuilder : DbConnectionStringBuilder
{
	public string? BridgeHost
	{
		get => GetString("BridgeHost");
		set => this["BridgeHost"] = value;
	}

	public int? BridgePort
	{
		get => GetInt("BridgePort");
		set => this["BridgePort"] = value;
	}

	public SslMode? SslMode
	{
		get
		{
			if (!TryGetValue("SslMode", out var value)) return null;
			return Enum.TryParse<SslMode>(value.ToString(), true, out var mode) ? mode : null;
		}
		set => this["SslMode"] = value?.ToString();
	}

	public bool? Compression
	{
		get => GetBool("Compression");
		set => this["Compression"] = value;
	}

	//internal login mode

	public string? ServerName
	{
		get => GetString("ServerName");
		set => this["ServerName"] = value;
	}

	public string? Username
	{
		get => GetString("Username");
		set => this["Username"] = value;
	}

	public string? Password
	{
		get => GetString("Password");
		set => this["Password"] = value;
	}

	//proxy login mode

	public string? OracleHost
	{
		get => GetString("OracleHost");
		set => this["OracleHost"] = value;
	}

	public int? OraclePort
	{
		get => GetInt("OraclePort");
		set => this["OraclePort"] = value;
	}

	public string? OracleSID
	{
		get => GetString("OracleSID");
		set => this["OracleSID"] = value;
	}

	public string? OracleServiceName
	{
		get => GetString("OracleServiceName");
		set => this["OracleServiceName"] = value;
	}

	public string? OracleUser
	{
		get => GetString("OracleUser");
		set => this["OracleUser"] = value;
	}

	public string? OraclePassword
	{
		get => GetString("OraclePassword");
		set => this["OraclePassword"] = value;
	}

	public string ToOracleConnectionString()
	{
		if (string.IsNullOrWhiteSpace(OracleHost))
			throw new ArgumentException("OracleHost is required");

		if (string.IsNullOrWhiteSpace(OracleUser))
			throw new ArgumentException("OracleUser is required");

		if (string.IsNullOrWhiteSpace(OraclePassword))
			throw new ArgumentException("OraclePassword is required");

		if (string.IsNullOrWhiteSpace(OracleSID) && string.IsNullOrWhiteSpace(OracleServiceName))
			throw new ArgumentException("Either OracleSID or OracleServiceName must be specified");

		var connectData = OracleSID != null
			? $"(SID={OracleSID})"
			: $"(SERVICE_NAME={OracleServiceName})";

		var address =
			$"(PROTOCOL=TCP)(HOST={OracleHost})"
			+ (OraclePort != null ? $"(PORT={OraclePort})" : "");

		var dataSource = $"(DESCRIPTION=(ADDRESS={address})(CONNECT_DATA={connectData}))";
		return $"User Id={OracleUser};Password={OraclePassword};Data Source={dataSource}";
	}

	protected string? GetString(string key)
		=> TryGetValue(key, out var value) ? value.ToString() : null;

	protected int? GetInt(string key)
		=> TryGetValue(key, out var value) ? Convert.ToInt32(value) : null;

	protected bool? GetBool(string key)
		=> TryGetValue(key, out var value) ? Convert.ToBoolean(value) : null;
}

public enum SslMode
{
	None,
	Require,
	Strict
}