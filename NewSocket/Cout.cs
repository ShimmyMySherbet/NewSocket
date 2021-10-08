using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NewSocket
{
    public class Cout
    {
        public static List<ConsoleColor> Colors = new[]
        {
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.White,
            ConsoleColor.Cyan,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.DarkYellow
        }.ToList();

        public static object LK = new object();

        private static ConcurrentDictionary<string, ConsoleColor> m_Colors = new ConcurrentDictionary<string, ConsoleColor>();

        public static void Write(string source, string message)
        {
            lock (LK)
            {
                var pre = Console.ForegroundColor;
                Console.ForegroundColor = GetThreadColor(source);
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] [{source}] {message}");
                Console.ForegroundColor = pre;
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