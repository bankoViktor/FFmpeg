using System;

namespace FFmpeg.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class FFmpegAttribute : Attribute
    {
        public string Key { get; set; }
    }
}
