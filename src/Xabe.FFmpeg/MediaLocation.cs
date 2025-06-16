namespace Xabe.FFmpeg;

public readonly record struct MediaLocation
{
    private readonly Uri? _uri;

    private MediaLocation(Uri uri)
        : this(uri.AbsoluteUri)
    {
    }

    private MediaLocation(string location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        location = location.Trim('"').Trim('\''); // Remove surrounding quotes if present

        if (Uri.TryCreate(location, UriKind.Absolute, out var uri))
        {
            _uri = uri;
            return;
        }

        try
        {
            var fullPath = Path.GetFullPath(location);
            uri = new Uri(fullPath);

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException($"Could not convert path to an absolute URI: {location}.", nameof(location));
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid location format. Could not parse as an absolute URI or a valid file path: {location}", nameof(location), ex);
        }

        _uri = uri;
    }

    public static MediaLocation Create(Uri location)
    {
        return new MediaLocation(location);
    }

    public static MediaLocation Create(string location)
    {
        return new MediaLocation(location);
    }

    public static implicit operator MediaLocation(string location)
    {
        return new MediaLocation(location);
    }

    public static implicit operator MediaLocation(Uri uri)
    {
        return new MediaLocation(uri);
    }

    public static implicit operator MediaLocation(FileInfo fileInfo)
    {
        return new MediaLocation(fileInfo.FullName);
    }

    public static implicit operator string(MediaLocation mediaLocation)
    {
        return mediaLocation.ToString();
    }

    public static implicit operator FileInfo(MediaLocation mediaLocation)
    {
        var uri = mediaLocation._uri;

        if (uri is null || !uri.IsFile)
        {
            throw new InvalidOperationException("The MediaLocation does not represent a file URI.");
        }

        return new FileInfo(uri.LocalPath);
    }

    public bool Exists()
    {
        if (_uri is null)
        {
            return false;
        }

        // For non-file URIs, existence check is not applicable
        return !_uri.IsFile || File.Exists(_uri.LocalPath);
    }

    public override string ToString()
    {
        return _uri is not null
            ? _uri.IsFile ? _uri.LocalPath : _uri.AbsoluteUri
            : string.Empty;
    }

    internal string Escape()
    {
        return ToString().Escape();
    }
}
