using System;

namespace FFmpeg
{
    public class FFmpegArgumentsOutputBuilder : FFmpegArgumentsBaseBuilder
    {
        // output,per-stream
        public FFmpegArgumentsOutputBuilder SetFramesCount(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            _parameters["frames:v"] = count.ToString();

            return this;
        }

        // output,per-stream
        /// <summary>
        /// Установливает максимальную частоту кадров (значение в Гц, дробь).
        /// </summary>
        /// <param name="framerate">Частота кадров</param>
        public FFmpegArgumentsOutputBuilder SetMaxFramerate(double framerate)
        {
            if (framerate < 1)
                throw new ArgumentOutOfRangeException(nameof(framerate));

            _parameters["fpsmax"] = framerate.ToString();

            return this;
        }
    }
}
