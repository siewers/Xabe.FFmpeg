namespace Xabe.FFmpeg.Exceptions;

using System;
using System.Diagnostics.CodeAnalysis;

internal sealed class ExceptionCheck(string searchPhrase, bool containsFileIsEmptyMessage, Func<string, string, Exception> exceptionFactory)
{
    /// <summary>
    ///     Checks output log and throws exception - some errors are only fatal if the text "Output file is empty" is found in
    ///     the log
    /// </summary>
    /// <param name="log"></param>
    internal bool CheckLog(string log)
    {
        return log.Contains(searchPhrase) && (!containsFileIsEmptyMessage || log.Contains("Output file is empty"));
    }

    [DoesNotReturn]
    public void Throw(string errorMessage, string inputParameters)
    {
        throw exceptionFactory(errorMessage, inputParameters);
    }
}
