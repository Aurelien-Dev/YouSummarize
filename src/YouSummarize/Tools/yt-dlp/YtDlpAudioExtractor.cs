using System.Diagnostics;
using System.Text.Json;

public sealed class YtDlpAudioExtractor
{
    private readonly string _ytDlpPath;
    private readonly string? _cookiesPath;

    public YtDlpAudioExtractor()
    {
        _ytDlpPath = GetYtDlpPath();
        _cookiesPath = BuildCookiesFile();
    }

    // Décode le contenu base64 de YTDLP_COOKIES, l'écrit dans un fichier temporaire et retourne son chemin.
    // Retourne null si la variable d'environnement n'est pas définie.
    private static string? BuildCookiesFile()
    {
        var cookiesBase64 = Environment.GetEnvironmentVariable("YTDLP_COOKIES");
        if (string.IsNullOrWhiteSpace(cookiesBase64))
            return null;

        var cookiesContent = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookiesBase64));
        var path = Path.Combine(Path.GetTempPath(), "yt-dlp-cookies.txt");
        File.WriteAllText(path, cookiesContent);
        return path;
    }

    public async Task<string> DownloadAudioAsync(string url, string outputDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required.", nameof(url));

        Directory.CreateDirectory(outputDirectory);

        if (!File.Exists(_ytDlpPath))
        {
            throw new FileNotFoundException(
                $"yt-dlp executable not found: {_ytDlpPath}");
        }

        var outputTemplate = Path.Combine(outputDirectory, "%(id)s.%(ext)s");

        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Récupère uniquement le meilleur flux audio disponible
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add("bestaudio");

        // Nom de fichier basé uniquement sur l'ID de la vidéo
        startInfo.ArgumentList.Add("-o");
        startInfo.ArgumentList.Add(outputTemplate);

        // Demande à yt-dlp d'afficher le chemin final du fichier
        startInfo.ArgumentList.Add("--print");
        startInfo.ArgumentList.Add("after_move:filepath");
        startInfo.ArgumentList.Add("--js-runtimes");
        startInfo.ArgumentList.Add("node:/usr/bin/node");

        if (File.Exists(_cookiesPath))
        {
            startInfo.ArgumentList.Add("--cookies");
            startInfo.ArgumentList.Add(_cookiesPath);
        }

        startInfo.ArgumentList.Add(url);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"yt-dlp failed with exit code {process.ExitCode}.{Environment.NewLine}" + standardError);
        }

        var outputFile = standardOutput
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();

        if (string.IsNullOrWhiteSpace(outputFile))
        {
            throw new InvalidOperationException("yt-dlp completed successfully but did not return an output file.");
        }

        outputFile = outputFile.Trim();

        if (!File.Exists(outputFile))
        {
            throw new FileNotFoundException($"yt-dlp returned a file that does not exist: {outputFile}");
        }

        return outputFile;
    }

    public async Task<VideoMetadata> GetMetadataAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL is required.", nameof(url));

        if (!File.Exists(_ytDlpPath))
        {
            throw new FileNotFoundException($"yt-dlp executable not found: {_ytDlpPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("--dump-json");
        startInfo.ArgumentList.Add("--no-playlist");
        startInfo.ArgumentList.Add("--js-runtimes");
        startInfo.ArgumentList.Add("node:/usr/bin/node");

        if (File.Exists(_cookiesPath))
        {
            startInfo.ArgumentList.Add("--cookies");
            startInfo.ArgumentList.Add(_cookiesPath);
        }

        startInfo.ArgumentList.Add(url);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var standardOutputTask =
            process.StandardOutput.ReadToEndAsync(cancellationToken);

        var standardErrorTask =
            process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"yt-dlp failed with exit code {process.ExitCode}.{Environment.NewLine}" +
                standardError);
        }

        using var document = JsonDocument.Parse(standardOutput);
        var root = document.RootElement;

        return new VideoMetadata
        {
            Id = GetString(root, "id"),
            Title = GetString(root, "title"),
            Description = GetString(root, "description"),

            DurationSeconds = GetDouble(root, "duration"),

            Uploader = GetString(root, "uploader"),
            Channel = GetString(root, "channel"),
            ChannelId = GetString(root, "channel_id"),

            Thumbnail = GetString(root, "thumbnail"),

            WebpageUrl = GetString(root, "webpage_url"),

            ViewCount = GetInt64(root, "view_count"),
            LikeCount = GetInt64(root, "like_count"),

            Extractor = GetString(root, "extractor")
        };
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static double? GetDouble(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.TryGetDouble(out var value)
            ? value
            : null;
    }

    private static long? GetInt64(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
               property.TryGetInt64(out var value)
            ? value
            : null;
    }

    private static string GetYtDlpPath()
    {
        var baseDirectory = AppContext.BaseDirectory;

        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                baseDirectory,
                "Tools",
                "yt-dlp",
                "win-x64",
                "yt-dlp.exe");
        }

        if (OperatingSystem.IsLinux())
        {
            return Path.Combine(
                baseDirectory,
                "Tools",
                "yt-dlp",
                "linux-x64",
                "yt-dlp_musllinux");
        }

        throw new PlatformNotSupportedException($"yt-dlp is not configured for {Environment.OSVersion.Platform}.");
    }

    public sealed class VideoMetadata
    {
        public string? Id { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }

        public double? DurationSeconds { get; init; }

        public string? Uploader { get; init; }
        public string? Channel { get; init; }
        public string? ChannelId { get; init; }

        public string? Thumbnail { get; init; }

        public string? WebpageUrl { get; init; }

        public long? ViewCount { get; init; }
        public long? LikeCount { get; init; }

        public string? Extractor { get; init; }

        public TimeSpan? Duration =>
            DurationSeconds is null
                ? null
                : TimeSpan.FromSeconds(DurationSeconds.Value);
    }
}