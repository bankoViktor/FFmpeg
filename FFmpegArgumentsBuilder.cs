using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFmpeg
{
    public class FFmpegArgumentsBuilder
    {
        private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private FFmpegArgumentsInputBuilder _inputBuilder;
        private FFmpegArgumentsOutputBuilder _outputBuilder;

        internal string Build(string inputFile, string outputFile)
        {
            if (inputFile == null)
                throw new ArgumentNullException(nameof(inputFile));

            if (string.IsNullOrWhiteSpace(inputFile))
                throw new ArgumentException("Not valid parameter value.", nameof(inputFile));

            if (outputFile == null)
                throw new ArgumentNullException(nameof(outputFile));

            if (string.IsNullOrWhiteSpace(outputFile))
                throw new ArgumentException("Not valid parameter value.", nameof(outputFile));

            var builder = new StringBuilder();
            foreach (var parameter in _parameters)
            {
                if (parameter.Key.Length > 0)
                {
                    builder.Append(" -");
                    builder.Append(parameter.Key);
                }

                if (parameter.Value.Length > 0)
                {
                    builder.Append(' ');
                    builder.Append(parameter.Value);
                }
            }

            if (_inputBuilder != null)
            {
                var inConfgs = _inputBuilder.Build();
                builder.Append(inConfgs);
            }

            builder.Append(" -i ");
            builder.Append(Quotes(inputFile));

            if (_outputBuilder != null)
            {
                var outConfgs = _outputBuilder.Build();
                builder.Append(outConfgs);
            }

            builder.Append(' ');
            builder.Append(Quotes(outputFile));

            return builder.ToString();
        }

        private string Quotes(string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if ((str.First() != '\"' || str.First() != '\'') && (str.Last() != '\"' || str.Last() != '\''))
            {
                return "\"" + str + "\"";
            }

            return str;
        }

        internal bool ContainsParameter(string parameterName)
        {
            if (parameterName == null)
                throw new ArgumentNullException(nameof(parameterName));

            return _parameters != null && _parameters.ContainsKey(parameterName);
        }

        public FFmpegArgumentsBuilder InputConfigure(Action<FFmpegArgumentsInputBuilder> configurator)
        {
            _inputBuilder = new FFmpegArgumentsInputBuilder();

            if (configurator != null)
                configurator.Invoke(_inputBuilder);

            return this;
        }

        public FFmpegArgumentsBuilder OutputConfigure(Action<FFmpegArgumentsOutputBuilder> configurator)
        {
            _outputBuilder = new FFmpegArgumentsOutputBuilder();

            if (configurator != null)
                configurator.Invoke(_outputBuilder);

            return this;
        }

        /// <summary>
        /// Если <b>TRUE</b>, то перезаписывает выходные файлы без запроса, <b>FALSE</b> в противном случае.
        /// </summary>
        public FFmpegArgumentsBuilder RewriteIfExists()
        {
            _parameters.Remove("n");
            _parameters["y"] = "";

            return this;
        }

        /// <summary>
        /// Установливает уровень ведения журнала и флаги, используемые библиотекой.
        /// <para />
        /// Возможные знначения определены в <see cref="FFmpegLogLevels"/>.
        /// </summary>
        public FFmpegArgumentsBuilder LogLevel(params string[] levels)
        {
            return LogLevel(false, levels);
        }

        /// <summary>
        /// Установливает уровень ведения журнала и флаги, используемые библиотекой.
        /// <para />
        /// Возможные знначения определены в <see cref="FFmpegLogLevels"/>.
        /// </summary>
        public FFmpegArgumentsBuilder LogLevel(bool isShowLevel, params string[] levels)
        {
            var str = levels.Aggregate((c, n) => c + "+" + n);

            if (isShowLevel)
            {
                var _str = "level";
                if (str.Length > 0)
                {
                    _str += "+" + str;
                }
                str = _str;
            }

            _parameters["loglevel"] = str;

            return this;
        }

        /// <summary>
        /// Подавить печать баннера.
        /// </summary>
        public FFmpegArgumentsBuilder HideBunner()
        {
            _parameters["hide_banner"] = "";

            return this;
        }

        /// <summary>
        /// Определяет вывод информации о прогрессе обработки.
        /// <para/>
        /// Константы определены в <see cref="FFmpegProgressUrls"/>.
        /// </summary>
        public FFmpegArgumentsBuilder Progress(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            _parameters["progress"] = url;

            return this;
        }

        public FFmpegArgumentsBuilder NoStatistics()
        {
            _parameters["nostats"] = "";

            return this;
        }
    }
}
