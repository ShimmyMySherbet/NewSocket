using System;

namespace NewSocket.Models
{
    public static class Utils
    {
        public static string GetReadableSpeed(TimeSpan time, double bytes)
        {
            var segments = time.TotalSeconds / 1;
            var bps = bytes / segments;
            return BytesPerSecToReadable(bps) + "p/s";
        }

        public static string BytesPerSecToReadable(double BPS)
        {
            if (BPS >= (1024 * 1024 * 1024))
            {
                return $"{Math.Round(BPS / (1024 * 1024 * 1024), 2)}Gb";
            }
            else if (BPS >= 1024 * 1024)
            {
                return $"{Math.Round(BPS / (1024 * 1024), 2)}Mb";
            }
            else if (BPS >= 1024)
            {
                return $"{Math.Round(BPS / 1024, 2)}Kb";
            }
            else
            {
                return $"{Math.Round(BPS, 2)}b";
            }
        }
    }
}