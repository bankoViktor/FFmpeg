# FFmpeg CSharp Wrapper

This CSharp wrapper for FFmpeg.

### Compatibility

This application requires at least:
* .Net Core 3.1 or above

## Usage

1. Initialize
	
```C#

FFmpegWrapper.Initialization(@"D:\ffmpeg")

```

2. Run
	
```C#

var progress = new Progress<double>(v => Console.WriteLine("{0,8:F1}%", v));

FFmegWrapper.Execute(in_file, out_file, progress, argsBuilder =>
{
	argsBuilder.LogLevel("level");
	argsBuilder.HideBunner();
	argsBuilder.NoStatistics();
	argsBuilder.RewriteIfExists();
	argsBuilder.OutputConfigure(outputconfigurator =>
	{
		outputconfigurator.SetFramerate(25);
	});
});

```
	
3. Profit

## Authors

* **Banko Viktor S.** - *Initial work* - [bankoViktor](https://github.com/bankoViktor)

## License

[MIT](https://choosealicense.com/licenses/mit/)

## Acknowledgments

* [FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds) - Builds for downloading
* [FFmpeg Documentation](https://ffmpeg.org/ffmpeg.html) 
* [FFprobe Documentation](https://ffmpeg.org/ffprobe.html) 
