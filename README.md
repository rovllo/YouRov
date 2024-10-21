# YouTube Video Downloader

## Description

YouTube Video Downloader is a powerful, user-friendly command-line application designed to download YouTube videos efficiently. Built with C# and .NET, this tool allows users to download videos in various qualities, extract audio, and automatically combine video and audio streams for the best possible quality.

## Features

- **Multiple Quality Options**: Download videos in different resolutions and formats.
- **Audio Extraction**: Option to download audio-only versions of videos.
- **Automatic Stream Combination**: Intelligently combines separate video and audio streams for optimal quality.
- **Real-time Progress Display**: Shows download progress with a dynamic console-based progress bar.
- **User-friendly Interface**: Simple command-line interface for easy interaction.
- **Format Selection**: Allows users to choose their preferred format before downloading.
- **Standalone Execution**: No need for additional software or dependencies (includes FFmpeg).

## Installation

1. Go to the [Releases](https://github.com/yourusername/youtube-video-downloader/releases) page.
2. Download the latest release for your operating system (Windows).
3. Extract the zip file to your desired location.

No installation is required. The application is self-contained and includes all necessary dependencies.

## Usage

1. Open a command prompt or terminal.
2. Navigate to the folder containing the extracted files.
3. Run the application by typing `YoutubeVideoDownloader.exe` and pressing Enter.
4. When prompted, paste the URL of the YouTube video you want to download.
5. Choose your preferred quality/format from the displayed list.
6. Wait for the download to complete.

Example:

```
> YoutubeVideoDownloader.exe
Enter YouTube URL: https://www.youtube.com/watch?v=dQw4w9WgXcQ
Available formats:
1. 1080p MP4 (Video: 120.5MB + Audio: 10.2MB)
2. 720p MP4 (Video: 80.3MB + Audio: 10.2MB)
3. Audio Only M4A (17.5MB)
Enter the number of the format you want to download: 1
Downloading: Never Gonna Give You Up
[████████████████████████████████████████████████] 100%
Download complete!
```

## Technical Details

- **Language**: C#
- **Framework**: .NET 6.0
- **Dependencies**: 
  - YoutubeExplode
  - FFmpeg (included in the release)

## Troubleshooting

- **Video Unavailable**: Ensure the video is not private or age-restricted.
- **Slow Download**: Check your internet connection or try a lower quality option.
- **FFmpeg Error**: Make sure FFmpeg.exe is in the same directory as the main application.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This tool is for personal use only. Respect copyright laws and YouTube's terms of service. The developers are not responsible for any misuse of this software.

## Acknowledgments

- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode) for YouTube interaction.
- [FFmpeg](https://ffmpeg.org/) for media processing.

---

For issues, feature requests, or questions, please open an issue on this GitHub repository.
