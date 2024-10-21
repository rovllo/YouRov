using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using FFMpegCore;
using FFMpegCore.Enums;

class Program
{


	static async Task Main(string[] args)
	{
		var youtube = new YoutubeClient();

		while (true)
		{
			Console.Write("Enter YouTube URL (or press Enter without typing to exit): ");
			string url = Console.ReadLine();
			if (string.IsNullOrEmpty(url))
				break;

			try
			{
				Console.WriteLine("Fetching video information...");
				var video = await youtube.Videos.GetAsync(url);
				var title = video.Title;

				Console.WriteLine("Getting available download formats...");
				var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);


				var combinedStreams = CombineStreams(streamManifest);

				// Filter combinedStreams to keep only the highest quality audio for each video quality
				var filteredStreams = combinedStreams
					.GroupBy(s => s.VideoStream.VideoQuality)
					.Select(g => g.OrderByDescending(s => s.AudioStream.Bitrate).First())
					.OrderByDescending(s => s.VideoStream.VideoQuality)
					.ToList();

				Console.WriteLine("\nAvailable formats:");
				for (int i = 0; i < filteredStreams.Count; i++)
				{
					var streamInfo = filteredStreams[i];
					Console.WriteLine($"{i + 1}. {streamInfo.VideoStream.VideoQuality.Label} {streamInfo.VideoStream.Container} " +
									  $"(Video: {streamInfo.VideoStream.Size.MegaBytes:F1}MB + Audio: {streamInfo.AudioStream.Size.MegaBytes:F1}MB, " +
									  $"Audio Bitrate: {streamInfo.AudioStream.Bitrate.KiloBitsPerSecond:F0}kbps)");
				}

				Console.Write("\nEnter the number of the format you want to download: ");
				if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > filteredStreams.Count)
				{
					throw new ArgumentException("Invalid choice. Please enter a valid number.");
				}
				choice--; // Adjust for zero-based index

				var selectedStream = filteredStreams[choice];

				var videosFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "videos");
				Directory.CreateDirectory(videosFolder);

				var safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));
				var fileName = $"{safeTitle}.mp4";
				var filePath = Path.Combine(videosFolder, fileName);

				var videoFilePath = Path.Combine(videosFolder, $"{safeTitle}_video.{selectedStream.VideoStream.Container.Name}");
				var audioFilePath = Path.Combine(videosFolder, $"{safeTitle}_audio.{selectedStream.AudioStream.Container.Name}");

				if (File.Exists(filePath))
				{
					Console.Write($"File '{fileName}' already exists. Do you want to overwrite it? (Y/N): ");
					var response = Console.ReadLine()?.Trim().ToUpper();
					if (response != "Y")
					{
						Console.WriteLine("Download cancelled.");
						continue;
					}
				}


				Console.WriteLine($"\nDownloading: {title}");
				Console.WriteLine();
				Console.WriteLine();

				Console.Write("Video progress:");
				int videoProgressLine = Console.CursorTop - 3;
				var videoProgress = new Progress<double>(p => DrawProgressBar(p, 50, videoProgressLine, "Video"));
				Console.WriteLine();


				Console.Write("Audio progress:");
				int audioProgressLine = Console.CursorTop - 3;
				var audioProgress = new Progress<double>(p => DrawProgressBar(p, 50, audioProgressLine, "Audio"));
				Console.WriteLine();


				await youtube.Videos.Streams.DownloadAsync(selectedStream.VideoStream, videoFilePath, videoProgress);
				await youtube.Videos.Streams.DownloadAsync(selectedStream.AudioStream, audioFilePath, audioProgress);

				Console.SetCursorPosition(0, audioProgressLine + 4);
				Console.WriteLine("Combining video and audio...");
				await FFMpegArguments
					.FromFileInput(videoFilePath)
					.AddFileInput(audioFilePath)
					.OutputToFile(filePath, true, options => options
						.WithVideoCodec(VideoCodec.LibX264)
						.WithAudioCodec(AudioCodec.Aac)
						.WithFastStart())
					.ProcessAsynchronously();

				File.Delete(videoFilePath);
				File.Delete(audioFilePath);

				Console.WriteLine($"\nDownload and combination complete: {filePath}");
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"An error occurred: {ex.Message}");
				Console.ResetColor();
			}

			Console.WriteLine("\nPress Enter to download another video or press Enter without typing to exit.");
			Console.ReadLine();
			Console.Clear();
		}
	}

	static List<CombinedStreamInfo> CombineStreams(StreamManifest streamManifest)
	{
		var videoOnlyStreams = streamManifest.GetVideoOnlyStreams()
			.Where(s => s.Container == Container.Mp4)
			.OrderByDescending(s => s.VideoQuality)
			.ThenByDescending(s => s.Bitrate)
			.ToList();

		var audioOnlyStreams = streamManifest.GetAudioOnlyStreams()
			.OrderByDescending(s => s.Bitrate)
			.ToList();

		var combinedStreams = new List<CombinedStreamInfo>();

		foreach (var videoStream in videoOnlyStreams)
		{
			// Find the audio stream with the closest bitrate to the video stream
			var matchingAudioStream = audioOnlyStreams
				.OrderBy(a => Math.Abs(a.Bitrate.BitsPerSecond - videoStream.Bitrate.BitsPerSecond))
				.First();

			combinedStreams.Add(new CombinedStreamInfo(videoStream, matchingAudioStream));
		}

		return combinedStreams;
	}

	static object _lockObject = new object();

	static void DrawProgressBar(double progress, int width, int cursorTop, string label)
	{
		lock (_lockObject)
		{
			Console.SetCursorPosition(0, cursorTop);

			int filledWidth = (int)(width * progress);
			int emptyWidth = width - filledWidth;

			string progressBar = $"{label}: [{new string('█', filledWidth)}{new string('░', emptyWidth)}] {progress:P0}";

			Console.Write(progressBar.PadRight(Console.WindowWidth - 1));

			// Clear the next three lines
			for (int i = 1; i <= 3; i++)
			{
				Console.SetCursorPosition(0, cursorTop + i);
				Console.Write(new string(' ', Console.WindowWidth - 1));
			}
		}
	}
}

class CombinedStreamInfo
{
	public IVideoStreamInfo VideoStream { get; }
	public IAudioStreamInfo AudioStream { get; }

	public CombinedStreamInfo(IVideoStreamInfo videoStream, IAudioStreamInfo audioStream)
	{
		VideoStream = videoStream;
		AudioStream = audioStream;
	}
}