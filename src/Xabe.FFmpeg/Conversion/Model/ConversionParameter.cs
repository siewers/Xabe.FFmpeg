namespace Xabe.FFmpeg;

using System;
using System.Collections.Generic;

internal sealed record ConversionParameter
{
    private ConversionParameter(string name, string? value = null, ParameterPosition position = ParameterPosition.PostInput)
    {
        Parameter = $"-{name.TrimStart('-').Trim()} {value?.Trim()} ";
        Key = name.Trim();
        Position = position;
    }

    public string Parameter { get; }

    public string Key { get; }

    public ParameterPosition Position { get; }

    public bool Equals(ConversionParameter? other)
    {
        return other is not null &&
               Key == other.Key &&
               Position == other.Position &&
               Key != "-i";
    }

    public override int GetHashCode()
    {
        var hashCode = 495346454;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
        hashCode = hashCode * -1521134295 + Position.GetHashCode();
        return hashCode;
    }

    public static ConversionParameter Create(string name, ParameterPosition position = ParameterPosition.PostInput)
    {
        return new ConversionParameter(name, null, position);
    }

    public static ConversionParameter Create(string name, TimeSpan value, ParameterPosition position = ParameterPosition.PostInput)
    {
        return new ConversionParameter(name, value.ToFFmpeg(), position);
    }

    public static ConversionParameter Create(string name, string value, ParameterPosition position = ParameterPosition.PostInput)
    {
        return new ConversionParameter(name, value, position);
    }

    public static ConversionParameter Create(string name, int value, ParameterPosition position = ParameterPosition.PostInput)
    {
        return new ConversionParameter(name, value.ToString(), position);
    }

    public static ConversionParameter Create(string name, long value, ParameterPosition position = ParameterPosition.PostInput)
    {
        return new ConversionParameter(name, value.ToString(), position);
    }
}
