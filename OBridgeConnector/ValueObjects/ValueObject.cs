using OBridgeConnector.OracleTypes;

namespace OBridgeConnector.ValueObjects;

public abstract class ValueObject
{
	public abstract Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token);

	public virtual bool GetBoolean() => throw new InvalidCastException();
	public virtual byte GetByte() => throw new InvalidCastException();
	public virtual char GetChar() => throw new InvalidCastException();
	public virtual long GetBytes(long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new InvalidCastException();
	public virtual long GetChars(long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new InvalidCastException();
	public virtual DateTime GetDateTime() => throw new InvalidCastException();
	public virtual DateTimeOffset GetDateTimeOffset() => throw new InvalidCastException();
	public virtual decimal GetDecimal() => throw new InvalidCastException();
	public virtual double GetDouble() => throw new InvalidCastException();
	public virtual float GetFloat() => throw new InvalidCastException();
	public virtual Guid GetGuid() => throw new InvalidCastException();
	public virtual short GetInt16() => throw new InvalidCastException();
	public virtual int GetInt32() => throw new InvalidCastException();
	public virtual long GetInt64() => throw new InvalidCastException();
	public virtual OracleIntervalYM GetOracleIntervalYM() => throw new InvalidCastException();
	public virtual OracleIntervalDS GetOracleIntervalDS() => throw new InvalidCastException();
	public virtual TimeSpan GetTimeSpan() => throw new InvalidCastException();

	public abstract string GetString();
	public abstract object GetValue();
}