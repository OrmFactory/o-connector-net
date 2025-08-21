using System.Globalization;

namespace OBridgeConnector.ValueObjects;

public class FloatValue : ValueObject
{
	private float value;

	public override void ReadFromBatch(BatchReader reader)
	{
		value = reader.ReadFloat();
	}

	public override float GetFloat()
	{
		return value;
	}

	public override double GetDouble()
	{
		return value;
	}

	public override decimal GetDecimal()
	{
		return (decimal)value;
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
		return typeof(float);
	}
}