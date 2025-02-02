namespace Xabe.FFmpeg;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     Get information about media file
/// </summary>
// ReSharper disable once InheritdocConsiderUsage
internal sealed class FFprobeWrapper : FFmpeg
{
    private async Task<ProbeModel?> GetProbeData(string videoFilePath, CancellationToken cancellationToken)
    {
        var arguments = $"-v panic -print_format json -show_format -show_streams {videoFilePath}";
        var result = await Start(arguments, cancellationToken);

        if (string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        var probeModel = JsonDeserializer.Deserialize<ProbeModel>(result);
        return probeModel;
    }

    private static double GetVideoFrameRate(VideoStreamModel videoStream, TimeSpan duration)
    {
        var frameCount = GetFrameCount(videoStream);
        var fr = videoStream.r_frame_rate.Split('/');

        if (frameCount > 0)
        {
            return Math.Round(frameCount / duration.TotalSeconds, 3);
        }

        return Math.Round(double.Parse(fr[0]) / double.Parse(fr[1]), 3);
    }

    private static long GetFrameCount(StreamModelBase videoStream)
    {
        return long.TryParse(videoStream.nb_frames, out var frameCount) ? frameCount : 0;
    }

    private static string GetVideoAspectRatio(int width, int height)
    {
        var cd = GetGcd(width, height);

        if (cd <= 0)
        {
            return "0:0";
        }

        return width / cd + ":" + height / cd;
    }

    private static TimeSpan GetStreamDuration(StreamModelBase streamModel, FormatModel formatModel)
    {
        return streamModel.Duration ?? streamModel.Tags.Duration ?? formatModel.Duration;
    }

    private static int GetGcd(int width, int height)
    {
        while (width != 0 &&
               height != 0)
        {
            if (width > height)
            {
                width -= height;
            }
            else
            {
                height -= width;
            }
        }

        return width == 0 ? height : width;
    }

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
                                                                                      if (!processExited &&
                                                                                          !process.HasExited)
                                                                                      {
                                                                                          process.CloseMainWindow();
                                                                                          process.Kill();
                                                                                      }
                                                                                  }
                                                                                  catch
                                                                                  {
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

    /// <summary>
    ///     Get properties from media file
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="mediaInfo">Empty media info</param>
    /// <returns>Properties</returns>
    public async Task<MediaInfo> SetProperties(MediaInfo mediaInfo, CancellationToken cancellationToken)
    {
        var path = mediaInfo.Path.Escape();

        var probeResult = await GetProbeData(path, cancellationToken);

        if (probeResult is null)
        {
            throw new ArgumentException($"Invalid file. Cannot load file {path}");
        }

        if (probeResult.Format is null)
        {
            throw new ArgumentException($"Invalid file. No format found {path}");
        }

        if (probeResult.Streams is null || probeResult.Streams.Length == 0)
        {
            throw new ArgumentException($"Invalid file. No streams found {path}");
        }

        mediaInfo.Size = probeResult.Format.Size;
        mediaInfo.CreationTime = probeResult.Format.Tags.CreationTime?.UtcDateTime;
        mediaInfo.VideoStreams = PrepareVideoStreams(probeResult);
        mediaInfo.AudioStreams = PrepareAudioStreams(probeResult);
        mediaInfo.SubtitleStreams = PrepareSubtitleStreams(probeResult);
        mediaInfo.Duration = probeResult.Format.Duration;
        return mediaInfo;
    }

    private static IEnumerable<IVideoStream> PrepareVideoStreams(ProbeModel probeModel)
    {
        return probeModel.Streams.OfType<VideoStreamModel>().Select(model => new VideoStream(model, probeModel.Format));
    }

    private static IEnumerable<IAudioStream> PrepareAudioStreams(ProbeModel probeModel)
    {
        return probeModel.Streams
                         .OfType<AudioStreamModel>()
                         .Select(model => new AudioStream
                                          {
                                              Codec = model.CodecName,
                                              Duration = GetStreamDuration(model, probeModel.Format),
                                              Path = probeModel.Format.FileName,
                                              Index = model.Index,
                                              Bitrate = Math.Abs(model.BitRate ?? model.Tags.BitRate ?? 0),
                                              Channels = model.Channels,
                                              ChannelLayout = model.ChannelLayout,
                                              SampleRate = model.SampleRate,
                                              Language = model.Tags.Language,
                                              IsDefault = model.Disposition.IsDefault,
                                              Title = model.Tags.Title,
                                              IsForced = model.Disposition.IsForced,
                                          }
                                );
    }

    private static IEnumerable<ISubtitleStream> PrepareSubtitleStreams(ProbeModel probeModel)
    {
        return probeModel.Streams
                         .OfType<SubtitleStreamModel>()
                         .Select(model => new SubtitleStream
                                          {
                                              Codec = model.CodecName,
                                              Path = probeModel.Format.FileName,
                                              Index = model.Index,
                                              Language = model.Tags.Language,
                                              Title = model.Tags.Title,
                                              IsDefault = model.Disposition.IsDefault,
                                              IsForced = model.Disposition.IsForced,
                                          }
                                );
    }
}
