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

	public abstract string GetString();
	public abstract object GetValue();
}

public class IntervalYearToMonthValue : ValueObject
{
	private OracleIntervalYM interval = new();

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		interval = new();
		var meta = await reader.ReadByte(token);
		var hasYears = (meta & 0x01) != 0;
		var hasMonths = (meta & 0x02) != 0;
		var isNegative = (meta & 0x80) != 0;
		if (hasYears) interval.Years = await reader.Read7BitEncodedInt(token);
		if (hasMonths) interval.Months = await reader.Read7BitEncodedInt(token);
		if (isNegative)
		{
			interval.Years = -interval.Years;
			interval.Months = -interval.Months;
		}
	}

	public override OracleIntervalYM GetOracleIntervalYM()
	{
		return interval;
	}

	public override string GetString()
	{
		return interval.ToString();
	}

	public override string ToString()
	{
		return interval.ToString();
	}

	public override object GetValue()
	{
		return interval;
	}
}