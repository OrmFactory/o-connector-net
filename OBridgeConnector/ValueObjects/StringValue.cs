namespace OBridgeConnector.ValueObjects;

public class StringValue : ValueObject
{
	private string value = "";

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		value = await reader.ReadString(token);
	}

	public override void ReadFromSpan(ref SpanReader reader)
	{
		value = reader.ReadString();
	}

	public override string GetString()
	{
		return value;
	}

	public override string ToString()
	{
		return value;
	}

	public override object GetValue()
	{
		return value;
	}

	public override Type GetDefaultType()
	{
		return typeof(string);
	}
}