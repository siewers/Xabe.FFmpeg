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

    public void Add(ConversionParameter item)
    {
        _items.Remove(item);
        _items.Add(item);
    }

    public ConversionParameters AddPostInput(string name)
    {
        Add(ConversionParameter.PostInput(name));
        return this;
    }

    public ConversionParameters AddPostInput<T>(string name, T value)
    {
        Add(ConversionParameter.PostInput(name, value));
        return this;
    }

    public ConversionParameters AddPreInput(string name)
    {
        Add(ConversionParameter.Create(name, ParameterPosition.PreInput));
        return this;
    }

    public ConversionParameters AddPreInput<T>(string name, T value)
    {
        Add(ConversionParameter.PreInput(name, value));
        return this;
    }

    public ConversionParameters Remove(string name)
    {
        _items.RemoveWhere(parameter => parameter.Name == name);
        return this;
    }
}
