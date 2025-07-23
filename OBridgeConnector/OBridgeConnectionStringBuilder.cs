using System.Data.Common;

namespace OBridgeConnector;

public abstract class BaseBridgeConnectionStringBuilder : DbConnectionStringBuilder
{
	protected BaseBridgeConnectionStringBuilder() { }

	public string? BridgeHost
	{
		get => GetOptionalString("BridgeHost");
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

	protected string? GetOptionalString(string key)
		=> TryGetValue(key, out var value) ? value?.ToString() : null;

	protected int? GetInt(string key)
		=> TryGetValue(key, out var value) ? Convert.ToInt32(value) : null;

	protected bool? GetBool(string key)
		=> TryGetValue(key, out var value) ? Convert.ToBoolean(value) : null;
}

public class BridgeNamedConnectionBuilder : BaseBridgeConnectionStringBuilder
{
	public string? ServerName
	{
		get => GetOptionalString("ServerName");
		set => this["ServerName"] = value;
	}

	public string? Username
	{
		get => GetOptionalString("Username");
		set => this["Username"] = value;
	}

	public string? Password
	{
		get => GetOptionalString("Password");
		set => this["Password"] = value;
	}
}

public class BridgeProxyConnectionBuilder : BaseBridgeConnectionStringBuilder
{
	public string? OracleHost
	{
		get => GetOptionalString("OracleHost");
		set => this["OracleHost"] = value;
	}

	public int? OraclePort
	{
		get => GetInt("OraclePort");
		set => this["OraclePort"] = value;
	}

	public string? OracleSID
	{
		get => GetOptionalString("OracleSID");
		set => this["OracleSID"] = value;
	}

	public string? OracleServiceName
	{
		get => GetOptionalString("OracleServiceName");
		set => this["OracleServiceName"] = value;
	}

	public string? OracleUser
	{
		get => GetOptionalString("OracleUser");
		set => this["OracleUser"] = value;
	}

	public string? OraclePassword
	{
		get => GetOptionalString("OraclePassword");
		set => this["OraclePassword"] = value;
	}
}

public enum SslMode
{
	None,
	Require,
	Strict
}