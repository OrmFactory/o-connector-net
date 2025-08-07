using OBridgeConnector.OracleTypes;

namespace OBridgeConnector.ValueObjects;

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

	public override void ReadFromSpan(ref SpanReader reader)
	{
		interval = new();
		var meta = reader.ReadByte();
		var hasYears = (meta & 0x01) != 0;
		var hasMonths = (meta & 0x02) != 0;
		var isNegative = (meta & 0x80) != 0;
		if (hasYears) interval.Years = reader.Read7BitEncodedInt();
		if (hasMonths) interval.Months = reader.Read7BitEncodedInt();
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

	public override Type GetDefaultType()
	{
		return typeof(OracleIntervalYM);
	}
}