namespace OBridgeConnector.ValueObjects;

public class NullValueObject : ValueObject
{
	public static NullValueObject Instance = new NullValueObject();

	private NullValueObject()
	{

	}

	public override Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		return Task.CompletedTask;
	}

	public override string GetString()
	{
		throw new NullReferenceException();
	}

	public override object GetValue()
	{
		return null;
	}

	public override Type GetDefaultType()
	{
		throw new NullReferenceException();
	}
}