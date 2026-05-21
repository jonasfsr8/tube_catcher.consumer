using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using receive_system.module.Interfaces;
using receive_system.root.DTOs;
using System.Diagnostics;

namespace receive_system.root.ExternalServices
{
    public class YoutubeService : IYoutubeService
    {
        private readonly YtDlpSettingsDto _settings;
        private readonly ILogger<YoutubeService> _logger;

        public YoutubeService(IOptions<YtDlpSettingsDto> settings, ILogger<YoutubeService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> DownloadAsync(string url, string format, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[*] serving metadata...");
            var outputPath = $@"{_settings.OutputPath}\%(title)s.%(ext)s";

            string arguments;

            if (format == "mp3")
            {
                arguments = $"-x --audio-format mp3 -o \"{outputPath}\" {url}";
            }
            else
            {
                // video
                arguments = format switch
                {
                    "720p" => $"-f \"bestvideo[height<=720]+bestaudio/best[height<=720]\" -o \"{outputPath}\" {url}",
                    "1080p" => $"-f \"bestvideo[height<=1080]+bestaudio/best[height<=1080]\" -o \"{outputPath}\" {url}",
                    "1440p" => $"-f \"bestvideo[height<=1440]+bestaudio/best[height<=1440]\" -o \"{outputPath}\" {url}",
                    "2160p" => $"-f \"bestvideo[height<=2160]+bestaudio/best[height<=2160]\" -o \"{outputPath}\" {url}",
                    _ => throw new Exception("Invalid format.")
                };
            }

            var psi = new ProcessStartInfo
            {
                FileName = _settings.Path,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                throw new Exception($"yt-dlp error: {error}");

            _logger.LogInformation("[*] extraction completed...");
            return output;
        }
    }
}
