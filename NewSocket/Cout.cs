using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
namespace NewSocket
{
    /// <summary>
    /// Debug class for thread color coded messages
    /// can be toggled on or off
    ///
    /// Won't be in final release
    /// </summary>
    public class Cout
    {
        public static bool EnableLogging { get; set; } = false;

        private static List<ConsoleColor> Colors = new[]
        {
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Cyan,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.DarkYellow
        }.ToList();

        public static object LK = new object();

        private static ConcurrentDictionary<string, ConsoleColor> m_Colors = new ConcurrentDictionary<string, ConsoleColor>();

        public static void Write(string source, string message)
        {
            if (!EnableLogging) return;
            lock (LK)
            {
                var pre = Console.ForegroundColor;
                Console.ForegroundColor = GetThreadColor(source);
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] [{source}] {message}");
                Console.ForegroundColor = pre;
                Debug.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] [{source}] {message}");
            }
        }

        private static ConsoleColor GetThreadColor(string id)
        {
            if (!m_Colors.ContainsKey(id.ToLower().Trim()))
            {
                var n = Colors[0];
                Colors.RemoveAt(0);
                m_Colors[id.ToLower().Trim()] = n;
                return n;
            }
            else
            {
                return m_Colors[id.ToLower().Trim()];
            }
        }
    }
}