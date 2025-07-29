using System.Data.Common;

namespace OBridgeConnector;

public class OBridgeColumn : DbColumn
{
	public static async Task<OBridgeColumn> FromReader(int columnOrdinal, AsyncBinaryReader reader, CancellationToken token)
	{

	}


}

public abstract class ValueObject
{
	public abstract Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token);

	public virtual bool GetBoolean() => throw new NotSupportedException();
	public virtual byte GetByte() => throw new NotSupportedException();
	public virtual char GetChar() => throw new NotSupportedException();
	public virtual long GetBytes(long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
	public virtual long GetChars(long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
	public virtual DateTime GetDateTime() => throw new NotSupportedException();
	public virtual decimal GetDecimal() => throw new NotSupportedException();
	public virtual double GetDouble() => throw new NotSupportedException();
	public virtual float GetFloat() => throw new NotSupportedException();
	public virtual Guid GetGuid() => throw new NotSupportedException();
	public virtual short GetInt16() => throw new NotSupportedException();
	public virtual int GetInt32() => throw new NotSupportedException();
	public virtual long GetInt64() => throw new NotSupportedException();

	public abstract string GetString();
}

public class DateTimeValue : ValueObject
{
	private readonly int precision;

	private int year;
	private int month;
	private int day;
	private int hour;
	private int minute;
	private int second;
	private int nanosecond;
	private int timeZoneOffsetMinutes;

	bool isDateOnly;
	bool hasFraction;
	bool hasTimezone;

	public DateTimeValue(int precision)
	{
		this.precision = precision;
	}

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		var bits = new AsyncBitReader(reader);

		isDateOnly = await bits.ReadBit(token);
		hasFraction = await bits.ReadBit(token);
		hasTimezone = await bits.ReadBit(token);


		year = await bits.ReadSignedBits(15, token);
		month = await bits.ReadBits(4, token);
		day = await bits.ReadBits(5, token);

		if (isDateOnly) return;

		hour = await bits.ReadBits(5, token);
		minute = await bits.ReadBits(6, token);
		second = await bits.ReadBits(6, token);

		nanosecond = 0;
		if (hasFraction && precision > 0)
		{
			int bitLength = FractionBitLengths[precision];
			int scaled = await bits.ReadBits(bitLength, token);
			nanosecond = scaled * PowersOf10[9 - precision];
		}

		if (hasTimezone)
		{
			timeZoneOffsetMinutes = await bits.ReadSignedBits(11, token);
		}
	}

	public override string GetString()
	{
		return ToString();
	}

	public override string ToString()
	{
		var date = $"{year:D4}-{month:D2}-{day:D2}";
		if (isDateOnly) return date;

		var dateTime = date +  $" {hour:D2}:{minute:D2}:{second:D2}";

		if (hasFraction)
		{
			var fraction = nanosecond.ToString().PadLeft(9, '0').Substring(0, precision);
			dateTime += $".{fraction}";
		}

		if (hasTimezone)
		{
			var offset = timeZoneOffsetMinutes;
			var sign = offset < 0 ? '-' : '+';
			offset = Math.Abs(offset);
			var tzHour = offset / 60;
			var tzMin = offset % 60;
			return $"{dateTime}{sign}{tzHour:D2}:{tzMin:D2}";
		}
		return dateTime;
	}

	private static readonly int[] FractionBitLengths = new int[]
	{
		0,  // precision 0
		4,  // precision 1
		7,  // precision 2
		10, // precision 3
		14, // precision 4
		17, // precision 5
		20, // precision 6
		24, // precision 7
		27, // precision 8
		30  // precision 9
	};

	private static readonly int[] PowersOf10 = new int[]
	{
		1,              // 10^0
		10,             // 10^1
		100,            // 10^2
		1_000,          // 10^3
		10_000,         // 10^4
		100_000,        // 10^5
		1_000_000,      // 10^6
		10_000_000,     // 10^7
		100_000_000,    // 10^8
		1_000_000_000   // 10^9
	};
}