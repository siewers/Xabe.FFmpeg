namespace Xabe.FFmpeg;

using System.Collections;

internal sealed class ConversionParameters : IEnumerable<ConversionParameter>
{
    private readonly HashSet<ConversionParameter> _items = [];

    public IEnumerator<ConversionParameter> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void Add(ConversionParameter item)
    {
        _items.Remove(item);
        _items.Add(item);
    }

    public ConversionParameters Add(string name, ParameterPosition position = ParameterPosition.PostInput)
    {
        Add(ConversionParameter.Create(name, position));
        return this;
    }

    public ConversionParameters Add(string name, TimeSpan value, ParameterPosition position = ParameterPosition.PostInput)
    {
        Add(ConversionParameter.Create(name, value, position));
        return this;
    }

    public ConversionParameters Add(string name, string value, ParameterPosition position = ParameterPosition.PostInput)
    {
        Add(ConversionParameter.Create(name, value, position));
        return this;
    }

    public ConversionParameters Add(string name, int value, ParameterPosition position = ParameterPosition.PostInput)
    {
        Add(ConversionParameter.Create(name, value, position));
        return this;
    }

    public ConversionParameters Add(string name, long value, ParameterPosition position = ParameterPosition.PostInput)
    {
        Add(ConversionParameter.Create(name, value, position));
        return this;
    }

    public ConversionParameters Remove(string name, ParameterPosition position = ParameterPosition.PostInput)
    {
        _items.Remove(ConversionParameter.Create(name, position));
        return this;
    }

    internal void Remove(ConversionParameter item)
    {
        _items.Remove(item);
    }

    internal void Clear()
    {
        _items.Clear();
    }
}
