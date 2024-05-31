using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class LogHelper
    {
        private static readonly StringBuilder _log = new StringBuilder();

        public static void Log(params object[] log)
        {
            StringBuilder builder = new StringBuilder();
            foreach (object s in log)
            {
                builder.AppendLine($"{DateTime.Now.ToString()} D: {s.ToString()}");
            }

            Console.Write(builder.ToString());
        }

        public static void LogError(params object[] log)
        {
            StringBuilder builder = new StringBuilder();
            foreach (object s in log)
            {
                builder.AppendLine($"E: <color=#ff0000>{s.ToString()}</color>");
            }

            Console.Write(builder.ToString());
        }

        public static void LogWarning(params object[] log)
        {
            StringBuilder builder = new StringBuilder();
            foreach (object s in log)
            {
                builder.AppendLine($"w: <color=#FFFF00>{s.ToString()}</color>");
            }

            Console.Write(builder.ToString());
        }

        public static string LogInfo {  get { return _log.ToString(); } }
    }
}