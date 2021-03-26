using FFmpeg.Attributes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FFmpeg
{
    public class FFmpegProgressInfo
    {
        [FFmpeg(Key = "frame")]
        public long Frame { get; set; }


        [FFmpeg(Key = "out_time")]
        public TimeSpan OutTime { get; set; }


        public event EventHandler Updated;
        public event EventHandler Completed;

        /*
            * Пример выхлопа FFmpeg с параметром -progress <url>
            * 
            * frame=46
            * fps=0.00
            * stream_0_0_q=0.0
            * bitrate=   0.2kbits/s
            * total_size=48
            * out_time_us=2113016
            * out_time_ms=2113016
            * out_time=00:00:02.113016
            * dup_frames=0
            * drop_frames=0
            * speed=2.13x
            * progress=continue
            */

        public void Parse(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;

            // Key=Value
            var eq = data.IndexOf('=');
            var key = data[0..eq];
            eq++;
            var value = data[eq..];

            // Fire Updated event by first key 'frame'
            if (key.ToLower() == "frame")
            {
                Updated?.Invoke(this, EventArgs.Empty);
            }

            // Fire Completed event by 'progress=end'
            if (key.ToLower() == "progress" && value.ToLower() == "end")
            {
                Completed?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Set Property Value
            var prop = GetType().GetProperties()
                .Where(p => string.Compare(key, p.GetCustomAttribute<FFmpegAttribute>()?.Key, true) == 0)
                .FirstOrDefault();
            if (prop != null)
            {
                var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                var typedValue = converter.ConvertFromString(value);
                prop.SetValue(this, typedValue);
            }
        }
    }
}
