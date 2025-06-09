namespace Xabe.FFmpeg.Probe;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Models;

/// <summary>
///     Get information about media file
/// </summary>
internal sealed class FFprobeWrapper : FFmpeg
{
    public Task<string> Start(string args, CancellationToken cancellationToken)
    {
        return RunProcess(args, cancellationToken);
    }

    private async Task<string> RunProcess(string args, CancellationToken cancellationToken)
    {
        return await Task.Factory.StartNew(() =>
                                           {
                                               using (var process = RunProcess(args, FFprobePath, null, standardOutput: true))
                                               {
                                                   var processExited = false;
                                                   cancellationToken.Register(() =>
                                                                              {
                                                                                  try
                                                                                  {
                                                                                      if (!processExited && !process.HasExited)
                                                                                      {
                                                                                          process.CloseMainWindow();
                                                                                          process.Kill();
                                                                                      }
                                                                                  }
                                                                                  catch
                                                                                  {
                                                                                      // ignored
                                                                                  }
                                                                              }
                                                                             );

                                                   var output = process.StandardOutput.ReadToEnd();
                                                   process.WaitForExit();
                                                   processExited = true;
                                                   return output;
                                               }
                                           },
                                           cancellationToken,
                                           TaskCreationOptions.LongRunning,
                                           TaskScheduler.Default
                                          );
    }

    public async Task<ProbeModel> GetProbeModel(FileInfo mediaFile, CancellationToken cancellationToken)
    {
        var arguments = $"-v panic -print_format json -show_format -show_streams {mediaFile.FullName}";
        var probeResult = await Start(arguments, cancellationToken);

        if (string.IsNullOrWhiteSpace(probeResult))
        {
            throw new ArgumentException($"Invalid file. Cannot load file {mediaFile.FullName}.");
        }

        var probeData = JsonDeserializer.Deserialize<ProbeModel>(probeResult);

        if (probeData is null)
        {
            throw new ArgumentException($"Invalid file. Cannot deserialize probe data {mediaFile.FullName}.");
        }

        if (probeData.Format is null)
        {
            throw new ArgumentException($"Invalid file. No format found {mediaFile.FullName}.");
        }

        if (probeData.Streams is null || probeData.Streams.Length == 0)
        {
            throw new ArgumentException($"Invalid file. No streams found {mediaFile.FullName}.");
        }

        return probeData;
    }
}
