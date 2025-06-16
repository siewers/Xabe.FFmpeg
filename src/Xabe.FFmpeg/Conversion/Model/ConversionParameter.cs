namespace Xabe.FFmpeg;

internal readonly record struct ConversionParameter
{
    private ConversionParameter(string name, string? value, ParameterPosition position)
    {
        Name = name.Trim();
        Value = $"-{name.TrimStart('-').Trim()} {value?.Trim()} ";
        Position = position;
    }

    public string Name { get; }

    public string Value { get; }

    public ParameterPosition Position { get; }

    public bool Equals(ConversionParameter? other)
    {
        return other.HasValue &&
               Name == other.Value.Name &&
               Position == other.Value.Position &&
               Name is not "-i";
    }

    public override int GetHashCode()
    {
        var hashCode = 495346454;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
        hashCode = hashCode * -1521134295 + Position.GetHashCode();
        return hashCode;
    }

    public static ConversionParameter PostInput(string name)
    {
        return new ConversionParameter(name, value: null, ParameterPosition.PostInput);
    }

    public static ConversionParameter PostInput<T>(string name, T value)
    {
        return Create(name, value, ParameterPosition.PostInput);
    }

    public static ConversionParameter PreInput(string name)
    {
        return new ConversionParameter(name, value: null, ParameterPosition.PreInput);
    }

    public static ConversionParameter PreInput<T>(string name, T value)
    {
        return Create(name, value, ParameterPosition.PreInput);
    }

    public static ConversionParameter Create(string name, ParameterPosition position)
    {
        return new ConversionParameter(name, value: null, position);
    }

    private static ConversionParameter Create<T>(string name, T value, ParameterPosition position)
    {
        var stringValue = string.Format(FFmpegFormatProvider.Instance, "{0}", value);
        return Create(name, stringValue, position);
    }

    private static ConversionParameter Create(string name, string value, ParameterPosition position)
    {
        return new ConversionParameter(name, value, position);
    }

    private sealed class FFmpegFormatProvider : IFormatProvider, ICustomFormatter
    {
        public static readonly FFmpegFormatProvider Instance = new();

        private FFmpegFormatProvider()
        {
        }

        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            return arg switch
            {
                TimeSpan timeSpan => ToFFmpeg(timeSpan),
                _ => arg?.ToString() ?? string.Empty,
            };
        }

        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        /// <summary>
        ///     Returns FFmpeg formatted time.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan" /> to format</param>
        /// <returns>The FFmpeg formated time</returns>
        private static string ToFFmpeg(TimeSpan timeSpan)
        {
            var milliseconds = timeSpan.Milliseconds;
            var seconds = timeSpan.Seconds;
            var minutes = timeSpan.Minutes;
            var hours = (int)timeSpan.TotalHours;

            return $"{hours:D}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
        }
    }
}
