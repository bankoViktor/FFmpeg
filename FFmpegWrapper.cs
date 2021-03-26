using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FFmpeg
{
    /// <summary>
    /// Программная обертка для FFmpeg.
    /// <para />
    /// Перед использование необходимо проиницйиализировать методом <see cref="Initialization"/>.
    /// </summary>
    public static class FFmpegWrapper
    {
        /// <summary>
        /// Каталог поиска PE-файла (<b>ffmpeg.exe</b> или <b>ffprobe.exe</b>).
        /// <para />
        /// Если PE не будет найден в указанной папке, то будет осуществлен поиск 
        /// в подкаталоге <b>.\x64</b> (или <b>.\x86</b> для 32-битной ОС).
        /// </summary>
        public static string PECatalog { get; private set; }

        /// <summary>
        /// Каталог поиска PE-файла (<b>ffmpeg.exe</b> или <b>ffprobe.exe</b>).
        /// <para />
        /// Если PE не будет найден в указанной папке, то будет осуществлен поиск 
        /// в подкаталоге <b>.\x64</b> (или <b>.\x86</b> для 32-битной ОС).
        /// </summary>
        /// <param name="peCatalog">Каталог поиска PE-файла</param>
        public static void Initialization(string peCatalog)
        {
            if (!File.GetAttributes(peCatalog).HasFlag(FileAttributes.Directory))
                throw new Exception($"Задана не директория: '{peCatalog}'");

            PECatalog = peCatalog;
        }

        public static void Execute(string inputFile, string outputFile, IProgress<double> progress, Action<FFmpegArgumentsBuilder> argsConfigurator)
        {
            if (argsConfigurator == null)
                throw new ArgumentNullException(nameof(argsConfigurator));

            var _argsBuilder = new FFmpegArgumentsBuilder();

            argsConfigurator.Invoke(_argsBuilder);

            Execute(inputFile, outputFile, progress, _argsBuilder);
        }

        public static void Execute(string inputFile, string outputFile, Action<FFmpegArgumentsBuilder> argsConfigurator)
        {
            Execute(inputFile, outputFile, null, argsConfigurator);
        }

        public static void Execute(string inputFile, string outputFile, IProgress<double> progress, FFmpegArgumentsBuilder argsConfigurator)
        {
            if (argsConfigurator == null)
                throw new ArgumentNullException(nameof(argsConfigurator));

            if (File.Exists(outputFile))
            {
                if (!argsConfigurator.ContainsParameter("y") || argsConfigurator.ContainsParameter("n"))
                {
                    Console.WriteLine($"Выходной файл '{outputFile}' существует. Выход.");
                    return;
                }
            }

            if (progress != null)
            {
                argsConfigurator.Progress(FFmpegProgressUrls.StdOut);
            }

            var args = argsConfigurator.Build(inputFile, outputFile);

            Execute(args, progress);
        }

        public static void Execute(string inputFile, string outputFile, FFmpegArgumentsBuilder argsConfigurator)
        {
            Execute(inputFile, outputFile, null, argsConfigurator);
        }

        public static void Execute(string args, IProgress<double> progress)
        {
            if (PECatalog == null)
                throw new Exception($"Инициализируй {nameof(FFmpegWrapper)} используя {nameof(FFmpegWrapper.Initialization)}.");

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (string.IsNullOrWhiteSpace(args))
                throw new ArgumentException($"Не допустимое значение аргументов: '{args}'.");

            Console.WriteLine("Executing start");
            Console.WriteLine("Args: " + args);
            Console.WriteLine();

            using var process = new Process()
            {
                StartInfo =
                {
                    FileName = GetPE("FFmpeg.exe"),
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true, // fire Exited event
            };

            if (progress != null)
                process.StartInfo.RedirectStandardOutput = true;

            var totalDuration = (TimeSpan?)null;
            var progressInfo = new FFmpegProgressInfo();
            progressInfo.Updated += (s, e) =>
            {
                if (progress != null)
                {
                    if (!totalDuration.HasValue)
                        throw new Exception("Не найдена длительность входного файла. Возможно установлен параметр '-nostats' или не установлен флаг 'info' в параметре '-loglevel'.");

                    var percent = (progressInfo.OutTime.TotalSeconds / totalDuration.Value.TotalSeconds) * 100;
                    progress.Report(percent);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                Debug.WriteLine(e.Data);

                var mark = "Duration: ";
                if (e.Data != null && e.Data.Contains(mark))
                {
                    var begin = e.Data.IndexOf(mark);
                    if (begin > -1)
                    {
                        begin += mark.Length;
                        var end = e.Data.IndexOf(',', begin);
                        if (end > -1)
                        {
                            var dur = e.Data.Substring(begin, end - begin);
                            totalDuration = TimeSpan.Parse(dur);
                        }
                    }

                    Console.WriteLine("Duration " + totalDuration);
                }

                if (e.Data != null && (e.Data.Contains("[fatal]") || e.Data.Contains("[error]")))
                {
                    var i = e.Data.IndexOf(' ');
                    if (i > -1)
                    {
                        var msg = e.Data[i..];
                        throw new Exception($"FFmpeg Error: {msg}");
                    }
                }
            };
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    //Console.WriteLine(e.Data);
                    progressInfo.Parse(e.Data);
                }
            };
            process.Exited += (s, e) =>
            {
                if (process.ExitCode == 0)
                {
                    Debug.WriteLine("F I N I S H E D");
                }
                else
                {
                    throw new Exception($"FFmpeg Error: exit code {process.ExitCode}");
                }
            };

            process.Start();

            process.BeginErrorReadLine();

            if (progress != null)
                process.BeginOutputReadLine();

            process.WaitForExit();

            Console.WriteLine("Executed completed");
        }

        public static void Execute(string args)
        {
            Execute(args, null);
        }

        public static async void ExecuteAsync(string inputFile, string outputFile, IProgress<double> progress, Action<FFmpegArgumentsBuilder> argsConfigurator)
        {
            await Task.Run(() => Execute(inputFile, outputFile, progress, argsConfigurator));
        }

        public static async void ExecuteAsync(string inputFile, string outputFile, Action<FFmpegArgumentsBuilder> argsConfigurator)
        {
            await Task.Run(() => Execute(inputFile, outputFile, argsConfigurator));
        }

        public static async void ExecuteAsync(string inputFile, string outputFile, IProgress<double> progress, FFmpegArgumentsBuilder argsConfigurator)
        {
            await Task.Run(() => Execute(inputFile, outputFile, progress, argsConfigurator));
        }

        public static async void ExecuteAsync(string inputFile, string outputFile, FFmpegArgumentsBuilder argsConfigurator)
        {
            await Task.Run(() => Execute(inputFile, outputFile, argsConfigurator));
        }

        public static async void ExecuteAsync(string args, IProgress<double> progress)
        {
            await Task.Run(() => Execute(args, progress));
        }

        public static async void ExecuteAsync(string args)
        {
            await Task.Run(() => Execute(args));
        }


        #region Private

        private static string GetPE(string peName)
        {
            var path = Path.Combine(PECatalog, peName);
            if (!File.Exists(path))
            {
                var bitOS = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                path = Path.Combine(PECatalog, bitOS, peName);
                if (!File.Exists(path))
                    throw new FileNotFoundException($"Не найден исполняемый файл {nameof(FFmpegWrapper)} в '{path}'.");
            }

            return path;
        }

        #endregion

        #region Verbose

        public static void Snapshot(string inputFile, string outputFile, TimeSpan position, int? width = null, int? height = null)
        {
            Execute(inputFile, outputFile, argsBuilder => argsBuilder
                .RewriteIfExists()
                .HideBunner()
                .LogLevel("level")
                .InputConfigure(input =>
                {
                    input.SetPosition(position.TotalSeconds);
                })
                .OutputConfigure(output =>
                {
                    if (width.HasValue || height.HasValue)
                    {
                        var w = width ?? throw new ArgumentNullException(nameof(width));
                        var h = height ?? throw new ArgumentNullException(nameof(height));
                        output.SetSize(w, h);
                    }
                    output.SetFramesCount(1);
                })
            );
        }

        public static async void SnapshotAsync(string inputFile, string outputFile, TimeSpan position, int? width = null, int? height = null)
        {
            await Task.Run(() => Snapshot(inputFile, outputFile, position, width, height));
        }

        #endregion
    }
}
