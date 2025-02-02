using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xabe.FFmpeg
{
    /// <inheritdoc />
    public partial class Conversion
    {
        /// <summary>
        ///     Convert one file to another with destination format using hardware acceleration (if possible). Using cuvid. Works only on Windows/Linux with NVidia GPU.
        /// </summary>
        /// <returns>IConversion object</returns>
        internal async static Task<Device[]> GetAvailableDevices()
        {
            var format = Format.dshow;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                format = Format.v4l2;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                format = Format.avfoundation;

            }

            var conversion = New().AddParameter($"-list_devices true -f {format} -i dummy");
            var text = new StringBuilder();
            conversion.OnDataReceived += (_, e) => text.AppendLine(e.Data);
            await conversion.Start();

            var result = text.ToString();

            var devices = new List<Device>();
            var matches = Regex.Matches(result, "\"([^\"]*)\"");
            for (var i = 0; i < matches.Count; i += 2)
            {
                devices.Add(new Device()
                {
                    Name = matches[i].Value.Substring(1, matches[i].Value.Length - 2),
                    AlternativeName = matches[i + 1].Value.Substring(1, matches[i + 1].Value.Length - 2)
                });
            }

            return devices.ToArray();
        }
    }
}
