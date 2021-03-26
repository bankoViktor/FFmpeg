using System;
using System.Collections.Generic;
using System.Text;

namespace FFmpeg
{
    public abstract class FFmpegArgumentsBaseBuilder
    {
        protected readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();


        internal string Build()
        {
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

            return builder.ToString();
        }

        // input/output
        public FFmpegArgumentsBaseBuilder SetPosition(double position)
        {
            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            _parameters["ss"] = position.ToString();

            return this;
        }

        // input/output,per-stream
        public FFmpegArgumentsBaseBuilder SetSize(int width, int height)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height));

            _parameters["s"] = $"{width}x{height}";

            return this;
        }

        // input/output,per-stream
        /// <summary>
        /// Установливает частоту кадров (значение в Гц, дробь).
        /// </summary>
        /// <param name="framerate">Частота кадров</param>
        public FFmpegArgumentsBaseBuilder Framerate(double framerate)
        {
            if (framerate < 1)
                throw new ArgumentOutOfRangeException(nameof(framerate));

            _parameters["r"] = framerate.ToString();

            return this;
        }
    }
}
