using System.Collections;
using System.Data.Common;

namespace OBridgeConnector;

public class OBridgeParameterCollection : DbParameterCollection
{
	private List<OBridgeParameter> parameters = new();

	public override int Add(object value)
	{
		if (value is not OBridgeParameter p)
			throw new ArgumentException("Value must be an OBridgeParameter", nameof(value));
		parameters.Add(p);
		return parameters.Count - 1;
	}

	public override void AddRange(Array values)
	{
		if (values is null) throw new ArgumentNullException(nameof(values));
		foreach (var v in values)
			Add(v);
	}

	public override void Clear()
	{
		parameters.Clear();
	}

	public override bool Contains(object value)
	{
		return value is OBridgeParameter p && parameters.Contains(p);
	}

	public override bool Contains(string parameterName)
	{
		return parameters.Any(p => string.Equals(p.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase));
	}

	public override int IndexOf(object value)
	{
		if (value is not OBridgeParameter p)
			return -1;
		return parameters.IndexOf(p);
	}

	public override int IndexOf(string parameterName)
	{
		return parameters.FindIndex(p => string.Equals(p.ParameterName, parameterName, StringComparison.OrdinalIgnoreCase));
	}

	public override void Insert(int index, object value)
	{
		if (value is not OBridgeParameter p)
			throw new ArgumentException("Value must be an OBridgeParameter", nameof(value));
		parameters.Insert(index, p);
	}

	public override void Remove(object value)
	{
		parameters.Remove(value as OBridgeParameter);
	}

	public override void RemoveAt(int index)
	{
		parameters.RemoveAt(index);
	}

	public override void RemoveAt(string parameterName)
	{
		var index = IndexOf(parameterName);
		if (index >= 0) RemoveAt(index);
	}

	protected override void SetParameter(int index, DbParameter value)
	{
		if (value is not OBridgeParameter p)
			throw new ArgumentException("Value must be an OBridgeParameter", nameof(value));
		parameters[index] = p;
	}

	protected override void SetParameter(string parameterName, DbParameter value)
	{
		var index = IndexOf(parameterName);
		if (index < 0)
			throw new IndexOutOfRangeException($"Parameter '{parameterName}' not found.");
		SetParameter(index, value);
	}

	public override int Count => parameters.Count;
	public override object SyncRoot => ((ICollection) parameters).SyncRoot;

	public override void CopyTo(Array array, int index)
	{
		((ICollection)parameters).CopyTo(array, index);
	}

	public override IEnumerator GetEnumerator()
	{
		return parameters.GetEnumerator();
	}

	protected override DbParameter GetParameter(int index)
	{
		return parameters[index];
	}

	protected override DbParameter GetParameter(string parameterName)
	{
		var index = IndexOf(parameterName);
		if (index < 0)
			throw new IndexOutOfRangeException($"Parameter '{parameterName}' not found.");
		return parameters[index];
	}
}