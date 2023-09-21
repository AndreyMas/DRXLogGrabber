using System;
using System.Collections.Generic;

namespace DrxLogGrabber
{
    public class Config
    {
        public List<string>? LogPath { get; set; }
        public string? OutputPath { get; set; }
        public string? FtpAddress { get; set; }
        public string? FtpLogin { get; set; }
        public string? FtpPass { get; set; }
        public List<string>? Services { get; set; }
        public string? Year { get; set; }
    }
}
