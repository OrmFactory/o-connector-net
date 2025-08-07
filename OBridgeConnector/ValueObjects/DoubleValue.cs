using System.Globalization;

namespace OBridgeConnector.ValueObjects;

public class DoubleValue : ValueObject
{
	private double value;

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		value = await reader.ReadDouble(token);
	}

	public override void ReadFromSpan(ref SpanReader reader)
	{
		value = reader.ReadDouble();
	}

	public override decimal GetDecimal()
	{
		return (decimal)value;
	}

	public override double GetDouble()
	{
		return value;
	}

	public override float GetFloat()
	{
		return (float)value;
	}

	public override string GetString()
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public override string ToString()
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public override object GetValue()
	{
		return value;
	}

	public override Type GetDefaultType()
	{
		return typeof(double);
	}
}