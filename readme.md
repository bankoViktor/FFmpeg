# FFmpeg CSharp Wrapper

This CSharp wrapper for FFmpeg.

### Compatibility

This application requires at least:
* Windows
* .Net Core 3.1 or above

## Usage

	1. Initialize
	
```C#
FFMpegWrapper.Initialization(@"D:\ffmpeg")
```

	2. Run
	
```C#

var progress = new Progress<double>(v => Console.WriteLine("{0,8:F1}%", v));

FFMpegWrapper.Execute(in_file, out_file, progress, argsBuilder =>
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

* [FFmpeg-Builds](https://github.com/BtbN/FFmpeg-Builds)