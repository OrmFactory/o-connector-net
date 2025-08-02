namespace OBridgeConnector.ValueObjects;

public enum TimeZoneEnum
{
	WithoutTimeZone,
	WithTimeZone,
	LocalTimeZone
}

public class DateTimeValue : ValueObject
{
	private readonly int precision;
	private readonly TimeZoneEnum timeZone;

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

	public DateTimeValue(int precision, TimeZoneEnum timeZone)
	{
		this.precision = precision;
		this.timeZone = timeZone;
	}

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		hour = 0;
		minute = 0;
		second = 0;
		nanosecond = 0;
		timeZoneOffsetMinutes = 0;

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

	public override DateTime GetDateTime()
	{
		var kind = DateTimeKind.Unspecified;
		if (timeZone == TimeZoneEnum.LocalTimeZone) kind = DateTimeKind.Utc;
		
		var dt = new DateTime(year, month, day, hour, minute, second, kind).AddTicks(nanosecond / 100);

		if (timeZone == TimeZoneEnum.LocalTimeZone)
		{
			return dt.ToLocalTime();
		}

		if (timeZone == TimeZoneEnum.WithTimeZone)
		{
			return dt.AddMinutes(timeZoneOffsetMinutes);
		}

		return dt;
	}

	public override DateTimeOffset GetDateTimeOffset()
	{
		var baseTime = new DateTime(year, month, day, hour, minute, second).AddTicks(nanosecond / 100);

		if (timeZone == TimeZoneEnum.WithTimeZone)
		{
			return new DateTimeOffset(baseTime, TimeSpan.FromMinutes(timeZoneOffsetMinutes));
		}

		var offset = TimeZoneInfo.Local.GetUtcOffset(baseTime);
		return new DateTimeOffset(baseTime, offset);
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

	public override object GetValue()
	{
		if (timeZone == TimeZoneEnum.WithTimeZone)
			return GetDateTimeOffset();

		return GetDateTime();
	}

	public override Type GetDefaultType()
	{
		if (timeZone == TimeZoneEnum.WithTimeZone) return typeof(DateTimeOffset);
		return typeof(DateTime);
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